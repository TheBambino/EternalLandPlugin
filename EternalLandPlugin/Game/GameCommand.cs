using EternalLandPlugin.Account;
using MessagePack;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
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
                                else if (cmd[1] == "MainWorld" || cmd[1] == "DefaultTerritory")
                                {
                                    var d = new MapManager.MapData(0, 0, Main.maxTilesX, Main.maxTilesY, cmd[1]);
                                    DataBase.SaveMap(cmd[1], d);
                                    tsp.SendSuccessEX("执行完成.");
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
                                DataBase.SaveMap(cmd[1], data);
                                tsp.SendSuccessEX("执行完成.");
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
                                eplr.JoinMap(MapManager.CreateMultiPlayerMap(cmd[1], int.Parse(cmd[2]), int.Parse(cmd[3])));
                                tsp.SendSuccessEX("执行完成.");
                                break;
                            case "update":
                                if (eplr.IsInAnotherWorld)
                                {
                                    eplr.Map.Save();
                                    eplr.SendSuccessEX("执行完成.");
                                }
                                else
                                {
                                    eplr.SendErrorEX("未处于其他地图.");
                                }
                                break;
                            case "setspawn":
                                if (eplr.IsInAnotherWorld)
                                {
                                    eplr.Map.SetSpawn(eplr.TileX, eplr.TileY);
                                    eplr.Map.Save();
                                    eplr.SendSuccessEX("执行完成.");
                                }
                                else
                                {
                                    eplr.SendErrorEX("未处于其他地图.");
                                }
                                break;
                            /*case "wld":
                                if (cmd.Count > 2) eplr.JoinMap(MapManager.CreateMultiPlayerMap(new MapManager.MapData(2152, 394, int.Parse(cmd[1]), int.Parse(cmd[2])), 4100, 400));
                                else eplr.JoinMap(MapManager.CreateMultiPlayerMap(new MapManager.MapData(int.Parse(cmd[1]), int.Parse(cmd[2]), 200, 200), 4100, 450));
                                break;
                            case "clear":
                                eplr.JoinMap(MapManager.CreateMultiPlayerMap(new MapManager.MapData(), 4100, 400));
                                break;*/
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
                return;
            }
            else if (cmd.Count < 1)
            {
                tsp.SendErrorEX(error + "请输入 /territory(属地, sd) help 查看命令");
                return;
            }

            switch (cmd[0].ToLower())
            {
                case "1":
                    if(cmd.Count > 2) NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(tsp.TPlayer.position, ushort.Parse(cmd[1]), int.Parse(cmd[2]), 1), tsp.Index);
                    else NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(tsp.TPlayer.position, ushort.Parse(cmd[1]), -1, 1), tsp.Index);

                    break;
                case "create":
                    if (eplr.TerritoryUUID == Guid.Empty)
                    {
                        eplr.SendEX($"正在创建属地, 请稍候...");
                        var uuid = MapManager.CreateMultiPlayerMap("DefaultTerritory", 2100, 240);
                        if (MapManager.GetMapFromUUID(uuid, out var map))
                        {
                            map.Name = eplr.Name;
                            map.UUID = uuid;
                            map.Owner = eplr.ID;
                            map.KeepAlive = true;
                            map.Save();
                            eplr.SendSuccessEX($"成功创建属地, 正在传送 <{"注: ".ToColorful()}属地大小为 4200 * 1200, 超出此区域的建造将无效>");
                            Log.Info($"{eplr} 创建了属地.");
                            eplr.TerritoryUUID = uuid;
                            eplr.Save();
                            eplr.VisitTerritory(uuid);
                            
                        }
                        else
                        {
                            eplr.SendErrorEX($"创建失败, 请联系管理员.");
                        }
                    }
                    else
                    {
                        eplr.SendErrorEX($"你已拥有一块属地, 请使用 {"/territory(属地, sd) my".ToColorful()} 前往.");
                    }
                    break;
                case "my":
                    if (eplr.TerritoryUUID != Guid.Empty)
                    {
                        if (eplr.MapUUID != eplr.TerritoryUUID)
                        {
                            eplr.VisitTerritory(eplr.TerritoryUUID);
                        }
                        else
                        {
                            eplr.SendErrorEX($"你已处在属地中.");
                        }
                    }
                    else
                    {
                        eplr.SendErrorEX($"你尚未创建属地, 请输入 {"/territory(属地, sd) create".ToColorful()} 来获取.");
                    }
                    break;
                case "visit":
                    if (cmd.Count < 2)
                    {
                        eplr.SendErrorEX(error + "请输入想要参观的玩家名(不需要全部输入");
                        return;
                    }
                    if (UserManager.TryGetEPlayeFuzzy(cmd[1], out var list, true))
                    {
                        if (list.Count == 1)
                        {
                            var wantvisit = list[0];
                            if (wantvisit.TerritoryUUID != Guid.Empty)
                            {
                                eplr.VisitTerritory(wantvisit.TerritoryUUID);
                            }
                            else
                            {
                                eplr.SendErrorEX(error + $"此玩家尚未申请属地.");
                            }
                        }
                        else
                        {
                            eplr.tsp.SendMultipleError(list);
                        }
                    }
                    else
                    {
                        eplr.SendErrorEX(error + $"未找到名称中含有 {cmd[1].ToColorful()} 的玩家.");
                    }
                    break;
                case "clear":
                    if (eplr.MapUUID == eplr.TerritoryUUID)
                    {
                        eplr.Map.Items.ForEach(i => i = new EItem());
                        MapManager.SendAllItem(eplr);
                    }
                    break;
            }
        }
    }
}
