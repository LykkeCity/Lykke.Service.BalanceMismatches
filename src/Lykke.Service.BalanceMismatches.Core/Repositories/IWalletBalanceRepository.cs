using System.Threading.Tasks;

namespace Lykke.Service.BalanceMismatches.Core.Repositories
{
    public interface IWalletBalanceRepository
    {
        Task<decimal?> GetWalletBalanceAsync(string walletAddress);

        Task UpdateAsync(string walletAddress, decimal newValue);
    }
}
