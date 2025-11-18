namespace DataAccessLayer.DTO
{
    public record BookingResponse(Guid UserId, Guid BookingId, Guid HotelId, 
        string HotelName, string HotelLocation, string HotelDescription,List<RoomBookingResponse> Rooms,
         DateTime CheckInDate, DateTime CheckOutDate, decimal TotalBill,string UserName, string UserEmail, string BookingStatus)
    {
        public BookingResponse() : this(default, default, default, default, default, default, default, default, default, default, default, default,default)
        {

        }
    }
}
