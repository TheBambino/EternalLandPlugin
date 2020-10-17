using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace EternalLandPlugin
{
    class Log
    {
        public static void Info(string text)
        {
            TShock.Log.ConsoleInfo($"<EternalLand> " + text);
        }

        public static void Error(string text)
        {
            TShock.Log.ConsoleError($"<EternalLand> " + text);
        }
    }
}
