using System;
using System.Diagnostics;
using Foundation;
using Maui.Phone.Interfaces;
using UIKit;
using Microsoft.Extensions.Logging;

namespace Maui.Phone.Services
{
	public partial class DeviceOrientationService : IDeviceOrientationService
	{
        public DeviceOrientationService() { }

		public partial DeviceOrientation GetOrientation()
		{
            _logger.LogInformation("GetOrientation()");
            UIInterfaceOrientation orientation = (UIInterfaceOrientation)(UIDevice.CurrentDevice.ValueForKey(new NSString("orientation")) as NSNumber).Int32Value;
            bool isPortrait = orientation == UIInterfaceOrientation.Portrait || orientation == UIInterfaceOrientation.PortraitUpsideDown;
            return isPortrait ? DeviceOrientation.Portrait : DeviceOrientation.Landscape;
        }

		public partial void SetOrientation(DeviceOrientation orientation)
		{
            _logger.LogInformation("Setting Orientation to {Orientation}", orientation);
            UIInterfaceOrientation newValue;
            UIInterfaceOrientationMask maskValue;
            switch (orientation)
			{
				case DeviceOrientation.Landscape:
                    newValue = UIInterfaceOrientation.LandscapeLeft;
                    maskValue = UIInterfaceOrientationMask.LandscapeLeft;
					break;
				case DeviceOrientation.Portrait:
				case DeviceOrientation.Undefined:
				default:
                    newValue = UIInterfaceOrientation.Portrait;
                    maskValue = UIInterfaceOrientationMask.Portrait;
                    break;
            }

            AppDelegate.OrientationLock = maskValue;
            try
            {
                //UIDevice.CurrentDevice.SetValueForKey(NSNumber.FromNInt((int)newValue), new NSString("orientation"));
                if (UIDevice.CurrentDevice.CheckSystemVersion(16, 0))
                {
#pragma warning disable CA1422 // There is no alternative currently, even though Apple "deprecated" it. 
                    (UIApplication.SharedApplication.KeyWindow.RootViewController as UINavigationController)
                        .SetNeedsUpdateOfSupportedInterfaceOrientations();
#pragma warning restore CA1422

                    Platform.GetCurrentUIViewController()?.SetNeedsUpdateOfSupportedInterfaceOrientations();
                    Platform.GetCurrentUIViewController()?.View.Window.WindowScene.RequestGeometryUpdate(
                        new UIWindowSceneGeometryPreferencesIOS(maskValue),
                            error =>
                            {
                                _logger.LogError("Problem orienting screen: {Error}", error.LocalizedDescription);
                            });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting orientation");
            }
        }

        public partial int GetNavBarHeight()
        {
            var height = Platform.GetCurrentUIViewController().View.Window.WindowScene.StatusBarManager.StatusBarFrame.Height;
            _logger.LogInformation("NavBar Height: {Height} {HeightInt}", height, (int)height);
            //UIApplication.SharedApplication.StatusBarFrame.Height;
            if (height == 0)
                height = 10;
            return (int)height;

        }
    }
}

