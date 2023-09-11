using Maui.WatchCommunication.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.WatchCommunication.Interfaces
{
    public interface IHandler
    {
        public event WatchResponseReceivedEventDelegate WatchResponseReceived;
        void SendCommand(string command, string stringData, double numberData);

        void ProcessResponse(ICommunicationPacket packet);
        void Connect();
        void Disconnect();
    }
}
