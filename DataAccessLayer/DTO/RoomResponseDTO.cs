namespace DataAccessLayer.DTO
{
    public class RoomResponseDTO
    {
        public Guid HotelID { get; init; }
        public Guid RoomID { get; init; }
        public string? RoomType { get; init; }
        public decimal RoomPrice { get; init; }
        public int NoOfRoomsAvailable { get; init; }
    }
}
