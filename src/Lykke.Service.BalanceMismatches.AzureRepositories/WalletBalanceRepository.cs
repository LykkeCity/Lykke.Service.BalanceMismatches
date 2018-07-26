﻿using AzureStorage;
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

        public async Task<decimal?> GetWalletBalanceAsync(string walletAddress)
        {
            var balanceEntity = await _storage.GetDataAsync(WalletBalance.GetPartitionKey(), WalletBalance.GetRowKey(walletAddress));
            if (balanceEntity == null)
                return 0;
            return decimal.Parse(balanceEntity.BalanceStr);
        }

        public async Task UpdateAsync(string walletAddress, decimal newValue)
        {
            var entity = new WalletBalance
            {
                PartitionKey = WalletBalance.GetPartitionKey(),
                RowKey = WalletBalance.GetRowKey(walletAddress),
                BalanceStr = newValue.ToString(),
            };

            await _storage.InsertOrReplaceAsync(entity);
        }
    }
}
