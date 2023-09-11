using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.WatchCommunication.Interfaces
{
    public interface ICommandObject
    {

        public DateTime TimeStamp { get; set; }

        public string Command { get; set; }

        public object[] BinaryData { get; set; }

        public string StringData { get; set; }
        public double NumberData { get; set; }
    }
}
