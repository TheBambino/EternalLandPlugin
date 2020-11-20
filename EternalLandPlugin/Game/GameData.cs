using EternalLandPlugin.Account;
using MessagePack;
using System;
using System.Collections.Generic;
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
        public readonly static Dictionary<string, Skill> Skills = new Dictionary<string, Skill>();

        public static bool GetMapData(string name, out MapManager.MapData map)
        {
            map = null;
            if (!Map.ContainsKey(name)) return false;
            try
            {
                map = GetMapDataDirect(name).Result;
                if (map == null) return false;
                return true;
            }
            catch (Exception ex) { Log.Error(ex); map = null; return false; }
        }
        async static Task<MapManager.MapData> GetMapDataDirect(string name)
        {
            return await Task.Run(() =>
            {
                return MessagePackSerializer.Deserialize<MapManager.MapData>(Map[name], MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block));
            });
        }
    }
}

