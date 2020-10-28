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
            var buffer = args.Buffer;
            using var reader = new BinaryReader(new MemoryStream(buffer));
            reader.BaseStream.Position = 2L;
            try
            {
                PacketTypes type = (PacketTypes)reader.ReadInt16();
                if (type != PacketTypes.Status && type != PacketTypes.RemoveItemOwner && (int)type <= 140) TSPlayer.All.SendErrorMessage(type.ToString());
                if (reader.BaseStream.Length > 26)
                {
                    reader.BaseStream.Position = 26L;
                    switch (type)
                    {
                        case PacketTypes.ProjectileNew:
                            int id = reader.ReadInt16();
                            Microsoft.Xna.Framework.Vector2 position = reader.ReadVector2();
                            //TSPlayer.All.SendErrorMessage(id.ToString() + " " + position.X + " " + position.Y);
                            break;
                        case (PacketTypes)20:
                        case (PacketTypes)10:
                        case PacketTypes.Tile:
                            args.Handled = true;
                            EternalLand.OnlineEPlayer.Where(e => !e.gameInfo.IsInAnotherWorld).ForEach(eplr =>
                            {

                            });
                            break;
                    }
                }
            }
            catch { }
        }

        public static void OnSendData(SendDataEventArgs args)
        {

        }

        public static void OnReceiveNewProj(object o, GetDataHandlers.NewProjectileEventArgs args)
        {
            if (args.Player.Account != null)
            {
                var eplr = args.Player.EPlayer();
                SendToAnotherPlayer(eplr, args);
            }
        }

        static void SendToAnotherPlayer(EPlayer eplr, GetDataHandlers.NewProjectileEventArgs args)
        {
            if (MapManager.GetMapFromUUID(eplr.gameInfo.MapUUID, out var map))
                args.Handled = true;
                args.Data.Position = 3L;
                short num = args.Data.ReadInt16();
                Vector2 pos = StreamExt.ReadVector2(args.Data);
                Vector2 vel = StreamExt.ReadVector2(args.Data);
                byte owner = args.Data.ReadInt8();
                short type = args.Data.ReadInt16();
                NewProjectileData newProjectileData = new NewProjectileData((byte)args.Data.ReadByte());
                float[] array = new float[Projectile.maxAI];
                for (int i = 0; i < Projectile.maxAI; i++)
                {
                    array[i] = ((!newProjectileData.AI[i]) ? 0f : args.Data.ReadSingle());
                }

                var uuid = map.GetNewProjUUID();
                var data = new RawDataWriter().SetType(PacketTypes.ProjectileNew);
                var binaryWriter = data.writer;
                binaryWriter.Write((short)uuid);
                binaryWriter.WriteVector2(args.Position);
                binaryWriter.WriteVector2(args.Velocity);
                binaryWriter.Write((byte)args.Owner);
                binaryWriter.Write((short)args.Type);
                BitsByte bb21 = (byte)0;
                for (int num17 = 0; num17 < Projectile.maxAI; num17++)
                {
                    if (array[num17] != 0f)
                    {
                        bb21[num17] = true;
                    }
                }
                if (args.Damage != 0)
                {
                    bb21[4] = true;
                }
                if (args.Knockback != 0f)
                {
                    bb21[5] = true;
                }
                if (args.Type > 0 && args.Type < 950 && ProjectileID.Sets.NeedsUUID[args.Type])
                {
                    bb21[7] = true;
                }
                binaryWriter.Write(bb21);
                for (int num18 = 0; num18 < Projectile.maxAI; num18++)
                {
                    if (bb21[num18])
                    {
                        binaryWriter.Write(array[num18]);
                    }
                }
                if (bb21[4])
                {
                    binaryWriter.Write((short)args.Damage);
                }
                if (bb21[5])
                {
                    binaryWriter.Write(args.Knockback);
                }
                if (bb21[6])
                {
                    //binaryWriter.Write((short)projectile.originalDamage);
                }
                if (bb21[7])
                {
                    binaryWriter.Write((short)uuid);
                }
                data.writer = binaryWriter;
                map.Player.ForEach(p => {
                    var eplr = UserManager.GetEPlayerFromID(p);
                    if (eplr != null)
                    {
                        eplr.SendRawData(data.GetByteData());
                    }
                });
            }
        }

        public static void OnReceiveKillProj(object o, GetDataHandlers.ProjectileKillEventArgs args)
        {
            args.Player.SendSuccessMessage(args.ProjectileIndex.ToString());
        }
    }
}
