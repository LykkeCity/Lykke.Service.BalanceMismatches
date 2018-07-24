using Lykke.Service.BalanceMismatches.Core.Repositories;
using Lykke.Service.BalanceMismatches.Core.Services;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.Service.BalanceMismatches.Services
{
    public class HotWalletBalancesManager : IHotWalletBalancesManager
    {
        private readonly IDistributedCache _cache;
        private readonly IHotWalletManager _hotWalletRepository;
        private readonly IWalletBalanceRepository _walletBalanceRepository;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public HotWalletBalancesManager(
            IDistributedCache cache,
            IHotWalletManager hotWalletRepository,
            IWalletBalanceRepository walletBalanceRepository)
        {
            _cache = cache;
            _hotWalletRepository = hotWalletRepository;
            _walletBalanceRepository = walletBalanceRepository;
        }

        public async Task<decimal> GetByAssetIdAsync(string assetId)
        {
            var hotWalletId = _hotWalletRepository.GetIdByAssetId(assetId);
            await _lock.WaitAsync();
            try
            {
                decimal result = await FetchAssetBalanceAsync(hotWalletId);
                return result;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<(decimal, decimal)> UpdateAsync(string assetId, decimal diff)
        {
            var hotWalletId = _hotWalletRepository.GetIdByAssetId(assetId);

            await _lock.WaitAsync();
            try
            {
                decimal currentBalance = await FetchAssetBalanceAsync(hotWalletId);

                decimal newBalance = currentBalance + diff;
                if (currentBalance < 0)
                    throw new InvalidOperationException(
                        $"{assetId} hot wallet balance change from {currentBalance} with diff = {diff} resulted in negative value");

                await UpdateAssetVolumeAsync(hotWalletId, currentBalance);
                return (currentBalance, newBalance);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<decimal> FetchAssetBalanceAsync(string walletId)
        {
            string assetVolumeStr = await _cache.GetStringAsync(walletId);
            if (!string.IsNullOrWhiteSpace(assetVolumeStr))
                return decimal.Parse(assetVolumeStr);

            decimal result = 0;
            var walletBalance = await _walletBalanceRepository.GetWalletBalanceAsync(walletId);
            if (walletBalance.HasValue)
                result = walletBalance.Value;
            await _cache.SetStringAsync(walletId, result.ToString());
            return result;
        }

        private async Task UpdateAssetVolumeAsync(string walletId, decimal newBalance)
        {
            await _cache.SetStringAsync(walletId, newBalance.ToString());

            await _walletBalanceRepository.UpdateAsync(walletId, newBalance);
        }
    }
}
