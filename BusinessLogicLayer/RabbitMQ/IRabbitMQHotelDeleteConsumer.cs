
namespace BusinessLogicLayer.RabbitMQ
{
    public interface IRabbitMQHotelDeleteConsumer
    {
        Task Consume();
        void Dispose();
        Task Initialize(int delayMilliseconds);
    }
}