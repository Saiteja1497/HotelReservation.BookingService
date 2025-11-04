namespace BusinessLogicLayer.RabbitMQ
{
    public record HotelNameUpdateMessage(Guid HotelID, string? HotelNewName);
  
}
