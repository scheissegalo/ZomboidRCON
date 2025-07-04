using Microsoft.VisualBasic;
using RconSharp;
using System.Diagnostics;
using ZomboidRCON.Models;
using ZomboidRCON.UpdateSystem;
using ZomboidRCON.Wrapper;

namespace ZomboidRCON
{
    public partial class Main : Form
    {
        private Server server;
        private Updator updator;
        private bool isConnected = false;
        private int refreshRate = 30000;

        public Main(RconClient clientConnection, string host, int port)
        {
            InitializeComponent();
            server = new Server(clientConnection, host, port);
        }
        public Main()
        {
            InitializeComponent();
            cmbRefreshRate.SelectedIndex = 1;
        }

        private async void RefreshLoop()
        {
            while (isConnected)
            {
                RefreshPlayers();
                await Task.Delay(refreshRate);
            }
        }

        private async void RefreshPlayers()
        {
            refreshBtn.Enabled = false;
            refreshBtn.Cursor = Cursors.WaitCursor;
            playersView.Items.Clear();
            List<Player> players = await server.GetPlayers();
            Debug.WriteLine(players.Count);
            foreach (Player player in players)
            {

                ListViewItem item = playersView.Items.Add(player.Name);
                item.ToolTipText = "Last known access level: " + player.accessLevel.ToString();
                item.SubItems.Add(player.accessLevel.ToString());
                item.SubItems.Add(player.GodmodeEnabled.ToString());
                if (player.isOnline)
                    item.Group = playersView.Groups[0];
                else item.Group = playersView.Groups[1];
            }
            refreshBtn.Enabled = true;
            refreshBtn.Cursor = Cursors.Hand;
        }

        public void ResetConnection(RconClient clientConnection, string host, int port, string dbname)
        {
            clientConnection.ConnectionClosed += ClientConnection_ConnectionClosed;
            server = new Server(clientConnection, host, port, dbname);
            isConnected = true;
            //RefreshPlayers();
            RefreshLoop();
            //server.DownloadHelp();
        }

        private void ClientConnection_ConnectionClosed()
        {
            isConnected = false;
            MessageBox.Show(this, "Connection Closed!", "ZomboidRCON");
        }

        private void Main_Load(object sender, EventArgs e)
        {
            UpdateCheck();
            //List<PlayerPerks> pp = server.PlayerPerkList();
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            if (server == null) Hide();
            ConnectionForm ConnectionForm = new ConnectionForm(this);
            ConnectionForm.Show();
        }

        private void creditsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new InfoForms.Credits().Show();
        }

