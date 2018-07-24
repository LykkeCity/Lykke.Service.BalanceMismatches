using System.Threading.Tasks;

namespace Lykke.Service.BalanceMismatches.Core.Services
{
    public interface IHotWalletBalancesManager
    {
        Task<decimal> GetByAssetIdAsync(string assetId);

        Task<(decimal, decimal)> UpdateAsync(string assetId, decimal diff);
    }
}
