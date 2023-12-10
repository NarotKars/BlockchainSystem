using BlockChainSystem.Models;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace BlockChainSystem.Services
{
    public class WalletService
    {
        private readonly TransactionService transactionService;
        private readonly MongoClient client;
        public WalletService(TransactionService transactionService, IConfiguration configuration)
        {
            this.transactionService = transactionService;
            this.client = ConnectionManager.GetMongoClient(configuration);
        }

        public (string PublicKey, string PrivateKey) GeneratePublicAndPrivateKeys()
        {
            using RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            return (rsa.ToXmlString(false), rsa.ToXmlString(true));
        }

        public string GenerateAddressFromPublicKey(string publicKey)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] publicKeyBytes = Encoding.UTF8.GetBytes(publicKey);
                byte[] hashBytes = sha256.ComputeHash(publicKeyBytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.AppendFormat("{0:x2}", b);
                }

                return builder.ToString();
            }
        }

        public async Task<decimal> GetBalance(string address)
        {
            var transactions = await this.transactionService.GetTransactions();
            var mongoDb = this.client.GetDatabase("BlockChain");
            var node = (await mongoDb.GetCollection<Node>("node").FindAsync(node => node.Wallet.Address == address)).ToEnumerable().FirstOrDefault();
            if (node == null)
            {
                throw new RESTException("Invalid node address", System.Net.HttpStatusCode.BadRequest);
            }
            var sent = transactions.Where(t => t.SenderAddress == address).Sum(x => x.Amount);
            transactions = await this.transactionService.GetTransactions();
            var received = transactions.Where(t => t.ReceiverAddress == address).Sum(x => x.Amount);
            return node.Wallet.InitialBalance - sent + received;
        }
    }
}
