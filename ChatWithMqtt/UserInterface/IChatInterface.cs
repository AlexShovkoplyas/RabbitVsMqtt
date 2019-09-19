using System.Collections.Generic;

namespace ChatWithMqtt.UserInterface
{
    public interface IChatInterface
    {
        void PrintMessage(MessageInfo message, bool isIncoming = true);
        void PrintSignedUsers(List<string> users);
        void PrintAvailableRooms(List<string> rooms);
        void PrintOnlineUsers(List<string> users);
        void PrintMenuItems(IEnumerable<(string number, string title)> items);
        string SelectMenuOption();
    }
}