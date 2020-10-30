using EternalLandPlugin.Account;
using Microsoft.Xna.Framework;
using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Events;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.Net.Sockets;
using Terraria.Social;
using TerrariaApi.Server;
using TShockAPI;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;

namespace EternalLandPlugin.Game
{
    public class MapManager
    {
        public class MapData
        {
            public MapData(Point topleft, Point bottomright, string name = "UnKnown", bool keepalive = false)
            {
                Name = name;
                KeepAlive = keepalive;
                ReadTile(topleft.X, topleft.Y, bottomright.X - topleft.X, bottomright.Y - topleft.Y);
            }

            public MapData(int StartX, int StartY, int width, int height, string name = "UnKnown", bool keepalive = false)
            {
                Name = name;
                KeepAlive = keepalive;
                ReadTile(StartX, StartY, width, height);
            }

            public MapData(FakeTileProvider data, int startx, int starty)
            {
                Width = data.Width;
                Height = data.Height;
                StartX = startx;
                StartY = starty;
                Tile = data;
            }

            public MapData()
            {
                Width = Main.maxTilesX;
                Height = Main.maxTilesY;
                StartX = 0;
                StartY = 0;
                Tile = new FakeTileProvider(0, 0);
            }

            async void ReadTile(int StartX, int StartY, int width, int height)
            {
                await Task.Run(() =>
                {
                    Height = height;
                    Width = width;
                    Tile = new FakeTileProvider(width, height);
                    int y = 0;
                    int chest = 0;
                    int sign = 0;
                    for (int tiley = StartY; y < Height; tiley++)
                    {
                        int x = 0;
                        for (int tilex = StartX; x < Width; tilex++)
                        {
                            try
                            {
                                ITile temptile = Main.tile[tilex, tiley] ?? new Tile();
                                Tile[x, y].CopyFrom(temptile);
                                if ((TileID.Sets.BasicChest[(int)temptile.type] && temptile.frameX % 36 == 0 && temptile.frameY % 36 == 0) || (temptile.type == 88 && temptile.frameX % 54 == 0 && temptile.frameY % 36 == 0))
                                {
                                    int chestid = (short)Terraria.Chest.FindChest(tilex, tiley);
                                    var temp = Main.chest[chestid];
                                    temp.x = x;
                                    temp.y = y;
                                    if (chestid != -1) Chest.Add((short)chest, temp);
                                    chest++;
                                }
                                if ((temptile.type == 85 | temptile.type == 55 || temptile.type == 425) && temptile.frameX % 36 == 0 && temptile.frameY % 36 == 0)
                                {
                                    int signid = (short)Terraria.Sign.ReadSign(tilex, tiley, true);
                                    var temp = Main.sign[signid];
                                    temp.x = x;
                                    temp.y = y;
                                    if (signid != -1) Sign.Add((short)sign, temp);
                                    sign++;
                                }
                            }
                            catch { }

                            x++;
                        }
                        y++;
                    }
                });
            }
            internal void ApplyTiles(FakeTileProvider Tiles, int AbsoluteX, int AbsoluteY)
            {
                for (int y = AbsoluteY; y < AbsoluteY + Height; y++)
                {
                    for (int x = AbsoluteX; x < AbsoluteX + Width; x++)
                    {
                        Tiles[x - AbsoluteX, y - AbsoluteY].CopyFrom(Tile[x - AbsoluteX, y - AbsoluteY] ?? new Tile());
                    }
                }
                /*Intersect(AbsoluteX, AbsoluteY, Tiles.GetLength(0), Tiles.GetLength(1), out var RX, out var RY, out var RWidth, out var RHeight);
                int num = RX + RWidth;
                int num2 = RY + RHeight;
                for (int i = RX; i < num; i++)
                {
                    for (int j = RY; j < num2; j++)
                    {
                        //Console.WriteLine(Utils.GetItemFromTile(tilex, tiley, temptile));
                        ITile tile = Tile[i - X, j - Y];
                        if (tile != null)
                        {
                            Tiles[i - RX, j - AbsoluteY] = tile;
                        }
                    }
                }*/
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="realx">玩家所在的X坐标</param>
            /// <param name="realy">玩家所在的Y坐标</param>
            /// <returns></returns>
            public async Task<FakeTileProvider> GetChuck(int realx, int realy)
            {
                return await Task.Run(() =>
                {
                    FakeTileProvider tiles = new FakeTileProvider(ChuckSize, ChuckSize);
                    for (int tiley = realy; tiley < realy + ChuckSize; tiley++)
                    {
                        for (int tilex = realx; tilex < realx + ChuckSize; tilex++)
                        {
                            if (tilex < StartX + Width && tilex > StartX && tiley < StartY + Height && tiley > StartY)
                            {
                                tiles[tilex - realx, tiley - realy].CopyFrom(Tile[tilex - StartX, tiley - StartY]);
                            }
                            else
                            {
                                tiles[tilex - realx, tiley - realy].CopyFrom(new Tile());
                            }
                        }
                    }
                    return tiles;
                });
            }
            /// <summary>
            /// 获取指定坐标在此地图内的相对坐标
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="RX"></param>
            /// <param name="RY"></param>
            /// <returns></returns>
            public bool GetRelative(int x, int y, out int RX, out int RY)
            {
                if (StartX == -1 || StartY == -1)
                {
                    RX = -1;
                    RY = -1;
                    return false;
                }
                else
                {
                    RX = StartX <= x ? x - StartX : 0;
                    RY = StartY <= y ? y - StartY : 0;
                    return true;
                }
            }
            /// <summary>
            /// 获取相对坐标在世界内的绝对坐标
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="AX"></param>
            /// <param name="AY"></param>
            /// <returns></returns>
            public bool GetAbslute(int x, int y, out int AX, out int AY)
            {
                if (StartX == -1 || StartY == -1)
                {
                    AX = -1;
                    AY = -1;
                    return false;
                }
                else
                {
                    AX = x >= 0 && x < Width ? StartX + x : x < Width ? StartX : StartX + Width;
                    AY = y >= 0 && x < Height ? StartY + y : y < Height ? StartY : StartY + Height;
                    return true;
                }
            }
            public void SetSpawn(int relativex, int relativey) => GetAbslute(relativex, relativey, out SpawnX, out SpawnY);

            public List<EPlayer> GetAllPlayers()
            {
                List<EPlayer> list = new List<EPlayer>();
                try
                {
                    Player.ForEach(p =>
                    {
                        if (UserManager.TryGetEPlayerFromID(p, out var eplr)) list.Add(eplr);
                        else Player.Remove(p);
                    });
                }
                catch { }
                return list;
            }
            public int FindChest(int rx, int ry)
            {
                var chest = -1;
                Chest.ForEach(c => { if (c.Value.x == rx && c.Value.y == ry) { chest = c.Key; return; } });
                return chest;
            }
            public int FindSign(int rx, int ry)
            {
                var sign = -1;
                Sign.ForEach(c => { if (c.Value.x == rx && c.Value.y == ry) { sign = c.Key; return; } });
                return sign;
            }
            public string Name = "UnKnown";
            public bool KeepAlive = false;
            public static readonly int ChuckSize = 100;
            public bool Origin = false;
            public int StartX = -1;
            public int StartY = -1;
            public int Width;
            public int Height;
            public int SpawnX = -1;
            public int SpawnY = -1;
            public List<int> Player = new List<int>();
            public Dictionary<short, Chest> Chest = new Dictionary<short, Chest>();
            public Dictionary<short, Sign> Sign = new Dictionary<short, Sign>();
            public Projectile[] Proj = new Projectile[1000];
            public NPC[] Npc = new NPC[1000];
            //public ITile[,] Tile;
            public FakeTileProvider Tile
            {
                get;
                set;
            }


            public int GetNewProjUUID()
            {
                int result = 1000;
                for (int i = 0; i < 1000; i++)
                {
                    if (Proj[i] == null)
                    {
                        return i;
                    }
                    else if (!Proj[i].active)
                    {
                        return i;
                    }
                }
                int time = 9999999;
                for (int i = 0; i < 1000; i++)
                {
                    if (Proj[i] != null && !Proj[i].netImportant && Proj[i].timeLeft < time)
                    {
                        return i;
                    }
                }
                return result;
            }
        }
        public static bool GetMapFromUUID(Guid uuid, out MapData data)
        {
            foreach (var map in GameData.ActiveMap)
            {
                if (map.Key == uuid)
                {
                    data = map.Value;
                    return true;
                }
            }
            data = new MapData();
            return false;
        }
        public static Guid CreateMultiPlayerMap(MapData data, int x = -1, int y = -1)
        {
            data.StartX = x == -1 ? (Main.maxTilesX / 2) - (data.Width / 2) : x;
            data.StartY = y == -1 ? (Main.maxTilesX / 2) - (data.Width / 2) : y;
            var uuid = Guid.NewGuid();
            GameData.ActiveMap.Add(uuid, data);
            return uuid;
        }
        public static void AddPlayerToWorld(EPlayer eplr, Guid uuid)
        {
            if (GetMapFromUUID(uuid, out var data) && !data.Player.Contains(eplr.ID))
            {
                eplr.JoinMap(uuid);
            }
        }
        public static void CheckMapAlive()
        {
            Dictionary<Guid, int> check = new Dictionary<Guid, int>();
            bool restartnow = false;
            while (true)
            {
                restartnow = false;
                foreach (var map in GameData.ActiveMap)
                {
                    if (!check.ContainsKey(map.Key)) check.Add(map.Key, 0);
                    if (!map.Value.GetAllPlayers().Any()) check[map.Key]++;
                    if (check[map.Key] > 30)
                    {
                        check.Remove(map.Key);
                        GameData.ActiveMap.Remove(map.Key);
                        restartnow = true;
                        Utils.Broadcast($"世界 {map.Key} 已销毁.");
                        break;
                    }
                    check.ForEach(c => { if (!MapManager.GetMapFromUUID(c.Key, out var az)) check.Remove(c.Key); });
                }
                if (restartnow) continue;
                Thread.Sleep(1000);
            }
        }
        public static void SendProjectile(EPlayer eplr)
        {
            if (eplr.GameInfo.IsInAnotherWorld)
            {
                Main.projectile.Where(p => p.active && eplr.GameInfo.Map.Player.Contains(p.owner)).ForEach(proj => eplr.SendData(PacketTypes.ProjectileNew, "", proj.identity));
            }
            else
            {
                Main.projectile.Where(p => p.active && eplr.GameInfo.Map.Player.Contains(p.owner)).ForEach(proj => eplr.SendData(PacketTypes.ProjectileNew, "", proj.identity));
            }
        }
        public static void SendMap(EPlayer eplr, MapData data, int x, int y)
        {
            SendMapDerict(eplr, data.Width, data.Height, data, x, y);
            //ChangeWorldInfo(eplr);
        }
        public static void SendMapDerict(EPlayer eplr, int width, int height, MapData data, int StartX = 0, int StartY = 0)
        {
            Send(StartX, StartY, (short)width, (short)height, data, eplr);
            var num = Math.Max(width, height);
            SendSquare(num, StartX, StartY, data, eplr, 0);
            int sectionX = Netplay.GetSectionX(StartX);
            int sectionX2 = Netplay.GetSectionX(StartX + width - 1);
            int sectionY = Netplay.GetSectionY(StartY);
            int sectionY2 = Netplay.GetSectionY(StartY + height - 1);
            NetMessage.SendData(11, -1, -1, null, sectionX, (float)sectionY, (float)sectionX2, (float)sectionY2, 0, 0, 0);
        }

