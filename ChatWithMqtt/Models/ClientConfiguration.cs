namespace ChatWithMqtt
{
    public class ClientConfiguration
    {
        public string ClientId { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public int KeepAlivePeriod { get; set; } = 50000;

        public int CommunicationTimeout { get; set; } = 100000;
    }
}