using Android.App;
using Android.Content;
using Android.Gms.Auth.Api.SignIn;
using Android.Runtime;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Serilog;

[assembly: UsesPermission(Android.Manifest.Permission.ReadExternalStorage, MaxSdkVersion = 32)]

namespace Maui.Phone;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}