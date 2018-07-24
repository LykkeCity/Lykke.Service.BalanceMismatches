using JetBrains.Annotations;
using System.Collections.Generic;

namespace Lykke.Service.BalanceMismatches.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class BalanceMismatchesSettings
    {
        public DbSettings Db { get; set; }

        public CqrsSettings Cqrs { get; set; }

        public List<HotWallet> HotWallets { get; set; }
    }

    public class HotWallet
    {
        public string AssetId { get; set; }

        public string WalletId { get; set; }
    }
}
