using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EternalLandPlugin.Account;
using Microsoft.Xna.Framework;
using TShockAPI;

namespace EternalLandPlugin
{
    class Utils
    {
        public static EPlayer GetEPlayerFromID(int id)
        {
            EPlayer eplr = null;
            EternalLand.EPlayers.ForEach(e => { if (e != null && e.ID == id) eplr = e; });
            return eplr;
        }

        public static bool TryGetEPlayerFromID(int id, out EPlayer eplr)
        {
            eplr = null;
            var list = (from temp in EternalLand.EPlayers where temp.ID == id select temp).ToList();
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

        public static bool TryGetEPlayeFuzzy(string name, out EPlayer eplr)
        {
            eplr = null;
            var list = (from temp in EternalLand.EPlayers where temp.Name.ToLower().Contains(name.ToLower()) select temp).ToList();
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
    }
}
