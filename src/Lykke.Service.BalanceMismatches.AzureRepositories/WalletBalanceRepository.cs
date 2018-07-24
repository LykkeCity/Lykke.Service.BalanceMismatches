using AzureStorage;
using Lykke.Common.Log;
using Lykke.Service.BalanceMismatches.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace Lykke.Service.BalanceMismatches.AzureRepositories
{
    public class WalletBalanceRepository : IWalletBalanceRepository
    {
        private readonly INoSQLTableStorage<WalletBalance> _storage;

        public WalletBalanceRepository(ILogFactory logFactory, Func<ILogFactory, INoSQLTableStorage<WalletBalance>> storageFactoryMethod)
        {
            _storage = storageFactoryMethod(logFactory);
        }

        public async Task<decimal?> GetWalletBalanceAsync(string walletId)
        {
            var balanceEntity = await _storage.GetDataAsync(WalletBalance.GetPartitionKey(), WalletBalance.GetRowKey(walletId));
            if (balanceEntity == null)
                return 0;
            return decimal.Parse(balanceEntity.BalanceStr);
        }

        public async Task UpdateAsync(string walletId, decimal newValue)
        {
            var entity = new WalletBalance
            {
                PartitionKey = WalletBalance.GetPartitionKey(),
                RowKey = WalletBalance.GetRowKey(walletId),
                BalanceStr = newValue.ToString(),
            };

            await _storage.InsertOrReplaceAsync(entity);
        }
    }
}
