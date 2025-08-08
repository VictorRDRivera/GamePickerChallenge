using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GamePicker.Repository.Entities
{
    public class GameRecommendationEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MongoId { get; set; } = ObjectId.GenerateNewId().ToString();
        public int GameId { get; set; }
        public string? Title { get; set; }
        public string? Genre { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public int RecommendedTimes { get; set; } = 1;
    }

}
