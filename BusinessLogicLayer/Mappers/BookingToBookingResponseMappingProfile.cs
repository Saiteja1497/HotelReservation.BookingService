using AutoMapper;
using DataAccessLayer.DTO;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Mappers
{
    public class BookingToBookingResponseMappingProfile:Profile
    {
        public BookingToBookingResponseMappingProfile()
        {
            CreateMap<Booking, BookingResponse>()
              .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
              .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.BookingId))
              .ForMember(dest => dest.HotelId, opt => opt.MapFrom(src => src.HotelId))
              .ForMember(dest => dest.Rooms, opt => opt.MapFrom(src => src.Rooms))
              .ForMember(dest => dest.CheckInDate, opt => opt.MapFrom(src => src.CheckInDate))
              .ForMember(dest => dest.CheckOutDate, opt => opt.MapFrom(src => src.CheckOutDate))
              .ForMember(dest => dest.TotalBill, opt => opt.MapFrom(src => src.TotalBill));
        }
    }
}
