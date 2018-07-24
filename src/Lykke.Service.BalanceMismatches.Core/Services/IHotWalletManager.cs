namespace Lykke.Service.BalanceMismatches.Core.Services
{
    public interface IHotWalletManager
    {
        string GetIdByAssetId(string assetId);

        bool IsAssetIdConfigured(string assetId);
    }
}
