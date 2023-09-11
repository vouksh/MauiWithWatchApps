using System;
using Maui.Phone.Interfaces;
using Microsoft.Extensions.Logging;

namespace Maui.Phone.Services
{
	public partial class DeviceOrientationService : IDeviceOrientationService
	{
		private readonly ILogger<DeviceOrientationService> _logger;
		public DeviceOrientationService(ILogger<DeviceOrientationService> logger)
		{
			_logger = logger;
		}
		public partial DeviceOrientation GetOrientation();
		public partial void SetOrientation(DeviceOrientation orientation);
        public partial int GetNavBarHeight();

        public double GetScreenHeight()
        {
            var density = DeviceDisplay.MainDisplayInfo.Density;
            var height = DeviceDisplay.MainDisplayInfo.Height / density;
            _logger.LogInformation("Screen Height: {Height}", height);
            return height;
        }

        public double GetScreenWidth()
        {
            var density = DeviceDisplay.MainDisplayInfo.Density;
            var width = DeviceDisplay.MainDisplayInfo.Width / density;
            _logger.LogInformation("Screen Width: {Width}", width);
            return width;
        }
    }
}

