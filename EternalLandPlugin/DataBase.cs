using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EternalLandPlugin.Account;
using TShockAPI;
using TShockAPI.DB;

namespace EternalLandPlugin
{
    class DataBase
    {
        public static QueryResult RunSql(string sql, object[] args = null)
        {
            return args == null ? DbExt.QueryReader(TShock.DB, sql) : DbExt.QueryReader(TShock.DB, sql, args);
        }

        public static bool AddEPlayer(int id, string name)
        {
            Log.Info($"向数据库中添加玩家 {name}");
            if (Utils.GetTSPlayerFuzzy(name, out List<TSPlayer> list))
            {
                EternalLand.EPlayers[list[0].Index] = new EPlayer { ID = id, Name = name };
            }
            return RunSql($"INSERT INTO EternalLand (ID,Name) VALUE (@0,@1)", new object[]
            {
                id,
                name
            }).Read();
        }

        public static async Task<EPlayer> GetEPlayer(int id)
        {
            return await Task.Run(() =>
            {
                var reader = RunSql($"SELECT * FROM EternalLand WHERE ID={id}");
                if (reader.Read())
                {
                    return new EPlayer
                    {
                        ID = reader.Get<int>("ID"),
                        Name = reader.Get<string>("Name"),
                        Money = reader.Get<long>("Money")
                    };
                }
                return null;
            });
        }

        public static async Task SaveEPlayer(EPlayer eplr)
        {
            Task.Run(() =>
            {
                var reader = RunSql($"UPDATE EternalLand SET Money=@1 WHERE ID = @0", new object[]
                  {
                eplr.ID,
                eplr.Money
                  });
            });
        }

        public static async void SaveAllEPlayer()
        {
            await Task.Run(() =>
            {
                EternalLand.EPlayers.ForEach(eplr =>
                {
                    if (eplr != null)
                    {
                        SaveEPlayer(eplr);
                    }
                });
            });
        }

        public static bool UpdateMoney(int id, long money)
        {
            return RunSql($"UPDATE EternalLand SET Money=@0 WHERE ID = @1", new object[]
            {
                money,
                id
            }).Read();
        }
    }
}
