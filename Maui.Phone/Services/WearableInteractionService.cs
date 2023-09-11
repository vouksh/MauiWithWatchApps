using Maui.WatchCommunication.Interfaces;
using Microsoft.Extensions.Logging;
using System;
namespace Maui.Phone.Services
{
	public partial class WearableInteractionService
	{
		private readonly IHandler _handler;
		private readonly ILogger<WearableInteractionService> _logger;
		private readonly PageService _pageService;
        public WearableInteractionService(IHandler handler, ILogger<WearableInteractionService> logger, PageService pageService)
        {
            _handler = handler;
            _logger = logger;
            _pageService = pageService;
            _handler.WatchResponseReceived += Handler_WatchResponseReceived;
        }

        public IHandler Handler { get { return _handler; } }

        private void Handler_WatchResponseReceived(object sender, WatchCommunication.Shared.WatchResponseEventArgs e)
        {
            _logger.LogInformation("Received response from watch: {Command} ({StringData}{NumberData})", e.Command, e.StringParam, e.NumberParam);
        }

        public void SendMessage()
        {
            _logger.LogInformation("Sending command to watch");
            _handler.SendCommand(Maui.WatchCommunication.Constants.Phone.QueryStatus, string.Empty, double.NaN);
        }

        public void Connect()
        {
            _handler.Connect();
        }
    }
}

