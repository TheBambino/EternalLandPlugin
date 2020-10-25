using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace EternalLandPlugin.Game
{
    class GameNet
    {
        public static void OnSendData(SendBytesEventArgs args)
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
                            TSPlayer.All.SendErrorMessage(id.ToString() + " " + position.X + " " + position.Y);
                            break;
                        case (PacketTypes)20:
                        case (PacketTypes)10:
                        case PacketTypes.Tile:
                            args.Handled = true;

                            break;
                    }
                }
            }
            catch { }
        }

        public static void OnNewProj(object o, GetDataHandlers.NewProjectileEventArgs args)
        {
            args.Player.SendSuccessMessage(args.Type.ToString());
        }
    }
}
