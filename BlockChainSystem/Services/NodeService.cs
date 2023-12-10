using BlockChainSystem.Models;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace BlockChainSystem.Services
{
    public class NodeService
    {
        private readonly WalletService walletService;
        private readonly BlockService blockService;
        private readonly TransactionService transactionService;
        private readonly ChainService chainService;
        private readonly MongoClient client;
        const int difficulty = 2;
        public NodeService(WalletService walletService,
                           BlockService blockService,
                           TransactionService transactionService,
                           ChainService chainService,
                           IConfiguration configuration)
        {
            this.client = ConnectionManager.GetMongoClient(configuration);
            this.walletService = walletService;
            this.blockService = blockService;
            this.chainService = chainService;
            this.transactionService = transactionService;
        }

        public async Task CreateNode(NodeCreationParams parameters)
        {
            var keys = this.walletService.GeneratePublicAndPrivateKeys();
            var node = new Node()
            {
                Wallet = new Wallet()
                {
                    PublicKey = keys.PublicKey,
                    PrivateKey = keys.PrivateKey,
                    Address = GenerateAddressFromPublicKey(keys.PublicKey),
                    InitialBalance = parameters.InitialBalance
                },
                IsFullNode = parameters.IsFullNode
            };

            var mongoDb = this.client.GetDatabase("BlockChain");
            var nodes = mongoDb.GetCollection<Node>("node");
            await nodes.InsertOneAsync(node);
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

        public async Task MineBlock(string transactionId)
        {
            await this.transactionService.UpdateTransactionStatus(transactionId, TransactionStatus.Mining);
            var transaction = await this.transactionService.GetTransaction(transactionId);
            var latestBlock = await this.chainService.GetLatestBlock();
            var block = new Block
            {
                Data = transaction,
                TimeStamp = DateTime.UtcNow,
                PreviousHash = latestBlock != null ? latestBlock.Hash : "0"
            };

            block.Hash = CalculateHash(block);
            while (!block.Hash.StartsWith(new string('0', difficulty)))
            {
                if (await this.transactionService.GetTransactionStatus(transactionId) == TransactionStatus.ReadyForHashVerification)
                {
                    return;
                }
                block.Nonce++;
                block.Hash = CalculateHash(block);
            }

            await this.transactionService.UpdateTransactionStatus(transactionId, TransactionStatus.ReadyForHashVerification);
            await this.blockService.Add(block);
        }

        private string CalculateHash(Block block)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var rawData = $"{block.TimeStamp}{block.PreviousHash}{block.Nonce}";
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public async Task CheckHashValidation(string transactionId)
        {
            string leadingZeros = new string('0', difficulty);

            var mongoDb = this.client.GetDatabase("BlockChain");
            var block = await this.chainService.GetLatestBlock();
            string calculatedHash = this.CalculateHash(block);

            if(calculatedHash.StartsWith(leadingZeros))
            {
                await this.transactionService.UpdateTransactionStatus(transactionId, TransactionStatus.Finished);
            }
            else
            {
                await this.blockService.Remove(block.Id);
                await this.transactionService.UpdateTransactionStatus(transactionId, TransactionStatus.ReadyForMining);
            }
        }


        public async Task<IEnumerable<Node>> GetNodes()
        {
            var mongoDb = this.client.GetDatabase("BlockChain");
            return (await mongoDb.GetCollection<Node>("node").FindAsync(_ => true)).ToEnumerable();
        }
    }
}