namespace Lykke.Service.BalanceMismatches.Core.Services
{
    public interface IHotWalletManager
    {
        bool IsAssetIdConfigured(string assetId);
    }
}
