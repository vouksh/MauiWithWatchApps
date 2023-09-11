using Foundation;
using UIKit;

namespace Maui.Phone;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        base.FinishedLaunching(application, launchOptions);
        //Platform.Init(Platform.GetCurrentUIViewController);
        return true;
    }

    [Export("application:supportedInterfaceOrientationsForWindow:")]
    public UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, UIWindow forWindow)
    {
        return OrientationLock;
    }

    public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
    {
        return Platform.OpenUrl(application, url, options);
    }

    public static UIInterfaceOrientationMask OrientationLock { get; set; } = UIInterfaceOrientationMask.Portrait;
}