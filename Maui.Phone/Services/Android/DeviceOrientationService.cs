using System;
using Android.Content;
using Android.Hardware.Lights;
using Android.Runtime;
using Android.Views;
using Maui.Phone.Interfaces;
using Microsoft.Extensions.Logging;

namespace Maui.Phone.Services
{
    public partial class DeviceOrientationService : IDeviceOrientationService
    {
        private MainActivity _activity;

        public DeviceOrientationService()
        {
            _activity = MainActivity.Instance;
        }

        public partial DeviceOrientation GetOrientation()
        {
            //IWindowManager windowManager = Android.App.Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            SurfaceOrientation orientation = MainActivity.Instance.WindowManager.DefaultDisplay.Rotation;
            bool isLandscape = orientation == SurfaceOrientation.Rotation90 || orientation == SurfaceOrientation.Rotation270;
            return isLandscape ? DeviceOrientation.Landscape : DeviceOrientation.Portrait;
        }

        public partial void SetOrientation(DeviceOrientation orientation)
        {
            switch (orientation)
            {
                case DeviceOrientation.Landscape:
                    MainActivity.Instance.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
                    break;
                case DeviceOrientation.Undefined:
                case DeviceOrientation.Portrait:
                default:
                    MainActivity.Instance.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
                    break;
            }
        }

        public partial int GetNavBarHeight()
        {
            int statusBarHeight = -1;
            int resourceId = MainActivity.Instance.Resources.GetIdentifier("status_bar_height", "dimen", "android");
            if (resourceId > 0)
            {
                statusBarHeight = MainActivity.Instance.Resources.GetDimensionPixelSize(resourceId);
            }
            if (statusBarHeight < 0)
                statusBarHeight = 30;
            _logger.LogInformation("NavBar Height: {Height}", statusBarHeight);
            return statusBarHeight;
        }
    }
}

