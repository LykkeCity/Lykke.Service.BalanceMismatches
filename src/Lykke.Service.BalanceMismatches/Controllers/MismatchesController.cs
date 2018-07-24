using Lykke.Service.BalanceMismatches.Client.Models;
using Lykke.Service.BalanceMismatches.Core.Services;
using Lykke.Service.BalanceMismatches.Models;
using Lykke.Service.Balances.Client;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.BalanceMismatches.Controllers
{
    [Route("api/[controller]")]
    public class MismatchesController : Controller
    {
        private readonly IBalancesClient _balancesClient;
        private readonly IHotWalletBalancesManager _hotWalletsBalanceRepository;

        public MismatchesController(IBalancesClient balancesClient, IHotWalletBalancesManager hotWalletsBalanceRepository)
        {
            _balancesClient = balancesClient;
            _hotWalletsBalanceRepository = hotWalletsBalanceRepository;
        }

        /// <summary>Fetches asset balance mismatch.</summary>
        [HttpPost("{assetId}")]
        [SwaggerOperation("GetBalanceMismath")]
        [ProducesResponseType(typeof(AssetBalanceMismatchResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetBalanceMismath(string assetId)
        {
            if (string.IsNullOrWhiteSpace(assetId))
                return BadRequest($"{assetId} is empty");

            var totalBalances = await _balancesClient.GetTotalBalances();

            var assetBalance = totalBalances.FirstOrDefault(b => b.AssetId == assetId);
            if (assetBalance == null)
                return NotFound();

            var hotWalletBalance = await _hotWalletsBalanceRepository.GetByAssetIdAsync(assetId);

            return Ok(
                new AssetBalanceMismatchResponse
                {
                    AssetId = assetId,
                    HotWalletBalance = hotWalletBalance,
                    ClientSumBalance = assetBalance.Balance,
                    ClientSumReserved = assetBalance.Reserved,
                });
        }

        /// <summary>Fetches asset balance mismatch.</summary>
        /// <remarks>This method must be avaialble only via swagger. It MUST NOT be exposed for service clients.</remarks>
        [HttpPost("hotwallet/{assetId}/{change}")]
        [SwaggerOperation("ChangeHotWalletBalance")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ChangeHotWalletBalance(string assetId, decimal diff)
        {
            if (string.IsNullOrWhiteSpace(assetId))
                return BadRequest($"{assetId} is empty");

            if (diff == 0)
                return BadRequest($"{diff} is 0");

            (decimal oldBalance, decimal newBalance) = await _hotWalletsBalanceRepository.UpdateAsync(assetId, diff);

            return Ok(
                new HotWalletBalanceChangeResponse
                {
                    AssetId = assetId,
                    OldBalance = oldBalance,
                    NewBalance = newBalance,
                });
        }
    }
}
