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
                return BadRequest($"{nameof(assetId)} is empty");

            var hotWalletBalance = await _hotWalletsBalanceRepository.GetByAssetIdAsync(assetId);
            if (!hotWalletBalance.HasValue)
                return NotFound();

            var totalBalances = await _balancesClient.GetTotalBalances();

            decimal clientSumBalance = 0;
            decimal clientSumReserved = 0;
            var assetBalance = totalBalances.FirstOrDefault(b => b.AssetId == assetId);
            if (assetBalance != null)
            {
                clientSumBalance = assetBalance.Balance;
                clientSumReserved = assetBalance.Reserved;
            }

            return Ok(
                new AssetBalanceMismatchResponse
                {
                    AssetId = assetId,
                    HotWalletBalance = hotWalletBalance.Value,
                    ClientSumBalance = clientSumBalance,
                    ClientSumReserved = clientSumReserved,
                });
        }

        /// <summary>Fetches asset balance mismatch.</summary>
        /// <remarks>This method must be avaialble only via swagger. It MUST NOT be exposed for service clients.</remarks>
        [HttpPost("hotwallet/{assetId}/{diff}")]
        [SwaggerOperation("ChangeHotWalletBalance")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ChangeHotWalletBalance(string assetId, decimal diff)
        {
            if (string.IsNullOrWhiteSpace(assetId))
                return BadRequest($"{nameof(assetId)} is empty");

            if (diff == 0)
                return BadRequest($"{nameof(diff)} is 0");

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
