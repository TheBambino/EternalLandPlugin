using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EternalLandPlugin.Account;
using TShockAPI;
using TShockAPI.DB;

namespace EternalLandPlugin
{
    class DataBase
    {
        static EPlayer GetEPlayerFromReader(QueryResult reader)
        {
            var t = typeof(EPlayer);
            var list = new List<string>();
            var eplr = new EPlayer();
            try {
                for (int i = 0; i < reader.Reader.FieldCount; i++)
                {
                    list.Add(reader.Reader.GetName(i));
                }
                t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).ForEach(p => {
                    var _attrs = p.GetCustomAttributes(typeof(ShouldSave), false);  //反射获得用户自定义属性
                    if (list.Contains(p.Name))
                    {
                        Type type = p.PropertyType;
                        if (type == typeof(string)) p.SetValue(eplr, reader.Get<string>(p.Name));
                        else if (type == typeof(long)) p.SetValue(eplr, reader.Get<long>(p.Name));
                        else if (type == typeof(double)) p.SetValue(eplr, reader.Get<double>(p.Name));
                        else if (type == typeof(int)) p.SetValue(eplr, reader.Get<int>(p.Name));
                        else p.SetValue(eplr, reader.Get<string>(p.Name));
                    }
                });
                t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).ForEach(f =>
                {
                    var _attrs = f.GetCustomAttributes(typeof(ShouldSave), false);  //反射获得用户自定义属性
                    if (list.Contains(f.Name))
                    {
                        Type type = f.FieldType;
                        if (type == typeof(string)) f.SetValue(eplr, reader.Get<string>(f.Name));
                        else if (type == typeof(long)) f.SetValue(eplr, reader.Get<long>(f.Name));
                        else if (type == typeof(double)) f.SetValue(eplr, reader.Get<double>(f.Name));
                        else if (type == typeof(int)) f.SetValue(eplr, reader.Get<int>(f.Name));
                        else f.SetValue(eplr, reader.Get<string>(f.Name));
                    }
                });
            } catch (Exception ex){ Log.Error(ex.Message); }
            return eplr;
        }

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
                if (reader.Read()) return GetEPlayerFromReader(reader);
                return null;
            });
        }

        public static async Task<List<EPlayer>> GetEPlayerFuzzy(string name)
        {
            return await Task.Run(() =>
            {
                List<EPlayer> list = new List<EPlayer>();
                var reader = RunSql($"SELECT * FROM EternalLand WHERE Name LIKE @0", new object[] { "%" + name + "%" });
                while (reader.Read())
                {
                    var eplr = GetEPlayerFromReader(reader);
                    if (EternalLand.OnlineEPlayer.Where(e => e.ID == eplr.ID).Any()) list.Add(Utils.GetEPlayerFromID(eplr.ID));
                    else list.Add(eplr);
                }
                return list;
            });
        }

        public static async Task SaveEPlayer(EPlayer eplr)
        {
            await Task.Run(() =>
            {
                try {
                    string sql = string.Empty;
                    List<object> value = new List<object>() { eplr.ID };
                    int num = 1;
                    var t = eplr.GetType();
                    var properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var _attrs = properties[i].GetCustomAttributes(typeof(ShouldSave), false);  //反射获得用户自定义属性
                        if (_attrs.Where(a => a is ShouldSave).Any())
                        {
                            sql += $"{(num == 1 ? "" : ",")}{properties[i].Name}=@{num}";
                            value.Add(properties[i].GetValue(eplr));
                            num++;
                        }

                    }
                    var field = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    for (int i = 0; i < field.Length; i++)
                    {
                        var _attrs = field[i].GetCustomAttributes(typeof(ShouldSave), false);  //反射获得用户自定义属性
                        if (_attrs.Where(a => a is ShouldSave).Any())
                        {
                            sql += $",{field[i].Name}=@{num}";
                            value.Add(field[i].GetValue(eplr));
                            num++;
                        }
                    }
                    var reader = RunSql($"UPDATE EternalLand SET {sql} WHERE ID = @0", value.ToArray());
                } catch (Exception ex) { Log.Error(ex.Message); }
            });
        }

        public static async void SaveAllEPlayer()
        {
            await Task.Run(() =>
            {
                EternalLand.OnlineEPlayer.ForEach(async eplr => await SaveEPlayer(eplr));
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
