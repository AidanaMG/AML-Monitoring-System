

namespace Aml.Application.RulesAbstractions
{
    public class AccountActivity
    {
        public int AccountId { get; init; }
        public decimal CurrentAccountBalance { get; init; }
        public decimal IncomingP2pAmountSumLast24h { get; init; }
    }
}
