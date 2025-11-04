
namespace BusinessLogicLayer.RabbitMQ
{
    public interface IRabbitMQHotelNameUpdateConsumer
    {
        Task Consume();
        Task Initialize(int delayMilliseconds);
        void Dispose();
    }
}