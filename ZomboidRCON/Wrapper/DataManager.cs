using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using ZomboidRCON.Models;

namespace ZomboidRCON.Wrapper
{
    internal class DataManager
    {
        private LiteDatabase database;

        public DataManager(string dbPath)
        {
            database = new LiteDatabase(dbPath + ".zrdb");
            CreateDefault(database);
        }

        public List<Player> Players { get { return database.GetCollection<Player>("players").FindAll().ToList(); } }

        private void CreateDefault(LiteDatabase database)
        {
            if (!database.CollectionExists("Players"))
            {
                var col = database.GetCollection<Player>("players");
                col.EnsureIndex(x => x.Name, true);
            }
        }

        public void AddPlayer(Player player)
        {
            var col = database.GetCollection<Player>("players");
            if (col.Exists(Query.EQ("Name", player.Name)))
            {
                Player p = col.FindOne(x => x.Name.Equals(player.Name));
                p.isOnline = true;
                col.Update(p);
                return;
            }
            col.Insert(player);
        }

        public void AddPlayer(IEnumerable<Player> players)
        {
            var col = database.GetCollection<Player>("players");
            col.InsertBulk(players);
        }

        public void SetAllPlayersOffline()
        {
            var col = database.GetCollection<Player>("players");
            col.UpdateMany(p => new Player { Name = p.Name, isOnline = false, accessLevel = p.accessLevel }, p => p.isOnline == true);
        }
    }
}
