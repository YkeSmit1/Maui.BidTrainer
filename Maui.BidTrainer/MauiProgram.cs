using Maui.BidTrainer.Services;
using Maui.BidTrainer.ViewModels;
using Serilog;
#if DEBUG
    using Microsoft.Extensions.Logging;
#endif

namespace Maui.BidTrainer;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("RobotoMono-Regular.ttf", "RobotoMonoRegular");
            });
        
        IServiceCollection services = builder.Services;

        var combine = Path.Combine(FileSystem.AppDataDirectory, "logs", "log.txt");
        services.AddSerilog(
            new LoggerConfiguration()
                .WriteTo.Debug()
                .WriteTo.File(combine, rollingInterval: RollingInterval.Day)
                .CreateLogger());
        services.AddLogging(logging => logging.AddSerilog());
        
        // Services
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton<BoardService>();
        builder.Services.AddSingleton<BidService>();
        builder.Services.AddSingleton<ResultsService>();
        builder.Services.AddSingleton<CardService>();
        builder.Services.AddSingleton<LessonService>();
        
        // ViewModels
        builder.Services.AddTransient<StartViewModel>();
        builder.Services.AddTransient<BidTrainerViewModel>();
        builder.Services.AddTransient<BiddingBoxViewModel>();
        builder.Services.AddTransient<HandViewModel>();
        
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}