        #region 一堆地图操作函数
        public static void ChangeWorldInfo(EPlayer eplr, bool toorigin = false)
        {
            byte[] array;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.BaseStream.Position = 2L;
                    binaryWriter.Write((byte)7);
                    WorldInfo(binaryWriter);
                    long position = binaryWriter.BaseStream.Position;
                    binaryWriter.BaseStream.Position = 0L;
                    binaryWriter.Write((short)position);
                    binaryWriter.BaseStream.Position = position;
                    array = memoryStream.ToArray();
                }
            }
            eplr.SendRawData(array);
        }

        static void WorldInfo(BinaryWriter binaryWriter, bool toorigin = false)
        {
            binaryWriter.Write((int)Main.time);
            BitsByte bb6 = (byte)0;
            bb6[0] = Main.dayTime;
            bb6[1] = Main.bloodMoon;
            bb6[2] = Main.eclipse;
            binaryWriter.Write(bb6);
            binaryWriter.Write((byte)Main.moonPhase);
            binaryWriter.Write((short)Main.maxTilesX);
            binaryWriter.Write((short)Main.maxTilesY);
            binaryWriter.Write((short)Main.spawnTileX);
            binaryWriter.Write((short)Main.spawnTileY);
            binaryWriter.Write(toorigin ? (short)Main.worldSurface : (short)Main.maxTilesY); //地表开始位置
            binaryWriter.Write(toorigin ? (short)Main.worldSurface : (short)Main.maxTilesY);
            //if (!toorigin) return;
            binaryWriter.Write(Main.worldID);
            binaryWriter.Write(Main.worldName);
            binaryWriter.Write((byte)Main.GameMode);
            binaryWriter.Write(Main.ActiveWorldFileData.UniqueId.ToByteArray());
            binaryWriter.Write(Main.ActiveWorldFileData.WorldGeneratorVersion);
            binaryWriter.Write((byte)Main.moonType);
            binaryWriter.Write((byte)WorldGen.treeBG1);
            binaryWriter.Write((byte)WorldGen.treeBG2);
            binaryWriter.Write((byte)WorldGen.treeBG3);
            binaryWriter.Write((byte)WorldGen.treeBG4);
            binaryWriter.Write((byte)WorldGen.corruptBG);
            binaryWriter.Write((byte)WorldGen.jungleBG);
            binaryWriter.Write((byte)WorldGen.snowBG);
            binaryWriter.Write((byte)WorldGen.hallowBG);
            binaryWriter.Write((byte)WorldGen.crimsonBG);
            binaryWriter.Write((byte)WorldGen.desertBG);
            binaryWriter.Write((byte)WorldGen.oceanBG);
            binaryWriter.Write((byte)WorldGen.mushroomBG);
            binaryWriter.Write((byte)WorldGen.underworldBG);
            binaryWriter.Write((byte)Main.iceBackStyle);
            binaryWriter.Write((byte)Main.jungleBackStyle);
            binaryWriter.Write((byte)Main.hellBackStyle);
            binaryWriter.Write(Main.windSpeedTarget);
            binaryWriter.Write((byte)Main.numClouds);
            for (int num7 = 0; num7 < 3; num7++)
            {
                binaryWriter.Write(Main.treeX[num7]);
            }
            for (int num8 = 0; num8 < 4; num8++)
            {
                binaryWriter.Write((byte)Main.treeStyle[num8]);
            }
            for (int num9 = 0; num9 < 3; num9++)
            {
                binaryWriter.Write(Main.caveBackX[num9]);
            }
            for (int num10 = 0; num10 < 4; num10++)
            {
                binaryWriter.Write((byte)Main.caveBackStyle[num10]);
            }
            WorldGen.TreeTops.SyncSend(binaryWriter);
            if (!Main.raining)
            {
                Main.maxRaining = 0f;
            }
            binaryWriter.Write(Main.maxRaining);
            BitsByte bb7 = (byte)0;
            bb7[0] = WorldGen.shadowOrbSmashed;
            bb7[1] = NPC.downedBoss1;
            bb7[2] = NPC.downedBoss2;
            bb7[3] = NPC.downedBoss3;
            bb7[4] = Main.hardMode;
            bb7[5] = NPC.downedClown;
            bb7[6] = Main.ServerSideCharacter;
            bb7[7] = NPC.downedPlantBoss;
            binaryWriter.Write(bb7);
            BitsByte bb8 = (byte)0;
            bb8[0] = NPC.downedMechBoss1;
            bb8[1] = NPC.downedMechBoss2;
            bb8[2] = NPC.downedMechBoss3;
            bb8[3] = NPC.downedMechBossAny;
            bb8[4] = (Main.cloudBGActive >= 1f);
            bb8[5] = WorldGen.crimson;
            bb8[6] = Main.pumpkinMoon;
            bb8[7] = Main.snowMoon;
            binaryWriter.Write(bb8);
            BitsByte bb9 = (byte)0;
            bb9[1] = Main.fastForwardTime;
            bb9[2] = Main.slimeRain;
            bb9[3] = NPC.downedSlimeKing;
            bb9[4] = NPC.downedQueenBee;
            bb9[5] = NPC.downedFishron;
            bb9[6] = NPC.downedMartians;
            bb9[7] = NPC.downedAncientCultist;
            binaryWriter.Write(bb9);
            BitsByte bb10 = (byte)0;
            bb10[0] = NPC.downedMoonlord;
            bb10[1] = NPC.downedHalloweenKing;
            bb10[2] = NPC.downedHalloweenTree;
            bb10[3] = NPC.downedChristmasIceQueen;
            bb10[4] = NPC.downedChristmasSantank;
            bb10[5] = NPC.downedChristmasTree;
            bb10[6] = NPC.downedGolemBoss;
            bb10[7] = BirthdayParty.PartyIsUp;
            binaryWriter.Write(bb10);
            BitsByte bb11 = (byte)0;
            bb11[0] = NPC.downedPirates;
            bb11[1] = NPC.downedFrost;
            bb11[2] = NPC.downedGoblins;
            bb11[3] = Sandstorm.Happening;
            bb11[4] = DD2Event.Ongoing;
            bb11[5] = DD2Event.DownedInvasionT1;
            bb11[6] = DD2Event.DownedInvasionT2;
            bb11[7] = DD2Event.DownedInvasionT3;
            binaryWriter.Write(bb11);
            BitsByte bb12 = (byte)0;
            bb12[0] = NPC.combatBookWasUsed;
            bb12[1] = LanternNight.LanternsUp;
            bb12[2] = NPC.downedTowerSolar;
            bb12[3] = NPC.downedTowerVortex;
            bb12[4] = NPC.downedTowerNebula;
            bb12[5] = NPC.downedTowerStardust;
            bb12[6] = Main.forceHalloweenForToday;
            bb12[7] = Main.forceXMasForToday;
            binaryWriter.Write(bb12);
            BitsByte bb13 = (byte)0;
            bb13[0] = NPC.boughtCat;
            bb13[1] = NPC.boughtDog;
            bb13[2] = NPC.boughtBunny;
            bb13[3] = NPC.freeCake;
            bb13[4] = Main.drunkWorld;
            bb13[5] = NPC.downedEmpressOfLight;
            bb13[6] = NPC.downedQueenSlime;
            bb13[7] = Main.getGoodWorld;
            binaryWriter.Write(bb13);
            binaryWriter.Write((short)WorldGen.SavedOreTiers.Copper);
            binaryWriter.Write((short)WorldGen.SavedOreTiers.Iron);
            binaryWriter.Write((short)WorldGen.SavedOreTiers.Silver);
            binaryWriter.Write((short)WorldGen.SavedOreTiers.Gold);
            binaryWriter.Write((short)WorldGen.SavedOreTiers.Cobalt);
            binaryWriter.Write((short)WorldGen.SavedOreTiers.Mythril);
            binaryWriter.Write((short)WorldGen.SavedOreTiers.Adamantite);
            binaryWriter.Write((sbyte)Main.invasionType);
            if (SocialAPI.Network != null)
            {
                binaryWriter.Write(SocialAPI.Network.GetLobbyId());
            }
            else
            {
                binaryWriter.Write(0uL);
            }
            binaryWriter.Write(Sandstorm.IntendedSeverity);
        }

