using Android;
using Android.Gms.Common.Apis;
using Android.Gms.Extensions;
using Android.Gms.Wearable;
using Maui.WatchCommunication.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Compatibility;
using System.Text;
using System.Text.Json;

namespace Maui.WatchCommunication
{
    public class WearableDevice : Java.Lang.Object, MessageClient.IOnMessageReceivedListener, DataClient.IOnDataChangedListener, CapabilityClient.IOnCapabilityChangedListener
    {
        //private GoogleApiClient _apiClient;
        private string _primaryDeviceId = string.Empty;
        private ILogger<WearableDevice> _logger;
        public delegate void WearableDeviceMessageReceivedDelegate(ICommunicationPacket commandObject);
        public event WearableDeviceMessageReceivedDelegate MessageReceived;
        private readonly MessageClient _messageClient;
        private readonly DataClient _dataClient;
        private readonly CapabilityClient _capabilityClient;
        private readonly NodeClient _nodeClient;
        //private readonly ChannelClient _channelClient;
        public const string MessagePath = "/message-data";
        public const string FileTransfer = "/file-transfer";
        public const string CAPABILITY_KINESIAU_WEAR = "kinesiau_watch_app";
        private INode? _primaryNode;
        
        public WearableDevice(ILogger<WearableDevice> logger)
        {
            _logger = logger;
            _messageClient = WearableClass.GetMessageClient(Platform.AppContext);
            _dataClient = WearableClass.GetDataClient(Platform.AppContext);
            _capabilityClient = WearableClass.GetCapabilityClient(Platform.AppContext);
            _nodeClient = WearableClass.GetNodeClient(Platform.AppContext);
            //_channelClient = WearableClass.GetChannelClient(Platform.AppContext);
            Connect();
        }

        public async void Connect()
        {
            await _messageClient.AddListenerAsync(this);
            await _dataClient.AddListenerAsync(this);
            await _capabilityClient.AddListenerAsync(this, CAPABILITY_KINESIAU_WEAR);
            if (await VerifyCompanionApp() && _primaryNode != null)
            {
                _logger.LogInformation("Companion app found and paired to device {DeviceName} ({DeviceID})", _primaryNode.DisplayName, _primaryNode.Id);
            }
            //await GetDeviceId();
        }

        public async void Disconnect()
        {
            await _messageClient.RemoveListenerAsync(this);
            await _dataClient.RemoveListenerAsync(this);
            await _capabilityClient.RemoveListenerAsync(this, CAPABILITY_KINESIAU_WEAR);
        }

        public async Task<bool> VerifyCompanionApp()
        {
            var capabilityInfo = await _capabilityClient.GetCapabilityAsync(CAPABILITY_KINESIAU_WEAR, CapabilityClient.FilterAll);
            if (capabilityInfo != null)
            {
                if (!capabilityInfo.Nodes.Any())
                {
                    _logger.LogError("No devices found with companion app");
                    return false;
                }
                else
                {
                    _logger.LogInformation("Found {Count} devices with companion app", capabilityInfo.Nodes.Count);
                    _primaryDeviceId = capabilityInfo.Nodes.First().Id;
                    _primaryNode = capabilityInfo.Nodes.First();
                    return true;
                }
            }
            return false;
        }

        private async Task GetDeviceId()
        {
            try
            {
                var existingDevice = string.Empty; //Preferences.Default.Get<string>("PrimaryDeviceId", string.Empty);
                if (string.IsNullOrEmpty(existingDevice))
                {
                    var nodes = await WearableClass.GetNodeClient(Platform.AppContext).GetConnectedNodesAsync();

                    foreach (var node in nodes)
                    {
                        if (node.IsNearby)
                        {
                            existingDevice = node.Id;
                            _logger.LogInformation("Set paired device {DeviceName} ({DeviceID})", node.DisplayName, existingDevice);
                        }
                        break;
                    }
                    Preferences.Default.Set("PrimaryDeviceId", existingDevice);
                }
                _primaryDeviceId = existingDevice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to set device ID");
            }
        }

        public async void SendMessage(string command, string stringData = "", double numberData = double.NaN, IEnumerable<object>? byteData = null)
        {
            if (string.IsNullOrEmpty(_primaryDeviceId))
            {
                await GetDeviceId();
            }
            var cmdObj = new CommandObject
            {
                Command = command,
                StringData = stringData,
                NumberData = 0,
                TimeStamp = DateTime.Now,
                BinaryData = byteData?.ToArray() ?? Array.Empty<object>()
            };
            try
            {
                _logger.LogInformation("Sending command {Command} to Node ID {NodeId}", command, _primaryDeviceId);
                await WearableClass.GetMessageClient(Platform.AppContext).SendMessage(_primaryDeviceId, MessagePath, cmdObj.GetPayload(), new MessageOptions(MessageOptions.MessagePriorityHigh));
                var fileName = $"{DateTime.Now:yyyy-MM-dd}.{DateTime.Now:HHmmss}.{command}";
                //var tempPath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);
                //await File.WriteAllBytesAsync(tempPath, cmdObj.GetPayload());
                //var dataRequest = PutDataRequest.Create(FileTransfer);
                var dataRequest = PutDataMapRequest.Create(FileTransfer);
                dataRequest.DataMap.PutByteArray("COMMAND", cmdObj.GetPayload());
                //Asset asset = Asset.CreateFromBytes(cmdObj.GetPayload());
                //dataRequest.PutAsset(fileName, asset);
                dataRequest.SetUrgent();

                var dataPut = await WearableClass.GetDataClient(Platform.AppContext).PutDataItemAsync(dataRequest.AsPutDataRequest());
                //File.Delete(tempPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to watch");
            }
        }

        public void OnDataChanged(DataEventBuffer dataEvents)
        {
            foreach (var dataEvent in dataEvents.Where(de => de.Type == DataEvent.TypeChanged && de.DataItem.Uri.Host == _primaryNode?.Id).ToList())
            {
                try
                {
                    var item = DataMapItem.FromDataItem(dataEvent.DataItem);
                    var cmdBytes = item.DataMap.GetByteArray("COMMAND");
                    var cmdObj = JsonSerializer.Deserialize<CommandObject>(cmdBytes);
                    _logger.LogInformation(cmdObj.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Data Item not a valid command object");
                }
            }
        }

        public void OnMessageReceived(IMessageEvent messageEvent)
        {
            if (messageEvent.SourceNodeId == _primaryDeviceId)
            {
                switch(messageEvent.Path)
                {
                    case MessagePath:
                        var messageBytes = messageEvent.GetData();
                        var messageString = Encoding.UTF8.GetString(messageBytes);
                        var messageData = JsonSerializer.Deserialize<CommandObject>(messageString);
                        var packet = new CommunicationPacket
                        {
                            Phrase = Phrase.Received,
                            Command = CommandType.ContextUpdate,
                            CommandData = messageData,
                            ErrorMessage = string.Empty
                        };
                        MessageReceived?.Invoke(packet);
                        if (messageData != null)
                            _logger.LogInformation("Message from watch: {Command} ({Data})", messageData.Command, messageData.StringData);
                        break;
                }
            } 
            else
            {
                _logger.LogWarning("Received message from unknown wearable device: {DeviceId}", messageEvent.SourceNodeId);
            }
        }

        public void OnCapabilityChanged(ICapabilityInfo capabilityInfo)
        {
            _logger.LogInformation("OnCapabilityChanged");
        }
    }
}
