using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.BlockchainCashinDetector.Contract.Events;
using Lykke.Service.BalanceMismatches.Core.Services;
using System.Threading.Tasks;

namespace Lykke.Service.BalanceMismatches.Cqrs
{
    internal class CashinProjection
    {
        private readonly IHotWalletManager _hotWalletManager;
        private readonly IHotWalletBalancesManager _hotWalletBalancesManager;
        private readonly ILog _log;

        public CashinProjection(
            IHotWalletManager hotWalletManager,
            IHotWalletBalancesManager hotWalletBalancesManager,
            ILogFactory logFactory)
        {
            _hotWalletManager = hotWalletManager;
            _hotWalletBalancesManager = hotWalletBalancesManager;
            _log = logFactory.CreateLog(this);
        }

        internal async Task Handle(CashinCompletedEvent evt)
        {
            if (!_hotWalletManager.IsAssetIdConfigured(evt.AssetId))
            {
                _log.Warning($"HotWallet is not configured for asset {evt.AssetId}");
                return;
            }

            await _hotWalletBalancesManager.UpdateAsync(evt.AssetId, evt.Amount);
        }
    }
}
