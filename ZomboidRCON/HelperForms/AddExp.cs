using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZomboidRCON.Models;
using ZomboidRCON.Wrapper;

namespace ZomboidRCON.HelperForms
{
    internal partial class AddExp : Form
    {
        Server server;
        Player player;

        public AddExp(Player player, Server server)
        {
            this.player = player;
            this.server = server;

            InitializeComponent();
            PopulatePerkComboBox();
        }

        private void PopulatePerkComboBox()
        {
            perkComboBox.DataSource = Enum.GetValues(typeof(PerkName));
        }

        private async void btnGiveExp_Click(object sender, EventArgs e)
        {
            try
            {
                int expAmount = (int)numExp.Value;
                PerkName selectedPerk = (PerkName)perkComboBox.SelectedItem; // From ComboBox
                bool success = await server.AddExperienceToPlayer(player, selectedPerk, expAmount);
                if (success)
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error giving item: " + ex.Message, "ZomboidRCON");
            }
        }
    }
}
