using Lykke.Service.BalanceMismatches.Core.Services;
using System.Collections.Generic;

namespace Lykke.Service.BalanceMismatches.Services
{
    public class HotWalletManager : IHotWalletManager
    {
        private readonly HashSet<string> _assetIds;

        public HotWalletManager(IEnumerable<string> assets)
        {
            foreach (var assetId in assets)
            {
                _assetIds.Add(assetId);
            }
        }

        public bool IsAssetIdConfigured(string assetId)
        {
            return _assetIds.Contains(assetId);
        }
    }
}
