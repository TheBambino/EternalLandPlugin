using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace EternalLandPlugin
{
    class ProcessCommand
    {
        public static void Bank(CommandArgs args)
        {
            var cmd = args.Parameters;
            var tsp = args.Player;
            var eplr = tsp.EPlayer();
            if(eplr == null)
            {
                tsp.SendErrorMessage("你尚未注册, 无法使用此命令.");
                return;
            }
            if (cmd.Count > 0)
            {
                switch (cmd[0])
                {
                    case "pay":
                    case "给":
                        if (cmd.Count >= 3 && long.TryParse(cmd[2], out long value))
                        {

                        }
                        else
                        {
                            tsp.SendErrorEX("格式错误. /bank pay <玩家名> <支付金额>");
                        }
                }
            }
            else
            {
                tsp.SendEX($"当前你的账户资产为 {eplr.Money}");
            }
        }
    }
}
