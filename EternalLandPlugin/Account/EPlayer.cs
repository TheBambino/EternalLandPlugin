using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using TShockAPI;

namespace EternalLandPlugin.Account
{
    public class EPlayer
    {
        public void Save()
        {
            DataBase.SaveEPlayer(this);
        }

        public TSPlayer tSPlayer { get { return TShock.Players.SingleOrDefault(t => t != null && t.Account != null && t.Account.ID == ID); } }

        public int ID { get; set; }

        public long Money { get; set; }
        #region 各种功能函数
        public void SendCombatMessage(string msg, Color color = default)
        {
            color = color == default ? Color.White : color;
            Random random = new Random();
            tSPlayer.SendData(PacketTypes.CreateCombatTextExtended, msg, (int)color.PackedValue, tSPlayer.X + random.Next(-75, 75), tSPlayer.Y + random.Next(-50, 50));
        }
        #endregion
    }
}
