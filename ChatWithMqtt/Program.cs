using System;
using ChatWithMqtt.ChatClient;
using ChatWithMqtt.Services;
using ChatWithMqtt.UserInterface;

namespace ChatWithMqtt
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter your name");
            var name = Console.ReadLine();

            var chatClient = new MqttChatClient(name);
            var userInterface = new ChatInterface();
            var configuration = new ClientConfiguration()
            {
                ClientId = "Client_" + name,
                Host = "localhost",
                Port = 1883,
                KeepAlivePeriod = 50000,
                CommunicationTimeout = 50000
            };

            IChatService chatService = new ChatService(userInterface, chatClient, configuration);

            chatService.Start();
        }
    }
}
