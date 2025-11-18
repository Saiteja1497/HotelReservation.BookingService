using AutoMapper;
using Azure;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.RabbitMQ;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Validators;
using DataAccessLayer.DTO;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace BusinessLogicLayer.Services
{
    public class HotelBookingService : IHotelBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<HotelBookingService> _logger;
        private readonly IValidator<BookingAddRequest> _bookingAddRequestValidator;
        private readonly IValidator<BookingUpdateRequest> _bookingUpdateRequestValidator;
        private readonly IValidator<RoomBookingAddRequest> _roomBookingAddRequestValidator;
        private readonly IValidator<RoomBookingUpdateRequest> _roomBookingUpdateRequestValidator;
        private readonly IMapper _mapper;
        private readonly UsersMicroserviceClient _usersMicroserviceClient;
        private readonly HotelsMicroserviceClient _hotelsMicroserviceClient;
        private readonly IRabbitMQPublisher _rabbitPublisher;
        public HotelBookingService(IBookingRepository bookingRepository,ILogger<HotelBookingService> logger,
            IValidator<BookingAddRequest> bookingAddRequestValidator, IValidator<BookingUpdateRequest> bookingUpdateRequestValidator,
            IValidator<RoomBookingAddRequest> roomBookingAddRequestValidator, IValidator<RoomBookingUpdateRequest> roomBookingUpdateRequestValidator,
            IMapper mapper, UsersMicroserviceClient usersMicroserviceClient, HotelsMicroserviceClient hotelsMicroserviceClient, IRabbitMQPublisher rabbitPublisher)
        {
            _bookingRepository = bookingRepository;
            _logger = logger;
            _bookingAddRequestValidator = bookingAddRequestValidator;
            _bookingUpdateRequestValidator = bookingUpdateRequestValidator;
            _roomBookingAddRequestValidator = roomBookingAddRequestValidator;
            _roomBookingUpdateRequestValidator = roomBookingUpdateRequestValidator;
            _mapper = mapper;
            _usersMicroserviceClient = usersMicroserviceClient;
            _hotelsMicroserviceClient = hotelsMicroserviceClient;
            _rabbitPublisher = rabbitPublisher;
        }
        public async Task<BookingResponse?> AddBooking(BookingAddRequest bookingAddRequest)
        {
            if(bookingAddRequest == null)
            {
                throw new ArgumentNullException(nameof(bookingAddRequest));
            }
            ValidationResult result = await _bookingAddRequestValidator.ValidateAsync(bookingAddRequest);
            if (!result.IsValid)
            {
                _logger.LogError("Invalid booking add request.");
                string errors = string.Join(", ", result.Errors.Select(error=>error.ErrorMessage));
                throw new ArgumentException(errors);
            }
            HotelDTO? hotel = await _hotelsMicroserviceClient.GetHotel(bookingAddRequest.HotelId);

            if (hotel == null)
            {
                throw new ArgumentException("Hotel Does not Exists");
            }
            foreach (RoomBookingAddRequest roomBookingAddRequest in bookingAddRequest.Rooms)
            {
                ValidationResult roomsResult = await _roomBookingAddRequestValidator.ValidateAsync(roomBookingAddRequest);

                if (!roomsResult.IsValid)
                {
                    string roomErrors = string.Join(", ", roomsResult.Errors.Select(error => error.ErrorMessage));
                    throw new ArgumentException(roomErrors);
                }

                //Add logic to verify if the room exists in the hotel and is available for booking.

                var roomExists = hotel.Rooms.Any(r => r.RoomID == roomBookingAddRequest.RoomId);
                if (!roomExists)
                {
                    throw new ArgumentException($"Room with ID {roomBookingAddRequest.RoomId} does not exist in the hotel.");
                }

            }


            //User Id validation can be added here to check if the user exists in the system.
            UserDTO? user = await _usersMicroserviceClient.GetUser(bookingAddRequest.UserId);
            if (user == null)
            {
                throw new ArgumentException("User Does not Exists");
            }

            Booking bookingEntity = _mapper.Map<Booking>(bookingAddRequest);
            bookingEntity.HotelLocation = hotel.HotelLocation;
            bookingEntity.HotelName = hotel.HotelName;
            bookingEntity.HotelDescription = hotel.HotelDescription;
            foreach (RoomBooking roomBooking in bookingEntity.Rooms)
            {
                roomBooking.TotalPrice = roomBooking.RoomPrice * roomBooking.NoOfRoomsBooked;
            }
            bookingEntity.TotalBill = bookingEntity.Rooms.Sum(temp => temp.TotalPrice);
            bookingEntity.Status = "Pending";
            Booking? addedBooking = await _bookingRepository.AddBooking(bookingEntity);
            if(addedBooking == null)
            {
                _logger.LogError("Failed to add booking.");
                return null;
            }


            BookingResponse bookingResponse = _mapper.Map<BookingResponse>(addedBooking);
            if(bookingResponse != null)
            {
                if (user != null)
                {
                    _mapper.Map(user, bookingResponse);
                }
                try
                {
                    var paymentEvent = new PaymentRequestEvent
                    {
                        BookingId = bookingResponse.BookingId,
                        UserId = bookingResponse.UserId,
                        Amount = bookingResponse.TotalBill,
                        Currency = "USD",
                        PaymentMethod = "Internal"
                    };
                    // IRabbitPublisher injected into this service (add to ctor)
                    await _rabbitPublisher.Initialize();
                    await _rabbitPublisher.Publish<PaymentRequestEvent>(paymentEvent);
                    _logger.LogInformation("Published payment.requested for Booking {BookingId}", bookingResponse.BookingId);
                    // set booking Status to "Pending" immediately if you want:
                    // bookingResponse.Status = "Pending";
                    // optionally update DB status via repository.UpdateBooking
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish payment.requested for Booking {BookingId}", bookingResponse.BookingId);
                }

            }
            return bookingResponse;
        }

        public async Task<bool> DeleteBooking(Guid bookingId)
        {
            if(bookingId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(bookingId));
            }
           Booking? existingBooking = _bookingRepository.GetBookingByCondition(Builders<Booking>.Filter.Eq(b => b.BookingId, bookingId)).Result;
           //Booking? existingBooking = await _bookingRepository.GetBookingByCondition(bookingId);
            if (existingBooking == null)
            {
                _logger.LogWarning("Booking with ID {BookingID} not found for deletion.", bookingId);
                return false;
            }
            return await _bookingRepository.DeleteBooking(bookingId);
        }

        public async Task<List<BookingResponse?>> GetAllBookings()
        {
            IEnumerable<Booking?> bookings = await _bookingRepository.GetAllBookings();
            if (bookings == null)
            {
                _logger.LogWarning("No booking found matching the given condition.");
                return null;
            }
            IEnumerable<BookingResponse?> bookingResponses = _mapper.Map<IEnumerable<BookingResponse>>(bookings);
            foreach (BookingResponse? response in bookingResponses)
            {
                if (response == null)
                {
                    continue;
                }
                UserDTO? user = await _usersMicroserviceClient.GetUser(response.UserId);
                if (user != null)
                {
                    //_mapper.Map<UserDTO, BookingResponse>(user);
                    _mapper.Map(user, response);
                }
            }
            return bookingResponses.ToList();

        }

        public async Task<BookingResponse?> GetBookingByCondition(FilterDefinition<Booking> filter)
        {
            Booking? booking = await _bookingRepository.GetBookingByCondition(filter);
            if (booking == null)
            {
                _logger.LogWarning("No booking found matching the given condition.");
                return null;
            }
            BookingResponse bookingResponse = _mapper.Map<BookingResponse>(booking);
            if (bookingResponse != null)
            {

                UserDTO? user = await _usersMicroserviceClient.GetUser(bookingResponse.UserId);
                if (user != null)
                {
                    _mapper.Map(user, bookingResponse);
                }
            }
            return bookingResponse;
        }

        public async Task<BookingResponse?> GetBookingByBookingID(Guid bookingID)
        {
            Booking? booking = await _bookingRepository.GetBookingByBookingID(bookingID);
            if (booking == null)
            {
                _logger.LogWarning("No booking found matching the given condition.");
                return null;
            }
            BookingResponse bookingResponse = _mapper.Map<BookingResponse>(booking);
            if (bookingResponse != null)
            {

                UserDTO? user = await _usersMicroserviceClient.GetUser(bookingResponse.UserId);
                if (user != null)
                {
                    _mapper.Map(user, bookingResponse);
                }
            }
            return bookingResponse;
        }


        public async Task<List<BookingResponse?>> GetBookingsByCondition(FilterDefinition<Booking> filter)
        {
            IEnumerable<Booking?> bookings = await _bookingRepository.GetBookingsByCondition(filter);
            if (bookings == null)
            {
                _logger.LogWarning("No booking found matching the given condition.");
                return null;
            }
            IEnumerable<BookingResponse?> bookingResponses = _mapper.Map<IEnumerable<BookingResponse>>(bookings);
            foreach (BookingResponse? response in bookingResponses)
            {
                if (response == null)
                {
                    continue;
                }

                UserDTO? user = await _usersMicroserviceClient.GetUser(response.UserId);
                if (user != null)
                {
                    _mapper.Map(user, response);

                }
            }
            return bookingResponses.ToList();
        }



        public async Task<List<BookingResponse?>> GetBookingsByUserID(Guid userID)
        {
            IEnumerable<Booking?> bookings = await _bookingRepository.GetBookingsByUserID(userID);
            if (bookings == null)
            {
                _logger.LogWarning("No booking found matching the given condition.");
                return null;
            }
            IEnumerable<BookingResponse?> bookingResponses = _mapper.Map<IEnumerable<BookingResponse>>(bookings);
            foreach (BookingResponse? response in bookingResponses)
            {
                if (response == null)
                {
                    continue;
                }

                UserDTO? user = await _usersMicroserviceClient.GetUser(response.UserId);
                if (user != null)
                {
                    _mapper.Map(user, response);

                }
            }
            return bookingResponses.ToList();
        }

        public async Task<BookingResponse?> UpdateBooking(BookingUpdateRequest bookingUpdateRequest)
        {
            if (bookingUpdateRequest == null)
            {
                throw new ArgumentNullException(nameof(bookingUpdateRequest));
            }
            ValidationResult result = await _bookingUpdateRequestValidator.ValidateAsync(bookingUpdateRequest);
            if (!result.IsValid)
            {
                _logger.LogError("Invalid booking update request.");
                string errors = string.Join(", ", result.Errors.Select(error => error.ErrorMessage));
                throw new ArgumentException(errors);
            }
            //hotels validation from hotel microserivce
            HotelDTO? hotel = await _hotelsMicroserviceClient.GetHotel(bookingUpdateRequest.HotelId);
            if (hotel == null)
            {
                throw new ArgumentException("Hotel Does not Exists");
            }
            foreach (RoomBookingUpdateRequest roomBookingUpdateRequest in bookingUpdateRequest.Rooms)
            {
                ValidationResult roomsResult = await _roomBookingUpdateRequestValidator.ValidateAsync(roomBookingUpdateRequest);

                if (!roomsResult.IsValid)
                {
                    string roomErrors = string.Join(", ", roomsResult.Errors.Select(error => error.ErrorMessage));
                    throw new ArgumentException(roomErrors);
                }
                var roomExists = hotel.Rooms.Any(r => r.RoomID == roomBookingUpdateRequest.RoomId);
                if (!roomExists)
                {
                    throw new ArgumentException($"Room with ID {roomBookingUpdateRequest.RoomId} does not exist in the hotel.");
                }
            }

            //User Id validation can be added here to check if the user exists in the system.

            UserDTO? user = await _usersMicroserviceClient.GetUser(bookingUpdateRequest.UserId);
            if (user == null)
            {
                throw new ArgumentException("User Does not Exists");
            }



            Booking bookingEntity = _mapper.Map<Booking>(bookingUpdateRequest);
            bookingEntity.HotelLocation = hotel.HotelLocation;
            bookingEntity.HotelName = hotel.HotelName;
            bookingEntity.HotelDescription = hotel.HotelDescription;
            foreach (RoomBooking roomBooking in bookingEntity.Rooms)
            {
                roomBooking.TotalPrice = roomBooking.RoomPrice * roomBooking.NoOfRoomsBooked;
            }
            bookingEntity.TotalBill = bookingEntity.Rooms.Sum(temp => temp.TotalPrice);
            bookingEntity.Status = bookingUpdateRequest.Status;
            Booking? updatedBooking = await _bookingRepository.UpdateBooking(bookingEntity);
            if (updatedBooking == null)
            {
                _logger.LogError("Failed to update booking.");
                return null;
            }
            BookingResponse bookingResponse = _mapper.Map<BookingResponse>(updatedBooking);
            if (bookingResponse != null)
            {
                if (user != null)
                {
                    _mapper.Map(user, bookingResponse);
                }

            }
            return bookingResponse;
        }
    }
}
