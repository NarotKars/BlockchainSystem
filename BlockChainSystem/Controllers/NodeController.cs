using BlockChainSystem.Models;
using BlockChainSystem.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BlockChainSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NodeController : Controller
    {
        private readonly NodeService nodeService;
        private readonly WalletService walletService;
        public NodeController(NodeService nodeService, WalletService walletService)
        {
            this.nodeService = nodeService;
            this.walletService = walletService;
        }

        [HttpPost]
        public async Task AddNode(NodeCreationParams parameters)
        {
            await this.nodeService.CreateNode(parameters);
        }

        [HttpGet]
        public async Task<IEnumerable<Node>> GetNodes()
        {
            return await this.nodeService.GetNodes();
        }

        [HttpPost("mine/{transactionId}")]
        public async Task Mine(string transactionId)
        {
            await this.nodeService.MineBlock(transactionId);
        }

        [HttpPut("verifyHash/{transactionId}")]
        public async Task VerifyHash(string transactionId)
        {
            await this.nodeService.CheckHashValidation(transactionId);
        }

        [HttpGet("balance")]
        public async Task<decimal> GetBalance(string address)
        {
            return await this.walletService.GetBalance(address);
        }
    }
}
