using EternalLandPlugin.Account;
using EternalLandPlugin.Net;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Streams;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Models.Projectiles;

namespace EternalLandPlugin.Game
{
    class GameNet
    {
        public static void OnSendBytes(SendBytesEventArgs args)
        {

        }

        public async static void OnSendData(SendDataEventArgs args)
        {
            await Task.Run(() =>
            {
                switch (args.MsgId)
                {
                    case (PacketTypes)20:
                    case (PacketTypes)10:
                    case PacketTypes.Tile:
                        if (args.remoteClient == -1)
                        {
                            args.Handled = true;
                            EternalLand.OnlineEPlayer.Where(e => !e.GameInfo.IsInAnotherWorld).ForEach(e =>
                            {
                                NetMessage.SendData((int)args.MsgId, e.Index, -1, args.text, args.number, args.number2, args.number3, args.number4, args.number5, args.number6, args.number7);
                            });
                        }
                        break;
                    case PacketTypes.ProjectileNew:
                        if (args.ignoreClient != -1)
                        {
                            args.Handled = true;
                            var epl = TShock.Players[args.ignoreClient].EPlayer();
                            if (epl != null && MapManager.GetMapFromUUID(epl.GameInfo.MapUUID, out var map))
                            {
                                EternalLand.OnlineEPlayer.ForEach(e =>
                                {
                                    if (e != epl && map.Player.Contains(e.ID))
                                    {
                                        NetMessage.SendData((int)args.MsgId, e.Index, -1, args.text, args.number, args.number2, args.number3, args.number4, args.number5, args.number6, args.number7);
                                    }
                                });
                            }
                            else
                            {
                                EternalLand.OnlineEPlayer.ForEach(e =>
                                {
                                    if (e != epl && !e.GameInfo.IsInAnotherWorld)
                                    {
                                        NetMessage.SendData((int)args.MsgId, e.Index, -1, args.text, args.number, args.number2, args.number3, args.number4, args.number5, args.number6, args.number7);
                                    }
                                });
                            }
                        }
                        break;
                    case PacketTypes.ProjectileDestroy:
                        var proj = Main.projectile[args.number];
                        if (args.number < 255 && args.number > -1 && proj.owner != 255)
                        {                            
                            var eplr = TShock.Players[proj.owner].EPlayer();
                            if (eplr != null && eplr.GameInfo.IsInAnotherWorld) args.Handled = true;
                        }
                        break;
                }
            });
        }

        public static void OnReceiveNewProj(object o, GetDataHandlers.NewProjectileEventArgs args)
        {
            var eplr = args.Player.EPlayer();
            if (eplr != null)
            {
                SendToAnotherPlayer(eplr, args);
            }
        }

