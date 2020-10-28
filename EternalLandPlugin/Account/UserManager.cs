using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace EternalLandPlugin.Account
{
    class UserManager
    {
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
    }
}
