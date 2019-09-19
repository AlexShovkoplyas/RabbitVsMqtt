using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ChatWithMqtt.ChatClient
{
    public static class TopicsHelper
    {
        private const string placeholderPattern = "{.+?}";


        public const string ChatRoomNamePlaceholder = "{chatRoomName}";
        public const string UserPlaceholder = "{user}";

        //topology
        private static string _roomsListTopic = "chatRooms/List";
        //private static string _roomsListSignedTopic = $"chatRooms/List/Signed/{UserPlaceholder}";
        private static string _roomMessageTopic = $"chatRooms/{ChatRoomNamePlaceholder}/{UserPlaceholder}";
        private static string _roomUsersSignedTopic = $"chatRooms/{ChatRoomNamePlaceholder}/users/Signed/{UserPlaceholder}";
        private static string _roomUsersOnlineTopic = $"chatRooms/{ChatRoomNamePlaceholder}/users/Online/{UserPlaceholder}";
        private static string _usersOfflineTopic = $"chatRooms/users/Online/{UserPlaceholder}";

        public static string ReplacePlaceholdersWithWildcard(string topic)
        {
            var regex = new Regex(placeholderPattern);
            return regex.Replace(topic, "+");
        }

        public static string ApplyPlaceholders(string topic, Dictionary<string, string> values)
        {
            string result = topic;
            foreach (var value in values)
            {
                result = result.Replace($"{value.Key}", value.Value);
            }

            return result;
        }

        public static string AvailableRoomsListSubscription = _roomsListTopic;
        public static string RoomMessageSubscription = ReplacePlaceholdersWithWildcard(_roomMessageTopic);
        public static string RoomUsersSignedSubscription = ReplacePlaceholdersWithWildcard(_roomUsersSignedTopic);
        public static string RoomUsersOnlineSubscription = ReplacePlaceholdersWithWildcard(_roomUsersOnlineTopic);
        public static string RoomUsersOfflineSubscription = ReplacePlaceholdersWithWildcard(_usersOfflineTopic);

        public static bool IsChatRoomsListTopic(string topic) => topic == _roomsListTopic;

        public static bool IsChatRoomMessagesTopic(string topic, out string chat, out string user) => 
            IsDefinedTopic(_roomMessageTopic, topic, out chat, out user);

        public static bool IsSignedUsersTopic(string topic, out string chat, out string user) =>
            IsDefinedTopic(_roomUsersSignedTopic, topic, out chat, out user);

        public static bool IsOnlineUsersTopic(string topic, out string chat, out string user) =>
            IsDefinedTopic(_roomUsersOnlineTopic, topic, out chat, out user);

        public static bool IsOfflineUsersTopic(string topic, out string user) =>
            IsDefinedTopic(_usersOfflineTopic, topic, out user, out string chat);


        private static bool IsDefinedTopic(string topicPattern, string inputTopic, out string chat, out string user)
        {
            var pattern = Regex.Replace(topicPattern, placeholderPattern, "([^/]+)") + "$";
            var regex = new Regex(pattern);
            if (regex.IsMatch(inputTopic))
            {
                var match = regex.Match(inputTopic);
                chat = match.Groups[1].Value;
                user = match.Groups[2].Value;
                return true;
            }

            chat = string.Empty;
            user = string.Empty;
            return false;
        }

        public static string GetChatRoomMessagesTopic(string chat, string user) =>
            ConstructTopic(_roomMessageTopic, chat, user);

        public static string GetSignedUsersTopic(string chat, string user) =>
            ConstructTopic(_roomUsersSignedTopic, chat, user);

        public static string GetOfflineUsersTopic( string user) =>
            ConstructTopic(_usersOfflineTopic, "", user);

        public static string GetOnlineUsersTopic(string chat, string user) =>
            ConstructTopic(_roomUsersOnlineTopic, chat, user);

        private static string ConstructTopic(string patternTopic, string chat, string user) => 
            patternTopic
                .Replace(ChatRoomNamePlaceholder, chat)
                .Replace(UserPlaceholder,user);
    }
}