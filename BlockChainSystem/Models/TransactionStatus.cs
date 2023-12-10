namespace BlockChainSystem.Models
{
    public enum TransactionStatus
    {
        ReadyForVerification,
        ReadyForMining,
        Mining,
        ReadyForHashVerification,
        Finished,
        Canceled,
        SignitureNotVerified
    }
}
