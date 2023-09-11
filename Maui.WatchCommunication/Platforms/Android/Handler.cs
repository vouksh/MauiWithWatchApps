using Android.Gms.Common.Apis;
using Maui.WatchCommunication.Interfaces;
using Android.Gms.Wearable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Maui.WatchCommunication.Shared;
using Microsoft.Extensions.Logging;

namespace Maui.WatchCommunication
{
    public partial class Handler
    {

        WearableDevice _wearableDevice;
        public WearableDevice WearableDevice
        {
            get { return _wearableDevice; }
        }
        partial void Initialize()
        {
            var wearableLogger = _loggerFactory.CreateLogger<WearableDevice>();
            _wearableDevice = new WearableDevice(wearableLogger);
            _wearableDevice.Connect();
            _wearableDevice.MessageReceived += ProcessResponse;
        }
        public partial void ProcessResponse(ICommunicationPacket packet)
        {
            this.WatchResponseReceived?.Invoke(this, WatchResponseEventArgs.Create(packet.CommandData.TimeStamp, packet.CommandData.Command, packet.CommandData.StringData, packet.CommandData.NumberData));
        }

        public partial void SendCommand(string command, string stringData, double numberData)
        {
            _wearableDevice.SendMessage(command, stringData, numberData);
        }

        public partial void Connect()
        {
            _wearableDevice.Connect();
        }

        public partial void Disconnect()
        {
            _wearableDevice.Disconnect();
        }
    }
}
