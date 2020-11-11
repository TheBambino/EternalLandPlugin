using EternalLandPlugin.Account;
using System.Collections.Generic;
using TShockAPI;

namespace EternalLandPlugin
{
    class ProcessCommand
    {
        public static void Bank(CommandArgs args)
        {
            var cmd = args.Parameters;
            var tsp = args.Player;
            var eplr = tsp.EPlayer();
            if (eplr == null)
            {
                tsp.SendErrorMessage("你尚未注册, 无法使用此命令.");
                return;
            }
            if (cmd.Count > 0)
            {
                switch (cmd[0])
                {
                    case "pay":
                    case "给":
                        long value = -1;
                        if (cmd.Count < 3 || long.TryParse(cmd[2], out value))
                        {
                            if (UserManager.GetTSPlayerFuzzy(cmd[1], out List<TSPlayer> list))
                            {
                                var receive = list[0];
                                if (receive.Name == tsp.Name)
                                {
                                    tsp.SendErrorEX("请勿转给自己.");
                                }
                                else if (receive.Account == null)
                                {
                                    tsp.SendErrorEX("该玩家尚未注册.");
                                }
                                else if (!eplr.TakeMoney(value, $"<转给玩家: {receive.Name}>", new Microsoft.Xna.Framework.Color(155, 56, 48), true))
                                {
                                    tsp.SendErrorEX($"资产不足. 当前你的账户中还剩余 {eplr.Money.ToColorful()}.");
                                }
                                else
                                {
                                    receive.EPlayer().GiveMoney(value, $"<来自玩家 {eplr.Name}>", new Microsoft.Xna.Framework.Color(155, 56, 48), true);
                                    tsp.SendSuccessEX($"成功向玩家 {receive.Name.ToColorful()} 支付 {value.ToColorful()}. 当前余额: {eplr.Money.ToColorful()}.");
                                    receive.SendSuccessEX($"玩家 {tsp.Name.ToColorful()} 向你支付 {value.ToColorful()}. 当前余额: {receive.EPlayer().Money.ToColorful()}.");
                                    Log.Info($"玩家 {tsp.Name} 向 {receive.Name} 支付 {value}.");
                                }
                            }
                            else
                            {
                                tsp.SendErrorEX($"未找到名称中含有 {cmd[1].ToColorful()} 的玩家.");
                            }
                        }
                        else
                        {
                            tsp.SendErrorEX("格式错误. /bank pay <玩家名> <支付金额>.");
                        }
                        break;
                    case "bal":
                    case "余额":
                        if (cmd.Count >= 2)
                        {
                            if (tsp.HasPermission("eternalland.admin"))
                            {
                                if (UserManager.TryGetEPlayeFuzzy(cmd[1], out var e, true))
                                {
                                    if (e.Count >= 2) tsp.SendMultipleError(e);
                                    else tsp.SendEX($"玩家 {e[0].Name.ToColorful()} 账户中的资产为 {tsp.EPlayer().Money.ToColorful()}");
                                }
                                else
                                {
                                    tsp.SendErrorEX($"未找到名称中包含 {cmd[1].ToColorful()} 的玩家.");
                                }
                            }
                            else
                            {
                                tsp.SendErrorEX($"你没有权限使用此命令.");
                            }
                        }
                        else
                        {
                            tsp.SendEX($"当前你账户中的资产为 [c/8DF9D8:{tsp.EPlayer().Money.ToColorful()}]");
                        }
                        break;
                    case "take":
                    case "t":
                        if (cmd.Count < 3)
                        {
                            tsp.SendErrorEX("格式错误. /bank take <玩家名> <数额>.");
                            return;
                        }
                        else if (!UserManager.TryGetEPlayeFuzzy(cmd[1], out var take_eplr, true))
                        {
                            tsp.SendErrorEX($"未找到名称中包含 {cmd[1].ToColorful()} 的玩家.");
                            return;
                        }
                        else if (take_eplr.Count >= 2)
                        {
                            tsp.SendMultipleError(take_eplr);
                            return;
                        }
                        else if (!long.TryParse(cmd[2], out long take_num))
                        {
                            tsp.SendErrorEX("格式错误. /bank take <玩家名> <数额>.");
                            return;
                        }
                        else if (take_eplr[0].Money < take_num)
                        {
                            tsp.SendErrorEX($"玩家 {take_eplr[0].Name.ToColorful()} 当前资产仅有 {take_eplr[0].Money.ToColorful()}, 无法取走 {take_num.ToColorful()}.");
                            return;
                        }
                        else
                        {
                            take_eplr[0].TakeMoney(take_num, "管理员取走", new Microsoft.Xna.Framework.Color(155, 56, 48), true);
                            tsp.SendSuccessEX($"成功从玩家 {take_eplr[0].Name.ToColorful()} 的账户中取走 {take_num.ToColorful()}, 当前剩余 {(take_eplr[0].Money).ToColorful()}");
                        }
                        break;
                    case "give":
                    case "g":
                        if (cmd.Count < 3)
                        {
                            tsp.SendErrorEX("格式错误. /bank give <玩家名> <数额>.");
                            return;
                        }
                        else if (!UserManager.TryGetEPlayeFuzzy(cmd[1], out var give_eplr, true))
                        {
                            tsp.SendErrorEX($"未找到名称中包含 {cmd[1].ToColorful()} 的玩家.");
                            return;
                        }
                        else if (give_eplr.Count >= 2)
                        {
                            tsp.SendMultipleError(give_eplr);
                            return;
                        }
                        else if (!long.TryParse(cmd[2], out long give_num))
                        {
                            tsp.SendErrorEX("格式错误. /bank give <玩家名> <数额>.");
                            return;
                        }
                        else
                        {
                            give_eplr[0].GiveMoney(give_num, "管理员给予", new Microsoft.Xna.Framework.Color(155, 56, 48), true);
                            tsp.SendSuccessEX($"成功给予玩家 {give_eplr[0].Name.ToColorful()} {give_num.ToColorful()} 资产, 当前共有 {(give_eplr[0].Money).ToColorful()}.");
                        }
                        break;
                    case "help":
                    default:
                        tsp.SendEX($"{(cmd[0] == "help" ? "" : "无效的命令.\n")}当前可用命令:\n/bank pay <玩家名> <数额> -- 向指定玩家转账.\n/bank bal -- 查询自己的余额.{(tsp.HasPermission("eternalland.admin") ? " -- 管理员命令 -- \n/bank give|take <玩家名> <数额> -- 给予或取走玩家的资产." : "")}");
                        break;
                }
            }
            else
            {
                tsp.SendEX($"当前你的账户资产为 {eplr.Money.ToColorful()}.");
            }
        }
    }
}
