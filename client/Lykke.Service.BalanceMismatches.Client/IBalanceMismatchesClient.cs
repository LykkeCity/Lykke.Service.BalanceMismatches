using JetBrains.Annotations;
using Lykke.Service.BalanceMismatches.Client.Models;
using Refit;
using System.Threading.Tasks;

namespace Lykke.Service.BalanceMismatches.Client
{
    /// <summary>
    /// BalanceMismatches client interface.
    /// </summary>
    [PublicAPI]
    public interface IBalanceMismatchesClient
    {
        /// <summary>
        /// Fetches mismatch between asset hotwallet and sum of client balances for this wallet.
        /// </summary>
        /// <param name="assetId">Asset id.</param>
        /// <returns>Asset balance mismatch.</returns>
        [Post("/api/Mismatches/{assetId}")]
        Task<AssetBalanceMismatchResponse> GetBalanceMismathAsync(string assetId);
    }
}
