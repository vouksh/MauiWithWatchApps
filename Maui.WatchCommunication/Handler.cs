using Maui.WatchCommunication.Interfaces;
using Maui.WatchCommunication.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.WatchCommunication
{
    public partial class Handler : IHandler
    {
        public event WatchResponseReceivedEventDelegate WatchResponseReceived;
        private readonly ILogger<Handler> _logger;
        private readonly ILoggerProvider _loggerProvider;
        private readonly ILoggerFactory _loggerFactory;
        public Handler(ILogger<Handler> logger, ILoggerProvider loggerProvider, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerProvider = loggerProvider;
            _loggerFactory = loggerFactory;
            Initialize();
        }
        partial void Initialize();
        public partial void ProcessResponse(ICommunicationPacket packet);
        public partial void SendCommand(string command, string stringData, double numberData);
        public partial void Connect();
        public partial void Disconnect();
        protected virtual void OnWatchResponseReceived(WatchResponseEventArgs e)
        {
            WatchResponseReceived?.Invoke(this, e);
        }
    }
}
