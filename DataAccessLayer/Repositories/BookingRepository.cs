using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Text.Json;

namespace DataAccessLayer.Repositories
{
    public class BookingRepository:IBookingRepository
    {
        private readonly ILogger<BookingRepository> _logger;
        private readonly IMongoCollection<Booking> _bookings;
        private readonly string _collectionName = "bookings";
        public BookingRepository(ILogger<BookingRepository> logger,IMongoDatabase mongoDatabase) 
        {
            _logger = logger;
            _bookings = mongoDatabase.GetCollection<Booking>(_collectionName);

        }

        public async Task<Booking?> AddBooking(Booking booking)
        {
            if(booking == null)
            {
                throw new ArgumentNullException(nameof(booking));
            }
            booking.BookingId = Guid.NewGuid();
            booking._Id = booking.BookingId;
            foreach(var room in booking.Rooms)
            {
                room._Id = Guid.NewGuid();
            }
            await _bookings.InsertOneAsync(booking);
            return booking;
        }

        public async Task<bool> DeleteBooking(Guid bookingID)
        {
            if(bookingID == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(bookingID));
            }
            FilterDefinition<Booking> filter = Builders<Booking>.Filter.Eq(b => b.BookingId, bookingID);
            Booking? existingBooking = (await _bookings.FindAsync(filter)).FirstOrDefault();
            if(existingBooking == null)
            {
                _logger.LogWarning("Booking with ID {BookingID} not found for deletion.", bookingID);
                return false;
            }
            DeleteResult deleteResult = await _bookings.DeleteOneAsync(filter);
            return deleteResult.DeletedCount > 0;
        }

        public async Task<IEnumerable<Booking?>> GetAllBookings()
        {
            IEnumerable<Booking> bookings = (await _bookings.FindAsync(Builders<Booking>.Filter.Empty)).ToList();
            _logger.LogInformation(JsonSerializer.Serialize(bookings, new JsonSerializerOptions { WriteIndented = true }));
            return bookings;
        }

        public async Task<Booking?> GetBookingByCondition(FilterDefinition<Booking> filter)
        {
            var response = (await _bookings.FindAsync(filter)).FirstOrDefault();
            if (response == null)
            { _logger.LogWarning("No booking found matching the provided condition."); }



            return response;
        }

        public async Task<Booking?> GetBookingByBookingID(Guid bookingID)
        {
            

            var response = (await _bookings.FindAsync(Builders<Booking>.Filter.Eq(b => b.BookingId, bookingID))).FirstOrDefault();
            if (response == null)
            {
                _logger.LogWarning("Booking with ID {BookingID} not found.", bookingID);
            }

            return response;
        }

        public async Task<IEnumerable<Booking>> GetBookingsByCondition(FilterDefinition<Booking> filter)
        {
            return (await _bookings.FindAsync(filter)).ToList();
        }

        public async Task<IEnumerable<Booking?>> GetBookingsByUserID(Guid userID)
        {
            
            var response = (await _bookings.FindAsync(Builders<Booking>.Filter.Eq(b => b.UserId, userID))).ToList();
            if (response == null || response.Count == 0)
            {
                _logger.LogWarning("No bookings found for User ID {UserID}.", userID);
            }
            return response;
        }

        public async Task<Booking?> UpdateBooking(Booking booking)
        {
            if (booking == null)
            {
                throw new ArgumentNullException(nameof(booking));
            }
            FilterDefinition<Booking> filter = Builders<Booking>.Filter.Eq(b => b.BookingId, booking.BookingId);
            Booking? existingBooking = (await _bookings.FindAsync(filter)).FirstOrDefault();
            if (existingBooking == null)
            {
                _logger.LogWarning("Booking with ID {BookingID} not found.", booking.BookingId);
                return null;
            }
            booking._Id = existingBooking._Id;
            ReplaceOneResult replaceOneResult= await _bookings.ReplaceOneAsync(filter, booking);
            return booking;
        }
    }
}
