using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TradeBot.GUIForms
{
    public partial class MainWindowForm : Form, IAppender
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        Bot.BotCore bot = null;
        Thread thread = null;
        public MainWindowForm()
        {
            InitializeComponent();
            bot = new Bot.BotCore();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
           
            if (startButton.Text.Equals("Stop")){
                startButton.Text = "Start";
                bot.stop();
            }
            else
            {
                bot.start();
                thread = new Thread(new ThreadStart(bot.run));
                thread.Start();
                /*new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    bot.start();
                }).Start();*/
                
                startButton.Text="Stop";
            }
            
        }

        private void botConfigButton_Click(object sender, EventArgs e)
        {
            Form form = new SteamConfigForm();
            form.ShowDialog();
        }

        private void bitStampConfigButton_Click(object sender, EventArgs e)
        {
            Form form = new BitStampConfigForm();
            form.ShowDialog();
        }

        public void DoAppend(LoggingEvent loggingEvent)
        {
            logsTextBox.AppendText(loggingEvent.TimeStamp + " " + loggingEvent.MessageObject.ToString() + Environment.NewLine);
        }

        private void sendTextButton_Click(object sender, EventArgs e)
        {
            //log.Info("TEST");
        }
    }
}
