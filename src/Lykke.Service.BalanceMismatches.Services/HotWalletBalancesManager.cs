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
        private readonly IHotWalletManager _hotWalletManager;
        private readonly IWalletBalanceRepository _walletBalanceRepository;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public HotWalletBalancesManager(
            IDistributedCache cache,
            IHotWalletManager hotWalletManager,
            IWalletBalanceRepository walletBalanceRepository)
        {
            _cache = cache;
            _hotWalletManager = hotWalletManager;
            _walletBalanceRepository = walletBalanceRepository;
        }

        public async Task<decimal?> GetByAssetIdAsync(string assetId)
        {
            var hotWalletAddress = _hotWalletManager.GetAddressByAssetId(assetId);
            if (hotWalletAddress == null)
                return null;

            await _lock.WaitAsync();
            try
            {
                decimal result = await FetchAssetBalanceAsync(hotWalletAddress);
                return result;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<(decimal, decimal)> UpdateAsync(string assetId, decimal diff)
        {
            var hotWalletAddress = _hotWalletManager.GetAddressByAssetId(assetId);
            if (hotWalletAddress == null)
                throw new InvalidOperationException($"HotWallet is not configured for asset {assetId}");

            await _lock.WaitAsync();
            try
            {
                decimal currentBalance = await FetchAssetBalanceAsync(hotWalletAddress);

                decimal newBalance = currentBalance + diff;
                if (currentBalance < 0)
                    throw new InvalidOperationException(
                        $"{assetId} hot wallet balance change from {currentBalance} with diff = {diff} resulted in negative value");

                await UpdateAssetVolumeAsync(hotWalletAddress, currentBalance);
                return (currentBalance, newBalance);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<decimal> FetchAssetBalanceAsync(string walletAddress)
        {
            string assetVolumeStr = await _cache.GetStringAsync(walletAddress);
            if (!string.IsNullOrWhiteSpace(assetVolumeStr))
                return decimal.Parse(assetVolumeStr);

            decimal result = 0;
            var walletBalance = await _walletBalanceRepository.GetWalletBalanceAsync(walletAddress);
            if (walletBalance.HasValue)
                result = walletBalance.Value;
            await _cache.SetStringAsync(walletAddress, result.ToString());
            return result;
        }

        private async Task UpdateAssetVolumeAsync(string walletAddress, decimal newBalance)
        {
            await _cache.SetStringAsync(walletAddress, newBalance.ToString());

            await _walletBalanceRepository.UpdateAsync(walletAddress, newBalance);
        }
    }
}
