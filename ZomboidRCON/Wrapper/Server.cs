using RconSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ZomboidRCON.Models;
using ZomboidRCON.Helpers;

namespace ZomboidRCON.Wrapper
{
    public class Server
    {
        private DataManager dataManager;
        private RconClient client;
        private string host;
        private int port;

        public Func<string, string, Task>? OnMessage { get; set; }

        public Server(RconClient client, string host, int port)
        {
            this.client = client;
            this.host = host;
            this.port = port;
            AppLog.Log("Server", $"Creating DataManager with db={host.Replace(".", "") + port + "_db"}");
            dataManager = new DataManager(host.Replace(".", "") + port + "_db");
            _ = GetPlayers();
        }
        public Server(RconClient client, string host, int port, string dbName)
        {
            this.client = client;
            this.host = host;
            this.port = port;
            string dbFile = dbName.Replace(".", "").Replace(":", "") + port + "_db";
            AppLog.Log("Server", $"Creating DataManager with db={dbFile}");
            dataManager = new DataManager(dbFile);
            _ = GetPlayers();
        }

        private async Task ShowMessage(string message, string title = "ZomboidRCON")
        {
            if (OnMessage != null)
                await OnMessage(message, title);
        }

        public async Task<List<Player>> GetPlayers()
        {
            dataManager.SetAllPlayersOffline();
            List<Player> players = new List<Player>();
            try
            {
                AppLog.Log("Server", "Executing 'players' RCON command...");
                string response = await client.ExecuteCommandAsync("players");
                AppLog.Log("Server", $"Raw players response: '{response}'");
                string[] arr = response.Split('\n');
                AppLog.Log("Server", $"Split into {arr.Length} lines");
                foreach (string item in arr)
                {
                    string trimmed = item.TrimStart();
                    AppLog.Log("Server", $"  Line: '{item}' -> trimmed: '{trimmed}' startsWithDash={trimmed.StartsWith('-')}");
                    if (trimmed.StartsWith('-'))
                    {
                        string user = trimmed.Substring(1).Trim();
                        if (string.IsNullOrWhiteSpace(user)) continue;
                        Player player = new Player
                        {
                            Name = user,
                            isOnline = true,
                            accessLevel = AccessLevel.Unknown,
                        };
                        players.Add(player);
                        dataManager.AddPlayer(player);
                    }
                }
                AppLog.Log("Server", $"Parsed {players.Count} players from RCON response");
            }
            catch (TaskCanceledException)
            {
                AppLog.Log("Server", "GetPlayers: TaskCanceledException");
                await ShowMessage("Unable to execute Fetch players command. Try reconnecting");
            }
            catch (Exception ex)
            {
                AppLog.Log("Server", $"GetPlayers error: {ex}");
                await ShowMessage("Error fetching players: " + ex.Message);
            }

            var dmPlayers = dataManager.Players;
            AppLog.Log("Server", $"Returning {dmPlayers.Count} players from DataManager");
            return dmPlayers;
        }

        public async void DownloadHelp()
        {
            using (StreamWriter writetext = new StreamWriter("commands-help.txt"))
            {
                try
                {
                    string response = await client.ExecuteCommandAsync("help");
                    writetext.WriteLine(response);
                }
                catch (TaskCanceledException)
                {
                    await ShowMessage("Unable to get help. Try reconnecting");
                }
            }
        }

        public async void AddPlayerToWhiteList(Player player)
        {
            if (!player.isOnline)
            {
                await ShowMessage("Player is offline, command cannot be executed");
                return;
            }
            try
            {
                string response = await client.ExecuteCommandAsync("addusertowhitelist " + player.Name);
                await ShowMessage(response);
            }
            catch (TaskCanceledException)
            {
                await ShowMessage("Unable to add player to whitelist. Try reconnecting");
            }
        }

        public async void ChangePlayerGodmodeStatus(Player player, bool enable)
        {
            if (!player.isOnline)
            {
                await ShowMessage("Player is offline, command cannot be executed");
                return;
            }
            try
            {
                string response = await client.ExecuteCommandAsync("godmode " + player.Name + (enable ? " -true" : " -false"));
                player.GodmodeEnabled = enable;
                await ShowMessage(response);
            }
            catch (TaskCanceledException)
            {
                await ShowMessage("Unable change godmod on player. Try reconnecting");
            }
        }

        public async void KickPlayer(Player player)
        {
            if (!player.isOnline)
            {
                await ShowMessage("Player is offline, command cannot be executed");
                return;
            }
            try
            {
                string response = await client.ExecuteCommandAsync("kickuser " + player.Name);
                await ShowMessage(response);
            }
            catch (TaskCanceledException)
            {
                await ShowMessage("Unable to kick player. Try reconnecting");
            }
        }

        public async Task<bool> AddExperienceToPlayer(Player player, PerkName perkName, int xpAmount)
        {
            if (!player.isOnline)
            {
                await ShowMessage("Player is offline, command cannot be executed");
                return false;
            }

            try
            {
                string command = $"addxp \"{player.Name}\" {perkName}={xpAmount}";
                string response = await client.ExecuteCommandAsync(command);
                await ShowMessage(response);
                return true;
            }
            catch (TaskCanceledException)
            {
                await ShowMessage("Unable to add experience to player. Try reconnecting");
                return false;
            }
        }

        public async Task<bool> TeleportToPlayer(Player player, Player toPlayer)
        {
            if (!player.isOnline || !toPlayer.isOnline)
            {
                await ShowMessage("Both players have to be online, command cannot be executed");
                return false;
            }
            try
            {
                string response = await client.ExecuteCommandAsync("teleport " + player.Name + " " + toPlayer.Name);
                await ShowMessage(response);
                return true;
            }
            catch (TaskCanceledException)
            {
                await ShowMessage("Unable to teleport player. Try reconnecting");
                return false;
            }
        }

