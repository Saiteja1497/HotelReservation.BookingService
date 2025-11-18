using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Entities
{
    public class Booking
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public Guid _Id { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid BookingId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid HotelId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public string HotelName { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.String)]
        public string HotelLocation { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.String)]
        public string HotelDescription { get; set; } = string.Empty;
        public List<RoomBooking> Rooms { get; set; } = new();

        [BsonRepresentation(BsonType.String)]
        public DateTime CheckInDate { get; set; }
        [BsonRepresentation(BsonType.String)]
        public DateTime CheckOutDate { get; set; }
        [BsonRepresentation(BsonType.Double)]
        public decimal TotalBill{ get; set; }
        [BsonRepresentation(BsonType.String)]
        public string Status { get; set; }
        [BsonRepresentation(BsonType.String)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
