using EternalLandPlugin.Game;
using EternalLandPlugin.Net;
using MessagePack;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Biomes;
using Terraria.Localization;
using Terraria.UI.Gamepad;
using TShockAPI;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Vector2 = EternalLandPlugin.Game.MapTools.Vector2;
using Timer = System.Timers.Timer;
using Newtonsoft.Json;

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
                            if (IsInAnotherWorld)
                            {
                                try
                                {
                                    foreach (var key in LoadedChuck.Keys)
                                    {
                                        if (!LoadedChuck[key] && key.Intersects(new Rectangle(TileX - 75, TileY - 75, 150, 150)))
                                        {
                                            SendChuck(key.X, key.Y, Map.GetChuck(key.X, key.Y).Result);
                                            LoadedChuck[key] = true;
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

        public TSPlayer tsp
        {
            get { try { return UserManager.GetTSPlayerFromName(Name, out TSPlayer t) ? t : null; } catch { return null; } }
        }
        public Player plr { get { var t = tsp; return t == null ? null : t.TPlayer; } }

        #region -- 玩家信息 --

        public int ID = -1;

        public int Index { get { return tsp is null ? -1 : tsp.Index; } }

        public string Name = "Unknown";

        public int TileX = 0;

        public int TileY = 0;

        public int X = 0;

        public int Y = 0;

        public int SpawnX => IsInAnotherWorld ? TempCharacter == null ? Character.SpawnX * 16 : TempCharacter.SpawnX * 16 : Main.spawnTileX * 16;

        public int SpawnY => IsInAnotherWorld ? TempCharacter == null ? Character.SpawnY * 16 - 48 : TempCharacter.SpawnY * 16 - 48 : Main.spawnTileY * 16 - 48;
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

        /*[ShouldSave]
        public long MobKillCount = 0;

        [ShouldSave]
        public long BossKillCount = 0;

        [ShouldSave]
        public int DeathCount = 0;*/

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

        //[ShouldSave(Serializable = true)]
        public List<EItem> Bag => TempCharacter == null ? Character.Bag : TempCharacter.Bag;
        [ShouldSave]
        public long Point = 0;
        public bool IsInAnotherWorld
        {
            get
            {
                if (MapUUID != Guid.Empty && Map != null)
                {
                    return true;
                }
                else if (MapUUID != Guid.Empty && tsp != null) BackToOriginMap();
                return false;
            }
        }
        public Guid MapUUID = Guid.Empty;
        public MapManager.MapData Map { get { return GameData.ActiveMap.ContainsKey(MapUUID) ? GameData.ActiveMap[MapUUID] : null; } }
        public EPlayerData TempCharacter = null;
        public EPlayerData Character = null;
        [ShouldSave]
        public Guid TerritoryUUID = Guid.Empty;
        [ShouldSave(Serializable = true)]
        public Statistic Statistic = new Statistic();

        public int SettingPoint = 0;
        public Point[] ChoosePoint = new Point[2];
        #endregion
        #endregion

        #region -- 各种功能函数 --

        #region 游戏相关
        public void SendBag(List<EItem> list = null)
        {
            if (list is null) UserManager.SetBag(this);
            else
            {
                for (int i = 0; i < 260; i++)
                {
                    var item = list[i] ?? new EItem();
                    tsp.SendRawData(new RawDataWriter().SetType(PacketTypes.PlayerSlot).PackByte((byte)Index).PackInt16((short)i).PackInt16((short)item.stack).PackByte((byte)item.prefix).PackInt16((short)item.type).GetByteData());
                }
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
                TempCharacter = GameData.Character[name];
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
            SendCharacterData(Character);
            TempCharacter = null;
            UserManager.UpdateInfoToOtherPlayers(this);
        }
        public void BackToOriginMap()
        {
            try
            {
                if (MapUUID != Guid.Empty && Map != null)
                {
                    Map.Player.Remove(ID);
                }
                LeaveMap(Map);
                MapUUID = Guid.Empty;
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
                MapManager.SendAllItem(this);
            }
            catch (Exception ex) { Log.Error(ex); }
        }
        public void VisitTerritory(Guid uuid)
        {
            if (MapManager.IsTerritoryActive(uuid))
            {
                JoinMap(uuid, false);
            }
            else
            {
                SendEX($"正在从缓存中读入地图, 请稍候...", default, false);
                if (MapManager.ReBuildTerritory(uuid))
                {
                    JoinMap(uuid, false);
                }
                else
                {
                    SendErrorEX("重建属地失败, 请向管理员反馈.");
                    return;
                }
                if (MapManager.GetMapFromUUID(uuid, out var map) && map.Owner != ID && UserManager.TryGetEPlayerFromID(map.Owner, out var owner))
                {
                    owner.SendInfoEX($"玩家 {Name.ToColorful()} 进入了你的属地.");
                }
            }
            SendEX($"欢迎{(uuid == TerritoryUUID ? "回到" : $"来到 {Map.Name.ToColorful()} 的")}属地.");
            Log.Info($"{Name} 进入了{(uuid == TerritoryUUID ? "自己的" : $" {Map.Name} 的")}属地.");
        }
        public void JoinMap(Guid uuid, bool broadcase = true)
        {
            if (uuid == Guid.Empty)
            {
                if (broadcase) SendEX($"返回主世界.");
                BackToOriginMap();
                return;
            }
            if (MapUUID != Guid.Empty)
            {
                if (uuid == MapUUID)
                {
                    SendErrorEX($"无法传送至同一个地图.");
                    return;
                }
                LeaveMap(Map);
            }
            MapUUID = uuid;
            if (broadcase) SendEX($"传送至世界 [c/F97E63:{uuid.ToString().Split('-')[0]}], 可能会造成片刻卡顿.");
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
            MapManager.SendAllItem(this);
            if (!DynamicLoading.IsAlive) DynamicLoading.Start();
            Map.GetAbslute(Map.SpawnX, Map.SpawnY, out int spawnx, out int spawny);
            Utils.Sleep(500);
            tsp.Teleport(spawnx * 16, spawny * 16 - 48);
        }
        public void LeaveMap(MapManager.MapData map)
        {
            if (map == null) return;
            map.Player.Remove(ID);
            if (map.Owner == ID && tsp != null)
            {
                DataBase.SaveMap(map.UUID.ToString(), map);
                SendEX($"已保存属地数据.");
                Log.Info($"{Name} 离开了{(Map.UUID == TerritoryUUID ? "自己" : $" {Map.Name} ")}的属地.");
            }
            map.Proj.Where(p => p != null && p.owner == Index).ForEach(p =>
            {
                p.SetDefault(0);
                p.active = false;
                p.timeLeft = 0;
                p.Update(this);
            });
        }
        readonly Dictionary<Rectangle, bool> LoadedChuck = new Dictionary<Rectangle, bool>();
        Thread DynamicLoading;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">将要发的送到的左上角X坐标</param>
        /// <param name="y">将要发的送到的左上角Y坐标</param>
        /// <param name="data"></param>
        public void SendChuck(int x, int y, MapManager.MapData data)
        {
            MapManager.SendMap(this, data, x, y);
        }
        public void SendChuck(int x, int y, FakeTileProvider data)
        {
            MapManager.SendMap(this, new MapManager.MapData(data, x, y), x, y);
        }
        #endregion
        #region 组合技系统
        private const int KEY_SPACE = 0;
        private const int KEY_LEFT = 1;
        private const int KEY_RIGHT = 2;
        private const int KEY_UP = 3;
        private const int KEY_DOWN = 4;

        public bool[] KeyBuffer = new bool[6];
        bool[] LastFrameKeyBuffer = new bool[6];
        public List<Action<Vector2, int>> SkillBuffer = new List<Action<Vector2, int>>();

        public List<Skill> ActiveSkills = new List<Skill>() { new Skill(new List<int> { 1, 2, 1, 2 }, "Sk", "还有点用", "ssss", 250, 5000, true, delegate (Vector2 v, int owner) { Utils.Broadcast("哦哦哦哦哦哦哦!" + owner); EternalLand.EPlayers[owner].SendData(PacketTypes.PlayLegacySound, "", 28); }) };
        public void KeyDown(int key)
        {
            ActiveSkills.ForEach(s =>
            {
                if (s.KeyDown(key))
                {
                    SendCombatMessage($"技能 {Name} 已就绪.");
                }
            }
            );
        }
        public void ReleseSkill(int direction)
        {
            ActiveSkills.Where(s => s.ShouldRelese).ForEach(s =>
            {
                s.Run(new Vector2(), Index);
                s.ShouldRelese = false;
            });
        }
        #endregion
        public override bool Equals(object obj)
        {
            return ID.ToString() == obj.ToString();
        }
        int timenum = 0;
        Point lastpoint;
        public bool Update()
        {
            try
            {
                if (!Online) return false;
                if (lastpoint == null) lastpoint = new Point();
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
                    if (timenum < 60 && Status != StatusType.Normal) timenum++;
                    else if (timenum >= 60)
                    {
                        timenum = 0;
                        Statistic.PlayTime++;
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                if (!KeyBuffer[i] && plr.controlJump)
                                {
                                    KeyDown(i);
                                    KeyBuffer[i] = true;
                                }
                                else if (!plr.controlJump)
                                {
                                    KeyBuffer[i] = false;
                                }
                                break;
                            case 1:
                                if (!KeyBuffer[i] && plr.controlLeft)
                                {
                                    KeyDown(i);
                                    KeyBuffer[i] = true;
                                }
                                else if (!plr.controlLeft)
                                {
                                    KeyBuffer[i] = false;
                                }
                                break;
                            case 2:
                                if (!KeyBuffer[i] && plr.controlRight)
                                {

                                    KeyDown(i);
                                    KeyBuffer[i] = true;
                                }
                                else if (!plr.controlRight)
                                {
                                    KeyBuffer[i] = false;
                                }
                                break;
                            case 3:
                                if (!KeyBuffer[i] && plr.controlUp)
                                {
                                    KeyDown(i);
                                    KeyBuffer[i] = true;
                                }
                                else if (!plr.controlUp)
                                {
                                    KeyBuffer[i] = false;
                                }
                                break;
                            case 4:
                                if (!KeyBuffer[i] && plr.controlDown)
                                {
                                    KeyDown(i);
                                    KeyBuffer[i] = true;
                                }
                                else if (!plr.controlDown)
                                {
                                    KeyBuffer[i] = false;
                                }
                                break;
                            case 5:
                                if (!KeyBuffer[i] && plr.controlUseItem)
                                {
                                    ReleseSkill(plr.direction);
                                    KeyBuffer[i] = true;
                                }
                                else if (!plr.controlDown)
                                {
                                    KeyBuffer[i] = false;
                                }
                                break;
                        }
                    }
                    if (lastpoint.X > TileX ? lastpoint.X - TileX < 50 : TileX - lastpoint.X < 50 && lastpoint.Y > TileY ? lastpoint.Y - TileY < 50 : TileY - lastpoint.Y < 50)
                    {
                        Statistic.Move += (long)Utils.GetDistance(lastpoint, new Point(TileX, TileY));
                    }
                    lastpoint.X = TileX;
                    lastpoint.Y = TileY;
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
        public void SendCombatMessage(string msg, Color color = default, bool onlysendtome = false)
        {
            color = color == default ? Color.White : color;
            Random random = new Random();
            if (tsp != null)
            {
                if (onlysendtome) tsp.SendData(PacketTypes.CreateCombatTextExtended, msg, (int)color.PackedValue, tsp.X + random.Next(-75, 75), tsp.Y + random.Next(-50, 50));
                else SendDataToAll(PacketTypes.CreateCombatTextExtended, msg, (int)color.PackedValue, tsp.X + random.Next(-75, 75), tsp.Y + random.Next(-50, 50));
            }
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
        public void Broadcast(object text, Color color = default, bool withtitle = true, bool playsound = true)
        {
            color = color == default ? Color.White : color;
            if (MapUUID == Guid.Empty)
            {
                if (withtitle) EternalLand.OnlineEPlayerWhoInMainMap.ForEach(e => e.SendEX(text, color));
                else EternalLand.OnlineEPlayerWhoInMainMap.ForEach(e =>
                {
                    e.tsp.SendMessage(text.ToString(), color);
                    if(playsound) NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(e.plr.position, 122, -1, 0.62f), e.Index);
                });
            }
            else
            {
                if (withtitle) Map.GetAllPlayers().ForEach(e => e.SendEX(text, color));
                else Map.GetAllPlayers().ForEach(e =>
                {
                    e.tsp.SendMessage(text.ToString(), color);
                    if (playsound) NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(e.plr.position, 122, -1, 0.62f), e.Index);
                });
            }
        }
        public void SendSuccessEX(object text, bool playsound = true)
        {
            tsp.SendSuccessEX(text, playsound);
        }

        public void SendInfoEX(object text, bool playsound = true)
        {
            tsp.SendInfoEX(text, playsound);
        }
        public void SendErrorEX(object text, bool playsound = true)
        {
            tsp.SendErrorEX(text, playsound);
        }
        public void SendEX(object text, Color color = default, bool playsound = true)
        {
            tsp.SendEX(text, playsound, color);
        }
        public void SendMultipleError(IEnumerable<object> matches)
        {
            SendErrorEX("检索出多个满足条件的项目: ");
            Utils.BuildLinesFromTerms(matches.ToArray<object>(), null, ", ", 80).ForEach(new Action<string>(tsp.SendInfoEX));
            SendErrorEX("使用 \"部分1 部分2\" 来输入包含空格的关键词.");
        }

        public void SendData(PacketTypes msgType, string text = "", int number = 0, float number2 = 0f, float number3 = 0f, float number4 = 0f, int number5 = 0, int number6 = 0, int number7 = 0)
        {
            if (UserManager.GetTSPlayerFromName(Name, out var tsp))
            {
                if (!tsp.RealPlayer || tsp.ConnectionAlive)
                {
                    NetMessage.SendData((int)msgType, tsp.Index, 255, NetworkText.FromLiteral(text), number, number2, number3, number4, number5, number6, number7);
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
    public class Skill : ICloneable
    {

        public Skill(List<int> keys, string name, string description, string hint, int interval, int cooldown, bool directrelese, Action<Vector2, int> _skill)
        {
            Name = name;
            Description = description;
            Hint = hint;
            DirectRelese = directrelese;
            skill = _skill;
            CoolDown = cooldown;
            Keys = keys;
            Interval = interval;
            ComboTimer = new Timer() { Interval = interval, AutoReset = false, Enabled = true };
            ComboTimer.Elapsed += delegate { KeyQueue.Clear(); };
        }
        public bool ShouldRelese = false;
        Timer ComboTimer;

        public string Name;
        public string Description;
        public string Hint;
        public bool DirectRelese;

        public List<int> Keys;
        public int StartKey => Keys[0];
        public int CoolDown;
        public int Interval = 250;
        /// <summary>
        /// 所释放的技能
        /// </summary>
        public Action<Vector2, int> skill;

        List<int> KeyQueue = new List<int>();

        public async void Run(Vector2 direction, int owner)
        {
            await Task.Run(() => skill.Invoke(direction, owner));

        }
        public bool KeyDown(int key)
        {
            if (KeyQueue.Count == 0 && key != StartKey)
            {
                return false;
            }
            if (KeyQueue.Count < Keys.Count && Keys[KeyQueue.Count] == key)
            {
                KeyQueue.Add(key);
                if (ComboTimer.Enabled)
                {
                    ComboTimer.Stop();
                    ComboTimer.Start();
                }
                else
                {
                    ComboTimer.Start();
                }
            }
            else
            {
                KeyQueue.Clear();
            }
            if (Keys.SequenceEqual(KeyQueue))
            {
                ComboTimer.Stop();
                KeyQueue.Clear();
                ShouldRelese = true;
                return true;
            }
            return false;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
        public Skill Clone()
        {
            return (Skill)MemberwiseClone();
        }
    }
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class ShouldSave : Attribute
    {
        public bool Serializable = false;
    }
    public struct Statistic
    {
        public Statistic(bool temp = false)
        {
            KillPlayers = 0;
            Dead = 0;
            Damage_Player = 0;
            Damage_NPC = 0;
            GetDamage = 0;
            Heal = 0;
            PlayTime = 0;
            CritCount = 0;
            Move = 0;
            Chat = 0;
        }
        public long KillPlayers;
        public long Dead;
        public long Damage_Player;
        public long Damage_NPC;
        public long GetDamage;
        public long Heal;
        public long PlayTime;
        public long CritCount;
        public long Move;
        public long Chat;
    }
    [MessagePackObject]
    [Serializable]
    public class EItem
    {
        [SerializationConstructor]
        public EItem()
        {
        }
        public EItem(Item item)
        {
            type = item.netID;
            stack = item.stack;
            prefix = item.prefix;
            wet = item.wet;
            active = item.active;
            timeSinceItemSpawned = item.timeSinceItemSpawned;
            position = item.position;
            velocity = item.velocity;
        }
        public EItem(int id, int stack, int prefix)
        {
            type = id;
            this.stack = stack;
            this.prefix = prefix;
        }
        [Key(0)]
        public int type = 0;
        [Key(1)]
        public int stack = 0;
        [Key(2)]
        public int prefix = 0;
        [JsonIgnore]
        [Key(3)]
        public bool wet = false;
        [Key(4)]
        [JsonIgnore]
        public bool active = false;
        [Key(5)]
        [JsonIgnore]
        public int timeSinceItemSpawned = 0;
        [Key(6)]
        [JsonIgnore]
        public MapTools.Vector2 position = new Vector2();
        [Key(7)]
        [JsonIgnore]
        public Vector2 velocity = new Vector2();
        [IgnoreMember]
        [JsonIgnore]
        public int height => item.height;
        [IgnoreMember]
        [JsonIgnore]
        public int width => item.width;
        [Key(8)]
        [JsonIgnore]
        public int ownTime = 0;
        [Key(9)]
        [JsonIgnore]
        public int ownIgnore = 255;
        [Key(10)]
        [JsonIgnore]
        public int keepTime = 0;
        [Key(11)]
        [JsonIgnore]
        public int playerIndexTheItemIsReservedFor = 255;
        [IgnoreMember]
        [JsonIgnore]
        public Item item
        {
            get
            {
                var item = new Item();
                item.SetDefaults(type);
                item.stack = stack;
                item.prefix = (byte)prefix;
                item.active = active;
                item.wet = wet;
                item.position = position;
                item.velocity = velocity;
                item.timeSinceItemSpawned = timeSinceItemSpawned;
                item.playerIndexTheItemIsReservedFor = playerIndexTheItemIsReservedFor;
                item.keepTime = keepTime;
                item.ownIgnore = ownIgnore;
                item.ownTime = ownTime;
                return item;
            }
        }
        public void FindOwner(int whoAmI, MapManager.MapData map)
        {
            if (keepTime > 0) return;
            int num = playerIndexTheItemIsReservedFor;
            playerIndexTheItemIsReservedFor = 255;
            bool flag = true;
            if (type == 267 && ownIgnore != -1)
            {
                flag = false;
            }
            if (flag)
            {
                float num2 = NPC.sWidth;
                for (int i = 0; i < 255; i++)
                {
                    if (ownIgnore == i && ownTime > 0)
                    {
                        continue;
                    }
                    Player player = Main.player[i];
                    if (!player.active)
                    {
                        continue;
                    }
                    Player.ItemSpaceStatus status = player.ItemSpace(map.Items[whoAmI]);
                    if (player.CanPullItem(map.Items[whoAmI], status))
                    {
                        float num3 = Math.Abs(player.position.X + (float)(player.width / 2) - position.X - (float)(width / 2)) + Math.Abs(player.position.Y + (float)(player.height / 2) - position.Y - (float)height);
                        if (player.manaMagnet && (type == 184 || type == 1735 || type == 1868))
                        {
                            num3 -= (float)Item.manaGrabRange;
                        }
                        if (player.lifeMagnet && (type == 58 || type == 1734 || type == 1867))
                        {
                            num3 -= (float)Item.lifeGrabRange;
                        }
                        if (type == 4143)
                        {
                            num3 -= (float)Item.manaGrabRange;
                        }
                        if(UserManager.TryGetEPlayeFromName(Main.player[i].name, out var eplr) && eplr.MapUUID == map.UUID) playerIndexTheItemIsReservedFor = i;
                    }
                }
            }
            if (playerIndexTheItemIsReservedFor != 255)
            {
                // map.SendRawDataToPlayer(new RawDataWriter().SetType(PacketTypes.UpdateItemDrop).PackInt16((short)whoAmI).PackVector2(position).PackVector2(velocity).PackInt16((short)stack).PackByte((byte)prefix).PackByte((byte)false.ToInt()).PackInt16((short)type).GetByteData());
                map.SendRawDataToPlayer(new RawDataWriter().SetType(PacketTypes.ItemOwner).PackInt16((short)whoAmI).PackByte((byte)playerIndexTheItemIsReservedFor).GetByteData());

            }
        }
        public void TryCombiningIntoNearbyItems(int i, MapManager.MapData map)
        {
            bool flag = true;
            int num = type;
            if ((uint)(num - 71) <= 3u)
            {
                flag = false;
            }
            if (Terraria.ID.ItemID.Sets.NebulaPickup[type])
            {
                flag = false;
            }
            if (flag)
            {
                CombineWithNearbyItems(i, map);
            }
        }
        public bool CanCombineStackInWorld()
        {
            int num = type;
            if (num == 75)
            {
                return false;
            }
            if (item.createTile < 0 && item.createWall <= 0 && (item.ammo <= 0 || item.notAmmo) && !item.consumable && (type < 205 || type > 207) && type != 1128 && type != 530 && item.dye <= 0 && item.paint <= 0)
            {
                return item.material;
            }
            return true;
        }
        public void CombineWithNearbyItems(int myItemIndex, MapManager.MapData map)
        {
            if (!CanCombineStackInWorld() || stack >= item.maxStack)
            {
                return;
            }
            for (int i = myItemIndex + 1; i < 400; i++)
            {
                Item item = map.Items[i];
                if (!item.active || item.type != type || item.stack <= 0 || playerIndexTheItemIsReservedFor != item.playerIndexTheItemIsReservedFor)
                {
                    continue;
                }
                float num = Math.Abs(position.X + (float)(item.width / 2) - (item.position.X + (float)(item.width / 2))) + Math.Abs(position.Y + (float)(item.height / 2) - (item.position.Y + (float)(item.height / 2)));
                int num2 = 30;
                if ((double)map.numberOfNewItems > 40.0)
                {
                    num2 *= 2;
                }
                if ((double)map.numberOfNewItems > 80.0)
                {
                    num2 *= 2;
                }
                if ((double)map.numberOfNewItems > 120.0)
                {
                    num2 *= 2;
                }
                if ((double)map.numberOfNewItems > 160.0)
                {
                    num2 *= 2;
                }
                if ((double)map.numberOfNewItems > 200.0)
                {
                    num2 *= 2;
                }
                if ((double)map.numberOfNewItems > 240.0)
                {
                    num2 *= 2;
                }
                if (num < (float)num2)
                {
                    position = (position + (MapTools.Vector2)item.position) / 2f;
                    velocity = (velocity + (MapTools.Vector2)item.velocity) / 2f;
                    int num3 = item.stack;
                    if (num3 > item.maxStack - stack)
                    {
                        num3 = item.maxStack - stack;
                    }
                    item.stack -= num3;
                    stack += num3;
                    if (item.stack <= 0)
                    {
                        item.SetDefaults();
                        item.active = false;
                    }
                    if (Main.netMode != 0 && item.playerIndexTheItemIsReservedFor == Main.myPlayer)
                    {
                        map.GetAllPlayers().ForEach(e =>
                        {
                            e.SendRawData(new RawDataWriter().SetType(PacketTypes.ItemDrop).PackInt16((short)myItemIndex).PackVector2(item.position).PackVector2(item.velocity).PackInt16((short)item.stack).PackByte((byte)item.prefix).PackByte((byte)true.ToInt()).PackInt16((short)item.type).GetByteData());
                            e.SendRawData(new RawDataWriter().SetType(PacketTypes.ItemDrop).PackInt16((short)i).PackVector2(item.position).PackVector2(item.velocity).PackInt16((short)item.stack).PackByte((byte)item.prefix).PackByte((byte)true.ToInt()).PackInt16((short)item.type).GetByteData());
                        });
                    }
                }
            }
        }

        public static implicit operator EItem(Terraria.Item v)
        {
            return new EItem(v);
        }
        public static implicit operator Terraria.Item(EItem v)
        {
            var item = new Item();
            item.SetDefaults(v.type);
            item.stack = v.stack;
            item.prefix = (byte)v.prefix;
            item.wet = v.wet;
            item.active = v.active;
            item.timeSinceItemSpawned = v.timeSinceItemSpawned;
            item.position = v.position;
            item.velocity = v.velocity;
            item.playerIndexTheItemIsReservedFor = v.playerIndexTheItemIsReservedFor;
            item.keepTime = v.keepTime;
            item.ownIgnore = v.ownIgnore;
            item.ownTime = v.ownTime;
            return item;
        }
    }
    public class EPlayerData
    {
        public EPlayerData(TSPlayer tsp, string name = null)
        {
            Name = name ?? tsp.Name;
            Life = tsp.TPlayer.statLife;
            MaxLife = tsp.TPlayer.statLifeMax2;
            Mana = tsp.TPlayer.statMana;
            MaxMana = tsp.TPlayer.statManaMax2;
            SpawnX = tsp.TPlayer.SpawnX;
            SpawnY = tsp.TPlayer.SpawnY;
            skinVariant = (int)tsp.TPlayer.skinVariant;
            hair = (int)tsp.TPlayer.hair;
            hairDye = tsp.TPlayer.hairDye;
            hairColor = tsp.TPlayer.hairColor;
            pantsColor = tsp.TPlayer.pantsColor;
            underShirtColor = tsp.TPlayer.underShirtColor;
            shoeColor = tsp.TPlayer.shoeColor;
            hideVisibleAccessory = tsp.TPlayer.hideVisibleAccessory;
            skinColor = tsp.TPlayer.skinColor;
            eyeColor = tsp.TPlayer.eyeColor;
            var bag = new List<EItem>();
            tsp.TPlayer.inventory.ForEach(i => bag.Add(i == null ? new EItem() : i.ToEItem()));
            tsp.TPlayer.armor.ForEach(i => bag.Add(i == null ? new EItem() : i.ToEItem()));
            tsp.TPlayer.dye.ForEach(i => bag.Add(i == null ? new EItem() : i.ToEItem()));
            tsp.TPlayer.miscEquips.ForEach(i => bag.Add(i == null ? new EItem() : i.ToEItem()));
            tsp.TPlayer.miscDyes.ForEach(i => bag.Add(i == null ? new EItem() : i.ToEItem()));
            tsp.TPlayer.bank.item.ForEach(i => bag.Add(i == null ? new EItem() : i.ToEItem()));
            bag.Add(tsp.TPlayer.trashItem == null ? new EItem() : tsp.TPlayer.trashItem.ToEItem());
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
