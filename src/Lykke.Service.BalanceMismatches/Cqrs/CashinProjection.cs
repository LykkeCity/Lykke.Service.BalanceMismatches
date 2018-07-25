using Lykke.Job.BlockchainCashinDetector.Contract.Events;
using Lykke.Service.BalanceMismatches.Core.Services;
using System.Threading.Tasks;

namespace Lykke.Service.BalanceMismatches.Cqrs
{
    internal class CashinProjection
    {
        private readonly IHotWalletManager _hotWalletManager;
        private readonly IHotWalletBalancesManager _hotWalletBalancesManager;

        public CashinProjection(IHotWalletManager hotWalletManager, IHotWalletBalancesManager hotWalletBalancesManager)
        {
            _hotWalletManager = hotWalletManager;
            _hotWalletBalancesManager = hotWalletBalancesManager;
        }

        internal async Task Handle(CashinCompletedEvent evt)
        {
            if (!_hotWalletManager.IsAssetIdConfigured(evt.AssetId))
                return;

            await _hotWalletBalancesManager.UpdateAsync(evt.AssetId, evt.Amount);
        }
    }
}
