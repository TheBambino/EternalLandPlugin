using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EternalLandPlugin.Account;
using EternalLandPlugin.Net;
using Microsoft.Xna.Framework;
using OTAPI;
using Terraria;
using Terraria.Localization;
using Terraria.Net.Sockets;
using TerrariaApi.Server;
using TShockAPI;

namespace EternalLandPlugin
{
    [ApiVersion(2, 1)]
    public class EternalLand : TerrariaPlugin
    {
        public override string Name => "EternalLandPlugin";
        public override Version Version => new Version(2, 0);
        public override string Author => "Megghy";
        public override string Description => "永恒之地服务器插件.";
        public static List<TSPlayer> OnlineTSPlayer { get { return (from p in TShock.Players where p != null select p).ToList(); } }
        public static List<EPlayer> OnlineEPlayer { get { return (from p in EPlayers where p != null select p).ToList(); } }
        public static EPlayer[] EPlayers = new EPlayer[255];

        public EternalLand(Main game) : base(game)
        {

        }
        public override void Initialize()
        {
            ServerApi.Hooks.GamePostInitialize.Register(this, PostInitialize);


        }
        public async void PostInitialize(EventArgs args)
        {
            Main.ServerSideCharacter = true;
            ServerApi.Hooks.NetGreetPlayer.Register(this, ProcessPacket.PlayerJoin);
            ServerApi.Hooks.NetGetData.Register(this, ProcessPacket.GetData);
            ServerApi.Hooks.NpcSpawn.Register(this, ProcessPacket.NpcSpawn);
            ServerApi.Hooks.NpcStrike.Register(this, ProcessPacket.NpcStrike);
            ServerApi.Hooks.NpcKilled.Register(this, ProcessPacket.NpcKill);
            ServerApi.Hooks.WorldSave.Register(this, delegate { DataBase.SaveAllEPlayer(); });
            ServerApi.Hooks.ServerChat.Register(this, delegate (ServerChatEventArgs args) { Terraria.Chat.ChatHelper.SendChatMessageToClientAs((byte)args.Who, NetworkText.FromLiteral(args.Text), Color.White, args.Who); });
            GetDataHandlers.KillMe += ProcessPacket.PlayerDeath;
            Hooks.Game.PostUpdate += delegate (ref GameTime gameTime) { EternalLandUpdate(); };
            Hooks.Player.PreUpdate += PlayerUpdate;

            TShockAPI.Hooks.AccountHooks.AccountCreate += ProcessPacket.PlayerRegister;


            Commands.ChatCommands.Add(new Command("eternalland.bank.use", ProcessCommand.Bank, new string[]
            {
                "bank",
                "资产"
            })
            {
                HelpText = "资产相关命令."
            });

            StatusSender.SendStatus();
        }

        protected override void Dispose(bool disposing)
        {
            ServerApi.Hooks.NetGreetPlayer.Deregister(this, ProcessPacket.PlayerJoin);
            TShockAPI.Hooks.AccountHooks.AccountCreate -= ProcessPacket.PlayerRegister;
            ServerApi.Hooks.NetGetData.Deregister(this, ProcessPacket.GetData);
            ServerApi.Hooks.NpcSpawn.Deregister(this, ProcessPacket.NpcSpawn);
            ServerApi.Hooks.NpcStrike.Deregister(this, ProcessPacket.NpcStrike);
            ServerApi.Hooks.NpcKilled.Deregister(this, ProcessPacket.NpcKill);
            base.Dispose(disposing);
        }

        protected async void EternalLandUpdate()
        {
            await Task.Run(() =>
            {
                OnlineEPlayer.ForEach(e => e.Update());
            });
        }

        protected HookResult PlayerUpdate(Player plr, ref int i)
        {
            return HookResult.Continue;
        }
    }
}

