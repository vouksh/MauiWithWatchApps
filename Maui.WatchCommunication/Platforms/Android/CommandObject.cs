using Maui.WatchCommunication.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Maui.WatchCommunication
{
    public class CommandObject : ICommandObject
    {
        public DateTime TimeStamp { get; set; }
        public string Command { get; set; }
        public object[] BinaryData { get; set; }
        public string StringData { get; set; }
        public double NumberData { get; set; }

        public byte[] GetPayload() 
        { 
            var jsonString = JsonSerializer.Serialize(this);
            return Encoding.UTF8.GetBytes(jsonString);
        }
    }

    public class CommunicationPacket : ICommunicationPacket
    {
        public CommandType Command { get; set; }
        public Phrase Phrase { get; set; }
        public ICommandObject CommandData { get; set; }
        public string ErrorMessage { get; set; }
    }
}
