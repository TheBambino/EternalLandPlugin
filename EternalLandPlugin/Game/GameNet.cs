﻿using EternalLandPlugin.Account;
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
using Terraria.Localization;
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
        public static NetworkText ETText = NetworkText.FromLiteral("EternalLand");
        public static void OnSendData(SendDataEventArgs args)
        {
            switch (args.MsgId)
            {
                case PacketTypes.TileSendSquare:
                case PacketTypes.TileSendSection:
                case PacketTypes.Tile:
                case PacketTypes.TileFrameSection:
                case PacketTypes.TileGetSection:
                case PacketTypes.PlaceTileEntity:
                case PacketTypes.UpdateTileEntity:
                case PacketTypes.TileEntityDisplayDollItemSync:
                case PacketTypes.TileEntityHatRackItemSync:
                    if (args.ignoreClient != -1 && args.ignoreClient != 255 && TShock.Players[args.ignoreClient] != null)
                    {
                        args.Handled = true;
                        var epl = TShock.Players[args.ignoreClient].EPlayer();
                        if (epl != null && MapManager.GetMapFromUUID(epl.MapUUID, out var map))
                        {
                            map.SendDataToPlayer(args.MsgId, "EternalLand", args.number, args.number2, args.number3, args.number4, args.number5);
                            return;
                        }
                        else
                        {
                            EternalLand.OnlineEPlayer.ForEach(e =>
                            {
                                if (!e.IsInAnotherWorld)
                                {
                                    NetMessage.SendData((int)args.MsgId, e.Index, -1, ETText, args.number, args.number2, args.number3, args.number4, args.number5, args.number6, args.number7);
                                }
                            });
                        }
                    }
                    else if (args.remoteClient == -1)
                    {
                        args.Handled = true;
                        EternalLand.OnlineEPlayer.ForEach(e =>
                        {
                            if (!e.IsInAnotherWorld)
                            {
                                NetMessage.SendData((int)args.MsgId, e.Index, 255, ETText, args.number, args.number2, args.number3, args.number4, args.number5, args.number6, args.number7);
                            }
                        });
                    }
                    else if (args.ignoreClient != 255)
                    {
                        var epl = TShock.Players[args.remoteClient].EPlayer();
                        if (epl != null && epl.IsInAnotherWorld)
                        {
                            args.Handled = true;
                            return;
                        }
                    }
                    break;
                case PacketTypes.ProjectileNew:
                    MuitiMapDataCheck(args);
                    break;
                case PacketTypes.ProjectileDestroy:
                    /*var proj = Main.projectile[args.number];
                    if (args.number < 255 && args.number > -1 && proj.owner != 255)
                    {                            
                        var eplr = TShock.Players[proj.owner].EPlayer();
                        if (eplr != null && eplr.IsInAnotherWorld) args.Handled = true;
                    }*/
                    break;
                case PacketTypes.ItemDrop:
                case PacketTypes.UpdateItemDrop:
                    MuitiMapDataCheck(args);
                    break;
                case PacketTypes.PlaceObject:
                    MuitiMapDataCheck(args);
                    break;
            }
        }
        public async static void OnGetData(GetDataEventArgs args)
        {
            if (args.MsgID == PacketTypes.ItemOwner || args.MsgID == PacketTypes.RemoveItemOwner || args.MsgID == PacketTypes.PlayerUpdate || args.MsgID == PacketTypes.ItemDrop || args.MsgID == PacketTypes.Status)
            {
                return;
            }
            var eplr = EternalLand.EPlayers[args.Msg.whoAmI];
            //Utils.Broadcast(args.MsgID);
            using (MemoryStream r = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length - 1))
            {
                using (var reader = new BinaryReader(r))
                {

                    switch (args.MsgID)
                    {
                        case PacketTypes.Tile:
                            int type = reader.ReadByte();
                            int x = (int)reader.ReadInt16();
                            int y = (int)reader.ReadInt16();
                            short data = reader.ReadInt16();
                            int style = (int)reader.ReadByte();
                            bool flag = data == 1;
                            args.Handled = true;
                            await OnTileEdit(eplr, type, x, y, data, style, flag);
                            break;
                        case PacketTypes.PlaceObject:
                            x = (int)reader.ReadInt16();
                            y = (int)reader.ReadInt16();
                            type = reader.ReadInt16();
                            style = (int)reader.ReadInt16();
                            int alternate = (int)reader.ReadByte();
                            int random = (int)reader.ReadSByte();
                            int direction;
                            if (reader.ReadBoolean())
                            {
                                direction = 1;
                            }
                            else
                            {
                                direction = -1;
                            }
                            args.Handled = await OnPlaceObject(eplr, x, y, type, style, alternate, random, direction);
                            break;
                        case PacketTypes.DoorUse:
                            byte action = reader.ReadByte();
                            x = (int)reader.ReadInt16();
                            y = (int)reader.ReadInt16();
                            if (!WorldGen.InWorld(x, y, 3))
                            {
                                return;
                            }
                            direction = (reader.ReadByte() == 0) ? -1 : 1;
                            args.Handled = await OnDoorUse(eplr, x, y, action, direction);
                            break;
                    }
                }
            }
        }
        public static void MuitiMapDataCheck(SendDataEventArgs args)
        {
            if (args.ignoreClient != -1 && args.ignoreClient != 255 && TShock.Players[args.ignoreClient] != null)
            {
                var epl = TShock.Players[args.ignoreClient].EPlayer();
                if (epl != null && MapManager.GetMapFromUUID(epl.MapUUID, out var map))
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
                        if (e != epl && !e.IsInAnotherWorld)
                        {
                            NetMessage.SendData((int)args.MsgId, e.Index, 255, args.text, args.number, args.number2, args.number3, args.number4, args.number5, args.number6, args.number7);
                        }
                    });
                }
            }
            else if (args.remoteClient == -1 && args.ignoreClient == -1)
            {
                EternalLand.OnlineEPlayer.ForEach(e =>
                {
                    if (!e.IsInAnotherWorld)
                    {
                        NetMessage.SendData((int)args.MsgId, e.Index, -1, args.text, args.number, args.number2, args.number3, args.number4, args.number5, args.number6, args.number7);
                    }
                });
            }
        }

        public static void OnReceiveNewProj(object o, GetDataHandlers.NewProjectileEventArgs args)
        {
            var eplr = args.Player.EPlayer();
            if (eplr != null)
            {
                //SendToAnotherPlayer(eplr, args);
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

        public async static Task<bool> OnTileEdit(EPlayer eplr, int type, int x, int y, int data, int style, bool flag)
        {
            return await Task.Run(() =>
            {
                if (eplr != null)
                {
                    #region 区域选择
                    if (eplr.SettingPoint != 0 && type == 0)
                    {
                        eplr.ChoosePoint[eplr.SettingPoint - 1] = new Point(x, y);
                        if (eplr.SettingPoint == 1)
                        {
                            eplr.SendEX($"已进行地图左上角选择 <{(x + "-" + y).ToColorful()}>. 请继续选择右下角.");
                            eplr.SettingPoint = 2;
                        }
                        else if (eplr.ChoosePoint[0].X >= x || eplr.ChoosePoint[0].Y >= y)
                        {
                            eplr.SendErrorEX($"第二个点位 <{(x + "-" + y).ToColorful()}> 与第一个点位 <{(eplr.ChoosePoint[0].X + "-" + eplr.ChoosePoint[0].Y).ToColorful()}> 坐标重合或未处于右下角, 请重新选择.");
                        }
                        else
                        {
                            eplr.SendEX($"已选择全部点位. 请使用 {"//map create <地图名>".ToColorful()} 进行地图创建.");
                            eplr.SettingPoint = 0;
                        }
                        return true;
                    }

                    #endregion
                    if (eplr.IsInAnotherWorld)
                    {
                        try
                        {
                            var map = eplr.Map;
                            if (map.IsInMap(x, y)) map.GetAllPlayers().ForEach(p =>
                            {
                                if (p.ID != eplr.ID) p.SendData(PacketTypes.Tile, "EternalLand", (int)type, (float)x, (float)y, (float)data, style);
                            });
                            else
                            {
                                eplr.SendErrorEX("超出地图范围.");
                                eplr.SendData(PacketTypes.Tile, "", (int)GetDataHandlers.EditAction.KillActuator, x, y);
                                eplr.SendData(PacketTypes.Tile, "", (int)GetDataHandlers.EditAction.KillTile, x, y);
                                eplr.SendData(PacketTypes.Tile, "", (int)GetDataHandlers.EditAction.KillWall, x, y);
                                eplr.SendData(PacketTypes.Tile, "", (int)GetDataHandlers.EditAction.KillWire, x, y);
                                eplr.SendData(PacketTypes.Tile, "", (int)GetDataHandlers.EditAction.KillWire2, x, y);
                                eplr.SendData(PacketTypes.Tile, "", (int)GetDataHandlers.EditAction.KillWire3, x, y);
                                eplr.SendData(PacketTypes.Tile, "", (int)GetDataHandlers.EditAction.KillWire4, x, y);
                                return true;
                            }
                            //Utils.Broadcast(type + " " + data + " " + style);
                            if (type == 0)
                            {
                                map.KillTile(x, y, flag, false, false);
                            }
                            if (type == 1)
                            {
                                map.PlaceTile(x, y, (int)data, false, true, -1, style);
                            }
                            if (type == 2)
                            {
                                map.KillWall(x, y, flag);
                            }
                            if (type == 3)
                            {
                                map.PlaceWall(x, y, (int)data, false);
                            }
                            if (type == 4)
                            {
                                map.KillTile(x, y, flag, false, true);
                            }
                            if (type == 5)
                            {
                                map.PlaceWire(x, y);
                            }
                            if (type == 6)
                            {
                                map.KillWire(x, y);
                            }
                            if (type == 7)
                            {
                                map.PoundTile(x, y);
                            }
                            if (type == 8)
                            {
                                map.PlaceActuator(x, y);
                            }
                            if (type == 9)
                            {
                                map.KillActuator(x, y);
                            }
                            if (type == 10)
                            {
                                map.PlaceWire2(x, y);
                            }
                            if (type == 11)
                            {
                                map.KillWire2(x, y);
                            }
                            if (type == 12)
                            {
                                map.PlaceWire3(x, y);
                            }
                            if (type == 13)
                            {
                                map.KillWire3(x, y);
                            }
                            if (type == 14)
                            {
                                map.SlopeTile(x, y, (int)data, false);
                            }
                            if (type == 15)
                            {
                                map.FrameTrack(x, y, true, false);
                            }
                            if (type == 16)
                            {
                                map.PlaceWire4(x, y);
                            }
                            if (type == 17)
                            {
                                map.KillWire4(x, y);
                            }
                            if (type == 18)
                            {
                                map.SetCurrentUser(eplr.Index);
                                map.PokeLogicGate(x, y);
                                map.SetCurrentUser(-1);
                                return true;
                            }
                            if (type == 19)
                            {
                                map.SetCurrentUser(eplr.Index);
                                map.Actuate(x, y);
                                map.SetCurrentUser(-1);
                                return true;
                            }
                            if (type == 20)
                            {
                                if (!WorldGen.InWorld(x, y, 2))
                                {
                                    return true;
                                }
                                int type3 = (int)map[x, y].type;
                                map.KillTile(x, y, flag, false, false);
                                data = ((short)(((int)map[x, y].type == type3) ? 1 : 0));
                                if (Main.netMode == 2)
                                {
                                    if ((type == 1 || type == 21) && TileID.Sets.Falling[(int)data])
                                    {
                                        map.GetAllPlayers().ForEach(e => MapManager.SendSquare(1, x, y, map, e));

                                        return true;
                                    }
                                }
                                return true;
                            }
                            else
                            {
                                if (type == 21)
                                {
                                    map.ReplaceTile(x, y, (ushort)data, style);
                                }
                                if (type == 22)
                                {
                                    map.ReplaceWall(x, y, (ushort)data);
                                }
                                if (type == 23)
                                {
                                    map.SlopeTile(x, y, (int)data, false);
                                    map.PoundTile(x, y);
                                }
                                if ((type == 1 || type == 21) && TileID.Sets.Falling[(int)data])
                                {
                                    map.GetAllPlayers().ForEach(e => MapManager.SendSquare(1, x, y, map, e));
                                    return true;
                                }
                            }
                            return true;
                        }
                        catch { return true; }
                    }
                }
                return false;
            });
        }
        public async static Task<bool> OnPlaceObject(EPlayer eplr, int x, int y, int type, int style, int alternate, int random, int direction)
        {
            return await Task.Run(() =>
            {
                if (eplr != null)
                {
                    if (!WorldGen.InWorld(x, y, 10))
                    {
                        return true;
                    }
                    if (eplr.IsInAnotherWorld)
                    {
                        try
                        {
                            var map = eplr.Map;
                            map.PlaceObject(x, y, type, false, style, alternate, random, direction);
                            map.GetAllPlayers().ForEach(e => { if (e != eplr) e.SendData(PacketTypes.PlaceObject, "EternalLand", x, y, type, style, alternate, random, direction); });
                        }
                        catch { }
                    }
                    else
                    {
                        EternalLand.OnlineEPlayerWhoInMainMap.ForEach(e => NetMessage.SendData(79, e.Index, 255, ETText, x, y, type, style, alternate, random, direction));

                    }
                    return true;
                }
                return false;
            });
        }
        public async static Task<bool> OnDoorUse(EPlayer eplr, int x, int y, int action, int direction)
        {
            return await Task.Run(() =>
            {
                if (eplr != null && eplr.IsInAnotherWorld)
                {
                    var map = eplr.Map;
                    if (action == 0)
                    {
                        map.OpenDoor(x, y, direction);
                    }
                    else if (action == 1)
                    {
                        map.CloseDoor(x, y, true);
                    }
                    else if (action == 2)
                    {
                        map.ShiftTrapdoor(x, y, direction == 1, 1);
                    }
                    else if (action == 3)
                    {
                        map.ShiftTrapdoor(x, y, direction == 1, 0);
                    }
                    else if (action == 4)
                    {
                        map.ShiftTallGate(x, y, false, true);
                    }
                    else if (action == 5)
                    {
                        map.ShiftTallGate(x, y, true, true);
                    }
                    if (Main.netMode == 2)
                    {
                        map.SendDataToPlayer(19, -1, -1, null, (int)action, (float)x, (float)y, (float)((direction == 1) ? 1 : 0));
                    }
                }
                else
                {
                    EternalLand.OnlineEPlayerWhoInMainMap.ForEach(e => e.SendData(PacketTypes.DoorUse, "EternalLand", action, x, y, direction));
                }
                return true;
            });
        }
        public static void OnChestOpen(object o, GetDataHandlers.ChestOpenEventArgs args)
        {
            var eplr = args.Player.EPlayer();
            if (eplr != null && eplr.IsInAnotherWorld)
            {
                args.Handled = true;
                var map = eplr.Map;
                if (Main.netMode != 2)
                {
                    return;
                }
                int x = args.X;
                int y = args.Y;
                int chestid = map.FindChest(x, y);
                if (chestid <= -1 || map.UsingChest(chestid) != -1)
                {
                    return;
                }
                for (int num95 = 0; num95 < 40; num95++)
                {
                    eplr.SendRawData(new RawDataWriter().SetType(PacketTypes.ChestItem).PackInt16((short)chestid).PackByte((byte)num95).PackInt16((short)map.Chest[chestid].item[num95].stack).PackByte((byte)map.Chest[chestid].item[num95].prefix).PackInt16((short)map.Chest[chestid].item[num95].type).GetByteData());
                }
                eplr.SendRawData(new RawDataWriter().SetType(PacketTypes.ChestOpen).PackInt16((short)chestid).PackInt16((short)x).PackInt16((short)y).PackByte((byte)(map.Chest[chestid].name.Length == 0 || map.Chest[chestid].name.Length > 20 ? 255 : map.Chest[chestid].name.Length)).PackString(map.Chest[chestid].name).GetByteData());
                Main.player[eplr.Index].chest = chestid;
                map.SendDataToPlayer(80, -1, eplr.Index, null, eplr.Index, (float)chestid);
                map.SetCurrentUser(eplr.Index);
                map.HitSwitch(x, y);
                map.SetCurrentUser(-1);
            }
        }
        public static void OnLiquidSet(object o, GetDataHandlers.LiquidSetEventArgs args)
        {
            var eplr = args.Player.EPlayer();
            if (eplr != null && eplr.IsInAnotherWorld)
            {
                var map = eplr.Map;
                args.Handled = true;
                int x = args.TileX;
                int y = args.TileY;
                byte liquid = (byte)args.Type;
                byte liquidType = args.Amount;
                if (Main.netMode == 2 && Netplay.SpamCheck)
                {
                    int num88 = eplr.Index;
                    int num89 = (int)(Main.player[num88].position.X + (float)(Main.player[num88].width / 2));
                    int num90 = (int)(Main.player[num88].position.Y + (float)(Main.player[num88].height / 2));
                    int num91 = 10;
                    int num92 = num89 - num91;
                    int num93 = num89 + num91;
                    int num94 = num90 - num91;
                    int num95 = num90 + num91;
                    if (x < num92 || x > num93 || y < num94 || y > num95)
                    {
                        NetMessage.BootPlayer(eplr.Index, NetworkText.FromKey("Net.CheatingLiquidSpam"));
                        return;
                    }
                }
                if (map[x, y] == null)
                {
                    map[x, y] = map.CreateTile();
                }
                lock (map[x, y])
                {
                    map[x, y].liquid = liquid;
                    map[x, y].liquidType(liquidType);
                    map.SquareTileFrame(x, y);
                }
            }
        }
    }
}

