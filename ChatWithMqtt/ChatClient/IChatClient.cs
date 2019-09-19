using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatWithMqtt.ChatClient
{
    public interface IChatClient
    {
        Task Connect(ClientConfiguration configuration);

        Task Disconnect();

        Task SubscribeChatRoom(string chatRoomName);

        Task UnsubscribeChatRoom(string chatRoomName);

        Task EnterChatRoom(string chatRoomName, Action<MessageInfo, bool> messageProcessingAction);

        Task LeaveChatRoom();

        Task PublishMessage(string message);

        List<string> GetChatRooms();

        List<string> GetRegisteredUsers();

        List<string> GetOnlineUsers();
    }
}