using System.Threading;
using System.Threading.Tasks;
using Lykke.Service.BalanceMismatches.Core.Repositories;
using Lykke.Service.BalanceMismatches.Core.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Lykke.Service.BalanceMismatches.Services
{
    public class HotWalletBalancesManager : IHotWalletBalancesManager
    {
        private readonly IDistributedCache _cache;
        private readonly IWalletBalanceRepository _walletBalanceRepository;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public HotWalletBalancesManager(IDistributedCache cache, IWalletBalanceRepository walletBalanceRepository)
        {
            _cache = cache;
            _walletBalanceRepository = walletBalanceRepository;
        }

        public async Task<decimal?> GetByAssetIdAsync(string assetId)
        {
            await _lock.WaitAsync();
            try
            {
                decimal result = await FetchAssetBalanceAsync(assetId);
                return result;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<(decimal, decimal)> UpdateAsync(string assetId, decimal diff)
        {
            await _lock.WaitAsync();
            try
            {
                decimal currentBalance = await FetchAssetBalanceAsync(assetId);

                decimal newBalance = currentBalance + diff;

                await UpdateAssetVolumeAsync(assetId, newBalance);
                return (currentBalance, newBalance);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<decimal> FetchAssetBalanceAsync(string assetId)
        {
            string assetVolumeStr = await _cache.GetStringAsync(assetId);
            if (!string.IsNullOrWhiteSpace(assetVolumeStr))
                return decimal.Parse(assetVolumeStr);

            decimal result = 0;
            var walletBalance = await _walletBalanceRepository.GetWalletBalanceAsync(assetId);
            if (walletBalance.HasValue)
                result = walletBalance.Value;
            await _cache.SetStringAsync(assetId, result.ToString());
            return result;
        }

        private async Task UpdateAssetVolumeAsync(string assetId, decimal newBalance)
        {
            await _cache.SetStringAsync(assetId, newBalance.ToString());

            await _walletBalanceRepository.UpdateAsync(assetId, newBalance);
        }
    }
}
