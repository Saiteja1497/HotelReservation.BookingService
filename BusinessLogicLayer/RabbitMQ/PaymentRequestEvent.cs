namespace BusinessLogicLayer.RabbitMQ
{
    public class PaymentRequestEvent
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string PaymentMethod { get; set; } = "Internal";
    }
}
