using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace AuthBackend.Services
{
    public class RabbitMQBackgroundService : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQBackgroundService> _logger;
        private readonly EmailSender _emailSender;

        public RabbitMQBackgroundService(ILogger<RabbitMQBackgroundService> logger, EmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "email_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var emailMessage = JsonConvert.DeserializeObject<EmailMessage>(message);

                _logger.LogInformation($"[{DateTime.Now}] Получено сообщение для {emailMessage.Email} с кодом {emailMessage.Code}");

                // Отправка email
                _emailSender.SendEmail(emailMessage.Email, emailMessage.Code);
            };

            _channel.BasicConsume(queue: "email_queue", autoAck: true, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }

        private class EmailMessage
        {
            public string Email { get; set; }
            public string Code { get; set; }
        }
    }
}