        public async Task<bool> TeleportPlayerToCoordinates(Player player, int x, int y, int z)
        {
            if (!player.isOnline)
            {
                await ShowMessage("Player is offline, command cannot be executed");
                return false;
            }
            try
            {
                string response = await client.ExecuteCommandAsync("teleportto " + player.Name + " " + x + "," + y + "," + z);
                await ShowMessage(response);
                return true;
            }
            catch (TaskCanceledException)
            {
                await ShowMessage("Unable to teleport player. Try reconnecting");
                return false;
            }
        }

        public async Task<bool> SpawnVehicleForPlayer(Player player, Variant variant)
        {
            if (!player.isOnline)
            {
                await ShowMessage("Player is offline, command cannot be executed");
                return false;
            }
            try
            {
                string response = await client.ExecuteCommandAsync("addvehicle " + variant.VariantID + " " + player.Name);
                await ShowMessage(response);
                return true;
            }
            catch (TaskCanceledException)
            {
                await ShowMessage("Unable to spawn vehicle for player. Try reconnecting");
                return false;
            }
        }

        public async Task<bool> SpawnVehicleForPlayer(Player player, string vehicleID)
        {
            if (!player.isOnline)
            {
                await ShowMessage("Player is offline, command cannot be executed");
                return false;
            }
            try
            {
                string response = await client.ExecuteCommandAsync("addvehicle " + vehicleID + " " + player.Name);
                await ShowMessage(response);
                return true;
            }
            catch (TaskCanceledException)
            {
                await ShowMessage("Unable to spawn vehicle for player. Try reconnecting");
                return false;
            }
        }

        public async Task<bool> GiveItemToPlayer(Player player, string itemID, int count = 1)
        {
            if (!player.isOnline)
            {
                await ShowMessage("Player is offline, command cannot be executed");
                return false;
            }
            try
            {
                string command = $"additem \"{player.Name}\" \"{itemID}\"";
                if (count > 1)
                {
                    command += $" {count}";
                }
                string response = await client.ExecuteCommandAsync(command);
                await ShowMessage(response);
                return true;
            }
            catch (TaskCanceledException)
            {
                await ShowMessage("Unable to give item to player. Try reconnecting");
                return false;
            }
        }

        public async void SetAccessLevel(Player player, AccessLevel accessLevel)
        {
            string access = "none";
            switch (accessLevel)
            {
                case AccessLevel.Observer: access = "observer"; break;
                case AccessLevel.GM: access = "gm"; break;
                case AccessLevel.Overseer: access = "overseer"; break;
                case AccessLevel.Moderator: access = "moderator"; break;
                case AccessLevel.Admin: access = "admin"; break;
                case AccessLevel.None: break;
                case AccessLevel.Unknown: break;
                default: access = "none"; break;
            }
            try
            {
                string response = await client.ExecuteCommandAsync("setaccesslevel " + player.Name + " " + access);
                await ShowMessage(response);
            }
            catch (TaskCanceledException)
            {
                await ShowMessage("Unable to change player's access level. Try reconnecting");
            }
        }

        public async Task<String> ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return "";
            try
            {
                return await client.ExecuteCommandAsync(command);
            }
            catch (TaskCanceledException ece)
            {
                throw ece;
            }
        }

        public async Task<string> ShowOptions()
        {
            try
            {
                return await client.ExecuteCommandAsync("showoptions");
            }
            catch (TaskCanceledException)
            {
                return "Unable to fetch options. Try reconnecting.";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public async Task<string> ReloadOptions()
        {
            try
            {
                return await client.ExecuteCommandAsync("reloadoptions");
            }
            catch (TaskCanceledException)
            {
                return "Unable to reload options. Try reconnecting.";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public void Disconnect()
        {
            client.Disconnect();
        }

        public List<Vehicle> Vehicles { get { return dataManager.Vehicles; } }
        public string Host { get { return host; } }
        public int Port { get { return port; } }

        public async Task BanPlayer(Player player, bool banIP = false, string reason = "")
        {
            if (!player.isOnline)
            {
                await ShowMessage("Player is not online!", "Error");
                return;
            }

            string command = $"banuser \"{player.Name}\"";
            if (banIP) command += " -ip";
            if (!string.IsNullOrWhiteSpace(reason)) command += $" -r \"{reason}\"";

            try
            {
                string response = await ExecuteCommand(command);
                await ShowMessage($"Successfully banned {player.Name}!\nResponse: {response}", "Success");
            }
            catch (Exception ex)
            {
                await ShowMessage($"Failed to ban {player.Name}: {ex.Message}", "Error");
            }
        }
    }

    public enum PerkName
    {
        Fitness,
        Strength,
        Sprinting,
        Lightfoot,
        Nimble,
        Sneak,
        Axe,
        Blunt,
        SmallBlunt,
        LongBlade,
        SmallBlade,
        Spear,
        Maintenance,
        Woodwork,
        Cooking,
        Farming,
        Doctor,
        Electricity,
        MetalWelding,
        Mechanics,
        Tailoring,
        Aiming,
        Reloading,
        Fishing,
        Trapping,
        PlantScavenging,
        Blacksmith,
        Masonry,
        Pottery,
        Carving,
        FlintKnapping,
        Glassmaking,
        Tracking,
        Husbandry,
        Butchering,
        Melting
    }
}
