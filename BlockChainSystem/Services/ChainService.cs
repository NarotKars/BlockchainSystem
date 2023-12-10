using BlockChainSystem.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace BlockChainSystem.Services
{
    public class ChainService
    {
        private readonly MongoClient client;
        public ChainService(IConfiguration configuration)
        {
            this.client = ConnectionManager.GetMongoClient(configuration);
        }

        public async Task<Chain> GetChain()
        {
            var chain = new List<Block>();

            var mongoDb = this.client.GetDatabase("BlockChain");
            var blocks = (await mongoDb.GetCollection<Block>("block").FindAsync(_ => true)).ToList();

            foreach(var block in blocks)
            {
                if(block.PreviousHash == "0")
                {
                    chain.Add(block);
                    break;
                }
            }

            var index = 1;

            for(int i = 1; i < blocks.Count(); i++)
            {
                for(int j = 0; j < blocks.Count(); j++)
                {
                    if (chain[index - 1].Hash == blocks[j].PreviousHash)
                    {
                        chain.Add(blocks[j]);
                    }
                }
            }

            return new Chain()
            {
                Blocks = chain
            };
        }

        public async Task<Block> GetLatestBlock()
        {
            return (await this.GetChain()).Blocks.LastOrDefault();
        }
    }
}
