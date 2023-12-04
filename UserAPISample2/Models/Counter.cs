using MongoDB.Bson.Serialization.Attributes;

namespace UserAPISample2.Models
{
    public class Counter
    {
        [BsonId]
        [BsonElement("_id")] 
        public string Id { get; set; } = "";

        [BsonElement("seq")] 
        public long Seq { get; set; } = default;
    }
}
