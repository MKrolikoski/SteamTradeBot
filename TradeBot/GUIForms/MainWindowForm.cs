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
    public partial class MainWindowForm : Form
    {
        Bot.BotCore bot = null;
        public MainWindowForm()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (startButton.Text.Equals("Stop")){
                startButton.Text = "Start";
                bot.turnOff();
                bot = null;
            }
            else
            {
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    bot = new Bot.BotCore();
                }).Start();
                
                startButton.Text="Stop";
            }
            
        }

        private void botConfigButton_Click(object sender, EventArgs e)
        {
            Form form = new SteamConfigForm();
            form.ShowDialog();
        }
    }
}
