using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;

namespace AuthBackend.Services
{
    public class RabbitMQProducer : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQProducer> _logger;

        public RabbitMQProducer(ILogger<RabbitMQProducer> logger)
        {
            _logger = logger;
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "email_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public void SendEmail(string email, string code)
        {
            var message = new { Email = email, Code = code };
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            _channel.BasicPublish(exchange: "", routingKey: "email_queue", basicProperties: null, body: body);
            _logger.LogInformation($"[{DateTime.Now}] Код {code} отправлен на {email}");
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}