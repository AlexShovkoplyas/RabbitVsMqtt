using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Protocol;
using Newtonsoft.Json;

namespace ChatWithMqtt.ChatClient
{
    public class MqttChatClient : IChatClient
    {
        private readonly List<(string chatRoomName, BlockingCollection<MessageInfo> messageQueue)> _chatsHistory = 
            new List<(string chatRoomName, BlockingCollection<MessageInfo> messageQueue)>();

        private readonly IMqttClient _mqttClient;
        private readonly string _userName;

        private List<string> _chatRoomsList;
        private List<string> _currentChatRegisteredUsers;
        private List<string> _currentChatOnlineUsers;
        private List<string> _currentChatOfflineUsers;
        private string _currentChatRoom;

        public MqttChatClient(string userUserName)
        {
            _userName = userUserName;
            _mqttClient = new MqttFactory().CreateMqttClient();
        }

        private CancellationTokenSource _messagesCancellationTokenSource = new CancellationTokenSource();

        public async Task Connect(ClientConfiguration configuration)
        {
            var lastWillMessage = new MqttApplicationMessageBuilder()
                .WithTopic(TopicsHelper.GetOfflineUsersTopic(_userName))
                .WithPayload("true")
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(true)
                .Build();

            var options = new MqttClientOptionsBuilder()
                .WithClientId(configuration.ClientId)
                .WithCleanSession(false)
                .WithTcpServer(configuration.Host, configuration.Port)
                .WithCredentials(configuration.Username, configuration.Password)
                .WithKeepAlivePeriod(TimeSpan.FromMilliseconds(configuration.KeepAlivePeriod))
                .WithCommunicationTimeout(TimeSpan.FromMilliseconds(configuration.CommunicationTimeout))
                .WithWillMessage(lastWillMessage)
                .Build();

            _mqttClient.UseApplicationMessageReceivedHandler(MessageHandler);
            await _mqttClient.ConnectAsync(options);

            //temporary solution
            await PublishRoomList();

            //Update status as online
            var userOnlineMessage = new MqttApplicationMessageBuilder()
                .WithTopic(TopicsHelper.GetOfflineUsersTopic(_userName))
                .WithPayload("false")
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(true)
                .Build();
            await _mqttClient.PublishAsync(userOnlineMessage);

            await _mqttClient.SubscribeAsync(TopicsHelper.AvailableRoomsListSubscription);
        }

        public async Task Disconnect()
        {
            _mqttClient.ApplicationMessageReceivedHandler = null;
            await _mqttClient.DisconnectAsync();
        }

        public async Task SubscribeChatRoom(string chatRoomName)
        {
            _chatsHistory.Add((chatRoomName, new BlockingCollection<MessageInfo>(new ConcurrentQueue<MessageInfo>())));

            await _mqttClient.SubscribeAsync(TopicsHelper.GetChatRoomMessagesTopic(chatRoomName, "+"), MqttQualityOfServiceLevel.AtLeastOnce);

            var messageMqtt = new MqttApplicationMessageBuilder()
                .WithTopic(TopicsHelper.GetSignedUsersTopic(chatRoomName, _userName))
                .WithPayload("true")
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(true)
                .Build();

            await _mqttClient.PublishAsync(messageMqtt);
        }

        public async Task UnsubscribeChatRoom(string chatRoomName)
        {
            await _mqttClient.UnsubscribeAsync(TopicsHelper.GetChatRoomMessagesTopic(chatRoomName, "+"));

            _chatsHistory.Remove(_chatsHistory.Single(i => i.chatRoomName == chatRoomName));

            var messageMqtt = new MqttApplicationMessageBuilder()
                .WithTopic(TopicsHelper.GetSignedUsersTopic(chatRoomName, _userName))
                .WithPayload(new byte[0])
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(true)
                .Build();

            await _mqttClient.PublishAsync(messageMqtt);
        }

        public async Task EnterChatRoom(string chatRoomName, Action<MessageInfo, bool> messageProcessingAction)
        {
            _currentChatRegisteredUsers = new List<string>();
            _currentChatOfflineUsers = new List<string>();
            _currentChatOnlineUsers = new List<string>();

            await _mqttClient.SubscribeAsync(TopicsHelper.GetSignedUsersTopic(_currentChatRoom, "+"), MqttQualityOfServiceLevel.AtLeastOnce);
            await _mqttClient.SubscribeAsync(TopicsHelper.GetOnlineUsersTopic(_currentChatRoom, "+"), MqttQualityOfServiceLevel.AtLeastOnce);

            var messageMqtt = new MqttApplicationMessageBuilder()
                .WithTopic(TopicsHelper.GetOnlineUsersTopic(chatRoomName, _userName))
                .WithPayload("true")
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(true)
                .Build();

            await _mqttClient.PublishAsync(messageMqtt);

            _currentChatRoom = chatRoomName;

            _messagesCancellationTokenSource = new CancellationTokenSource();

            _ = Task.Run(() =>
            {
                foreach (var message in _chatsHistory.Single(c => c.chatRoomName == _currentChatRoom).messageQueue
                    .GetConsumingEnumerable(_messagesCancellationTokenSource.Token))
                {
                    messageProcessingAction(message, _userName == message.UserName);
                }
            });
        }

