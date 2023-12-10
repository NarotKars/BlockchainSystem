using BlockChainSystem.Models;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace BlockChainSystem.Services
{
    public class TransactionService
    {
        private readonly IConfiguration configuration;
        private readonly MongoClient client;
        public TransactionService(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.client = ConnectionManager.GetMongoClient(configuration);
        }

        public async Task<IEnumerable<Transaction>> GetTransactions()
        {
            var mongoDb = this.client.GetDatabase("BlockChain");
            return (await mongoDb.GetCollection<Transaction>("Transacions").FindAsync(_ => true)).ToEnumerable();
        }

        public async Task CreateTransaction(TransactionCreationParams parameters)
        {
            var transaction = new Transaction()
            {
                ReceiverAddress = parameters.ReceiverAddress,
                SenderAddress = parameters.SenderAddress,
                Status = TransactionStatus.ReadyForVerification,
                Amount = parameters.Amount
            };

            string transactionData = transaction.ReceiverAddress + transaction.SenderAddress + transaction.Amount;
            byte[] dataBytes = Encoding.UTF8.GetBytes(transactionData);

            var mongoDb = this.client.GetDatabase("BlockChain");
            var senderNode = (await mongoDb.GetCollection<Node>("node").FindAsync(node => node.Wallet.Address == parameters.SenderAddress)).ToList().FirstOrDefault();
            if (senderNode == null)
            {
                throw new RESTException("Incorrect sender address", System.Net.HttpStatusCode.BadRequest);
            }
            if (senderNode.Wallet.InitialBalance - parameters.Amount < 0)
            {
                throw new RESTException("Insufficient balance", System.Net.HttpStatusCode.BadRequest);
            }
            byte[] signature = SignData(dataBytes, senderNode.Wallet.PrivateKey);

            transaction.Signiture = signature;

            await mongoDb.GetCollection<Transaction>("Transacions").InsertOneAsync(transaction);
        }

        static byte[] SignData(byte[] data, string privateKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);
            return rsa.SignData(data, new SHA256CryptoServiceProvider());
        }

        public async Task VerifySignature(string transactionId)
        {
            var transaction = await GetTransaction(transactionId);
            string transactionData = transaction.ReceiverAddress + transaction.SenderAddress + transaction.Amount;
            byte[] dataBytes = Encoding.UTF8.GetBytes(transactionData);

            var mongoDb = this.client.GetDatabase("BlockChain");
            var node = (await mongoDb.GetCollection<Node>("node").FindAsync(node => node.Wallet.Address == transaction.SenderAddress)).ToList().FirstOrDefault();
            if (node == default)
            {
                throw new RESTException("Invalid node", System.Net.HttpStatusCode.BadRequest);
            }
            using RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(node.Wallet.PublicKey);
            var isValid = rsa.VerifyData(dataBytes, SHA256.Create(), transaction.Signiture);

            if (isValid)
            {
                await this.UpdateTransactionStatus(transactionId, TransactionStatus.ReadyForMining);
            }
            else
            {
                await this.UpdateTransactionStatus(transactionId, TransactionStatus.SignitureNotVerified);
            }
        }

        public async Task<Transaction> GetTransaction(string transactionId)
        {
            var mongoDb = this.client.GetDatabase("BlockChain");
            var transaction = (await mongoDb.GetCollection<Transaction>("Transacions").FindAsync(t => t.Id == transactionId)).ToList().FirstOrDefault();
            if (transaction == default)
            {
                throw new RESTException("Invalid transaction", System.Net.HttpStatusCode.BadRequest);
            }
            return transaction;
        }

        public async Task UpdateTransactionStatus(string transactionId, TransactionStatus status)
        {
            var filter = Builders<Transaction>.Filter.Eq(t => t.Id, transactionId);
            var update = Builders<Transaction>.Update.Set(nameof(Transaction.Status), status);

            var mongoDb = this.client.GetDatabase("BlockChain");
            var transactions = mongoDb.GetCollection<Transaction>("Transacions");
            await transactions.UpdateOneAsync(filter, update);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByNodeAddress(string address)
        {
            var senderFilter = Builders<Transaction>.Filter.AnyEq(nameof(Transaction.SenderAddress), address);
            var receiverFilter = Builders<Transaction>.Filter.AnyEq(nameof(Transaction.ReceiverAddress), address);
            var transactions = await GetTransactions();
            var senderTransactions = transactions.Where(t => t.SenderAddress == address);
            var receiverTransactions = transactions.Where(t => t.ReceiverAddress == address);
            return senderTransactions.Union(receiverTransactions);
        }

        public async Task<TransactionStatus> GetTransactionStatus(string transactionId)
        {
            var transaction = await this.GetTransaction(transactionId);
            return transaction.Status;
        }
    }
}
