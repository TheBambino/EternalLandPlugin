using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace EternalLandPlugin.Net
{
    internal class ProcessPacket
    {
        public static void GetData(GetDataEventArgs args)
        {

        }

        public static void PlayerJoin(GreetPlayerEventArgs args)
        {
            var plr = Main.player[args.Who];
            var tsp = plr.TSPlayer();
            if (tsp.Account != null) EternalLand.EPlayers[args.Who] = DataBase.GetEPlayer(tsp.Account.ID);
        }

        public static void PlayerLeave(LeaveEventArgs args)
        {
            var plr = Main.player[args.Who];
            var tsp = plr.TSPlayer();
            tsp.EPlayer().Save();
            if (tsp.Account != null) EternalLand.EPlayers[args.Who] = null;
        }

        public static void PlayerRegister(AccountCreateEventArgs args)
        {
            var account = args.Account;
            var tsp = Utils.GetTSPlayerFromID(account.ID);
            DataBase.AddEPlayer(account.ID);
            if (tsp != null) EternalLand.EPlayers[tsp.Index] = DataBase.GetEPlayer(account.ID);
        }

        public static void NpcSpawn(NpcSpawnEventArgs args)
        {
            var npc = Main.npc[args.NpcId];
            Bank.OnSpawn(npc);
        }

        public static void NpcStrike(NpcStrikeEventArgs args)
        {
            try
            {
                TSPlayer tsp = Utils.GetTSPlayerFromName(args.Player.name);
                Bank.OnStrike(args.Npc, tsp, args.Damage);
            }
            catch { }
        }

        public static void NpcKill(NpcKilledEventArgs args)
        {
            Bank.OnKill(args.npc);
        }
    }
}
