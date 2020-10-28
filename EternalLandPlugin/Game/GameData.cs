using EternalLandPlugin.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EternalLandPlugin.Game
{
    class GameData
    {
       

        public readonly static Dictionary<string, EPlayerData> Character = new Dictionary<string, EPlayerData>();

        public readonly static Dictionary<string, MapManager.MapData> Map = new Dictionary<string, MapManager.MapData>();

        public readonly static Dictionary<Guid, MapManager.MapData> ActiveMap = new Dictionary<Guid, MapManager.MapData>();
    }
}

