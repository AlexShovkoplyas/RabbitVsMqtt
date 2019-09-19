using System;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace MqttNetBug
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serverLogger = new MqttLoggerFactory().Create("Server");
            var clientLogger = new MqttLoggerFactory().Create("Client");

            IMqttFactory mqttFactory = new MqttFactory();

            //STARTING SERVER
            var mqttServer = mqttFactory.CreateMqttServer(serverLogger);
            await mqttServer.StartAsync(new MqttServerOptionsBuilder().WithDefaultEndpointPort(2001).Build());

            //STARTING CLIENT
            var mqttClient = mqttFactory.CreateMqttClient();

            var mqttClient2 = mqttFactory.CreateMqttClient(clientLogger);

            var options = new MqttClientOptionsBuilder()
                .WithClientId("Client11")
                .WithTcpServer("localhost", 2001)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(600))
                .WithCleanSession(false).Build();
            await mqttClient.ConnectAsync(options);

            var options2 = new MqttClientOptionsBuilder()
                .WithClientId("Client22")
                .WithTcpServer("localhost", 2001)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(600))
                .WithCleanSession(false).Build();
            await mqttClient2.ConnectAsync(options2);

            await mqttClient.SubscribeAsync("testtopic/1", MqttQualityOfServiceLevel.AtLeastOnce);
            await mqttClient.SubscribeAsync("testtopic/2", MqttQualityOfServiceLevel.AtLeastOnce);

            mqttClient.UseApplicationMessageReceivedHandler(async e =>
            {
                if (e.ApplicationMessage.Topic =="testtopic/1")
                {
                    Console.WriteLine("Message was consumed from [testtopic/1]");
                    var messageMqtt2 = new MqttApplicationMessageBuilder()
                        .WithTopic("testtopic/2")
                        .WithPayload("Hello2")
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .WithRetainFlag(false)
                        .Build();

                    _ = Task.Run(async () =>
                    {
                        await mqttClient.SubscribeAsync("testtopic/3",
                                MqttQualityOfServiceLevel.AtLeastOnce);
                        Console.WriteLine("Client subscribed to [testtopic/3]");
                    });
                    

                    //await mqttClient.PublishAsync(messageMqtt2);
                    //Console.WriteLine("Message was published to [testtopic/2]");

                    //await Task.Run(async () => await mqttClient.PublishAsync(messageMqtt2));
                    //await Task.Delay(2000);
                }
            });

            var messageMqtt1 = new MqttApplicationMessageBuilder()
                .WithTopic("testtopic/1")
                .WithPayload("Hello1")
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();
            Console.WriteLine("Message was published to [testtopic/1]");

            await mqttClient.PublishAsync(messageMqtt1);

            Console.WriteLine("Finish!");
            Console.ReadKey();
        }
    }
}
