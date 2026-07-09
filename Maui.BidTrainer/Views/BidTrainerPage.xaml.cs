using Common;
using Engine.DotNet;
using Maui.BidTrainer.Services;
using Maui.BidTrainer.ViewModels;

namespace Maui.BidTrainer.Views;

public partial class BidTrainerPage
{
    private readonly BoardService boardService;
    private readonly SettingsService settingsService;
    private readonly ResultsService resultService;
    private readonly CardService cardService;
    private readonly LessonService lessonService;

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

    private BoardDto Board => pbn.Boards[CurrentBoardIndex];
    private bool repeatMistakesMode;

    // ViewModels
    private AuctionViewModel AuctionViewModel => (AuctionViewModel)AuctionView.BindingContext;
    private HandViewModel HandViewModelNorth => (HandViewModel)PanelNorth.BindingContext;
    private HandViewModel HandViewModelSouth => (HandViewModel)PanelSouth.BindingContext;
    private BidTrainerViewModel BidTrainerViewModel => (BidTrainerViewModel)BindingContext;

    // Event-handlers
    private readonly EventHandler settingsServiceOnSettingsChanged;
    private readonly EventHandler<BoardService.DisplayAlertEventArgs> onDisplayAlertRequested;
    private readonly EventHandler onAuctionCleared;
    private readonly EventHandler<string> onAuctionBidAdded;
    private readonly EventHandler<BoardService.BoardCompletedEventArgs> onBoardCompleted;

    public BidTrainerPage(SettingsService settingsService, BoardService boardService, ResultsService resultService, CardService cardService, LessonService lessonService)
    {
        InitializeComponent();
        this.settingsService = settingsService;
        this.cardService = cardService;
        this.lessonService = lessonService;
        this.boardService = boardService;
        this.resultService = resultService;
        
        settingsServiceOnSettingsChanged = (_, _) =>
        {
            BidTrainerViewModel.Username = Preferences.Get("Username", "");
            this.cardService.GenerateCardImages();
            ShowBothHands();
        };
        onDisplayAlertRequested = async void (_, args) => await DisplayAlert(args.Title, args.Message, "OK");
        onAuctionCleared = (_, _) => AuctionViewModel.Clear();
        onAuctionBidAdded = (_, bid) => { AuctionViewModel.AddBid(bid); };
        onBoardCompleted = async void (_, result) => await OnBoardCompleted(result);
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        boardService.DisplayAlertRequested += onDisplayAlertRequested;
        boardService.AuctionCleared += onAuctionCleared;
        boardService.AuctionBidAdded += onAuctionBidAdded;
        boardService.BoardCompleted += onBoardCompleted;
        settingsService.SettingsChanged += settingsServiceOnSettingsChanged;
        await Start();
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

    private async Task Start()
    {
        try
        {
            await Initialize();
            await StartLessonAsync();
            await StartNextBoard();
        }
        catch (Exception e)
        {
            await DisplayAlert("Error", e.ToString(), "OK");
        }
    }

    private async Task Initialize()
    {
        await resultService.LoadResultsFromFile();

        await Utils.CopyFileToAppDataDirectory("four_card_majors.db3");
        Api.Setup(Path.Combine(FileSystem.AppDataDirectory, "four_card_majors.db3"));
        cardService.GenerateCardImages();
    }

    private async Task StartLessonAsync()
    {
        var lesson = (await lessonService.GetLessonsAsync()).Single(l => l.LessonNr == CurrentLesson);
        await Utils.CopyFileToAppDataDirectory(lesson.PbnFile);
        await pbn.LoadAsync(Path.Combine(FileSystem.AppDataDirectory, lesson.PbnFile));
        Api.SetModules(lesson.Modules);
        if (CurrentBoardIndex == 0) 
            resultService.RemoveLessonResults(lesson.LessonNr);
        BidTrainerViewModel.Lesson = lesson.LessonNr;
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
                BidTrainerViewModel.Mistake = "Previous mistakes";
            }
            else 
            {
                BidTrainerViewModel.Mistake = "";
                CurrentBoardIndex = 0;

                if (CurrentLesson != (await lessonService.GetLessonsAsync()).Last().LessonNr)
                {
                    var percentage = resultService.GetPercentage(CurrentLesson);
                    await DisplayAlert("Info", $"End of lesson. Your have scored {percentage}", "OK");
                    CurrentLesson++;
                    await Shell.Current.GoToAsync($"{nameof(TheoryPage)}?Lesson={CurrentLesson}");
                    await StartLessonAsync();
                }
                else
                {
                    await DisplayAlert("Info", "End of lessons", "OK");
                    CurrentLesson = 2;
                    await Shell.Current.GoToAsync(nameof(ResultsPage2), new Dictionary<string, object> { ["Results"] = resultService.Results });
                }
            }
        }

        PanelNorth.IsVisible = false;
        BiddingBoxView.IsEnabled = true;
        BidTrainerViewModel.Board = CurrentBoardIndex + 1;
        ShowBothHands();
        boardService.StartBoard(Board);
    }

    private void ShowBothHands()
    {
        HandViewModelNorth.ShowHand(Board.Deal[Player.North]);
        HandViewModelSouth.ShowHand(Board.Deal[Player.South]);
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
}