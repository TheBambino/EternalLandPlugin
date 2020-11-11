using EternalLandPlugin.Account;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace EternalLandPlugin.Game
{
    class GameCommand
    {
        public static void OnPlayerCommand(TShockAPI.Hooks.PlayerCommandEventArgs args)
        {
            var eplr = args.Player.EPlayer();
            var cmd = args.Parameters;
            if (eplr != null)
            {
                switch (args.CommandName.ToLower())
                {
                    case "home":
                        args.Handled = true;
                        args.Player.Teleport((float)(Main.spawnTileX * 16), (float)(Main.spawnTileY * 16 - 48), 1);
                        break;
                    case "tp":
                        if (cmd.Count >= 1)
                        {
                            var temptp = TSPlayer.FindByNameOrID(cmd[0]);
                            if (temptp.Count > 1)
                            {
                                eplr.tsp.SendMultipleError(temptp);
                                args.Handled = true;
                                return;
                            }
                            else if (temptp.Any() && temptp[0].EPlayer() != null && eplr.MapUUID != temptp[0].EPlayer().MapUUID)
                            {
                                eplr.SendErrorEX($"你无法直接传送至位于其他世界的玩家.");
                                args.Handled = true;
                            }
                        }

                        break;
                }
            }
        }

        public async static void AdminCommand(CommandArgs args)
        {
            var tsp = args.Player;
            var eplr = tsp.EPlayer();
            if (eplr == null)
            {
                tsp.SendErrorEX("你尚未登录.");
                return;
            }
            string error = "命令無效. ";
            var cmd = args.Parameters;
            switch (args.Message.Split(' ')[0].ToLower())
            {
                case "/char":
                    if (args.Parameters.Count >= 1)
                    {
                        switch (cmd[0].ToLower())
                        {
                            case "save":
                                if (cmd.Count < 2)
                                {
                                    tsp.SendErrorEX(error);
                                    break;
                                }
                                else if (GameData.Character.ContainsKey(cmd[1]))
                                {
                                    tsp.SendErrorEX("角色已存在. 如想更新角色信息请使用//updatecharacter <名称>");
                                    break;
                                }
                                var data = new EPlayerData(tsp, cmd[1]);
                                DataBase.SaveCharacter(data);
                                GameData.Character.Add(cmd[1], data);
                                tsp.SendSuccessEX("执行完成.");
                                break;
                            case "update":
                                if (cmd.Count < 2)
                                {
                                    tsp.SendErrorEX(error);
                                    break;
                                }
                                else if (!GameData.Character.ContainsKey(cmd[1]))
                                {
                                    tsp.SendErrorEX($"未找到角色存档: {cmd[1]}");
                                    break;
                                }
                                data = new EPlayerData(tsp, cmd[1]);
                                DataBase.SaveCharacter(data);
                                GameData.Character[cmd[1]] = data;
                                tsp.SendSuccessEX("已更新角色数据.");
                                break;
                            case "turnto":
                                if (cmd.Count < 2)
                                {
                                    tsp.SendErrorEX(error);
                                    break;
                                }
                                else if (!GameData.Character.ContainsKey(cmd[1]))
                                {
                                    tsp.SendErrorEX($"未找到角色存档: {cmd[1]}");
                                    break;
                                }
                                eplr.ChangeCharacter(cmd[1]);
                                tsp.SendSuccessEX("执行完成.");
                                break;
                            case "turnback":
                                eplr.SetToOriginCharacter();
                                tsp.SendSuccessEX("执行完成.");
                                break;

                        }
                    }
                    break;
                case "/map":
                    if (args.Parameters.Count >= 1)
                    {
                        switch (cmd[0].ToLower())
                        {
                            case "set":
                                if (eplr.SettingPoint == 1 || eplr.SettingPoint == 2)
                                {
                                    eplr.SendEX($"你已在进行区域{(eplr.SettingPoint == 1 ? "左上" : "右下")}角选定.");
                                }
                                else
                                {
                                    eplr.SendEX($"开始进行地图区域选定. 请用任何镐子点击左上角物块.");
                                    eplr.SettingPoint = 1;
                                }
                                break;
                            case "create":
                                if (cmd.Count < 2)
                                {
                                    tsp.SendErrorEX(error);
                                    break;
                                }
                                else if (cmd[1] == "MainWorld")
                                {
                                    var d = new MapManager.MapData(0, 0, Main.maxTilesX, Main.maxTilesY, cmd[1]);
                                    if (DataBase.SaveMap(cmd[1], d).Result) tsp.SendSuccessEX("执行完成.");
                                    return;
                                }
                                else if (eplr.SettingPoint != 0)
                                {
                                    tsp.SendErrorEX($"你正在选择第 {eplr.SettingPoint} 个点位, 无法创建地图.");
                                    break;
                                }
                                else if (eplr.ChoosePoint[0] == null || eplr.ChoosePoint[1] == null)
                                {
                                    tsp.SendErrorEX($"点位选择不完整, 请重新选择.");
                                    break;
                                }
                                else if (GameData.Map.ContainsKey(cmd[1]))
                                {
                                    tsp.SendErrorEX($"此地图名已存在. 如想更新地图请使用 {"//map update".ToColorful()}.");
                                    break;
                                }
                                var data = new MapManager.MapData(eplr.ChoosePoint[0], eplr.ChoosePoint[1], cmd[1]);
                                if (DataBase.SaveMap(cmd[1], data).Result)
                                {
                                    tsp.SendSuccessEX("执行完成.");
                                }
                                else
                                {
                                    tsp.SendErrorEX($"发生错误, 详细信息请查看控制台.");
                                }
                                break;
                            case "goto":
                                if (cmd.Count < 4)
                                {
                                    tsp.SendErrorEX(error + ", 需包含生成到的坐标");
                                    break;
                                }
                                else if (!GameData.Map.ContainsKey(cmd[1]))
                                {
                                    tsp.SendErrorEX($"未找到地图: {cmd[1]}");
                                    break;
                                }
                                eplr.JoinMap(await MapManager.CreateMultiPlayerMap(cmd[1], int.Parse(cmd[2]), int.Parse(cmd[3])));
                                tsp.SendSuccessEX("执行完成.");
                                break;
                            case "wld":
                                if (cmd.Count > 2) eplr.JoinMap(MapManager.CreateMultiPlayerMap(new MapManager.MapData(2152, 394, int.Parse(cmd[1]), int.Parse(cmd[2])), 4100, 400));
                                else eplr.JoinMap(MapManager.CreateMultiPlayerMap(new MapManager.MapData(int.Parse(cmd[1]), int.Parse(cmd[2]), 200, 200), 4100, 450));
                                break;
                            case "clear":
                                eplr.JoinMap(MapManager.CreateMultiPlayerMap(new MapManager.MapData(), 4100, 400));
                                break;
                            case "back":
                                eplr.BackToOriginMap();
                                break;
                            case "join":
                                if (UserManager.TryGetEPlayeFuzzy(cmd[1], out var t))
                                {
                                    eplr.JoinMap(t[0].MapUUID);

                                }
                                break;
                        }
                    }
                    break;
            }
        }
        public static void Territory(CommandArgs args)
        {
            var tsp = args.Player;
            var eplr = tsp.EPlayer();
            string error = "命令無效. ";
            var cmd = args.Parameters;
            if (eplr == null)
            {
                tsp.SendErrorEX("你尚未登录.");
                //return;
            }
            else if (cmd.Count < 1)
            {
                tsp.SendErrorEX(error + "请输入/territory(属地, sd) help 查看命令");
                return;
            }
            
            switch (cmd[0].ToLower())
            {
                case "create":

                    break;
                case "1":
                    try {
                        var az = new FakeTileProvider(10, 10);
                        for (int y = 0; y < 10; y++)
                        {
                            for (int x = 0; x < 10; x++)
                            {
                                az[x, y] = new Tile() { type = 20 };
                            }
                        }
                        var za = MessagePackSerializer.Deserialize<FakeTileProvider>(MessagePackSerializer.Serialize(az, MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block)), MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block));
                    }
                    catch (Exception ex) { Utils.Broadcast(ex);
                        Console.WriteLine(ex);
                    }
                    
                    
                    break;
            }
        }
    }
}
