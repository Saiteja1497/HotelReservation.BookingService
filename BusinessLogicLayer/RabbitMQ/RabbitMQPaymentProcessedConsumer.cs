using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Services;
using DataAccessLayer.DTO;
using DataAccessLayer.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQPaymentProcessedConsumer : IDisposable, IRabbitMQPaymentProcessedConsumer
    {

        private readonly IConfiguration _configuration;
        private IChannel _channel;
        private IConnection _connection;
        private readonly ILogger<RabbitMQPaymentProcessedConsumer> _logger;
        private readonly IDistributedCache _cache;
        private readonly IHotelBookingService _hotelBookigService;
        public RabbitMQPaymentProcessedConsumer(IConfiguration configuration, 
            ILogger<RabbitMQPaymentProcessedConsumer> logger,
            IDistributedCache cache,IHotelBookingService hotelBookingService)
        {
            _configuration = configuration;
            _logger = logger;
             _cache = cache;
            _hotelBookigService = hotelBookingService;
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
            string routingKey = "payment.processed";
            string queueName = "bookings.payment.processed.queue";
            string exchangeName = _configuration["RabbitMQ_Payments_Exchange"]!;
            await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Direct, durable: true);
            await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            await _channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: routingKey,arguments:null);
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
                        PaymentProcessedEvent? paymentProcessedEvent = JsonSerializer.Deserialize<PaymentProcessedEvent>(message);
                        if (paymentProcessedEvent != null)
                        {
                            await HandleBookingStatusUpdation(paymentProcessedEvent);
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



        private async Task HandleBookingStatusUpdation(PaymentProcessedEvent paymentProcessedEvent)
        {
            _logger.LogInformation($"Updating Payment Informationfor Booking ID: {paymentProcessedEvent.BookingId}");
            BookingResponse? existingBooking = _hotelBookigService.GetBookingByCondition(Builders<Booking>.Filter.Eq(b => b.BookingId, paymentProcessedEvent.BookingId)).Result;
            if(existingBooking != null)
            {
                BookingUpdateRequest existingBookingUpdateRequest = new BookingUpdateRequest
                {
                    BookingId = existingBooking.BookingId,
                    UserId = existingBooking.UserId,
                    HotelId = existingBooking.HotelId,
                    Rooms = existingBooking.Rooms.Select(r => new RoomBookingUpdateRequest
                    {
                        RoomId = r.RoomId,
                        RoomType = r.RoomType,
                        RoomPrice = r.RoomPrice,
                        NoOfRoomsBooked = r.NoOfRoomsBooked
                    }).ToList(),
                    CheckInDate = existingBooking.CheckInDate,
                    CheckOutDate = existingBooking.CheckOutDate,
                    Status = paymentProcessedEvent.PaymentStatus
                };

                BookingResponse? bookingResponse =  await _hotelBookigService.UpdateBooking(existingBookingUpdateRequest);
                if(bookingResponse != null)
                {
                    string cacheKey = $"Booking_{bookingResponse.BookingId}";
                    string bookingResponseJson = JsonSerializer.Serialize(bookingResponse);
                    await _cache.SetStringAsync(cacheKey, bookingResponseJson);
                    _logger.LogInformation($"Updated Payment Information for Booking ID: {paymentProcessedEvent.BookingId} to Status: {paymentProcessedEvent.PaymentStatus}");
                }
                else
                { 
                    _logger.LogInformation($"Failed to update Booking ID: {paymentProcessedEvent.BookingId}");
                }

            }
            else
            {
                _logger.LogInformation($"Booking ID: {paymentProcessedEvent.BookingId} not found.");
            }
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
