namespace DataAccessLayer.DTO
{
    public record PaymentResponse(Guid PaymentID,Guid BookingID, Guid UserID, decimal Amount, string Currency, string PaymentMethod, string PaymentStatus, string? TransactionRef, DateTime ProcessedAt)
    {
        public PaymentResponse():this(default, default, default, default, default, default, default, default,default)
        {
        }
    };
}
