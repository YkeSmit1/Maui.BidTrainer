using System.Text.Json;
using Common;
using Engine.DotNet;
using Maui.BidTrainer.Services;
using Maui.BidTrainer.ViewModels;
using Serilog;

namespace Maui.BidTrainer.Views;

public partial class BidTrainerPage
{
    private readonly BoardService boardService;
    private readonly StartPage startPage = new();
    private readonly SettingsService settingsService;
    private readonly ResultsService resultService;
    private Dictionary<(string suit, string card), string> dictionary;

    // Bidding
    private readonly Pbn pbn = new();

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
    private List<Lesson> lessons;
    private Lesson Lesson => lessons.Single(l => l.LessonNr == CurrentLesson);
    private bool repeatMistakesMode;

    // ViewModels
    private AuctionViewModel AuctionViewModel => (AuctionViewModel)AuctionView.BindingContext;
    private HandViewModel HandViewModelNorth => (HandViewModel)PanelNorth.BindingContext;
    private HandViewModel HandViewModelSouth => (HandViewModel)PanelSouth.BindingContext;
    private readonly ILogger logger = IPlatformApplication.Current!.Services.GetService<ILogger>();
    private readonly EventHandler settingsServiceOnSettingsChanged;
    private EventHandler<BoardService.DisplayAlertEventArgs> onDisplayAlertRequested;
    private EventHandler onAuctionCleared;
    private EventHandler<string> onAuctionBidAdded;
    private EventHandler<BoardService.BoardCompletedEventArgs> onBoardCompleted;

    public BidTrainerPage(SettingsService settingsService, BoardService boardService, ResultsService resultService)
    {
        InitializeComponent();
        this.settingsService = settingsService;
        settingsServiceOnSettingsChanged = (_, _) => UpdateUi();
        this.settingsService.SettingsChanged += settingsServiceOnSettingsChanged;
        
        this.boardService = boardService;
        this.resultService = resultService;
        InitializeBoardService();
        
        AuctionViewModel.Bids.Clear();
        _ = Start();
    }

    private void InitializeBoardService()
    {
        onDisplayAlertRequested = async (_, args) => await DisplayAlert(args.Title, args.Message, "OK");
        onAuctionCleared = (_, _) => AuctionViewModel.Bids.Clear();
        onAuctionBidAdded = (_, bid) =>
        {
            if (AuctionViewModel.Bids.Any() && AuctionViewModel.Bids.Last() == "?")
                AuctionViewModel.Bids.RemoveAt(AuctionViewModel.Bids.Count - 1);
            AuctionViewModel.Bids.Add(bid);
        };
        onBoardCompleted = async (_, result) => await OnBoardCompleted(result);
    }
    
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        boardService.DisplayAlertRequested += onDisplayAlertRequested;
        boardService.AuctionCleared += onAuctionCleared;
        boardService.AuctionBidAdded += onAuctionBidAdded;
        boardService.BoardCompleted += onBoardCompleted;
        settingsService.SettingsChanged += settingsServiceOnSettingsChanged;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        boardService.DisplayAlertRequested -= onDisplayAlertRequested;
        boardService.AuctionCleared -= onAuctionCleared;
        boardService.AuctionBidAdded -= onAuctionBidAdded;
        boardService.BoardCompleted -= onBoardCompleted;
        settingsService.SettingsChanged -= settingsServiceOnSettingsChanged;
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
        StatusLabel.Text = $"Username: {Preferences.Get("Username", "")}\nLesson: {Lesson.LessonNr}\nBoard: {CurrentBoardIndex + 1}";
        GenerateCardImages();
    }

    private async Task Initialize()
    {
        using var lessonsReader = new StreamReader(await FileSystem.OpenAppPackageFileAsync("lessons.json"));
        lessons = JsonSerializer.Deserialize<List<Lesson>>(await lessonsReader.ReadToEndAsync());

        await resultService.LoadResultsFromFile();

        await Utils.CopyFileToAppDataDirectory("four_card_majors.db3");
        Api.Setup(Path.Combine(FileSystem.AppDataDirectory, "four_card_majors.db3"));
    }

    private async Task StartLessonAsync()
    {
        await Utils.CopyFileToAppDataDirectory(Lesson.PbnFile);
        await pbn.LoadAsync(Path.Combine(FileSystem.AppDataDirectory, Lesson.PbnFile));
        Api.SetModules(Lesson.Modules);
        if (CurrentBoardIndex == 0) 
            resultService.RemoveLessonResults(Lesson.LessonNr);
    }

    private async Task OnBoardCompleted(BoardService.BoardCompletedEventArgs args)
    {
        BiddingBoxView.IsEnabled = false;
        PanelNorth.IsVisible = true;
        if (!repeatMistakesMode)
        {
            args.Result.Lesson = CurrentLesson;
            args.Result.Board = CurrentBoardIndex + 1;
            resultService.AddResult(args.Result, CurrentBoardIndex);
        }
        await resultService.UploadResultsAsync();
        CurrentBoardIndex = GetNextBoardNumber();

        await resultService.SaveResultsToFile();
        await DisplayAlert("Info", $"Hand is done. Contract:{args.Contract}", "Ok");

        await StartNextBoard();
    }

    private async Task StartNextBoard()
    {
        if (CurrentBoardIndex == -1)
        {
            var wrongBoards = resultService.GetWrongBoards(CurrentLesson);
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
                    var percentage = resultService.GetPercentage(Lesson.LessonNr);
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

        PanelNorth.IsVisible = false;
        BiddingBoxView.IsEnabled = true;
        StatusLabel.Text = $"Username: {Preferences.Get("Username", "")}\nLesson: {Lesson.LessonNr}\nBoard: {CurrentBoardIndex + 1}";
        
        ShowBothHands();
        boardService.StartBoard(pbn.Boards[CurrentBoardIndex]);
    }


    private void ShowBothHands()
    {
        var alternateSuits = Preferences.Get("AlternateSuits", true);
        var cardProfile = Preferences.Get("CardImageSettings", "default");
        HandViewModelNorth.ShowHand(Deal[Player.North], alternateSuits, cardProfile, dictionary);
        HandViewModelSouth.ShowHand(Deal[Player.South], alternateSuits, cardProfile, dictionary);
    }


    private int GetNextBoardNumber()
    {
        if (repeatMistakesMode)
        {
            var wrongBoards = resultService.GetWrongBoards(CurrentLesson);
            return wrongBoards.Last() == CurrentBoardIndex ? -1 : wrongBoards.SkipWhile(x => x != CurrentBoardIndex).ElementAt(1);
        }
        if (CurrentBoardIndex == pbn.Boards.Count - 1)
            return -1;
        return CurrentBoardIndex + 1;
    }

    private async Task ShowReport()
    {
        await Shell.Current.GoToAsync(nameof(ResultsPage2), new Dictionary<string, object> { ["Results"] = resultService.Results });
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

    private async void Switch_Toggled(object sender, ToggledEventArgs e)
    {
        try
        {
            boardService.SetHintMode(e.Value);
            LabelMode.Text = e.Value ? "Hint" : "Bid";
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