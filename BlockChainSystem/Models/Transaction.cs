using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BlockChainSystem.Models
{
    public class Transaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string ReceiverAddress { get; set; }
        public string SenderAddress { get; set; }
        public decimal Amount { get; set; }
        public byte[] Signiture { get; set; }
        public TransactionStatus Status { get; set; }
    }
}
