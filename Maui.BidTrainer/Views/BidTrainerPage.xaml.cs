using System.Collections.ObjectModel;
using System.Text.Json;
using Common;
using CommunityToolkit.Mvvm.Input;
using EngineWrapper;
using Maui.BidTrainer.Services;
using Maui.BidTrainer.ViewModels;
using Serilog;

namespace Maui.BidTrainer.Views;

public partial class BidTrainerPage
{
    private readonly StartPage startPage = new();
    private readonly SettingsService settingsService;
    private Dictionary<(string suit, string card), string> dictionary;

    // Bidding
    private readonly Auction auction = new();
    private readonly Pbn pbn = new();
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
    private Results results = new();
    private bool repeatMistakesMode;

    // ViewModels
    private BiddingBoxViewModel BiddingBoxViewModel => (BiddingBoxViewModel)BiddingBoxView.BindingContext;
    private AuctionViewModel AuctionViewModel => (AuctionViewModel)AuctionView.BindingContext;
    private HandViewModel HandViewModelNorth => (HandViewModel)PanelNorth.BindingContext;
    private HandViewModel HandViewModelSouth => (HandViewModel)PanelSouth.BindingContext;
    private readonly ILogger logger = IPlatformApplication.Current!.Services.GetService<ILogger>();

    public BidTrainerPage(SettingsService settingsService)
    {
        InitializeComponent();
        this.settingsService = settingsService;
        this.settingsService.SettingsChanged += (_, _) => UpdateUi();
        
        BiddingBoxViewModel.DoBidCommand = new AsyncRelayCommand<Bid>(ClickBiddingBoxButton, bid => auction.BidIsPossible(bid));
        AuctionViewModel.Bids.Clear();
        logger.Information("Test");
    }

    private void GenerateCardImages()
    {
        var cardProfile = Preferences.Get("CardImageSettings", "default");
        var settings = CardImageSettings.GetCardImageSettings(cardProfile);
        dictionary = SplitImages.Split(settings);
    }

    private async Task Start()
    {
        try
        {
            if (!startPage.IsLoaded)
                await Initialize();
            UpdateUi();
            await StartLessonAsync();
            await StartNextBoard();
        }
        catch (Exception e)
        {
            await DisplayAlert("Error", e.ToString(), "OK");
        }
    }

    private void UpdateUi()
    {
        MainThread.BeginInvokeOnMainThread(() =>
            StatusLabel.Text = $"Username: {Preferences.Get("Username", "")}\nLesson: {Lesson.LessonNr}\nBoard: {CurrentBoardIndex + 1}");
        GenerateCardImages();
    }

    private async Task Initialize()
    {
        using var lessonsReader = new StreamReader(await FileSystem.OpenAppPackageFileAsync("lessons.json"));
        lessons = JsonSerializer.Deserialize<List<Lesson>>(await lessonsReader.ReadToEndAsync());

        var resultsFile = Path.Combine(FileSystem.AppDataDirectory, "results.json");
        if (File.Exists(resultsFile))
            try
            {
                results = JsonSerializer.Deserialize<Results>(await File.ReadAllTextAsync(resultsFile));
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Error when loading results");
            }

        await Utils.CopyFileToAppDataDirectory("four_card_majors.db3");
        Pinvoke.Setup(Path.Combine(FileSystem.AppDataDirectory, "four_card_majors.db3"));
    }

    private async Task StartLessonAsync()
    {
        await Utils.CopyFileToAppDataDirectory(Lesson.PbnFile);
        await pbn.LoadAsync(Path.Combine(FileSystem.AppDataDirectory, Lesson.PbnFile));
        Pinvoke.SetModules(Lesson.Modules);
        if (CurrentBoardIndex == 0)
            results.AllResults.Remove(Lesson.LessonNr);
    }

    private async Task ClickBiddingBoxButton(Bid bid)
    {
        try
        {
            if (isInHintMode)
            {
                currentResult.UsedHint = true;
                await DisplayAlert("Information", BidManager.GetInformation(bid, auction), "OK");
            }
            else
            {
                var engineBid = BidManager.GetBid(auction, Deal[Player.South]);

                if (bid != engineBid)
                {
                    var message = $"The correct bid is {engineBid}. Description: {engineBid.description}.";
                    var engineBidInformation = BidManager.GetInformation(engineBid, auction);
                    var bidInformation = BidManager.GetInformation(bid, auction);
                    var s = $"{message}\n\nCorrect bid {engineBid}\n{engineBidInformation}\n\nYour bid {bid}\n{bidInformation}";
                    await DisplayAlert("Incorrect bid", s, "OK");
                    currentResult.AnsweredCorrectly = false;
                }
                UpdateBidControls(engineBid);

                await BidTillSouth();
            }
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }

    private void UpdateBidControls(Bid bid)
    {
        auction.AddBid(bid);
        if (AuctionViewModel.Bids.Any() && AuctionViewModel.Bids.Last() == "?")
            AuctionViewModel.Bids.RemoveAt(AuctionViewModel.Bids.Count - 1);
        AuctionViewModel.Bids.Add(bid.ToString());
        BiddingBoxViewModel.DoBidCommand?.NotifyCanExecuteChanged();
    }

    private async Task StartNextBoard()
    {
        if (CurrentBoardIndex == -1)
        {
            var wrongBoards = results.AllResults[CurrentLesson].GetWrongBoards();
            if (!repeatMistakesMode && wrongBoards.Count > 0)
            {
                repeatMistakesMode = true;
                CurrentBoardIndex = wrongBoards.First();
                MistakeLabel.Text = "Previous mistakes";
            }
            else 
            {
                MistakeLabel.Text = "";
                CurrentBoardIndex = 0;

                if (Lesson.LessonNr != lessons.Last().LessonNr)
                {
                    var percentage = results.AllResults[Lesson.LessonNr].Percentage;
                    await DisplayAlert("Info", $"End of lesson. Your have scored {percentage}", "OK");
                    CurrentLesson++;
                    await Shell.Current.GoToAsync($"{nameof(TheoryPage)}?Lesson={CurrentLesson}");
                    await StartLessonAsync();
                }
                else
                {
                    await DisplayAlert("Info", "End of lessons", "OK");
                    CurrentLesson = 2;
                    await ShowReport();
                }
            }
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            PanelNorth.IsVisible = false;
            BiddingBoxView.IsEnabled = true;
            StatusLabel.Text = $"Username: {Preferences.Get("Username", "")}\nLesson: {Lesson.LessonNr}\nBoard: {CurrentBoardIndex + 1}";
        });
        ShowBothHands();
        await StartBidding();
    }

