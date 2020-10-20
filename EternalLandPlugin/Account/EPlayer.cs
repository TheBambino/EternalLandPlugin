using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace EternalLandPlugin.Account
{
    public class EPlayer
    {
        public EPlayer()
        {
            HungrTimer.Interval = 5000;
            HungrTimer.Elapsed += delegate
            {
                if (HungrValue == 0 && tsp != null && !tsp.GodMode && plr.statLife > 0)
                {
                    Life -= tsp.TPlayer.statLife - 10 < 0 ? tsp.TPlayer.statLife : 10;
                    NetMessage._currentPlayerDeathReason = Terraria.DataStructures.PlayerDeathReason.ByCustomReason($"{Name} 饿死了."); tsp.SendData((PacketTypes)117, "", tsp.Index, 10); SendCombatMessage("你饿的前胸贴后背!", Color.Red);
                }
            };
            HungrTimer.AutoReset = true;
            HungrTimer.Start();
            HealByHungrFull.Elapsed += delegate {
                if(HungrFull && plr.statLife < plr.statLifeMax2) tsp.Heal(5);
            };
            HealByHungrFull.Start();
        }
        public void Dispose()
        {
            HungrTimer.Dispose();
            PingChecker.Stop();
        }

        public override string ToString()
        {
            return Name;
        }

        public async void Save() => await DataBase.SaveEPlayer(this);

        public TSPlayer tsp { get { return Utils.GetTSPlayerFromID(ID) ?? new TSPlayer(-1); } }
        public Player plr { get { return tsp.Index == -1 ? new Player() : tsp.TPlayer; } }

        #region -- 玩家信息 --

        public int ID = -1;

        public int Index { get { return tsp.Index; } }

        public string Name = "Unknown";

        [ShouldSave]
        public long Money = -1;

        int _life = -1;
        public int Life
        {
            get { return _life; }
            set
            {
                _life = value <= 0 ? -1 : tsp.GodMode ? tsp.TPlayer.statLifeMax2 : value;
                tsp.TPlayer.statLife = _life;
                NetMessage.SendData(16, -1, -1, null, tsp.Index);
            }
        }

        [ShouldSave]
        public long MobKillCount = 0;

        [ShouldSave]
        public long BossKillCount = 0;

        [ShouldSave]
        public int DeathCount = 0;

        public bool IsLifeFull => plr.statLifeMax2 == plr.statLife;
        #endregion

        #region -- 功能性字段 -- 

        public Stopwatch PingChecker = new Stopwatch();

        public long ping = -1;

        public bool Online = false;
        #region 饱食度相关字段

        double _hungrvalue = 36000;
        [ShouldSave]
        public double HungrValue
        {
            get { return _hungrvalue; }
            set
            {
                if (CanEat)
                {
                    CanEat = false;
                    _hungrvalue = value;
                }
            }
        }
        public int HungrLevel => HungrValue % 1800 == 0 ? HungrValue == 0 ? 0 : (int)(HungrValue / 1800) : HungrValue == 0 ? 0 : (int)(HungrValue / 1800) + 1;
        public enum StatusType { Normal, Moving, Battle, Mining }
        public StatusType Status = StatusType.Normal;
        public Timer HungrTimer = new Timer();
        public Timer HealByHungrFull = new Timer() { Interval = 500, AutoReset = true};
        public int HungrCoolDown = 0;
        public bool CanEat = true;
        public bool HungrFull => HungrValue > 34200;
        #endregion

        #endregion

        #region -- 各种功能函数 --

        public override bool Equals(object obj)
        {
            return ID.ToString() == obj.ToString();
        }

        bool ExtendHungr = false;
        public bool Update()
        {
            try
            {
                //玩家状态更新
                if (plr.controlUseItem && plr.HeldItem.damage != -1 && plr.HeldItem.pick == 0 && plr.HeldItem.axe == 0) Status = StatusType.Battle;
                else if (plr.controlUseItem && (plr.HeldItem.pick != 0 || plr.HeldItem.axe != 0)) Status = EPlayer.StatusType.Mining;
                else if (plr.controlDown || plr.controlJump || plr.controlUp || plr.controlLeft || plr.controlRight) Status = StatusType.Moving;
                else Status = EPlayer.StatusType.Normal;
                //禁用自动回复
                if (HungrValue == 0)
                {
                    plr.lifeRegen = 0;
                    plr.lifeRegenCount = 0;
                    plr.statLife = Life == -1 ? plr.statLife : Life;
                }
                //饱食度更新
                double normal = 1;
                double moving = 2;
                double mining = 3;
                double battle = 4;
                if (HungrValue != 0 && plr.statLife != 0)
                {
                    switch (Status)
                    {
                        case StatusType.Normal:
                            if (_hungrvalue >= normal) _hungrvalue -= IsLifeFull && HungrFull ? normal / 2 : HungrFull ? normal * 2 : normal;
                            else _hungrvalue = 0;
                            break;
                        case StatusType.Moving:
                            if (_hungrvalue >= moving) _hungrvalue -= IsLifeFull && HungrFull ? moving / 2 : HungrFull ? moving * 2 : moving;
                            else _hungrvalue = 0;
                            break;
                        case StatusType.Battle:
                            if (_hungrvalue >= battle) _hungrvalue -= IsLifeFull && HungrFull ? battle / 2 : HungrFull ? battle * 1.75 : battle;
                            else _hungrvalue = 0;
                            break;
                        case StatusType.Mining:
                            if (_hungrvalue >= mining) _hungrvalue -= IsLifeFull && HungrFull ? mining / 2 : HungrFull ? mining * 2 : mining;
                            else _hungrvalue = 0;
                            break;
                    }
                }

                if (!CanEat)
                {
                    if (HungrCoolDown < 17) HungrCoolDown++;
                    else
                    {
                        HungrCoolDown = 0;
                        CanEat = true;
                    }
                }
                

            }
            catch { }
            return false;
        }

        public void SendCombatMessage(string msg, Color color = default)
        {
            color = color == default ? Color.White : color;
            Random random = new Random();
            if (tsp != null) tsp.SendData(PacketTypes.CreateCombatTextExtended, msg, (int)color.PackedValue, tsp.X + random.Next(-75, 75), tsp.Y + random.Next(-50, 50));
        }

        public void GiveMoney(long value, string from = null, Color color = default, bool save = false)
        {
            if (from != null && Online) SendCombatMessage($"+ {value} <{from ?? "未知"}>", color == default ? Color.Yellow : color);
            Money += value;
            if (save) Save();
        }

        public bool TakeMoney(long value, string from = null, Color color = default, bool save = false)
        {
            if (Money >= value)
            {
                if (from != null && Online) SendCombatMessage($"- {value} <{from ?? "未知"}>", color == default ? Color.Yellow : color);
                Money -= value;
                if (save) Save();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Heal(int heal)
        {
            Life += heal;
            tsp.Heal(heal);
        }
        #endregion
    }
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class ShouldSave : Attribute
    {

    }
}
