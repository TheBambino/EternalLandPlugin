using System.Threading;
using System.Threading.Tasks;
using Terraria;

namespace EternalLandPlugin.AntiCheat
{
    class DataCheck
    {
        public async static void OnPlayerDamage(Player plr, int damage)
        {
            await Task.Run(() =>
            {
                int life = plr.statLife;
                Thread.Sleep(20);
                if (Main.player[plr.whoAmI].statLife >= life)
                {
                    plr.TSPlayer().SendEX("哼哼");
                }
            });
        }
    }
}
