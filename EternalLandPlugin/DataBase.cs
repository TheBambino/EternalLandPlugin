using EternalLandPlugin.Account;
using EternalLandPlugin.Game;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
            try
            {
                List<EItem> bag = JsonConvert.DeserializeObject<List<EItem>>(reader.Get<string>("Bag"));
                for (int i = 0; i < reader.Reader.FieldCount; i++)
                {
                    list.Add(reader.Reader.GetName(i));
                }
                t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).ForEach(temp =>
                {
                    var attr = temp.GetCustomAttribute(typeof(ShouldSave), false);  //反射获得用户自定义属性
                    if (list.Contains(temp.Name) && temp.Name != "Bag")
                    {
                        Type type = temp.PropertyType;
                        if (type == typeof(string)) temp.SetValue(eplr, reader.Get<string>(temp.Name));
                        else if (type == typeof(long)) temp.SetValue(eplr, reader.Get<long>(temp.Name));
                        else if (type == typeof(double)) temp.SetValue(eplr, reader.Get<double>(temp.Name));
                        else if (type == typeof(int)) temp.SetValue(eplr, reader.Get<int>(temp.Name));
                        else if (type == typeof(Guid)) temp.SetValue(eplr, Guid.Parse(reader.Get<string>(temp.Name)));
                        else if (type == typeof(List<EItem>)) temp.SetValue(eplr, JsonConvert.DeserializeObject<List<EItem>>(reader.Get<string>(temp.Name)));
                        else if (type == typeof(Microsoft.Xna.Framework.Color?)) temp.SetValue(eplr, TShock.Utils.DecodeColor(reader.Get<int>(temp.Name)));
                        else temp.SetValue(eplr, reader.Get<string>(temp.Name));
                    }
                });
                t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).ForEach(temp =>
                {
                    var attr = temp.GetCustomAttribute(typeof(ShouldSave), false);  //反射获得用户自定义属性
                    if (list.Contains(temp.Name) && temp.Name != "Bag")
                    {
                        Type type = temp.FieldType;
                        if (type == typeof(string)) temp.SetValue(eplr, reader.Get<string>(temp.Name));
                        else if (type == typeof(long)) temp.SetValue(eplr, reader.Get<long>(temp.Name));
                        else if (type == typeof(double)) temp.SetValue(eplr, reader.Get<double>(temp.Name));
                        else if (type == typeof(int)) temp.SetValue(eplr, reader.Get<int>(temp.Name));
                        else if (type == typeof(Guid)) temp.SetValue(eplr, Guid.Parse(reader.Get<string>(temp.Name)));
                        else if (type == typeof(List<EItem>)) temp.SetValue(eplr, JsonConvert.DeserializeObject<List<EItem>>(reader.Get<string>(temp.Name)));
                        else if (type == typeof(Microsoft.Xna.Framework.Color?)) temp.SetValue(eplr, TShock.Utils.DecodeColor(reader.Get<int>(temp.Name)));
                        else temp.SetValue(eplr, reader.Get<string>(temp.Name));
                    }
                    if (UserManager.GetTSPlayerFromID(eplr.ID, out var tsp))
                    {
                        eplr.Character = new EPlayerData(tsp) { Bag = bag };
                    }
                });
            }
            catch (Exception ex) { Log.Error((ex.InnerException == null ? ex : ex.InnerException)); }
            return eplr;
        }

        public static QueryResult RunSql(string sql, object[] args = null)
        {
            return args == null ? DbExt.QueryReader(TShock.DB, sql) : DbExt.QueryReader(TShock.DB, sql, args);
        }

        public static bool AddEPlayer(int id, string name)
        {
            Log.Info($"向数据库中添加玩家 {name}");
            if (UserManager.GetTSPlayerFuzzy(name, out List<TSPlayer> list))
            {
                EternalLand.EPlayers[list[0].Index] = new EPlayer() { ID = id, Name = name };
            }
            var bag = new List<EItem>();
            for (int i = 0; i < 260; i++)
            {
                bag.Add(new EItem());
            }
            return RunSql($"INSERT INTO EternalLand (ID,Name,Bag) VALUE (@0,@1,@2)", new object[]
            {
                id,
                name,
                JsonConvert.SerializeObject(bag)
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
                    if (EternalLand.OnlineEPlayer.Where(e => e.ID == eplr.ID).Any()) list.Add(UserManager.GetEPlayerFromID(eplr.ID));
                    else list.Add(eplr);
                }
                return list;
            });
        }

        public static async Task SaveEPlayer(EPlayer eplr)
        {
            await Task.Run(() =>
            {
                try
                {
                    string sql = string.Empty;
                    List<object> value = new List<object>() { eplr.ID };
                    int num = 1;
                    var t = eplr.GetType();
                    var properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var attr = properties[i].GetCustomAttribute(typeof(ShouldSave), false);  //反射获得用户自定义属性
                        if (attr != null)
                        {
                            var a = (ShouldSave)attr;
                            sql += $"{(num == 1 ? "" : ",")}{properties[i].Name}=@{num}";
                            value.Add(a.Serializable ? JsonConvert.SerializeObject(properties[i].GetValue(eplr)) : properties[i].GetValue(eplr));
                            num++;
                        }
                    }
                    var field = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    for (int i = 0; i < field.Length; i++)
                    {
                        var attr = field[i].GetCustomAttribute(typeof(ShouldSave), false);  //反射获得用户自定义属性
                        if (attr != null)
                        {
                            var a = (ShouldSave)attr;
                            sql += $",{field[i].Name}=@{num}";
                            value.Add(a.Serializable ? JsonConvert.SerializeObject(field[i].GetValue(eplr)) : field[i].GetValue(eplr));
                            num++;
                        }
                    }
                    var reader = RunSql($"UPDATE EternalLand SET {sql} WHERE ID = @0", value.ToArray());
                }
                catch (Exception ex) { Log.Error(ex.InnerException == null ? ex : ex.InnerException); }
            });
        }

        public static async void SaveAllEPlayer()
        {
            await Task.Run(() =>
            {
                EternalLand.OnlineEPlayer.ForEach(async eplr => await SaveEPlayer(eplr));
            });
        }

        public static async void SaveCharacter(EPlayerData data)
        {
            await Task.Run(() =>
            {
                string sql = string.Empty;
                try
                {
                    List<object> value = new List<object>();
                    int num = 0;
                    var t = typeof(EPlayerData);
                    var field = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    for (int i = 0; i < field.Length; i++)
                    {
                        sql += $"{(num == 0 ? "" : ",")}{field[i].Name}=@{num}";
                        if (field[i].FieldType == typeof(Microsoft.Xna.Framework.Color?)) value.Add(TShock.Utils.EncodeColor((Microsoft.Xna.Framework.Color?)field[i].GetValue(data)));
                        else if (field[i].FieldType == typeof(List<EItem>)) value.Add(JsonConvert.SerializeObject(field[i].GetValue(data)));
                        else if (field[i].FieldType == typeof(bool[])) value.Add(JsonConvert.SerializeObject(field[i].GetValue(data)));
                        else value.Add(field[i].GetValue(data));
                        num++;
                    }
                    var reader = RunSql($"REPLACE INTO EternalLandData SET {sql}", value.ToArray());
                }
                catch (Exception ex) { Log.Error((ex.InnerException == null ? ex : ex.InnerException) + $"\nSQL语句为 {sql}."); }
            });
        }

        public static async void GetAllCharacter()
        {
            await Task.Run(() =>
            {
                var reader = RunSql($"SELECT * FROM EternalLandData");
                while (reader.Read())
                {
                    var t = typeof(EPlayerData);
                    var list = new List<string>();
                    var data = new EPlayerData();
                    try
                    {
                        for (int i = 0; i < reader.Reader.FieldCount; i++)
                        {
                            list.Add(reader.Reader.GetName(i));
                        }
                        t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).ForEach(temp =>
                        {
                            var attr = temp.GetCustomAttribute(typeof(ShouldSave), false);  //反射获得用户自定义属性
                            if (list.Contains(temp.Name))
                            {
                                Type type = temp.FieldType;
                                if (type == typeof(string)) temp.SetValue(data, reader.Get<string>(temp.Name));
                                else if (type == typeof(int)) temp.SetValue(data, reader.Get<int>(temp.Name));
                                else if (type == typeof(List<EItem>)) temp.SetValue(data, JsonConvert.DeserializeObject<List<EItem>>(reader.Get<string>(temp.Name)));
                                else if (type == typeof(Microsoft.Xna.Framework.Color?)) temp.SetValue(data, TShock.Utils.DecodeColor(reader.Get<int>(temp.Name)));
                                else if (type == typeof(bool[])) temp.SetValue(data, JsonConvert.DeserializeObject<bool[]>(reader.Get<string>(temp.Name)));
                                else temp.SetValue(data, reader.Get<string>(temp.Name));
                            }
                        });
                    }
                    catch (Exception ex) { Log.Error(ex.InnerException == null ? ex : ex.InnerException); }
                    Game.GameData.Character.Add(data.Name, data);
                }
                Log.Info($"共载入 {GameData.Character.Count} 条角色数据.");
            });
        }

        public static async void SaveMap(string name, MapManager.MapData data)
        {
            await Task.Run(() =>
            {
                try
                {
                    lock (data)
                    {
                        System.Diagnostics.Stopwatch alltime = new System.Diagnostics.Stopwatch();
                        alltime.Start();
                        byte[] map = MessagePackSerializer.Serialize(value: data, MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block));
                        alltime.Stop();
                        Log.Info($"序列化地图 {name} 耗时 {alltime.ElapsedMilliseconds} ms, 文件大小为 {Math.Round(((double)map.Length) / 1024 / 1024, 4)} MB, 地图大小为 {data.Width} * {data.Height}.\n开始上传至数据库.");
                        IDbConnection dbConnection = TShock.DB.CloneEx();
                        QueryResult result;
                        try
                        {
                            dbConnection.Open();
                            using (IDbCommand dbCommand = dbConnection.CreateCommand())
                            {
                                var args = new object[] { name, map, map.Length };
                                dbCommand.CommandText = $"REPLACE INTO EternalLandMap SET Name=@0,Data=@1,Length=@2";
                                for (int i = 0; i < args.Length; i++)
                                {
                                    dbCommand.AddParameter("@" + i.ToString(), args[i]);
                                }
                                dbCommand.CommandTimeout = 60000;
                                result = new QueryResult(dbConnection, dbCommand.ExecuteReader());
                                Log.Info($"地图 {name} 上传完成.");
                                if (!GameData.Map.ContainsKey(name)) GameData.Map.Add(name, map);
                                else GameData.Map[name] = map;
                                return;
                            }
                        }
                        catch (Exception innerException)
                        {
                            Log.Error("SQL异常.\n" + innerException);
                            return;
                        }
                    }
                }
                catch (Exception ex) { Log.Error(ex.InnerException == null ? ex : ex.InnerException); }
            });
        }
        public static async void GetAllMap()
        {
            await Task.Run(() =>
            {
                Log.Info($"正在读入地图.");
                var reader = RunSql($"SELECT * FROM EternalLandMap");
                //BinaryFormatter formatter = new BinaryFormatter();
                Log.Info($"开始反序列化地图.");
                System.Diagnostics.Stopwatch alltime = new System.Diagnostics.Stopwatch();
                alltime.Start();
                while (reader.Read())
                {
                    string name = reader.Get<string>("Name");
                    try
                    {
                        int FileSize = reader.Reader.GetInt32(reader.Reader.GetOrdinal("Length"));
                        var rawData = new byte[FileSize];
                        reader.Reader.GetBytes(reader.Reader.GetOrdinal("Data"), 0, rawData, 0, FileSize);
                        GameData.Map.Add(name, rawData);
                    }
                    catch (Exception ex) { Log.Error(ex.InnerException == null ? ex : ex.InnerException); continue; }
                }
                alltime.Stop();
                Log.Info($"共载入 {GameData.Map.Count} 条地图数据, 共耗时 {alltime.ElapsedMilliseconds} ms.");
            });
        }
        public static bool UpdateMoney(int id, long money)
        {
            return RunSql($"UPDATE EternalLand SET Money=@0 WHERE ID=@1", new object[]
            {
                money,
                id
            }).Read();
        }
    }
}
