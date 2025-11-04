using Microsoft.Extensions.Hosting;

namespace BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQHotelDeleteHostedService : IHostedService
    {
        private readonly IRabbitMQHotelDeleteConsumer _consumer;
        public RabbitMQHotelDeleteHostedService(IRabbitMQHotelDeleteConsumer consumer)
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
