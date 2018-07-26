namespace Lykke.Service.BalanceMismatches.Core.Services
{
    public interface IHotWalletManager
    {
        string GetAddressByAssetId(string assetId);

        bool IsAssetIdConfigured(string assetId);
    }
}
