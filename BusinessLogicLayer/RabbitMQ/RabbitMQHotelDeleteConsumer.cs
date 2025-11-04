using BusinessLogicLayer.Services;
using DataAccessLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using ZstdSharp.Unsafe;

namespace BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQHotelDeleteConsumer : IDisposable, IRabbitMQHotelDeleteConsumer
    {

        private readonly IConfiguration _configuration;
        private IChannel _channel;
        private IConnection _connection;
        private readonly ILogger<RabbitMQHotelDeleteConsumer> _logger;
        private readonly IDistributedCache _cache;
        public RabbitMQHotelDeleteConsumer(IConfiguration configuration, 
            ILogger<RabbitMQHotelDeleteConsumer> logger,
            IDistributedCache cache)
        {
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
        }

        public async Task Initialize(int delayMilliseconds = 3000)
        {
            bool connected = false;
            int attempt = 0;

            while (!connected)
            {
                try
                {
                    await SetupConnectionAndChannel();
                    connected = true;

                    _logger.LogInformation("RabbitMQ Connected.");
                }
                catch (Exception ex)
                {
                    attempt++;

                    _logger.LogError(ex, $"Attempt {attempt} failed to connect to RabbitMQ. Retrying in {delayMilliseconds} ms.");

                    Thread.Sleep(delayMilliseconds);
                }
            }

        }

        private async Task SetupConnectionAndChannel()
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


        public async Task Consume()
        {
            //string routingKey = "hotel.delete";
            var headers = new Dictionary<string, object>() {
                    {"x-match","all" },
                    {"event","hotel.delete" },
                    {"RowCount",1 }
                };
            string queueName = "bookings.hotel.delete.queue";
            string exchangeName = _configuration["RabbitMQ_Hotels_Exchange"]!;
            await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Headers, durable: true);
            await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            await _channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: string.Empty,arguments:headers);
            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, args) =>
            {
                byte[] body = args.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"Received message: {message}");
                try
                {
                    if (message != null)
                    {
                        HotelDeleteMessage? hotelDeleteMessage = JsonSerializer.Deserialize<HotelDeleteMessage>(message);
                        if (hotelDeleteMessage != null)
                        {
                            await HandleHotelDeletion(hotelDeleteMessage.HotelID);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Error processing message: {ex.Message}");
                }
            };

            await _channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

        }


        private async Task HandleHotelDeletion(Guid hotelID)
        {
            _logger.LogInformation($"Updating bookings for HotelID: {hotelID}");

            string cacheKeyToWrite = $"Hotel_{hotelID}";

            await _cache.RemoveAsync(cacheKeyToWrite);
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
