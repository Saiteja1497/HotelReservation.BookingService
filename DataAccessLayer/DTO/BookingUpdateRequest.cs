namespace DataAccessLayer.DTO
{
    public record BookingUpdateRequest(Guid UserId, Guid BookingId,Guid HotelId, List<RoomBookingUpdateRequest> Rooms,
       DateTime CheckInDate, DateTime CheckOutDate)
    {
        public BookingUpdateRequest() : this(default, default, default, default, default, default)
        {

        }

    }
}
