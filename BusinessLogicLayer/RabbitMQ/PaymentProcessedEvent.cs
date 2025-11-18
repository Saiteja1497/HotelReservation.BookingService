namespace BusinessLogicLayer.RabbitMQ
{
    public class PaymentProcessedEvent
    {
        public Guid BookingId { get; set; }
        public Guid PaymentId { get; set; }
        public string? PaymentStatus { get; set; }  // Success or Failed
        public string? TransactionRef { get; set; }
    }
}
