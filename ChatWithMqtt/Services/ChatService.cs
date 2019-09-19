using System.Collections.Generic;
using ChatWithMqtt.ChatClient;
using ChatWithMqtt.MenuScreens;
using ChatWithMqtt.UserInterface;

namespace ChatWithMqtt.Services
{
    public class ChatService : IChatService
    {
        private IChatInterface _chatUi;
        private IChatClient _chatClient;
        private ClientConfiguration _configuration;

        public ChatService(IChatInterface chatUi, IChatClient chatClient, ClientConfiguration configuration)
        {
            (_chatUi, _chatClient, _configuration) = (chatUi, chatClient, configuration);
        }

        private MenuScreen _currentScreen;

        public void Start()
        {
            SetupScreen();
            while (true)
            {
                _currentScreen.View();
            }            
        }

        private void SetupScreen()
        {
            var chatMenu = new ChatMenuScreen(_chatUi);
            var mainMenu = new MainMenuScreen(_chatUi);

            mainMenu.MenuItems = new List<MenuItem>
            {
                new MenuItem("0", "Connect", (_) => { _chatClient.Connect(_configuration); }, null),
                new MenuItem("1", "List chat rooms", (_) => { _chatUi.PrintAvailableRooms(_chatClient.GetChatRooms()); }, null),
                new MenuItem("2", "Subscribe chat room", (room) => _chatClient.SubscribeChatRoom(room).Wait(), null),
                new MenuItem("3", "Unsubscribe chat room", (room) => _chatClient.UnsubscribeChatRoom(room).Wait(), null),
                new MenuItem("4", "Enter chat room", (room) => _chatClient.EnterChatRoom(room, _chatUi.PrintMessage).Wait(), chatMenu),
                new MenuItem("5", "Leave chat room", (_) => _chatClient.LeaveChatRoom().Wait(), null),
                new MenuItem("6", "Disconnect", (_) => _chatClient.Disconnect().Wait(), null),
            };

            chatMenu.MenuItems = new List<MenuItem>
            {
                new MenuItem("10", "Connect", (_) => _chatClient.Connect(_configuration), null),
                new MenuItem("11", "List signed users", (_) => { _chatUi.PrintSignedUsers(_chatClient.GetRegisteredUsers()); }, null),
                new MenuItem("12", "List online users", (_) =>{_chatUi.PrintOnlineUsers(_chatClient.GetOnlineUsers());}, null),
                new MenuItem("13", "Publish message", (msg) => _chatClient.PublishMessage(msg), null),
                new MenuItem("14", "Leave room", (_) => _chatClient.LeaveChatRoom(), mainMenu),
                new MenuItem("15", "Disconnect", (_) => _chatClient.Disconnect(), null),
            };

            _currentScreen = mainMenu;
        }
    }
}