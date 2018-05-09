namespace TradeBot.Database
{
    public enum TransactionStage
    {
        WAITING_FOR_TRANSACTION,
        WAITING_FOR_CONFIRMATION,
        WAITING_FOR_TRADEOFFER,
        WAITING_FOR_ETH,
        SENDING_ETH
    }
}
