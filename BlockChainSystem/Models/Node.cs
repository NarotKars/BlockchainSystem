using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BlockChainSystem.Models
{
    public class Node
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public bool IsFullNode { get; set; }
        public Wallet Wallet { get; set; }
    }
}
