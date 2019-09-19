using System;
using System.Collections.Generic;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqFun
{
    class Program
    {
        static void Main(string[] args)
        {
            var exchangeName = "chapter2-example";
            var queueName = "example";
            var routingKey = "routing_key";

            var alternateExchangeName = "alternate-example";
            var alternateQueueName = "alternate-example";

            ConnectionFactory factory = new ConnectionFactory();

            factory.Uri = new Uri("amqp://rabbitmq:rabbitmq@localhost:5672/");

            using (IConnection conn = factory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    //channel.ExchangeDeclare(alternateExchangeName, ExchangeType.Fanout);
                    //channel.QueueDeclare(alternateQueueName, false, false, false);

                    //var args = new Dictionary<string, object> {{"alternate-exchange", alternateExchangeName}};
                    channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);

                    channel.QueueDelete(queueName);

                    //var args2 = new Dictionary<string, object> {{"x-max-length", 1}};
                    channel.QueueDeclare(queueName, true, false, false);

                    channel.QueueBind(queueName, exchangeName, routingKey, null);

                    channel.QueueDeclare(queueName+"2", true, false, false);

                    channel.QueueBind(queueName + "2", exchangeName, routingKey, null);

                    channel.QueuePurge(queueName);

                    //channel.QueueUnbind(queueName, exchangeName, routingKey);

                    //channel.QueueBind(alternateQueueName, alternateExchangeName, routingKey, null);
                    //channel.QueuePurge(alternateQueueName);


                    channel.BasicReturn += (sender, eventArgs) =>
                    {
                        Console.WriteLine($"Basic.Return {eventArgs.ReplyText}");
                    };

                    

                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes("Hello, world!");
                    byte[] messageBodyBytes2 = System.Text.Encoding.UTF8.GetBytes("Hello, world 2!");


                    IBasicProperties props = channel.CreateBasicProperties();
                    //props.Persistent = true;
                    props.ContentType = "text/plain/xxx";
                    props.DeliveryMode = 2;
                    props.Headers = new Dictionary<string, object>();
                    props.Headers.Add("latitude", 51.5252949);
                    props.Headers.Add("longitude", -0.0905493);

                    //var consumer = new EventingBasicConsumer(channel);
                    //consumer.Received += (ch, ea) =>
                    //{
                    //    using (IModel channel = conn.CreateModel())
                    //    {
                    //        var body = ea.Body;
                    //        Console.WriteLine(System.Text.Encoding.UTF8.GetString(body));
                    //        channel.BasicAck(ea.DeliveryTag, false);
                    //    }
                    //};

                    channel.BasicQos(0, 1, false);

                    channel.BasicAcks += (sender, eventArgs) =>
                    {
                        Console.WriteLine($"Publish Acknowledgement : {eventArgs.DeliveryTag}");
                    };

                    channel.BasicNacks += (sender, eventArgs) =>
                    {
                        Console.WriteLine($"Publish Negative Acknowledgement : {eventArgs.DeliveryTag}");
                    };

                    channel.ConfirmSelect();

                    channel.BasicPublish(exchangeName, routingKey, true, props, messageBodyBytes);
                    channel.BasicPublish(exchangeName, routingKey, true, props, messageBodyBytes);

                    Console.WriteLine("Before");

                    channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));

                    Console.WriteLine("After");

                    //var consumer = new Consumer(conn);

                    //String consumerTag = consumer.Model.BasicConsume(queueName, false, consumer);

                    //consumer.Model.BasicCancel(consumerTag);

                    //channel.BasicQos(0, 2, true);

                    //channel.ConfirmSelect();
                    //channel.TxSelect();

                    //channel.BasicPublish(exchangeName, routingKey, true, props, messageBodyBytes2);
                    //channel.BasicPublish(exchangeName, routingKey, true, props, messageBodyBytes2);
                    //channel.BasicPublish(exchangeName, routingKey, true, props, messageBodyBytes2);
                    //channel.BasicPublish(exchangeName, routingKey, true, props, messageBodyBytes2);
                    //channel.TxCommit();

                    //channel.TxSelect();
                    //BasicGetResult result = null;
                    //for (int i = 0; i < 4; i++)
                    //{
                    //    result = channel.BasicGet(queueName, false);
                    //    if (result == null)
                    //    {
                    //        Console.WriteLine("No messages");
                    //    }
                    //    else
                    //    {
                    //        byte[] body = result.Body;
                    //        Console.WriteLine($"Message {System.Text.Encoding.UTF8.GetString(body)}");

                    //    }
                    //}
                    //Console.ReadLine();
                    //channel.BasicAck(2, true);

                    Console.ReadLine();
                }
            }

            Console.WriteLine("Hello World!");
        }
    }
}
