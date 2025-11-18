using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.DTO;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BookingService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly ILogger<BookingsController> _logger;
        private readonly IHotelBookingService _hotelBookingService;
        public BookingsController(ILogger<BookingsController> logger, IHotelBookingService hotelBookingService)
        {
            _logger = logger;
            _hotelBookingService = hotelBookingService;
        }

        [HttpGet]
        public async Task<IEnumerable<BookingResponse?>> GetAllBookings()
        {
            return await _hotelBookingService.GetAllBookings();
        }

        [HttpGet("search/bookingid/{bookingID}")]
        public async Task<BookingResponse?> GetBookingByBookingID(Guid bookingID)
        {
            BookingResponse? bookingResponse = await _hotelBookingService.GetBookingByBookingID(bookingID);
            return bookingResponse;
        }

        [HttpGet("search/roomid/{roomID}")]
        public async Task<IEnumerable<BookingResponse?>> GetBookingsByRoomID(Guid roomID)
        {
            FilterDefinition<Booking> filter = Builders<Booking>.Filter.ElemMatch(temp => temp.Rooms, Builders<RoomBooking>.Filter.Eq(r => r.RoomId, roomID));
            List<BookingResponse?> bookingResponses = await _hotelBookingService.GetBookingsByCondition(filter);
            return bookingResponses;
        }


        [HttpGet("search/bookingDate/{bookingDate}")]
        public async Task<IEnumerable<BookingResponse?>> GetBookingsByBookingDate(DateTime bookingDate)
        {
            FilterDefinition<Booking> filter = Builders<Booking>.Filter.Eq(temp => temp.CheckInDate.ToString("yyyy-MM-dd"), bookingDate.ToString("yyyy-MM-dd"));
            List<BookingResponse?> bookingResponses = await _hotelBookingService.GetBookingsByCondition(filter);
            return bookingResponses;
        }


        [HttpGet("search/userID/{userID}")]
        public async Task<IEnumerable<BookingResponse?>> GetBookingsByUserID(Guid userID)
        {
            // FilterDefinition<Booking> filter = Builders<Booking>.Filter.Eq(temp => temp.UserId,userID);
            // List<BookingResponse?> bookingResponses = await _hotelBookingService.GetBookingsByCondition(filter);
            List<BookingResponse?> bookingResponses = await _hotelBookingService.GetBookingsByUserID(userID);

            return bookingResponses;
        }






        [HttpPost]
        public async Task<IActionResult?> Post(BookingAddRequest bookingAddRequest)
        {
            if (bookingAddRequest == null)
            {
                _logger.LogError("Booking add request is null.");
                return BadRequest("Invalid Booking Data");
            }
            BookingResponse? addedBooking = await _hotelBookingService.AddBooking(bookingAddRequest);
            if (addedBooking == null)
            {
                _logger.LogError("Failed to add booking.");
                return Problem("Failed to add booking.");
            }
            return Created($"api/Bookings/search/bookingid/{addedBooking?.BookingId}", addedBooking);
        }




        [HttpPut("{bookingID}")]
        public async Task<IActionResult?> Put( Guid bookingID, BookingUpdateRequest bookingUpdateRequest)
        {
            if (bookingUpdateRequest == null)
            {
                _logger.LogError("Booking update request is null.");
                return BadRequest("Invalid Booking Data");
            }
            if (bookingID != bookingUpdateRequest.BookingId)
            {
                _logger.LogError("Booking ID mismatch.");
                return BadRequest("Booking ID mismatch.");
            }

            BookingResponse? updatedBooking = await _hotelBookingService.UpdateBooking(bookingUpdateRequest);
            if (updatedBooking == null)
            {
                _logger.LogError("Failed to add booking.");
                return Problem("Failed to add booking.");
            }
            return Ok(updatedBooking);
        }



        [HttpDelete("{bookingID}")]
        public async Task<IActionResult?> Delete(Guid bookingID)
        {
            if (bookingID == Guid.Empty)
            {
                _logger.LogError("Invalid Booking ID.");
                return BadRequest("Invalid Booking ID");
            }
            bool isDeleted = await _hotelBookingService.DeleteBooking(bookingID);
            if (!isDeleted)
            {
                _logger.LogError("Failed to delete booking.");
                return Problem("Failed to delete booking.");
            }
            return Ok();
        }




    }
}
