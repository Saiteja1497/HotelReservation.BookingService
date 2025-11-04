using AutoMapper;
using DataAccessLayer.DTO;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Mappers
{
    public class RoomBookingAddRequestToRoomBookingMappingProfile:Profile
    {
        public RoomBookingAddRequestToRoomBookingMappingProfile()
        {
            CreateMap<RoomBookingAddRequest, RoomBooking>()
                .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.RoomId))
                .ForMember(dest => dest.RoomPrice, opt => opt.MapFrom(src => src.RoomPrice))
                .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.RoomType))
                .ForMember(dest => dest.NoOfRoomsBooked, opt => opt.MapFrom(src => src.NoOfRoomsBooked))
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore())
                .ForMember(dest => dest._Id, opt => opt.Ignore());

        }
    }
}
