using System.Collections.ObjectModel;
using MvvmHelpers;
using MvvmHelpers.Commands;
using MvvmHelpers.Interfaces;
using Newtonsoft.Json;

namespace Maui.BidTrainer.ViewModels
{
    public class StartViewModel : ObservableObject
    {
        public ObservableCollection<Lesson> Lessons { get; set; }
        public IAsyncCommand<int> StartLessonCommand { get; set; } = new AsyncCommand<int>(ChooseLesson);
        public IAsyncCommand ContinueWhereLeftOffCommand { get; set; } = new AsyncCommand(ContinueWhereLeftOff);

        public async Task LoadLessonsAsync()
        {
            using var reader = new StreamReader(await FileSystem.Current.OpenAppPackageFileAsync("lessons.json"));
            Lessons = JsonConvert.DeserializeObject<ObservableCollection<Lesson>>(await reader.ReadToEndAsync());
            OnPropertyChanged(nameof(Lessons));
        }

        private static async Task ChooseLesson(int lessonNr)
        {
            Preferences.Set("CurrentLesson", lessonNr);
            Preferences.Set("CurrentBoardIndex", 0);
            await Shell.Current.GoToAsync("BidTrainer");
        }

        private static async Task ContinueWhereLeftOff()
        {
            await Shell.Current.GoToAsync("BidTrainer");
        }
    }
}
