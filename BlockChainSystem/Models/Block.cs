using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BlockChainSystem.Models
{
    public class Block
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string PreviousHash { get; set; }
        public DateTime TimeStamp { get; set; }
        public Transaction Data { get; set; }
        public string Hash { get; set; }
        public int Nonce { get; set; }
    }
}
