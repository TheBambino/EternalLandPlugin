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

        public static TSPlayer GetTSPlayerFromName(string name)
        {
            foreach (var tsp in TShock.Players)
            {
                if (tsp.Name == name)
                {
                    return tsp;
                }
            }
            return null;
        }

        public static TSPlayer GetTSPlayerFromID(int id)
        {
            foreach (var tsp in TShock.Players)
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

        public static EPlayer EPlayer(this TSPlayer tsp)
        {
            return tsp.Account != null ? Utils.GetEPlayerFromID(tsp.Account.ID) : null;
        }

        public static TSPlayer TSPlayer(this Terraria.Player plr)
        {
            return Utils.GetTSPlayerFromName(plr.name);
        }


        public static void SendSuccessEX(this TSPlayer tsp, object text)
        {
            tsp.SendSuccessMessage(ServerPrefix + text);
        }

        public static void SendInfoEX(this TSPlayer tsp, object text)
        {
            tsp.SendInfoMessage(ServerPrefix + text);
        }

        public static void SendErrorEX(this TSPlayer tsp, object text)
        {
            tsp.SendErrorMessage(ServerPrefix + text);
        }

        public static void SendEX(this TSPlayer tsp, object text, Color color = default)
        {
            color = color == default ? new Color(212, 239, 245) : color;
            tsp.SendMessage(ServerPrefix + text, color);
        }
    }
}