    private async Task StartBidding()
    {
        auction.Clear(Dealer);
        AuctionViewModel.Bids = new ObservableCollection<string>(auction.Bids.SelectMany(x => x.Value.Values).Select(_ => ""));
        BiddingBoxViewModel.DoBidCommand?.NotifyCanExecuteChanged();
        startTimeBoard = DateTime.Now;
        currentResult = new Result();
        await BidTillSouth();
    }

    private void ShowBothHands()
    {
        var alternateSuits = Preferences.Get("AlternateSuits", true);
        var cardProfile = Preferences.Get("CardImageSettings", "default");
        HandViewModelNorth.ShowHand(Deal[Player.North], alternateSuits, cardProfile, dictionary);
        HandViewModelSouth.ShowHand(Deal[Player.South], alternateSuits, cardProfile, dictionary);
    }

    private async Task BidTillSouth()
    {
        while (auction.CurrentPlayer != Player.South && !auction.IsEndOfBidding())
        {
            var bid = BidManager.GetBid(auction, Deal[auction.CurrentPlayer]);
            UpdateBidControls(bid);
        }

        AuctionViewModel.Bids.Add("?");

        if (auction.IsEndOfBidding())
        {
            BiddingBoxView.IsEnabled = false;
            PanelNorth.IsVisible = true;
            if (!repeatMistakesMode)
            {
                currentResult.TimeElapsed = DateTime.Now - startTimeBoard;
                currentResult.Lesson = CurrentLesson;
                currentResult.Board = CurrentBoardIndex + 1;
                results.AddResult(Lesson.LessonNr, CurrentBoardIndex, currentResult);
            }
            await DisplayAlert("Info", $"Hand is done. Contract:{auction.currentContract}", "OK");
            await UploadResultsAsync();
            CurrentBoardIndex = GetNextBoardNumber();

            var resultsFile = Path.Combine(FileSystem.AppDataDirectory, "results.json");
            await File.WriteAllTextAsync(resultsFile, JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));

            await StartNextBoard();
        }
    }

    private int GetNextBoardNumber()
    {
        if (repeatMistakesMode)
        {
            var wrongBoards = results.AllResults[CurrentLesson].GetWrongBoards();
            return wrongBoards.Last() == CurrentBoardIndex ? -1 : wrongBoards.SkipWhile(x => x != CurrentBoardIndex).ElementAt(1);
        }
        if (CurrentBoardIndex == pbn.Boards.Count - 1)
            return -1;
        return CurrentBoardIndex + 1;
    }

    private async Task UploadResultsAsync()
    {
        var username = Preferences.Get("Username", "");
        if (username == "") return;
        var res = results.AllResults.Values.SelectMany(x => x.Results.Values).ToList();
        await UpdateOrCreateAccount(username, res.Count, res.Count(x => x.AnsweredCorrectly), res.Sum(x => x.TimeElapsed.Ticks));
        return;

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
            if (user == null || string.IsNullOrWhiteSpace(user.Value.id))
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

    private async Task ShowReport()
    {
        await Shell.Current.GoToAsync(DeviceInfo.Platform == DevicePlatform.WinUI ? nameof(ResultsPage2) : nameof(ResultsPage), new Dictionary<string, object> { ["Results"] = results });
    }

    private async void ButtonClickedStartLesson(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(StartPage));
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }

    private async void ButtonClickedNextBoard(object sender, EventArgs e)
    {
        try
        {
            CurrentBoardIndex = GetNextBoardNumber();
            await StartNextBoard();
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }

    private async void ButtonClickedResults(object sender, EventArgs e)
    {
        try
        {
            await ShowReport();
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }

    private async void ButtonClickedLeaderBoard(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(LeaderboardPage));
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }

    private async void ButtonClickedSettings(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            await Start();
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }

    private async void Switch_Toggled(object sender, ToggledEventArgs e)
    {
        try
        {
            isInHintMode = e.Value;
            LabelMode.Text = isInHintMode ? "Hint" : "Bid";
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }

    private async void ButtonClickedShowLog(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(LogPage));
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }
}