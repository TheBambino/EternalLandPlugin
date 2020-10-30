﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using EternalLandPlugin.Game;
using EternalLandPlugin.Net;
using Newtonsoft.Json;
using Terraria;
using Terraria.Localization;
using TShockAPI;
using TShockAPI.Net;
using Timer = System.Timers.Timer;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;

namespace EternalLandPlugin.Account
{
    public class EPlayer : IDisposable
    {
        public EPlayer()
        {
            if (EternalLand.IsGameMode)
            {
                DynamicLoading = new Thread(new ThreadStart(delegate
                {
                    try
                    {
                        Thread.Sleep(500);
                        while (true)
                        {
                            if (GameInfo.IsInAnotherWorld)
                            {
                                try
                                {
                                    foreach (var key in LoadedChuck.Keys)
                                    {
                                        if (!LoadedChuck[key] && key.IntersectsWith(new Rectangle(TileX - 75, TileY - 75, 150, 150)))
                                        {
                                            SendChuck(key.X, key.Y, new MapManager.MapData(GameInfo.Map.GetChuck(key.X, key.Y).Result, key.X, key.Y));
                                            LoadedChuck[key] = true;
                                            Thread.Sleep(50);
                                        }
                                    }
                                }
                                catch { }
                            }
                            Thread.Sleep(100);
                        }
                    }
                    catch { }
                }));
            }
            else
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
                HealByHungrFull.Elapsed += delegate
                {
                    if (HungrFull && plr.statLife < plr.statLifeMax2) tsp.Heal(5);
                };
                HealByHungrFull.Start();
            }
        }
        public void Dispose()
        {
            PingChecker.Stop();
        }

        public override string ToString()
        {
            return Name;
        }

        public async void Save() => await DataBase.SaveEPlayer(this);

        public TSPlayer tsp { get { return UserManager.GetTSPlayerFromName(Name, out TSPlayer t) ? t : null; } }
        public Player plr { get { return tsp == null ? null : tsp.TPlayer; } }

        #region -- 玩家信息 --

        public int ID = -1;

        public int Index { get { return tsp.Index; } }

        public string Name = "Unknown";

        public int TileX = 0;

        public int TileY = 0;

        public int X = 0;

        public int Y = 0;

        public int SpawnX => GameInfo.IsInAnotherWorld ? GameInfo.TempCharacter == null ? GameInfo.Character.SpawnX * 16 : GameInfo.TempCharacter.SpawnX * 16: Main.spawnTileX * 16;

        public int SpawnY => GameInfo.IsInAnotherWorld ? GameInfo.TempCharacter == null ? GameInfo.Character.SpawnY * 16 - 48: GameInfo.TempCharacter.SpawnY * 16 - 48 : Main.spawnTileY * 16 - 48;
        [ShouldSave]
        public long Money = 0;

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

        public bool Online { get { return tsp != null; } }
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
        public Timer HealByHungrFull = new Timer() { Interval = 500, AutoReset = true };
        public int HungrCoolDown = 0;
        public bool CanEat = true;
        public bool HungrFull => HungrValue > 34200;
        #endregion

        #region 小游戏字段!

        [ShouldSave(Serializable = true)]
        public List<EItem> Bag => GameInfo.TempCharacter == null ? GameInfo.Character.Bag : GameInfo.TempCharacter.Bag;

        public GameInfo GameInfo = new GameInfo();

        public int SettingPoint = 0;

        public Point[] ChoosePoint = new Point[2];
        #endregion
        #endregion

        #region -- 各种功能函数 --

        #region 游戏相关
        public void SendBag(List<EItem> list = null)
        {
            list = list ?? (GameInfo.TempCharacter == null ? GameInfo.Character.Bag : GameInfo.TempCharacter.Bag);
            for (int i = 0; i < 260; i++)
            {
                var item = list[i] ?? new EItem();
                tsp.SendRawData(new RawDataWriter().SetType(PacketTypes.PlayerSlot).PackByte((byte)Index).PackInt16((short)i).PackInt16((short)item.Stack).PackByte((byte)item.Prefix).PackInt16((short)item.ID).GetByteData());
            }
        }

