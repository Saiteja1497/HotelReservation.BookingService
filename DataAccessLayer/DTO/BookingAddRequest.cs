namespace DataAccessLayer.DTO
{
    public record BookingAddRequest(Guid UserId,Guid HotelId, List<RoomBookingAddRequest> Rooms,
        DateTime CheckInDate, DateTime CheckOutDate)
    {
        public BookingAddRequest() : this(default, default, default, default, default)
        {

        }
       
    }
}
