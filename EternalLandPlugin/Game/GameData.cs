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
        public static Dictionary<string, EPlayerData> Character = new Dictionary<string, EPlayerData>();

        public static Dictionary<string, MapManager.MapData> Map = new Dictionary<string, MapManager.MapData>();
    }
}
