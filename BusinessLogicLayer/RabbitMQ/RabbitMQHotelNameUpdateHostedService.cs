using Microsoft.Extensions.Hosting;

namespace BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQHotelNameUpdateHostedService : IHostedService
    {
        private readonly IRabbitMQHotelNameUpdateConsumer _consumer;
        public RabbitMQHotelNameUpdateHostedService(IRabbitMQHotelNameUpdateConsumer consumer)
        {
            _consumer = consumer;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _consumer.Initialize(3000);
            await _consumer.Consume();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _consumer.Dispose();
            return Task.CompletedTask;
        }
    }
}
