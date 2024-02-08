using Android.App;
using Android.Runtime;

namespace Maui.BidTrainer
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
            DependencyService.Register<ICosmosDbHelper, CosmosDbHelper>();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
