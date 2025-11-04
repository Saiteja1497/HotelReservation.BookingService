using AutoMapper;
using DataAccessLayer.DTO;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Mappers
{
    public class RoomBookingToRoomBookingResponseMappingProfile:Profile
    {
        public RoomBookingToRoomBookingResponseMappingProfile()
        {
            CreateMap<RoomBooking,RoomBookingResponse>()
                .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.RoomId))
                .ForMember(dest => dest.RoomPrice, opt => opt.MapFrom(src => src.RoomPrice))
                .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.RoomType))
                .ForMember(dest => dest.NoOfRoomsBooked, opt => opt.MapFrom(src => src.NoOfRoomsBooked))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice));
        }
    }
}
