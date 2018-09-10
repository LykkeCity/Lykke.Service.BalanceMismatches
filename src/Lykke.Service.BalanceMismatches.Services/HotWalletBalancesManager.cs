using System;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Service.BalanceMismatches.Core.Repositories;
using Lykke.Service.BalanceMismatches.Core.Services;
using StackExchange.Redis;

namespace Lykke.Service.BalanceMismatches.Services
{
    public class HotWalletBalancesManager : IHotWalletBalancesManager
    {
        private const string _operationKeyPattern = "BalanceMismatches:opId:{0}";
        private const string _assetKeyPattern = "BalanceMismatches:assetId:{0}";

        private readonly IDatabase _db;
        private readonly IWalletBalanceRepository _walletBalanceRepository;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public HotWalletBalancesManager(IConnectionMultiplexer connectionMultiplexer, IWalletBalanceRepository walletBalanceRepository)
        {
            _db = connectionMultiplexer.GetDatabase();
            _walletBalanceRepository = walletBalanceRepository;
        }

        public async Task<decimal?> GetByAssetIdAsync(string assetId)
        {
            await _lock.WaitAsync();
            try
            {
                return await FetchAssetBalanceAsync(assetId, true);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<(decimal, decimal)> UpdateAsync(string assetId, decimal diff, string operationId)
        {
            var operationKey = string.IsNullOrEmpty(operationId)
                ? null
                : string.Format(_operationKeyPattern, operationId);

            await _lock.WaitAsync();
            try
            {
                decimal currentBalance = await FetchAssetBalanceAsync(assetId, false);
                if (operationKey != null && await _db.KeyExistsAsync(operationKey))
                    return (currentBalance, currentBalance);

                decimal newBalance = currentBalance + diff;

                var assetKey = string.Format(_assetKeyPattern, assetId);
                await _db.StringSetAsync(assetKey, newBalance.ToString(), TimeSpan.FromHours(1));

                await _walletBalanceRepository.UpdateAsync(assetId, newBalance);

                return (currentBalance, newBalance);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<decimal> FetchAssetBalanceAsync(string assetId, bool cachedIfNeeded)
        {
            var assetKey = string.Format(_assetKeyPattern, assetId);
            string assetVolumeStr = await _db.StringGetAsync(assetKey);
            if (!string.IsNullOrWhiteSpace(assetVolumeStr))
                return decimal.Parse(assetVolumeStr);

            decimal result = 0;
            var walletBalance = await _walletBalanceRepository.GetWalletBalanceAsync(assetId);
            if (walletBalance.HasValue)
                result = walletBalance.Value;

            if (cachedIfNeeded)
                await _db.StringSetAsync(assetKey, result.ToString());

            return result;
        }
    }
}
