using System;
namespace Maui.Phone.Interfaces
{
	public interface IDeviceOrientationService
	{
        public DeviceOrientation GetOrientation();
        public void SetOrientation(DeviceOrientation orientation);
        public double GetScreenHeight();
        public double GetScreenWidth();
        public int GetNavBarHeight();
    }

    public enum DeviceOrientation
    {
        Portrait,
        Landscape,
        Undefined
    }
}

