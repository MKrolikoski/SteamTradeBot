using System;
using System.Linq;
using System.Threading.Tasks;
using SteamKit2;
using SteamTrade;
using SteamTrade.TradeOffer;

namespace TradeBot.Bot
{
    /// <summary>
    /// The abstract base class for users of SteamBot that will allow a user
    /// to extend the functionality of the Bot.
    /// </summary>
    public abstract class UserHandler
    {
        public BotCore Bot { get; private set; }
        public SteamID OtherSID { get; private set; }

        private bool _lastMessageWasFromTrade;
        private Task<Inventory> otherInventoryTask;
        private TaskCompletionSource<string> _waitingOnUserResponse;

        protected SteamWeb SteamWeb
        {
            get
            {
                if (Bot == null || Bot.steamWeb == null)
                {
                    throw new InvalidOperationException("You cannot use 'SteamWeb' before the Bot has been initialized!");
                }
                return Bot.steamWeb;
            }
        }

        public UserHandler(BotCore bot, SteamID sid)
        {
            Bot = bot;
            OtherSID = sid;
        }

        private bool HandleWaitingOnUserResponse(string message)
        {
            if (_waitingOnUserResponse == null)
                return false;

            _waitingOnUserResponse.SetResult(message);
            _waitingOnUserResponse = null;
            return true;
        }

        /// <summary>
        /// Gets the other's inventory and stores it in OtherInventory.
        /// </summary>
        /// <example> This sample shows how to find items in the other's inventory from a user handler.
        /// <code>
        /// GetInventory(); // Not necessary unless you know the user's inventory has changed
        /// foreach (var item in OtherInventory)
        /// {
        ///     if (item.Defindex == 5021)
        ///     {
        ///         // Bot has a key in its inventory
        ///     }
        /// }
        /// </code>
        /// </example>
        public Inventory OtherInventory
        {
            get
            {
                otherInventoryTask.Wait();
                return otherInventoryTask.Result;
            }
        }




        /// <summary>
        /// Called when the bot is invited to a Steam group
        /// </summary>
        /// <returns>
        /// Whether to accept.
        /// </returns>
        public abstract bool OnGroupAdd();

        /// <summary>
        /// Called when the user adds the bot as a friend.
        /// </summary>
        /// <returns>
        /// Whether to accept.
        /// </returns>
        public abstract bool OnFriendAdd();

        /// <summary>
        /// Called when the user removes the bot as a friend.
        /// </summary>
        public abstract void OnFriendRemove();

        /// <summary>
        /// Called whenever a message is sent to the bot.
        /// This is limited to regular and emote messages.
        /// </summary>
        public abstract void OnMessage(string message, EChatEntryType type);

        public void OnMessageHandler(string message, EChatEntryType type)
        {
            _lastMessageWasFromTrade = false;
            if (!HandleWaitingOnUserResponse(message))
            {
                OnMessage(message, type);
            }
        }

        /// <summary>
        /// Called when the bot is fully logged in.
        /// </summary>
        public abstract void OnLoginCompleted();

        /// <summary>
        /// Called whenever a user requests a trade.
        /// </summary>
        /// <returns>
        /// Whether to accept the request.
        /// </returns>
        public abstract bool OnTradeRequest();


        /// <summary>
        /// Called when a trade offer is updated, including the first time it is seen.
        /// When the bot is restarted, this might get called again for trade offers it's been previously called on.  Thus, you can't rely on
        /// this method being called only once after an offer is accepted!  If you need to rely on that functionality (say for giving users non-Steam currency),
        ///  you need to keep track of which trades have been paid out yourself
        /// </summary>
        public abstract void OnTradeOfferUpdated(TradeOffer offer);

        /// <summary>
        /// Called when a chat message is sent in a chatroom
        /// </summary>
        /// <param name="chatID">The SteamID of the group chat</param>
        /// <param name="sender">The SteamID of the sender</param>
        /// <param name="message">The message sent</param>
        public virtual void OnChatRoomMessage(SteamID chatID, SteamID sender, string message)
        {

        }

        /// <summary>
        /// Called when an 'exec' command is given via botmanager.
        /// </summary>
        /// <param name="command">The command message.</param>
        public virtual void OnBotCommand(string command)
        {

        }

        /// <summary>
        /// Called when user accepts or denies bot's trade request.
        /// </summary>
        /// <param name="accepted">True if user accepted bot's request, false if not.</param>
        /// <param name="response">String response of the callback.</param>
        public virtual void OnTradeRequestReply(bool accepted, string response)
        {

        }



        #region Trade events
        // see the various events in SteamTrade.Trade for descriptions of these handlers.

        public abstract void OnTradeError(string error);


        public abstract void OnTradeTimeout();

        public void _OnTradeAwaitingConfirmation(long tradeOfferID)
        {
            OnTradeAwaitingConfirmation(tradeOfferID);
        }
        public abstract void OnTradeAwaitingConfirmation(long tradeOfferID);


        public abstract void OnTradeInit();

        public abstract void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem);

        public abstract void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem);

        public void OnTradeMessageHandler(string message)
        {
            _lastMessageWasFromTrade = true;
            if (!HandleWaitingOnUserResponse(message))
            {
                OnTradeMessage(message);
            }
        }

        public abstract void OnTradeMessage(string message);

        public void OnTradeReadyHandler(bool ready)
        {
            OnTradeReady(ready);
        }

        public abstract void OnTradeReady(bool ready);

        public abstract void OnTradeAcceptHandler();

        public abstract void OnTradeAccept();

        #endregion Trade events
    }
}
