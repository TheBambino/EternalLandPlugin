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

namespace EternalLandPlugin.Game
{
    public class MapManager
    {
        public class MapData
        {
            public MapData(Point16 topleft, Point16 bottomright,string name = "UnKnown", bool keepalive = false)
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
                await Task.Run(() => {
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
                 return await Task.Run(() => {
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
            public bool GetRelative(int x, int y, out int RX,out int RY)
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
                eplr.SendMap(uuid);
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
                    if (!map.Value.Player.Any()) check[map.Key]++;
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
