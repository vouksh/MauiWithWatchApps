using Foundation;
using Maui.WatchCommunication.Interfaces;
using Maui.WatchCommunication.Platforms.iOS;
using Maui.WatchCommunication.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using WatchConnectivity;

namespace Maui.WatchCommunication
{
    public partial class Handler : NSObject
    {
        private WCSession _session;
        private SessionDelegate _sessionDelegate;

        partial void Initialize()
        {
            _sessionDelegate = new SessionDelegate();
            _session = WCSession.DefaultSession;
            _session.Delegate = _sessionDelegate;

            try
            {
                InvokeOnMainThread(() =>
                {
                    if (WCSession.IsSupported)
                    {
                        _session.ActivateSession();
                    } else
                    {
                        _logger.LogError("Watch session not supported");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating watch session");
            }
            NSNotificationCenter.DefaultCenter.AddObserver((NSString)NotificationName.DataDidFlow, DataDidFlow);
        }

        public bool WatchIsReachable
        {
            get
            {
                _logger.LogInformation("WatchIsReachable - Paired: {Paired}; Installed: {Installed}; Reachable: {Reachable}", _session.Paired, _session.WatchAppInstalled, _session.Reachable);
                return  _session != null &&
                        _session.Paired &&
                        _session.WatchAppInstalled &&
                        _session.Reachable;
            }
        }
            

        private void DataDidFlow(NSNotification notification)
        {
            if (notification.Object is CommunicationPacket packet)
            {
                ProcessResponse(packet);
            }
        }

        public partial void ProcessResponse(ICommunicationPacket packet)
        {
            _logger.LogInformation("Received communcation packet, sent at {TimeStamp:G}", packet.CommandData.TimeStamp);
            if (packet is CommunicationPacket commPacket)
            {
                switch (commPacket.Command)
                {
                    case CommandType.ContextUpdate:
                    case CommandType.UserInfo:
                    case CommandType.Message:
                        HandleCommand(commPacket);
                        break;
                    case CommandType.FileTransfer:
                        HandleFileTransfer(commPacket);
                        break;
                }
            }
        }

        private void HandleCommand(CommunicationPacket packet)
        {
            var command = packet.CommandData.Command;
            var commandArgs = packet.CommandData.StringData;
            var timeStamp = packet.CommandData.TimeStamp;
            var commandNumber = packet.CommandData.NumberData;

            switch(command)
            {
                case Constants.Watch.WatchStatus:
                    _logger.LogInformation("Watch status: {AppStatus}", (AppStatus)commandNumber);
                    OnWatchResponseReceived(WatchResponseEventArgs.Create(timeStamp, command, numberParam: commandNumber));
                    break;
            }
        }

        private void HandleFileTransfer(CommunicationPacket packet)
        {
            if (packet.CommandData is CommandObject commandObject) {
                switch (packet.Phrase)
                {
                    case Phrase.Failed:
                        _logger.LogError("File transfer failed for file {FileName}", packet.File.FileUrl);
                        break;
                    case Phrase.Received:
                        _logger.LogInformation("Watch sent file {FileName}", packet.File.FileUrl);
                        break;
                }
            }
        }

        public partial void SendCommand(string command, string stringData, double numberData)
        {
            if (!WatchIsReachable)
            {
                _logger.LogError("Unable to reach watch to send command {Command}", command);
                return;
            }
            var keys = new[] {
                (NSString) PayloadKey.TimeStamp,
                (NSString) PayloadKey.Command,
                (NSString) PayloadKey.BinaryData,
                (NSString) PayloadKey.StringData,
                (NSString) PayloadKey.NumberData};
            var objects = new NSObject[]
            {
                NSDate.Now,
                new NSString(command),
                new NSArray(),
                new NSString(stringData),
                new NSNumber(numberData)
            };
            var commandDict = new NSDictionary<NSString, NSObject>(keys, objects);
            WCSession.DefaultSession.TransferUserInfo(commandDict);
        }

        public partial void Connect()
        {
            WCSession.DefaultSession.ActivateSession();
        }

        public partial void Disconnect()
        {
            
        }
    }
}
