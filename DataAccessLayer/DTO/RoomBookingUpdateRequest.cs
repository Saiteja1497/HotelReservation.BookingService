namespace DataAccessLayer.DTO
{
    public record RoomBookingUpdateRequest(Guid RoomBookingId, Guid RoomId,string RoomType, decimal RoomPrice, int NoOfRoomsBooked)
    {
        public RoomBookingUpdateRequest() : this(default,default, default, default, default)
        {
        }
    }
}
