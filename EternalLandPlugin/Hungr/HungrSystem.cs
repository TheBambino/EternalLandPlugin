using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using EternalLandPlugin.Account;

namespace EternalLandPlugin.Hungr
{
    class HungrSystem
    {
        public static void OnEat(EPlayer eplr, int time)
        {

            int level = time / 9000 == 0 ? 1 : time % 18000 == 0 ? time / 18000 : time / 18000 + 1;
            ChangeHungr(eplr, (eplr.HungrValue / 1800) + level + 1);
        }

        public static void ChangeHungr(EPlayer eplr, double hungr)
        {
            eplr.HungrValue = (hungr >= 20 ? 36600 : hungr * 1800);
        }
    }
}
