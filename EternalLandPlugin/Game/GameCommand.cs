using EternalLandPlugin.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace EternalLandPlugin.Game
{
    class GameCommand
    {
        public static void AdminCommand(CommandArgs args)
        {
            var tsp = args.Player;
            var eplr = tsp.EPlayer();
            string error = "命令無效.";
            var cmd = args.Parameters;
            if (args.Parameters.Count >= 1)
            {
                switch (cmd[0])
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
                    case "wld":
                        if (cmd.Count > 2) eplr.JoinMap(MapManager.CreateMultiPlayerMap(new MapManager.MapData(2152, 394,int.Parse(cmd[1]), int.Parse(cmd[2])), 4100, 400));
                        else eplr.JoinMap(MapManager.CreateMultiPlayerMap(new MapManager.MapData(eplr.TileX - 25, eplr.TileX - 25, 50, 50), 4100, 400));

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
                            eplr.JoinMap(t[0].GameInfo.MapUUID);
                        }
                        break;
                }
            }
            
        }
    }
}
