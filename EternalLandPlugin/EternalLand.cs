using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EternalLandPlugin.Account;
using EternalLandPlugin.Game;
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

        public static bool IsGameMode = true;
        public static List<TSPlayer> OnlineTSPlayer { get { return (from p in TShock.Players where p != null select p).ToList(); } }
        public static List<EPlayer> OnlineEPlayer { get { return (from p in EPlayers where p != null && p.Online select p).ToList(); } }

        public static EPlayer[] EPlayers = new EPlayer[255];

        public EternalLand(Main game) : base(game)
        {

        }
        public override void Initialize()
        {
            ServerApi.Hooks.GamePostInitialize.Register(this, PostInitialize);


        }
        public void PostInitialize(EventArgs args)
        {
            Main.ServerSideCharacter = true;
            ServerApi.Hooks.NetGreetPlayer.Register(this, NetProccess.PlayerJoin);
            ServerApi.Hooks.ServerLeave.Register(this, NetProccess.PlayerLeave);
            ServerApi.Hooks.NetGetData.Register(this, NetProccess.GetData);
            ServerApi.Hooks.WorldSave.Register(this, delegate { DataBase.SaveAllEPlayer(); });
            Hooks.Game.PostUpdate += delegate (ref GameTime gameTime) { EternalLandUpdate(); };
            Hooks.Player.PreUpdate += PlayerUpdate;
            TShockAPI.Hooks.AccountHooks.AccountCreate += NetProccess.PlayerRegister;
            GetDataHandlers.KillMe += NetProccess.PlayerDeath;
            if (!IsGameMode)
            {
                ServerApi.Hooks.NpcSpawn.Register(this, NetProccess.NpcSpawn);
                ServerApi.Hooks.NpcStrike.Register(this, NetProccess.NpcStrike);
                ServerApi.Hooks.NpcKilled.Register(this, NetProccess.NpcKill);
                ServerApi.Hooks.ServerChat.Register(this, delegate (ServerChatEventArgs args) { Terraria.Chat.ChatHelper.SendChatMessageToClientAs((byte)args.Who, NetworkText.FromLiteral(args.Text), Color.White, args.Who); });

                Commands.ChatCommands.Add(new Command("eternalland.bank.use", ProcessCommand.Bank, new string[]
                {
                "bank",
                "资产"
                })
                {
                    HelpText = "资产相关命令."
                });
            }
            else
            {
                DataBase.GetAllCharacter();
                ServerApi.Hooks.NetSendBytes.Register(this, GameNet.OnSendBytes);
                ServerApi.Hooks.NetSendData.Register(this,GameNet.OnSendData);
                GetDataHandlers.PlayerInfo += delegate (object o, GetDataHandlers.PlayerInfoEventArgs args) { if(args.Player.IsLoggedIn) args.Handled = true; };
                GetDataHandlers.NewProjectile += GameNet.OnReceiveNewProj;
                GetDataHandlers.ProjectileKill += GameNet.OnReceiveKillProj;
                GetDataHandlers.PlayerSpawn += GameNet.OnPlayerSpawn;

                Commands.ChatCommands.Add(new Command("eternalland.game.admin", GameCommand.AdminCommand, new string[]
                {
                    "/char"
                })
                {
                    HelpText = "小遊戲服管理員命令."
                });

                new Thread(new ThreadStart(MapManager.CheckMapAlive)).Start();
                new Thread(new ThreadStart(delegate {
                    while (true)
                    {
                        Thread.Sleep(1111);
                        OnlineEPlayer.ForEach(e => {
                            NetMessage.SendData(4, -1, -1, null, e.Index);
                            UserManager.SetBag(e);
                        });
                    }
                })).Start();
            }
            StatusSender.SendStatus();
        }

        protected override void Dispose(bool disposing)
        {
            ServerApi.Hooks.NetGreetPlayer.Deregister(this, NetProccess.PlayerJoin);
            TShockAPI.Hooks.AccountHooks.AccountCreate -= NetProccess.PlayerRegister;
            ServerApi.Hooks.NetGetData.Deregister(this, NetProccess.GetData);
            ServerApi.Hooks.NpcSpawn.Deregister(this, NetProccess.NpcSpawn);
            ServerApi.Hooks.NpcStrike.Deregister(this, NetProccess.NpcStrike);
            ServerApi.Hooks.NpcKilled.Deregister(this, NetProccess.NpcKill);
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

