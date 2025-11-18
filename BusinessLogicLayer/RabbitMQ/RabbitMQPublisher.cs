using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQPublisher : IRabbitMQPublisher,IDisposable
    {
        
        private readonly IConfiguration _configuration;
        private IChannel _channel;
        private IConnection _connection;
        public RabbitMQPublisher(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task Initialize()
        {
            Console.WriteLine($"RabbitMQ_HostName: {_configuration["RABBITMQ_HOSTNAME"]}");
            Console.WriteLine($"RabbitMQ_UserName: {_configuration["RABBITMQ_USERNAME"]}");
            Console.WriteLine($"RabbitMQ_Password: {_configuration["RABBITMQ_PASSWORD"]}");
            Console.WriteLine($"RabbitMQ_Port: {_configuration["RABBITMQ_PORT"]}");


            string hostName = _configuration.GetValue<string>("RABBITMQ_HOSTNAME")!;
            string userName = _configuration.GetValue<string>("RABBITMQ_USERNAME")!;
            string password = _configuration.GetValue<string>("RABBITMQ_PASSWORD")!;
            string port = _configuration.GetValue<string>("RABBITMQ_PORT")!;

            ConnectionFactory connectionFactory = new ConnectionFactory()
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                Port = Convert.ToInt32(port)
            };

            _connection = await connectionFactory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
        }


        public async Task Publish<T>(PaymentRequestEvent evt)
        {
            string messageJson = JsonSerializer.Serialize(evt);
            byte[] messageBodyInBytes = Encoding.UTF8.GetBytes(messageJson);
            string exchangeName = _configuration["RabbitMQ_Payments_Exchange"]!;
            await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Direct, durable: true);
            BasicProperties basicProperties = new BasicProperties();
            //basicProperties.Headers= headers;
            await _channel.BasicPublishAsync(exchange: exchangeName,
                                  routingKey: "payment.requested", true,
                                  basicProperties: basicProperties,
                                  body: messageBodyInBytes);
        }

        public void Dispose()
        {
            if (_channel != null && _channel.IsClosed == false)
            {
                _channel.Dispose();
            }
            if (_connection != null && _connection.IsOpen)
            {
                _connection.Dispose();
            }
        }
    }
}
