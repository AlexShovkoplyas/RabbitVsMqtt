using System;
using System.Threading.Tasks;

namespace MqttNetRpc
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var subscriberConfiguration = new ClientConfiguration()
            {
                ClientId = "SubscriberClient" + Guid.NewGuid(),
                Host = "localhost",
                Port = 1883,
                //Username = "test",
                //Password = "test"
            };

            var publisherConfiguration = new ClientConfiguration()
            {
                ClientId = "PublisherClient2" + Guid.NewGuid(),
                Host = "localhost",
                Port = 1883,
                //Username = "test",
                //Password = "test"
            };

            var bootstrapper = new Bootstrapper();

            await bootstrapper.InitializeSubscriber(subscriberConfiguration);
            await bootstrapper.InitializePublisher(publisherConfiguration);

        }
    }
}
