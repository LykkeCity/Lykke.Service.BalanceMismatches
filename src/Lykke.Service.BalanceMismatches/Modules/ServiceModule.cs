using Autofac;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Service.Balances.Client;
using Lykke.Service.BalanceMismatches.AzureRepositories;
using Lykke.Service.BalanceMismatches.Core.Repositories;
using Lykke.Service.BalanceMismatches.Core.Services;
using Lykke.Service.BalanceMismatches.Services;
using Lykke.Service.BalanceMismatches.Settings;
using Lykke.SettingsReader;
using System;
using System.Linq;

namespace Lykke.Service.BalanceMismatches.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HotWalletBalancesManager>()
                .As<IHotWalletBalancesManager>()
                .SingleInstance();

            builder.RegisterType<HotWalletManager>()
                .As<IHotWalletManager>()
                .SingleInstance()
                .WithParameter(
                    TypedParameter.From(
                        _appSettings.Nested(s => s.BalanceMismatchesService.HotWallets).CurrentValue.Select(i => (i.AssetId, i.WalletId))));

            builder.RegisterBalancesClient(_appSettings.Nested(s => s.BalancesServiceClient).CurrentValue);

            Func<ILogFactory, INoSQLTableStorage<WalletBalance>> storageInit = l =>
                AzureTableStorage<WalletBalance>.Create(_appSettings.Nested(s => s.BalanceMismatchesService.Db.DataConnString), "HotWalletBalances", l);

            builder.RegisterType<WalletBalanceRepository>()
                .As<IWalletBalanceRepository>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(storageInit));
        }
    }
}
