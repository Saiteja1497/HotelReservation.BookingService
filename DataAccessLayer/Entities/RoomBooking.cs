using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Entities
{
    public class RoomBooking
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public Guid _Id { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid RoomId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public string RoomType { get; set; } = string.Empty;
        [BsonRepresentation(BsonType.Double)]
        public decimal RoomPrice { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int NoOfRoomsBooked { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
