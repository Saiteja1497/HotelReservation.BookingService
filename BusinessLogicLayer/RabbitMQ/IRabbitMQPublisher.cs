namespace BusinessLogicLayer.RabbitMQ
{
    public interface IRabbitMQPublisher
    {
        // Task Publish<T>(Dictionary<string, object> headers, T message);
        Task Publish<T>(PaymentRequestEvent evt);
        Task Initialize();
    }
}
