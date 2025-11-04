using AutoMapper;
using DataAccessLayer.DTO;


namespace BusinessLogicLayer.Mappers
{
    public class UserDTOToBookingResponseMappingProfile:Profile
    {
        public UserDTOToBookingResponseMappingProfile()
        {
            CreateMap<UserDTO, BookingResponse>()
                 .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                 .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.Email));
        }
    }
}