        static void Send(int X, int Y, short Width, short Height, MapData data, EPlayer eplr)
        {
            byte[] array;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.BaseStream.Position = 2L;
                    binaryWriter.Write((byte)10);
                    CompressTileBlock(X, Y, Width, Height, binaryWriter, data);
                    long position = binaryWriter.BaseStream.Position;
                    binaryWriter.BaseStream.Position = 0L;
                    binaryWriter.Write((short)position);
                    binaryWriter.BaseStream.Position = position;
                    array = memoryStream.ToArray();
                }
            }
            eplr.SendRawData(array);
        }

        static int CompressTileBlock(int xStart, int yStart, short width, short height, BinaryWriter writer, MapData data)
        {

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(xStart);
                    binaryWriter.Write(yStart);
                    binaryWriter.Write(width);
                    binaryWriter.Write(height);
                    CompressTileBlock_Inner(binaryWriter, xStart, yStart, width, height, data);
                    memoryStream.Position = 0L;
                    MemoryStream memoryStream2 = new MemoryStream();
                    using (DeflateStream deflateStream = new DeflateStream(memoryStream2, CompressionMode.Compress, leaveOpen: true))
                    {
                        memoryStream.CopyTo(deflateStream);
                        deflateStream.Flush();
                        deflateStream.Close();
                        deflateStream.Dispose();
                    }
                    if (memoryStream.Length <= memoryStream2.Length)
                    {
                        memoryStream.Position = 0L;
                        writer.Write((byte)0);
                        writer.Write(memoryStream.GetBuffer());
                    }
                    else
                    {
                        memoryStream2.Position = 0L;
                        writer.Write((byte)1);
                        writer.Write(memoryStream2.GetBuffer());
                    }
                }
            }
            return 0;
        }

        public static void CompressTileBlock_Inner(BinaryWriter writer, int xStart, int yStart, int width, int height, MapData data)
        {
            short[] array = new short[8000];
            short[] array2 = new short[1000];
            short[] array3 = new short[1000];
            short num = 0;
            short num2 = 0;
            short num3 = 0;
            short num4 = 0;
            int num5 = 0;
            int num6 = 0;
            byte b = 0;
            byte[] array4 = new byte[15];
            ITile tile = null;
            var appliedTiles = data.Tile;
            for (int i = yStart; i < yStart + height; i++)
            {
                for (int j = xStart; j < xStart + width; j++)
                {
                    ITile tile2 = appliedTiles[j - xStart, i - yStart];
                    if (tile2.isTheSameAs(tile))
                    {
                        num4 = (short)(num4 + 1);
                        continue;
                    }
                    if (tile != null)
                    {
                        if (num4 > 0)
                        {
                            array4[num5] = (byte)(num4 & 0xFF);
                            num5++;
                            if (num4 > 255)
                            {
                                b = (byte)(b | 0x80);
                                array4[num5] = (byte)((num4 & 0xFF00) >> 8);
                                num5++;
                            }
                            else
                            {
                                b = (byte)(b | 0x40);
                            }
                        }
                        array4[num6] = b;
                        writer.Write(array4, num6, num5 - num6);
                        num4 = 0;
                    }
                    num5 = 3;
                    byte b2;
                    byte b3;
                    b = (b3 = (b2 = 0));
                    if (tile2.active())
                    {
                        b = (byte)(b | 2);
                        array4[num5] = (byte)tile2.type;
                        num5++;
                        if (tile2.type > 255)
                        {
                            array4[num5] = (byte)(tile2.type >> 8);
                            num5++;
                            b = (byte)(b | 0x20);
                        }
                        if (TileID.Sets.BasicChest[tile2.type] && tile2.frameX % 36 == 0 && tile2.frameY % 36 == 0)
                        {
                            short num7 = (short)data.FindChest(j - xStart, i - yStart);
                            if (num7 != -1)
                            {
                                array[num] = num7;
                                num = (short)(num + 1);
                            }
                        }
                        if (tile2.type == 88 && tile2.frameX % 54 == 0 && tile2.frameY % 36 == 0)
                        {
                            short num8 = (short)data.FindChest(j - xStart, i - yStart);
                            if (num8 != -1)
                            {
                                array[num] = num8;
                                num = (short)(num + 1);
                            }
                        }
                        if ((tile2.type == 85 | tile2.type == 55 || tile2.type == 425 || tile2.type == 573) && tile2.frameX % 36 == 0 && tile2.frameY % 36 == 0)
                        {
                            short num9 = (short)data.FindSign(j - xStart, i - yStart);
                            if (num9 != -1)
                            {
                                array2[num2++] = num9;
                            }
                        }
                        if (tile2.type == 378 && tile2.frameX % 36 == 0 && tile2.frameY == 0)
                        {
                            int num13 = TETrainingDummy.Find(j, i);
                            if (num13 != -1)
                            {
                                array3[num3++] = (short)num13;
                            }
                        }
                        if (tile2.type == 395 && tile2.frameX % 36 == 0 && tile2.frameY == 0)
                        {
                            int num14 = TEItemFrame.Find(j, i);
                            if (num14 != -1)
                            {
                                array3[num3++] = (short)num14;
                            }
                        }
                        if (tile2.type == 520 && tile2.frameX % 18 == 0 && tile2.frameY == 0)
                        {
                            int num15 = TEFoodPlatter.Find(j, i);
                            if (num15 != -1)
                            {
                                array3[num3++] = (short)num15;
                            }
                        }
                        if (tile2.type == 471 && tile2.frameX % 54 == 0 && tile2.frameY == 0)
                        {
                            int num16 = TEWeaponsRack.Find(j, i);
                            if (num16 != -1)
                            {
                                array3[num3++] = (short)num16;
                            }
                        }
                        if (tile2.type == 470 && tile2.frameX % 36 == 0 && tile2.frameY == 0)
                        {
                            int num17 = TEDisplayDoll.Find(j, i);
                            if (num17 != -1)
                            {
                                array3[num3++] = (short)num17;
                            }
                        }
                        if (tile2.type == 475 && tile2.frameX % 54 == 0 && tile2.frameY == 0)
                        {
                            int num18 = TEHatRack.Find(j, i);
                            if (num18 != -1)
                            {
                                array3[num3++] = (short)num18;
                            }
                        }
                        if (tile2.type == 597 && tile2.frameX % 54 == 0 && tile2.frameY % 72 == 0)
                        {
                            int num19 = TETeleportationPylon.Find(j, i);
                            if (num19 != -1)
                            {
                                array3[num3++] = (short)num19;
                            }
                        }
                        if (Main.tileFrameImportant[tile2.type])
                        {
                            array4[num5] = (byte)(tile2.frameX & 0xFF);
                            num5++;
                            array4[num5] = (byte)((tile2.frameX & 0xFF00) >> 8);
                            num5++;
                            array4[num5] = (byte)(tile2.frameY & 0xFF);
                            num5++;
                            array4[num5] = (byte)((tile2.frameY & 0xFF00) >> 8);
                            num5++;
                        }
                        if (tile2.color() != 0)
                        {
                            b2 = (byte)(b2 | 8);
                            array4[num5] = tile2.color();
                            num5++;
                        }
                    }
                    if (tile2.wall != 0)
                    {
                        b = (byte)(b | 4);
                        array4[num5] = (byte)tile2.wall;
                        num5++;
                        if (tile2.wallColor() != 0)
                        {
                            b2 = (byte)(b2 | 0x10);
                            array4[num5] = tile2.wallColor();
                            num5++;
                        }
                    }
                    if (tile2.liquid != 0)
                    {
                        b = (tile2.lava() ? ((byte)(b | 0x10)) : ((!tile2.honey()) ? ((byte)(b | 8)) : ((byte)(b | 0x18))));
                        array4[num5] = tile2.liquid;
                        num5++;
                    }
                    if (tile2.wire())
                    {
                        b3 = (byte)(b3 | 2);
                    }
                    if (tile2.wire2())
                    {
                        b3 = (byte)(b3 | 4);
                    }
                    if (tile2.wire3())
                    {
                        b3 = (byte)(b3 | 8);
                    }
                    int num20 = tile2.halfBrick() ? 16 : ((tile2.slope() != 0) ? (tile2.slope() + 1 << 4) : 0);
                    b3 = (byte)(b3 | (byte)num20);
                    if (tile2.actuator())
                    {
                        b2 = (byte)(b2 | 2);
                    }
                    if (tile2.inActive())
                    {
                        b2 = (byte)(b2 | 4);
                    }
                    if (tile2.wire4())
                    {
                        b2 = (byte)(b2 | 0x20);
                    }
                    if (tile2.wall > 255)
                    {
                        array4[num5] = (byte)(tile2.wall >> 8);
                        num5++;
                        b2 = (byte)(b2 | 0x40);
                    }
                    num6 = 2;
                    if (b2 != 0)
                    {
                        b3 = (byte)(b3 | 1);
                        array4[num6] = b2;
                        num6--;
                    }
                    if (b3 != 0)
                    {
                        b = (byte)(b | 1);
                        array4[num6] = b3;
                        num6--;
                    }
                    tile = tile2;
                }
            }
            if (num4 > 0)
            {
                array4[num5] = (byte)(num4 & 0xFF);
                num5++;
                if (num4 > 255)
                {
                    b = (byte)(b | 0x80);
                    array4[num5] = (byte)((num4 & 0xFF00) >> 8);
                    num5++;
                }
                else
                {
                    b = (byte)(b | 0x40);
                }
            }
            array4[num6] = b;
            writer.Write(array4, num6, num5 - num6);
            writer.Write(num);
            for (int k = 0; k < num; k++)
            {
                Chest chest = Main.chest[array[k]];
                writer.Write(array[k]);
                writer.Write((short)chest.x);
                writer.Write((short)chest.y);
                writer.Write(chest.name);
            }
            writer.Write(num2);
            for (int l = 0; l < num2; l++)
            {
                Sign sign = Main.sign[array2[l]];
                writer.Write(array2[l]);
                writer.Write((short)sign.x);
                writer.Write((short)sign.y);
                writer.Write(sign.text);
            }
            writer.Write(num3);
            for (int m = 0; m < num3; m++)
            {
                TileEntity.Write(writer, TileEntity.ByID[array3[m]]);
            }
        }

        public static void SendSquare(int Size, int X, int Y, MapData data, EPlayer eplr, int Number5 = 0)
        {
            byte[] array;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
                binaryWriter.BaseStream.Position = 2L;
                binaryWriter.Write((byte)20);
                WriteTiles(binaryWriter, Size, X, Y, data, Number5);
                long position = binaryWriter.BaseStream.Position;
                binaryWriter.BaseStream.Position = 0L;
                binaryWriter.Write((short)position);
                binaryWriter.BaseStream.Position = position;
                array = memoryStream.ToArray();
            }
            eplr.SendRawData(array);
        }

        private static void WriteTiles(BinaryWriter binaryWriter, int Size, int X, int Y, MapData data, int number5 = 0)
        {
            int num3 = Size;
            int num4 = X;
            int num5 = Y;
            if (num3 < 0)
            {
                num3 = 0;
            }
            if (num4 < num3)
            {
                num4 = num3;
            }
            if (num4 >= Main.maxTilesX + num3)
            {
                num4 = Main.maxTilesX - num3 - 1;
            }
            if (num5 < num3)
            {
                num5 = num3;
            }
            if (num5 >= Main.maxTilesY + num3)
            {
                num5 = Main.maxTilesY - num3 - 1;
            }
            if (number5 == 0)
            {
                binaryWriter.Write((ushort)(num3 & 0x7FFF));
            }
            else
            {
                binaryWriter.Write((ushort)((num3 & 0x7FFF) | 0x8000));
                binaryWriter.Write((byte)number5);
            }
            binaryWriter.Write((short)num4);
            binaryWriter.Write((short)num5);
            var appliedTiles = data.Tile;
            for (int l = num4; l < num4 + num3; l++)
            {
                for (int m = num5; m < num5 + num3; m++)
                {
                    BitsByte bb = (byte)0;
                    BitsByte bb2 = (byte)0;
                    byte b2 = 0;
                    byte b3 = 0;
                    ITile tile = appliedTiles[l - num4, m - num5];
                    bb[0] = tile.active();
                    bb[2] = (tile.wall > 0);
                    bb[3] = (tile.liquid > 0 && Main.netMode == 2);
                    bb[4] = tile.wire();
                    bb[5] = tile.halfBrick();
                    bb[6] = tile.actuator();
                    bb[7] = tile.inActive();
                    bb2[0] = tile.wire2();
                    bb2[1] = tile.wire3();
                    if (tile.active() && tile.color() > 0)
                    {
                        bb2[2] = true;
                        b2 = tile.color();
                    }
                    if (tile.wall > 0 && tile.wallColor() > 0)
                    {
                        bb2[3] = true;
                        b3 = tile.wallColor();
                    }
                    bb2 = (byte)((byte)bb2 + (byte)(tile.slope() << 4));
                    bb2[7] = tile.wire4();
                    binaryWriter.Write(bb);
                    binaryWriter.Write(bb2);
                    if (b2 > 0)
                    {
                        binaryWriter.Write(b2);
                    }
                    if (b3 > 0)
                    {
                        binaryWriter.Write(b3);
                    }
                    if (tile.active())
                    {
                        binaryWriter.Write(tile.type);
                        if (Main.tileFrameImportant[tile.type])
                        {
                            binaryWriter.Write(tile.frameX);
                            binaryWriter.Write(tile.frameY);
                        }
                    }
                    if (tile.wall > 0)
                    {
                        binaryWriter.Write(tile.wall);
                    }
                    if (tile.liquid > 0 && Main.netMode == 2)
                    {
                        binaryWriter.Write(tile.liquid);
                        binaryWriter.Write(tile.liquidType());
                    }
                }
            }
        }
        #endregion
    }
    class MapTools
    {
		public static int GetChestItemDrop(int ax, int ay, int type, MapManager.MapData map)
		{
			
			int num = map.Tile[ax, ay].frameX / 36;
			if (type == 467)
			{
				return Chest.chestItemSpawn2[num];
			}
			return Chest.chestItemSpawn[num];
		}
		public static void KillTile_GetTreeDrops(int i, int j, ITile tileCache, ref bool bonusWood, ref int dropItem, ref int secondaryItem, MapManager.MapData map)
		{
			map.GetRelative(i, j, out int Ax, out int Ay);
			if (tileCache.frameX >= 22 && tileCache.frameY >= 198)
			{
				if (Main.netMode != 1)
				{
					if (Utils.RANDOM.Next(2) == 0)
					{
						int k;
						for (k = j; map.Tile[Ax, Ay + k - j] != null && (!map.Tile[Ax, Ay + k - j].active() || !Main.tileSolid[map.Tile[Ax, Ay + k - j].type] || Main.tileSolidTop[map.Tile[Ax, Ay + k - j].type]); k++)
						{
						}
						if (map.Tile[Ax, Ay + k - j] != null)
						{
							ITile tile = map.Tile[Ax, Ay + k - j];
							if (tile.type == 2 || tile.type == 109 || tile.type == 477 || tile.type == 492 || tile.type == 147 || tile.type == 199 || tile.type == 23)
							{
								dropItem = 9;
								secondaryItem = 27;
							}
							else
							{
								dropItem = 9;
							}
						}
					}
					else
					{
						dropItem = 9;
					}
				}
			}
			else
			{
				dropItem = 9;
			}
			if (dropItem != 9)
			{
				return;
			}
			GetTreeBottom(i, j, out int x, out int y);
			map.GetAbslute(x, y, out int tempax, out int tempay);
			if (map.Tile[tempax, tempay].active())
			{
				switch (map.Tile[tempax, tempay].type)
				{
					case 23:
						dropItem = 619;
						break;
					case 60:
						dropItem = 620;
						break;
					case 109:
					case 492:
						dropItem = 621;
						break;
					case 199:
						dropItem = 911;
						break;
					case 70:
						if (Utils.RANDOM.Next(2) == 0)
						{
							dropItem = 183;
						}
						else
						{
							dropItem = 0;
						}
						break;
					case 147:
						dropItem = 2503;
						break;
				}
			}
			int num = Player.FindClosest(new Vector2(tempax * 16, tempay * 16), 16, 16);
			int axe = Main.player[num].inventory[Main.player[num].selectedItem].axe;
			if (Utils.RANDOM.Next(100) < axe || Main.rand.Next(3) == 0)
			{
				bonusWood = true;
			}
		}
		public static void GetTreeBottom(int i, int j, out int x, out int y)
		{
			x = i;
			y = j;
			ITile tileSafely = Framing.GetTileSafely(x, y);
			if (tileSafely.type == 323)
			{
				while (y < Main.maxTilesY - 50 && (!tileSafely.active() || tileSafely.type == 323))
				{
					y++;
					tileSafely = Framing.GetTileSafely(x, y);
				}
				return;
			}
			int num = tileSafely.frameX / 22;
			int num2 = tileSafely.frameY / 22;
			if (num == 3 && num2 <= 2)
			{
				x++;
			}
			else if (num == 4 && num2 >= 3 && num2 <= 5)
			{
				x--;
			}
			else if (num == 1 && num2 >= 6 && num2 <= 8)
			{
				x--;
			}
			else if (num == 2 && num2 >= 6 && num2 <= 8)
			{
				x++;
			}
			else if (num == 2 && num2 >= 9)
			{
				x++;
			}
			else if (num == 3 && num2 >= 9)
			{
				x--;
			}
			tileSafely = Framing.GetTileSafely(x, y);
			while (y < Main.maxTilesY - 50 && (!tileSafely.active() || TileID.Sets.IsATreeTrunk[tileSafely.type] || tileSafely.type == 72))
			{
				y++;
				tileSafely = Framing.GetTileSafely(x, y);
			}
		}
		public static void SetGemTreeDrops(int gemType, int seedType, ITile tileCache, ref int dropItem, ref int secondaryItem)
		{
			if (Main.rand.Next(10) == 0)
			{
				dropItem = gemType;
			}
			else
			{
				dropItem = 3;
			}
			if (tileCache.frameX >= 22 && tileCache.frameY >= 198 && Main.rand.Next(2) == 0)
			{
				secondaryItem = seedType;
			}
		}
		public static void dropXmasTree(int ax, int ay, int obj, MapManager.MapData map)
		{
			int num = ax;
			int num2 = ay;
			if (map.Tile[ax, ay].frameX < 10)
			{
				num -= map.Tile[ax, ay].frameX;
				num2 -= map.Tile[ax, ay].frameY;
			}
			int num3 = 0;
			if ((map.Tile[num, num2].frameY & 1) == 1)
			{
				num3++;
			}
			if ((map.Tile[num, num2].frameY & 2) == 2)
			{
				num3 += 2;
			}
			if ((map.Tile[num, num2].frameY & 4) == 4)
			{
				num3 += 4;
			}
			int num4 = 0;
			if ((map.Tile[num, num2].frameY & 8) == 8)
			{
				num4++;
			}
			if ((map.Tile[num, num2].frameY & 0x10) == 16)
			{
				num4 += 2;
			}
			if ((map.Tile[num, num2].frameY & 0x20) == 32)
			{
				num4 += 4;
			}
			int num5 = 0;
			if ((map.Tile[num, num2].frameY & 0x40) == 64)
			{
				num5++;
			}
			if ((map.Tile[num, num2].frameY & 0x80) == 128)
			{
				num5 += 2;
			}
			if ((map.Tile[num, num2].frameY & 0x100) == 256)
			{
				num5 += 4;
			}
			if ((map.Tile[num, num2].frameY & 0x200) == 512)
			{
				num5 += 8;
			}
			int num6 = 0;
			if ((map.Tile[num, num2].frameY & 0x400) == 1024)
			{
				num6++;
			}
			if ((map.Tile[num, num2].frameY & 0x800) == 2048)
			{
				num6 += 2;
			}
			if ((map.Tile[num, num2].frameY & 0x1000) == 4096)
			{
				num6 += 4;
			}
			if ((map.Tile[num, num2].frameY & 0x2000) == 8192)
			{
				num6 += 8;
			}
			if (obj == 0 && num3 > 0)
			{
				int number = Item.NewItem(ax * 16, ay * 16, 16, 16, 1874 + num3 - 1);
				if (Main.netMode == 1)
				{
					NetMessage.SendData(21, -1, -1, null, number, 1f);
				}
			}
			else if (obj == 1 && num4 > 0)
			{
				int number2 = Item.NewItem(ax * 16, ay * 16, 16, 16, 1878 + num4 - 1);
				if (Main.netMode == 1)
				{
					NetMessage.SendData(21, -1, -1, null, number2, 1f);
				}
			}
			else if (obj == 2 && num5 > 0)
			{
				int number3 = Item.NewItem(ax * 16, ay * 16, 16, 16, 1884 + num5 - 1);
				if (Main.netMode == 1)
				{
					NetMessage.SendData(21, -1, -1, null, number3, 1f);
				}
			}
			else if (obj == 3 && num6 > 0)
			{
				int number4 = Item.NewItem(ax * 16, ay * 16, 16, 16, 1895 + num6 - 1);
				if (Main.netMode == 1)
				{
					NetMessage.SendData(21, -1, -1, null, number4, 1f);
				}
			}
		}
		public static void KillTile_GetItemDrops(int Rx, int Ry, ITile tileCache, MapManager.MapData map, out int dropItem, out int dropItemStack, out int secondaryItem, out int secondaryItemStack, bool includeLargeObjectDrops = false)
		{
			map.GetRelative(Rx, Ry, out int Ax, out int Ay);
			dropItem = 0;
			dropItemStack = 1;
			secondaryItem = 0;
			secondaryItemStack = 1;
			int num = 0;
			if (includeLargeObjectDrops)
			{
				switch (tileCache.type)
				{
					case 21:
					case 467:
						dropItem = GetChestItemDrop(Ax, Ay, tileCache.type, map);
						break;
					case 88:
						num = tileCache.frameX / 54;
						dropItem = WorldGen.GetDresserItemDrop(num);
						break;
				}
			}
			switch (tileCache.type)
			{
				case 10:
				case 11:
				case 12:
				case 14:
				case 15:
				case 16:
				case 17:
				case 18:
				case 20:
				case 21:
				case 26:
				case 27:
				case 28:
				case 29:
				case 31:
				case 32:
				case 34:
				case 35:
				case 42:
				case 55:
				case 69:
				case 77:
				case 79:
				case 82:
				case 85:
				case 86:
				case 87:
				case 88:
				case 89:
				case 90:
				case 91:
				case 92:
				case 93:
				case 94:
				case 95:
				case 96:
				case 97:
				case 98:
				case 99:
				case 100:
				case 101:
				case 102:
				case 103:
				case 104:
				case 105:
				case 106:
				case 113:
				case 114:
				case 115:
				case 125:
				case 126:
				case 127:
				case 128:
				case 132:
				case 133:
				case 134:
				case 138:
				case 139:
				case 142:
				case 143:
				case 162:
				case 165:
				case 172:
				case 173:
				case 184:
				case 185:
				case 186:
				case 187:
				case 192:
				case 205:
				case 207:
				case 209:
				case 212:
				case 215:
				case 216:
				case 217:
				case 218:
				case 219:
				case 220:
				case 228:
				case 231:
				case 233:
				case 235:
				case 236:
				case 237:
				case 238:
				case 240:
				case 241:
				case 242:
				case 243:
				case 244:
				case 245:
				case 246:
				case 247:
				case 254:
				case 269:
				case 270:
				case 271:
				case 275:
				case 276:
				case 277:
				case 278:
				case 279:
				case 280:
				case 281:
				case 282:
				case 283:
				case 285:
				case 286:
				case 287:
				case 288:
				case 289:
				case 290:
				case 291:
				case 292:
				case 293:
				case 294:
				case 295:
				case 296:
				case 297:
				case 298:
				case 299:
				case 300:
				case 301:
				case 302:
				case 303:
				case 304:
				case 305:
				case 306:
				case 307:
				case 308:
				case 309:
				case 310:
				case 316:
				case 317:
				case 318:
				case 319:
				case 320:
				case 334:
				case 335:
				case 337:
				case 338:
				case 339:
				case 349:
				case 352:
				case 354:
				case 355:
				case 356:
				case 358:
				case 359:
				case 360:
				case 361:
				case 362:
				case 363:
				case 364:
				case 373:
				case 374:
				case 375:
				case 376:
				case 377:
				case 378:
				case 384:
				case 386:
				case 387:
				case 388:
				case 389:
				case 390:
				case 391:
				case 392:
				case 393:
				case 394:
				case 395:
				case 405:
				case 406:
				case 410:
				case 411:
				case 412:
				case 413:
				case 414:
				case 425:
				case 440:
				case 441:
				case 443:
				case 444:
				case 452:
				case 453:
				case 454:
				case 455:
				case 456:
				case 457:
				case 461:
				case 462:
				case 463:
				case 464:
				case 465:
				case 466:
				case 467:
				case 468:
				case 469:
				case 470:
				case 471:
				case 475:
				case 480:
				case 481:
				case 482:
				case 483:
				case 484:
				case 485:
				case 486:
				case 487:
				case 488:
				case 489:
				case 490:
				case 491:
				case 493:
				case 497:
				case 499:
				case 504:
				case 505:
				case 506:
				case 509:
				case 510:
				case 511:
				case 518:
				case 521:
				case 522:
				case 523:
				case 524:
				case 525:
				case 526:
				case 527:
				case 529:
				case 530:
				case 531:
				case 532:
				case 533:
				case 538:
				case 542:
				case 543:
				case 544:
				case 545:
				case 547:
				case 548:
				case 549:
				case 550:
				case 551:
				case 552:
				case 553:
				case 554:
				case 555:
				case 556:
				case 558:
				case 559:
				case 560:
				case 564:
				case 565:
				case 567:
				case 568:
				case 569:
				case 570:
				case 572:
				case 573:
				case 580:
				case 581:
				case 582:
				case 590:
				case 591:
				case 592:
				case 594:
				case 595:
				case 597:
				case 598:
				case 599:
				case 600:
				case 601:
				case 602:
				case 603:
				case 604:
				case 605:
				case 606:
				case 607:
				case 608:
				case 609:
				case 610:
				case 611:
				case 612:
				case 613:
				case 614:
				case 615:
				case 617:
					break;
				case 179:
				case 180:
				case 181:
				case 182:
				case 183:
				case 381:
				case 534:
				case 536:
				case 539:
					dropItem = 3;
					break;
				case 512:
				case 513:
				case 514:
				case 515:
				case 516:
				case 517:
				case 535:
				case 537:
				case 540:
					dropItem = 129;
					break;
				case 0:
				case 2:
				case 109:
				case 199:
				case 477:
				case 492:
					dropItem = 2;
					break;
				case 426:
					dropItem = 3621;
					break;
				case 430:
					dropItem = 3633;
					break;
				case 431:
					dropItem = 3634;
					break;
				case 432:
					dropItem = 3635;
					break;
				case 433:
					dropItem = 3636;
					break;
				case 434:
					dropItem = 3637;
					break;
				case 427:
					dropItem = 3622;
					break;
				case 435:
					dropItem = 3638;
					break;
				case 436:
					dropItem = 3639;
					break;
				case 437:
					dropItem = 3640;
					break;
				case 438:
					dropItem = 3641;
					break;
				case 439:
					dropItem = 3642;
					break;
				case 446:
					dropItem = 3736;
					break;
				case 447:
					dropItem = 3737;
					break;
				case 448:
					dropItem = 3738;
					break;
				case 449:
					dropItem = 3739;
					break;
				case 450:
					dropItem = 3740;
					break;
				case 451:
					dropItem = 3741;
					break;
				case 368:
					dropItem = 3086;
					break;
				case 369:
					dropItem = 3087;
					break;
				case 367:
					dropItem = 3081;
					break;
				case 379:
					dropItem = 3214;
					break;
				case 353:
					dropItem = 2996;
					break;
				case 365:
					dropItem = 3077;
					break;
				case 366:
					dropItem = 3078;
					break;
				case 357:
					dropItem = 3066;
					break;
				case 1:
					dropItem = 3;
					break;
				case 442:
					dropItem = 3707;
					break;
				case 383:
					dropItem = 620;
					break;
				case 315:
					dropItem = 2435;
					break;
				case 330:
					dropItem = 71;
					break;
				case 331:
					dropItem = 72;
					break;
				case 332:
					dropItem = 73;
					break;
				case 333:
					dropItem = 74;
					break;
				case 408:
					dropItem = 3460;
					break;
				case 409:
					dropItem = 3461;
					break;
				case 415:
					dropItem = 3573;
					break;
				case 416:
					dropItem = 3574;
					break;
				case 417:
					dropItem = 3575;
					break;
				case 418:
					dropItem = 3576;
					break;
				case 421:
					dropItem = 3609;
					break;
				case 422:
					dropItem = 3610;
					break;
				case 498:
					dropItem = 4139;
					break;
				case 424:
					dropItem = 3616;
					break;
				case 445:
					dropItem = 3725;
					break;
				case 429:
					dropItem = 3629;
					break;
				case 272:
					dropItem = 1344;
					break;
				case 273:
					dropItem = 2119;
					break;
				case 274:
					dropItem = 2120;
					break;
				case 618:
					dropItem = 4962;
					break;
				case 460:
					dropItem = 3756;
					break;
				case 541:
					dropItem = 4392;
					break;
				case 472:
					dropItem = 3951;
					break;
				case 473:
					dropItem = 3953;
					break;
				case 474:
					dropItem = 3955;
					break;
				case 478:
					dropItem = 4050;
					break;
				case 479:
					dropItem = 4051;
					break;
				case 496:
					dropItem = 4091;
					break;
				case 495:
					dropItem = 4090;
					break;
				case 346:
					dropItem = 2792;
					break;
				case 347:
					dropItem = 2793;
					break;
				case 348:
					dropItem = 2794;
					break;
				case 350:
					dropItem = 2860;
					break;
				case 336:
					dropItem = 2701;
					break;
				case 340:
					dropItem = 2751;
					break;
				case 341:
					dropItem = 2752;
					break;
				case 342:
					dropItem = 2753;
					break;
				case 343:
					dropItem = 2754;
					break;
				case 344:
					dropItem = 2755;
					break;
				case 351:
					dropItem = 2868;
					break;
				case 500:
					dropItem = 4229;
					break;
				case 501:
					dropItem = 4230;
					break;
				case 502:
					dropItem = 4231;
					break;
				case 503:
					dropItem = 4232;
					break;
				case 546:
				case 557:
					dropItem = 4422;
					break;
				case 561:
					dropItem = 4554;
					break;
				case 574:
					dropItem = 4717;
					break;
				case 575:
					dropItem = 4718;
					break;
				case 576:
					dropItem = 4719;
					break;
				case 577:
					dropItem = 4720;
					break;
				case 578:
					dropItem = 4721;
					break;
				case 562:
					dropItem = 4564;
					break;
				case 571:
					dropItem = 4564;
					dropItemStack = Utils.RANDOM.Next(1, 3);
					break;
				case 563:
					dropItem = 4547;
					break;
				case 251:
					dropItem = 1725;
					break;
				case 252:
					dropItem = 1727;
					break;
				case 253:
					dropItem = 1729;
					break;
				case 325:
					dropItem = 2692;
					break;
				case 370:
					dropItem = 3100;
					break;
				case 396:
					dropItem = 3271;
					break;
				case 400:
					dropItem = 3276;
					break;
				case 401:
					dropItem = 3277;
					break;
				case 403:
					dropItem = 3339;
					break;
				case 397:
					dropItem = 3272;
					break;
				case 398:
					dropItem = 3274;
					break;
				case 399:
					dropItem = 3275;
					break;
				case 402:
					dropItem = 3338;
					break;
				case 404:
					dropItem = 3347;
					break;
				case 407:
					dropItem = 3380;
					break;
				case 579:
					dropItem = 4761;
					break;
				case 593:
					dropItem = 4868;
					break;
				case 170:
					dropItem = 1872;
					break;
				case 284:
					dropItem = 2173;
					break;
				case 214:
					dropItem = 85;
					break;
				case 213:
					dropItem = 965;
					break;
				case 211:
					dropItem = 947;
					break;
				case 6:
					dropItem = 11;
					break;
				case 7:
					dropItem = 12;
					break;
				case 8:
					dropItem = 13;
					break;
				case 9:
					dropItem = 14;
					break;
				case 202:
					dropItem = 824;
					break;
				case 234:
					dropItem = 1246;
					break;
				case 226:
					dropItem = 1101;
					break;
				case 224:
					dropItem = 1103;
					break;
				case 36:
					dropItem = 1869;
					break;
				case 311:
					dropItem = 2260;
					break;
				case 312:
					dropItem = 2261;
					break;
				case 313:
					dropItem = 2262;
					break;
				case 229:
					dropItem = 1125;
					break;
				case 230:
					dropItem = 1127;
					break;
				case 221:
					dropItem = 1104;
					break;
				case 222:
					dropItem = 1105;
					break;
				case 223:
					dropItem = 1106;
					break;
				case 248:
					dropItem = 1589;
					break;
				case 249:
					dropItem = 1591;
					break;
				case 250:
					dropItem = 1593;
					break;
				case 191:
					dropItem = 9;
					break;
				case 203:
					dropItem = 836;
					break;
				case 204:
					dropItem = 880;
					break;
				case 166:
					dropItem = 699;
					break;
				case 167:
					dropItem = 700;
					break;
				case 168:
					dropItem = 701;
					break;
				case 169:
					dropItem = 702;
					break;
				case 123:
					dropItem = 424;
					break;
				case 124:
					dropItem = 480;
					break;
				case 157:
					dropItem = 619;
					break;
				case 158:
					dropItem = 620;
					break;
				case 159:
					dropItem = 621;
					break;
				case 161:
					dropItem = 664;
					break;
				case 206:
					dropItem = 883;
					break;
				case 232:
					dropItem = 1150;
					break;
				case 198:
					dropItem = 775;
					break;
				case 314:
					dropItem = Minecart.GetTrackItem(tileCache);
					break;
				case 189:
					dropItem = 751;
					break;
				case 195:
					dropItem = 763;
					break;
				case 194:
					dropItem = 154;
					break;
				case 193:
					dropItem = 762;
					break;
				case 196:
					dropItem = 765;
					break;
				case 197:
					dropItem = 767;
					break;
				case 22:
					dropItem = 56;
					break;
				case 140:
					dropItem = 577;
					break;
				case 23:
					dropItem = 2;
					break;
				case 25:
					dropItem = 61;
					break;
				case 30:
					dropItem = 9;
					break;
				case 208:
					dropItem = 911;
					break;
				case 372:
					dropItem = 3117;
					break;
				case 371:
					dropItem = 3113;
					break;
				case 174:
					dropItem = 713;
					break;
				case 37:
					dropItem = 116;
					break;
				case 38:
					dropItem = 129;
					break;
				case 39:
					dropItem = 131;
					break;
				case 40:
					dropItem = 133;
					break;
				case 41:
					dropItem = 134;
					break;
				case 43:
					dropItem = 137;
					break;
				case 44:
					dropItem = 139;
					break;
				case 45:
					dropItem = 141;
					break;
				case 46:
					dropItem = 143;
					break;
				case 47:
					dropItem = 145;
					break;
				case 48:
					dropItem = 147;
					break;
				case 49:
					dropItem = 148;
					break;
				case 51:
					dropItem = 150;
					break;
				case 53:
					dropItem = 169;
					break;
				case 151:
					dropItem = 607;
					break;
				case 152:
					dropItem = 609;
					break;
				case 56:
					dropItem = 173;
					break;
				case 57:
					dropItem = 172;
					break;
				case 58:
					dropItem = 174;
					break;
				case 70:
					dropItem = 176;
					break;
				case 75:
					dropItem = 192;
					break;
				case 76:
					dropItem = 214;
					break;
				case 78:
					dropItem = 222;
					break;
				case 81:
					dropItem = 275;
					break;
				case 80:
					dropItem = 276;
					break;
				case 188:
					dropItem = 276;
					break;
				case 107:
					dropItem = 364;
					break;
				case 108:
					dropItem = 365;
					break;
				case 111:
					dropItem = 366;
					break;
				case 150:
					dropItem = 604;
					break;
				case 112:
					dropItem = 370;
					break;
				case 116:
					dropItem = 408;
					break;
				case 117:
					dropItem = 409;
					break;
				case 118:
					dropItem = 412;
					break;
				case 119:
					dropItem = 413;
					break;
				case 120:
					dropItem = 414;
					break;
				case 121:
					dropItem = 415;
					break;
				case 122:
					dropItem = 416;
					break;
				case 136:
					dropItem = 538;
					break;
				case 385:
					dropItem = 3234;
					break;
				case 141:
					dropItem = 580;
					break;
				case 145:
					dropItem = 586;
					break;
				case 146:
					dropItem = 591;
					break;
				case 147:
					dropItem = 593;
					break;
				case 148:
					dropItem = 594;
					break;
				case 153:
					dropItem = 611;
					break;
				case 154:
					dropItem = 612;
					break;
				case 155:
					dropItem = 613;
					break;
				case 156:
					dropItem = 614;
					break;
				case 160:
					dropItem = 662;
					break;
				case 175:
					dropItem = 717;
					break;
				case 176:
					dropItem = 718;
					break;
				case 177:
					dropItem = 719;
					break;
				case 163:
					dropItem = 833;
					break;
				case 164:
					dropItem = 834;
					break;
				case 200:
					dropItem = 835;
					break;
				case 210:
					dropItem = 937;
					break;
				case 130:
					dropItem = 511;
					break;
				case 131:
					dropItem = 512;
					break;
				case 321:
					dropItem = 2503;
					break;
				case 322:
					dropItem = 2504;
					break;
				case 54:
					dropItem = 170;
					break;
				case 326:
					dropItem = 2693;
					break;
				case 327:
					dropItem = 2694;
					break;
				case 458:
					dropItem = 3754;
					break;
				case 459:
					dropItem = 3755;
					break;
				case 345:
					dropItem = 2787;
					break;
				case 328:
					dropItem = 2695;
					break;
				case 329:
					dropItem = 2697;
					break;
				case 507:
					dropItem = 4277;
					break;
				case 508:
					dropItem = 4278;
					break;
				case 255:
				case 256:
				case 257:
				case 258:
				case 259:
				case 260:
				case 261:
					dropItem = 1970 + tileCache.type - 255;
					break;
				case 262:
				case 263:
				case 264:
				case 265:
				case 266:
				case 267:
				case 268:
					dropItem = 1970 + tileCache.type - 262;
					break;
				case 59:
				case 60:
					dropItem = 176;
					break;
				case 190:
					dropItem = 183;
					break;
				case 63:
				case 64:
				case 65:
				case 66:
				case 67:
				case 68:
					dropItem = tileCache.type - 63 + 177;
					break;
				case 566:
					dropItem = 999;
					break;
				case 129:
					if (tileCache.frameX >= 324)
					{
						dropItem = 4988;
					}
					else
					{
						dropItem = 502;
					}
					break;
				case 3:
					if (tileCache.frameX == 144)
					{
						dropItem = 5;
					}
					else if (WorldGen.KillTile_ShouldDropSeeds(Rx, Ry))
					{
						dropItem = 283;
					}
					break;
				case 519:
					if (tileCache.frameY == 90 && Utils.RANDOM.Next(2) == 0)
					{
						dropItem = 183;
					}
					break;
				case 528:
					if (Utils.RANDOM.Next(2) == 0)
					{
						dropItem = 183;
					}
					break;
				case 110:
					if (tileCache.frameX == 144)
					{
						dropItem = 5;
					}
					break;
				case 24:
					if (tileCache.frameX == 144)
					{
						dropItem = 60;
					}
					break;
				case 201:
					if (tileCache.frameX == 270)
					{
						dropItem = 2887;
					}
					break;
				case 73:
					if (WorldGen.KillTile_ShouldDropSeeds(Rx, Ry))
					{
						dropItem = 283;
					}
					break;
				case 52:
				case 62:
				case 382:
					if (Main.rand.Next(2) == 0 && WorldGen.GetPlayerForTile(Rx, Ry).cordage)
					{
						dropItem = 2996;
					}
					break;
				case 227:
					num = tileCache.frameX / 34;
					dropItem = 1107 + num;
					if (num >= 8 && num <= 11)
					{
						dropItem = 3385 + num - 8;
					}
					break;
				case 4:
					num = tileCache.frameY / 22;
					switch (num)
					{
						case 0:
							dropItem = 8;
							break;
						case 8:
							dropItem = 523;
							break;
						case 9:
							dropItem = 974;
							break;
						case 10:
							dropItem = 1245;
							break;
						case 11:
							dropItem = 1333;
							break;
						case 12:
							dropItem = 2274;
							break;
						case 13:
							dropItem = 3004;
							break;
						case 14:
							dropItem = 3045;
							break;
						case 15:
							dropItem = 3114;
							break;
						case 16:
							dropItem = 4383;
							break;
						case 17:
							dropItem = 4384;
							break;
						case 18:
							dropItem = 4385;
							break;
						case 19:
							dropItem = 4386;
							break;
						case 20:
							dropItem = 4387;
							break;
						case 21:
							dropItem = 4388;
							break;
						default:
							dropItem = 426 + num;
							break;
					}
					break;
				case 239:
					num = tileCache.frameX / 18;
					if (num == 0)
					{
						dropItem = 20;
					}
					if (num == 1)
					{
						dropItem = 703;
					}
					if (num == 2)
					{
						dropItem = 22;
					}
					if (num == 3)
					{
						dropItem = 704;
					}
					if (num == 4)
					{
						dropItem = 21;
					}
					if (num == 5)
					{
						dropItem = 705;
					}
					if (num == 6)
					{
						dropItem = 19;
					}
					if (num == 7)
					{
						dropItem = 706;
					}
					if (num == 8)
					{
						dropItem = 57;
					}
					if (num == 9)
					{
						dropItem = 117;
					}
					if (num == 10)
					{
						dropItem = 175;
					}
					if (num == 11)
					{
						dropItem = 381;
					}
					if (num == 12)
					{
						dropItem = 1184;
					}
					if (num == 13)
					{
						dropItem = 382;
					}
					if (num == 14)
					{
						dropItem = 1191;
					}
					if (num == 15)
					{
						dropItem = 391;
					}
					if (num == 16)
					{
						dropItem = 1198;
					}
					if (num == 17)
					{
						dropItem = 1006;
					}
					if (num == 18)
					{
						dropItem = 1225;
					}
					if (num == 19)
					{
						dropItem = 1257;
					}
					if (num == 20)
					{
						dropItem = 1552;
					}
					if (num == 21)
					{
						dropItem = 3261;
					}
					if (num == 22)
					{
						dropItem = 3467;
					}
					break;
				case 380:
					num = tileCache.frameY / 18;
					dropItem = 3215 + num;
					break;
				case 5:
				case 596:
				case 616:
					{
						bool bonusWood = false;
						KillTile_GetTreeDrops(Rx, Ry, tileCache, ref bonusWood, ref dropItem, ref secondaryItem, map);
						if (bonusWood)
						{
							dropItemStack++;
						}
						break;
					}
				case 323:
					{
						dropItem = 2504;
						if (tileCache.frameX <= 132 && tileCache.frameX >= 88)
						{
							secondaryItem = 27;
						}
						int j;
						for (j = Ay; !map.Tile[Ax, j].active() || !Main.tileSolid[map.Tile[Ax, j].type]; j++)
						{
						}
						if (map.Tile[Ax, j].active())
						{
							switch (map.Tile[Ax, j].type)
							{
								case 234:
									dropItem = 911;
									break;
								case 116:
									dropItem = 621;
									break;
								case 112:
									dropItem = 619;
									break;
							}
						}
						break;
					}
				case 171:
					if (tileCache.frameX >= 10)
					{
						dropXmasTree(Ax, Ay, 0, map);
						dropXmasTree(Ax, Ay, 1, map);
						dropXmasTree(Ax, Ay, 2, map);
						dropXmasTree(Ax, Ay, 3, map);
					}
					break;
				case 324:
					switch (tileCache.frameY / 22)
					{
						case 0:
							dropItem = 2625;
							break;
						case 1:
							dropItem = 2626;
							break;
						case 2:
							dropItem = 4072;
							break;
						case 3:
							dropItem = 4073;
							break;
						case 4:
							dropItem = 4071;
							break;
					}
					break;
				case 419:
					switch (tileCache.frameX / 18)
					{
						case 0:
							dropItem = 3602;
							break;
						case 1:
							dropItem = 3618;
							break;
						case 2:
							dropItem = 3663;
							break;
					}
					break;
				case 428:
					switch (tileCache.frameY / 18)
					{
						case 0:
							dropItem = 3630;
							break;
						case 1:
							dropItem = 3632;
							break;
						case 2:
							dropItem = 3631;
							break;
						case 3:
							dropItem = 3626;
							break;
					}
					//PressurePlateHelper.DestroyPlate(new Point(x, y));  这个似乎是压力板?
					break;
				case 420:
					switch (tileCache.frameY / 18)
					{
						case 0:
							dropItem = 3603;
							break;
						case 1:
							dropItem = 3604;
							break;
						case 2:
							dropItem = 3605;
							break;
						case 3:
							dropItem = 3606;
							break;
						case 4:
							dropItem = 3607;
							break;
						case 5:
							dropItem = 3608;
							break;
					}
					break;
				case 476:
					dropItem = 4040;
					break;
				case 494:
					dropItem = 4089;
					break;
				case 423:
					//TELogicSensor.Kill(x, y);  逻辑传感器
					switch (tileCache.frameY / 18)
					{
						case 0:
							dropItem = 3613;
							break;
						case 1:
							dropItem = 3614;
							break;
						case 2:
							dropItem = 3615;
							break;
						case 3:
							dropItem = 3726;
							break;
						case 4:
							dropItem = 3727;
							break;
						case 5:
							dropItem = 3728;
							break;
						case 6:
							dropItem = 3729;
							break;
					}
					break;
				case 520:
					dropItem = 4326;
					break;
				case 225:
					if (Main.rand.Next(3) == 0)
					{
						tileCache.honey(honey: true);
						tileCache.liquid = byte.MaxValue;
						break;
					}
					dropItem = 1124;
					if (Main.netMode != 1 && Main.rand.Next(2) == 0)
					{
						int num3 = 1;
						if (Main.rand.Next(3) == 0)
						{
							num3 = 2;
						}
						for (int i = 0; i < num3; i++)
						{
							int type = Main.rand.Next(210, 212);
							int num4 = NPC.NewNPC(Rx * 16 + 8, Ry * 16 + 15, type, 1);
							Main.npc[num4].velocity.X = (float)Main.rand.Next(-200, 201) * 0.002f;
							Main.npc[num4].velocity.Y = (float)Main.rand.Next(-200, 201) * 0.002f;
							Main.npc[num4].netUpdate = true;
						}
					}
					break;
				case 178:
					switch (tileCache.frameX / 18)
					{
						case 0:
							dropItem = 181;
							break;
						case 1:
							dropItem = 180;
							break;
						case 2:
							dropItem = 177;
							break;
						case 3:
							dropItem = 179;
							break;
						case 4:
							dropItem = 178;
							break;
						case 5:
							dropItem = 182;
							break;
						case 6:
							dropItem = 999;
							break;
					}
					break;
				case 149:
					if (tileCache.frameX == 0 || tileCache.frameX == 54)
					{
						dropItem = 596;
					}
					else if (tileCache.frameX == 18 || tileCache.frameX == 72)
					{
						dropItem = 597;
					}
					else if (tileCache.frameX == 36 || tileCache.frameX == 90)
					{
						dropItem = 598;
					}
					break;
				case 13:
					switch (tileCache.frameX / 18)
					{
						case 1:
							dropItem = 28;
							break;
						case 2:
							dropItem = 110;
							break;
						case 3:
							dropItem = 350;
							break;
						case 4:
							dropItem = 351;
							break;
						case 5:
							dropItem = 2234;
							break;
						case 6:
							dropItem = 2244;
							break;
						case 7:
							dropItem = 2257;
							break;
						case 8:
							dropItem = 2258;
							break;
						default:
							dropItem = 31;
							break;
					}
					break;
				case 19:
					num = tileCache.frameY / 18;
					switch (num)
					{
						case 0:
							dropItem = 94;
							break;
						case 1:
							dropItem = 631;
							break;
						case 2:
							dropItem = 632;
							break;
						case 3:
							dropItem = 633;
							break;
						case 4:
							dropItem = 634;
							break;
						case 5:
							dropItem = 913;
							break;
						case 6:
							dropItem = 1384;
							break;
						case 7:
							dropItem = 1385;
							break;
						case 8:
							dropItem = 1386;
							break;
						case 9:
							dropItem = 1387;
							break;
						case 10:
							dropItem = 1388;
							break;
						case 11:
							dropItem = 1389;
							break;
						case 12:
							dropItem = 1418;
							break;
						case 13:
							dropItem = 1457;
							break;
						case 14:
							dropItem = 1702;
							break;
						case 15:
							dropItem = 1796;
							break;
						case 16:
							dropItem = 1818;
							break;
						case 17:
							dropItem = 2518;
							break;
						case 18:
							dropItem = 2549;
							break;
						case 19:
							dropItem = 2566;
							break;
						case 20:
							dropItem = 2581;
							break;
						case 21:
							dropItem = 2627;
							break;
						case 22:
							dropItem = 2628;
							break;
						case 23:
							dropItem = 2629;
							break;
						case 24:
							dropItem = 2630;
							break;
						case 25:
							dropItem = 2744;
							break;
						case 26:
							dropItem = 2822;
							break;
						case 27:
							dropItem = 3144;
							break;
						case 28:
							dropItem = 3146;
							break;
						case 29:
							dropItem = 3145;
							break;
						case 30:
						case 31:
						case 32:
						case 33:
						case 34:
						case 35:
							dropItem = 3903 + num - 30;
							break;
						default:
							switch (num)
							{
								case 36:
									dropItem = 3945;
									break;
								case 37:
									dropItem = 3957;
									break;
								case 38:
									dropItem = 4159;
									break;
								case 39:
									dropItem = 4180;
									break;
								case 40:
									dropItem = 4201;
									break;
								case 41:
									dropItem = 4222;
									break;
								case 42:
									dropItem = 4311;
									break;
								case 43:
									dropItem = 4416;
									break;
								case 44:
									dropItem = 4580;
									break;
							}
							break;
					}
					break;
				case 33:
					num = tileCache.frameY / 22;
					dropItem = 105;
					switch (num)
					{
						case 1:
							dropItem = 1405;
							break;
						case 2:
							dropItem = 1406;
							break;
						case 3:
							dropItem = 1407;
							break;
						case 4:
						case 5:
						case 6:
						case 7:
						case 8:
						case 9:
						case 10:
						case 11:
						case 12:
						case 13:
							dropItem = 2045 + num - 4;
							break;
						default:
							if (num >= 14 && num <= 16)
							{
								dropItem = 2153 + num - 14;
								break;
							}
							switch (num)
							{
								case 17:
									dropItem = 2236;
									break;
								case 18:
									dropItem = 2523;
									break;
								case 19:
									dropItem = 2542;
									break;
								case 20:
									dropItem = 2556;
									break;
								case 21:
									dropItem = 2571;
									break;
								case 22:
									dropItem = 2648;
									break;
								case 23:
									dropItem = 2649;
									break;
								case 24:
									dropItem = 2650;
									break;
								case 25:
									dropItem = 2651;
									break;
								case 26:
									dropItem = 2818;
									break;
								case 27:
									dropItem = 3171;
									break;
								case 28:
									dropItem = 3173;
									break;
								case 29:
									dropItem = 3172;
									break;
								case 30:
									dropItem = 3890;
									break;
								case 31:
									dropItem = 3936;
									break;
								case 32:
									dropItem = 3962;
									break;
								case 33:
									dropItem = 4150;
									break;
								case 34:
									dropItem = 4171;
									break;
								case 35:
									dropItem = 4192;
									break;
								case 36:
									dropItem = 4213;
									break;
								case 37:
									dropItem = 4303;
									break;
								case 38:
									dropItem = 4571;
									break;
							}
							break;
					}
					break;
				case 137:
					num = tileCache.frameY / 18;
					if (num == 0)
					{
						dropItem = 539;
					}
					if (num == 1)
					{
						dropItem = 1146;
					}
					if (num == 2)
					{
						dropItem = 1147;
					}
					if (num == 3)
					{
						dropItem = 1148;
					}
					if (num == 4)
					{
						dropItem = 1149;
					}
					break;
				case 135:
					num = tileCache.frameY / 18;
					if (num == 0)
					{
						dropItem = 529;
					}
					if (num == 1)
					{
						dropItem = 541;
					}
					if (num == 2)
					{
						dropItem = 542;
					}
					if (num == 3)
					{
						dropItem = 543;
					}
					if (num == 4)
					{
						dropItem = 852;
					}
					if (num == 5)
					{
						dropItem = 853;
					}
					if (num == 6)
					{
						dropItem = 1151;
					}
					break;
				case 144:
					if (tileCache.frameX == 0)
					{
						dropItem = 583;
					}
					if (tileCache.frameX == 18)
					{
						dropItem = 584;
					}
					if (tileCache.frameX == 36)
					{
						dropItem = 585;
					}
					if (tileCache.frameX == 54)
					{
						dropItem = 4484;
					}
					if (tileCache.frameX == 72)
					{
						dropItem = 4485;
					}
					break;
				case 61:
				case 74:
					if (tileCache.frameX == 144 && tileCache.type == 61)
					{
						dropItem = 331;
						dropItemStack = Main.rand.Next(2, 4);
					}
					else if (tileCache.frameX == 162 && tileCache.type == 61)
					{
						dropItem = 223;
					}
					else if (tileCache.frameX >= 108 && tileCache.frameX <= 126 && tileCache.type == 61 && Main.rand.Next(20) == 0)
					{
						dropItem = 208;
					}
					else if (Main.rand.Next(100) == 0)
					{
						dropItem = 195;
					}
					break;
				case 71:
				case 72:
					if (Main.rand.Next(40) == 0)
					{
						dropItem = 194;
					}
					else if (Main.rand.Next(2) == 0)
					{
						dropItem = 183;
					}
					break;
				case 50:
					if (tileCache.frameX == 90)
					{
						dropItem = 165;
					}
					else
					{
						dropItem = 149;
					}
					break;
				case 83:
				case 84:
					{
						num = tileCache.frameX / 18;
						dropItem = 313 + num;
						int num2 = 307 + num;
						if (num == 6)
						{
							dropItem = 2358;
							num2 = 2357;
						}
						bool flag = WorldGen.IsHarvestableHerbWithSeed(tileCache.type, num);
						if (WorldGen.GetPlayerForTile(Rx, Ry).HeldItem.type == 213)
						{
							dropItemStack = Main.rand.Next(1, 3);
							secondaryItem = num2;
							secondaryItemStack = Main.rand.Next(1, 6);
						}
						else if (flag)
						{
							secondaryItem = num2;
							secondaryItemStack = Main.rand.Next(1, 4);
						}
						break;
					}
				case 589:
					SetGemTreeDrops(999, 4857, tileCache, ref dropItem, ref secondaryItem);
					if (dropItem == 3)
					{
						dropItemStack = Main.rand.Next(1, 3);
					}
					break;
				case 584:
					SetGemTreeDrops(181, 4852, tileCache, ref dropItem, ref secondaryItem);
					if (dropItem == 3)
					{
						dropItemStack = Main.rand.Next(1, 3);
					}
					break;
				case 583:
					SetGemTreeDrops(180, 4851, tileCache, ref dropItem, ref secondaryItem);
					if (dropItem == 3)
					{
						dropItemStack = Main.rand.Next(1, 3);
					}
					break;
				case 586:
					SetGemTreeDrops(179, 4854, tileCache, ref dropItem, ref secondaryItem);
					if (dropItem == 3)
					{
						dropItemStack = Main.rand.Next(1, 3);
					}
					break;
				case 585:
					SetGemTreeDrops(177, 4853, tileCache, ref dropItem, ref secondaryItem);
					if (dropItem == 3)
					{
						dropItemStack = Main.rand.Next(1, 3);
					}
					break;
				case 587:
					SetGemTreeDrops(178, 4855, tileCache, ref dropItem, ref secondaryItem);
					if (dropItem == 3)
					{
						dropItemStack = Main.rand.Next(1, 3);
					}
					break;
				case 588:
					SetGemTreeDrops(182, 4856, tileCache, ref dropItem, ref secondaryItem);
					if (dropItem == 3)
					{
						dropItemStack = Main.rand.Next(1, 3);
					}
					break;
			}
		}

	}
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 13)]
    public struct StructTile
    {
        public byte wall;

        public byte liquid;

        public byte bTileHeader;

        public byte bTileHeader2;

        public byte bTileHeader3;

        public ushort type;

        public short sTileHeader;

        public short frameX;

        public short frameY;
    }
    public class FakeTileProvider : ITileCollection, IDisposable
    {
        private StructTile[,] Data;

        public int Width
        {
            get;
        }

        public int Height
        {
            get;
        }

        public ITile this[int X, int Y]
        {
            get
            {
                return new TileReference(Data, X, Y);
            }
            set
            {
                new TileReference(Data, X, Y).CopyFrom(value);
            }
        }

        public FakeTileProvider(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
            Data = new StructTile[Width, Height];
        }

        public void Dispose()
        {
            if (Data == null)
            {
                return;
            }
            int length = Data.GetLength(0);
            int length2 = Data.GetLength(1);
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length2; j++)
                {
                    Data[i, j].bTileHeader = 0;
                    Data[i, j].bTileHeader2 = 0;
                    Data[i, j].bTileHeader3 = 0;
                    Data[i, j].frameX = 0;
                    Data[i, j].frameY = 0;
                    Data[i, j].liquid = 0;
                    Data[i, j].type = 0;
                    Data[i, j].wall = 0;
                }
            }
            Data = null;
        }
    }
    public sealed class TileReference : ITile
    {
        public const int Type_Solid = 0;

        public const int Type_Halfbrick = 1;

        public const int Type_SlopeDownRight = 2;

        public const int Type_SlopeDownLeft = 3;

        public const int Type_SlopeUpRight = 4;

        public const int Type_SlopeUpLeft = 5;

        public const int Liquid_Water = 0;

        public const int Liquid_Lava = 1;

        public const int Liquid_Honey = 2;

        internal readonly int X;

        internal readonly int Y;

        private StructTile[,] Data;

        private const double ActNum = 0.4;

        public ushort type
        {
            get
            {
                return Data[X, Y].type;
            }
            set
            {
                Data[X, Y].type = value;
            }
        }

        public byte wall
        {
            get
            {
                return Data[X, Y].wall;
            }
            set
            {
                Data[X, Y].wall = value;
            }
        }

        public byte liquid
        {
            get
            {
                return Data[X, Y].liquid;
            }
            set
            {
                Data[X, Y].liquid = value;
            }
        }

        public short frameX
        {
            get
            {
                return Data[X, Y].frameX;
            }
            set
            {
                Data[X, Y].frameX = value;
            }
        }

        public short frameY
        {
            get
            {
                return Data[X, Y].frameY;
            }
            set
            {
                Data[X, Y].frameY = value;
            }
        }

        public short sTileHeader
        {
            get
            {
                return Data[X, Y].sTileHeader;
            }
            set
            {
                Data[X, Y].sTileHeader = value;
            }
        }

        public byte bTileHeader
        {
            get
            {
                return Data[X, Y].bTileHeader;
            }
            set
            {
                Data[X, Y].bTileHeader = value;
            }
        }

        public byte bTileHeader2
        {
            get
            {
                return Data[X, Y].bTileHeader2;
            }
            set
            {
                Data[X, Y].bTileHeader2 = value;
            }
        }

        public byte bTileHeader3
        {
            get
            {
                return Data[X, Y].bTileHeader3;
            }
            set
            {
                Data[X, Y].bTileHeader3 = value;
            }
        }

        public int collisionType
        {
            get
            {
                if (!active())
                {
                    return 0;
                }
                if (halfBrick())
                {
                    return 2;
                }
                if (slope() > 0)
                {
                    return 2 + slope();
                }
                if (Main.tileSolid[type] && !Main.tileSolidTop[type])
                {
                    return 1;
                }
                return -1;
            }
        }

        ushort ITile.wall { get => wall; set => wall = (byte)value; }

        public TileReference(StructTile[,] Data, int X, int Y)
        {
            this.X = X;
            this.Y = Y;
            this.Data = Data;
        }

        public void Initialise()
        {
            type = 0;
            wall = 0;
            liquid = 0;
            sTileHeader = 0;
            bTileHeader = 0;
            bTileHeader2 = 0;
            bTileHeader3 = 0;
            frameX = 0;
            frameY = 0;
        }

        public void ClearEverything()
        {
            type = 0;
            wall = 0;
            ClearMetadata();
        }

        public void ClearTile()
        {
            slope(0);
            halfBrick(HalfBrick: false);
            active(Active: false);
        }

        public void ClearMetadata()
        {
            liquid = 0;
            sTileHeader = 0;
            bTileHeader = 0;
            bTileHeader2 = 0;
            bTileHeader3 = 0;
            frameX = 0;
            frameY = 0;
        }

        public void ResetToType(ushort Type)
        {
            liquid = 0;
            sTileHeader = 32;
            bTileHeader = 0;
            bTileHeader2 = 0;
            bTileHeader3 = 0;
            frameX = 0;
            frameY = 0;
            type = Type;
        }

        public void CopyFrom(ITile From)
        {
            type = From.type;
            wall = (byte)From.wall;
            liquid = From.liquid;
            sTileHeader = From.sTileHeader;
            bTileHeader = From.bTileHeader;
            bTileHeader2 = From.bTileHeader2;
            bTileHeader3 = From.bTileHeader3;
            frameX = From.frameX;
            frameY = From.frameY;
        }

        public bool isTheSameAs(ITile Tile)
        {
            if (Tile == null || sTileHeader != Tile.sTileHeader)
            {
                return false;
            }
            if (active())
            {
                if (type != Tile.type)
                {
                    return false;
                }
                if (Main.tileFrameImportant[type] && (frameX != Tile.frameX || frameY != Tile.frameY))
                {
                    return false;
                }
            }
            if (wall != Tile.wall || liquid != Tile.liquid)
            {
                return false;
            }
            if (Tile.liquid == 0)
            {
                if (wallColor() != Tile.wallColor())
                {
                    return false;
                }
                if (wire4() != Tile.wire4())
                {
                    return false;
                }
            }
            else if (bTileHeader != Tile.bTileHeader)
            {
                return false;
            }
            return true;
        }

        public Color actColor(Color oldColor)
        {
            if (!inActive())
            {
                return oldColor;
            }
            return new Color((byte)(0.4 * (double)(int)oldColor.R), (byte)(0.4 * (double)(int)oldColor.G), (byte)(0.4 * (double)(int)oldColor.B), oldColor.A);
        }

        public bool lava()
        {
            return (bTileHeader & 0x20) == 32;
        }

        public void lava(bool Lava)
        {
            if (Lava)
            {
                bTileHeader = (byte)((bTileHeader & 0x9Fu) | 0x20u);
            }
            else
            {
                bTileHeader &= 223;
            }
        }

        public bool honey()
        {
            return (bTileHeader & 0x40) == 64;
        }

        public void honey(bool Honey)
        {
            if (Honey)
            {
                bTileHeader = (byte)((bTileHeader & 0x9Fu) | 0x40u);
            }
            else
            {
                bTileHeader &= 191;
            }
        }

        public byte liquidType()
        {
            return (byte)((bTileHeader & 0x60) >> 5);
        }

        public void liquidType(int LiquidType)
        {
            switch (LiquidType)
            {
                case 0:
                    bTileHeader &= 159;
                    break;
                case 1:
                    lava(Lava: true);
                    break;
                case 2:
                    honey(Honey: true);
                    break;
            }
        }

        public bool checkingLiquid()
        {
            return (bTileHeader3 & 8) == 8;
        }

        public void checkingLiquid(bool CheckingLiquid)
        {
            if (CheckingLiquid)
            {
                bTileHeader3 |= 8;
            }
            else
            {
                bTileHeader3 &= 247;
            }
        }

        public bool skipLiquid()
        {
            return (bTileHeader3 & 0x10) == 16;
        }

        public void skipLiquid(bool SkipLiquid)
        {
            if (SkipLiquid)
            {
                bTileHeader3 |= 16;
            }
            else
            {
                bTileHeader3 &= 239;
            }
        }

        public byte frameNumber()
        {
            return (byte)((bTileHeader2 & 0x30) >> 4);
        }

        public void frameNumber(byte FrameNumber)
        {
            bTileHeader2 = (byte)((bTileHeader2 & 0xCFu) | (uint)((FrameNumber & 3) << 4));
        }

        public byte wallFrameNumber()
        {
            return (byte)((bTileHeader2 & 0xC0) >> 6);
        }

        public void wallFrameNumber(byte WallFrameNumber)
        {
            bTileHeader2 = (byte)((bTileHeader2 & 0x3Fu) | (uint)((WallFrameNumber & 3) << 6));
        }

        public int wallFrameX()
        {
            return (bTileHeader2 & 0xF) * 36;
        }

        public void wallFrameX(int WallFrameX)
        {
            bTileHeader2 = (byte)((bTileHeader2 & 0xF0u) | ((uint)(WallFrameX / 36) & 0xFu));
        }

        public int wallFrameY()
        {
            return (bTileHeader3 & 7) * 36;
        }

        public void wallFrameY(int WallFrameY)
        {
            bTileHeader3 = (byte)((bTileHeader3 & 0xF8u) | ((uint)(WallFrameY / 36) & 7u));
        }

        public byte color()
        {
            return (byte)((uint)sTileHeader & 0x1Fu);
        }

        public void color(byte Color)
        {
            if (Color > 30)
            {
                Color = 30;
            }
            sTileHeader = (short)((sTileHeader & 0xFFE0) | Color);
        }

        public byte wallColor()
        {
            return (byte)(bTileHeader & 0x1Fu);
        }

        public void wallColor(byte WallColor)
        {
            if (WallColor > 30)
            {
                WallColor = 30;
            }
            bTileHeader = (byte)((bTileHeader & 0xE0u) | WallColor);
        }

        public bool active()
        {
            return (sTileHeader & 0x20) == 32;
        }

        public void active(bool Active)
        {
            if (Active)
            {
                sTileHeader |= 32;
            }
            else
            {
                sTileHeader = (short)(sTileHeader & 0xFFDF);
            }
        }

        public bool inActive()
        {
            return (sTileHeader & 0x40) == 64;
        }

        public void inActive(bool InActive)
        {
            if (InActive)
            {
                sTileHeader |= 64;
            }
            else
            {
                sTileHeader = (short)(sTileHeader & 0xFFBF);
            }
        }

        public bool nactive()
        {
            return (sTileHeader & 0x60) == 32;
        }

        public bool wire()
        {
            return (sTileHeader & 0x80) == 128;
        }

        public void wire(bool Wire)
        {
            if (Wire)
            {
                sTileHeader |= 128;
            }
            else
            {
                sTileHeader = (short)(sTileHeader & 0xFF7F);
            }
        }

        public bool wire2()
        {
            return (sTileHeader & 0x100) == 256;
        }

        public void wire2(bool Wire2)
        {
            if (Wire2)
            {
                sTileHeader |= 256;
            }
            else
            {
                sTileHeader = (short)(sTileHeader & 0xFEFF);
            }
        }

        public bool wire3()
        {
            return (sTileHeader & 0x200) == 512;
        }

        public void wire3(bool Wire3)
        {
            if (Wire3)
            {
                sTileHeader |= 512;
            }
            else
            {
                sTileHeader = (short)(sTileHeader & 0xFDFF);
            }
        }

        public bool wire4()
        {
            return (bTileHeader & 0x80) == 128;
        }

        public void wire4(bool Wire4)
        {
            if (Wire4)
            {
                bTileHeader |= 128;
            }
            else
            {
                bTileHeader &= 127;
            }
        }

        public bool actuator()
        {
            return (sTileHeader & 0x800) == 2048;
        }

        public void actuator(bool Actuator)
        {
            if (Actuator)
            {
                sTileHeader |= 2048;
            }
            else
            {
                sTileHeader = (short)(sTileHeader & 0xF7FF);
            }
        }

        public bool halfBrick()
        {
            return (sTileHeader & 0x400) == 1024;
        }

        public void halfBrick(bool HalfBrick)
        {
            if (HalfBrick)
            {
                sTileHeader |= 1024;
            }
            else
            {
                sTileHeader = (short)(sTileHeader & 0xFBFF);
            }
        }

        public byte slope()
        {
            return (byte)((sTileHeader & 0x7000) >> 12);
        }

        public void slope(byte Slope)
        {
            sTileHeader = (short)((sTileHeader & 0x8FFF) | ((Slope & 7) << 12));
        }

        public bool topSlope()
        {
            byte b = slope();
            if (b != 1)
            {
                return b == 2;
            }
            return true;
        }

        public bool bottomSlope()
        {
            byte b = slope();
            if (b != 3)
            {
                return b == 4;
            }
            return true;
        }

        public bool leftSlope()
        {
            byte b = slope();
            if (b != 2)
            {
                return b == 4;
            }
            return true;
        }

        public bool rightSlope()
        {
            byte b = slope();
            if (b != 1)
            {
                return b == 3;
            }
            return true;
        }

        public bool HasSameSlope(ITile Tile)
        {
            return (sTileHeader & 0x7400) == (Tile.sTileHeader & 0x7400);
        }

        public int blockType()
        {
            if (halfBrick())
            {
                return 1;
            }
            int num = slope();
            if (num > 0)
            {
                num++;
            }
            return num;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public new string ToString()
        {
            return $"Tile Type:{type} Active:{active()} " + $"Wall:{wall} Slope:{slope()} fX:{frameX} fY:{frameY}";
        }

        public void actColor(ref Vector3 oldColor)
        {

        }

        public void Clear(TileDataType types)
        {
        }
    }
}
