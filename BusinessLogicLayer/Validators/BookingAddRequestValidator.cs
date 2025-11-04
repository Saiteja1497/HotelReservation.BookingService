using DataAccessLayer.DTO;
using FluentValidation;

namespace BusinessLogicLayer.Validators
{
    public class BookingAddRequestValidator:AbstractValidator<BookingAddRequest>
    {
        public BookingAddRequestValidator()
        {
            RuleFor(x => x.HotelId).NotEmpty().WithMessage("HotelId is required.");
            RuleFor(x => x.CheckInDate).LessThan(x => x.CheckOutDate).WithMessage("Check-in date must be before check-out date.")
                .NotEmpty().WithMessage("Check-in date is required.");
            RuleFor(x => x.CheckOutDate).GreaterThan(x => x.CheckInDate).WithMessage("Check-out date must be after check-in date.")
                .NotEmpty().WithMessage("Check-out date is required.");
            RuleFor(x => x.Rooms).NotEmpty().WithMessage("At least one room must be booked.")
                .Must(rooms => rooms != null && rooms.Count > 0).WithMessage("Rooms list cannot be empty.");

        }
    }
}
