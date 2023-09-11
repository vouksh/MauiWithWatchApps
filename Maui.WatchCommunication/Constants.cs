using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.WatchCommunication
{
    public struct Constants
    {
        public struct Phone
        {
            public const string QueryStatus = "QUERY_STATUS";
            public const string TransferFiles = "TRANSFER_FILES";
        }

        public struct Watch
        {
            public const string WatchStatus = "WATCH_STATUS";
        }
    }

    public enum AppStatus
    {
        Unknown,
        Running,
        Stopped,
        Error,
        Syncing
    }
    public enum CommandType
    {
        ContextUpdate,
        Message,
        FileTransfer,
        UserInfo
    }

    public enum Phrase
    {
        Updated,
        Sent,
        Received,
        Replied,
        Transferring,
        Canceled,
        Finished,
        Failed,
    }

    public struct PayloadKey
    {
        public const string TimeStamp = "TimeStamp";
        public const string Command = "Command";
        public const string BinaryData = "BinaryData";
        public const string StringData = "StringData";
        public const string NumberData = "NumberData";
    }
}
