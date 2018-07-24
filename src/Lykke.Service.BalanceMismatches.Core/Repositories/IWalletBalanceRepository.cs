using System.Threading.Tasks;

namespace Lykke.Service.BalanceMismatches.Core.Repositories
{
    public interface IWalletBalanceRepository
    {
        Task<decimal?> GetWalletBalanceAsync(string walletId);

        Task UpdateAsync(string walletId, decimal newValue);
    }
}
