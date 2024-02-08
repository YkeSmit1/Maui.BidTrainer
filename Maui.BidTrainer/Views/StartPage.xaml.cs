using Maui.BidTrainer.ViewModels;

namespace Maui.BidTrainer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class StartPage
    {
        public StartPage ()
		{
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await ((StartViewModel)BindingContext).LoadLessonsAsync();
        }
    }
}