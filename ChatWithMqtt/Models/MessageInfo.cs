using System;

namespace ChatWithMqtt
{
    public class MessageInfo : Notification
    {
        public MessageInfo(string user, DateTime timeStamp, string message)
        {
            (UserName, TimeStamp, Message) = (user, timeStamp, message);
        }

        public string UserName { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Message { get; set; }
    }

    public class Notification
    {
    }
}