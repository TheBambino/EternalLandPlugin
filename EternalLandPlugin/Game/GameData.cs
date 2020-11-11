using EternalLandPlugin.Account;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EternalLandPlugin.Game
{
    class GameData
    {
        /*public class MapByte
        {
            public MapByte(byte[] map, byte[] tile)
            {
                Map = map;
                Tile = tile;
            }
            public byte[] Map { get; set; }
            public byte[] Tile { get; set; }
        }*/
        public readonly static Dictionary<string, EPlayerData> Character = new Dictionary<string, EPlayerData>();

        //public readonly static Dictionary<string, MapByte> Map = new Dictionary<string, MapByte>();
        public readonly static Dictionary<string, byte[]> Map = new Dictionary<string, byte[]>();

        public readonly static Dictionary<Guid, MapManager.MapData> ActiveMap = new Dictionary<Guid, MapManager.MapData>();

        public static MapManager.MapData GetMapData(string name)
        {
            try { return GetMapDataDirect(name).Result; }
            catch (Exception ex){ Log.Error(ex); return null; }
        }
        async static Task<MapManager.MapData> GetMapDataDirect(string name)
        {
            return await Task.Run(() => {               
                return MessagePackSerializer.Deserialize<MapManager.MapData>(Map[name], MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block));
            });
        }
    }
}

