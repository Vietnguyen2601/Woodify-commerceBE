using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shared.Messaging;

/// <summary>
/// RabbitMQ Message Consumer - Nhận message từ RabbitMQ
/// </summary>
public class RabbitMQConsumer : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQConsumer(RabbitMQSettings settings)
    {
        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            UserName = settings.Username,
            Password = settings.Password,
            VirtualHost = settings.VirtualHost
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    /// <summary>
    /// Subscribe để nhận message từ queue
    /// </summary>
    public void Subscribe<T>(string queueName, Action<T> handler)
    {
        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<T>(json);
            
            if (message != null)
            {
                handler(message);
            }

            _channel.BasicAck(ea.DeliveryTag, multiple: false);
        };

        _channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer
        );
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
