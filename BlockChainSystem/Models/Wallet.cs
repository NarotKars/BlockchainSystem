namespace BlockChainSystem.Models
{
    public class Wallet
    {
        public string Address { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public decimal InitialBalance { get; set; }
    }
}
