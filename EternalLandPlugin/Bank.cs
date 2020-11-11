using EternalLandPlugin.Account;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

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

        public static readonly double Coefficient = 0.02;

        public static readonly Dictionary<int, DamageNPC> DamageList = new Dictionary<int, DamageNPC>();

        public static void OnSpawn(NPC npc)
        {
            if (!npc.friendly && !npc.townNPC && !DamageList.ContainsKey(npc.whoAmI)) DamageList.Add(npc.whoAmI, new DamageNPC(npc.whoAmI));
        }

        public static void OnStrike(NPC npc, TSPlayer tsp, long damage)
        {
            try
            {
                if (!DamageList.ContainsKey(npc.whoAmI)) DamageList.Add(npc.whoAmI, new DamageNPC(npc.whoAmI));
                if (tsp.IsLoggedIn) DamageList[npc.whoAmI].CauseDamage(tsp.Account.ID, damage > npc.life ? npc.life : damage);
            }
            catch { }
        }

        public static async void OnKill(NPC npc)
        {
            await Task.Run(() =>
            {
                if (DamageList.TryGetValue(npc.whoAmI, out DamageNPC info))
                {
                    DamageList.Remove(npc.whoAmI);
                    info.Damage.ForEach(value =>
                    {
                        if (value.Value != 0 && !npc.SpawnedFromStatue)
                        {
                            if (UserManager.TryGetEPlayerFromID(value.Key, out var eplr))
                            {
                                long money = (long)((info.TotalDamage / value.Value) * info.TotalDamage * Coefficient);
                                eplr.GiveMoney(money == 0 ? 1 : money, npc.FullName);
                                if (npc.boss) eplr.BossKillCount++;
                                else eplr.MobKillCount++;
                            }
                        }
                    });
                }
            });
        }
    }
}
