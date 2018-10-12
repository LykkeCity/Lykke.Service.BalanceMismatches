using Autofac;
using AzureStorage;
using AzureStorage.Tables;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.Balances.Client;
using Lykke.Service.BalanceMismatches.AzureRepositories;
using Lykke.Service.BalanceMismatches.Core.Repositories;
using Lykke.Service.BalanceMismatches.Core.Services;
using Lykke.Service.BalanceMismatches.Services;
using Lykke.Service.BalanceMismatches.Settings;
using Lykke.SettingsReader;
using StackExchange.Redis;

namespace Lykke.Service.BalanceMismatches.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var settings = _appSettings.CurrentValue.BalanceMismatchesService;

            builder.RegisterType<HotWalletBalancesManager>()
                .As<IHotWalletBalancesManager>()
                .SingleInstance();

            builder.RegisterType<HotWalletManager>()
                .As<IHotWalletManager>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(settings.Assets));

            builder.Register(context => ConnectionMultiplexer.Connect(settings.RedisConnString))
                .As<IConnectionMultiplexer>()
                .SingleInstance();

            builder.RegisterBalancesClient(_appSettings.Nested(s => s.BalancesServiceClient).CurrentValue);

            builder.Register(c =>
                {
                    var logFactory = c.Resolve<ILogFactory>();
                    return AzureTableStorage<WalletBalance>.Create(
                        _appSettings.Nested(s => s.BalanceMismatchesService.Db.DataConnString),
                        "HotWalletBalances",
                        logFactory);
                })
                .As<INoSQLTableStorage<WalletBalance>>()
                .SingleInstance();

            builder.RegisterType<WalletBalanceRepository>()
                .As<IWalletBalanceRepository>()
                .SingleInstance();
        }
    }
}
