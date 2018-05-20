namespace TradeBot.Database
{
    /// <summary>
    /// Class that represents stages of transactions.
    /// Trasaction stages are:
    /// WAITING_FOR_TRANSACTION,
    /// WAITING_FOR_CONFIRMATION,
    /// WAITING_FOR_TRADEOFFER,
    /// WAITING_FOR_ETH,
    /// SENDING_ETH
    /// </summary>
    public enum TransactionStage
    {
        /// <summary>
        /// waiting for transaction from user
        /// </summary>
        WAITING_FOR_TRANSACTION,
        /// <summary>
        /// waiting for cofirmation from user
        /// </summary>
        WAITING_FOR_CONFIRMATION,
        /// <summary>
        /// waiting for trade offer from user
        /// </summary>
        WAITING_FOR_TRADEOFFER,
        /// <summary>
        /// waiting for ethereum transfer from user
        /// </summary>
        WAITING_FOR_ETH,
        /// <summary>
        /// user is currently sending ethereum
        /// </summary>
        SENDING_ETH
    }
}
