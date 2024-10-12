namespace Maui.BidTrainer.Views;

public partial class LogPage
{
    public LogPage()
    {
        InitializeComponent();
        var combine = Path.Combine(FileSystem.Current.AppDataDirectory, "logs", $"log{DateTime.Now:yyyyMMdd}.txt");
        var destFileName = Path.Combine(FileSystem.Current.AppDataDirectory, "logs", $"log.txt");
        File.Copy(combine, destFileName, true);
        LogLabel.Text = File.ReadAllText(destFileName);
    }
}