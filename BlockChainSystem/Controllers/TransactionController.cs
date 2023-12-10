using BlockChainSystem.Models;
using BlockChainSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlockChainSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionService transactionService;

        public TransactionController(TransactionService transactionService)
        {
            this.transactionService = transactionService;
        }

        [HttpGet]
        public async Task<IEnumerable<Transaction>> Get()
        {
            return await this.transactionService.GetTransactions();
        }

        [HttpPost]
        public async Task Create(TransactionCreationParams parameters)
        {
            await this.transactionService.CreateTransaction(parameters);
        }

        [HttpPut("Verify/{transactionId}")]
        public async Task VerifySigniture(string transactionId, string verifierAddress)
        {
            await this.transactionService.VerifySignature(transactionId);
        }
    }
}
