namespace DataAccessLayer.DTO
{
    public record RoomBookingUpdateRequest( Guid RoomId,string RoomType, decimal RoomPrice, int NoOfRoomsBooked)
    {
        public RoomBookingUpdateRequest() : this(default,default, default, default)
        {
        }
    }
}
