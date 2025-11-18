namespace DataAccessLayer.DTO
{
    public record RoomBookingResponse( Guid RoomId, string RoomType, decimal RoomPrice, int NoOfRoomsBooked, decimal TotalPrice)
    {
        public RoomBookingResponse() : this(default,default, default, default, default)
        {
        }
    }
}
