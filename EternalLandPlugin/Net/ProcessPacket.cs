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
            if (tsp.Account != null) EternalLand.EPlayers[args.Who] = DataBase.GetEPlayer(tsp.Account.ID).Result;
        }

        public static void PlayerLeave(LeaveEventArgs args)
        {
            var plr = Main.player[args.Who];
            var tsp = plr.TSPlayer();
            tsp.EPlayer().Save();
            if (tsp.Account != null) EternalLand.EPlayers[args.Who] = null;
        }

        public async static void PlayerRegister(AccountCreateEventArgs args)
        {
            var account = args.Account;
            Utils.GetTSPlayerFromName(account.Name, out var tsp);
            DataBase.AddEPlayer(account.ID, account.Name);
            if (tsp != null) EternalLand.EPlayers[tsp.Index] = DataBase.GetEPlayer(account.ID).Result;
            tsp.SendSuccessEX($"注册成功! 请使用 {("/login <密码>").ToColorful()} 进行登陆.");
        }

        public static void NpcSpawn(NpcSpawnEventArgs args)
        {
            var npc = Main.npc[args.NpcId];
            Bank.OnSpawn(npc);
        }

        public static void NpcStrike(NpcStrikeEventArgs args)
        {
            if (Utils.GetTSPlayerFromName(args.Player.name, out TSPlayer tsp))
            {
                Bank.OnStrike(args.Npc, tsp, args.Damage);
            }
        }

        public static void NpcKill(NpcKilledEventArgs args)
        {
            Bank.OnKill(args.npc);
        }
    }
}
