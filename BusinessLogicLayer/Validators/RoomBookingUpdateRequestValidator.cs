using DataAccessLayer.DTO;
using FluentValidation;

namespace BusinessLogicLayer.Validators
{
    public class RoomBookingUpdateRequestValidator:AbstractValidator<RoomBookingUpdateRequest>
    {
        public RoomBookingUpdateRequestValidator()
        {
            RuleFor(x => x.RoomId).NotEmpty().WithMessage("RoomId is required.");
            RuleFor(x => x.RoomType).NotEmpty().WithMessage("RoomType is required.");
            RuleFor(x => x.RoomPrice).GreaterThan(0).WithMessage("RoomPrice must be greater than zero.")
                .NotEmpty().WithMessage("Room price is required."); ;
            RuleFor(x => x.NoOfRoomsBooked).GreaterThanOrEqualTo(0).WithMessage("NoOfRoomsAvailable cannot be negative.");
        }
    }
}