        public async Task LeaveChatRoom()
        {
            await _mqttClient.UnsubscribeAsync(TopicsHelper.GetSignedUsersTopic(_currentChatRoom, "+"));
            await _mqttClient.UnsubscribeAsync(TopicsHelper.GetOnlineUsersTopic(_currentChatRoom, "+"));

            _messagesCancellationTokenSource.Cancel();

            var messageMqtt = new MqttApplicationMessageBuilder()
                .WithTopic(TopicsHelper.GetOnlineUsersTopic(_currentChatRoom, _userName))
                .WithPayload("false")
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(true)
                .Build();

            await _mqttClient.PublishAsync(messageMqtt);

            _currentChatRoom = null;
        }

        public async Task PublishMessage(string message)
        {
            if (_currentChatRoom is null)
                return;

            var messageMqtt = new MqttApplicationMessageBuilder()
                .WithTopic(TopicsHelper.GetChatRoomMessagesTopic(_currentChatRoom, _userName))
                .WithPayload(message)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();

            await _mqttClient.PublishAsync(messageMqtt);
        }

        private async Task PublishRoomList()
        {
            var message = JsonConvert.SerializeObject(new List<string> { "Room1", "Room2", "Room3" });

            var messageMqtt = new MqttApplicationMessageBuilder()
                .WithTopic(TopicsHelper.AvailableRoomsListSubscription)
                .WithPayload(message)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(true)
                .Build();

            await _mqttClient.PublishAsync(messageMqtt);
        }

        private Task MessageHandler(MqttApplicationMessageReceivedEventArgs args)
        {
            string chat = null;
            string user = null;

            var topic = args.ApplicationMessage.Topic;
            var message = Encoding.Default.GetString(args.ApplicationMessage.Payload);

            if (TopicsHelper.IsChatRoomsListTopic(topic))
            {
                _chatRoomsList = JsonConvert.DeserializeObject<List<string>>(message);
            }
            else if (TopicsHelper.IsChatRoomMessagesTopic(topic, out chat, out user))
            {
                _chatsHistory.Single(i => i.chatRoomName == chat).messageQueue.Add(new MessageInfo(user, DateTime.Now, message));
            }
            else if (TopicsHelper.IsSignedUsersTopic(topic, out chat, out user))
            {
                if (message == "true")
                {
                    if (!_currentChatRegisteredUsers.Contains(user))
                    {
                        _currentChatRegisteredUsers.Add(user);
                    }
                }
                else if (message == "false")
                {
                    if (_currentChatRegisteredUsers.Contains(user))
                    {
                        _currentChatRegisteredUsers.Remove(user);
                    }
                }
            }
            else if (TopicsHelper.IsOnlineUsersTopic(topic, out chat, out user))
            {
                if (message == "true")
                {
                    if (!_currentChatOnlineUsers.Contains(user))
                    {
                        _currentChatOnlineUsers.Add(user);
                        Task.Run(async () =>
                        {
                            await _mqttClient.SubscribeAsync(TopicsHelper.GetOfflineUsersTopic(user),
                                MqttQualityOfServiceLevel.AtLeastOnce);
                        });
                    }
                }
                else if (message == "false")
                {
                    if (_currentChatOnlineUsers.Contains(user))
                    {
                        _currentChatOnlineUsers.Remove(user);
                        Task.Run(async () =>
                        {
                            await _mqttClient.UnsubscribeAsync(TopicsHelper.GetOfflineUsersTopic(user));
                        });
                    }
                }
            }
            else if (TopicsHelper.IsOfflineUsersTopic(topic, out user))
            {
                if (message == "true")
                {
                    if (!_currentChatOfflineUsers.Contains(user))
                    {
                        _currentChatOfflineUsers.Add(user);
                    }
                }
                else if (message == "false")
                {
                    if (_currentChatOfflineUsers.Contains(user))
                    {
                        _currentChatOfflineUsers.Remove(user);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public List<string> GetChatRooms()
        {
            return _chatRoomsList;
        }

        public List<string> GetRegisteredUsers()
        {
            return _currentChatRegisteredUsers;
        }

        public List<string> GetOnlineUsers()
        {
            return _currentChatOnlineUsers.Where(u => !_currentChatOfflineUsers.Contains(u)).ToList();
        }
    }
}