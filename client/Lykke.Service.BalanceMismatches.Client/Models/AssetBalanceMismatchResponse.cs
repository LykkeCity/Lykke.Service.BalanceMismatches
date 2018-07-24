using JetBrains.Annotations;

namespace Lykke.Service.BalanceMismatches.Client.Models
{
    /// <summary>
    /// Asset balance mismatch container.
    /// </summary>
    [PublicAPI]
    public class AssetBalanceMismatchResponse
    {
        /// <summary>Asset id.</summary>
        public string AssetId { get; set; }

        /// <summary>Hot wallet balance.</summary>
        public decimal HotWalletBalance { get; set; }

        /// <summary>Sum of client balances for this asset.</summary>
        public decimal ClientSumBalance { get; set; }

        /// <summary>Sum of client reserved volumes for this asset.</summary>
        public decimal ClientSumReserved { get; set; }
    }
}
