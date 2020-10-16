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

        public static bool AddEPlayer(int id)
        {
            return RunSql($"INSERT INTO EternalLand (ID) VALUE (@0)", new object[]
            {
                id,
            }).Read();
        }

        public static EPlayer GetEPlayer(int id)
        {
            var reader = RunSql($"SELECT * FROM EternalLand WHERE ID={id}");
            if (reader.Read())
            {
                return new EPlayer
                {
                    ID = reader.Get<int>("ID"),
                    Money = reader.Get<long>("Money")
                };
            }
            return null;
        }

        public static bool SaveEPlayer(EPlayer eplr)
        {
            var reader = RunSql($"UPDATE EternalLand SET Money=@1 WHERE ID = @0", new object[]
            {
                eplr.ID,
                eplr.Money
            });
            if (reader.Read()) return true;
            return false;
        }

        public static void SaveAllEPlayer()
        {
            EternalLand.EPlayers.ForEach(eplr =>
            {
                if (eplr != null)
                {
                    SaveEPlayer(eplr);
                }
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
