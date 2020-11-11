namespace EternalLandPlugin.MultiWorld
{
    /* class LoadWorld
     {
         //public static Main FakeMain;
         public static NPC FakeNPC = new NPC();
         public static TEntity FakeEntity = new TEntity();
         public static Netplay FakeNetPlayer = new Netplay();
         public static AppDomain donain;
         public static Stopwatch saveTime = new Stopwatch();

         public static int TilesX1 = 0;

         public static int TilesX2 = FakeMain.maxTilesX;

         public static int TilesX3 = FakeMain.maxTilesX;

         public static int TilesY1 = 0;

         public static int TilesY2 = FakeMain.maxTilesY;

         public class TEntity : TileEntity
         {

         }

         public class FakeMain : Main
         {

         }

         private void MakeFakeWorld()
         {
             new Thread(new ThreadStart(() =>
             {
                 typeof(ServerApi).Assembly.EntryPoint.Invoke(null, new object[] { "-world", "" });
             }));
             Stopwatch stopwatch = new Stopwatch();
             stopwatch.Start();
             double num6 = 16.666666666666668;
             double num7 = 0.0;
             int num8 = 0;
             new Stopwatch().Start();
             Netplay.StartServer();
             FakeMain.gameMenu = false;
             FakeMain m = new FakeMain();
             m.Run();
             while (!Netplay.Disconnect)
             {
                 double totalMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                 if (totalMilliseconds + num7 >= num6)
                 {
                     num8++;
                     num7 += totalMilliseconds - num6;
                     stopwatch.Reset();
                     stopwatch.Start();
                     if (Netplay.HasClients)
                     {

                         m.Update(new GameTime());
                     }
                     else if (saveTime.IsRunning)
                     {
                         saveTime.Stop();
                     }
                     //FakeMain.OnTickForThirdPartySoftwareOnly?.Invoke();
                     double num9 = stopwatch.Elapsed.TotalMilliseconds + num7;
                     if (num9 < num6)
                     {
                         int num10 = (int)(num6 - num9) - 1;
                         if (num10 > 1)
                         {
                             Thread.Sleep(num10 - 1);
                             if (!Netplay.HasClients)
                             {
                                 num7 = 0.0;
                                 Thread.Sleep(10);
                             }
                         }
                     }
                 }
                 Thread.Sleep(0);
             }
         }

         private void LoadWorldargs(CommandArgs args)
         {

             TilesX1 = 0;
             TilesX2 = FakeMain.maxTilesX;
             //TilesX3 = FakeMain.maxTilesX / 2;
             TilesX3 = FakeMain.maxTilesX;
             TilesY1 = 0;
             TilesY2 = FakeMain.maxTilesY;
             TShock.Utils.Broadcast("左半图以及Boss攻略进度将重置至初始状态，可能会出现短暂卡顿！", 123, 104, 238);
             for (int i = 0; i < FakeMain.npc.Length; i++)
             {
                 if (FakeMain.npc[i].active && !FakeMain.npc[i].townNPC && FakeMain.npc[i].netID != 488)
                 {
                     TSPlayer.Server.StrikeNPC(i, (int)((double)FakeMain.npc[i].life + (double)FakeMain.npc[i].defense * 0.6), 0f, 0);
                 }
             }
             loadWorld();
             TSPlayer.All.SendData(PacketTypes.WorldInfo);
             ResetSection();
             args.Player.Spawn(PlayerSpawnContext.RecallFromItem);
             TShock.Utils.Broadcast("左半图已重置完成！", 123, 104, 238);
         }

         public void ResetSection()
         {
             int sectionX = Netplay.GetSectionX(TilesX1);
             int sectionX2 = Netplay.GetSectionX(TilesX3);
             int sectionY = Netplay.GetSectionY(TilesY1);
             int sectionY2 = Netplay.GetSectionY(TilesY2);
             foreach (RemoteClient item in Netplay.Clients.Where((RemoteClient s) => s.IsActive))
             {
                 for (int i = sectionX; i <= sectionX2; i++)
                 {
                     for (int j = sectionY; j <= sectionY2; j++)
                     {
                         item.TileSections[i, j] = false;
                     }
                 }
             }
         }

         public static void loadWorld()
         {
             FakeMain.checkXMas();
             FakeMain.checkHalloween();
             using MemoryStream memoryStream = new MemoryStream(FileUtilities.ReadAllBytes(".\\LWorlds\\" + FakeMain.ActiveWorldFileData.Name + ".wld", cloud: false));
             using BinaryReader binaryReader = new BinaryReader(memoryStream);
             try
             {
                 WorldGen.loadFailed = false;
                 WorldGen.loadSuccess = false;
                 int num2 = WorldFile._versionNumber = binaryReader.ReadInt32();
                 if (num2 <= 0 || num2 > 230)
                 {
                     WorldGen.loadFailed = true;
                     return;
                 }
                 int num3 = LoadWorld_Version2(binaryReader);
                 if (num2 < 141)
                 {
                     FakeMain.ActiveWorldFileData.CreationTime = File.GetCreationTime(FakeMain.worldPathName);
                 }
                 WorldFile.CheckSavedOreTiers();
                 binaryReader.Close();
                 memoryStream.Close();
                 if (num3 != 0)
                 {
                     WorldGen.loadFailed = true;
                 }
                 else
                 {
                     WorldGen.loadSuccess = true;
                 }
                 if (WorldGen.loadFailed || !WorldGen.loadSuccess)
                 {
                     return;
                 }
                 WorldFile.ConvertOldTileEntities();
                 WorldFile.ClearTempTiles();
                 WorldGen.gen = true;
                 WorldGen.waterLine = FakeMain.maxTilesY;
                 Liquid.QuickWater(2);
                 WorldGen.WaterCheck();
                 int num4 = 0;
                 Liquid.quickSettle = true;
                 int num5 = Liquid.numLiquid + LiquidBuffer.numLiquidBuffer;
                 float num6 = 0f;
                 while (Liquid.numLiquid > 0 && num4 < 100000)
                 {
                     num4++;
                     float num7 = (float)(num5 - (Liquid.numLiquid + LiquidBuffer.numLiquidBuffer)) / (float)num5;
                     if (Liquid.numLiquid + LiquidBuffer.numLiquidBuffer > num5)
                     {
                         num5 = Liquid.numLiquid + LiquidBuffer.numLiquidBuffer;
                     }
                     if (num7 > num6)
                     {
                         num6 = num7;
                     }
                     else
                     {
                         num7 = num6;
                     }
                     Liquid.UpdateLiquid();
                 }
                 Liquid.quickSettle = false;
                 FakeMain.weatherCounter = WorldGen.genRand.Next(3600, 18000);
                 Cloud.resetClouds();
                 WorldGen.WaterCheck();
                 WorldGen.gen = false;
                 NPC.setFireFlyChance();
                 if (FakeMain.slimeRainTime > 0.0)
                 {
                     FakeMain.StartSlimeRain(announce: false);
                 }
                 NPC.SetWorldSpecificMonstersByWorldID();
             }
             catch
             {
                 WorldGen.loadFailed = true;
                 WorldGen.loadSuccess = false;
                 try
                 {
                     binaryReader.Close();
                     memoryStream.Close();
                 }
                 catch
                 {
                 }
             }
         }

         public static int LoadWorld_Version2(BinaryReader reader)
         {
             int _versionNumber = WorldFile._versionNumber;
             reader.BaseStream.Position = 0L;
             if (!LoadFileFormatHeader(reader, out bool[] importance, out int[] positions))
             {
                 return 5;
             }
             if (reader.BaseStream.Position != positions[0])
             {
                 return 5;
             }
             LoadHeader(reader);
             if (reader.BaseStream.Position != positions[1])
             {
                 return 5;
             }
             LoadWorldTiles(reader, importance);
             if (reader.BaseStream.Position != positions[2])
             {
                 return 5;
             }
             LoadChests(reader);
             if (reader.BaseStream.Position != positions[3])
             {
                 return 5;
             }
             LoadSigns(reader);
             if (reader.BaseStream.Position != positions[4])
             {
                 return 5;
             }
             LoadNPCs(reader);
             if (reader.BaseStream.Position != positions[5])
             {
                 return 5;
             }
             if (_versionNumber >= 116)
             {
                 if (_versionNumber < 122)
                 {
                     LoadDummies(reader);
                     if (reader.BaseStream.Position != positions[6])
                     {
                         return 5;
                     }
                 }
                 else
                 {
                     LoadTileEntities(reader);
                     if (reader.BaseStream.Position != positions[6])
                     {
                         return 5;
                     }
                 }
             }
             if (_versionNumber >= 170)
             {
                 LoadWeightedPressurePlates(reader);
                 if (reader.BaseStream.Position != positions[7])
                 {
                     return 5;
                 }
             }
             if (_versionNumber >= 189)
             {
                 LoadTownManager(reader);
                 if (reader.BaseStream.Position != positions[8])
                 {
                     return 5;
                 }
             }
             if (_versionNumber >= 210)
             {
                 WorldFile.LoadBestiary(reader, _versionNumber);
                 if (reader.BaseStream.Position != positions[9])
                 {
                     return 5;
                 }
             }
             else
             {
                 WorldFile.LoadBestiaryForVersionsBefore210();
             }
             if (_versionNumber >= 220)
             {
                 WorldFile.LoadCreativePowers(reader, _versionNumber);
                 if (reader.BaseStream.Position != positions[10])
                 {
                     return 5;
                 }
             }
             return LoadFooter(reader);
         }

         public static void LoadChests(BinaryReader reader)
         {
             int num = reader.ReadInt16();
             int num2 = reader.ReadInt16();
             int num3;
             int num4;
             if (num2 < 40)
             {
                 num3 = num2;
                 num4 = 0;
             }
             else
             {
                 num3 = 40;
                 num4 = num2 - 40;
             }
             int i;
             for (i = 0; i < num; i++)
             {
                 Chest chest = new Chest
                 {
                     x = reader.ReadInt32(),
                     y = reader.ReadInt32(),
                     name = reader.ReadString()
                 };
                 for (int j = 0; j < num3; j++)
                 {
                     short num5 = reader.ReadInt16();
                     Item item = new Item();
                     if (num5 > 0)
                     {
                         item.netDefaults(reader.ReadInt32());
                         item.stack = num5;
                         item.Prefix(reader.ReadByte());
                     }
                     else if (num5 < 0)
                     {
                         item.netDefaults(reader.ReadInt32());
                         item.Prefix(reader.ReadByte());
                         item.stack = 1;
                     }
                     chest.item[j] = item;
                 }
                 for (int k = 0; k < num4; k++)
                 {
                     short num5 = reader.ReadInt16();
                     if (num5 > 0)
                     {
                         reader.ReadInt32();
                         reader.ReadByte();
                     }
                 }
                 FakeMain.chest[i] = chest;
             }
             List<Point16> list = new List<Point16>();
             for (int l = 0; l < i; l++)
             {
                 if (FakeMain.chest[l] != null)
                 {
                     Point16 item2 = new Point16(FakeMain.chest[l].x, FakeMain.chest[l].y);
                     if (list.Contains(item2))
                     {
                         FakeMain.chest[l] = null;
                     }
                     else
                     {
                         list.Add(item2);
                     }
                 }
             }
             for (; i < 8000; i++)
             {
                 FakeMain.chest[i] = null;
             }
             if (WorldFile._versionNumber < 115)
             {
                 WorldFile.FixDresserChests();
             }
         }


         public static void LoadDummies(BinaryReader reader)
         {
             int num = reader.ReadInt32();
             for (int i = 0; i < num; i++)
             {
                 DeprecatedClassLeftInForLoading.dummies[i] = new DeprecatedClassLeftInForLoading(reader.ReadInt16(), reader.ReadInt16());
             }
             for (int j = num; j < 1000; j++)
             {
                 DeprecatedClassLeftInForLoading.dummies[j] = null;
             }
         }

         private static bool LoadFileFormatHeader(BinaryReader reader, out bool[] importance, out int[] positions)
         {
             importance = null;
             positions = null;
             if ((WorldFile._versionNumber = reader.ReadInt32()) >= 135)
             {
                 try
                 {
                     FakeMain.WorldFileMetadata = FileMetadata.Read(reader, FileType.World);
                 }
                 catch
                 {
                     Console.WriteLine(Language.GetTextValue("Error.UnableToLoadWorld"));
                     return false;
                 }
             }
             else
             {
                 FakeMain.WorldFileMetadata = FileMetadata.FromCurrentSettings(FileType.World);
             }
             short num = reader.ReadInt16();
             positions = new int[num];
             for (int i = 0; i < num; i++)
             {
                 positions[i] = reader.ReadInt32();
             }
             short num2 = reader.ReadInt16();
             importance = new bool[num2];
             byte b = 0;
             byte b2 = 128;
             for (int j = 0; j < num2; j++)
             {
                 if (b2 == 128)
                 {
                     b = reader.ReadByte();
                     b2 = 1;
                 }
                 else
                 {
                     b2 = (byte)(b2 << 1);
                 }
                 if ((b & b2) == b2)
                 {
                     importance[j] = true;
                 }
             }
             return true;
         }

         private static int LoadFooter(BinaryReader reader)
         {
             if (!reader.ReadBoolean())
             {
                 return 6;
             }
             if (reader.ReadString() != FakeMain.worldName)
             {
                 return 6;
             }
             if (reader.ReadInt32() != FakeMain.worldID)
             {
                 return 6;
             }
             return 0;
         }

         public static void LoadHeader(BinaryReader reader)
         {
             int versionNumber = WorldFile._versionNumber;
             FakeMain.worldName = reader.ReadString();
             if (versionNumber >= 179)
             {
                 string seed = (versionNumber != 179) ? reader.ReadString() : reader.ReadInt32().ToString();
                 FakeMain.ActiveWorldFileData.SetSeed(seed);
                 FakeMain.ActiveWorldFileData.WorldGeneratorVersion = reader.ReadUInt64();
             }
             if (versionNumber >= 181)
             {
                 FakeMain.ActiveWorldFileData.UniqueId = new Guid(reader.ReadBytes(16));
             }
             else
             {
                 FakeMain.ActiveWorldFileData.UniqueId = Guid.NewGuid();
             }
             FakeMain.worldID = reader.ReadInt32();
             FakeMain.leftWorld = reader.ReadInt32();
             FakeMain.rightWorld = reader.ReadInt32();
             FakeMain.topWorld = reader.ReadInt32();
             FakeMain.bottomWorld = reader.ReadInt32();
             FakeMain.maxTilesY = reader.ReadInt32();
             FakeMain.maxTilesX = reader.ReadInt32();
             WorldGen.clearWorld();
             if (versionNumber >= 209)
             {
                 FakeMain.GameMode = reader.ReadInt32();
                 if (versionNumber >= 222)
                 {
                     FakeMain.drunkWorld = reader.ReadBoolean();
                 }
                 if (versionNumber >= 227)
                 {
                     FakeMain.getGoodWorld = reader.ReadBoolean();
                 }
             }
             else
             {
                 if (versionNumber >= 112)
                 {
                     FakeMain.GameMode = (reader.ReadBoolean() ? 1 : 0);
                 }
                 else
                 {
                     FakeMain.GameMode = 0;
                 }
                 if (versionNumber == 208 && reader.ReadBoolean())
                 {
                     FakeMain.GameMode = 2;
                 }
             }
             if (versionNumber >= 141)
             {
                 FakeMain.ActiveWorldFileData.CreationTime = DateTime.FromBinary(reader.ReadInt64());
             }
             FakeMain.moonType = reader.ReadByte();
             FakeMain.treeX[0] = reader.ReadInt32();
             FakeMain.treeX[1] = reader.ReadInt32();
             FakeMain.treeX[2] = reader.ReadInt32();
             FakeMain.treeStyle[0] = reader.ReadInt32();
             FakeMain.treeStyle[1] = reader.ReadInt32();
             FakeMain.treeStyle[2] = reader.ReadInt32();
             FakeMain.treeStyle[3] = reader.ReadInt32();
             FakeMain.caveBackX[0] = reader.ReadInt32();
             FakeMain.caveBackX[1] = reader.ReadInt32();
             FakeMain.caveBackX[2] = reader.ReadInt32();
             FakeMain.caveBackStyle[0] = reader.ReadInt32();
             FakeMain.caveBackStyle[1] = reader.ReadInt32();
             FakeMain.caveBackStyle[2] = reader.ReadInt32();
             FakeMain.caveBackStyle[3] = reader.ReadInt32();
             FakeMain.iceBackStyle = reader.ReadInt32();
             FakeMain.jungleBackStyle = reader.ReadInt32();
             FakeMain.hellBackStyle = reader.ReadInt32();
             FakeMain.spawnTileX = reader.ReadInt32();
             FakeMain.spawnTileY = reader.ReadInt32();
             FakeMain.worldSurface = reader.ReadDouble();
             FakeMain.rockLayer = reader.ReadDouble();
             WorldFile._tempTime = reader.ReadDouble();
             WorldFile._tempDayTime = reader.ReadBoolean();
             WorldFile._tempMoonPhase = reader.ReadInt32();
             WorldFile._tempBloodMoon = reader.ReadBoolean();
             WorldFile._tempEclipse = reader.ReadBoolean();
             FakeMain.eclipse = WorldFile._tempEclipse;
             FakeMain.dungeonX = reader.ReadInt32();
             FakeMain.dungeonY = reader.ReadInt32();
             WorldGen.crimson = reader.ReadBoolean();
             NPC.downedBoss1 = reader.ReadBoolean();
             NPC.downedBoss2 = reader.ReadBoolean();
             NPC.downedBoss3 = reader.ReadBoolean();
             NPC.downedQueenBee = reader.ReadBoolean();
             NPC.downedMechBoss1 = reader.ReadBoolean();
             NPC.downedMechBoss2 = reader.ReadBoolean();
             NPC.downedMechBoss3 = reader.ReadBoolean();
             NPC.downedMechBossAny = reader.ReadBoolean();
             NPC.downedPlantBoss = reader.ReadBoolean();
             NPC.downedGolemBoss = reader.ReadBoolean();
             if (versionNumber >= 118)
             {
                 NPC.downedSlimeKing = reader.ReadBoolean();
             }
             NPC.savedGoblin = reader.ReadBoolean();
             NPC.savedWizard = reader.ReadBoolean();
             NPC.savedMech = reader.ReadBoolean();
             NPC.downedGoblins = reader.ReadBoolean();
             NPC.downedClown = reader.ReadBoolean();
             NPC.downedFrost = reader.ReadBoolean();
             NPC.downedPirates = reader.ReadBoolean();
             WorldGen.shadowOrbSmashed = reader.ReadBoolean();
             WorldGen.spawnMeteor = reader.ReadBoolean();
             WorldGen.shadowOrbCount = reader.ReadByte();
             WorldGen.altarCount = reader.ReadInt32();
             FakeMain.hardMode = reader.ReadBoolean();
             FakeMain.invasionDelay = reader.ReadInt32();
             FakeMain.invasionSize = reader.ReadInt32();
             FakeMain.invasionType = reader.ReadInt32();
             FakeMain.invasionX = reader.ReadDouble();
             if (versionNumber >= 118)
             {
                 FakeMain.slimeRainTime = reader.ReadDouble();
             }
             if (versionNumber >= 113)
             {
                 FakeMain.sundialCooldown = reader.ReadByte();
             }
             WorldFile._tempRaining = reader.ReadBoolean();
             WorldFile._tempRainTime = reader.ReadInt32();
             WorldFile._tempMaxRain = reader.ReadSingle();
             WorldGen.SavedOreTiers.Cobalt = reader.ReadInt32();
             WorldGen.SavedOreTiers.Mythril = reader.ReadInt32();
             WorldGen.SavedOreTiers.Adamantite = reader.ReadInt32();
             WorldGen.setBG(0, reader.ReadByte());
             WorldGen.setBG(1, reader.ReadByte());
             WorldGen.setBG(2, reader.ReadByte());
             WorldGen.setBG(3, reader.ReadByte());
             WorldGen.setBG(4, reader.ReadByte());
             WorldGen.setBG(5, reader.ReadByte());
             WorldGen.setBG(6, reader.ReadByte());
             WorldGen.setBG(7, reader.ReadByte());
             FakeMain.cloudBGActive = reader.ReadInt32();
             FakeMain.cloudBGAlpha = (((double)FakeMain.cloudBGActive < 1.0) ? 0f : 1f);
             FakeMain.cloudBGActive = -WorldGen.genRand.Next(8640, 86400);
             FakeMain.numClouds = reader.ReadInt16();
             FakeMain.windSpeedTarget = reader.ReadSingle();
             FakeMain.windSpeedCurrent = FakeMain.windSpeedTarget;
             if (versionNumber < 95)
             {
                 return;
             }
             FakeMain.anglerWhoFinishedToday.Clear();
             for (int num = reader.ReadInt32(); num > 0; num--)
             {
                 FakeMain.anglerWhoFinishedToday.Add(reader.ReadString());
             }
             if (versionNumber < 99)
             {
                 return;
             }
             NPC.savedAngler = reader.ReadBoolean();
             if (versionNumber < 101)
             {
                 return;
             }
             FakeMain.anglerQuest = reader.ReadInt32();
             if (versionNumber < 104)
             {
                 return;
             }
             NPC.savedStylist = reader.ReadBoolean();
             if (versionNumber >= 129)
             {
                 NPC.savedTaxCollector = reader.ReadBoolean();
             }
             if (versionNumber >= 201)
             {
                 NPC.savedGolfer = reader.ReadBoolean();
             }
             if (versionNumber < 107)
             {
                 if (FakeMain.invasionType > 0 && FakeMain.invasionSize > 0)
                 {
                     FakeMain.FakeLoadInvasionStart();
                 }
             }
             else
             {
                 FakeMain.invasionSizeStart = reader.ReadInt32();
             }
             if (versionNumber < 108)
             {
                 WorldFile._tempCultistDelay = 86400;
             }
             else
             {
                 WorldFile._tempCultistDelay = reader.ReadInt32();
             }
             if (versionNumber < 109)
             {
                 return;
             }
             int num2 = reader.ReadInt16();
             for (int i = 0; i < num2; i++)
             {
                 if (i < 663)
                 {
                     NPC.killCount[i] = reader.ReadInt32();
                 }
                 else
                 {
                     reader.ReadInt32();
                 }
             }
             if (versionNumber < 128)
             {
                 return;
             }
             FakeMain.fastForwardTime = reader.ReadBoolean();
             FakeMain.UpdateTimeRate();
             if (versionNumber < 131)
             {
                 return;
             }
             NPC.downedFishron = reader.ReadBoolean();
             NPC.downedMartians = reader.ReadBoolean();
             NPC.downedAncientCultist = reader.ReadBoolean();
             NPC.downedMoonlord = reader.ReadBoolean();
             NPC.downedHalloweenKing = reader.ReadBoolean();
             NPC.downedHalloweenTree = reader.ReadBoolean();
             NPC.downedChristmasIceQueen = reader.ReadBoolean();
             NPC.downedChristmasSantank = reader.ReadBoolean();
             NPC.downedChristmasTree = reader.ReadBoolean();
             if (versionNumber < 140)
             {
                 return;
             }
             NPC.downedTowerSolar = reader.ReadBoolean();
             NPC.downedTowerVortex = reader.ReadBoolean();
             NPC.downedTowerNebula = reader.ReadBoolean();
             NPC.downedTowerStardust = reader.ReadBoolean();
             NPC.TowerActiveSolar = reader.ReadBoolean();
             NPC.TowerActiveVortex = reader.ReadBoolean();
             NPC.TowerActiveNebula = reader.ReadBoolean();
             NPC.TowerActiveStardust = reader.ReadBoolean();
             NPC.LunarApocalypseIsUp = reader.ReadBoolean();
             if (NPC.TowerActiveSolar)
             {
                 NPC.ShieldStrengthTowerSolar = NPC.ShieldStrengthTowerMax;
             }
             if (NPC.TowerActiveVortex)
             {
                 NPC.ShieldStrengthTowerVortex = NPC.ShieldStrengthTowerMax;
             }
             if (NPC.TowerActiveNebula)
             {
                 NPC.ShieldStrengthTowerNebula = NPC.ShieldStrengthTowerMax;
             }
             if (NPC.TowerActiveStardust)
             {
                 NPC.ShieldStrengthTowerStardust = NPC.ShieldStrengthTowerMax;
             }
             if (versionNumber < 170)
             {
                 WorldFile._tempPartyManual = false;
                 WorldFile._tempPartyGenuine = false;
                 WorldFile._tempPartyCooldown = 0;
                 WorldFile.TempPartyCelebratingNPCs.Clear();
             }
             else
             {
                 WorldFile._tempPartyManual = reader.ReadBoolean();
                 WorldFile._tempPartyGenuine = reader.ReadBoolean();
                 WorldFile._tempPartyCooldown = reader.ReadInt32();
                 int num3 = reader.ReadInt32();
                 WorldFile.TempPartyCelebratingNPCs.Clear();
                 for (int j = 0; j < num3; j++)
                 {
                     WorldFile.TempPartyCelebratingNPCs.Add(reader.ReadInt32());
                 }
             }
             if (versionNumber < 174)
             {
                 WorldFile._tempSandstormHappening = false;
                 WorldFile._tempSandstormTimeLeft = 0;
                 WorldFile._tempSandstormSeverity = 0f;
                 WorldFile._tempSandstormIntendedSeverity = 0f;
             }
             else
             {
                 WorldFile._tempSandstormHappening = reader.ReadBoolean();
                 WorldFile._tempSandstormTimeLeft = reader.ReadInt32();
                 WorldFile._tempSandstormSeverity = reader.ReadSingle();
                 WorldFile._tempSandstormIntendedSeverity = reader.ReadSingle();
             }
             DD2Event.Load(reader, versionNumber);
             if (versionNumber > 194)
             {
                 WorldGen.setBG(8, reader.ReadByte());
             }
             else
             {
                 WorldGen.setBG(8, 0);
             }
             if (versionNumber >= 215)
             {
                 WorldGen.setBG(9, reader.ReadByte());
             }
             else
             {
                 WorldGen.setBG(9, 0);
             }
             if (versionNumber > 195)
             {
                 WorldGen.setBG(10, reader.ReadByte());
                 WorldGen.setBG(11, reader.ReadByte());
                 WorldGen.setBG(12, reader.ReadByte());
             }
             else
             {
                 WorldGen.setBG(10, WorldGen.treeBG1);
                 WorldGen.setBG(11, WorldGen.treeBG1);
                 WorldGen.setBG(12, WorldGen.treeBG1);
             }
             if (versionNumber >= 204)
             {
                 NPC.combatBookWasUsed = reader.ReadBoolean();
             }
             if (versionNumber < 207)
             {
                 WorldFile._tempLanternNightCooldown = 0;
                 WorldFile._tempLanternNightGenuine = false;
                 WorldFile._tempLanternNightManual = false;
                 WorldFile._tempLanternNightNextNightIsGenuine = false;
             }
             else
             {
                 WorldFile._tempLanternNightCooldown = reader.ReadInt32();
                 WorldFile._tempLanternNightGenuine = reader.ReadBoolean();
                 WorldFile._tempLanternNightManual = reader.ReadBoolean();
                 WorldFile._tempLanternNightNextNightIsGenuine = reader.ReadBoolean();
             }
             WorldGen.TreeTops.Load(reader, versionNumber);
             if (versionNumber >= 212)
             {
                 FakeMain.forceHalloweenForToday = reader.ReadBoolean();
                 FakeMain.forceXMasForToday = reader.ReadBoolean();
             }
             else
             {
                 FakeMain.forceHalloweenForToday = false;
                 FakeMain.forceXMasForToday = false;
             }
             if (versionNumber >= 216)
             {
                 WorldGen.SavedOreTiers.Copper = reader.ReadInt32();
                 WorldGen.SavedOreTiers.Iron = reader.ReadInt32();
                 WorldGen.SavedOreTiers.Silver = reader.ReadInt32();
                 WorldGen.SavedOreTiers.Gold = reader.ReadInt32();
             }
             else
             {
                 WorldGen.SavedOreTiers.Copper = -1;
                 WorldGen.SavedOreTiers.Iron = -1;
                 WorldGen.SavedOreTiers.Silver = -1;
                 WorldGen.SavedOreTiers.Gold = -1;
             }
             if (versionNumber >= 217)
             {
                 NPC.boughtCat = reader.ReadBoolean();
                 NPC.boughtDog = reader.ReadBoolean();
                 NPC.boughtBunny = reader.ReadBoolean();
             }
             else
             {
                 NPC.boughtCat = false;
                 NPC.boughtDog = false;
                 NPC.boughtBunny = false;
             }
             if (versionNumber >= 223)
             {
                 NPC.downedEmpressOfLight = reader.ReadBoolean();
                 NPC.downedQueenSlime = reader.ReadBoolean();
             }
             else
             {
                 NPC.downedEmpressOfLight = false;
                 NPC.downedQueenSlime = false;
             }
         }

         public static void LoadNPCs(BinaryReader reader)
         {
             int num = 0;
             bool flag = reader.ReadBoolean();
             while (flag)
             {
                 NPC nPC = FakeMain.npc[num];
                 if (WorldFile._versionNumber >= 190)
                 {
                     nPC.SetDefaults(reader.ReadInt32());
                 }
                 else
                 {
                     nPC.SetDefaults(NPCID.FromLegacyName(reader.ReadString()));
                 }
                 nPC.GivenName = reader.ReadString();
                 nPC.position.X = reader.ReadSingle();
                 nPC.position.Y = reader.ReadSingle();
                 nPC.homeless = reader.ReadBoolean();
                 nPC.homeTileX = reader.ReadInt32();
                 nPC.homeTileY = reader.ReadInt32();
                 if (WorldFile._versionNumber >= 213 && ((BitsByte)reader.ReadByte())[0])
                 {
                     nPC.townNpcVariationIndex = reader.ReadInt32();
                 }
                 num++;
                 flag = reader.ReadBoolean();
             }
             if (WorldFile._versionNumber < 140)
             {
                 return;
             }
             flag = reader.ReadBoolean();
             while (flag)
             {
                 NPC nPC = FakeMain.npc[num];
                 if (WorldFile._versionNumber >= 190)
                 {
                     nPC.SetDefaults(reader.ReadInt32());
                 }
                 else
                 {
                     nPC.SetDefaults(NPCID.FromLegacyName(reader.ReadString()));
                 }
                 nPC.position = reader.ReadVector2();
                 num++;
                 flag = reader.ReadBoolean();
             }
         }


         public static void LoadSigns(BinaryReader reader)
         {
             short num = reader.ReadInt16();
             int i;
             for (i = 0; i < num; i++)
             {
                 string text = reader.ReadString();
                 int x = reader.ReadInt32();
                 int y = reader.ReadInt32();
                 ITile tile = FakeMain.tile[x, y];
                 Sign sign;
                 if (tile.active() && FakeMain.tileSign[tile.type])
                 {
                     sign = new Sign();
                     sign.text = text;
                     sign.x = x;
                     sign.y = y;
                 }
                 else
                 {
                     sign = null;
                 }
                 FakeMain.sign[i] = sign;
             }
             List<Point16> list = new List<Point16>();
             for (int j = 0; j < 1000; j++)
             {
                 if (FakeMain.sign[j] != null)
                 {
                     Point16 item = new Point16(FakeMain.sign[j].x, FakeMain.sign[j].y);
                     if (list.Contains(item))
                     {
                         FakeMain.sign[j] = null;
                     }
                     else
                     {
                         list.Add(item);
                     }
                 }
             }
             for (; i < 1000; i++)
             {
                 FakeMain.sign[i] = null;
             }
         }


         public static void LoadTileEntities(BinaryReader reader)
         {
             TileEntity.ByID.Clear();
             TileEntity.ByPosition.Clear();
             int num = reader.ReadInt32();
             int num2 = 0;
             for (int i = 0; i < num; i++)
             {
                 TileEntity tileEntity = TileEntity.Read(reader);
                 tileEntity.ID = num2++;
                 TileEntity.ByID[tileEntity.ID] = tileEntity;
                 if (TileEntity.ByPosition.TryGetValue(tileEntity.Position, out TileEntity value))
                 {
                     TileEntity.ByID.Remove(value.ID);
                 }
                 TileEntity.ByPosition[tileEntity.Position] = tileEntity;
             }
             TileEntity.TileEntitiesNextID = num;
             List<Point16> list = new List<Point16>();
             foreach (KeyValuePair<Point16, TileEntity> item in TileEntity.ByPosition)
             {
                 if (!WorldGen.InWorld(item.Value.Position.X, item.Value.Position.Y, 1))
                 {
                     list.Add(item.Value.Position);
                 }
                 else if (!TileEntity.manager.CheckValidTile(item.Value.type, item.Value.Position.X, item.Value.Position.Y))
                 {
                     list.Add(item.Value.Position);
                 }
             }
             try
             {
                 foreach (Point16 item2 in list)
                 {
                     TileEntity tileEntity2 = TileEntity.ByPosition[item2];
                     if (TileEntity.ByID.ContainsKey(tileEntity2.ID))
                     {
                         TileEntity.ByID.Remove(tileEntity2.ID);
                     }
                     if (TileEntity.ByPosition.ContainsKey(item2))
                     {
                         TileEntity.ByPosition.Remove(item2);
                     }
                 }
             }
             catch
             {
             }
         }


         private static void LoadTownManager(BinaryReader reader)
         {
             WorldGen.TownManager.Load(reader);
         }

         private static void LoadWeightedPressurePlates(BinaryReader reader)
         {
             PressurePlateHelper.Reset();
             PressurePlateHelper.NeedsFirstUpdate = true;
             int num = reader.ReadInt32();
             for (int i = 0; i < num; i++)
             {
                 Point key = new Point(reader.ReadInt32(), reader.ReadInt32());
                 PressurePlateHelper.PressurePlatesPressed.Add(key, new bool[255]);
             }
         }

         public static void LoadWorldTiles(BinaryReader reader, bool[] importance)
         {
             for (int i = 0; i < FakeMain.maxTilesX; i++)
             {
                 float num = (float)i / (float)FakeMain.maxTilesX;
                 for (int j = 0; j < FakeMain.maxTilesY; j++)
                 {
                     int num2 = -1;
                     byte b;
                     byte b2 = b = 0;
                     ITile tile = FakeMain.tile[i, j];
                     byte b3 = reader.ReadByte();
                     if ((b3 & 1) == 1)
                     {
                         b2 = reader.ReadByte();
                         if ((b2 & 1) == 1)
                         {
                             b = reader.ReadByte();
                         }
                     }
                     byte b4;
                     if ((b3 & 2) == 2)
                     {
                         tile.active(active: true);
                         if ((b3 & 0x20) == 32)
                         {
                             b4 = reader.ReadByte();
                             num2 = reader.ReadByte();
                             num2 = ((num2 << 8) | b4);
                         }
                         else
                         {
                             num2 = reader.ReadByte();
                         }
                         tile.type = (ushort)num2;
                         if (importance[num2])
                         {
                             tile.frameX = reader.ReadInt16();
                             tile.frameY = reader.ReadInt16();
                             if (tile.type == 144)
                             {
                                 tile.frameY = 0;
                             }
                         }
                         else
                         {
                             tile.frameX = -1;
                             tile.frameY = -1;
                         }
                         if ((b & 8) == 8)
                         {
                             tile.color(reader.ReadByte());
                         }
                     }
                     if ((b3 & 4) == 4)
                     {
                         tile.wall = reader.ReadByte();
                         if (tile.wall >= 316)
                         {
                             tile.wall = 0;
                         }
                         if ((b & 0x10) == 16)
                         {
                             tile.wallColor(reader.ReadByte());
                         }
                     }
                     b4 = (byte)((b3 & 0x18) >> 3);
                     if (b4 != 0)
                     {
                         tile.liquid = reader.ReadByte();
                         if (b4 > 1)
                         {
                             if (b4 == 2)
                             {
                                 tile.lava(lava: true);
                             }
                             else
                             {
                                 tile.honey(honey: true);
                             }
                         }
                     }
                     if (b2 > 1)
                     {
                         if ((b2 & 2) == 2)
                         {
                             tile.wire(wire: true);
                         }
                         if ((b2 & 4) == 4)
                         {
                             tile.wire2(wire2: true);
                         }
                         if ((b2 & 8) == 8)
                         {
                             tile.wire3(wire3: true);
                         }
                         b4 = (byte)((b2 & 0x70) >> 4);
                         if (b4 != 0 && (FakeMain.tileSolid[tile.type] || TileID.Sets.NonSolidSaveSlopes[tile.type]))
                         {
                             if (b4 == 1)
                             {
                                 tile.halfBrick(halfBrick: true);
                             }
                             else
                             {
                                 tile.slope((byte)(b4 - 1));
                             }
                         }
                     }
                     if (b > 0)
                     {
                         if ((b & 2) == 2)
                         {
                             tile.actuator(actuator: true);
                         }
                         if ((b & 4) == 4)
                         {
                             tile.inActive(inActive: true);
                         }
                         if ((b & 0x20) == 32)
                         {
                             tile.wire4(wire4: true);
                         }
                         if ((b & 0x40) == 64)
                         {
                             b4 = reader.ReadByte();
                             tile.wall = (ushort)((b4 << 8) | tile.wall);
                             if (tile.wall >= 316)
                             {
                                 tile.wall = 0;
                             }
                         }
                     }
                     int num3;
                     switch ((byte)((b3 & 0xC0) >> 6))
                     {
                         case 0:
                             num3 = 0;
                             break;
                         case 1:
                             num3 = reader.ReadByte();
                             break;
                         default:
                             num3 = reader.ReadInt16();
                             break;
                     }
                     if (num2 != -1)
                     {
                         if ((double)j <= FakeMain.worldSurface)
                         {
                             if ((double)(j + num3) <= FakeMain.worldSurface)
                             {
                                 WorldGen.tileCounts[num2] += (num3 + 1) * 5;
                             }
                             else
                             {
                                 int num4 = (int)(FakeMain.worldSurface - (double)j + 1.0);
                                 int num5 = num3 + 1 - num4;
                                 WorldGen.tileCounts[num2] += num4 * 5 + num5;
                             }
                         }
                         else
                         {
                             WorldGen.tileCounts[num2] += num3 + 1;
                         }
                     }
                     while (num3 > 0)
                     {
                         j++;
                         FakeMain.tile[i, j].CopyFrom(tile);
                         num3--;
                     }
                 }
             }
             WorldGen.AddUpAlignmentCounts(clearCounts: true);
             if (WorldFile._versionNumber < 105)
             {
                 WorldGen.FixHearts();
             }
         }

         public static void clearWorld()
         {
             FakeMain.getGoodWorld = false;
             FakeMain.drunkWorld = false;
             NPC.ResetBadgerHatTime();
             NPC.freeCake = false;
             FakeMain.mapDelay = 2;
             FakeMain.ResetWindCounter(resetExtreme: true);
             WorldGen.TownManager = new TownRoomManager();
             WorldGen.Hooks.ClearWorld();
             TileEntity.Clear();
             FakeMain.checkXMas();
             FakeMain.checkHalloween();
             if (FakeMain.mapReady)
             {
                 for (int i = 0; i < WorldGen.lastMaxTilesX; i++)
                 {
                     _ = (float)i / (float)WorldGen.lastMaxTilesX;
                 }
                 FakeMain.Map.Clear();
             }
             NPC.MoonLordCountdown = 0;
             FakeMain.forceHalloweenForToday = false;
             FakeMain.forceXMasForToday = false;
             NPC.RevengeManager.Reset();
             FakeMain.pumpkinMoon = false;
             FakeMain.clearMap = true;
             FakeMain.mapTime = 0;
             FakeMain.updateMap = false;
             FakeMain.mapReady = false;
             FakeMain.refreshMap = false;
             FakeMain.eclipse = false;
             FakeMain.slimeRain = false;
             FakeMain.slimeRainTime = 0.0;
             FakeMain.slimeWarningTime = 0;
             FakeMain.sundialCooldown = 0;
             FakeMain.fastForwardTime = false;
             BirthdayParty.WorldClear();
             LanternNight.WorldClear();
             WorldGen.mysticLogsEvent.WorldClear();
             Sandstorm.WorldClear();
             FakeMain.UpdateTimeRate();
             FakeMain.wofNPCIndex = -1;
             NPC.waveKills = 0f;
             WorldGen.spawnHardBoss = 0;
             WorldGen.totalSolid2 = 0;
             WorldGen.totalGood2 = 0;
             WorldGen.totalEvil2 = 0;
             WorldGen.totalBlood2 = 0;
             WorldGen.totalSolid = 0;
             WorldGen.totalGood = 0;
             WorldGen.totalEvil = 0;
             WorldGen.totalBlood = 0;
             WorldFile.ResetTemps();
             FakeMain.maxRaining = 0f;
             WorldGen.totalX = 0;
             WorldGen.totalD = 0;
             WorldGen.tEvil = 0;
             WorldGen.tBlood = 0;
             WorldGen.tGood = 0;
             WorldGen.spawnEye = false;
             WorldGen.prioritizedTownNPCType = 0;
             WorldGen.shadowOrbCount = 0;
             WorldGen.altarCount = 0;
             WorldGen.SavedOreTiers.Copper = -1;
             WorldGen.SavedOreTiers.Iron = -1;
             WorldGen.SavedOreTiers.Silver = -1;
             WorldGen.SavedOreTiers.Gold = -1;
             WorldGen.SavedOreTiers.Cobalt = -1;
             WorldGen.SavedOreTiers.Mythril = -1;
             WorldGen.SavedOreTiers.Adamantite = -1;
             FakeMain.cloudBGActive = 0f;
             FakeMain.raining = false;
             FakeMain.hardMode = false;
             FakeMain.helpText = 0;
             FakeMain.BartenderHelpTextIndex = 0;
             FakeMain.dungeonX = 0;
             FakeMain.dungeonY = 0;
             NPC.downedBoss1 = false;
             NPC.downedBoss2 = false;
             NPC.downedBoss3 = false;
             NPC.downedQueenBee = false;
             NPC.downedSlimeKing = false;
             NPC.downedMechBossAny = false;
             NPC.downedMechBoss1 = false;
             NPC.downedMechBoss2 = false;
             NPC.downedMechBoss3 = false;
             NPC.downedFishron = false;
             NPC.downedAncientCultist = false;
             NPC.downedMoonlord = false;
             NPC.downedHalloweenKing = false;
             NPC.downedHalloweenTree = false;
             NPC.downedChristmasIceQueen = false;
             NPC.downedChristmasSantank = false;
             NPC.downedChristmasTree = false;
             NPC.downedPlantBoss = false;
             NPC.downedGolemBoss = false;
             NPC.downedEmpressOfLight = false;
             NPC.downedQueenSlime = false;
             NPC.combatBookWasUsed = false;
             NPC.savedStylist = false;
             NPC.savedGoblin = false;
             NPC.savedWizard = false;
             NPC.savedMech = false;
             NPC.savedTaxCollector = false;
             NPC.savedAngler = false;
             NPC.savedBartender = false;
             NPC.savedGolfer = false;
             NPC.boughtCat = false;
             NPC.boughtDog = false;
             NPC.boughtBunny = false;
             NPC.downedGoblins = false;
             NPC.downedClown = false;
             NPC.downedFrost = false;
             NPC.downedPirates = false;
             NPC.downedMartians = false;
             NPC.downedTowerSolar = (NPC.downedTowerVortex = (NPC.downedTowerNebula = (NPC.downedTowerStardust = (NPC.LunarApocalypseIsUp = false))));
             NPC.TowerActiveSolar = (NPC.TowerActiveVortex = (NPC.TowerActiveNebula = (NPC.TowerActiveStardust = false)));
             DD2Event.ResetProgressEntirely();
             NPC.ClearFoundActiveNPCs();
             FakeMain.BestiaryTracker.Reset();
             FakeMain.PylonSystem.Reset();
             Terraria.GameContent.Creative.CreativePowerManager.Instance.Reset();
             FakeMain.CreativeMenu.Reset();
             WorldGen.shadowOrbSmashed = false;
             WorldGen.spawnMeteor = false;
             WorldGen.stopDrops = false;
             FakeMain.invasionDelay = 0;
             FakeMain.invasionType = 0;
             FakeMain.invasionSize = 0;
             FakeMain.invasionWarn = 0;
             FakeMain.invasionX = 0.0;
             FakeMain.invasionSizeStart = 0;
             FakeMain.treeX[0] = FakeMain.maxTilesX;
             FakeMain.treeX[1] = FakeMain.maxTilesX;
             FakeMain.treeX[2] = FakeMain.maxTilesX;
             FakeMain.treeStyle[0] = 0;
             FakeMain.treeStyle[1] = 0;
             FakeMain.treeStyle[2] = 0;
             FakeMain.treeStyle[3] = 0;
             WorldGen.noLiquidCheck = false;
             Liquid.numLiquid = 0;
             LiquidBuffer.numLiquidBuffer = 0;
             if (FakeMain.netMode == 1 || WorldGen.lastMaxTilesX > FakeMain.maxTilesX || WorldGen.lastMaxTilesY > FakeMain.maxTilesY)
             {
                 for (int j = 0; j < WorldGen.lastMaxTilesX; j++)
                 {
                     float num = (float)j / (float)WorldGen.lastMaxTilesX;
                     for (int k = 0; k < WorldGen.lastMaxTilesY; k++)
                     {
                         FakeMain.tile[j, k] = null;
                     }
                 }
             }
             WorldGen.lastMaxTilesX = FakeMain.maxTilesX;
             WorldGen.lastMaxTilesY = FakeMain.maxTilesY;
             if (FakeMain.netMode != 2)
             {
                 FakeMain.sectionManager = new WorldSections(FakeMain.maxTilesX / 200, FakeMain.maxTilesY / 150);
             }
             if (FakeMain.netMode != 1)
             {
                 for (int l = 0; l < FakeMain.maxTilesX; l++)
                 {
                     float num2 = (float)l / (float)FakeMain.maxTilesX;
                     for (int m = 0; m < FakeMain.maxTilesY; m++)
                     {
                         if (FakeMain.tile[l, m] == null)
                         {
                             var itile = OTAPI.Hooks.Tile.CreateTile?.Invoke();
                             if (itile == null)
                             {
                                 DynamicMethod dynamicMethod = new DynamicMethod("GetTileCollection", typeof(ITile), null);
                                 ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
                                 iLGenerator.Emit(OpCodes.Newobj, typeof(global::Terraria.Tile).GetConstructors().Single((ConstructorInfo x) => x.GetParameters().Length == 0));
                                 iLGenerator.Emit(OpCodes.Ret);
                                 itile = (ITile)dynamicMethod.CreateDelegate(typeof(Func<ITile>));
                             }
                             FakeMain.tile[l, m] = itile;
                         }
                         else
                         {
                             FakeMain.tile[l, m].ClearEverything();
                         }
                     }
                 }
             }
             for (int n = 0; n < FakeMain.countsAsHostForGameplay.Length; n++)
             {
                 FakeMain.countsAsHostForGameplay[n] = false;
             }
             CombatText.clearAll();
             for (int num3 = 0; num3 < 6000; num3++)
             {
                 FakeMain.dust[num3] = new Dust();
                 FakeMain.dust[num3].dustIndex = num3;
             }
             for (int num4 = 0; num4 < 600; num4++)
             {
                 FakeMain.gore[num4] = new Gore();
             }
             for (int num5 = 0; num5 < 400; num5++)
             {
                 FakeMain.item[num5] = new Item();
                 FakeMain.timeItemSlotCannotBeReusedFor[num5] = 0;
             }
             for (int num6 = 0; num6 < 200; num6++)
             {
                 FakeMain.npc[num6] = new NPC();
             }
             for (int num7 = 0; num7 < 1000; num7++)
             {
                 FakeMain.projectile[num7] = new Projectile();
             }
             for (int num8 = 0; num8 < 8000; num8++)
             {
                 FakeMain.chest[num8] = null;
             }
             for (int num9 = 0; num9 < 1000; num9++)
             {
                 FakeMain.sign[num9] = null;
             }
             for (int num10 = 0; num10 < Liquid.maxLiquid; num10++)
             {
                 FakeMain.liquid[num10] = new Liquid();
             }
             for (int num11 = 0; num11 < 50000; num11++)
             {
                 FakeMain.liquidBuffer[num11] = new LiquidBuffer();
             }
             WorldGen.setWorldSize();
             Star.SpawnStars();
         }
     }*/
}