        void SendCharacterData(EPlayerData data, bool onlysendbag = false)
        {
            SendBag(data.Bag);
            if (!onlysendbag)
            {
                var plr = tsp.TPlayer;
                plr.statLife = data.Life;
                plr.statLifeMax = data.MaxLife;
                plr.statMana = data.Mana;
                plr.statManaMax = data.MaxMana;
                plr.SpawnX = data.SpawnX;
                plr.SpawnY = data.SpawnY;
                plr.skinVariant = data.skinVariant;
                plr.hair = data.hair;
                plr.hairDye = (byte)data.hairDye;
                plr.hairColor = (Color)data.hairColor;
                plr.pantsColor = (Color)data.pantsColor;
                plr.underShirtColor = (Color)data.underShirtColor;
                plr.shoeColor = (Color)data.shoeColor;
                plr.hideVisibleAccessory = data.hideVisibleAccessory;
                plr.skinColor = (Color)data.skinColor;
                plr.eyeColor = (Color)data.eyeColor;
                this.SendDataToAll(PacketTypes.PlayerInfo, "", Index);
                this.SendDataToAll(PacketTypes.PlayerHp, "", Index);
                this.SendDataToAll(PacketTypes.PlayerMana, "", Index);
            }
        }

        public bool ChangeCharacter(string name)
        {
            if (GameData.Character.ContainsKey(name))
            {
                GameInfo.TempCharacter = GameData.Character[name];
                SendCharacterData(GameData.Character[name]);
                UserManager.UpdateInfoToOtherPlayers(this);
                return true;
            }
            else
            {
                this.SendErrorEX($"<Internal Error> 无法变更玩家角色 => 未找到名为 {name} 的角色信息.");
                return false;
            }
        }

        public void SetToOriginCharacter()
        {
            SendCharacterData(GameInfo.Character);
            GameInfo.TempCharacter = null;
            UserManager.UpdateInfoToOtherPlayers(this);
        }
        public void BackToOriginMap()
        {
            GameInfo.MapUUID = Guid.Empty;
            tsp.SendData(PacketTypes.WorldInfo, "", 0, 0f, 0f, 0f, 0);
            int sectionX = Netplay.GetSectionX(0);
            int sectionX2 = Netplay.GetSectionX(Main.maxTilesX);
            int sectionY = Netplay.GetSectionY(0);
            int sectionY2 = Netplay.GetSectionY(Main.maxTilesY);
            for (int i = sectionX; i <= sectionX2; i++)
            {
                for (int j = sectionY; j <= sectionY2; j++)
                {
                    Netplay.Clients[Index].TileSections[i, j] = false;
                }
            }
            tsp.Teleport(SpawnX, SpawnY);
            UserManager.UpdateInfoToOtherPlayers(this);
            MapManager.SendProjectile(this);
        }
        public void JoinMap(Guid uuid)
        {
            if (uuid == Guid.Empty)
            {
                SendEX($"返回主世界.");
                BackToOriginMap();
                return;
            }
            GameInfo.MapUUID = uuid;
            SendEX($"传送至世界 [c/F97E63:{uuid.ToString().Split('-')[0]}], 可能会造成片刻卡顿.");
            int sectionX = Netplay.GetSectionX(0);
            int sectionX2 = Netplay.GetSectionX(Main.maxTilesX);
            int sectionY = Netplay.GetSectionY(0);
            int sectionY2 = Netplay.GetSectionY(Main.maxTilesY);
            for (int i = sectionX; i <= sectionX2; i++)
            {
                for (int j = sectionY; j <= sectionY2; j++)
                {
                    Netplay.Clients[Index].TileSections[i, j] = false;
                }
            }
            GameData.ActiveMap[uuid].Player.Add(ID);
            MapManager.ChangeWorldInfo(this);
            LoadedChuck.Clear();
            for (int ry = 0; ry < Main.maxTilesY / MapManager.MapData.ChuckSize; ry++)
            {
                for (int rx = 0; rx < Main.maxTilesX / MapManager.MapData.ChuckSize; rx++)
                {
                    LoadedChuck.Add(new Rectangle(rx * MapManager.MapData.ChuckSize, (ry) * MapManager.MapData.ChuckSize, MapManager.MapData.ChuckSize, MapManager.MapData.ChuckSize), false);
                }
            }
            UserManager.UpdateInfoToOtherPlayers(this);
            MapManager.SendProjectile(this);
            if (!DynamicLoading.IsAlive) DynamicLoading.Start();
        }

