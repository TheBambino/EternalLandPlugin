using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;
using System.Threading;

namespace EternalLandPlugin
{
    class Bank
    {
        public class DamageNPC
        {
            public DamageNPC(int whoami, int userid = -1, long damage = 0)
            {
                whoAmI = whoami;
                Damage.Add(userid, damage);
            }
            int whoAmI = -1;

            public long TotalDamage = 0;

            public NPC NPC { get { return Main.npc[whoAmI] ?? new NPC(); } }

            public readonly Dictionary<int, long> Damage = new Dictionary<int, long>();

            public void CauseDamage(int userid, long damage)
            {
                if (!Damage.ContainsKey(userid)) Damage.Add(userid, 0);
                Damage[userid] += damage;
                TotalDamage += damage;
            }
        }

        public static readonly double Coefficient = 0.1;

        public static readonly Dictionary<int, DamageNPC> DamageList = new Dictionary<int, DamageNPC>();

        public static void OnSpawn(NPC npc)
        {
            if(!npc.friendly && !npc.townNPC && !DamageList.ContainsKey(npc.whoAmI)) DamageList.Add(npc.whoAmI, new DamageNPC(npc.whoAmI));
        }

        public static void OnStrike(NPC npc, TSPlayer tsp, long damage)
        {
            if (!DamageList.ContainsKey(npc.whoAmI)) DamageList.Add(npc.whoAmI, new DamageNPC(npc.whoAmI));
            if(tsp.IsLoggedIn) DamageList[npc.whoAmI].CauseDamage(tsp.Account.ID, damage > npc.life ? npc.life : damage);
        }

        public static async void OnKill(NPC npc)
        {
            await Task.Run(() => {
                if (DamageList.TryGetValue(npc.whoAmI, out DamageNPC info))
                {
                    DamageList.Remove(npc.whoAmI);
                    info.Damage.ForEach(value =>
                    {
                        if (value.Value != 0 && !npc.SpawnedFromStatue)
                        {
                            var eplr = Utils.GetEPlayerFromID(value.Key);
                            long money = (long)((info.TotalDamage / value.Value) * info.TotalDamage * Coefficient);
                            eplr.Money += money;
                            eplr.SendCombatMessage($"+ {money} <{npc.FullName}>", Color.Yellow);
                        }
                    });
                }
            });
        }
    }
}
