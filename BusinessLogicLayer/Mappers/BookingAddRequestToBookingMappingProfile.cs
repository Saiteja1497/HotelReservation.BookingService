using AutoMapper;
using DataAccessLayer.DTO;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Mappers
{
    public class BookingAddRequestToBookingMappingProfile:Profile
    {
        public BookingAddRequestToBookingMappingProfile()
        {
            CreateMap<BookingAddRequest, Booking>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.HotelId, opt => opt.MapFrom(src => src.HotelId))
                .ForMember(dest => dest.Rooms, opt => opt.MapFrom(src => src.Rooms))
                .ForMember(dest => dest.CheckInDate, opt => opt.MapFrom(src => src.CheckInDate))
                .ForMember(dest => dest.CheckOutDate, opt => opt.MapFrom(src => src.CheckOutDate))
                .ForMember(dest => dest.TotalBill, opt => opt.Ignore())
                .ForMember(dest => dest.BookingId, opt => opt.Ignore())
                .ForMember(dest => dest._Id, opt => opt.Ignore());
        }
    }
}
