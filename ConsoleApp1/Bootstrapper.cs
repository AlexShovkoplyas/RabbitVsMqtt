using System;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.Rpc;
using MQTTnet.Extensions.Rpc.Options;
using MQTTnet.Protocol;
using MqttNetRpc.Contracts;
using Newtonsoft.Json;

namespace MqttNetRpc
{
    public class Bootstrapper
    {
        public async Task InitializePublisher(ClientConfiguration configuration)
        {
            var options = new MqttClientOptionsBuilder()
                .WithClientId(configuration.ClientId)
                .WithCleanSession(false)
                .WithTcpServer(configuration.Host, configuration.Port)
                .WithCredentials(configuration.Username, configuration.Password)
                .WithKeepAlivePeriod(TimeSpan.FromMilliseconds(configuration.KeepAlivePeriod))
                .WithCommunicationTimeout(TimeSpan.FromMilliseconds(configuration.CommunicationTimeout)).Build();

            var mqttClient = new MqttFactory().CreateMqttClient();
            var connectResult = await mqttClient.ConnectAsync(options);

            Console.WriteLine($"Publisher connection result : {connectResult.ResultCode}");

            var rpcOptions = new MqttRpcClientOptions()
            {
                TopicGenerationStrategy = new DefaultMqttRpcClientTopicGenerationStrategy()
            };

            var rpcClient = new MqttRpcClient(mqttClient, rpcOptions);

            var command = new CommandDto
            {
                Id = 1,
                Message = "Say Hello world!"
            };

            var payload = JsonConvert.SerializeObject(command);

            Console.WriteLine($"Publisher requesting work \"DoWork\" with Id : 1");

            var responseMqtt = await rpcClient.ExecuteAsync(TimeSpan.FromSeconds(50), "DoWork", payload, MqttQualityOfServiceLevel.AtLeastOnce);

            var response = Encoding.Default.GetString(responseMqtt);

            Console.WriteLine($"Publisher got response : {response}");
        }

        public async Task InitializeSubscriber(ClientConfiguration configuration)
        {
            var options = new MqttClientOptionsBuilder()
                .WithClientId(configuration.ClientId)
                .WithCleanSession(false)
                .WithTcpServer(configuration.Host, configuration.Port)
                .WithCredentials(configuration.Username, configuration.Password)
                .WithKeepAlivePeriod(TimeSpan.FromMilliseconds(configuration.KeepAlivePeriod))
                .WithCommunicationTimeout(TimeSpan.FromMilliseconds(configuration.CommunicationTimeout)).Build();

            var mqttClient = new MqttFactory().CreateMqttClient();

            string responsePayload = string.Empty;

            mqttClient.UseApplicationMessageReceivedHandler(args =>
            {
                 var payloadMqtt = args.ApplicationMessage.Payload;
                var topic = args.ApplicationMessage.Topic;

                var payload = Encoding.Default.GetString(payloadMqtt);
                var message =  JsonConvert.DeserializeObject<CommandDto>(payload);

                Console.WriteLine($"Subscriber have received message from topic : {topic}");

                if (topic.StartsWith("MQTTnet.RPC/"))
                {
                    if (topic.EndsWith("/DoWork"))
                    {
                        Console.WriteLine($"Subscriber have done work with id : {message.Id}");
                        responsePayload = "Work done with success";
                    }

                    if (topic.EndsWith("/DoOtherWork"))
                    {
                        responsePayload = "Other Work done with success";
                    }

                    var messageMqtt = new MqttApplicationMessageBuilder()
                        .WithTopic(topic + "/response")
                        .WithPayload(responsePayload)
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .WithRetainFlag(false)
                        .Build();

                    Task.Run(async () => await mqttClient.PublishAsync(messageMqtt));
                }
            }); 

            var connectResult = await mqttClient.ConnectAsync(options);

            Console.WriteLine($"Subscriber connection result : {connectResult.ResultCode}");

            //await mqttClient.SubscribeAsync("#");
            await mqttClient.SubscribeAsync("MQTTnet.RPC/+/DoWork");
            await mqttClient.SubscribeAsync("MQTTnet.RPC/+/DoOtherWork");
        }
    }
}