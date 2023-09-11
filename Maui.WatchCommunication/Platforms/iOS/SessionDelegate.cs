using CoreFoundation;
using Foundation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchConnectivity;

namespace Maui.WatchCommunication.Platforms.iOS
{
    internal struct NotificationName
    {
        public const string DataDidFlow = "DataDidFlow";
        public const string ActivationDidComplete = "ActivationDidComplete";
        public const string ReachabilityDidChange = "ReachabilityDidChange";
    }
    internal class SessionDelegate : WCSessionDelegate
    {
        public override void ActivationDidComplete(WCSession session, WCSessionActivationState activationState, NSError error)
        {
            var keys = new NSString[]
            {
                new NSString("TimeStamp"),
                new NSString("CommandData"),
                new NSString("NumberData")
            };
            var vals = new NSObject[]
            {
                NSDate.Now,
                new NSString("ActivationState"),
                new NSNumber((int)activationState)
            };
            var commandData = new NSDictionary<NSString, NSObject>(keys, vals);
            var commandStatus = new CommunicationPacket(CommandType.ContextUpdate, Phrase.Updated, commandData);

            this.PostNotificationOnMainQueue(NotificationName.ActivationDidComplete, commandStatus);
        }
        /// <summary>
        /// Called when WCSession reachability is changed.
        /// </summary>
        public override void SessionReachabilityDidChange(WCSession session)
        {
            this.PostNotificationOnMainQueue(NotificationName.ReachabilityDidChange);
        }

        /// <summary>
        /// Called when an app context is received.
        /// </summary>
        public override void DidReceiveApplicationContext(WCSession session, NSDictionary<NSString, NSObject> applicationContext)
        {
            var commandStatus = new CommunicationPacket(CommandType.ContextUpdate, Phrase.Received, applicationContext);
            var command = commandStatus.CommandData.Command;
            var commandArgs = commandStatus.CommandData.StringData;
            var timeStamp = commandStatus.CommandData.TimeStamp;
            var commandNumber = commandStatus.CommandData.NumberData;
            this.PostNotificationOnMainQueue(NotificationName.DataDidFlow, commandStatus);
        }

        /// <summary>
        /// Called when a message is received and the peer doesn't need a response.
        /// </summary>
        public override void DidReceiveMessage(WCSession session, NSDictionary<NSString, NSObject> message)
        {
            var commandStatus = new CommunicationPacket(CommandType.ContextUpdate, Phrase.Received, message);
            this.PostNotificationOnMainQueue(NotificationName.DataDidFlow, commandStatus);
        }

        /// <summary>
        /// Called when a message is received and the peer needs a response.
        /// </summary>
        public override void DidReceiveMessage(WCSession session, NSDictionary<NSString, NSObject> message, WCSessionReplyHandler replyHandler)
        {
            this.DidReceiveMessage(session, message);
            replyHandler(message); // Echo back the time stamp.
        }

        /// <summary>
        /// Called when a piece of message data is received and the peer doesn't need a respons
        /// </summary>
        public override void DidReceiveMessageData(WCSession session, NSData messageData)
        {
            var commandStatus = new CommunicationPacket(CommandType.ContextUpdate, Phrase.Received) { CommandData = CommandObject.Create(messageData) };
            this.PostNotificationOnMainQueue(NotificationName.DataDidFlow, commandStatus);
        }

        /// <summary>
        /// Called when a piece of message data is received and the peer needs a response.
        /// </summary>
        public override void DidReceiveMessageData(WCSession session, NSData messageData, WCSessionReplyDataHandler replyHandler)
        {
            this.DidReceiveMessageData(session, messageData);
            replyHandler(messageData); // Echo back the time stamp.
        }

        /// <summary>
        /// Called when a userInfo is received.
        /// </summary>
        public override void DidReceiveUserInfo(WCSession session, NSDictionary<NSString, NSObject> userInfo)
        {
            var commandStatus = new CommunicationPacket(CommandType.ContextUpdate, Phrase.Received, userInfo);
            var command = commandStatus.CommandData.Command;
            var commandArgs = commandStatus.CommandData.StringData;
            var timeStamp = commandStatus.CommandData.TimeStamp;
            var commandNumber = commandStatus.CommandData.NumberData;
            this.PostNotificationOnMainQueue(NotificationName.DataDidFlow, commandStatus);
        }

