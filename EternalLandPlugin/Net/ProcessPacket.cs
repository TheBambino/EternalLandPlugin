using EternalLandPlugin.Account;
using EternalLandPlugin.Game;
using EternalLandPlugin.Hungr;
using System;
using System.IO;
using System.IO.Streams;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Hooks;

namespace EternalLandPlugin.Net
{
    internal class NetProccess
    {
        public static void GetData(GetDataEventArgs args)
        {
            var plr = Main.player[args.Msg.whoAmI];
            var tsp = plr.TSPlayer();
            using (MemoryStream reader = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length - 1))
            {
                switch (args.MsgID)
                {
                    case PacketTypes.ItemOwner:
                        int item = reader.ReadInt16();
                        int ID = (int)reader.ReadByte();
                        if (item == 0) StatusSender.GetPingPakcet(args.Msg.whoAmI);
                        break;
                    case PacketTypes.PlayerHp:
                        reader.ReadByte();
                        int life = reader.ReadInt16();
                        args.Handled = PlayerHeal(plr, life);
                        break;
                    case PacketTypes.PlayerHurtV2:
                        reader.ReadByte();
                        PlayerDeathReason playerDeathReason = new PlayerDeathReason();
                        BitsByte bitsByte = (BitsByte)reader.ReadByte();
                        if (bitsByte[0])
                        {
                            playerDeathReason._sourcePlayerIndex = (int)reader.ReadInt16();
                        }
                        if (bitsByte[1])
                        {
                            playerDeathReason._sourceNPCIndex = (int)reader.ReadInt16();
                        }
                        if (bitsByte[2])
                        {
                            playerDeathReason._sourceProjectileIndex = (int)reader.ReadInt16();
                        }
                        if (bitsByte[3])
                        {
                            playerDeathReason._sourceOtherIndex = (int)reader.ReadByte();
                        }
                        if (bitsByte[4])
                        {
                            playerDeathReason._sourceProjectileType = (int)reader.ReadInt16();
                        }
                        if (bitsByte[5])
                        {
                            playerDeathReason._sourceItemType = (int)reader.ReadInt16();
                        }
                        if (bitsByte[6])
                        {
                            playerDeathReason._sourceItemPrefix = (int)reader.ReadByte();
                        }
                        if (bitsByte[7])
                        {
                            playerDeathReason._sourceCustomReason = reader.ReadString();
                        }
                        int damage = reader.ReadInt16();
                        args.Handled = PlayerDamage(plr, damage);
                        break;
                }
            }

            if (args.MsgID == PacketTypes.PlayerUpdate)
            {
                if (plr.controlUseItem && (plr.HeldItem.useStyle == 2 || plr.HeldItem.useStyle == 9 || plr.HeldItem.netID == 5) && UserManager.TryGetEPlayeFromName(plr.name, out Account.EPlayer eplr) && eplr.HungrValue < 34200 && eplr.CanEat)
                {
                    HungrSystem.OnEat(eplr, plr.HeldItem.buffTime);
                }
            }
        }

        public static void PlayerJoin(GreetPlayerEventArgs args)
        {
            var plr = Main.player[args.Who];
            var tsp = plr.TSPlayer();
            NetMessage.SendData(39, args.Who, -1, null, 0);
            UserAccount userAccount = TShock.UserAccounts.GetUserAccountByName(tsp.Name);
            var eplr = userAccount == null ? null : DataBase.GetEPlayer(userAccount.ID).Result;
            if (eplr != null)
            {
                EternalLand.EPlayers[args.Who] = eplr;
                eplr.SendBag();
                Log.Info($"玩家 {eplr.Name} 数据已读取.");
                if (eplr.Name == "咕咕咕") eplr.ChangeCharacter("y0");
                UserManager.SetPlayerActive(eplr);
            }
        }

        public static void PlayerLeave(LeaveEventArgs args)
        {
            var plr = Main.player[args.Who];
            var tsp = plr.TSPlayer();
            var eplr = tsp.EPlayer();
            if (eplr != null)
            {
                eplr.Save();
                EternalLand.EPlayers[args.Who].Dispose();
                EternalLand.EPlayers[args.Who] = null;
                Log.Info($"玩家 {eplr.Name} 数据已保存.");
                if (eplr.TerritoryUUID != Guid.Empty && MapManager.GetMapFromUUID(eplr.TerritoryUUID, out var map))
                {
                    if (map.Player.Contains(eplr.ID)) map.Player.Remove(eplr.ID);
                    if (!map.Player.Any() && map.Save())
                    {
                        map.Dispose();
                    }
                }
            }
        }

        public static void PlayerRegister(AccountCreateEventArgs args)
        {
            var account = args.Account;
            UserManager.GetTSPlayerFromName(account.Name, out var tsp);
            DataBase.AddEPlayer(account.ID, account.Name);
            //tsp.SendData(PacketTypes.RemoveItemOwner, "", 0);
            tsp.SendSuccessEX($"注册成功! 请使用 {("/login <密码>").ToColorful()} 进行登陆.");
        }

        public static bool PlayerHeal(Player plr, int life)
        {

            if (UserManager.TryGetEPlayeFromName(plr.name, out var eplr) && eplr.HungrValue == 0)
            {
                int lifechange = life - plr.statLife;
                if (EternalLand.IsGameMode)
                {
                    eplr.Statistic.Heal += lifechange;
                }
                else
                {
                    if (eplr.Life == -1)
                    {
                        eplr.Life = life;
                        return false;
                    }
                    if (lifechange > 0 && !eplr.tsp.GodMode)
                    {
                        NetMessage.SendData(16, -1, -1, null, plr.whoAmI);
                        return true;
                    }
                }

            }
            return false;
        }

        public static bool PlayerDamage(Player plr, int damage)
        {
            if (UserManager.TryGetEPlayeFromName(plr.name, out var eplr) && eplr.HungrValue == 0)
            {
                AntiCheat.DataCheck.OnPlayerDamage(plr, damage);
                if (EternalLand.IsGameMode)
                {
                    // eplr.Statistic.GetDamage += damage;
                }
                else
                {
                    eplr.Life -= damage;
                }
            }
            return false;
        }

        public static void PlayerDeath(object o, GetDataHandlers.KillMeEventArgs args)
        {
            var eplr = args.Player.EPlayer();
            if (eplr != null)
            {
                if (EternalLand.IsGameMode)
                {
                    eplr.Statistic.Dead++;
                }
                else
                {

                }
            }
        }

        public static void NpcSpawn(NpcSpawnEventArgs args)
        {
            var npc = Main.npc[args.NpcId];
            Bank.OnSpawn(npc);
        }

        public static void NpcStrike(NpcStrikeEventArgs args)
        {
            if (UserManager.TryGetEPlayeFromName(args.Player.name, out EPlayer eplr))
            {

                if (EternalLand.IsGameMode)
                {
                    eplr.Statistic.Damage_NPC += args.Damage;
                    if (args.Critical) eplr.Statistic.CritCount++;
                }
                else
                {
                    //Bank.OnStrike(args.Npc, tsp, args.Damage);
                }
            }
        }

        /*public static void NpcKill(NpcKilledEventArgs args)
        {

            if (EternalLand.IsGameMode)
            {
                //eplr.Statistic.Damage_NPC++;
            }
            else
            {
                var npc = args.npc;
                Bank.OnKill(args.npc);
                if ((npc.FullName.Contains("Slime") || npc.FullName.Contains("史莱姆")) && Utils.RANDOM.Next(0, 100) > 50) Utils.DropItem((int)npc.position.X, (int)npc.position.Y, 4009);
            }
        }*/
    }
}
