namespace DataAccessLayer.DTO
{
    public record RoomBookingAddRequest(Guid RoomId, string RoomType, decimal RoomPrice, int NoOfRoomsBooked)
    {
        public RoomBookingAddRequest() : this(default,default, default, default)
        {
        }
    }
}
