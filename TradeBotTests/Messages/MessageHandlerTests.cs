using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using SteamKit2;

namespace TradeBot.Messages.Tests
{
    [TestClass()]
    public class MessageHandlerTests
    {
        [TestMethod()]
        public void processHelpMessageTest()
        {
            string message = "!help";
            SteamID from;
            Bot.Utils.TrySetSteamID("198662804", out from);
            List<string> parameters = new List<string>();
            Message expectedResponse = new Message(MessageType.HELP, parameters, from);
            MessageHandler messageHandler = new MessageHandler();
            Message actualResponse = messageHandler.parseMessage(message, from);
            Assert.AreEqual(expectedResponse, actualResponse);
        }

        [TestMethod()]
        public void processSellMessageTest()
        {
            string message = "!sell 12";
            SteamID from;
            Bot.Utils.TrySetSteamID("198662804", out from);
            List<string> parameters = new List<string>();
            parameters.Add("12");
            Message expectedResponse = new Message(MessageType.SELL, parameters, from);
            MessageHandler messageHandler = new MessageHandler();
            Message actualResponse = messageHandler.parseMessage(message, from);
            Assert.AreEqual(expectedResponse, actualResponse);
        }
        [TestMethod()]
        public void processBuyMessageTest()
        {
            string message = "!buy 34";
            SteamID from;
            Bot.Utils.TrySetSteamID("198662804", out from);
            List<string> parameters = new List<string>();
            parameters.Add("34");
            Message expectedResponse = new Message(MessageType.BUY, parameters, from);       
            MessageHandler messageHandler = new MessageHandler();
            Message actualResponse = messageHandler.parseMessage(message, from);
            Assert.AreEqual(expectedResponse, actualResponse);
        }
        [TestMethod()]
        public void processChangeWalletAddressMessageTest()
        {
            string message = "!setethaddress 1232190xsa0314";
            SteamID from;
            Bot.Utils.TrySetSteamID("198662804", out from);
            List<string> parameters = new List<string>();
            parameters.Add("1232190xsa0314");
            Message expectedResponse = new Message(MessageType.SETETHADDRESS, parameters, from);
            MessageHandler messageHandler = new MessageHandler();
            Message actualResponse = messageHandler.parseMessage(message, from);
            Assert.AreEqual(expectedResponse, actualResponse);
        }
        [TestMethod()]
        public void processUnknownMessageTest()
        {
            string message = "!unknown command";
            SteamID from;
            Bot.Utils.TrySetSteamID("198662804", out from);
            List<string> parameters = new List<string>();
            Message expectedResponse = new Message(MessageType.UNKNOWN, parameters, from);
            MessageHandler messageHandler = new MessageHandler();
            Message actualResponse = messageHandler.parseMessage(message, from);
            Assert.AreEqual(expectedResponse, actualResponse);
        }

        [TestMethod()]
        public void processDifferentCaseMessageTest()
        {
            string message = "!HeLP 1234";
            SteamID from;
            Bot.Utils.TrySetSteamID("198662804", out from);
            List<string> parameters = new List<string>();
            parameters.Add("1234");
            Message expectedResponse = new Message(MessageType.HELP, parameters, from);
            MessageHandler messageHandler = new MessageHandler();
            Message actualResponse = messageHandler.parseMessage(message, from);
            Assert.AreEqual(expectedResponse, actualResponse);
        }

        [TestMethod()]
        public void getParamsTest()
        {
            string parameters = "12 33 abc ADS";
            string[] paramsArray = { "12", "33", "abc", "ADS" };
            List<string> expectedParameters = new List<string>(paramsArray);
            MessageHandler messageHandler = new MessageHandler();
            List<string> actualParameters = messageHandler.getParams(parameters);
            CollectionAssert.AreEqual(expectedParameters, actualParameters);
        }
    }
}