        static void SendToAnotherPlayer(EPlayer eplr, GetDataHandlers.NewProjectileEventArgs args)
        {
            return;
            if (MapManager.GetMapFromUUID(eplr.GameInfo.MapUUID, out var map))
            {
                args.Handled = true;
                args.Data.Position = 3L;
                args.Data.ReadInt16();
                StreamExt.ReadVector2(args.Data);
                StreamExt.ReadVector2(args.Data);
                args.Data.ReadInt8();
                args.Data.ReadInt16();
                NewProjectileData newProjectileData = new NewProjectileData((byte)args.Data.ReadByte());
                float[] array = new float[Projectile.maxAI];
                for (int i = 0; i < Projectile.maxAI; i++)
                {
                    array[i] = ((!newProjectileData.AI[i]) ? 0f : args.Data.ReadSingle());
                }
                short dmg = (short)(newProjectileData.HasDamage ? args.Data.ReadInt16() : 0);
                float knockback = (newProjectileData.HasKnockback ? args.Data.ReadSingle() : 0f);
                short originDamage = 0;
                short uuid = 1000;
                if (newProjectileData.HasOriginalDamage)
                {
                    originDamage = args.Data.ReadInt16();
                }
                if (newProjectileData.HasUUUID)
                {
                    args.Data.ReadInt16();
                }
                var proj = CreateProjectile(args.Position, args.Velocity, args.Identity, uuid, args.Type, originDamage, args.Damage, args.Knockback, args.Owner, array);
                int index = 1000;
                for (int i = 0; i < 1000; i++)
                {
                    if (map.Proj[i] != null && map.Proj[i].identity == args.Identity && map.Proj[i].owner == args.Owner)
                    {
                        index = i;
                    }
                }
                if (index == 1000)
                {
                    bool changed = false;
                    for (int num84 = 0; num84 < 1000; num84++)
                    {
                        if (map.Proj[num84] != null && !map.Proj[num84].active)
                        {
                            map.Proj[num84] = proj;
                            changed = true;
                            break;
                        }
                    }
                    if (!changed)
                    {
                        int result = 1000;
                        int num = 9999999;
                        for (int i = 0; i < 1000; i++)
                        {
                            if (map.Proj[i] != null && !map.Proj[i].netImportant && map.Proj[i].timeLeft < num)
                            {
                                map.Proj[i] = proj;
                            }
                        }
                    }
                }
                else
                {
                    map.Proj[index] = proj;
                }
                map.Player.ForEach(p =>
                {
                    EPlayer eplr = UserManager.GetEPlayerFromID(p);
                    if (eplr != null)
                    {
                        eplr.SendProjectile(proj);
                    }
                });
            }
        }

        public static Projectile CreateProjectile(Vector2 position, Vector2 velocity, int identity, int uuid, int type, int origindamage, int damage, float knockback, int owner, float[] AI)
        {
            Projectile projectile = new Projectile();
            projectile.identity = identity;
            projectile.position = position;
            projectile.velocity = velocity;
            projectile.type = type;
            projectile.damage = damage;
            projectile.originalDamage = origindamage;
            projectile.knockBack = knockback;
            projectile.owner = owner;
            projectile.projUUID = uuid;
            for (int num85 = 0; num85 < Projectile.maxAI; num85++)
            {
                projectile.ai[num85] = AI[num85];
            }
            projectile.ProjectileFixDesperation();
            return projectile;
        }
        public static void OnReceiveKillProj(object o, GetDataHandlers.ProjectileKillEventArgs args)
        {
            args.Player.SendSuccessMessage(args.ProjectileIndex.ToString());
        }

        public static void OnPlayerSpawn(object o, GetDataHandlers.SpawnEventArgs args)
        {
            var eplr = args.Player.EPlayer();
            if (eplr != null)
            {
                args.Handled = true;
                args.Player.TPlayer.Spawn(args.SpawnContext);
                args.Player.Teleport(eplr.SpawnX, eplr.SpawnY);
            }
        }

        public async static void OnTileEdit(object o, GetDataHandlers.TileEditEventArgs args)
        {
            await Task.Run(() => {
                var eplr = args.Player.EPlayer();
                if (eplr != null)
                {
                    if (eplr.SettingPoint != 0)
                    {
                        eplr.ChoosePoint[eplr.SettingPoint - 1] = new Point(args.X, args.Y);
                        if (eplr.SettingPoint == 1)
                        {
                            eplr.SendEX($"已进行地图左上角选择 <{(args.X + " - " + args.Y).ToColorful()}>. 请继续选择右下角.");
                            eplr.SettingPoint = 2;
                        }
                        else if(eplr.ChoosePoint[0].X >= args.X || eplr.ChoosePoint[0].Y >= args.Y)
                        {
                            eplr.SendErrorEX($"第二个点位 <{(args.X + " - " + args.Y).ToColorful()}> 与第一个点位 <{(eplr.ChoosePoint[0].X + " - " + eplr.ChoosePoint[0].Y).ToColorful()}> 坐标重合或未处于右下角, 请重新选择.");
                        }
                        else
                        {
                            eplr.SendEX($"已选择全部点位. 请使用 {"//map create <地图名>".ToColorful()} 进行地图创建.");
                            eplr.SettingPoint = 0;
                        }
                    }
                }
            });
        }
    }
}

