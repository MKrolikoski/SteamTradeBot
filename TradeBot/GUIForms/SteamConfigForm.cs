using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TradeBot.Bot;

namespace TradeBot.GUIForms
{
    public partial class SteamConfigForm : Form
    {
        private BotConfig config = new BotConfig();

        public SteamConfigForm()
        {
            InitializeComponent();
            setDataInGrid();
        }


        private void setDataInGrid()
        {
            if (!File.Exists("config.cfg"))
            {
                config.createNew();
                if (File.Exists("sentry.bin"))
                    File.Delete("sentry.bin");
            }
            string fileContent = File.ReadAllText("config.cfg");
            config = JsonConvert.DeserializeObject<BotConfig>(fileContent);

            Dictionary<string, string> configDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContent);
            
           
            foreach (KeyValuePair<string, string> element in configDictionary)
            {
                string[] row = { element.Key, element.Value };
                configDataGridView.Rows.Add(row);
            }
        }

        private void saveConfigButton_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dgv in configDataGridView.Rows )
            {
                switch (dgv.Cells[0].Value)
                {
                    case "working":
                        config.working = ((string)dgv.Cells[1].Value).Equals("true") ? true : false;
                        break;
                    case "status":
                        BotStatus status;
                        Enum.TryParse((string)dgv.Cells[1].Value, out status);
                        config.status = status;
                        break;
                    case "login":
                        config.login = (string)dgv.Cells[1].Value;
                        break;
                    case "password":
                        config.password = (string)dgv.Cells[1].Value;
                        break;
                    case "shared_secret":
                        config.shared_secret = (string)dgv.Cells[1].Value;
                        break;
                    case "api_key":
                        config.api_key = (string)dgv.Cells[1].Value;
                        break;
                    case "buy_price":
                        config.buy_price = double.Parse((string)dgv.Cells[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case "sell_price":
                        config.sell_price = double.Parse((string)dgv.Cells[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case "transaction_toll":
                        config.transaction_toll = double.Parse((string)dgv.Cells[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                        break;
                }
            
            }
            try
            {
                //for test disabled
                config.exportTo("E:\\tmp.txt");
                //config.save();
                System.Windows.Forms.MessageBox.Show("Configuartion saved!");
            } catch (IOException exception)
            {
                System.Windows.Forms.MessageBox.Show("Error! Configuration can not be saved!");
            }
        }

        private void restoreButton_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to clear configuration file?", "Are you sure?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                //For tests disabled
                //config.createNew();
            }
            else if (dialogResult == DialogResult.No)
            {
                
            }
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Configuration file .cfg|*.cfg|Text file .txt|*.txt";
            saveFileDialog.Title = "Save a configuration file";
            saveFileDialog.ShowDialog();

            if(saveFileDialog.FileName != "")
            {
                try
                {
                    config.exportTo(Path.GetFullPath(saveFileDialog.FileName));
                } catch (IOException)
                {
                    System.Windows.Forms.MessageBox.Show("Error! Configuration can not be saved!");
                }
            }
        }

        private void SteamConfigForm_Load(object sender, EventArgs e)
        {

        }
    }
}
