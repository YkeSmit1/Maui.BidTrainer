using Common;
using EngineWrapper;
using Maui.BidTrainer.ViewModels;
using MvvmHelpers.Commands;
using Newtonsoft.Json;

namespace Maui.BidTrainer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BidTrainerPage
    {
        private readonly StartPage startPage = new StartPage();
        private readonly SettingsPage settingsPage = new SettingsPage();

        // Bidding
        private readonly Auction auction = new Auction();
        private readonly Pbn pbn = new Pbn();
        private bool isInHintMode;

        // Lesson
        private static int CurrentLesson
        {
            get => Preferences.Get(nameof(CurrentLesson), 2);
            set => Preferences.Set(nameof(CurrentLesson), value);
        }
        private static int CurrentBoardIndex
        {
            get => Preferences.Get(nameof(CurrentBoardIndex), 0);
            set => Preferences.Set(nameof(CurrentBoardIndex), value);
        }
        private Dictionary<Player, string> Deal => pbn.Boards[CurrentBoardIndex].Deal;
        private Player Dealer => pbn.Boards[CurrentBoardIndex].Dealer;
        private List<Lesson> lessons;
        private Lesson Lesson => lessons.Single(l => l.LessonNr == CurrentLesson);

        // Results
        private Result currentResult;
        private DateTime startTimeBoard;
        private Results results = new Results();

        // ViewModels
        private BiddingBoxViewModel BiddingBoxViewModel => (BiddingBoxViewModel)BiddingBoxView.BindingContext;
        private AuctionViewModel AuctionViewModel => (AuctionViewModel)AuctionView.BindingContext;
        private HandViewModel HandViewModelNorth => (HandViewModel)PanelNorth.BindingContext;
        private HandViewModel HandViewModelSouth => (HandViewModel)PanelSouth.BindingContext;

        public BidTrainerPage()
        {
            InitializeComponent();
            Application.Current.ModalPopping += PopModel;
            BiddingBoxViewModel.DoBid = new AsyncCommand<object>(ClickBiddingBoxButton, ButtonCanExecute);
            AuctionViewModel.Auction = auction;
        }
        private async Task Start()
        {
            try
            {

                using var lessonsReader = new StreamReader(await FileSystem.OpenAppPackageFileAsync("lessons.json"));
                lessons = JsonConvert.DeserializeObject<List<Lesson>>(await lessonsReader.ReadToEndAsync());

                var resultsFile = Path.Combine(FileSystem.AppDataDirectory, "results.json");
                if (File.Exists(resultsFile)) 
                    results = JsonConvert.DeserializeObject<Results>(await File.ReadAllTextAsync(resultsFile));

                await CopyFileToAppDataDirectory("four_card_majors.db3");

                Pinvoke.Setup(Path.Combine(FileSystem.Current.AppDataDirectory, "four_card_majors.db3"));

                await StartLessonAsync();
                await StartNextBoard();
            }
            catch (Exception e)
            {
                await DisplayAlert("Error", e.ToString(), "OK");
                throw;
            }
        }

        private static async Task CopyFileToAppDataDirectory(string filename)
        {
            var filePath = Path.Combine(FileSystem.AppDataDirectory, filename);

            await using Stream inputStream = await FileSystem.OpenAppPackageFileAsync(filename);
            using BinaryReader reader = new BinaryReader(inputStream);
            await using FileStream outputStream = File.Create(filePath);
            await using BinaryWriter writer = new BinaryWriter(outputStream);
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                writer.Write(buffer, 0, bytesRead);
            }
        }

        private async void PopModel(object sender, ModalPoppingEventArgs e)
        {
            if (e.Modal == startPage)
            {
                await StartLessonAsync();
                await StartNextBoard();
            }
            else if (e.Modal == settingsPage)
            {
                ((SettingsViewModel)settingsPage.BindingContext).Save();
                Device.BeginInvokeOnMainThread(() =>
                    StatusLabel.Text = $"Username: {Preferences.Get("Username", "")}\nLesson: {Lesson.LessonNr}\nBoard: {CurrentBoardIndex + 1}");
            }
        }

        private async Task StartLessonAsync()
        {
            await CopyFileToAppDataDirectory(Lesson.PbnFile);
            await pbn.LoadAsync(Path.Combine(FileSystem.AppDataDirectory, Lesson.PbnFile));
            Pinvoke.SetModules(Lesson.Modules);
            if (CurrentBoardIndex == 0)
                results.AllResults.Remove(Lesson.LessonNr);
        }

        private async Task ClickBiddingBoxButton(object parameter)
        {
            var bid = (Bid)parameter;
            if (isInHintMode)
            {
                currentResult.UsedHint = true;
                await DisplayAlert("Information", BidManager.GetInformation(bid, auction), "OK");
            }
            else
            {
                var engineBid = BidManager.GetBid(auction, Deal[Player.South]);
                UpdateBidControls(engineBid);

                if (bid != engineBid)
                {
                    await DisplayAlert("Incorrect bid", $"The correct bid is {engineBid}. Description: {engineBid.description}.", "OK");
                    currentResult.AnsweredCorrectly = false;
                }

                await BidTillSouth();
            }
        }

        private bool ButtonCanExecute(object param)
        {
            var bid = (Bid)param;
            return auction.BidIsPossible(bid);
        }

        private void UpdateBidControls(Bid bid)
        {
            auction.AddBid(bid);
            AuctionViewModel.Auction = ObjectCloner.ObjectCloner.DeepClone(auction);
            BiddingBoxViewModel.DoBid.RaiseCanExecuteChanged();
        }

        private async Task StartNextBoard()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                PanelNorth.IsVisible = false;
                BiddingBoxView.IsEnabled = true;
            });
            if (CurrentBoardIndex > pbn.Boards.Count - 1)
            {
                CurrentBoardIndex = 0;

                if (Lesson.LessonNr != lessons.Last().LessonNr)
                {
                    CurrentLesson++;
                    await StartLessonAsync();
                }
                else
                {
                    BiddingBoxView.IsEnabled = false;
                    await DisplayAlert("Info", "End of lessons", "OK");
                    CurrentLesson = 2;
                    ShowReport();
                    return;
                }
            }
            Device.BeginInvokeOnMainThread(() =>
                StatusLabel.Text = $"Username: {Preferences.Get("Username", "")}\nLesson: {Lesson.LessonNr}\nBoard: {CurrentBoardIndex + 1}");
            ShowBothHands();
            await StartBidding();
        }

        private async Task StartBidding()
        {
            auction.Clear(Dealer);
            AuctionViewModel.Auction = ObjectCloner.ObjectCloner.DeepClone(auction);
            BiddingBoxViewModel.DoBid.RaiseCanExecuteChanged();
            startTimeBoard = DateTime.Now;
            currentResult = new Result();
            await BidTillSouth();
        }

        private void ShowBothHands()
        {
            var alternateSuits = Preferences.Get("AlternateSuits", true);
            var cardProfile = Preferences.Get("CardImageSettings", "default");
            HandViewModelNorth.ShowHand(Deal[Player.North], alternateSuits, cardProfile);
            HandViewModelSouth.ShowHand(Deal[Player.South], alternateSuits, cardProfile);
        }

        private async Task BidTillSouth()
        {
            while (auction.CurrentPlayer != Player.South && !auction.IsEndOfBidding())
            {
                var bid = BidManager.GetBid(auction, Deal[auction.CurrentPlayer]);
                UpdateBidControls(bid);
            }

            if (auction.IsEndOfBidding())
            {
                BiddingBoxView.IsEnabled = false;
                PanelNorth.IsVisible = true;
                currentResult.TimeElapsed = DateTime.Now - startTimeBoard;
                await DisplayAlert("Info", $"Hand is done. Contract:{auction.currentContract}", "OK");
                results.AddResult(Lesson.LessonNr, CurrentBoardIndex, currentResult);
                await UploadResultsAsync();
                CurrentBoardIndex++;

                var resultsFile = Path.Combine(FileSystem.AppDataDirectory, "results.json");
                await File.WriteAllTextAsync(resultsFile, JsonConvert.SerializeObject(results, Formatting.Indented));

                await StartNextBoard();
            }
        }

        private async Task UploadResultsAsync()
        {
            var username = Preferences.Get("Username", "");
            if (username != "")
            {
                var res = results.AllResults.Values.SelectMany(x => x.Results.Values).ToList();
                await UpdateOrCreateAccount(username, res.Count(), res.Count(x => x.AnsweredCorrectly), res.Sum(x => x.TimeElapsed.Ticks));
            }

            static async Task UpdateOrCreateAccount(string username, int boardPlayed, int correctBoards, long timeElapsed)
            {
                var account = new Account
                {
                    username = username,
                    numberOfBoardsPlayed = boardPlayed,
                    numberOfCorrectBoards = correctBoards,
                    timeElapsed = new TimeSpan(timeElapsed)
                };

                var cosmosDbHelper = DependencyService.Get<ICosmosDbHelper>();
                var user = await cosmosDbHelper.GetAccount(username);
                if (user == null)
                {
                    account.id = Guid.NewGuid().ToString();
                    await cosmosDbHelper.InsertAccount(account);
                }
                else
                {
                    account.id = user.Value.id;
                    await cosmosDbHelper.UpdateAccount(account);
                }
            }
        }

        private void ShowReport()
        {
            var resultsPage = new ResultsPage(results);
            Application.Current.MainPage.Navigation.PushAsync(resultsPage);
        }

        private async void ButtonClickedStartLesson(object sender, EventArgs e)
        {
            await Application.Current.MainPage.Navigation.PushAsync(startPage);
        }

        private async void ButtonClickedNextBoard(object sender, EventArgs e)
        {
            CurrentBoardIndex++;
            await StartNextBoard();
        }

        private void ButtonClickedResults(object sender, EventArgs e)
        {
            ShowReport();
        }

        private async void ButtonClickedLeaderBoard(object sender, EventArgs e)
        {
            var accounts = await DependencyService.Get<ICosmosDbHelper>().GetAllAccounts();
            var leaderboardPage = new LeaderboardPage(accounts.OrderByDescending(x => (double)x.numberOfCorrectBoards / x.numberOfBoardsPlayed));
            await Application.Current.MainPage.Navigation.PushAsync(leaderboardPage);
        }

        private async void ButtonClickedSettings(object sender, EventArgs e)
        {
            await Application.Current.MainPage.Navigation.PushAsync(settingsPage);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Start();
        }

        private void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            isInHintMode = e.Value;
            LabelMode.Text = isInHintMode ? "Hint" : "Bid";
        }
    }
}