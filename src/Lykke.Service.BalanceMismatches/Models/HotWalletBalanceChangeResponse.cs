namespace Lykke.Service.BalanceMismatches.Models
{
    public class HotWalletBalanceChangeResponse
    {
        public string AssetId { get; set; }

        public decimal OldBalance { get; set; }

        public decimal NewBalance { get; set; }
    }
}
