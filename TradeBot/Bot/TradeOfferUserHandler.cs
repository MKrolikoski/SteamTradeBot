using SteamKit2;
using SteamTrade;
using SteamTrade.TradeOffer;
using System;
using TradeBot.Entity;
using TradeBot.Messages;

namespace TradeBot.Bot
{
    public class TradeOfferUserHandler : UserHandler
    {
        public TradeOfferUserHandler(BotCore bot, SteamID sid) : base(bot, sid) { }

        public override void OnTradeOfferUpdated(TradeOffer offer)
        {
            if (offer.OfferState == TradeOfferState.TradeOfferStateActive && !offer.IsOurOffer)
            {
                OnNewTradeOffer(offer);
            }
        }

        private void OnNewTradeOffer(TradeOffer offer)
        {
            var databaseHandler = Bot.GetDatabaseHandler();
            User user = databaseHandler.GetUser(offer.PartnerSteamId.ToString());
            if (user != null)
            {
                string response;
                Transaction transaction = databaseHandler.GetUserTransaction(user.SteamID);
                if (transaction != null && transaction.Confirmed)
                {
                    Tradeoffer tradeoffer = databaseHandler.GetUserTradeOffer(user.SteamID);
                    if (!tradeoffer.Accepted)
                    {
                        if (transaction.Buy)
                        {
                            if (CheckTradeOffer(tradeoffer, offer, MessageType.BUY, out response))
                            {
                                tradeoffer.Accepted = true;
                                databaseHandler.UpdateTradeOffer(tradeoffer);
                                response += " Please send " + databaseHandler.getTransactionEthValue(user.SteamID) + " to this address: " + Bot.getEthAddress();
                                Bot.sendMessage(offer.PartnerSteamId, response);
                            }
                            else
                            {
                                Bot.sendMessage(offer.PartnerSteamId, response);
                                offer.Decline();
                            }
                        }
                        else
                        {
                            if (CheckTradeOffer(tradeoffer, offer, MessageType.SELL, out response))
                            {
                                Bot.sendMessage(offer.PartnerSteamId, response);
                                TradeOfferAcceptResponse acceptResp = offer.Accept();
                                if (acceptResp.Accepted)
                                {
                                    Bot.AcceptAllTradeConfirmations();
                                    tradeoffer.Accepted = true;
                                    databaseHandler.UpdateTradeOffer(tradeoffer);
                                    response = "Sending ETH to " + user.WalletAddress + ".";
                                    Bot.sendMessage(offer.PartnerSteamId, response);
                                    if (Bot.sendEth(user.WalletAddress, databaseHandler.getTransactionEthValue(tradeoffer)))
                                    {
                                        response = "Etherneum transfer completed successfully.";
                                        Bot.sendMessage(offer.PartnerSteamId, response);
                                        databaseHandler.DeleteTransaction(transaction);
                                    }
                                    else
                                    {
                                        response = "Error while transfering etherneum.\nMake sure you've correctly set your ETH address and/or enabled ETH transactions on your account.";
                                        Bot.sendMessage(offer.PartnerSteamId, response);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Error while accepting trade");
                                }
                            }
                            else
                            {
                                Bot.sendMessage(offer.PartnerSteamId, response);
                                offer.Decline();
                            }
                        }
                    }
                    else
                    {
                        if (transaction.Buy)
                        {
                            if(Bot.checkIfTransfered(transaction.CreationDate, databaseHandler.getTransactionEthValue(tradeoffer)))
                            {
                                response = "Etherneum received.\nConfirming trade offer..";
                                Bot.sendMessage(offer.PartnerSteamId, response);
                                TradeOfferAcceptResponse acceptResp = offer.Accept();
                                if (acceptResp.Accepted)
                                {
                                    Bot.AcceptAllTradeConfirmations();
                                    response = "Trade offer confirmed.";
                                    Bot.sendMessage(offer.PartnerSteamId, response);
                                    databaseHandler.DeleteTransaction(transaction);
                                }
                            }
                        }
                        else
                        {
                            if (Bot.sendEth(user.WalletAddress, databaseHandler.getTransactionEthValue(tradeoffer)))
                            {
                                response = "Etherneum transfer completed successfully.";
                                Bot.sendMessage(offer.PartnerSteamId, response);
                                databaseHandler.DeleteTransaction(transaction);
                            }
                        }
                    }
                }
                else
                {
                    Bot.sendMessage(offer.PartnerSteamId, "Create a transaction or confirm current one before sending trade offers.");
                    offer.Decline();
                }
            }
            else
            {
                Console.WriteLine("User not in database.");
                offer.Decline();
            }
        }

        public override void OnMessage(string message, EChatEntryType type){}

        public override bool OnGroupAdd() { return false; }

        public override bool OnFriendAdd() { return true; }

        public override void OnFriendRemove() { }

        public override void OnLoginCompleted() { }

        public override bool OnTradeRequest() { return false; }

        public override void OnTradeError(string error) { }

        public override void OnTradeTimeout() { }

        public override void OnTradeAwaitingConfirmation(long tradeOfferID) { }

        public override void OnTradeInit() { }

        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem) { }

        public override void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem) { }

        public override void OnTradeMessage(string message) { }

        public override void OnTradeReady(bool ready) { }

        public override void OnTradeAccept() { }

        public override void OnTradeAcceptHandler() { }


        private bool CheckTradeOffer(Tradeoffer DBoffer, TradeOffer steamOffer, MessageType transactionType, out string response)
        {
            if (transactionType == MessageType.BUY)
            {
                //BUY
                if (steamOffer.Items.GetTheirItems().Count != 0 || steamOffer.Items.GetMyItems().Count == 0 || steamOffer.Items.GetMyItems().Count != DBoffer.Amount)
                {
                    response = "Incorrect number of items in trade offer.";
                    return false;
                }
                foreach (var item in steamOffer.Items.GetMyItems())
                {
                    if (item.AppId != 730 || item.ContextId != 2 || item.InstanceId != 143865972)
                    {
                        response = "Send only keys in trade offer.";
                        return false;
                    }
                }
            }
            else
            {
                //SELL
                if (steamOffer.Items.GetMyItems().Count != 0 || steamOffer.Items.GetTheirItems().Count == 0 || steamOffer.Items.GetTheirItems().Count != DBoffer.Amount)
                {
                    response = "Incorrect number of items in trade offer.";
                    return false;
                }
                foreach (var item in steamOffer.Items.GetTheirItems())
                {
                    if (item.AppId != 730 || item.ContextId !=2 || item.InstanceId != 143865972)
                    {
                        response = "Send only keys in trade offer.";
                        return false;
                    }
                }
            }
            response = "Correct trade offer.";
            return true;
        }
    }
}
