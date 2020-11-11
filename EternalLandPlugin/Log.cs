using TShockAPI;

namespace EternalLandPlugin
{
    class Log
    {
        public static void Info(object text)
        {
            TShock.Log.ConsoleInfo($"<EternalLand> " + text);
        }

        public static void Error(object text)
        {
            TShock.Log.ConsoleError($"<EternalLand> " + text);
        }
    }
}
