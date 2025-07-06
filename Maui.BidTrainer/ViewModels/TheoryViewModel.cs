using CommunityToolkit.Mvvm.ComponentModel;

namespace Maui.BidTrainer.ViewModels;


public partial class TheoryViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty] 
    public partial int Lesson {get; set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Lesson = int.Parse((string)query["Lesson"]);
    }
}