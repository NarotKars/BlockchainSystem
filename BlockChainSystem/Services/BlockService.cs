using BlockChainSystem.Models;
using MongoDB.Driver;

namespace BlockChainSystem.Services
{
    public class BlockService
    {
        private readonly MongoClient client;
        public BlockService(IConfiguration configuration)
        {
            this.client = ConnectionManager.GetMongoClient(configuration);
        }

        public async Task Add(Block block)
        {
            var mongoDb = this.client.GetDatabase("BlockChain");
            var blocks = (await mongoDb.GetCollection<Block>("block").FindAsync(_ => true)).ToEnumerable();
            if(blocks.Count() == 0)
            {
                block.PreviousHash = "0";
            }
            await mongoDb.GetCollection<Block>("block").InsertOneAsync(block);
        }

        public async Task Remove(string blockId)
        {
            var mongoDb = this.client.GetDatabase("BlockChain");
            await mongoDb.GetCollection<Block>("block").DeleteOneAsync(block => block.Id == blockId);
        }
    }
}
