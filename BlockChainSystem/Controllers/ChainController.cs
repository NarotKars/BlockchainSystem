using BlockChainSystem.Models;
using BlockChainSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlockChainSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChainController : Controller
    {
        private readonly ChainService chainService;

        public ChainController(ChainService chainService)
        {
            this.chainService = chainService;
        }

        [HttpGet]
        public async Task<Chain> GetChain()
        {
            return await this.chainService.GetChain();
        }
    }
}
