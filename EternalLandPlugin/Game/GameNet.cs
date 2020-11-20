using EternalLandPlugin.Account;
using EternalLandPlugin.Net;
using Microsoft.Xna.Framework;
using OTAPI.Tile;
using System.Data;
using System.IO;
using System.IO.Streams;
using System.Linq;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Models.Projectiles;
using Projectile = EternalLandPlugin.Game.MapTools.Projectile;

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
                case PacketTypes.PlayerSpawn:
                    MuitiMapDataCheck(args);
                    break;
            }
        }
        public static void MuitiMapDataCheck(SendDataEventArgs args)
        {
            if (args.ignoreClient != -1 && args.ignoreClient != 255 && TShock.Players[args.ignoreClient] != null)
            {
                args.Handled = true;
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
                args.Handled = true;
                EternalLand.OnlineEPlayerWhoInMainMap.ForEach(e =>
                {
                    NetMessage.SendData((int)args.MsgId, e.Index, 255, args.text, args.number, args.number2, args.number3, args.number4, args.number5, args.number6, args.number7);
                });
            }
        }
        public async static void OnGetData(GetDataEventArgs args)
        {
            /* if (args.MsgID == PacketTypes.ItemOwner || args.MsgID == PacketTypes.RemoveItemOwner || args.MsgID == PacketTypes.PlayerUpdate || args.MsgID == PacketTypes.ItemDrop || args.MsgID == PacketTypes.Status)
             {
                 return;
             }*/
            var eplr = EternalLand.EPlayers[args.Msg.whoAmI];
            //Utils.Broadcast(args.MsgID);
            if (eplr != null)
            {
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
                                args.Handled = OnTileEdit(eplr, type, x, y, data, style, flag);
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
                                args.Handled = OnPlaceObject(eplr, x, y, type, style, alternate, random, direction).Result;
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
                            case PacketTypes.PlaceChest:
                                action = reader.ReadByte();
                                x = reader.ReadInt16();
                                y = reader.ReadInt16();
                                style = reader.ReadInt16();
                                args.Handled = OnPlaceChest(eplr, action, x, y, style);
                                break;
                            case PacketTypes.ChestItem:
                                int id = (int)reader.ReadInt16();
                                int slot = (int)reader.ReadByte();
                                int stack = (int)reader.ReadInt16();
                                int prefix = (int)reader.ReadByte();
                                type = (int)reader.ReadInt16();
                                args.Handled = OnUpdateChestItem(eplr, id, slot, stack, prefix, type);
                                break;
                            case PacketTypes.SignRead:
                                x = (int)reader.ReadInt16();
                                y = (int)reader.ReadInt16();
                                args.Handled = OnRequireSign(eplr, x, y);
                                break;
                            case PacketTypes.SignNew:
                                id = (int)reader.ReadInt16();
                                x = (int)reader.ReadInt16();
                                y = (int)reader.ReadInt16();
                                string text = reader.ReadString();
                                int playerid = (int)reader.ReadByte();
                                args.Handled = OnUpdateSign(eplr, id, x, y, text, playerid);
                                break;
                            case PacketTypes.HitSwitch:
                                x = (int)reader.ReadInt16();
                                y = (int)reader.ReadInt16();
                                args.Handled = OnHitSwitch(eplr, x, y);
                                break;
                            case PacketTypes.PlayerHurtV2:
                                int num217 = (int)reader.ReadByte();
                                if (Main.netMode == 2 && args.Msg.whoAmI != num217 && (!Main.player[num217].hostile || !Main.player[args.Msg.whoAmI].hostile))
                                {
                                    args.Handled = true;
                                    return;
                                }
                                PlayerDeathReason playerDeathReason = PlayerDeathReason.FromReader(reader);
                                int damage = (int)reader.ReadInt16();
                                direction = (int)(reader.ReadByte() - 1);
                                BitsByte bitsByte = reader.ReadByte();
                                bool crit = bitsByte[0];
                                bool pvp = bitsByte[1];
                                int num219 = (int)reader.ReadSByte();
                                OnPlayerHurt(eplr, num217, damage, direction, crit, pvp, playerDeathReason, num219);
                                args.Handled = true;
                                break;
                            case PacketTypes.PlayerHp:
                                reader.ReadByte();
                                int life = reader.ReadInt16();
                                OnPlayerHeal(eplr, life);
                                break;
                            case PacketTypes.ProjectileNew:
                                id = reader.ReadInt16();

                                Vector2 pos = reader.ReadVector2();
                                Vector2 vel = reader.ReadVector2();
                                byte owner = reader.ReadByte();
                                type = reader.ReadInt16();
                                NewProjectileData newProjectileData = new NewProjectileData((byte)reader.ReadByte());
                                float[] array = new float[Terraria.Projectile.maxAI];
                                for (int i = 0; i < Terraria.Projectile.maxAI; i++)
                                {
                                    array[i] = ((!newProjectileData.AI[i]) ? 0f : reader.ReadSingle());
                                }
                                short dmg = (short)(newProjectileData.HasDamage ? reader.ReadInt16() : 0);
                                float knockback = newProjectileData.HasKnockback ? reader.ReadSingle() : 0f;
                                int origin = 0;
                                if (newProjectileData.HasOriginalDamage)
                                {
                                    origin = reader.ReadInt16();
                                }
                                int uuid = 1000;
                                if (newProjectileData.HasUUUID)
                                {
                                    uuid = reader.ReadInt16();
                                }
                                args.Handled = OnReceiveNewProj(eplr, pos, vel, id, uuid, type, origin, dmg, knockback, owner, array).Result;
                                break;
                            case PacketTypes.ItemDrop:
                                int index = (int)reader.ReadInt16();
                                Vector2 position = reader.ReadVector2();
                                Vector2 velocity = reader.ReadVector2();
                                stack = (int)reader.ReadInt16();
                                prefix = (int)reader.ReadByte();
                                bool nodelay = reader.ReadByte() == 1;
                                type = (int)reader.ReadInt16();
                                args.Handled = OnItemUpdate(eplr, index, position, velocity, stack, prefix, nodelay, type).Result;
                                break;
                            case PacketTypes.ItemOwner:
                                index = (int)reader.ReadInt16();
                                owner = reader.ReadByte();
                                if (index == 0) StatusSender.GetPingPakcet(args.Msg.whoAmI);
                                args.Handled = OnItemOwner(eplr, index, owner);
                                break;
                        }
                    }
                }
            }
        }
        public static void OnChat(ServerChatEventArgs args)
        {
            if (UserManager.TryGetEPlayeFromName(Main.player[args.Who].name, out var eplr))
            {
                args.Handled = true;
                string text = args.Text;
                if (!text.StartsWith("/") && !text.StartsWith(".") && text != "")
                {
                    eplr.Broadcast($"{eplr.Name} : {text}", default, false, false);
                    eplr.Statistic.Chat += text.Length;
                }
                else
                {
                    Commands.HandleCommand(eplr.tsp, text);
                }
            }
        }
        public static void OnPlayerHurt(EPlayer eplr, int hurtid, int damage, int direction, bool crit, bool pvp, PlayerDeathReason reason, int cooldown)
        {
            if (crit) eplr.SendCombatMessage("暴击!", Color.CornflowerBlue, true);
            if (Main.player[hurtid].statLife - damage > 0)
            {
                if (pvp)
                {
                    var hurter = EternalLand.EPlayers[reason._sourcePlayerIndex];
                    if (hurter != null)
                    {
                        hurter.Statistic.Damage_Player += damage;
                        hurter.Statistic.CritCount += crit ? 1 : 0;
                        hurter.Statistic.KillPlayers += eplr.plr.statLife - damage > 0 ? 0 : 1;
                    }
                    eplr.Statistic.GetDamage += damage;
                }
                if (Main.player[hurtid] != null) Main.player[hurtid].Hurt(reason, damage, direction, pvp, true, crit, cooldown);
                NetMessage.SendPlayerHurt(hurtid, reason, damage, direction, crit, pvp, cooldown, -1, eplr.Index);
            }
            else if (Main.player[hurtid] != null)
            {
                var deathreason = reason.GetDeathText(eplr.Name);
                Item item = null;
                if (reason._sourceItemType > 0)
                {
                    item = new Item();
                    item.SetDefaults(reason._sourceItemType);
                    item.prefix = (byte)reason._sourceItemPrefix;
                }
                deathreason._text = (item == null ? "" : TShock.Utils.ItemTag(item)) + " " + deathreason._text;
                Main.player[hurtid].KillMe(reason, damage, direction, pvp);
                NetMessage._currentPlayerDeathReason = new PlayerDeathReason();
                BitsByte bb = 0;
                bb[0] = pvp;
                NetMessage.SendData(118, -1, -1, null, hurtid, (float)damage, (float)direction, (float)bb, 0, 0, 0);
                UserManager.TryGetEPlayeFromName(Main.player[hurtid].name, out var hurteplr);
                hurteplr.Statistic.GetDamage += eplr.plr.statLife;
                hurteplr.Broadcast(deathreason._text, new Color(195, 83, 83), false);
                hurteplr.Statistic.Dead++;

            }
        }
        public static void OnNpcStrike(NpcStrikeEventArgs args)
        {
            if (UserManager.TryGetEPlayeFromName(args.Player.name, out EPlayer eplr))
            {
                eplr.Statistic.Damage_NPC += args.Damage;
                if (args.Critical) eplr.Statistic.CritCount++;
            }
        }
        public async static Task<bool> OnReceiveNewProj(EPlayer eplr, Vector2 position, Vector2 velocity, int identity, int uuid, int type, int origindamage, int damage, float knockback, int owner, float[] AI)
        {
            return await Task.Run(() =>
            {
                if (MapManager.GetMapFromUUID(eplr.MapUUID, out var map))
                {

                    if (uuid != 1000)
                    {
                        if (map.Proj[uuid] == null)
                        {
                            map.Proj[uuid] = CreateProjectile(map, position, velocity, identity, uuid, type, origindamage, damage, knockback, owner, AI);
                            map.Proj[uuid].SetDefault(type);
                        }
                        map.Proj[uuid].active = true;
                        map.Proj[uuid].velocity = velocity;
                        map.Proj[uuid].position = position;
                        map.Proj[uuid].ai = AI;
                        map.Proj[uuid].damage = damage;
                        map.Proj[uuid].originalDamage = origindamage;
                        map.Proj[uuid].knockBack = knockback;
                    }
                    else
                    {
                        int num = -1;
                        for (int i = 0; i < 1000; i++)
                        {
                            if (map.Proj[i] == null || !map.Proj[i].active || map.Proj[i].timeLeft < 500)
                            {
                                map.Proj[i] = CreateProjectile(map, position, velocity, identity, uuid, type, origindamage, damage, knockback, owner, AI);
                                num = i;
                                uuid = num;
                                break;
                            }
                        }
                        if (num == -1) map.Proj[0] = CreateProjectile(map, position, velocity, identity, uuid, type, origindamage, damage, knockback, owner, AI);
                    }


                    BitsByte bb = 0;
                    for (int num11 = 0; num11 < Terraria.Projectile.maxAI; num11++)
                    {
                        if (AI[num11] != 0f)
                        {
                            bb[num11] = true;
                        }
                    }
                    if (damage != 0)
                    {
                        bb[4] = true;
                    }
                    if (knockback != 0f)
                    {
                        bb[5] = true;
                    }
                    if (type > 0 && type < 950 && ProjectileID.Sets.NeedsUUID[type])
                    {
                        bb[7] = true;
                    }
                    if (origindamage != 0)
                    {
                        bb[6] = true;
                    }
                    var writer = new RawDataWriter().SetType(PacketTypes.ProjectileNew).PackInt16((short)identity).PackVector2(position).PackVector2(velocity).PackByte((byte)owner).PackInt16((short)type).PackByte(bb);
                    for (int num11 = 0; num11 < Terraria.Projectile.maxAI; num11++)
                    {
                        if (AI[num11] != 0f)
                        {
                            writer.PackSingle(AI[num11]);
                        }
                    }
                    if (damage != 0)
                    {
                        writer.PackInt16((short)damage);
                    }
                    if (knockback != 0f)
                    {
                        writer.PackSingle(knockback);
                    }
                    if (type > 0 && type < 950 && ProjectileID.Sets.NeedsUUID[type])
                    {
                        writer.PackInt16((short)uuid);
                    }
                    if (origindamage != 0)
                    {
                        writer.PackInt16((short)origindamage);
                    }
                    map.SendRawDataToPlayer(writer.GetByteData());
                    return true;
                }
                return false;
            });
        }
        public static Projectile CreateProjectile(MapManager.MapData map, Vector2 position, Vector2 velocity, int identity, int uuid, int type, int origindamage, int damage, float knockback, int owner, float[] AI)
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
            for (int num85 = 0; num85 < Terraria.Projectile.maxAI; num85++)
            {
                projectile.ai[num85] = AI[num85];
            }
            projectile.ProjectileFixDesperation(map);
            return projectile;
        }
        public static void OnReceiveKillProj(object o, GetDataHandlers.ProjectileKillEventArgs args)
        {
            var eplr = args.Player.EPlayer();
            if (eplr != null && eplr.IsInAnotherWorld)
            {
                eplr.Map.Proj.Where(p => p != null && p.owner == eplr.Index && p.identity == args.ProjectileIdentity).ForEach(p => { p.SetDefault(0); p.active = false; p.timeLeft = 0; });
                eplr.Map.SendDataToPlayer(PacketTypes.ProjectileDestroy, "EternalLand", args.ProjectileIndex, eplr.Index);
            }
        }
        public static void OnPlayerSpawn(object o, GetDataHandlers.SpawnEventArgs args)
        {
            var eplr = args.Player.EPlayer();
            if (eplr != null)
            {
                args.Handled = true;
                args.Player.TPlayer.Spawn(args.SpawnContext);
                args.Player.Teleport(eplr.SpawnX, eplr.SpawnY);
                if (eplr.IsInAnotherWorld) eplr.Map.SendRawDataToPlayer(new RawDataWriter().SetType(PacketTypes.PlayerSpawn).PackByte((byte)eplr.Index).PackInt16((short)eplr.SpawnX).PackInt16((short)eplr.SpawnY).PackInt32(TShock.Config.RespawnSeconds).PackByte((byte)2).GetByteData());
            }
        }
        public static bool OnPlayerHeal(EPlayer eplr, int life)
        {
            int lifechange = life - eplr.plr.statLife;
            eplr.Statistic.Heal += lifechange > 0 ? life : 0;
            return false;
        }
        public async static Task<bool> OnItemUpdate(EPlayer eplr, int index, Vector2 position, Vector2 velocity, int stack, int prefix, bool nodelay, int type)
        {
            return await Task.Run(() =>
            {
                if (eplr.IsInAnotherWorld)
                {
                    var map = eplr.Map;
                    if (map.timeItemSlotCannotBeReusedFor[index] > 0)
                    {
                        return true;
                    }
                    if (type == 0)
                    {
                        map.Items[index].active = false;
                        map.Items[index].type = 0;
                        map.SendRawDataToPlayer(new RawDataWriter().SetType(PacketTypes.ItemDrop).PackInt16((short)index).PackVector2(position).PackVector2(velocity).PackInt16((short)stack).PackByte((byte)prefix).PackByte((byte)nodelay.ToInt()).PackInt16((short)type).GetByteData());
                        return true;
                    }
                    else
                    {
                        if (index == 400)
                        {
                            EItem item2 = new EItem();
                            item2.type = type;
                            index = map.NewItem((int)position.X, (int)position.Y, item2.width, item2.height, item2.type, stack, true, 0, nodelay, false);
                            map.SendRawDataToPlayer(new RawDataWriter().SetType(PacketTypes.ItemDrop).PackInt16((short)index).PackVector2(position).PackVector2(velocity).PackInt16((short)stack).PackByte((byte)prefix).PackByte((byte)nodelay.ToInt()).PackInt16((short)type).GetByteData());
                            if (!nodelay)
                            {
                                map.Items[index].ownIgnore = eplr.Index;
                                map.Items[index].ownTime = 100;
                            }
                            map.Items[index].FindOwner(index, map);
                            return true;
                        }
                        map.Items[index].type = type;
                        map.Items[index].prefix = prefix;
                        map.Items[index].stack = stack;
                        map.Items[index].position = position;
                        map.Items[index].velocity = velocity;
                        map.Items[index].active = true;
                        map.Items[index].playerIndexTheItemIsReservedFor = Main.myPlayer;
                        map.GetAllPlayers().ForEach(e =>
                        {
                            e.SendRawData(new RawDataWriter().SetType(PacketTypes.ItemDrop).PackInt16((short)index).PackVector2(position).PackVector2(velocity).PackInt16((short)stack).PackByte((byte)prefix).PackByte((byte)nodelay.ToInt()).PackInt16((short)type).GetByteData());
                        });
                        return true;
                    }
                }
                return false;
            });
        }
        public static bool OnItemOwner(EPlayer eplr, int index, int owner)
        {
            if (eplr.IsInAnotherWorld)
            {
                var map = eplr.Map;
                if (map.Items[index].playerIndexTheItemIsReservedFor != eplr.Index) return true;
                map.Items[index].playerIndexTheItemIsReservedFor = owner;
                if (owner == Main.myPlayer)
                {
                    map.Items[index].keepTime = 15;
                }
                else
                {
                    map.Items[index].keepTime = 0;
                }
                map.Items[index].playerIndexTheItemIsReservedFor = 255;
                map.Items[index].keepTime = 15;
                eplr.Map.SendRawDataToPlayer(new RawDataWriter().SetType(PacketTypes.ItemOwner).PackInt16((short)index).PackByte((byte)map.Items[index].playerIndexTheItemIsReservedFor).GetByteData());

                return true;
            }
            return false;
        }
        public static bool OnTileEdit(EPlayer eplr, int type, int x, int y, int data, int style, bool flag)
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
                    TileEdit(eplr, type, x, y, data, style, flag);
                    return true;
                }
            }
            return false;
        }
        async static void TileEdit(EPlayer eplr, int type, int x, int y, int data, int style, bool flag)
        {
            await Task.Run(() =>
            {
                try
                {
                    var map = eplr.Map;
                    if (map.IsInMap(x, y))
                    {
                        map.GetAllPlayers().ForEach(p =>
                        {
                            if (p.ID != eplr.ID) p.SendData(PacketTypes.Tile, "EternalLand", (int)type, (float)x, (float)y, (float)data, style);

                        });
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
                        }
                        if (type == 19)
                        {
                            map.SetCurrentUser(eplr.Index);
                            map.Actuate(x, y);
                            map.SetCurrentUser(-1);
                        }
                        if (type == 20)
                        {
                            if (WorldGen.InWorld(x, y, 2))
                            {
                                int type3 = (int)map[x, y].type;
                                map.KillTile(x, y, flag, false, false);
                                data = ((short)(((int)map[x, y].type == type3) ? 1 : 0));
                                if ((type == 1 || type == 21) && TileID.Sets.Falling[(int)data])
                                {
                                    map.GetAllPlayers().ForEach(e => MapManager.SendSquare(1, x, y, map, e));
                                }
                            }
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
                            }
                        }
                    }
                    else
                    {
                        //eplr.SendErrorEX("超出地图范围.");
                        map.KillTile(x, y);
                        map.TileFrame(x, y);
                        eplr.SendData(PacketTypes.Tile, "", (int)GetDataHandlers.EditAction.KillActuator, x, y);
                        eplr.SendData(PacketTypes.Tile, "", (int)GetDataHandlers.EditAction.KillTile, x, y);
                        eplr.SendData(PacketTypes.Tile, "", (int)GetDataHandlers.EditAction.KillWall, x, y);
                        eplr.SendData(PacketTypes.Tile, "", (int)GetDataHandlers.EditAction.KillWire, x, y);
                        eplr.SendData(PacketTypes.Tile, "", (int)GetDataHandlers.EditAction.KillWire2, x, y);
                        eplr.SendData(PacketTypes.Tile, "", (int)GetDataHandlers.EditAction.KillWire3, x, y);
                        eplr.SendData(PacketTypes.Tile, "", (int)GetDataHandlers.EditAction.KillWire4, x, y);
                    }
                    //Utils.Broadcast(type + " " + data + " " + style);

                }
                catch { }
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
        public static bool OnUpdateChestItem(EPlayer eplr, int id, int slot, int stack, int prefix, int type)
        {
            if (eplr.IsInAnotherWorld)
            {
                var map = eplr.Map;
                if (map.Chest.Count >= id)
                {
                    if (map.Chest[id].item[slot] == null)
                    {
                        map.Chest[id].item[slot] = new Item();
                    }
                    map.Chest[id].item[slot].type = type;
                    map.Chest[id].item[slot].prefix = prefix;
                    map.Chest[id].item[slot].stack = stack;
                }
                return true;
            }
            return false;
        }
        public static bool OnRequireSign(EPlayer eplr, int x, int y)
        {
            if (eplr.IsInAnotherWorld)
            {
                var map = eplr.Map;
                int id = map.ReadSign(x, y, true);
                if (id >= 0)
                {
                    var sign = map.Sign[id];
                    eplr.SendRawData(new RawDataWriter().SetType(PacketTypes.SignNew).PackInt16((short)id).PackInt16((short)x).PackInt16((short)y).PackString(sign.text).PackByte((byte)eplr.Index).PackByte((byte)0).GetByteData());
                    return true;
                }
            }
            return false;
        }
        public static bool OnUpdateSign(EPlayer eplr, int id, int x, int y, string text, int playerid)
        {
            if (eplr.IsInAnotherWorld)
            {
                var map = eplr.Map;
                if (map.Sign.Count >= id)
                {
                    if (map.Sign[id].text != text)
                    {
                        map.Sign[id].text = text;
                        map.GetAllPlayers().ForEach(e =>
                        {
                            e.SendRawData(new RawDataWriter().SetType(PacketTypes.SignNew).PackInt16((short)id).PackInt16((short)x).PackInt16((short)y).PackString(text).PackByte((byte)eplr.Index).PackByte((byte)0).GetByteData());
                        });
                    }
                }
                else
                {
                    map.AddSign(x, y, text);
                    map.GetAllPlayers().ForEach(e =>
                    {
                        e.SendRawData(new RawDataWriter().SetType(PacketTypes.SignNew).PackInt16((short)id).PackInt16((short)x).PackInt16((short)y).PackString(text).PackByte((byte)eplr.Index).PackByte((byte)0).GetByteData());
                    });
                }
                return true;
            }
            return false;
        }
        public static bool OnPlaceChest(EPlayer eplr, byte action, int x, int y, int style)
        {
            if (eplr.IsInAnotherWorld)
            {
                var map = eplr.Map;
                if (Main.netMode == 2)
                {
                    if (action == 0)
                    {
                        int num106 = map.PlaceChest(x, y, 21, false, style);
                        if (num106 == -1)
                        {
                            map.SendDataToPlayer(34, eplr.Index, -1, null, (int)action, (float)x, (float)y, (float)style, num106);
                            map.NewItem(x * 16, y * 16, 32, 32, Chest.chestItemSpawn[style], 1, true, 0, false, false);
                            return true;
                        }
                        map.SendDataToPlayer(34, -1, -1, null, (int)action, (float)x, (float)y, (float)style, num106);
                        return true;
                    }
                    else if (action == 1 && map[x, y].type == 21)
                    {
                        ITile tile2 = map[x, y];
                        if (tile2.frameX % 36 != 0)
                        {
                            x--;
                        }
                        if (tile2.frameY % 36 != 0)
                        {
                            y--;
                        }
                        int number = Chest.FindChest(x, y);
                        map.KillTile(x, y, false, false, false);
                        if (!tile2.active())
                        {
                            map.SendDataToPlayer(34, -1, -1, null, (int)action, (float)x, (float)y, 0f, number);
                            return true;
                        }
                        return true;
                    }
                    else if (action == 2)
                    {
                        int num107 = map.PlaceChest(x, y, 88, false, style);
                        if (num107 == -1)
                        {
                            map.SendDataToPlayer(34, eplr.Index, -1, null, (int)action, (float)x, (float)y, (float)style, num107);
                            map.NewItem(x * 16, y * 16, 32, 32, Chest.dresserItemSpawn[style], 1, true, 0, false, false);
                            return true;
                        }
                        map.SendDataToPlayer(34, -1, -1, null, (int)action, (float)x, (float)y, (float)style, num107);
                        return true;
                    }
                    else if (action == 3 && map[x, y].type == 88)
                    {
                        ITile tile3 = map[x, y];
                        x -= (int)(tile3.frameX % 54 / 18);
                        if (tile3.frameY % 36 != 0)
                        {
                            y--;
                        }
                        int number2 = Chest.FindChest(x, y);
                        map.KillTile(x, y, false, false, false);
                        if (!tile3.active())
                        {
                            map.SendDataToPlayer(34, -1, -1, null, (int)action, (float)x, (float)y, 0f, number2);
                            return true;
                        }
                        return true;
                    }
                    else if (action == 4)
                    {
                        int num108 = map.PlaceChest(x, y, 467, false, style);
                        if (num108 == -1)
                        {
                            map.SendDataToPlayer(34, eplr.Index, -1, null, (int)action, (float)x, (float)y, (float)style, num108);
                            map.NewItem(x * 16, y * 16, 32, 32, Chest.chestItemSpawn2[style], 1, true, 0, false, false);
                            return true;
                        }
                        map.SendDataToPlayer(34, -1, -1, null, (int)action, (float)x, (float)y, (float)style, num108);
                        return true;
                    }
                    else
                    {
                        if (action != 5 || map[x, y].type != 467)
                        {
                            return true;
                        }
                        ITile tile4 = map[x, y];
                        if (tile4.frameX % 36 != 0)
                        {
                            x--;
                        }
                        if (tile4.frameY % 36 != 0)
                        {
                            y--;
                        }
                        int number3 = map.FindChest(x, y);
                        map.KillTile(x, y, false, false, false);
                        if (!tile4.active())
                        {
                            map.SendDataToPlayer(34, -1, -1, null, (int)action, (float)x, (float)y, 0f, number3);
                            return true;
                        }
                        return true;
                    }
                }
                return true;
            }
            return false;
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
        public static bool OnHitSwitch(EPlayer eplr, int x, int y)
        {
            if (eplr.IsInAnotherWorld)
            {
                var map = eplr.Map;
                map.SetCurrentUser(eplr.Index);
                map.HitSwitch(x, y);
                map.SetCurrentUser(-1);
                return true;
            }
            return false;
        }
    }
}

