using Foundation;
using Maui.WatchCommunication.Interfaces;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchConnectivity;

namespace Maui.WatchCommunication.Platforms.iOS
{
    public class CommandObject : ICommandObject
    {
        public CommandObject(NSDictionary commandData)
        {
            if (commandData.TryGetValue((NSString)PayloadKey.TimeStamp, out var timeStamp))
            {
                this.TimeStamp = (timeStamp as NSDate).ToDateTime();
            }
            if (commandData.TryGetValue((NSString)PayloadKey.Command, out var command))
            {
                this.Command = (NSString)command;
            }
            if (commandData.TryGetValue((NSString)PayloadKey.BinaryData, out var binaryData))
            {
                this.BinaryData = (binaryData as NSArray).ToArray<object>();
            }
            if (commandData.TryGetValue((NSString)PayloadKey.StringData, out var stringData))
            {
                this.StringData = (NSString)stringData;
            }
            if (commandData.TryGetValue((NSString)PayloadKey.NumberData, out var numberData))
            {
                this.NumberData = (numberData as NSNumber).DoubleValue;
            }
        }

        public static CommandObject Create(NSData commandData)
        {
            //var data = NSKeyedUnarchiver.UnarchiveTopLevelObject(commandData, out var error);
            var dictClass = NSKeyedUnarchiver.GlobalGetClass(nameof(NSDictionary));
            var data = NSKeyedUnarchiver.GetUnarchivedObject(dictClass, commandData, out var error);
            if (data is NSDictionary dictionary)
            {
                return new CommandObject(dictionary);
            }
            throw new InvalidOperationException("Data provided is not a command dictionary");
        }

        public DateTime TimeStamp { get; set; }

        public string Command { get; set; }

        public object[] BinaryData { get; set; }

        public string StringData { get; set; }
        public double NumberData { get; set; }
    }

    public class CommunicationPacket : NSObject, ICommunicationPacket
    {
        public CommunicationPacket(CommandType commandType, Phrase phrase, NSDictionary commandData = null)
        {
            Command = commandType;
            Phrase = phrase;
            CommandData = commandData != null ? new CommandObject(commandData) : null;
        }
        public CommandType Command { get; set; }
        public Phrase Phrase { get; set; }
        public ICommandObject CommandData { get; set; }
        public WCSessionFileTransfer FileTransfer { get; set; }
        public WCSessionFile File { get; set; }
        public WCSessionUserInfoTransfer UserInfoTransfer { get; set; }
        public string ErrorMessage { get; set; }
    }
}
