using DataAccessLayer.DTO;
using FluentValidation;

namespace BusinessLogicLayer.Validators
{
    public class RoomBookingAddRequestValidator : AbstractValidator<RoomBookingAddRequest>
    {
        public RoomBookingAddRequestValidator()
        {
            RuleFor(x => x.RoomType)
                .NotEmpty().WithMessage("Room type is required.")
                .MaximumLength(100).WithMessage("Room type cannot exceed 100 characters.");
            RuleFor(x => x.RoomPrice)
                .GreaterThan(0).WithMessage("Room price must be greater than zero.")
                .NotEmpty().WithMessage("Room price is required.");
            //RuleFor(x => x.NoOfRoomsBooked)
            //    .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        }
    }
}