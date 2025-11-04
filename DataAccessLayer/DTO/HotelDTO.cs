namespace DataAccessLayer.DTO
{
    public class HotelDTO
    {
        public Guid HotelID { get; init; }
        public string HotelName { get; init; } = string.Empty;
        public string HotelLocation { get; init; } = string.Empty;
        public string HotelDescription { get; init; } = string.Empty;
        public List<RoomResponseDTO>? Rooms { get; init; } = new();
    }
}
