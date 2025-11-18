
namespace BusinessLogicLayer.RabbitMQ
{
    public interface IRabbitMQPaymentProcessedConsumer
    {
        Task Consume();
        Task Initialize(int delayMilliseconds);
        void Dispose();
    }
}