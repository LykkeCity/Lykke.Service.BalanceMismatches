using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.BalanceMismatches.AzureRepositories
{
    public class WalletBalance : TableEntity
    {
        public string BalanceStr { get; set; }

        public static string GetPartitionKey()
        {
            return "HotWalletBalance";
        }

        public static string GetRowKey(string walletId)
        {
            return walletId;
        }
    }
}
