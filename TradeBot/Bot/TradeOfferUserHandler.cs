using SteamKit2;
using SteamTrade;
using SteamTrade.TradeOffer;
using System;
using System.Collections.Generic;
using System.Text;
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

        private void OnNewTradeOffer(TradeOffer steamOffer)
        {
            var databaseHandler = Bot.GetDatabaseHandler();
            User user = databaseHandler.GetUser(steamOffer.PartnerSteamId.ToString());
            BotCore.log.Info("Received offer from " + Bot.getPersonaName(user.SteamID) + "(steamID: " + user.SteamID + ")");
            BotCore.log.Info("PartnerID: " + steamOffer.PartnerSteamId);
            BotCore.log.Info("Message: " + steamOffer.Message);
            BotCore.log.Info("IsFirstOffer: " + steamOffer.IsFirstOffer);
            BotCore.log.Info("IsOurOffer: " + steamOffer.IsOurOffer);
            BotCore.log.Info("TradeofferID" + steamOffer.TradeOfferId);
            List<TradeOffer.TradeStatusUser.TradeAsset> theirAssets = steamOffer.Items.GetTheirItems();
            List<TradeOffer.TradeStatusUser.TradeAsset> myAssets = steamOffer.Items.GetMyItems();
            if(theirAssets.Count != 0)
            {
                var counter = 0;
                BotCore.log.Info("Their assets:");
                foreach (var asset in theirAssets)
                {

                    BotCore.log.Info("Asset no: " + counter);
                    BotCore.log.Info("AppId: " + asset.AppId);
                    BotCore.log.Info("ContextId: " + asset.ContextId);
                    BotCore.log.Info("AssetId: " + asset.AssetId);
                    BotCore.log.Info("Amount: " + asset.Amount);              
                    counter++;
                }
            }
            else
            {
                BotCore.log.Info("No assets in THEIR assets.");
            }
            if (myAssets.Count != 0)
            {
                var counter = 0;
                BotCore.log.Info("My assets:");
                foreach (var asset in myAssets)
                {

                    BotCore.log.Info("Asset no: " + counter);
                    BotCore.log.Info(" " + asset.AppId);
                    BotCore.log.Info(" " + asset.ContextId);
                    BotCore.log.Info(" " + asset.AssetId);
                    BotCore.log.Info(" " + asset.Amount);
                    counter++;
                }
            }
            else
            {
                BotCore.log.Info("No assets in MY assets.");
            }
        }

        #region old_OnNewTradeOffer
        //private void OnNewTradeOffer(TradeOffer steamOffer)
        //{
        //    var databaseHandler = Bot.GetDatabaseHandler();
        //    User user = databaseHandler.GetUser(steamOffer.PartnerSteamId.ToString());
        //    if (user != null)
        //    {
        //        string response;
        //        if(Bot.OfferAlreadyAdded(steamOffer))
        //        {
        //            if(!Bot.OfferActive(steamOffer))
        //            {
        //                steamOffer.Decline();
        //                response = "Received new offer from the same user or offer expired";
        //                Bot.DeleteOffer(steamOffer, response);
        //            }
        //            else
        //            {
        //                Transaction transaction = databaseHandler.GetUserTransaction(user.SteamID);
        //                if(transaction.Confirmed && transaction.MoneyTransfered)
        //                {
        //                    TradeOfferAcceptResponse acceptResp = steamOffer.Accept();
        //                    if (acceptResp.Accepted)
        //                    {
        //                        Bot.AcceptAllTradeConfirmations();
        //                        BotCore.log.Info("Accepted an offer from: " + Bot.getPersonaName(user.SteamID) + " (" + user.SteamID + ").");
        //                        response = "Transaction completed successfully - money has been transfered to you account.";
        //                        Bot.sendMessage(steamOffer.PartnerSteamId, response);
        //                        Bot.DeleteOffer(steamOffer, "Transaction completed");
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (CheckTradeOffer(steamOffer, out response))
        //            {
        //                BotCore.log.Info("Received an offer from: " + Bot.getPersonaName(user.SteamID) + " (" + user.SteamID + "). " + response);
        //                Bot.DeactivateOtherUserOffers(steamOffer);
        //                double offerValue = getOfferTotalValue(steamOffer);
        //                Bot.AddOffer(steamOffer, offerValue);
        //                response = "You will receive " + offerValue + "USD for your offer.\nTo confirm transaction type: '!confirm [your_paypal_email_address]' and we will transfer money to your account.";
        //                Bot.sendMessage(steamOffer.PartnerSteamId, response);
        //            }
        //            else
        //            {
        //                Bot.sendMessage(steamOffer.PartnerSteamId, response);
        //                steamOffer.Decline();
        //                BotCore.log.Info("Declined offer from: " + Bot.getPersonaName(user.SteamID) + " (" + user.SteamID + ")." + "Reason: " + response);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        steamOffer.Decline();
        //        BotCore.log.Info("Declined tradeoffer from " + steamOffer.PartnerSteamId + " (user not in db)");
        //    }
        //}
        #endregion

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


        #region old_offer_checking
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
        #endregion

        private bool CheckTradeOffer(TradeOffer steamOffer, out string response)
        {
            if (steamOffer.Items.GetMyItems().Count != 0 || steamOffer.Items.GetTheirItems().Count == 0)
            {
                response = "Incorrect number of items in trade offer.";
                return false;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("Items: ");
            foreach (var item in steamOffer.Items.GetTheirItems())
            {
                string marketName = Utils.getAssetMarketName(item, Bot.getAPIKey());
                if (item.AppId != 730 || item.ContextId != 2 || item.InstanceId != 143865972 || marketName.Equals("CS:GO Capsule Key") || marketName.Equals("Community Sticker Capsule 1 Key"))
                {
                    response = "Send only case keys in trade offer.";
                    return false;
                }
                sb.Append(Utils.getAssetMarketName(item, Bot.getAPIKey()));
            }
            response = sb.ToString();
            return true;
        }

        private double getOfferTotalValue(TradeOffer steamOffer)
        {
            double totalValue = 0;
            foreach (var item in steamOffer.Items.GetTheirItems())
            {
                string marketName = Utils.getAssetMarketName(item, Bot.getAPIKey());
                if (marketName.Equals("Operation Hydra Case Key"))
                    totalValue += Bot.getSellPriceHydra();
                else if (marketName.Equals("Revolver Case Key") || marketName.Equals("eSports Key") || marketName.Equals("CS:GO Case Key"))
                    totalValue += Bot.getSellPriceESports();
                else
                    totalValue += Bot.getSellPrice();                  
            }
            return totalValue;
        }
    }
}
