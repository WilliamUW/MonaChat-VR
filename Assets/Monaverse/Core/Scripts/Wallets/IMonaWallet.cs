using System.Threading.Tasks;
using Monaverse.Core;

namespace Monaverse.Wallets
{
    public interface IMonaWallet
    {
        Task<string> Connect(MonaWalletConnection monaWalletConnection);
        Task<bool> Disconnect();
        Task<string> GetAddress();
        Task<string> SignMessage(string message);
        Task<bool> IsConnected();
        MonaWalletProvider GetProvider();
    }
}