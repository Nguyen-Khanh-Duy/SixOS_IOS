using Android.App;
using Android.Runtime;

namespace SixOSDatKhamAppMobile
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }
        [assembly: UsesPermission(Android.Manifest.Permission.Camera)]
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
