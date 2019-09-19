using System.Collections.Generic;
using System.Drawing;
using Colorful;
using Console = Colorful.Console;

namespace ChatWithMqtt.UserInterface
{
    public class ChatInterface : IChatInterface
    {

        public void PrintMessage(MessageInfo message, bool isIncoming = true)
        {
            //string messageTemplate = " User {0} DateTime {1} Msg {2}";
            //Formatter[] values = new Formatter[]
            //{
            //    new Formatter(message.UserName, Color.Chocolate),
            //    new Formatter(message.UserName, Color.DarkGray),
            //    new Formatter(message.UserName, Color.White)
            //};
            //Console.WriteLineFormatted(messageTemplate, Color.White, values);
            var prefix = isIncoming ? "-->" : "<--";
            Console.WriteLine($"{prefix} {message.UserName, -10} {message.TimeStamp, 10} {message.Message}");
        }

        public void PrintSignedUsers(List<string> users)
        {
            Console.WriteLine($"Signed users : {string.Join(", ",users)}");
        }

        public void PrintAvailableRooms(List<string> rooms)
        {
            Console.WriteLine($"Available rooms : {string.Join(", ", rooms)}");
        }

        public void PrintOnlineUsers(List<string> users)
        {
            Console.WriteLine($"Online users : {string.Join(", ", users)}");
        }

        public void PrintMenuItems(IEnumerable<(string number, string title)> items)
        {
            Console.WriteLine("Select command from Menu :");
            Console.WriteLine(new string('-', 20));
            foreach (var item in items)
            {
                Console.WriteLine($"{item.number,-5}|{item.title}");
            }
        }

        public string SelectMenuOption()
        {
            Console.Write("Select menu option : ");
            return Console.ReadLine();
        }
    }
}