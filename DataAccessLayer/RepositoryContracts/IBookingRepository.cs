using DataAccessLayer.Entities;
using MongoDB.Driver;

namespace DataAccessLayer.RepositoryContracts
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking?>> GetAllBookings();
        Task<IEnumerable<Booking>> GetBookingsByCondition(FilterDefinition<Booking> filter);
        Task<Booking?> GetBookingByCondition(FilterDefinition<Booking> filter);
        Task<IEnumerable<Booking?>> GetBookingsByUserID(Guid userID);
        Task<Booking?> GetBookingByBookingID(Guid bookingID);

        Task<Booking?> AddBooking(Booking booking);
        Task<Booking?> UpdateBooking(Booking booking);
        Task<bool> DeleteBooking(Guid bookingID);
    }
}
