using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqFun
{
    public class Consumer : IBasicConsumer
    {
        public Consumer(IConnection connection)
        {
            Model = connection.CreateModel();
        }

        public void HandleBasicCancel(string consumerTag)
        {
            Console.WriteLine($"Cancel consumer : {consumerTag}");
        }

        public void HandleBasicCancelOk(string consumerTag)
        {
            Console.WriteLine($"CancelOk consumer : {consumerTag}");
        }

        public void HandleBasicConsumeOk(string consumerTag)
        {
            Console.WriteLine($"ConsumeOk consumer : {consumerTag}");
        }

        public void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey,
            IBasicProperties properties, byte[] body)
        {
            Console.WriteLine($"Message {System.Text.Encoding.UTF8.GetString(body)}");
            Model.BasicAck(deliveryTag, false);
            //Model.BasicNack(deliveryTag, false, false);
        }

        public void HandleModelShutdown(object model, ShutdownEventArgs reason)
        {
            Console.WriteLine($"Model shutdown");
            Model.Close();
        }

        public IModel Model { get; }
        public event EventHandler<ConsumerEventArgs> ConsumerCancelled;
    }
}