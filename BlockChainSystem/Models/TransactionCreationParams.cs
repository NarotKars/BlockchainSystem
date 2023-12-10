namespace BlockChainSystem.Models
{
    public class TransactionCreationParams
    {
        public string ReceiverAddress { get; set; }
        public string SenderAddress { get; set; }
        public decimal Amount { get; set; }
    }
}
