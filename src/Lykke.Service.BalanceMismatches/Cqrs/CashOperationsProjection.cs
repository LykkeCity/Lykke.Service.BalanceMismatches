using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.BlockchainCashoutProcessor.Contract.Events;
using Lykke.Service.BalanceMismatches.Core.Services;
using System;
using System.Threading.Tasks;

namespace Lykke.Service.BalanceMismatches.Cqrs
{
    internal class CashOperationsProjection
    {
        private readonly IHotWalletManager _hotWalletManager;
        private readonly IHotWalletBalancesManager _hotWalletBalancesManager;
        private readonly ILog _log;

        public CashOperationsProjection(
            IHotWalletManager hotWalletManager,
            IHotWalletBalancesManager hotWalletBalancesManager,
            ILogFactory logFactory)
        {
            _hotWalletManager = hotWalletManager;
            _hotWalletBalancesManager = hotWalletBalancesManager;
            _log = logFactory.CreateLog(this);
        }

        internal async Task Handle(Job.BlockchainCashinDetector.Contract.Events.CashinCompletedEvent evt)
        {
            if (!_hotWalletManager.IsAssetIdConfigured(evt.AssetId))
            {
                _log.Warning($"HotWallet is not configured for asset {evt.AssetId}");
                return;
            }

            await _hotWalletBalancesManager.UpdateAsync(evt.AssetId, evt.Amount);
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

        internal async Task Handle(CashoutCompletedEvent evt)
        {
            if (!_hotWalletManager.IsAssetIdConfigured(evt.AssetId))
            {
                _log.Warning($"HotWallet is not configured for asset {evt.AssetId}");
                return;
            }

            await _hotWalletBalancesManager.UpdateAsync(evt.AssetId, -Math.Abs(evt.Amount));
        }
    }
}
