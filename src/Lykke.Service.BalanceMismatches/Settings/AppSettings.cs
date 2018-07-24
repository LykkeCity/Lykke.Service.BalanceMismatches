using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.Balances.Client;

namespace Lykke.Service.BalanceMismatches.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public BalanceMismatchesSettings BalanceMismatchesService { get; set; }

        public BalancesServiceClientSettings BalancesServiceClient { get; set; }
    }
}
