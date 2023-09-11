using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Wearable;
using Android.OS;
using Maui.Phone.Services;
using Maui.WatchCommunication;
using Serilog;

namespace Maui.Phone;

[Activity(MainLauncher = true, Theme = "@style/Maui.SplashTheme",
	ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
	                       ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public static MainActivity Instance;
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
		Platform.Init(this, savedInstanceState);
		var activityPermission = Manifest.Permission.ActivityRecognition;

        if (Platform.AppContext.CheckSelfPermission(activityPermission) != Permission.Granted)
		{
			RequestPermissions(new string[] { activityPermission }, 1000);
		}
        Instance = this;
    }

    protected override void OnResume()
    {
        base.OnResume();
        var wearableService = MainApplication.Current.Services.GetService<WearableInteractionService>();
        if (wearableService != null)
        {
            var handler = (wearableService.Handler as WatchCommunication.Handler);
            WearableClass.GetDataClient(MainApplication.Current).AddListener(handler.WearableDevice);
            WearableClass.GetMessageClient(MainApplication.Current).AddListener(handler.WearableDevice);
            WearableClass.GetCapabilityClient(MainApplication.Current).AddListener(handler.WearableDevice, WearableDevice.CAPABILITY_KINESIAU_WEAR);
        }
    }

    protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
    }
}