using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZomboidRCON.Models;
using ZomboidRCON.Wrapper;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ZomboidRCON.HelperForms
{
    internal partial class ItemSpawnMenu : Form
    {
        private Player player;
        private Server server;

        private class ItemInfo
        {
            public string Name { get; set; }
            public string ID { get; set; }

            public override string ToString()
            {
                return $"{Name} ({ID})";
            }
        }

        private List<ItemInfo> items;
        public string SelectedItemID { get; private set; }

        public ItemSpawnMenu(Player player, Server server)
        {
            InitializeComponent();
            this.player = player;
            this.server = server;
            LoadItems();
            SetupComboBox();
        }

        private void LoadItems()
        {
            items = new List<ItemInfo>();
            string[] lines = File.ReadAllLines("pz_items.csv");

            // Skip header row
            for (int i = 1; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(',');
                if (parts.Length == 2)
                {
                    items.Add(new ItemInfo
                    {
                        Name = parts[0],
                        ID = parts[1]
                    });
                }
            }
        }

        private void SetupComboBox()
        {
            comboBox1.DataSource = items;
            comboBox1.DisplayMember = "Name";
            comboBox1.ValueMember = "ID";
        }

        private void ItemSpawnMenu_Load(object sender, EventArgs e)
        {
            Text = "Give item to '" + player.Name + "'";
            spawnBtn.Enabled = false;
        }

        private void itemIDTxt_TextChanged(object sender, EventArgs e)
        {
            spawnBtn.Enabled = !string.IsNullOrWhiteSpace(itemIDTxt.Text);
        }

        private async void spawnBtn_Click(object sender, EventArgs e)
        {
            spawnBtn.Enabled = false;
            itemIDTxt.Enabled = false;
            countNumeric.Enabled = false;

            try
            {
                int count = (int)countNumeric.Value;
                bool success = await server.GiveItemToPlayer(player, itemIDTxt.Text, count);
                if (success)
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error giving item: " + ex.Message, "ZomboidRCON");
            }

            spawnBtn.Enabled = true;
            itemIDTxt.Enabled = true;
            countNumeric.Enabled = true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = comboBox1.SelectedItem as ItemInfo;
            if (selectedItem != null)
            {
                itemIDTxt.Text = selectedItem.ID;
            }
            else
            {
                //e.Cancel = true;
                MessageBox.Show("Please select a valid item.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //itemIDTxt.Text = 
        }
    }
} 