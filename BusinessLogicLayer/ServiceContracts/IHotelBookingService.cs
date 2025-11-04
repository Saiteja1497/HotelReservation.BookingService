using DataAccessLayer.DTO;
using DataAccessLayer.Entities;
using MongoDB.Driver;

namespace BusinessLogicLayer.ServiceContracts
{
    public interface IHotelBookingService
    {
        Task<List<BookingResponse?>> GetAllBookings();
        Task<List<BookingResponse?>> GetBookingsByCondition(FilterDefinition<Booking> filter);
        Task<BookingResponse?> GetBookingByCondition(FilterDefinition<Booking> filter);
        Task<BookingResponse?> AddBooking(BookingAddRequest bookingAddRequest);
        Task<BookingResponse?> UpdateBooking(BookingUpdateRequest bookingUpdateRequest);
        Task<bool> DeleteBooking(Guid bookingId);
    }
}
