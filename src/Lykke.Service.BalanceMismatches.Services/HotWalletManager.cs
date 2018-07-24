using Lykke.Service.BalanceMismatches.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.BalanceMismatches.Services
{
    public class HotWalletManager : IHotWalletManager
    {
        private readonly Dictionary<string, string> _walletsDict;

        public HotWalletManager(IEnumerable<(string, string)> hotWallet)
        {
            _walletsDict = hotWallet.ToDictionary(i => i.Item1, i => i.Item2);
        }

        public string GetIdByAssetId(string assetId)
        {
            if (_walletsDict.ContainsKey(assetId))
                return _walletsDict[assetId];
            throw new InvalidOperationException($"HotWallet for assetId {assetId} is not configured");
        }

        public bool IsAssetIdConfigured(string assetId)
        {
            return _walletsDict.ContainsKey(assetId);
        }
    }
}