        private void accessLevelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new InfoForms.AccessLevelInfoForm().Show();
        }

        private void reportIssueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ps = new ProcessStartInfo("https://github.com/kwmx/ZomboidRCON/issues/new")
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }

        private void playerMenuStrip_Opened(object sender, EventArgs e)
        {

        }

        private void playerMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (playersView.SelectedItems.Count <= 0)
            {
                e.Cancel = true;
                playerMenuStrip.Hide();
                playerMenuStrip.Visible = false;
            }
        }
        private async Task<Player> GetPlayerByName(string name)
        {
            List<Player> players = await server.GetPlayers();
            foreach (Player player in players)
            {
                if (player.Name == name) return player;
            }
            return new Player { Name = name };
        }
        private async void addToWhitelistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = playersView.SelectedItems[0].Text;

            server.AddPlayerToWhiteList(await GetPlayerByName(name));
            RefreshPlayers();
        }

        private async void kickPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = playersView.SelectedItems[0].Text;
            server.KickPlayer(await GetPlayerByName(name));
            RefreshPlayers();
        }

        private void refreshBtn_Click(object sender, EventArgs e)
        {
            RefreshPlayers();
        }

        private async void noneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = playersView.SelectedItems[0].Text;
            server.SetAccessLevel(await GetPlayerByName(name), AccessLevel.None);
            RefreshPlayers();
        }

        private async void overseerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = playersView.SelectedItems[0].Text;
            server.SetAccessLevel(await GetPlayerByName(name), AccessLevel.Overseer);
            RefreshPlayers();
        }

        private async void gMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = playersView.SelectedItems[0].Text;
            server.SetAccessLevel(await GetPlayerByName(name), AccessLevel.GM);
            RefreshPlayers();
        }

        private async void moderatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = playersView.SelectedItems[0].Text;
            server.SetAccessLevel(await GetPlayerByName(name), AccessLevel.Moderator);
            RefreshPlayers();
        }

        private async void adminToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = playersView.SelectedItems[0].Text;
            server.SetAccessLevel(await GetPlayerByName(name), AccessLevel.Admin);
            RefreshPlayers();
        }

        private void commandConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperForms.CommandConsole cs = new HelperForms.CommandConsole(server);
            cs.Show();
        }

        private async void teleportToPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private async void enableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = playersView.SelectedItems[0].Text;
            server.ChangePlayerGodmodeStatus(await GetPlayerByName(name), true);
            RefreshPlayers();
        }

        private async void disableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = playersView.SelectedItems[0].Text;
            server.ChangePlayerGodmodeStatus(await GetPlayerByName(name), false);
            RefreshPlayers();
        }

        private async void spawnVehicleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Player player = await GetPlayerByName(playersView.SelectedItems[0].Text);
            HelperForms.VehicleSpawnMenu vsm = new HelperForms.VehicleSpawnMenu(player, server);
            vsm.Show();
        }

        private async void spawnItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Player player = await GetPlayerByName(playersView.SelectedItems[0].Text);
            HelperForms.ItemSpawnMenu ism = new HelperForms.ItemSpawnMenu(player, server);
            ism.Show();
        }

        private async void banPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Player player = await GetPlayerByName(playersView.SelectedItems[0].Text);
            //HelperForms.BanPlayerForm bpf = new HelperForms.BanPlayerForm(player, server);
            //bpf.Show();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (server != null)
                    server.Disconnect();
            }
            catch (Exception ex)
            {

            }
            Application.Exit();
        }

        private async void toPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Player player = await GetPlayerByName(playersView.SelectedItems[0].Text);
            HelperForms.TeleportToPlayer teleport = new HelperForms.TeleportToPlayer(player, server);
            teleport.Show();
        }

        private async void selectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Player player = await GetPlayerByName(playersView.SelectedItems[0].Text);
            HelperForms.TeleportToCoordinates teleport = new HelperForms.TeleportToCoordinates(player, server);
            teleport.Show();
        }

        private void openMapWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ps = new ProcessStartInfo("https://map.projectzomboid.com/#collapseThree")
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }
        private async void UpdateCheck()
        {
            if (updator == null)
            {
                updator = new Updator(Constants.RepoPath, Constants.AssemblyVersionToSemver, true, "ZomboidRCON.zip", "");
            }
            UpdateResult updateResult = await updator.CheckForUpdate();
            if (updateResult.UpdateStatus == UpdateStatus.UpdateNeeded)
            {
                UpdateForm updateForm = new UpdateForm(updator, updateResult.Release);
                updateForm.ShowDialog(this);
            }
        }

        private async void addExp_Click(object sender, EventArgs e)
        {
            Player player = await GetPlayerByName(playersView.SelectedItems[0].Text);
            HelperForms.AddExp ism = new HelperForms.AddExp(player, server);
            ism.Show();
            //await server.AddExperienceToPlayer(selectedPlayer, selectedPerk, xpAmount);
            //List<PlayerPerks> pp = server.PlayerPerkList();
        }

        private void mapMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void cmbRefreshRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbRefreshRate.SelectedIndex)
            {
                case 0:
                    refreshRate = 5000;
                    break;
                case 1:
                    refreshRate = 30000;
                    break;
                case 2:
                    refreshRate = 60000;
                    break;
            }
        }
    }
}