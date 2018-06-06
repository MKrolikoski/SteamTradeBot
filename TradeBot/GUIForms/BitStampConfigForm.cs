using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TradeBot.Bitstamp;

namespace TradeBot.GUIForms
{
    public partial class BitStampConfigForm : Form
    {
        BitstampConfig config = new BitstampConfig();
        public BitStampConfigForm()
        {
            InitializeComponent();
            setDataInGrid();
        }




    private void setDataInGrid()
        {
            if (!File.Exists("bitstamp_config.cfg"))
            {
                config.createNew();
            }
            string fileContent = File.ReadAllText("bitstamp_config.cfg");
            config = JsonConvert.DeserializeObject<BitstampConfig>(fileContent);

            Dictionary<string, string> configDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContent);


            foreach (KeyValuePair<string, string> element in configDictionary)
            {
                string[] row = { element.Key, element.Value };
                configDataGridView.Rows.Add(row);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void BitStampConfigForm_Load(object sender, EventArgs e)
        {

        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dgv in configDataGridView.Rows)
            {
                switch (dgv.Cells[0].Value)
                {
                    case "api_key":
                        config.api_key = (string)dgv.Cells[1].Value;
                        break;
                    case "api_secret":
                        config.api_secret = (string)dgv.Cells[1].Value;
                        break;
                    case "customer_id":
                        config.customer_id = (string)dgv.Cells[1].Value;
                        break;
                    case "eth_address":
                        config.eth_address = (string)dgv.Cells[1].Value;
                        break;
                }
            }
            try
            {
                //for tests disabled
                config.exportTo("E:\\tmp.txt");
                //config.save();
                System.Windows.Forms.MessageBox.Show("Configuartion saved!");
            }
            catch (IOException exception)
            {
                System.Windows.Forms.MessageBox.Show("Error! Configuration can not be saved!");
            }
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Configuration file .cfg|*.cfg|Text file .txt|*.txt";
            saveFileDialog.Title = "Save a configuration file";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                try
                {
                    config.exportTo(Path.GetFullPath(saveFileDialog.FileName));
                }
                catch (IOException)
                {
                    System.Windows.Forms.MessageBox.Show("Error! Configuration can not be saved!");
                }
            }
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to clear configuration file?", "Are you sure?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                //For test disabled
                //config.createNew();
            }
            else if (dialogResult == DialogResult.No)
            {

            }
        }
    }
}
