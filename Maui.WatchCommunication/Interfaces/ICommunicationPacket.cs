using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.WatchCommunication.Interfaces
{
    public interface ICommunicationPacket
    {
        public CommandType Command { get; set; }
        public Phrase Phrase { get; set; }
        public ICommandObject CommandData { get; set; }
        public string ErrorMessage { get; set; }
    }
}
