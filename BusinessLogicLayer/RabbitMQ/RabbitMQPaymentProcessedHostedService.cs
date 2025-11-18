using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQPaymentProcessedHostedService : IHostedService
    {
        //private readonly IRabbitMQPaymentProcessedConsumer _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RabbitMQPaymentProcessedHostedService> _logger;
        private IServiceScope _scope;
        private IRabbitMQPaymentProcessedConsumer _consumer;

        public RabbitMQPaymentProcessedHostedService(IServiceScopeFactory scopeFactory, ILogger<RabbitMQPaymentProcessedHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        //public RabbitMQPaymentProcessedHostedService(IRabbitMQPaymentProcessedConsumer consumer)
        //{
        //    _consumer = consumer;
        //}
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting RabbitMQ Payment Processed Consumer...");
            _scope = _scopeFactory.CreateScope();
            var _consumer = _scope.ServiceProvider.GetRequiredService<IRabbitMQPaymentProcessedConsumer>();

            
            await _consumer.Initialize(3000);
            await _consumer.Consume();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping RabbitMQ Payment Processed Consumer...");
            _consumer?.Dispose();
            _scope?.Dispose();
            return Task.CompletedTask;
        }
    }
}
