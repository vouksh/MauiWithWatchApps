using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.WatchCommunication.Shared
{
    public delegate void WatchResponseReceivedEventDelegate(object sender, WatchResponseEventArgs e);
    public class WatchResponseEventArgs : EventArgs
    {
        public WatchResponseEventArgs(DateTime timeStamp, string command, string? stringParam = null, double? numberParam = null)
        {
            TimeStamp = timeStamp;
            Command = command;
            StringParam = stringParam;
            NumberParam = numberParam;
        }

        public static WatchResponseEventArgs Create(DateTime timeStamp, string command, string? stringParam = null, double? numberParam = null)
        {
            return new WatchResponseEventArgs(timeStamp, command, stringParam, numberParam);
        }
        public DateTime TimeStamp { get; set; }
        public string Command { get; set; } = string.Empty;
        public string? StringParam { get; set; }
        public double? NumberParam { get; set; }
    }
}
