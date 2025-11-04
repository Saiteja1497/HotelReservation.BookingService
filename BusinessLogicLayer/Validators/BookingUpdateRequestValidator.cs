using DataAccessLayer.DTO;
using FluentValidation;

namespace BusinessLogicLayer.Validators
{
    public class BookingUpdateRequestValidator:AbstractValidator<BookingUpdateRequest>
    {
        public BookingUpdateRequestValidator()
        {
            RuleFor(x => x.BookingId).NotEmpty().WithMessage("BookingId is required.");
            RuleFor(x => x.HotelId).NotEmpty().WithMessage("HotelId is required.");
            RuleFor(x => x.CheckInDate)
                .NotEmpty().WithMessage("Check-in date is required.")
                .LessThan(x => x.CheckOutDate).WithMessage("Check-in date must be before check-out date.");
            RuleFor(x => x.CheckOutDate)
                .NotEmpty().WithMessage("Check-out date is required.")
                .GreaterThan(x => x.CheckInDate).WithMessage("Check-out date must be after check-in date.");
            RuleFor(x => x.Rooms)
                .NotEmpty().WithMessage("At least one room booking is required.")
                .Must(rooms => rooms != null && rooms.Count > 0).WithMessage("Rooms list cannot be empty.");
        }
    }
}
