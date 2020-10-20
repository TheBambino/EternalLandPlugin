using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EternalLandPlugin.Account;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace EternalLandPlugin
{
    class Utils
    {
        public static Random RANDOM = new Random();
        public static EPlayer GetEPlayerFromID(int id)
        {
            EPlayer eplr = null;
            EternalLand.OnlineEPlayer.ForEach(e => { if (e.ID == id) eplr = e; });
            return eplr;
        }

        public static bool TryGetEPlayerFromID(int id, out EPlayer eplr)
        {
            eplr = null;
            var list = (from temp in EternalLand.OnlineEPlayer where temp.ID == id select temp).ToList();
            if (list.Any())
            {
                eplr = list[0];
                return true;
            }
            else
            {
                eplr = null;
                return false;
            }
        }

        public static bool TryGetEPlayeFuzzy(string name, out List<EPlayer> eplrs, bool offline = false)
        {
            var list = (from temp in (offline ? DataBase.GetEPlayerFuzzy(name).Result : EternalLand.OnlineEPlayer) where temp.Name.ToLower().Contains(name.ToLower()) select temp).ToList();
            if (list.Any())
            {
                eplrs = list;
                return true;
            }
            else
            {
                eplrs = null;
                return false;
            }
        }

        public static bool TryGetEPlayeFromName(string name, out EPlayer eplr, bool offline = false)
        {
            eplr = null;
            var list = (from temp in (offline ? DataBase.GetEPlayerFuzzy(name).Result : EternalLand.OnlineEPlayer) where temp.Name == name select temp).ToList();
            if (list.Any())
            {
                eplr = list[0];
                return true;
            }
            else
            {
                eplr = null;
                return false;
            }
        }

        public static bool GetTSPlayerFromName(string name, out TSPlayer tsp)
        {
            tsp = null;
            foreach (var t in from t in EternalLand.OnlineTSPlayer where t.Name == name select t)
            {
                tsp = t;
                return true;
            }

            return false;
        }

        public static bool GetTSPlayerFuzzy(string name, out List<TSPlayer> list)
        {
            list = new List<TSPlayer>();
            foreach (var tsp in EternalLand.OnlineTSPlayer)
            {
                if (tsp.Name.ToLower().Contains(name.ToLower()))
                {
                    list.Add(tsp);
                }
            }
            if (list.Any()) return true;
            else return false;
        }

        public static TSPlayer GetTSPlayerFromID(int id)
        {
            foreach (var tsp in EternalLand.OnlineTSPlayer)
            {
                if (tsp.Account != null && tsp.Account.ID == id)
                {
                    return tsp;
                }
            }
            return null;
        }

        public static List<string> BuildLinesFromTerms(IEnumerable terms, Func<object, string> termFormatter = null, string separator = ", ", int maxCharsPerLine = 80)
        {
            List<string> list = new List<string>();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (object term in terms)
            {
                if (term == null && termFormatter == null)
                {
                    continue;
                }
                string text;
                if (termFormatter != null)
                {
                    try
                    {
                        if ((text = termFormatter(term)) == null)
                        {
                            continue;
                        }
                    }
                    catch (Exception innerException)
                    {
                        throw new ArgumentException("The method represented by termFormatter has thrown an exception. See inner exception for details.", innerException);
                    }
                }
                else
                {
                    text = term.ToString();
                }
                if (stringBuilder.Length + text.Length + separator.Length < maxCharsPerLine)
                {
                    stringBuilder.Append(text).Append(separator);
                    continue;
                }
                list.Add(stringBuilder.ToString());
                stringBuilder.Clear().Append(text).Append(separator);
            }
            if (stringBuilder.Length > 0)
            {
                list.Add(stringBuilder.ToString().Substring(0, stringBuilder.Length - separator.Length));
            }
            return list;
        }
        #region 游戏内快捷函数
        public static void DropItem(int x, int y, Item item, Vector2 vector = default)
        {
            int number = Item.NewItem((int)x, (int)y, (int)(vector == default ? 0 : vector.X), (int)(vector == default ? 0 : vector.Y), item.type, item.stack, true, item.prefix, true, false);
            NetMessage.SendData(21, -1, -1, null, number);
        }
        public static void DropItem(int x, int y, int id, int stack = 1, int prefix = 0, Vector2 vector = default)
        {
            int number = Item.NewItem((int)x, (int)y, (int)(vector == default ? 0 : vector.X), (int)(vector == default ? 0 : vector.Y), id, stack, true, prefix, true, false);
            NetMessage.SendData(21, -1, -1, null,number);
        }
        #endregion
    }

    public static class Expansion
    {
        static string ServerPrefix = "[c/f5b6b1:✿][c/ec7062:-永恒][c/ca6f1d:▪][c/f4cf40:cORE-][c/f9e79f:✿] ";

        public static string ToColorful(this object text, string colortext = "8DF9D8")
        {
            return $"[C/{colortext}:{text.ToString().Replace("[", "<").Replace("]", ">")}]";
        }

        public static EPlayer EPlayer(this TSPlayer tsp)
        {
            return tsp.Account != null ? Utils.GetEPlayerFromID(tsp.Account.ID) : null;
        }

        public static TSPlayer TSPlayer(this EPlayer eplr)
        {
            if (Utils.GetTSPlayerFromName(eplr.Name, out TSPlayer tsp))
            {
                return tsp;
            }
            else
            {
                return null;
            }
        }

        public static TSPlayer TSPlayer(this Terraria.Player plr)
        {
            Utils.GetTSPlayerFromName(plr.name, out TSPlayer tsp);
            return tsp;
        }

        public static void SendSuccessEX(this TSPlayer tsp, object text)
        {
            tsp.SendMessage(ServerPrefix + text, new Color(120, 194, 96));
        }

        public static void SendInfoEX(this TSPlayer tsp, object text)
        {
            tsp.SendMessage(ServerPrefix + text, new Color(216, 212, 82));
        }

        public static void SendErrorEX(this TSPlayer tsp, object text)
        {
            tsp.SendMessage(ServerPrefix + text, new Color(195, 83, 83));
        }

        public static void SendEX(this TSPlayer tsp, object text, Color color = default)
        {
            color = color == default ? new Color(212, 239, 245) : color;
            tsp.SendMessage(ServerPrefix + text, color);
        }

        public static void SendSuccessEX(this EPlayer eplr, object text)
        {
            eplr.tsp.SendMessage(ServerPrefix + text, new Color(120, 194, 96));
        }

        public static void SendInfoEX(this EPlayer eplr, object text)
        {
            eplr.tsp.SendMessage(ServerPrefix + text, new Color(216, 212, 82));
        }

        public static void SendErrorEX(this EPlayer eplr, object text)
        {
            eplr.tsp.SendMessage(ServerPrefix + text, new Color(195, 83, 83));
        }

        public static void SendEX(this EPlayer eplr, object text, Color color = default)
        {
            color = color == default ? new Color(212, 239, 245) : color;
            eplr.tsp.SendMessage(ServerPrefix + text, color);
        }
        public static void SendData(this EPlayer eplr, PacketTypes msgType, string text = "", int number = 0, float number2 = 0f, float number3 = 0f, float number4 = 0f, int number5 = 0)
        {
            if (Utils.GetTSPlayerFromName(eplr.Name, out var tsp))
            {
                if (!tsp.RealPlayer || tsp.ConnectionAlive)
                {
                    NetMessage.SendData((int)msgType, tsp.Index, -1, NetworkText.FromLiteral(text), number, number2, number3, number4, number5);
                }
            }
        }

        public static void SendMultipleError(this TSPlayer tsp, IEnumerable<object> matches)
        {
            tsp.SendErrorEX("检索出多个满足条件的项目: ");
            Utils.BuildLinesFromTerms(matches.ToArray<object>(), null, ", ", 80).ForEach(new Action<string>(tsp.SendInfoEX));
            tsp.SendErrorEX("使用 \"部分1 部分2\" 来输入包含空格的关键词.");
        }
    }
}