        /// <summary>
        /// Called when sending a userInfo is done.
        /// </summary>
        public override void DidFinishUserInfoTransfer(WCSession session, WCSessionUserInfoTransfer userInfoTransfer, NSError error)
        {
            var commandStatus = new CommunicationPacket(CommandType.UserInfo, Phrase.Finished, userInfoTransfer.UserInfo);

            if (error != null)
            {
                commandStatus.ErrorMessage = error.LocalizedDescription;
            }

            this.PostNotificationOnMainQueue(NotificationName.DataDidFlow, commandStatus);
        }

        /// <summary>
        /// Called when a file is received.
        /// </summary>
        public override void DidReceiveFile(WCSession session, WCSessionFile file)
        {
            var commandStatus = new CommunicationPacket(CommandType.FileTransfer, Phrase.Received)
            {
                File = file,
            };
            try
            {
                SaveFile(file);

                DispatchQueue.MainQueue.DispatchSync(() =>
                {
                    NSNotificationCenter.DefaultCenter.PostNotificationName(NotificationName.DataDidFlow,
                        commandStatus);
                });
            }
            catch (Exception ex)
            {
                DispatchQueue.MainQueue.DispatchSync(() =>
                {
                    NSNotificationCenter.DefaultCenter.PostNotificationName(NotificationName.DataDidFlow,
                        new CommunicationPacket(CommandType.FileTransfer, Phrase.Failed) { File = file });
                });
            }
        }

        private void SaveFile(WCSessionFile file)
        {
            var docPath = Path.Combine(AppContext.BaseDirectory, "Storage");
            var sourceFilePath = file.FileUrl.Path;

            if (sourceFilePath is null)
                throw new NullReferenceException($"sourcePath is null for URL ({file.FileUrl})");

            //var fileNameOnly = file.FileUrl.LastPathComponent;
            var fileNameOnly = Path.GetFileName(sourceFilePath);
            var splitFileName = fileNameOnly.Split(fileNameOnly.Contains('-') ? '-' : '.');

            var fileAssessmentId = splitFileName[0];
            if (DateTime.TryParseExact(fileAssessmentId, "yyMMddHHmmss", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal, out _))
            {
                docPath = Path.Combine(docPath, fileAssessmentId);
                Directory.CreateDirectory(docPath);
            }

            var filename = Path.Combine(docPath, fileNameOnly);
            if (File.Exists(filename))
                File.Delete(filename);

            try
            {
                File.Move(sourceFilePath, filename);
            }
            catch (Exception ex)
            {
                //Helper.Log($"Error saving file {fileNameOnly}: {ex}");
            }
        }

        /// <summary>
        /// Called when a file transfer is done.
        /// </summary>
        public override void DidFinishFileTransfer(WCSession session, WatchConnectivity.WCSessionFileTransfer fileTransfer, NSError error)
        {
            var commandStatus = new CommunicationPacket(CommandType.FileTransfer, Phrase.Finished);

            if (error != null)
            {
                commandStatus.ErrorMessage = error.LocalizedDescription;
                this.PostNotificationOnMainQueue(NotificationName.DataDidFlow, commandStatus);
            }
            else
            {
                commandStatus.FileTransfer = fileTransfer;

                var metaData = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(
                    new NSObject[] { new NSString(DateTime.UtcNow.ToString()) },
                                                           new NSString[] { (NSString)PayloadKey.TimeStamp });

                commandStatus.CommandData = new CommandObject(metaData);

                this.PostNotificationOnMainQueue(NotificationName.DataDidFlow, commandStatus);
            }
        }

        public override void DidBecomeInactive(WCSession session)
        {
            Console.WriteLine($"DidBecomeInactive: activationState = {session.ActivationState}");
        }

        public override void DidDeactivate(WCSession session)
        {
            // Activate the new session after having switched to a new watch.
            session.ActivateSession();
        }

        public override void SessionWatchStateDidChange(WCSession session)
        {
            var keys = new NSString[]
            {
                new NSString("TimeStamp"),
                new NSString("CommandData"),
                new NSString("NumberData")
            };
            var vals = new NSObject[]
            {
                NSDate.Now,
                new NSString("ActivationState"),
                new NSNumber((int)session.ActivationState)
            };
            var commandData = new NSDictionary<NSString, NSObject>(keys, vals);
            var commandStatus = new CommunicationPacket(CommandType.ContextUpdate, Phrase.Updated, commandData);
            this.PostNotificationOnMainQueue(NotificationName.ActivationDidComplete, commandStatus);
            //Console.WriteLine($"SessionWatchStateDidChange: activationState = {session.ActivationState}");
        }

        private void PostNotificationOnMainQueue(string name, CommunicationPacket packet = null)
        {
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                NSNotificationCenter.DefaultCenter.PostNotificationName(name, packet);
            });
        }
    }
}
