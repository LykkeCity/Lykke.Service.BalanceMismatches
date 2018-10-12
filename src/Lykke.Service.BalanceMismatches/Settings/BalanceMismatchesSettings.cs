using JetBrains.Annotations;
using System.Collections.Generic;

namespace Lykke.Service.BalanceMismatches.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class BalanceMismatchesSettings
    {
        public DbSettings Db { get; set; }

        public CqrsSettings Cqrs { get; set; }

        public string RedisConnString { get; set; }

        public List<string> Assets { get; set; }
    }
}