        readonly Dictionary<Rectangle, bool> LoadedChuck = new Dictionary<Rectangle, bool>();
        Thread DynamicLoading;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">将要发的送到的左上角X坐标</param>
        /// <param name="y">将要发的送到的左上角Y坐标</param>
        /// <param name="data"></param>
        void SendChuck(int x, int y, MapManager.MapData data)
        {
            MapManager.SendMap(this, data, x, y);
        }
        #endregion
        public override bool Equals(object obj)
        {
            return ID.ToString() == obj.ToString();
        }
        public bool Update()
        {
            try
            {
                if (!Online) return false;
                TileX = (int)(plr.position.X / 16);
                TileY = (int)(plr.position.Y / 16);
                X = (int)plr.position.X;
                Y = (int)plr.position.Y;
                //玩家状态更新
                if (plr.controlUseItem && plr.HeldItem.damage != -1 && plr.HeldItem.pick == 0 && plr.HeldItem.axe == 0) Status = StatusType.Battle;
                else if (plr.controlUseItem && (plr.HeldItem.pick != 0 || plr.HeldItem.axe != 0)) Status = EPlayer.StatusType.Mining;
                else if (plr.controlDown || plr.controlJump || plr.controlUp || plr.controlLeft || plr.controlRight) Status = StatusType.Moving;
                else Status = EPlayer.StatusType.Normal;

                if (EternalLand.IsGameMode)
                {

                }
                else
                {
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
        public void SendSuccessEX(object text)
        {
            tsp.SendMessage(Expansion.ServerPrefix + text, new Color(120, 194, 96));
        }
        public void SendInfoEX(object text)
        {
            tsp.SendMessage(Expansion.ServerPrefix + text, new Color(216, 212, 82));
        }
        public void SendErrorEX(object text)
        {
            tsp.SendMessage(Expansion.ServerPrefix + text, new Color(195, 83, 83));
        }
        public void SendEX(object text, Color color = default)
        {
            color = color == default ? new Color(212, 239, 245) : color;
            tsp.SendMessage(Expansion.ServerPrefix + text, color);
        }
        public void SendData(PacketTypes msgType, string text = "", int number = 0, float number2 = 0f, float number3 = 0f, float number4 = 0f, int number5 = 0)
        {
            if (UserManager.GetTSPlayerFromName(Name, out var tsp))
            {
                if (!tsp.RealPlayer || tsp.ConnectionAlive)
                {
                    NetMessage.SendData((int)msgType, tsp.Index, -1, NetworkText.FromLiteral(text), number, number2, number3, number4, number5);
                }
            }
        }
        public void SendRawData(byte[] data)
        {
            try { tsp.SendRawData(data); } catch { }
        }
        public void SendRawDataToAll(byte[] data)
        {
            try
            {
                EternalLand.OnlineEPlayer.ForEach(e => e.SendRawData(data));
            }
            catch { }
        }
        public void SendDataToAll(PacketTypes msgType, string text = "", int number = 0, float number2 = 0f, float number3 = 0f, float number4 = 0f, int number5 = 0)
        {
            if (UserManager.GetTSPlayerFromName(Name, out var tsp))
            {
                if (!tsp.RealPlayer || tsp.ConnectionAlive)
                {
                    NetMessage.SendData((int)msgType, -1, -1, NetworkText.FromLiteral(text), number, number2, number3, number4, number5);
                }
            }
        }
        public void SendProjectile(Projectile proj)
        {
            var data = new RawDataWriter().SetType(PacketTypes.ProjectileNew);
            var binaryWriter = data.writer;
            binaryWriter.Write(proj.projUUID);
            binaryWriter.WriteVector2(proj.position);
            binaryWriter.WriteVector2(proj.velocity);
            binaryWriter.Write(proj.owner);
            binaryWriter.Write(proj.type);
            BitsByte bb21 = (byte)0;
            for (int num17 = 0; num17 < Projectile.maxAI; num17++)
            {
                if (proj.ai[num17] != 0f)
                {
                    bb21[num17] = true;
                }
            }
            if (proj.damage != 0)
            {
                bb21[4] = true;
            }
            if (proj.knockBack != 0f)
            {
                bb21[5] = true;
            }
            if (proj.type > 0 && proj.type < 950 && Terraria.ID.ProjectileID.Sets.NeedsUUID[proj.type])
            {
                bb21[7] = true;
            }
            binaryWriter.Write(bb21);
            for (int num18 = 0; num18 < Projectile.maxAI; num18++)
            {
                if (bb21[num18])
                {
                    binaryWriter.Write(proj.ai[num18]);
                }
            }
            if (bb21[4])
            {
                binaryWriter.Write(proj.damage);
            }
            if (bb21[5])
            {
                binaryWriter.Write(proj.knockBack);
            }
            if (bb21[6])
            {
                binaryWriter.Write(proj.originalDamage);
            }
            if (bb21[7])
            {
                binaryWriter.Write(proj.projUUID);
            }
            data.writer = binaryWriter;
            SendRawData(data.GetByteData());
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
    public class GameInfo
    {
        public long Point = 0;

        public bool IsInAnotherWorld { get { return MapUUID != Guid.Empty; } }

        public Guid MapUUID = Guid.Empty;

        [JsonIgnore]
        public MapManager.MapData Map { get { return GameData.ActiveMap.ContainsKey(MapUUID) ? GameData.ActiveMap[MapUUID] : new MapManager.MapData(); } }

        public EPlayerData TempCharacter = new EPlayerData();

        public EPlayerData Character = new EPlayerData();
    }
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class ShouldSave : Attribute
    {
        public bool Serializable = false;
    }
    public class EItem
    {
        public EItem()
        {
        }
        public EItem(Item item)
        {
            ID = item.netID;
            Stack = item.stack;
            Prefix = item.prefix;
        }
        public EItem(int id, int stack, int prefix)
        {
            ID = id;
            Stack = stack;
            Prefix = prefix;
        }

        public int ID = 0;
        public int Stack = 0;
        public int Prefix = 0;
    }
    public class EPlayerData
    {
        public EPlayerData(TSPlayer tsp, string name = null)
        {
            Name = name ?? tsp.Name;
            Life = tsp.PlayerData.health;
            MaxLife = tsp.PlayerData.maxHealth;
            Mana = tsp.PlayerData.mana;
            MaxMana = tsp.PlayerData.maxMana;
            SpawnX = tsp.PlayerData.spawnX;
            SpawnY = tsp.PlayerData.spawnY;
            skinVariant = (int)tsp.PlayerData.skinVariant;
            hair = (int)tsp.PlayerData.hair;
            hairDye = tsp.PlayerData.hairDye;
            hairColor = tsp.PlayerData.hairColor;
            pantsColor = tsp.PlayerData.pantsColor;
            underShirtColor = tsp.PlayerData.underShirtColor;
            shoeColor = tsp.PlayerData.shoeColor;
            hideVisibleAccessory = tsp.PlayerData.hideVisuals;
            skinColor = tsp.PlayerData.skinColor;
            eyeColor = tsp.PlayerData.eyeColor;
            var bag = new List<EItem>();
            tsp.TPlayer.inventory.ForEach(i => bag.Add(i == null ? new EItem() : i.ToEItem()));
            bag.Add(tsp.TPlayer.trashItem == null ? new EItem() : tsp.TPlayer.trashItem.ToEItem());
            tsp.TPlayer.armor.ForEach(i => bag.Add(i == null ? new EItem() : i.ToEItem()));
            tsp.TPlayer.dye.ForEach(i => bag.Add(i == null ? new EItem() : i.ToEItem()));
            tsp.TPlayer.miscEquips.ForEach(i => bag.Add(i == null ? new EItem() : i.ToEItem()));
            tsp.TPlayer.miscDyes.ForEach(i => bag.Add(i == null ? new EItem() : i.ToEItem()));
            tsp.TPlayer.bank.item.ForEach(i => bag.Add(i == null ? new EItem() : i.ToEItem()));
            tsp.TPlayer.bank2.item.ForEach(i => bag.Add(i == null ? new EItem() : i.ToEItem()));
            tsp.TPlayer.bank3.item.ForEach(i => bag.Add(i == null ? new EItem() : i.ToEItem()));
            tsp.TPlayer.bank4.item.ForEach(i => bag.Add(i == null ? new EItem() : i.ToEItem()));
            Bag = bag;
        }
        public EPlayerData()
        {
            var bag = new List<EItem>();
            for (int i = 0; i < 260; i++)
            {
                bag.Add(new EItem());
            }
            Bag = bag;
        }
        public string Name = "default";
        public int Life = 100;
        public int MaxLife = 100;
        public int Mana = 20;
        public int MaxMana = 20;
        public int SpawnX = 0;
        public int SpawnY = 0;
        public int skinVariant = 0;
        public int hair = 0;
        public int hairDye = 0;
        public Color? hairColor = new Color();
        public Color? pantsColor = new Color();
        public Color? shirtColor = new Color();
        public Color? underShirtColor = new Color();
        public Color? shoeColor = new Color();
        public bool[] hideVisibleAccessory;
        public Color? skinColor = new Color();
        public Color? eyeColor = new Color();
        public List<EItem> Bag = new List<EItem>(260);
    }
}
