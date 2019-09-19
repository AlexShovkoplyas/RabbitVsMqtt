using System.Collections.Generic;
using ChatWithMqtt;
using ChatWithMqtt.ChatClient;
using NUnit.Framework;

namespace Tests
{
    public class TopicsHelperTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ReplacePlaceholdersWithWildcard()
        {
            var roomMessageTopic = "chatRooms/{chatRoomName}/{user}";
            var expectedResult = "chatRooms/+/+";

            var result = TopicsHelper.ReplacePlaceholdersWithWildcard(roomMessageTopic);

            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ApplyPlaceholders()
        {
            var roomMessageTopic = "chatRooms/{chatRoomName}/{user}";
            var expectedResult = "chatRooms/chat1/user1";

            var result = TopicsHelper.ApplyPlaceholders(roomMessageTopic,
                new Dictionary<string, string> { { TopicsHelper.ChatRoomNamePlaceholder, "chat1" }, { TopicsHelper.UserPlaceholder, "user1" } });

            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [TestCase("chatRooms/Chat1/User1", true, "Chat1", "User1")]
        [TestCase("chatRooms/Chat1/users/Signed/User1", false, "", "")]
        [TestCase("chatRooms/List", false, "", "")]

        public void IsChatRoomMessagesTopic(string topic, bool expectedResult, string expectedChat, string expectedUser)
        {

            var result = TopicsHelper.IsChatRoomMessagesTopic(topic, out string chat, out string user);

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(chat, Is.EqualTo(expectedChat));
            Assert.That(user, Is.EqualTo(expectedUser));
        }

        [TestCase("chatRooms/users/Online/user1", true, "user1")]
        [TestCase("chatRooms/Chat1/User1", false, "")]
        [TestCase("chatRooms/Chat1/users/Signed/User1", false, "")]
        [TestCase("chatRooms/List", false, "")]

        public void IsOfflineUsersTopic(string topic, bool expectedResult, string expectedUser)
        {

            var result = TopicsHelper.IsOfflineUsersTopic(topic, out string user);

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(user, Is.EqualTo(expectedUser));
        }
    }
}