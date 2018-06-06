using log4net;
using log4net.Appender;
using log4net.Core;
using System;
using System.Threading;
using System.Windows.Forms;

namespace TradeBot.GUIForms
{
    public partial class MainWindowForm : Form, IAppender
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly object _lockObj = new object();

        Bot.BotCore bot = null;
        Thread thread = null;
        public MainWindowForm()
        {
            InitializeComponent();
            bot = new Bot.BotCore(Program.UserHandlerCreator);
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
            try
            {
                if (logsTextBox == null)
                    return;
                lock (_lockObj)
                {
                    if (logsTextBox == null)
                        return;


                    var del = new Action<string>(s => logsTextBox.AppendText(s));
                    logsTextBox.BeginInvoke(del, loggingEvent.TimeStamp + " " + loggingEvent.MessageObject.ToString()+"\n");
                }
            }catch
            {

            }
        }

        private void sendTextButton_Click(object sender, EventArgs e)
        {
            //log.Info("TEST");
        }
    }
}
