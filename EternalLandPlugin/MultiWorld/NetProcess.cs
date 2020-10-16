using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using OTAPI;
using OTAPI.Callbacks.Terraria;
using OTAPI.Tile;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Events;
using Terraria.GameContent.Golf;
using Terraria.GameContent.Tile_Entities;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Net;
using Terraria.Testing;
using Terraria.UI;

namespace EternalLandPlugin.MultiWorld
{
	/*public class MessageBuffer
	{
		internal static bool NameCollision(Player player)
		{
			Hooks.Player.NameCollisionHandler nameCollision = Hooks.Player.NameCollision;
			HookResult? hookResult = (nameCollision != null) ? new HookResult?(nameCollision(player)) : null;
			bool flag = hookResult != null;
			return !flag || hookResult.Value == HookResult.Continue;
		}

		public static event TileChangeReceivedEvent OnTileChangeReceived;

		public void Reset()
		{
			Array.Clear(this.readBuffer, 0, this.readBuffer.Length);
			this.writeLocked = false;
			this.messageLength = 0;
			this.totalData = 0;
			this.spamCount = 0;
			this.broadcast = false;
			this.checkBytes = false;
			this.ResetReader();
			this.ResetWriter();
		}

		public void ResetReader()
		{
			if (this.readerStream != null)
			{
				this.readerStream.Close();
			}
			this.readerStream = new MemoryStream(this.readBuffer);
			this.reader = new BinaryReader(this.readerStream);
		}

		public void ResetWriter()
		{
		}

		public HookResult tempGetData(MessageBuffer buffer, ref byte packetid, ref int readoffset, ref int start, ref int length)
        {
			GetData(buffer, ref packetid, ref readoffset, ref start, ref length);
			return HookResult.Cancel;
        }

		public void GetData(MessageBuffer buffer, ref byte packetid, ref int readoffset, ref int start, ref int length)
		{
			this.reader = buffer.reader;
			if (this.whoAmI < 256)
			{
				Netplay.Clients[this.whoAmI].TimeOutTimer = 0;
			}
			else
			{
				Netplay.Connection.TimeOutTimer = 0;
			}
			byte b = 0;
			int num = 0;
			num = start + 1;
			b = this.readBuffer[start];
			if (b >= 140)
			{
				return;
			}
			LoadWorld.FakeMain.ActiveNetDiagnosticsUI.CountReadMessage((int)b, length);
			if (LoadWorld.FakeMain.netMode == 1 && Netplay.Connection.StatusMax > 0)
			{
				Netplay.Connection.StatusCount++;
			}
			if (LoadWorld.FakeMain.verboseNetplay)
			{
				for (int i = start; i < start + length; i++)
				{
				}
				for (int j = start; j < start + length; j++)
				{
					byte b2 = this.readBuffer[j];
				}
			}
			if (LoadWorld.FakeMain.netMode == 2 && b != 38 && Netplay.Clients[this.whoAmI].State == -1)
			{
				NetMessage.TrySendData(2, this.whoAmI, -1, Lang.mp[1].ToNetworkText(), 0, 0f, 0f, 0f, 0, 0, 0);
				return;
			}
			if (LoadWorld.FakeMain.netMode == 2)
			{
				if (Netplay.Clients[this.whoAmI].State < 10 && b > 12 && b != 93 && b != 16 && b != 42 && b != 50 && b != 38 && b != 68)
				{
					NetMessage.BootPlayer(this.whoAmI, Lang.mp[2].ToNetworkText());
				}
				if (Netplay.Clients[this.whoAmI].State == 0 && b != 1)
				{
					NetMessage.BootPlayer(this.whoAmI, Lang.mp[2].ToNetworkText());
				}
			}
			if (this.reader == null)
			{
				this.ResetReader();
			}
			this.reader.BaseStream.Position = (long)num;
			switch (b)
			{
				case 1:
					if (LoadWorld.FakeMain.netMode != 2)
					{
						return;
					}
					if (LoadWorld.FakeMain.dedServ && Netplay.IsBanned(Netplay.Clients[this.whoAmI].Socket.GetRemoteAddress()))
					{
						NetMessage.TrySendData(2, this.whoAmI, -1, Lang.mp[3].ToNetworkText(), 0, 0f, 0f, 0f, 0, 0, 0);
						return;
					}
					if (Netplay.Clients[this.whoAmI].State != 0)
					{
						return;
					}
					if (!(this.reader.ReadString() == "Terraria" + 230))
					{
						NetMessage.TrySendData(2, this.whoAmI, -1, Lang.mp[4].ToNetworkText(), 0, 0f, 0f, 0f, 0, 0, 0);
						return;
					}
					if (string.IsNullOrEmpty(Netplay.ServerPassword))
					{
						Netplay.Clients[this.whoAmI].State = 1;
						NetMessage.TrySendData(3, this.whoAmI, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
						return;
					}
					Netplay.Clients[this.whoAmI].State = -1;
					NetMessage.TrySendData(37, this.whoAmI, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
					return;
				case 2:
					if (LoadWorld.FakeMain.netMode != 1)
					{
						return;
					}
					Netplay.Disconnect = true;
					return;
				case 3:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						if (Netplay.Connection.State == 1)
						{
							Netplay.Connection.State = 2;
						}
						int num2 = (int)this.reader.ReadByte();
						if (num2 != LoadWorld.FakeMain.myPlayer)
						{
							LoadWorld.FakeMain.player[num2] = LoadWorld.FakeMain.ActivePlayerFileData.Player;
							LoadWorld.FakeMain.player[LoadWorld.FakeMain.myPlayer] = new Player();
						}
						LoadWorld.FakeMain.player[num2].whoAmI = num2;
						LoadWorld.FakeMain.myPlayer = num2;
						Player player = LoadWorld.FakeMain.player[num2];
						NetMessage.TrySendData(4, -1, -1, null, num2, 0f, 0f, 0f, 0, 0, 0);
						NetMessage.TrySendData(68, -1, -1, null, num2, 0f, 0f, 0f, 0, 0, 0);
						NetMessage.TrySendData(16, -1, -1, null, num2, 0f, 0f, 0f, 0, 0, 0);
						NetMessage.TrySendData(42, -1, -1, null, num2, 0f, 0f, 0f, 0, 0, 0);
						NetMessage.TrySendData(50, -1, -1, null, num2, 0f, 0f, 0f, 0, 0, 0);
						for (int k = 0; k < 59; k++)
						{
							NetMessage.TrySendData(5, -1, -1, null, num2, (float)k, (float)player.inventory[k].prefix, 0f, 0, 0, 0);
						}
						for (int l = 0; l < player.armor.Length; l++)
						{
							NetMessage.TrySendData(5, -1, -1, null, num2, (float)(59 + l), (float)player.armor[l].prefix, 0f, 0, 0, 0);
						}
						for (int m = 0; m < player.dye.Length; m++)
						{
							NetMessage.TrySendData(5, -1, -1, null, num2, (float)(58 + player.armor.Length + 1 + m), (float)player.dye[m].prefix, 0f, 0, 0, 0);
						}
						for (int n = 0; n < player.miscEquips.Length; n++)
						{
							NetMessage.TrySendData(5, -1, -1, null, num2, (float)(58 + player.armor.Length + player.dye.Length + 1 + n), (float)player.miscEquips[n].prefix, 0f, 0, 0, 0);
						}
						for (int num3 = 0; num3 < player.miscDyes.Length; num3++)
						{
							NetMessage.TrySendData(5, -1, -1, null, num2, (float)(58 + player.armor.Length + player.dye.Length + player.miscEquips.Length + 1 + num3), (float)player.miscDyes[num3].prefix, 0f, 0, 0, 0);
						}
						for (int num4 = 0; num4 < player.bank.item.Length; num4++)
						{
							NetMessage.TrySendData(5, -1, -1, null, num2, (float)(58 + player.armor.Length + player.dye.Length + player.miscEquips.Length + player.miscDyes.Length + 1 + num4), (float)player.bank.item[num4].prefix, 0f, 0, 0, 0);
						}
						for (int num5 = 0; num5 < player.bank2.item.Length; num5++)
						{
							NetMessage.TrySendData(5, -1, -1, null, num2, (float)(58 + player.armor.Length + player.dye.Length + player.miscEquips.Length + player.miscDyes.Length + player.bank.item.Length + 1 + num5), (float)player.bank2.item[num5].prefix, 0f, 0, 0, 0);
						}
						NetMessage.TrySendData(5, -1, -1, null, num2, (float)(58 + player.armor.Length + player.dye.Length + player.miscEquips.Length + player.miscDyes.Length + player.bank.item.Length + player.bank2.item.Length + 1), (float)player.trashItem.prefix, 0f, 0, 0, 0);
						for (int num6 = 0; num6 < player.bank3.item.Length; num6++)
						{
							NetMessage.TrySendData(5, -1, -1, null, num2, (float)(58 + player.armor.Length + player.dye.Length + player.miscEquips.Length + player.miscDyes.Length + player.bank.item.Length + player.bank2.item.Length + 2 + num6), (float)player.bank3.item[num6].prefix, 0f, 0, 0, 0);
						}
						for (int num7 = 0; num7 < player.bank4.item.Length; num7++)
						{
							NetMessage.TrySendData(5, -1, -1, null, num2, (float)(58 + player.armor.Length + player.dye.Length + player.miscEquips.Length + player.miscDyes.Length + player.bank.item.Length + player.bank2.item.Length + player.bank3.item.Length + 2 + num7), (float)player.bank4.item[num7].prefix, 0f, 0, 0, 0);
						}
						NetMessage.TrySendData(6, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
						if (Netplay.Connection.State == 2)
						{
							Netplay.Connection.State = 3;
							return;
						}
						return;
					}
				case 4:
					{
						int num8 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num8 = this.whoAmI;
						}
						if (num8 == LoadWorld.FakeMain.myPlayer && !LoadWorld.FakeMain.ServerSideCharacter)
						{
							return;
						}
						Player player2 = LoadWorld.FakeMain.player[num8];
						player2.whoAmI = num8;
						player2.skinVariant = (int)this.reader.ReadByte();
						player2.skinVariant = (int)MathHelper.Clamp((float)player2.skinVariant, 0f, 11f);
						player2.hair = (int)this.reader.ReadByte();
						if (player2.hair >= 162)
						{
							player2.hair = 0;
						}
						player2.name = this.reader.ReadString().Trim().Trim();
						player2.hairDye = this.reader.ReadByte();
						BitsByte bitsByte = this.reader.ReadByte();
						for (int num9 = 0; num9 < 8; num9++)
						{
							player2.hideVisibleAccessory[num9] = bitsByte[num9];
						}
						bitsByte = this.reader.ReadByte();
						for (int num10 = 0; num10 < 2; num10++)
						{
							player2.hideVisibleAccessory[num10 + 8] = bitsByte[num10];
						}
						player2.hideMisc = this.reader.ReadByte();
						player2.hairColor = this.reader.ReadRGB();
						player2.skinColor = this.reader.ReadRGB();
						player2.eyeColor = this.reader.ReadRGB();
						player2.shirtColor = this.reader.ReadRGB();
						player2.underShirtColor = this.reader.ReadRGB();
						player2.pantsColor = this.reader.ReadRGB();
						player2.shoeColor = this.reader.ReadRGB();
						BitsByte bitsByte2 = this.reader.ReadByte();
						player2.difficulty = 0;
						if (bitsByte2[0])
						{
							player2.difficulty = 1;
						}
						if (bitsByte2[1])
						{
							player2.difficulty = 2;
						}
						if (bitsByte2[3])
						{
							player2.difficulty = 3;
						}
						if (player2.difficulty > 3)
						{
							player2.difficulty = 3;
						}
						player2.extraAccessory = bitsByte2[2];
						BitsByte bitsByte3 = this.reader.ReadByte();
						player2.UsingBiomeTorches = bitsByte3[0];
						player2.happyFunTorchTime = bitsByte3[1];
						if (LoadWorld.FakeMain.netMode != 2)
						{
							return;
						}
						bool flag = false;
						if (Netplay.Clients[this.whoAmI].State < 10)
						{
							for (int num11 = 0; num11 < 255; num11++)
							{
								if (num11 != num8 && player2.name == LoadWorld.FakeMain.player[num11].name && Netplay.Clients[num11].IsActive)
								{
									flag = true;
								}
							}
						}
						if (flag && NameCollision(player2))
						{
							NetMessage.TrySendData(2, this.whoAmI, -1, NetworkText.FromKey(Lang.mp[5].Key, new object[]
							{
						player2.name
							}), 0, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						if (player2.name.Length > Player.nameLen)
						{
							NetMessage.TrySendData(2, this.whoAmI, -1, NetworkText.FromKey("Net.NameTooLong", new object[0]), 0, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						if (player2.name == "")
						{
							NetMessage.TrySendData(2, this.whoAmI, -1, NetworkText.FromKey("Net.EmptyName", new object[0]), 0, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						if (player2.difficulty == 3 && !LoadWorld.FakeMain.GameModeInfo.IsJourneyMode)
						{
							NetMessage.TrySendData(2, this.whoAmI, -1, NetworkText.FromKey("Net.PlayerIsCreativeAndWorldIsNotCreative", new object[0]), 0, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						if (player2.difficulty != 3 && LoadWorld.FakeMain.GameModeInfo.IsJourneyMode)
						{
							NetMessage.TrySendData(2, this.whoAmI, -1, NetworkText.FromKey("Net.PlayerIsNotCreativeAndWorldIsCreative", new object[0]), 0, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						Netplay.Clients[this.whoAmI].Name = player2.name;
						Netplay.Clients[this.whoAmI].Name = player2.name;
						NetMessage.TrySendData(4, -1, this.whoAmI, null, num8, 0f, 0f, 0f, 0, 0, 0);
						return;
					}
				case 5:
					{
						int num12 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num12 = this.whoAmI;
						}
						if (num12 == LoadWorld.FakeMain.myPlayer && !LoadWorld.FakeMain.ServerSideCharacter && !LoadWorld.FakeMain.player[num12].HasLockedInventory())
						{
							return;
						}
						Player player3 = LoadWorld.FakeMain.player[num12];
						Player obj = player3;
						lock (obj)
						{
							int num13 = (int)this.reader.ReadInt16();
							int stack = (int)this.reader.ReadInt16();
							int num14 = (int)this.reader.ReadByte();
							int type = (int)this.reader.ReadInt16();
							Item[] array = null;
							Item[] array2 = null;
							int num15 = 0;
							bool flag3 = false;
							if (num13 > 58 + player3.armor.Length + player3.dye.Length + player3.miscEquips.Length + player3.miscDyes.Length + player3.bank.item.Length + player3.bank2.item.Length + player3.bank3.item.Length + 1)
							{
								num15 = num13 - 58 - (player3.armor.Length + player3.dye.Length + player3.miscEquips.Length + player3.miscDyes.Length + player3.bank.item.Length + player3.bank2.item.Length + player3.bank3.item.Length + 1) - 1;
								array = player3.bank4.item;
								array2 = LoadWorld.FakeMain.clientPlayer.bank4.item;
							}
							else if (num13 > 58 + player3.armor.Length + player3.dye.Length + player3.miscEquips.Length + player3.miscDyes.Length + player3.bank.item.Length + player3.bank2.item.Length + 1)
							{
								num15 = num13 - 58 - (player3.armor.Length + player3.dye.Length + player3.miscEquips.Length + player3.miscDyes.Length + player3.bank.item.Length + player3.bank2.item.Length + 1) - 1;
								array = player3.bank3.item;
								array2 = LoadWorld.FakeMain.clientPlayer.bank3.item;
							}
							else if (num13 > 58 + player3.armor.Length + player3.dye.Length + player3.miscEquips.Length + player3.miscDyes.Length + player3.bank.item.Length + player3.bank2.item.Length)
							{
								flag3 = true;
							}
							else if (num13 > 58 + player3.armor.Length + player3.dye.Length + player3.miscEquips.Length + player3.miscDyes.Length + player3.bank.item.Length)
							{
								num15 = num13 - 58 - (player3.armor.Length + player3.dye.Length + player3.miscEquips.Length + player3.miscDyes.Length + player3.bank.item.Length) - 1;
								array = player3.bank2.item;
								array2 = LoadWorld.FakeMain.clientPlayer.bank2.item;
							}
							else if (num13 > 58 + player3.armor.Length + player3.dye.Length + player3.miscEquips.Length + player3.miscDyes.Length)
							{
								num15 = num13 - 58 - (player3.armor.Length + player3.dye.Length + player3.miscEquips.Length + player3.miscDyes.Length) - 1;
								array = player3.bank.item;
								array2 = LoadWorld.FakeMain.clientPlayer.bank.item;
							}
							else if (num13 > 58 + player3.armor.Length + player3.dye.Length + player3.miscEquips.Length)
							{
								num15 = num13 - 58 - (player3.armor.Length + player3.dye.Length + player3.miscEquips.Length) - 1;
								array = player3.miscDyes;
								array2 = LoadWorld.FakeMain.clientPlayer.miscDyes;
							}
							else if (num13 > 58 + player3.armor.Length + player3.dye.Length)
							{
								num15 = num13 - 58 - (player3.armor.Length + player3.dye.Length) - 1;
								array = player3.miscEquips;
								array2 = LoadWorld.FakeMain.clientPlayer.miscEquips;
							}
							else if (num13 > 58 + player3.armor.Length)
							{
								num15 = num13 - 58 - player3.armor.Length - 1;
								array = player3.dye;
								array2 = LoadWorld.FakeMain.clientPlayer.dye;
							}
							else if (num13 > 58)
							{
								num15 = num13 - 58 - 1;
								array = player3.armor;
								array2 = LoadWorld.FakeMain.clientPlayer.armor;
							}
							else
							{
								num15 = num13;
								array = player3.inventory;
								array2 = LoadWorld.FakeMain.clientPlayer.inventory;
							}
							if (flag3)
							{
								player3.trashItem = new Item();
								player3.trashItem.netDefaults(type);
								player3.trashItem.stack = stack;
								player3.trashItem.Prefix(num14);
								if (num12 == LoadWorld.FakeMain.myPlayer && !LoadWorld.FakeMain.ServerSideCharacter)
								{
									LoadWorld.FakeMain.clientPlayer.trashItem = player3.trashItem.Clone();
								}
							}
							else if (num13 <= 58)
							{
								int type2 = array[num15].type;
								int stack2 = array[num15].stack;
								array[num15] = new Item();
								array[num15].netDefaults(type);
								array[num15].stack = stack;
								array[num15].Prefix(num14);
								if (num12 == LoadWorld.FakeMain.myPlayer && !LoadWorld.FakeMain.ServerSideCharacter)
								{
									array2[num15] = array[num15].Clone();
								}
								if (num12 == LoadWorld.FakeMain.myPlayer && num15 == 58)
								{
									LoadWorld.FakeMain.mouseItem = array[num15].Clone();
								}
								if (num12 == LoadWorld.FakeMain.myPlayer && LoadWorld.FakeMain.netMode == 1)
								{
									LoadWorld.FakeMain.player[num12].inventoryChestStack[num13] = false;
									if (array[num15].stack != stack2 || array[num15].type != type2)
									{
										Recipe.FindRecipes(true);
										SoundEngine.PlaySound(7, -1, -1, 1, 1f, 0f);
									}
								}
							}
							else
							{
								array[num15] = new Item();
								array[num15].netDefaults(type);
								array[num15].stack = stack;
								array[num15].Prefix(num14);
								if (num12 == LoadWorld.FakeMain.myPlayer && !LoadWorld.FakeMain.ServerSideCharacter)
								{
									array2[num15] = array[num15].Clone();
								}
							}
							if (LoadWorld.FakeMain.netMode == 2 && num12 == this.whoAmI && num13 <= 58 + player3.armor.Length + player3.dye.Length + player3.miscEquips.Length + player3.miscDyes.Length)
							{
								NetMessage.TrySendData(5, -1, this.whoAmI, null, num12, (float)num13, (float)num14, 0f, 0, 0, 0);
							}
							return;
						}
						break;
					}
				case 6:
					break;
				case 7:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						LoadWorld.FakeMain.time = (double)this.reader.ReadInt32();
						BitsByte bitsByte4 = this.reader.ReadByte();
						LoadWorld.FakeMain.dayTime = bitsByte4[0];
						LoadWorld.FakeMain.bloodMoon = bitsByte4[1];
						LoadWorld.FakeMain.eclipse = bitsByte4[2];
						LoadWorld.FakeMain.moonPhase = (int)this.reader.ReadByte();
						LoadWorld.FakeMain.maxTilesX = (int)this.reader.ReadInt16();
						LoadWorld.FakeMain.maxTilesY = (int)this.reader.ReadInt16();
						LoadWorld.FakeMain.spawnTileX = (int)this.reader.ReadInt16();
						LoadWorld.FakeMain.spawnTileY = (int)this.reader.ReadInt16();
						LoadWorld.FakeMain.worldSurface = (double)this.reader.ReadInt16();
						LoadWorld.FakeMain.rockLayer = (double)this.reader.ReadInt16();
						LoadWorld.FakeMain.worldID = this.reader.ReadInt32();
						LoadWorld.FakeMain.worldName = this.reader.ReadString();
						LoadWorld.FakeMain.GameMode = (int)this.reader.ReadByte();
						LoadWorld.FakeMain.ActiveWorldFileData.UniqueId = new Guid(this.reader.ReadBytes(16));
						LoadWorld.FakeMain.ActiveWorldFileData.WorldGeneratorVersion = this.reader.ReadUInt64();
						LoadWorld.FakeMain.moonType = (int)this.reader.ReadByte();
						WorldGen.setBG(0, (int)this.reader.ReadByte());
						WorldGen.setBG(10, (int)this.reader.ReadByte());
						WorldGen.setBG(11, (int)this.reader.ReadByte());
						WorldGen.setBG(12, (int)this.reader.ReadByte());
						WorldGen.setBG(1, (int)this.reader.ReadByte());
						WorldGen.setBG(2, (int)this.reader.ReadByte());
						WorldGen.setBG(3, (int)this.reader.ReadByte());
						WorldGen.setBG(4, (int)this.reader.ReadByte());
						WorldGen.setBG(5, (int)this.reader.ReadByte());
						WorldGen.setBG(6, (int)this.reader.ReadByte());
						WorldGen.setBG(7, (int)this.reader.ReadByte());
						WorldGen.setBG(8, (int)this.reader.ReadByte());
						WorldGen.setBG(9, (int)this.reader.ReadByte());
						LoadWorld.FakeMain.iceBackStyle = (int)this.reader.ReadByte();
						LoadWorld.FakeMain.jungleBackStyle = (int)this.reader.ReadByte();
						LoadWorld.FakeMain.hellBackStyle = (int)this.reader.ReadByte();
						LoadWorld.FakeMain.windSpeedTarget = this.reader.ReadSingle();
						LoadWorld.FakeMain.numClouds = (int)this.reader.ReadByte();
						for (int num16 = 0; num16 < 3; num16++)
						{
							LoadWorld.FakeMain.treeX[num16] = this.reader.ReadInt32();
						}
						for (int num17 = 0; num17 < 4; num17++)
						{
							LoadWorld.FakeMain.treeStyle[num17] = (int)this.reader.ReadByte();
						}
						for (int num18 = 0; num18 < 3; num18++)
						{
							LoadWorld.FakeMain.caveBackX[num18] = this.reader.ReadInt32();
						}
						for (int num19 = 0; num19 < 4; num19++)
						{
							LoadWorld.FakeMain.caveBackStyle[num19] = (int)this.reader.ReadByte();
						}
						WorldGen.TreeTops.SyncReceive(this.reader);
						WorldGen.BackgroundsCache.UpdateCache();
						LoadWorld.FakeMain.maxRaining = this.reader.ReadSingle();
						LoadWorld.FakeMain.raining = (LoadWorld.FakeMain.maxRaining > 0f);
						BitsByte bitsByte5 = this.reader.ReadByte();
						WorldGen.shadowOrbSmashed = bitsByte5[0];
						NPC.downedBoss1 = bitsByte5[1];
						NPC.downedBoss2 = bitsByte5[2];
						NPC.downedBoss3 = bitsByte5[3];
						LoadWorld.FakeMain.hardMode = bitsByte5[4];
						NPC.downedClown = bitsByte5[5];
						LoadWorld.FakeMain.ServerSideCharacter = bitsByte5[6];
						NPC.downedPlantBoss = bitsByte5[7];
						BitsByte bitsByte6 = this.reader.ReadByte();
						NPC.downedMechBoss1 = bitsByte6[0];
						NPC.downedMechBoss2 = bitsByte6[1];
						NPC.downedMechBoss3 = bitsByte6[2];
						NPC.downedMechBossAny = bitsByte6[3];
						LoadWorld.FakeMain.cloudBGActive = (float)(bitsByte6[4] ? 1 : 0);
						WorldGen.crimson = bitsByte6[5];
						LoadWorld.FakeMain.pumpkinMoon = bitsByte6[6];
						LoadWorld.FakeMain.snowMoon = bitsByte6[7];
						BitsByte bitsByte7 = this.reader.ReadByte();
						LoadWorld.FakeMain.fastForwardTime = bitsByte7[1];
						LoadWorld.FakeMain.UpdateTimeRate();
						bool flag4 = bitsByte7[2];
						NPC.downedSlimeKing = bitsByte7[3];
						NPC.downedQueenBee = bitsByte7[4];
						NPC.downedFishron = bitsByte7[5];
						NPC.downedMartians = bitsByte7[6];
						NPC.downedAncientCultist = bitsByte7[7];
						BitsByte bitsByte8 = this.reader.ReadByte();
						NPC.downedMoonlord = bitsByte8[0];
						NPC.downedHalloweenKing = bitsByte8[1];
						NPC.downedHalloweenTree = bitsByte8[2];
						NPC.downedChristmasIceQueen = bitsByte8[3];
						NPC.downedChristmasSantank = bitsByte8[4];
						NPC.downedChristmasTree = bitsByte8[5];
						NPC.downedGolemBoss = bitsByte8[6];
						BirthdayParty.ManualParty = bitsByte8[7];
						BitsByte bitsByte9 = this.reader.ReadByte();
						NPC.downedPirates = bitsByte9[0];
						NPC.downedFrost = bitsByte9[1];
						NPC.downedGoblins = bitsByte9[2];
						Sandstorm.Happening = bitsByte9[3];
						DD2Event.Ongoing = bitsByte9[4];
						DD2Event.DownedInvasionT1 = bitsByte9[5];
						DD2Event.DownedInvasionT2 = bitsByte9[6];
						DD2Event.DownedInvasionT3 = bitsByte9[7];
						BitsByte bitsByte10 = this.reader.ReadByte();
						NPC.combatBookWasUsed = bitsByte10[0];
						LanternNight.ManualLanterns = bitsByte10[1];
						NPC.downedTowerSolar = bitsByte10[2];
						NPC.downedTowerVortex = bitsByte10[3];
						NPC.downedTowerNebula = bitsByte10[4];
						NPC.downedTowerStardust = bitsByte10[5];
						LoadWorld.FakeMain.forceHalloweenForToday = bitsByte10[6];
						LoadWorld.FakeMain.forceXMasForToday = bitsByte10[7];
						BitsByte bitsByte11 = this.reader.ReadByte();
						NPC.boughtCat = bitsByte11[0];
						NPC.boughtDog = bitsByte11[1];
						NPC.boughtBunny = bitsByte11[2];
						NPC.freeCake = bitsByte11[3];
						LoadWorld.FakeMain.drunkWorld = bitsByte11[4];
						NPC.downedEmpressOfLight = bitsByte11[5];
						NPC.downedQueenSlime = bitsByte11[6];
						LoadWorld.FakeMain.getGoodWorld = bitsByte11[7];
						WorldGen.SavedOreTiers.Copper = (int)this.reader.ReadInt16();
						WorldGen.SavedOreTiers.Iron = (int)this.reader.ReadInt16();
						WorldGen.SavedOreTiers.Silver = (int)this.reader.ReadInt16();
						WorldGen.SavedOreTiers.Gold = (int)this.reader.ReadInt16();
						WorldGen.SavedOreTiers.Cobalt = (int)this.reader.ReadInt16();
						WorldGen.SavedOreTiers.Mythril = (int)this.reader.ReadInt16();
						WorldGen.SavedOreTiers.Adamantite = (int)this.reader.ReadInt16();
						if (flag4)
						{
							LoadWorld.FakeMain.StartSlimeRain(true);
						}
						else
						{
							LoadWorld.FakeMain.StopSlimeRain(true);
						}
						LoadWorld.FakeMain.invasionType = (int)this.reader.ReadSByte();
						LoadWorld.FakeMain.LobbyId = this.reader.ReadUInt64();
						Sandstorm.IntendedSeverity = this.reader.ReadSingle();
						if (Netplay.Connection.State == 3)
						{
							LoadWorld.FakeMain.windSpeedCurrent = LoadWorld.FakeMain.windSpeedTarget;
							Netplay.Connection.State = 4;
						}
						LoadWorld.FakeMain.checkHalloween();
						LoadWorld.FakeMain.checkXMas();
						return;
					}
				case 8:
					{
						if (LoadWorld.FakeMain.netMode != 2)
						{
							return;
						}
						int num20 = this.reader.ReadInt32();
						int num21 = this.reader.ReadInt32();
						bool flag5 = true;
						if (num20 == -1 || num21 == -1)
						{
							flag5 = false;
						}
						else if (num20 < 10 || num20 > LoadWorld.FakeMain.maxTilesX - 10)
						{
							flag5 = false;
						}
						else if (num21 < 10 || num21 > LoadWorld.FakeMain.maxTilesY - 10)
						{
							flag5 = false;
						}
						int num22 = Netplay.GetSectionX(LoadWorld.FakeMain.spawnTileX) - 2;
						int num23 = Netplay.GetSectionY(LoadWorld.FakeMain.spawnTileY) - 1;
						int num24 = num22 + 5;
						int num25 = num23 + 3;
						if (num22 < 0)
						{
							num22 = 0;
						}
						if (num24 >= LoadWorld.FakeMain.maxSectionsX)
						{
							num24 = LoadWorld.FakeMain.maxSectionsX - 1;
						}
						if (num23 < 0)
						{
							num23 = 0;
						}
						if (num25 >= LoadWorld.FakeMain.maxSectionsY)
						{
							num25 = LoadWorld.FakeMain.maxSectionsY - 1;
						}
						int num26 = (num24 - num22) * (num25 - num23);
						List<Point> list = new List<Point>();
						for (int num27 = num22; num27 < num24; num27++)
						{
							for (int num28 = num23; num28 < num25; num28++)
							{
								list.Add(new Point(num27, num28));
							}
						}
						int num29 = -1;
						int num30 = -1;
						if (flag5)
						{
							num20 = Netplay.GetSectionX(num20) - 2;
							num21 = Netplay.GetSectionY(num21) - 1;
							num29 = num20 + 5;
							num30 = num21 + 3;
							if (num20 < 0)
							{
								num20 = 0;
							}
							if (num29 >= LoadWorld.FakeMain.maxSectionsX)
							{
								num29 = LoadWorld.FakeMain.maxSectionsX - 1;
							}
							if (num21 < 0)
							{
								num21 = 0;
							}
							if (num30 >= LoadWorld.FakeMain.maxSectionsY)
							{
								num30 = LoadWorld.FakeMain.maxSectionsY - 1;
							}
							for (int num31 = num20; num31 < num29; num31++)
							{
								for (int num32 = num21; num32 < num30; num32++)
								{
									if (num31 < num22 || num31 >= num24 || num32 < num23 || num32 >= num25)
									{
										list.Add(new Point(num31, num32));
										num26++;
									}
								}
							}
						}
						int num33 = 1;
						List<Point> list2;
						List<Point> list3;
						PortalHelper.SyncPortalsOnPlayerJoin(this.whoAmI, 1, list, out list2, out list3);
						num26 += list2.Count;
						if (Netplay.Clients[this.whoAmI].State == 2)
						{
							Netplay.Clients[this.whoAmI].State = 3;
						}
						NetMessage.TrySendData(9, this.whoAmI, -1, Lang.inter[44].ToNetworkText(), num26, 0f, 0f, 0f, 0, 0, 0);
						Netplay.Clients[this.whoAmI].StatusText2 = Language.GetTextValue("Net.IsReceivingTileData");
						Netplay.Clients[this.whoAmI].StatusMax += num26;
						for (int num34 = num22; num34 < num24; num34++)
						{
							for (int num35 = num23; num35 < num25; num35++)
							{
								NetMessage.SendSection(this.whoAmI, num34, num35, false);
							}
						}
						NetMessage.TrySendData(11, this.whoAmI, -1, null, num22, (float)num23, (float)(num24 - 1), (float)(num25 - 1), 0, 0, 0);
						if (flag5)
						{
							for (int num36 = num20; num36 < num29; num36++)
							{
								for (int num37 = num21; num37 < num30; num37++)
								{
									NetMessage.SendSection(this.whoAmI, num36, num37, true);
								}
							}
							NetMessage.TrySendData(11, this.whoAmI, -1, null, num20, (float)num21, (float)(num29 - 1), (float)(num30 - 1), 0, 0, 0);
						}
						for (int num38 = 0; num38 < list2.Count; num38++)
						{
							NetMessage.SendSection(this.whoAmI, list2[num38].X, list2[num38].Y, true);
						}
						for (int num39 = 0; num39 < list3.Count; num39++)
						{
							NetMessage.TrySendData(11, this.whoAmI, -1, null, list3[num39].X - num33, (float)(list3[num39].Y - num33), (float)(list3[num39].X + num33 + 1), (float)(list3[num39].Y + num33 + 1), 0, 0, 0);
						}
						for (int num40 = 0; num40 < 400; num40++)
						{
							if (LoadWorld.FakeMain.item[num40].active)
							{
								NetMessage.TrySendData(21, this.whoAmI, -1, null, num40, 0f, 0f, 0f, 0, 0, 0);
								NetMessage.TrySendData(22, this.whoAmI, -1, null, num40, 0f, 0f, 0f, 0, 0, 0);
							}
						}
						for (int num41 = 0; num41 < 200; num41++)
						{
							if (LoadWorld.FakeMain.npc[num41].active)
							{
								NetMessage.TrySendData(23, this.whoAmI, -1, null, num41, 0f, 0f, 0f, 0, 0, 0);
							}
						}
						for (int num42 = 0; num42 < 1000; num42++)
						{
							if (LoadWorld.FakeMain.projectile[num42].active && (LoadWorld.FakeMain.projPet[LoadWorld.FakeMain.projectile[num42].type] || LoadWorld.FakeMain.projectile[num42].netImportant))
							{
								NetMessage.TrySendData(27, this.whoAmI, -1, null, num42, 0f, 0f, 0f, 0, 0, 0);
							}
						}
						for (int num43 = 0; num43 < 289; num43++)
						{
							NetMessage.TrySendData(83, this.whoAmI, -1, null, num43, 0f, 0f, 0f, 0, 0, 0);
						}
						NetMessage.TrySendData(49, this.whoAmI, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
						NetMessage.TrySendData(57, this.whoAmI, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
						NetMessage.TrySendData(7, this.whoAmI, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
						NetMessage.TrySendData(103, -1, -1, null, NPC.MoonLordCountdown, 0f, 0f, 0f, 0, 0, 0);
						NetMessage.TrySendData(101, this.whoAmI, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
						NetMessage.TrySendData(136, this.whoAmI, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
						LoadWorld.FakeMain.BestiaryTracker.OnPlayerJoining(this.whoAmI);
						CreativePowerManager.Instance.SyncThingsToJoiningPlayer(this.whoAmI);
						LoadWorld.FakeMain.PylonSystem.OnPlayerJoining(this.whoAmI);
						return;
					}
				case 9:
					if (LoadWorld.FakeMain.netMode != 1)
					{
						return;
					}
					Netplay.Connection.StatusMax += this.reader.ReadInt32();
					Netplay.Connection.StatusText = NetworkText.Deserialize(this.reader).ToString();
					Netplay.Connection.StatusTextFlags = this.reader.ReadByte();
					return;
				case 10:
					if (LoadWorld.FakeMain.netMode != 1)
					{
						return;
					}
					NetMessage.DecompressTileBlock(this.readBuffer, num, length);
					return;
				case 11:
					if (LoadWorld.FakeMain.netMode != 1)
					{
						return;
					}
					WorldGen.SectionTileFrame((int)this.reader.ReadInt16(), (int)this.reader.ReadInt16(), (int)this.reader.ReadInt16(), (int)this.reader.ReadInt16());
					return;
				case 12:
					{
						int num44 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num44 = this.whoAmI;
						}
						Player player4 = LoadWorld.FakeMain.player[num44];
						player4.SpawnX = (int)this.reader.ReadInt16();
						player4.SpawnY = (int)this.reader.ReadInt16();
						player4.respawnTimer = this.reader.ReadInt32();
						if (player4.respawnTimer > 0)
						{
							player4.dead = true;
						}
						PlayerSpawnContext playerSpawnContext = (PlayerSpawnContext)this.reader.ReadByte();
						player4.Spawn(playerSpawnContext);
						if (num44 == LoadWorld.FakeMain.myPlayer && LoadWorld.FakeMain.netMode != 2)
						{
							LoadWorld.FakeMain.ActivePlayerFileData.StartPlayTimer();
							Player.Hooks.EnterWorld(LoadWorld.FakeMain.myPlayer);
						}
						if (LoadWorld.FakeMain.netMode != 2 || Netplay.Clients[this.whoAmI].State < 3)
						{
							return;
						}
						if (Netplay.Clients[this.whoAmI].State == 3)
						{
							Netplay.Clients[this.whoAmI].State = 10;
							NetMessage.buffer[this.whoAmI].broadcast = true;
							NetMessage.SyncConnectedPlayer(this.whoAmI);
							bool flag6 = NetMessage.DoesPlayerSlotCountAsAHost(this.whoAmI);
							LoadWorld.FakeMain.countsAsHostForGameplay[this.whoAmI] = flag6;
							if (NetMessage.DoesPlayerSlotCountAsAHost(this.whoAmI))
							{
								NetMessage.TrySendData(139, this.whoAmI, -1, null, this.whoAmI, (float)flag6.ToInt(), 0f, 0f, 0, 0, 0);
							}
							NetMessage.TrySendData(12, -1, this.whoAmI, null, this.whoAmI, (float)((byte)playerSpawnContext), 0f, 0f, 0, 0, 0);
							NetMessage.TrySendData(74, this.whoAmI, -1, NetworkText.FromLiteral(LoadWorld.FakeMain.player[this.whoAmI].name), LoadWorld.FakeMain.anglerQuest, 0f, 0f, 0f, 0, 0, 0);
							NetMessage.TrySendData(129, this.whoAmI, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
							NetMessage.greetPlayer(this.whoAmI);
							return;
						}
						NetMessage.TrySendData(12, -1, this.whoAmI, null, this.whoAmI, (float)((byte)playerSpawnContext), 0f, 0f, 0, 0, 0);
						return;
					}
				case 13:
					{
						int num45 = (int)this.reader.ReadByte();
						if (num45 == LoadWorld.FakeMain.myPlayer && !LoadWorld.FakeMain.ServerSideCharacter)
						{
							return;
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num45 = this.whoAmI;
						}
						Player player5 = LoadWorld.FakeMain.player[num45];
						BitsByte bitsByte12 = this.reader.ReadByte();
						BitsByte bitsByte13 = this.reader.ReadByte();
						BitsByte bitsByte14 = this.reader.ReadByte();
						BitsByte bitsByte15 = this.reader.ReadByte();
						player5.controlUp = bitsByte12[0];
						player5.controlDown = bitsByte12[1];
						player5.controlLeft = bitsByte12[2];
						player5.controlRight = bitsByte12[3];
						player5.controlJump = bitsByte12[4];
						player5.controlUseItem = bitsByte12[5];
						player5.direction = (bitsByte12[6] ? 1 : -1);
						if (bitsByte13[0])
						{
							player5.pulley = true;
							player5.pulleyDir = (byte)(bitsByte13[1] ? 2 : 1);
						}
						else
						{
							player5.pulley = false;
						}
						player5.vortexStealthActive = bitsByte13[3];
						player5.gravDir = (float)(bitsByte13[4] ? 1 : -1);
						player5.TryTogglingShield(bitsByte13[5]);
						player5.ghost = bitsByte13[6];
						player5.selectedItem = (int)this.reader.ReadByte();
						player5.position = this.reader.ReadVector2();
						if (bitsByte13[2])
						{
							player5.velocity = this.reader.ReadVector2();
						}
						else
						{
							player5.velocity = Vector2.Zero;
						}
						if (bitsByte14[6])
						{
							player5.PotionOfReturnOriginalUsePosition = new Vector2?(this.reader.ReadVector2());
							player5.PotionOfReturnHomePosition = new Vector2?(this.reader.ReadVector2());
						}
						else
						{
							player5.PotionOfReturnOriginalUsePosition = null;
							player5.PotionOfReturnHomePosition = null;
						}
						player5.tryKeepingHoveringUp = bitsByte14[0];
						player5.IsVoidVaultEnabled = bitsByte14[1];
						player5.sitting.isSitting = bitsByte14[2];
						player5.downedDD2EventAnyDifficulty = bitsByte14[3];
						player5.isPettingAnimal = bitsByte14[4];
						player5.isTheAnimalBeingPetSmall = bitsByte14[5];
						player5.tryKeepingHoveringDown = bitsByte14[7];
						player5.sleeping.SetIsSleepingAndAdjustPlayerRotation(player5, bitsByte15[0]);
						if (LoadWorld.FakeMain.netMode == 2 && Netplay.Clients[this.whoAmI].State == 10)
						{
							NetMessage.TrySendData(13, -1, this.whoAmI, null, num45, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 14:
					{
						int num46 = (int)this.reader.ReadByte();
						int num47 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						bool active = LoadWorld.FakeMain.player[num46].active;
						if (num47 == 1)
						{
							if (!LoadWorld.FakeMain.player[num46].active)
							{
								LoadWorld.FakeMain.player[num46] = new Player();
							}
							LoadWorld.FakeMain.player[num46].active = true;
						}
						else
						{
							LoadWorld.FakeMain.player[num46].active = false;
						}
						if (active == LoadWorld.FakeMain.player[num46].active)
						{
							return;
						}
						if (LoadWorld.FakeMain.player[num46].active)
						{
							Player.Hooks.PlayerConnect(num46);
							return;
						}
						Player.Hooks.PlayerDisconnect(num46);
						return;
					}
				case 15:
				case 25:
				case 26:
				case 44:
				case 67:
				case 93:
					return;
				case 16:
					{
						int num48 = (int)this.reader.ReadByte();
						if (num48 == LoadWorld.FakeMain.myPlayer && !LoadWorld.FakeMain.ServerSideCharacter)
						{
							return;
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num48 = this.whoAmI;
						}
						Player player6 = LoadWorld.FakeMain.player[num48];
						player6.statLife = (int)this.reader.ReadInt16();
						player6.statLifeMax = (int)this.reader.ReadInt16();
						if (player6.statLifeMax < 100)
						{
							player6.statLifeMax = 100;
						}
						player6.dead = (player6.statLife <= 0);
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(16, -1, this.whoAmI, null, num48, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 17:
					{
						byte b3 = this.reader.ReadByte();
						int num49 = (int)this.reader.ReadInt16();
						int num50 = (int)this.reader.ReadInt16();
						short num51 = this.reader.ReadInt16();
						int num52 = (int)this.reader.ReadByte();
						bool flag7 = num51 == 1;
						if (!WorldGen.InWorld(num49, num50, 3))
						{
							return;
						}
						if (LoadWorld.FakeMain.tile[num49, num50] == null)
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
							LoadWorld.FakeMain.tile[num49, num50] = itile;
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							if (!flag7)
							{
								if (b3 == 0 || b3 == 2 || b3 == 4)
								{
									Netplay.Clients[this.whoAmI].SpamDeleteBlock += 1f;
								}
								if (b3 == 1 || b3 == 3)
								{
									Netplay.Clients[this.whoAmI].SpamAddBlock += 1f;
								}
							}
							if (!Netplay.Clients[this.whoAmI].TileSections[Netplay.GetSectionX(num49), Netplay.GetSectionY(num50)])
							{
								flag7 = true;
							}
						}
						if (b3 == 0)
						{
							WorldGen.KillTile(num49, num50, flag7, false, false);
							if (LoadWorld.FakeMain.netMode == 1 && !flag7)
							{
								HitTile.ClearAllTilesAtThisLocation(num49, num50);
							}
						}
						if (b3 == 1)
						{
							WorldGen.PlaceTile(num49, num50, (int)num51, false, true, -1, num52);
						}
						if (b3 == 2)
						{
							WorldGen.KillWall(num49, num50, flag7);
						}
						if (b3 == 3)
						{
							WorldGen.PlaceWall(num49, num50, (int)num51, false);
						}
						if (b3 == 4)
						{
							WorldGen.KillTile(num49, num50, flag7, false, true);
						}
						if (b3 == 5)
						{
							WorldGen.PlaceWire(num49, num50);
						}
						if (b3 == 6)
						{
							WorldGen.KillWire(num49, num50);
						}
						if (b3 == 7)
						{
							WorldGen.PoundTile(num49, num50);
						}
						if (b3 == 8)
						{
							WorldGen.PlaceActuator(num49, num50);
						}
						if (b3 == 9)
						{
							WorldGen.KillActuator(num49, num50);
						}
						if (b3 == 10)
						{
							WorldGen.PlaceWire2(num49, num50);
						}
						if (b3 == 11)
						{
							WorldGen.KillWire2(num49, num50);
						}
						if (b3 == 12)
						{
							WorldGen.PlaceWire3(num49, num50);
						}
						if (b3 == 13)
						{
							WorldGen.KillWire3(num49, num50);
						}
						if (b3 == 14)
						{
							WorldGen.SlopeTile(num49, num50, (int)num51, false);
						}
						if (b3 == 15)
						{
							Minecart.FrameTrack(num49, num50, true, false);
						}
						if (b3 == 16)
						{
							WorldGen.PlaceWire4(num49, num50);
						}
						if (b3 == 17)
						{
							WorldGen.KillWire4(num49, num50);
						}
						if (b3 == 18)
						{
							Wiring.SetCurrentUser(this.whoAmI);
							Wiring.PokeLogicGate(num49, num50);
							Wiring.SetCurrentUser(-1);
							return;
						}
						if (b3 == 19)
						{
							Wiring.SetCurrentUser(this.whoAmI);
							Wiring.Actuate(num49, num50);
							Wiring.SetCurrentUser(-1);
							return;
						}
						if (b3 == 20)
						{
							if (!WorldGen.InWorld(num49, num50, 2))
							{
								return;
							}
							int type3 = (int)LoadWorld.FakeMain.tile[num49, num50].type;
							WorldGen.KillTile(num49, num50, flag7, false, false);
							num51 = (short)(((int)LoadWorld.FakeMain.tile[num49, num50].type == type3) ? 1 : 0);
							if (LoadWorld.FakeMain.netMode == 2)
							{
								NetMessage.TrySendData(17, -1, -1, null, (int)b3, (float)num49, (float)num50, (float)num51, num52, 0, 0);
								return;
							}
							return;
						}
						else
						{
							if (b3 == 21)
							{
								WorldGen.ReplaceTile(num49, num50, (ushort)num51, num52);
							}
							if (b3 == 22)
							{
								WorldGen.ReplaceWall(num49, num50, (ushort)num51);
							}
							if (b3 == 23)
							{
								WorldGen.SlopeTile(num49, num50, (int)num51, false);
								WorldGen.PoundTile(num49, num50);
							}
							if (LoadWorld.FakeMain.netMode != 2)
							{
								return;
							}
							NetMessage.TrySendData(17, -1, this.whoAmI, null, (int)b3, (float)num49, (float)num50, (float)num51, num52, 0, 0);
							if ((b3 == 1 || b3 == 21) && TileID.Sets.Falling[(int)num51])
							{
								NetMessage.SendTileSquare(-1, num49, num50, 1, TileChangeType.None);
								return;
							}
							return;
						}
						break;
					}
				case 18:
					if (LoadWorld.FakeMain.netMode != 1)
					{
						return;
					}
					LoadWorld.FakeMain.dayTime = (this.reader.ReadByte() == 1);
					LoadWorld.FakeMain.time = (double)this.reader.ReadInt32();
					LoadWorld.FakeMain.sunModY = this.reader.ReadInt16();
					LoadWorld.FakeMain.moonModY = this.reader.ReadInt16();
					return;
				case 19:
					{
						byte b4 = this.reader.ReadByte();
						int num53 = (int)this.reader.ReadInt16();
						int num54 = (int)this.reader.ReadInt16();
						if (!WorldGen.InWorld(num53, num54, 3))
						{
							return;
						}
						int num55 = (this.reader.ReadByte() == 0) ? -1 : 1;
						if (b4 == 0)
						{
							WorldGen.OpenDoor(num53, num54, num55);
						}
						else if (b4 == 1)
						{
							WorldGen.CloseDoor(num53, num54, true);
						}
						else if (b4 == 2)
						{
							WorldGen.ShiftTrapdoor(num53, num54, num55 == 1, 1);
						}
						else if (b4 == 3)
						{
							WorldGen.ShiftTrapdoor(num53, num54, num55 == 1, 0);
						}
						else if (b4 == 4)
						{
							WorldGen.ShiftTallGate(num53, num54, false, true);
						}
						else if (b4 == 5)
						{
							WorldGen.ShiftTallGate(num53, num54, true, true);
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(19, -1, this.whoAmI, null, (int)b4, (float)num53, (float)num54, (float)((num55 == 1) ? 1 : 0), 0, 0, 0);
							return;
						}
						return;
					}
				case 20:
					{
						ushort num56 = this.reader.ReadUInt16();
						short num57 = (short)(num56 & 32767);
						bool flag8 = (num56 & 32768) > 0;
						byte b5 = 0;
						if (flag8)
						{
							b5 = this.reader.ReadByte();
						}
						int num58 = (int)this.reader.ReadInt16();
						int num59 = (int)this.reader.ReadInt16();
						if (!WorldGen.InWorld(num58, num59, 3))
						{
							return;
						}
						TileChangeType type4 = TileChangeType.None;
						if (Enum.IsDefined(typeof(TileChangeType), b5))
						{
							type4 = (TileChangeType)b5;
						}
						if (MessageBuffer.OnTileChangeReceived != null)
						{
							MessageBuffer.OnTileChangeReceived(num58, num59, (int)num57, type4);
						}
						BitsByte bitsByte16 = 0;
						BitsByte bitsByte17 = 0;
						for (int num60 = num58; num60 < num58 + (int)num57; num60++)
						{
							for (int num61 = num59; num61 < num59 + (int)num57; num61++)
							{
								if (LoadWorld.FakeMain.tile[num60, num61] == null)
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
									LoadWorld.FakeMain.tile[num60, num61] = itile;
								}
								ITile tile = LoadWorld.FakeMain.tile[num60, num61];
								bool flag9 = tile.active();
								bitsByte16 = this.reader.ReadByte();
								bitsByte17 = this.reader.ReadByte();
								tile.active(bitsByte16[0]);
								tile.wall = (ushort)(bitsByte16[2] ? 1 : 0);
								bool flag10 = bitsByte16[3];
								if (LoadWorld.FakeMain.netMode != 2)
								{
									tile.liquid = (byte)(flag10 ? 1 : 0);
								}
								tile.wire(bitsByte16[4]);
								tile.halfBrick(bitsByte16[5]);
								tile.actuator(bitsByte16[6]);
								tile.inActive(bitsByte16[7]);
								tile.wire2(bitsByte17[0]);
								tile.wire3(bitsByte17[1]);
								if (bitsByte17[2])
								{
									tile.color(this.reader.ReadByte());
								}
								if (bitsByte17[3])
								{
									tile.wallColor(this.reader.ReadByte());
								}
								if (tile.active())
								{
									int type5 = (int)tile.type;
									tile.type = this.reader.ReadUInt16();
									if (LoadWorld.FakeMain.tileFrameImportant[(int)tile.type])
									{
										tile.frameX = this.reader.ReadInt16();
										tile.frameY = this.reader.ReadInt16();
									}
									else if (!flag9 || (int)tile.type != type5)
									{
										tile.frameX = -1;
										tile.frameY = -1;
									}
									byte b6 = 0;
									if (bitsByte17[4])
									{
										b6 += 1;
									}
									if (bitsByte17[5])
									{
										b6 += 2;
									}
									if (bitsByte17[6])
									{
										b6 += 4;
									}
									tile.slope(b6);
								}
								tile.wire4(bitsByte17[7]);
								if (tile.wall > 0)
								{
									tile.wall = this.reader.ReadUInt16();
								}
								if (flag10)
								{
									tile.liquid = this.reader.ReadByte();
									tile.liquidType((int)this.reader.ReadByte());
								}
							}
						}
						WorldGen.RangeFrame(num58, num59, num58 + (int)num57, num59 + (int)num57);
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData((int)b, -1, this.whoAmI, null, (int)num57, (float)num58, (float)num59, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 21:
				case 90:
					{
						int num62 = (int)this.reader.ReadInt16();
						Vector2 vector = this.reader.ReadVector2();
						Vector2 velocity = this.reader.ReadVector2();
						int stack3 = (int)this.reader.ReadInt16();
						int pre = (int)this.reader.ReadByte();
						int num63 = (int)this.reader.ReadByte();
						int num64 = (int)this.reader.ReadInt16();
						if (LoadWorld.FakeMain.netMode == 1)
						{
							if (num64 == 0)
							{
								LoadWorld.FakeMain.item[num62].active = false;
								return;
							}
							int num65 = num62;
							Item item = LoadWorld.FakeMain.item[num65];
							ItemSyncPersistentStats itemSyncPersistentStats = default(ItemSyncPersistentStats);
							itemSyncPersistentStats.CopyFrom(item);
							bool newAndShiny = (item.newAndShiny || item.netID != num64) && ItemSlot.Options.HighlightNewItems && (num64 < 0 || num64 >= 5045 || !ItemID.Sets.NeverAppearsAsNewInInventory[num64]);
							item.netDefaults(num64);
							item.newAndShiny = newAndShiny;
							item.Prefix(pre);
							item.stack = stack3;
							item.position = vector;
							item.velocity = velocity;
							item.active = true;
							if (b == 90)
							{
								item.instanced = true;
								item.playerIndexTheItemIsReservedFor = LoadWorld.FakeMain.myPlayer;
								item.keepTime = 600;
							}
							item.wet = Collision.WetCollision(item.position, item.width, item.height);
							itemSyncPersistentStats.PasteInto(item);
							return;
						}
						else
						{
							if (LoadWorld.FakeMain.timeItemSlotCannotBeReusedFor[num62] > 0)
							{
								return;
							}
							if (num64 == 0)
							{
								if (num62 < 400)
								{
									LoadWorld.FakeMain.item[num62].active = false;
									NetMessage.TrySendData(21, -1, -1, null, num62, 0f, 0f, 0f, 0, 0, 0);
									return;
								}
								return;
							}
							else
							{
								bool flag11 = false;
								if (num62 == 400)
								{
									flag11 = true;
								}
								if (flag11)
								{
									Item item2 = new Item();
									item2.netDefaults(num64);
									num62 = Item.NewItem((int)vector.X, (int)vector.Y, item2.width, item2.height, item2.type, stack3, true, 0, false, false);
								}
								Item item3 = LoadWorld.FakeMain.item[num62];
								item3.netDefaults(num64);
								item3.Prefix(pre);
								item3.stack = stack3;
								item3.position = vector;
								item3.velocity = velocity;
								item3.active = true;
								item3.playerIndexTheItemIsReservedFor = LoadWorld.FakeMain.myPlayer;
								if (flag11)
								{
									NetMessage.TrySendData(21, -1, -1, null, num62, 0f, 0f, 0f, 0, 0, 0);
									if (num63 == 0)
									{
										LoadWorld.FakeMain.item[num62].ownIgnore = this.whoAmI;
										LoadWorld.FakeMain.item[num62].ownTime = 100;
									}
									LoadWorld.FakeMain.item[num62].FindOwner(num62);
									return;
								}
								NetMessage.TrySendData(21, -1, this.whoAmI, null, num62, 0f, 0f, 0f, 0, 0, 0);
								return;
							}
						}
						break;
					}
				case 22:
					{
						int num66 = (int)this.reader.ReadInt16();
						int num67 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2 && LoadWorld.FakeMain.item[num66].playerIndexTheItemIsReservedFor != this.whoAmI)
						{
							return;
						}
						LoadWorld.FakeMain.item[num66].playerIndexTheItemIsReservedFor = num67;
						if (num67 == LoadWorld.FakeMain.myPlayer)
						{
							LoadWorld.FakeMain.item[num66].keepTime = 15;
						}
						else
						{
							LoadWorld.FakeMain.item[num66].keepTime = 0;
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							LoadWorld.FakeMain.item[num66].playerIndexTheItemIsReservedFor = 255;
							LoadWorld.FakeMain.item[num66].keepTime = 15;
							NetMessage.TrySendData(22, -1, -1, null, num66, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 23:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						int num68 = (int)this.reader.ReadInt16();
						Vector2 vector2 = this.reader.ReadVector2();
						Vector2 velocity2 = this.reader.ReadVector2();
						int num69 = (int)this.reader.ReadUInt16();
						if (num69 == 65535)
						{
							num69 = 0;
						}
						BitsByte bitsByte18 = this.reader.ReadByte();
						BitsByte bitsByte19 = this.reader.ReadByte();
						float[] array3 = new float[NPC.maxAI];
						for (int num70 = 0; num70 < NPC.maxAI; num70++)
						{
							if (bitsByte18[num70 + 2])
							{
								array3[num70] = this.reader.ReadSingle();
							}
							else
							{
								array3[num70] = 0f;
							}
						}
						int num71 = (int)this.reader.ReadInt16();
						int? playerCountForMultiplayerDifficultyOverride = new int?(1);
						if (bitsByte19[0])
						{
							playerCountForMultiplayerDifficultyOverride = new int?((int)this.reader.ReadByte());
						}
						float value = 1f;
						if (bitsByte19[2])
						{
							value = this.reader.ReadSingle();
						}
						int num72 = 0;
						if (!bitsByte18[7])
						{
							byte b7 = this.reader.ReadByte();
							if (b7 == 2)
							{
								num72 = (int)this.reader.ReadInt16();
							}
							else if (b7 == 4)
							{
								num72 = this.reader.ReadInt32();
							}
							else
							{
								num72 = (int)this.reader.ReadSByte();
							}
						}
						int num73 = -1;
						NPC npc = LoadWorld.FakeMain.npc[num68];
						if (npc.active && LoadWorld.FakeMain.multiplayerNPCSmoothingRange > 0 && Vector2.DistanceSquared(npc.position, vector2) < 640000f)
						{
							npc.netOffset += npc.position - vector2;
						}
						if (!npc.active || npc.netID != num71)
						{
							npc.netOffset *= 0f;
							if (npc.active)
							{
								num73 = npc.type;
							}
							npc.active = true;
							npc.SetDefaults(num71, new NPCSpawnParams
							{
								playerCountForMultiplayerDifficultyOverride = playerCountForMultiplayerDifficultyOverride,
								strengthMultiplierOverride = new float?(value)
							});
						}
						npc.position = vector2;
						npc.velocity = velocity2;
						npc.target = num69;
						npc.direction = (bitsByte18[0] ? 1 : -1);
						npc.directionY = (bitsByte18[1] ? 1 : -1);
						npc.spriteDirection = (bitsByte18[6] ? 1 : -1);
						if (bitsByte18[7])
						{
							num72 = (npc.life = npc.lifeMax);
						}
						else
						{
							npc.life = num72;
						}
						if (num72 <= 0)
						{
							npc.active = false;
						}
						npc.SpawnedFromStatue = bitsByte19[0];
						if (npc.SpawnedFromStatue)
						{
							npc.value = 0f;
						}
						for (int num74 = 0; num74 < NPC.maxAI; num74++)
						{
							npc.ai[num74] = array3[num74];
						}
						if (num73 > -1 && num73 != npc.type)
						{
							npc.TransformVisuals(num73, npc.type);
						}
						if (num71 == 262)
						{
							NPC.plantBoss = num68;
						}
						if (num71 == 245)
						{
							NPC.golemBoss = num68;
						}
						if (npc.type >= 0 && npc.type < 663 && LoadWorld.FakeMain.npcCatchable[npc.type])
						{
							npc.releaseOwner = (short)this.reader.ReadByte();
							return;
						}
						return;
					}
				case 24:
					{
						int num75 = (int)this.reader.ReadInt16();
						int num76 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num76 = this.whoAmI;
						}
						Player player7 = LoadWorld.FakeMain.player[num76];
						LoadWorld.FakeMain.npc[num75].StrikeNPC(player7.inventory[player7.selectedItem].damage, player7.inventory[player7.selectedItem].knockBack, player7.direction, false, false, false, LoadWorld.FakeMain.player[this.whoAmI]);
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(24, -1, this.whoAmI, null, num75, (float)num76, 0f, 0f, 0, 0, 0);
							NetMessage.TrySendData(23, -1, -1, null, num75, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 27:
					{
						int num77 = (int)this.reader.ReadInt16();
						Vector2 position = this.reader.ReadVector2();
						Vector2 velocity3 = this.reader.ReadVector2();
						int num78 = (int)this.reader.ReadByte();
						int num79 = (int)this.reader.ReadInt16();
						BitsByte bitsByte20 = this.reader.ReadByte();
						float[] array4 = new float[Projectile.maxAI];
						for (int num80 = 0; num80 < Projectile.maxAI; num80++)
						{
							if (bitsByte20[num80])
							{
								array4[num80] = this.reader.ReadSingle();
							}
							else
							{
								array4[num80] = 0f;
							}
						}
						int damage = (int)(bitsByte20[4] ? this.reader.ReadInt16() : 0);
						float knockBack = bitsByte20[5] ? this.reader.ReadSingle() : 0f;
						int originalDamage = (int)(bitsByte20[6] ? this.reader.ReadInt16() : 0);
						int num81 = (int)(bitsByte20[7] ? this.reader.ReadInt16() : -1);
						if (num81 >= 1000)
						{
							num81 = -1;
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							if (num79 == 949)
							{
								num78 = 255;
							}
							else
							{
								num78 = this.whoAmI;
								if (LoadWorld.FakeMain.projHostile[num79])
								{
									return;
								}
							}
						}
						int num82 = 1000;
						for (int num83 = 0; num83 < 1000; num83++)
						{
							if (LoadWorld.FakeMain.projectile[num83].owner == num78 && LoadWorld.FakeMain.projectile[num83].identity == num77 && LoadWorld.FakeMain.projectile[num83].active)
							{
								num82 = num83;
								break;
							}
						}
						if (num82 == 1000)
						{
							for (int num84 = 0; num84 < 1000; num84++)
							{
								if (!LoadWorld.FakeMain.projectile[num84].active)
								{
									num82 = num84;
									break;
								}
							}
						}
						if (num82 == 1000)
						{
							num82 = Projectile.FindOldestProjectile();
						}
						Projectile projectile = LoadWorld.FakeMain.projectile[num82];
						if (!projectile.active || projectile.type != num79)
						{
							projectile.SetDefaults(num79);
							if (LoadWorld.FakeMain.netMode == 2)
							{
								Netplay.Clients[this.whoAmI].SpamProjectile += 1f;
							}
						}
						projectile.identity = num77;
						projectile.position = position;
						projectile.velocity = velocity3;
						projectile.type = num79;
						projectile.damage = damage;
						projectile.originalDamage = originalDamage;
						projectile.knockBack = knockBack;
						projectile.owner = num78;
						for (int num85 = 0; num85 < Projectile.maxAI; num85++)
						{
							projectile.ai[num85] = array4[num85];
						}
						if (num81 >= 0)
						{
							projectile.projUUID = num81;
							LoadWorld.FakeMain.projectileIdentity[num78, num81] = num82;
						}
						projectile.ProjectileFixDesperation();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(27, -1, this.whoAmI, null, num82, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 28:
					{
						int num86 = (int)this.reader.ReadInt16();
						int num87 = (int)this.reader.ReadInt16();
						float num88 = this.reader.ReadSingle();
						int num89 = (int)(this.reader.ReadByte() - 1);
						byte b8 = this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							if (num87 < 0)
							{
								num87 = 0;
							}
							LoadWorld.FakeMain.npc[num86].PlayerInteraction(this.whoAmI);
						}
						if (num87 >= 0)
						{
							LoadWorld.FakeMain.npc[num86].StrikeNPC(num87, num88, num89, b8 == 1, false, true, LoadWorld.FakeMain.player[this.whoAmI]);
						}
						else
						{
							LoadWorld.FakeMain.npc[num86].life = 0;
							LoadWorld.FakeMain.npc[num86].HitEffect(0, 10.0);
							LoadWorld.FakeMain.npc[num86].active = false;
						}
						if (LoadWorld.FakeMain.netMode != 2)
						{
							return;
						}
						NetMessage.TrySendData(28, -1, this.whoAmI, null, num86, (float)num87, num88, (float)num89, (int)b8, 0, 0);
						if (LoadWorld.FakeMain.npc[num86].life <= 0)
						{
							NetMessage.TrySendData(23, -1, -1, null, num86, 0f, 0f, 0f, 0, 0, 0);
						}
						else
						{
							LoadWorld.FakeMain.npc[num86].netUpdate = true;
						}
						if (LoadWorld.FakeMain.npc[num86].realLife < 0)
						{
							return;
						}
						if (LoadWorld.FakeMain.npc[LoadWorld.FakeMain.npc[num86].realLife].life <= 0)
						{
							NetMessage.TrySendData(23, -1, -1, null, LoadWorld.FakeMain.npc[num86].realLife, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						LoadWorld.FakeMain.npc[LoadWorld.FakeMain.npc[num86].realLife].netUpdate = true;
						return;
					}
				case 29:
					{
						int num90 = (int)this.reader.ReadInt16();
						int num91 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num91 = this.whoAmI;
						}
						for (int num92 = 0; num92 < 1000; num92++)
						{
							if (LoadWorld.FakeMain.projectile[num92].owner == num91 && LoadWorld.FakeMain.projectile[num92].identity == num90 && LoadWorld.FakeMain.projectile[num92].active)
							{
								LoadWorld.FakeMain.projectile[num92].Kill();
								break;
							}
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(29, -1, this.whoAmI, null, num90, (float)num91, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 30:
					{
						int num93 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num93 = this.whoAmI;
						}
						bool flag12 = this.reader.ReadBoolean();
						LoadWorld.FakeMain.player[num93].hostile = flag12;
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(30, -1, this.whoAmI, null, num93, 0f, 0f, 0f, 0, 0, 0);
							LocalizedText localizedText = flag12 ? Lang.mp[11] : Lang.mp[12];
							Color color = LoadWorld.FakeMain.teamColor[LoadWorld.FakeMain.player[num93].team];
							ChatHelper.BroadcastChatMessage(NetworkText.FromKey(localizedText.Key, new object[]
							{
						LoadWorld.FakeMain.player[num93].name
							}), color, -1);
							return;
						}
						return;
					}
				case 31:
					{
						if (LoadWorld.FakeMain.netMode != 2)
						{
							return;
						}
						int x = (int)this.reader.ReadInt16();
						int y = (int)this.reader.ReadInt16();
						int num94 = Chest.FindChest(x, y);
						if (num94 <= -1 || Chest.UsingChest(num94) != -1)
						{
							return;
						}
						for (int num95 = 0; num95 < 40; num95++)
						{
							NetMessage.TrySendData(32, this.whoAmI, -1, null, num94, (float)num95, 0f, 0f, 0, 0, 0);
						}
						NetMessage.TrySendData(33, this.whoAmI, -1, null, num94, 0f, 0f, 0f, 0, 0, 0);
						LoadWorld.FakeMain.player[this.whoAmI].chest = num94;
						if (LoadWorld.FakeMain.myPlayer == this.whoAmI)
						{
							LoadWorld.FakeMain.recBigList = false;
						}
						NetMessage.TrySendData(80, -1, this.whoAmI, null, this.whoAmI, (float)num94, 0f, 0f, 0, 0, 0);
						if (LoadWorld.FakeMain.netMode == 2 && WorldGen.IsChestRigged(x, y))
						{
							Wiring.SetCurrentUser(this.whoAmI);
							var entity = LoadWorld.FakeMain.player[this.whoAmI];
							Hooks.Collision.PressurePlateHandler pressurePlate = Hooks.Collision.PressurePlate;
							bool flag = false;
							if (!flag)
							{
								Player player = entity as Player;
								Wiring.HitSwitch(x, y);
								int msgType = 59;
								int remoteClient = -1;
								int number = x;
								float number2 = (float)y;
								NetMessage.TrySendData(msgType, remoteClient, player.whoAmI, null, number, number2, 0f, 0f, 0, 0, 0);
							}
							Wiring.SetCurrentUser(-1);
							return;
						}
						return;
					}
				case 32:
					{
						int num96 = (int)this.reader.ReadInt16();
						int num97 = (int)this.reader.ReadByte();
						int stack4 = (int)this.reader.ReadInt16();
						int pre2 = (int)this.reader.ReadByte();
						int type6 = (int)this.reader.ReadInt16();
						if (num96 < 0 || num96 >= 8000)
						{
							return;
						}
						if (LoadWorld.FakeMain.chest[num96] == null)
						{
							LoadWorld.FakeMain.chest[num96] = new Chest(false);
						}
						if (LoadWorld.FakeMain.chest[num96].item[num97] == null)
						{
							LoadWorld.FakeMain.chest[num96].item[num97] = new Item();
						}
						LoadWorld.FakeMain.chest[num96].item[num97].netDefaults(type6);
						LoadWorld.FakeMain.chest[num96].item[num97].Prefix(pre2);
						LoadWorld.FakeMain.chest[num96].item[num97].stack = stack4;
						Recipe.FindRecipes(true);
						return;
					}
				case 33:
					{
						int num98 = (int)this.reader.ReadInt16();
						int num99 = (int)this.reader.ReadInt16();
						int num100 = (int)this.reader.ReadInt16();
						int num101 = (int)this.reader.ReadByte();
						string name = string.Empty;
						if (num101 != 0)
						{
							if (num101 <= 20)
							{
								name = this.reader.ReadString();
							}
							else if (num101 != 255)
							{
								num101 = 0;
							}
						}
						if (LoadWorld.FakeMain.netMode != 1)
						{
							if (num101 != 0)
							{
								int chest = LoadWorld.FakeMain.player[this.whoAmI].chest;
								Chest chest2 = LoadWorld.FakeMain.chest[chest];
								chest2.name = name;
								NetMessage.TrySendData(69, -1, this.whoAmI, null, chest, (float)chest2.x, (float)chest2.y, 0f, 0, 0, 0);
							}
							LoadWorld.FakeMain.player[this.whoAmI].chest = num98;
							Recipe.FindRecipes(true);
							NetMessage.TrySendData(80, -1, this.whoAmI, null, this.whoAmI, (float)num98, 0f, 0f, 0, 0, 0);
							return;
						}
						Player player8 = LoadWorld.FakeMain.player[LoadWorld.FakeMain.myPlayer];
						if (player8.chest == -1)
						{
							LoadWorld.FakeMain.playerInventory = true;
							SoundEngine.PlaySound(10, -1, -1, 1, 1f, 0f);
						}
						else if (player8.chest != num98 && num98 != -1)
						{
							LoadWorld.FakeMain.playerInventory = true;
							SoundEngine.PlaySound(12, -1, -1, 1, 1f, 0f);
							LoadWorld.FakeMain.recBigList = false;
						}
						else if (player8.chest != -1 && num98 == -1)
						{
							SoundEngine.PlaySound(11, -1, -1, 1, 1f, 0f);
							LoadWorld.FakeMain.recBigList = false;
						}
						player8.chest = num98;
						player8.chestX = num99;
						player8.chestY = num100;
						Recipe.FindRecipes(true);
						if (LoadWorld.FakeMain.tile[num99, num100].frameX >= 36 && LoadWorld.FakeMain.tile[num99, num100].frameX < 72)
						{
							AchievementsHelper.HandleSpecialEvent(LoadWorld.FakeMain.player[LoadWorld.FakeMain.myPlayer], 16);
							return;
						}
						return;
					}
				case 34:
					{
						byte b9 = this.reader.ReadByte();
						int num102 = (int)this.reader.ReadInt16();
						int num103 = (int)this.reader.ReadInt16();
						int num104 = (int)this.reader.ReadInt16();
						int num105 = (int)this.reader.ReadInt16();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num105 = 0;
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							if (b9 == 0)
							{
								int num106 = WorldGen.PlaceChest(num102, num103, 21, false, num104);
								if (num106 == -1)
								{
									NetMessage.TrySendData(34, this.whoAmI, -1, null, (int)b9, (float)num102, (float)num103, (float)num104, num106, 0, 0);
									Item.NewItem(num102 * 16, num103 * 16, 32, 32, Chest.chestItemSpawn[num104], 1, true, 0, false, false);
									return;
								}
								NetMessage.TrySendData(34, -1, -1, null, (int)b9, (float)num102, (float)num103, (float)num104, num106, 0, 0);
								return;
							}
							else if (b9 == 1 && LoadWorld.FakeMain.tile[num102, num103].type == 21)
							{
								ITile tile2 = LoadWorld.FakeMain.tile[num102, num103];
								if (tile2.frameX % 36 != 0)
								{
									num102--;
								}
								if (tile2.frameY % 36 != 0)
								{
									num103--;
								}
								int number = Chest.FindChest(num102, num103);
								WorldGen.KillTile(num102, num103, false, false, false);
								if (!tile2.active())
								{
									NetMessage.TrySendData(34, -1, -1, null, (int)b9, (float)num102, (float)num103, 0f, number, 0, 0);
									return;
								}
								return;
							}
							else if (b9 == 2)
							{
								int num107 = WorldGen.PlaceChest(num102, num103, 88, false, num104);
								if (num107 == -1)
								{
									NetMessage.TrySendData(34, this.whoAmI, -1, null, (int)b9, (float)num102, (float)num103, (float)num104, num107, 0, 0);
									Item.NewItem(num102 * 16, num103 * 16, 32, 32, Chest.dresserItemSpawn[num104], 1, true, 0, false, false);
									return;
								}
								NetMessage.TrySendData(34, -1, -1, null, (int)b9, (float)num102, (float)num103, (float)num104, num107, 0, 0);
								return;
							}
							else if (b9 == 3 && LoadWorld.FakeMain.tile[num102, num103].type == 88)
							{
								ITile tile3 = LoadWorld.FakeMain.tile[num102, num103];
								num102 -= (int)(tile3.frameX % 54 / 18);
								if (tile3.frameY % 36 != 0)
								{
									num103--;
								}
								int number2 = Chest.FindChest(num102, num103);
								WorldGen.KillTile(num102, num103, false, false, false);
								if (!tile3.active())
								{
									NetMessage.TrySendData(34, -1, -1, null, (int)b9, (float)num102, (float)num103, 0f, number2, 0, 0);
									return;
								}
								return;
							}
							else if (b9 == 4)
							{
								int num108 = WorldGen.PlaceChest(num102, num103, 467, false, num104);
								if (num108 == -1)
								{
									NetMessage.TrySendData(34, this.whoAmI, -1, null, (int)b9, (float)num102, (float)num103, (float)num104, num108, 0, 0);
									Item.NewItem(num102 * 16, num103 * 16, 32, 32, Chest.chestItemSpawn2[num104], 1, true, 0, false, false);
									return;
								}
								NetMessage.TrySendData(34, -1, -1, null, (int)b9, (float)num102, (float)num103, (float)num104, num108, 0, 0);
								return;
							}
							else
							{
								if (b9 != 5 || LoadWorld.FakeMain.tile[num102, num103].type != 467)
								{
									return;
								}
								ITile tile4 = LoadWorld.FakeMain.tile[num102, num103];
								if (tile4.frameX % 36 != 0)
								{
									num102--;
								}
								if (tile4.frameY % 36 != 0)
								{
									num103--;
								}
								int number3 = Chest.FindChest(num102, num103);
								WorldGen.KillTile(num102, num103, false, false, false);
								if (!tile4.active())
								{
									NetMessage.TrySendData(34, -1, -1, null, (int)b9, (float)num102, (float)num103, 0f, number3, 0, 0);
									return;
								}
								return;
							}
						}
						else if (b9 == 0)
						{
							if (num105 == -1)
							{
								WorldGen.KillTile(num102, num103, false, false, false);
								return;
							}
							SoundEngine.PlaySound(0, num102 * 16, num103 * 16, 1, 1f, 0f);
							WorldGen.PlaceChestDirect(num102, num103, 21, num104, num105);
							return;
						}
						else if (b9 == 2)
						{
							if (num105 == -1)
							{
								WorldGen.KillTile(num102, num103, false, false, false);
								return;
							}
							SoundEngine.PlaySound(0, num102 * 16, num103 * 16, 1, 1f, 0f);
							WorldGen.PlaceDresserDirect(num102, num103, 88, num104, num105);
							return;
						}
						else
						{
							if (b9 != 4)
							{
								Chest.DestroyChestDirect(num102, num103, num105);
								WorldGen.KillTile(num102, num103, false, false, false);
								return;
							}
							if (num105 == -1)
							{
								WorldGen.KillTile(num102, num103, false, false, false);
								return;
							}
							SoundEngine.PlaySound(0, num102 * 16, num103 * 16, 1, 1f, 0f);
							WorldGen.PlaceChestDirect(num102, num103, 467, num104, num105);
							return;
						}
						break;
					}
				case 35:
					{
						int num109 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num109 = this.whoAmI;
						}
						int num110 = (int)this.reader.ReadInt16();
						if (num109 != LoadWorld.FakeMain.myPlayer || LoadWorld.FakeMain.ServerSideCharacter)
						{
							LoadWorld.FakeMain.player[num109].HealEffect(num110, true);
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(35, -1, this.whoAmI, null, num109, (float)num110, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 36:
					{
						int num111 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num111 = this.whoAmI;
						}
						Player player9 = LoadWorld.FakeMain.player[num111];
						player9.zone1 = this.reader.ReadByte();
						player9.zone2 = this.reader.ReadByte();
						player9.zone3 = this.reader.ReadByte();
						player9.zone4 = this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(36, -1, this.whoAmI, null, num111, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 37:
					if (LoadWorld.FakeMain.netMode != 1)
					{
						return;
					}
					if (LoadWorld.FakeMain.autoPass)
					{
						NetMessage.TrySendData(38, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
						LoadWorld.FakeMain.autoPass = false;
						return;
					}
					Netplay.ServerPassword = "";
					LoadWorld.FakeMain.menuMode = 31;
					return;
				case 38:
					if (LoadWorld.FakeMain.netMode != 2)
					{
						return;
					}
					if (this.reader.ReadString() == Netplay.ServerPassword)
					{
						Netplay.Clients[this.whoAmI].State = 1;
						NetMessage.TrySendData(3, this.whoAmI, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
						return;
					}
					NetMessage.TrySendData(2, this.whoAmI, -1, Lang.mp[1].ToNetworkText(), 0, 0f, 0f, 0f, 0, 0, 0);
					return;
				case 39:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						int num112 = (int)this.reader.ReadInt16();
						LoadWorld.FakeMain.item[num112].playerIndexTheItemIsReservedFor = 255;
						NetMessage.TrySendData(22, -1, -1, null, num112, 0f, 0f, 0f, 0, 0, 0);
						return;
					}
				case 40:
					{
						int num113 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num113 = this.whoAmI;
						}
						int npcIndex = (int)this.reader.ReadInt16();
						LoadWorld.FakeMain.player[num113].SetTalkNPC(npcIndex, true);
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(40, -1, this.whoAmI, null, num113, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 41:
					{
						int num114 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num114 = this.whoAmI;
						}
						Player player10 = LoadWorld.FakeMain.player[num114];
						float itemRotation = this.reader.ReadSingle();
						int itemAnimation = (int)this.reader.ReadInt16();
						player10.itemRotation = itemRotation;
						player10.itemAnimation = itemAnimation;
						player10.channel = player10.inventory[player10.selectedItem].channel;
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(41, -1, this.whoAmI, null, num114, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 42:
					{
						int num115 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num115 = this.whoAmI;
						}
						else if (LoadWorld.FakeMain.myPlayer == num115 && !LoadWorld.FakeMain.ServerSideCharacter)
						{
							return;
						}
						int statMana = (int)this.reader.ReadInt16();
						int statManaMax = (int)this.reader.ReadInt16();
						LoadWorld.FakeMain.player[num115].statMana = statMana;
						LoadWorld.FakeMain.player[num115].statManaMax = statManaMax;
						return;
					}
				case 43:
					{
						int num116 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num116 = this.whoAmI;
						}
						int num117 = (int)this.reader.ReadInt16();
						if (num116 != LoadWorld.FakeMain.myPlayer)
						{
							LoadWorld.FakeMain.player[num116].ManaEffect(num117);
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(43, -1, this.whoAmI, null, num116, (float)num117, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 45:
					{
						int num118 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num118 = this.whoAmI;
						}
						int num119 = (int)this.reader.ReadByte();
						Player player11 = LoadWorld.FakeMain.player[num118];
						int team = player11.team;
						player11.team = num119;
						Color color2 = LoadWorld.FakeMain.teamColor[num119];
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(45, -1, this.whoAmI, null, num118, 0f, 0f, 0f, 0, 0, 0);
							LocalizedText localizedText2 = Lang.mp[13 + num119];
							if (num119 == 5)
							{
								localizedText2 = Lang.mp[22];
							}
							for (int num120 = 0; num120 < 255; num120++)
							{
								if (num120 == this.whoAmI || (team > 0 && LoadWorld.FakeMain.player[num120].team == team) || (num119 > 0 && LoadWorld.FakeMain.player[num120].team == num119))
								{
									ChatHelper.SendChatMessageToClient(NetworkText.FromKey(localizedText2.Key, new object[]
									{
								player11.name
									}), color2, num120);
								}
							}
							return;
						}
						return;
					}
				case 46:
					{
						if (LoadWorld.FakeMain.netMode != 2)
						{
							return;
						}
						int i2 = (int)this.reader.ReadInt16();
						int j2 = (int)this.reader.ReadInt16();
						int num121 = Sign.ReadSign(i2, j2, true);
						if (num121 >= 0)
						{
							NetMessage.TrySendData(47, this.whoAmI, -1, null, num121, (float)this.whoAmI, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 47:
					{
						int num122 = (int)this.reader.ReadInt16();
						int x2 = (int)this.reader.ReadInt16();
						int y2 = (int)this.reader.ReadInt16();
						string text = this.reader.ReadString();
						int num123 = (int)this.reader.ReadByte();
						BitsByte bitsByte21 = this.reader.ReadByte();
						if (num122 < 0 || num122 >= 1000)
						{
							return;
						}
						string a = null;
						if (LoadWorld.FakeMain.sign[num122] != null)
						{
							a = LoadWorld.FakeMain.sign[num122].text;
						}
						LoadWorld.FakeMain.sign[num122] = new Sign();
						LoadWorld.FakeMain.sign[num122].x = x2;
						LoadWorld.FakeMain.sign[num122].y = y2;
						Sign.TextSign(num122, text);
						if (LoadWorld.FakeMain.netMode == 2 && a != text)
						{
							num123 = this.whoAmI;
							NetMessage.TrySendData(47, -1, this.whoAmI, null, num122, (float)num123, 0f, 0f, 0, 0, 0);
						}
						if (LoadWorld.FakeMain.netMode == 1 && num123 == LoadWorld.FakeMain.myPlayer && LoadWorld.FakeMain.sign[num122] != null && !bitsByte21[0])
						{
							LoadWorld.FakeMain.playerInventory = false;
							LoadWorld.FakeMain.player[LoadWorld.FakeMain.myPlayer].SetTalkNPC(-1, true);
							LoadWorld.FakeMain.npcChatCornerItem = 0;
							LoadWorld.FakeMain.editSign = false;
							SoundEngine.PlaySound(10, -1, -1, 1, 1f, 0f);
							LoadWorld.FakeMain.player[LoadWorld.FakeMain.myPlayer].sign = num122;
							LoadWorld.FakeMain.npcChatText = LoadWorld.FakeMain.sign[num122].text;
							return;
						}
						return;
					}
				case 48:
					{
						int num124 = (int)this.reader.ReadInt16();
						int num125 = (int)this.reader.ReadInt16();
						byte liquid = this.reader.ReadByte();
						byte liquidType = this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2 && Netplay.SpamCheck)
						{
							int num126 = this.whoAmI;
							int num127 = (int)(LoadWorld.FakeMain.player[num126].position.X + (float)(LoadWorld.FakeMain.player[num126].width / 2));
							int num128 = (int)(LoadWorld.FakeMain.player[num126].position.Y + (float)(LoadWorld.FakeMain.player[num126].height / 2));
							int num129 = 10;
							int num130 = num127 - num129;
							int num131 = num127 + num129;
							int num132 = num128 - num129;
							int num133 = num128 + num129;
							if (num124 < num130 || num124 > num131 || num125 < num132 || num125 > num133)
							{
								NetMessage.BootPlayer(this.whoAmI, NetworkText.FromKey("Net.CheatingLiquidSpam", new object[0]));
								return;
							}
						}
						if (LoadWorld.FakeMain.tile[num124, num125] == null)
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
							LoadWorld.FakeMain.tile[num124, num125] = itile;
						}
						ITile obj2 = LoadWorld.FakeMain.tile[num124, num125];
						lock (obj2)
						{
							LoadWorld.FakeMain.tile[num124, num125].liquid = liquid;
							LoadWorld.FakeMain.tile[num124, num125].liquidType((int)liquidType);
							if (LoadWorld.FakeMain.netMode == 2)
							{
								WorldGen.SquareTileFrame(num124, num125, true);
							}
							return;
						}
						goto IL_51B2;
					}
				case 49:
					goto IL_51B2;
				case 50:
					{
						int num134 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num134 = this.whoAmI;
						}
						else if (num134 == LoadWorld.FakeMain.myPlayer && !LoadWorld.FakeMain.ServerSideCharacter)
						{
							return;
						}
						Player player12 = LoadWorld.FakeMain.player[num134];
						for (int num135 = 0; num135 < 22; num135++)
						{
							player12.buffType[num135] = (int)this.reader.ReadUInt16();
							if (player12.buffType[num135] > 0)
							{
								player12.buffTime[num135] = 60;
							}
							else
							{
								player12.buffTime[num135] = 0;
							}
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(50, -1, this.whoAmI, null, num134, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 51:
					{
						byte b10 = this.reader.ReadByte();
						byte b11 = this.reader.ReadByte();
						if (b11 == 1)
						{
							NPC.SpawnSkeletron();
							return;
						}
						if (b11 == 2)
						{
							if (LoadWorld.FakeMain.netMode == 2)
							{
								NetMessage.TrySendData(51, -1, this.whoAmI, null, (int)b10, (float)b11, 0f, 0f, 0, 0, 0);
								return;
							}
							SoundEngine.PlaySound(SoundID.Item1, (int)LoadWorld.FakeMain.player[(int)b10].position.X, (int)LoadWorld.FakeMain.player[(int)b10].position.Y);
							return;
						}
						else if (b11 == 3)
						{
							if (LoadWorld.FakeMain.netMode == 2)
							{
								LoadWorld.FakeMain.Sundialing();
								return;
							}
							return;
						}
						else
						{
							if (b11 == 4)
							{
								LoadWorld.FakeMain.npc[(int)b10].BigMimicSpawnSmoke();
								return;
							}
							return;
						}
						break;
					}
				case 52:
					{
						int num136 = (int)this.reader.ReadByte();
						int num137 = (int)this.reader.ReadInt16();
						int num138 = (int)this.reader.ReadInt16();
						if (num136 == 1)
						{
							Chest.Unlock(num137, num138);
							if (LoadWorld.FakeMain.netMode == 2)
							{
								NetMessage.TrySendData(52, -1, this.whoAmI, null, 0, (float)num136, (float)num137, (float)num138, 0, 0, 0);
								NetMessage.SendTileSquare(-1, num137, num138, 2, TileChangeType.None);
							}
						}
						if (num136 != 2)
						{
							return;
						}
						WorldGen.UnlockDoor(num137, num138);
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(52, -1, this.whoAmI, null, 0, (float)num136, (float)num137, (float)num138, 0, 0, 0);
							NetMessage.SendTileSquare(-1, num137, num138, 2, TileChangeType.None);
							return;
						}
						return;
					}
				case 53:
					{
						int num139 = (int)this.reader.ReadInt16();
						int type7 = (int)this.reader.ReadUInt16();
						int time = (int)this.reader.ReadInt16();
						LoadWorld.FakeMain.npc[num139].AddBuff(type7, time, true);
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(54, -1, -1, null, num139, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 54:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						int num140 = (int)this.reader.ReadInt16();
						NPC npc2 = LoadWorld.FakeMain.npc[num140];
						for (int num141 = 0; num141 < 5; num141++)
						{
							npc2.buffType[num141] = (int)this.reader.ReadUInt16();
							npc2.buffTime[num141] = (int)this.reader.ReadInt16();
						}
						return;
					}
				case 55:
					{
						int num142 = (int)this.reader.ReadByte();
						int num143 = (int)this.reader.ReadUInt16();
						int num144 = this.reader.ReadInt32();
						if (LoadWorld.FakeMain.netMode == 2 && num142 != this.whoAmI && !LoadWorld.FakeMain.pvpBuff[num143])
						{
							return;
						}
						if (LoadWorld.FakeMain.netMode == 1 && num142 == LoadWorld.FakeMain.myPlayer)
						{
							LoadWorld.FakeMain.player[num142].AddBuff(num143, num144, true, false);
							return;
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(55, num142, -1, null, num142, (float)num143, (float)num144, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 56:
					{
						int num145 = (int)this.reader.ReadInt16();
						if (num145 < 0 || num145 >= 200)
						{
							return;
						}
						if (LoadWorld.FakeMain.netMode == 1)
						{
							string givenName = this.reader.ReadString();
							LoadWorld.FakeMain.npc[num145].GivenName = givenName;
							int townNpcVariationIndex = this.reader.ReadInt32();
							LoadWorld.FakeMain.npc[num145].townNpcVariationIndex = townNpcVariationIndex;
							return;
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(56, this.whoAmI, -1, null, num145, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 57:
					if (LoadWorld.FakeMain.netMode != 1)
					{
						return;
					}
					WorldGen.tGood = this.reader.ReadByte();
					WorldGen.tEvil = this.reader.ReadByte();
					WorldGen.tBlood = this.reader.ReadByte();
					return;
				case 58:
					{
						int num146 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num146 = this.whoAmI;
						}
						float num147 = this.reader.ReadSingle();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(58, -1, this.whoAmI, null, this.whoAmI, num147, 0f, 0f, 0, 0, 0);
							return;
						}
						Player player13 = LoadWorld.FakeMain.player[num146];
						int type8 = player13.inventory[player13.selectedItem].type;
						if (type8 == 4372 || type8 == 4057 || type8 == 4715)
						{
							player13.PlayGuitarChord(num147);
							return;
						}
						if (type8 == 4673)
						{
							player13.PlayDrums(num147);
							return;
						}
						LoadWorld.FakeMain.musicPitch = num147;
						LegacySoundStyle type9 = SoundID.Item26;
						if (type8 == 507)
						{
							type9 = SoundID.Item35;
						}
						if (type8 == 1305)
						{
							type9 = SoundID.Item47;
						}
						SoundEngine.PlaySound(type9, player13.position);
						return;
					}
				case 59:
					{
						int x3 = (int)this.reader.ReadInt16();
						int y3 = (int)this.reader.ReadInt16();
						Wiring.SetCurrentUser(this.whoAmI);
						var entity = LoadWorld.FakeMain.player[this.whoAmI];
						Hooks.Collision.PressurePlateHandler pressurePlate = Hooks.Collision.PressurePlate;
						bool flag = false;
						if (!flag)
						{
							Player player = entity as Player;
							Wiring.HitSwitch(x3, y3);
							int msgType = 59;
							int remoteClient = -1;
							int number = x3;
							float number2 = (float)y3;
							NetMessage.TrySendData(msgType, remoteClient, player.whoAmI, null, number, number2, 0f, 0f, 0, 0, 0);
						}
						Wiring.SetCurrentUser(-1);
						return;
					}
				case 60:
					{
						int num148 = (int)this.reader.ReadInt16();
						int num149 = (int)this.reader.ReadInt16();
						int num150 = (int)this.reader.ReadInt16();
						byte b12 = this.reader.ReadByte();
						if (num148 >= 200)
						{
							NetMessage.BootPlayer(this.whoAmI, NetworkText.FromKey("Net.CheatingInvalid", new object[0]));
							return;
						}
						if (LoadWorld.FakeMain.netMode == 1)
						{
							LoadWorld.FakeMain.npc[num148].homeless = (b12 == 1);
							LoadWorld.FakeMain.npc[num148].homeTileX = num149;
							LoadWorld.FakeMain.npc[num148].homeTileY = num150;
							if (b12 == 1)
							{
								WorldGen.TownManager.KickOut(LoadWorld.FakeMain.npc[num148].type);
								return;
							}
							if (b12 == 2)
							{
								WorldGen.TownManager.SetRoom(LoadWorld.FakeMain.npc[num148].type, num149, num150);
								return;
							}
							return;
						}
						else
						{
							if (b12 == 1)
							{
								WorldGen.kickOut(num148);
								return;
							}
							WorldGen.moveRoom(num149, num150, num148);
							return;
						}
						break;
					}
				case 61:
					{
						int plr = (int)this.reader.ReadInt16();
						int num151 = (int)this.reader.ReadInt16();
						if (LoadWorld.FakeMain.netMode != 2)
						{
							return;
						}
						if (num151 >= 0 && num151 < 663 && NPCID.Sets.MPAllowedEnemies[num151])
						{
							if (!NPC.AnyNPCs(num151))
							{
								NPC.SpawnOnPlayer(plr, num151);
								return;
							}
							return;
						}
						else if (num151 == -4)
						{
							if (!LoadWorld.FakeMain.dayTime && !DD2Event.Ongoing)
							{
								ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Lang.misc[31].Key, new object[0]), new Color(50, 255, 130), -1);
								LoadWorld.FakeMain.startPumpkinMoon();
								NetMessage.TrySendData(7, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
								NetMessage.TrySendData(78, -1, -1, null, 0, 1f, 2f, 1f, 0, 0, 0);
								return;
							}
							return;
						}
						else if (num151 == -5)
						{
							if (!LoadWorld.FakeMain.dayTime && !DD2Event.Ongoing)
							{
								ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Lang.misc[34].Key, new object[0]), new Color(50, 255, 130), -1);
								LoadWorld.FakeMain.startSnowMoon();
								NetMessage.TrySendData(7, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
								NetMessage.TrySendData(78, -1, -1, null, 0, 1f, 1f, 1f, 0, 0, 0);
								return;
							}
							return;
						}
						else if (num151 == -6)
						{
							if (LoadWorld.FakeMain.dayTime && !LoadWorld.FakeMain.eclipse)
							{
								ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Lang.misc[20].Key, new object[0]), new Color(50, 255, 130), -1);
								LoadWorld.FakeMain.eclipse = true;
								NetMessage.TrySendData(7, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
								return;
							}
							return;
						}
						else
						{
							if (num151 == -7)
							{
								LoadWorld.FakeMain.invasionDelay = 0;
								LoadWorld.FakeMain.StartInvasion(4);
								NetMessage.TrySendData(7, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
								NetMessage.TrySendData(78, -1, -1, null, 0, 1f, (float)(LoadWorld.FakeMain.invasionType + 3), 0f, 0, 0, 0);
								return;
							}
							if (num151 == -8)
							{
								if (NPC.downedGolemBoss && LoadWorld.FakeMain.hardMode && !NPC.AnyDanger(false) && !NPC.AnyoneNearCultists())
								{
									WorldGen.StartImpendingDoom();
									NetMessage.TrySendData(7, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
									return;
								}
								return;
							}
							else if (num151 == -10)
							{
								if (!LoadWorld.FakeMain.dayTime && !LoadWorld.FakeMain.bloodMoon)
								{
									ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Lang.misc[8].Key, new object[0]), new Color(50, 255, 130), -1);
									LoadWorld.FakeMain.bloodMoon = true;
									if (LoadWorld.FakeMain.GetMoonPhase() == MoonPhase.Empty)
									{
										LoadWorld.FakeMain.moonPhase = 5;
									}
									AchievementsHelper.NotifyProgressionEvent(4);
									NetMessage.TrySendData(7, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
									return;
								}
								return;
							}
							else
							{
								if (num151 == -11)
								{
									ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Misc.CombatBookUsed", new object[0]), new Color(50, 255, 130), -1);
									NPC.combatBookWasUsed = true;
									NetMessage.TrySendData(7, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
									return;
								}
								if (num151 == -12)
								{
									ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Misc.LicenseCatUsed", new object[0]), new Color(50, 255, 130), -1);
									NPC.boughtCat = true;
									NetMessage.TrySendData(7, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
									return;
								}
								if (num151 == -13)
								{
									ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Misc.LicenseDogUsed", new object[0]), new Color(50, 255, 130), -1);
									NPC.boughtDog = true;
									NetMessage.TrySendData(7, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
									return;
								}
								if (num151 == -14)
								{
									ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Misc.LicenseBunnyUsed", new object[0]), new Color(50, 255, 130), -1);
									NPC.boughtBunny = true;
									NetMessage.TrySendData(7, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
									return;
								}
								if (num151 < 0)
								{
									int num152 = 1;
									if (num151 > -5)
									{
										num152 = -num151;
									}
									if (num152 > 0 && LoadWorld.FakeMain.invasionType == 0)
									{
										LoadWorld.FakeMain.invasionDelay = 0;
										LoadWorld.FakeMain.StartInvasion(num152);
									}
									NetMessage.TrySendData(78, -1, -1, null, 0, 1f, (float)(LoadWorld.FakeMain.invasionType + 3), 0f, 0, 0, 0);
									return;
								}
								return;
							}
						}
						break;
					}
				case 62:
					{
						int num153 = (int)this.reader.ReadByte();
						int num154 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num153 = this.whoAmI;
						}
						if (num154 == 1)
						{
							LoadWorld.FakeMain.player[num153].NinjaDodge();
						}
						if (num154 == 2)
						{
							LoadWorld.FakeMain.player[num153].ShadowDodge();
						}
						if (num154 == 4)
						{
							LoadWorld.FakeMain.player[num153].BrainOfConfusionDodge();
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(62, -1, this.whoAmI, null, num153, (float)num154, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 63:
					{
						int num155 = (int)this.reader.ReadInt16();
						int num156 = (int)this.reader.ReadInt16();
						byte b13 = this.reader.ReadByte();
						WorldGen.paintTile(num155, num156, b13, false);
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(63, -1, this.whoAmI, null, num155, (float)num156, (float)b13, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 64:
					{
						int num157 = (int)this.reader.ReadInt16();
						int num158 = (int)this.reader.ReadInt16();
						byte b14 = this.reader.ReadByte();
						WorldGen.paintWall(num157, num158, b14, false);
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(64, -1, this.whoAmI, null, num157, (float)num158, (float)b14, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 65:
					{
						BitsByte bitsByte22 = this.reader.ReadByte();
						int num159 = (int)this.reader.ReadInt16();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num159 = this.whoAmI;
						}
						Vector2 vector3 = this.reader.ReadVector2();
						int num160 = (int)this.reader.ReadByte();
						int num161 = 0;
						if (bitsByte22[0])
						{
							num161++;
						}
						if (bitsByte22[1])
						{
							num161 += 2;
						}
						bool flag13 = false;
						if (bitsByte22[2])
						{
							flag13 = true;
						}
						int num162 = 0;
						if (bitsByte22[3])
						{
							num162 = this.reader.ReadInt32();
						}
						if (flag13)
						{
							vector3 = LoadWorld.FakeMain.player[num159].position;
						}
						if (num161 == 0)
						{
							LoadWorld.FakeMain.player[num159].Teleport(vector3, num160, num162);
						}
						else if (num161 == 1)
						{
							LoadWorld.FakeMain.npc[num159].Teleport(vector3, num160, num162);
						}
						else if (num161 == 2)
						{
							LoadWorld.FakeMain.player[num159].Teleport(vector3, num160, num162);
							if (LoadWorld.FakeMain.netMode == 2)
							{
								RemoteClient.CheckSection(this.whoAmI, vector3, 1);
								NetMessage.TrySendData(65, -1, -1, null, 0, (float)num159, vector3.X, vector3.Y, num160, flag13.ToInt(), num162);
								int num163 = -1;
								float num164 = 9999f;
								for (int num165 = 0; num165 < 255; num165++)
								{
									if (LoadWorld.FakeMain.player[num165].active && num165 != this.whoAmI)
									{
										Vector2 vector4 = LoadWorld.FakeMain.player[num165].position - LoadWorld.FakeMain.player[this.whoAmI].position;
										if (vector4.Length() < num164)
										{
											num164 = vector4.Length();
											num163 = num165;
										}
									}
								}
								if (num163 >= 0)
								{
									ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Game.HasTeleportedTo", new object[]
									{
								LoadWorld.FakeMain.player[this.whoAmI].name,
								LoadWorld.FakeMain.player[num163].name
									}), new Color(250, 250, 0), -1);
								}
							}
						}
						if (LoadWorld.FakeMain.netMode == 2 && num161 == 0)
						{
							NetMessage.TrySendData(65, -1, this.whoAmI, null, num161, (float)num159, vector3.X, vector3.Y, num160, flag13.ToInt(), num162);
							return;
						}
						return;
					}
				case 66:
					{
						int num166 = (int)this.reader.ReadByte();
						int num167 = (int)this.reader.ReadInt16();
						if (num167 <= 0)
						{
							return;
						}
						Player player14 = LoadWorld.FakeMain.player[num166];
						player14.statLife += num167;
						if (player14.statLife > player14.statLifeMax2)
						{
							player14.statLife = player14.statLifeMax2;
						}
						player14.HealEffect(num167, false);
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(66, -1, this.whoAmI, null, num166, (float)num167, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 68:
					this.reader.ReadString();
					return;
				case 69:
					{
						int num168 = (int)this.reader.ReadInt16();
						int num169 = (int)this.reader.ReadInt16();
						int num170 = (int)this.reader.ReadInt16();
						if (LoadWorld.FakeMain.netMode == 1)
						{
							if (num168 < 0 || num168 >= 8000)
							{
								return;
							}
							Chest chest3 = LoadWorld.FakeMain.chest[num168];
							if (chest3 == null)
							{
								chest3 = new Chest(false);
								chest3.x = num169;
								chest3.y = num170;
								LoadWorld.FakeMain.chest[num168] = chest3;
							}
							else if (chest3.x != num169 || chest3.y != num170)
							{
								return;
							}
							chest3.name = this.reader.ReadString();
							return;
						}
						else
						{
							if (num168 < -1 || num168 >= 8000)
							{
								return;
							}
							if (num168 == -1)
							{
								num168 = Chest.FindChest(num169, num170);
								if (num168 == -1)
								{
									return;
								}
							}
							Chest chest4 = LoadWorld.FakeMain.chest[num168];
							if (chest4.x != num169 || chest4.y != num170)
							{
								return;
							}
							NetMessage.TrySendData(69, this.whoAmI, -1, null, num168, (float)num169, (float)num170, 0f, 0, 0, 0);
							return;
						}
						break;
					}
				case 70:
					{
						if (LoadWorld.FakeMain.netMode != 2)
						{
							return;
						}
						int num171 = (int)this.reader.ReadInt16();
						int who = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							who = this.whoAmI;
						}
						if (num171 < 200 && num171 >= 0)
						{
							NPC.CatchNPC(num171, who);
							return;
						}
						return;
					}
				case 71:
					{
						if (LoadWorld.FakeMain.netMode != 2)
						{
							return;
						}
						int x4 = this.reader.ReadInt32();
						int y4 = this.reader.ReadInt32();
						int type10 = (int)this.reader.ReadInt16();
						byte style = this.reader.ReadByte();
						NPC.ReleaseNPC(x4, y4, type10, (int)style, this.whoAmI);
						return;
					}
				case 72:
					if (LoadWorld.FakeMain.netMode != 1)
					{
						return;
					}
					for (int num172 = 0; num172 < 40; num172++)
					{
						LoadWorld.FakeMain.travelShop[num172] = (int)this.reader.ReadInt16();
					}
					return;
				case 73:
					switch (this.reader.ReadByte())
					{
						case 0:
							LoadWorld.FakeMain.player[this.whoAmI].TeleportationPotion();
							return;
						case 1:
							LoadWorld.FakeMain.player[this.whoAmI].MagicConch();
							return;
						case 2:
							LoadWorld.FakeMain.player[this.whoAmI].DemonConch();
							return;
						default:
							return;
					}
					break;
				case 74:
					if (LoadWorld.FakeMain.netMode != 1)
					{
						return;
					}
					LoadWorld.FakeMain.anglerQuest = (int)this.reader.ReadByte();
					LoadWorld.FakeMain.anglerQuestFinished = this.reader.ReadBoolean();
					return;
				case 75:
					{
						if (LoadWorld.FakeMain.netMode != 2)
						{
							return;
						}
						string name2 = LoadWorld.FakeMain.player[this.whoAmI].name;
						if (!LoadWorld.FakeMain.anglerWhoFinishedToday.Contains(name2))
						{
							LoadWorld.FakeMain.anglerWhoFinishedToday.Add(name2);
							return;
						}
						return;
					}
				case 76:
					{
						int num173 = (int)this.reader.ReadByte();
						if (num173 == LoadWorld.FakeMain.myPlayer && !LoadWorld.FakeMain.ServerSideCharacter)
						{
							return;
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num173 = this.whoAmI;
						}
						Player player15 = LoadWorld.FakeMain.player[num173];
						player15.anglerQuestsFinished = this.reader.ReadInt32();
						player15.golferScoreAccumulated = this.reader.ReadInt32();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(76, -1, this.whoAmI, null, num173, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 77:
					{
						int type11 = (int)this.reader.ReadInt16();
						ushort tileType = this.reader.ReadUInt16();
						short x5 = this.reader.ReadInt16();
						short y5 = this.reader.ReadInt16();
						Animation.NewTemporaryAnimation(type11, tileType, (int)x5, (int)y5);
						return;
					}
				case 78:
					if (LoadWorld.FakeMain.netMode != 1)
					{
						return;
					}
					LoadWorld.FakeMain.ReportInvasionProgress(this.reader.ReadInt32(), this.reader.ReadInt32(), (int)this.reader.ReadSByte(), (int)this.reader.ReadSByte());
					return;
				case 79:
					{
						int x6 = (int)this.reader.ReadInt16();
						int y6 = (int)this.reader.ReadInt16();
						short type12 = this.reader.ReadInt16();
						int style2 = (int)this.reader.ReadInt16();
						int num174 = (int)this.reader.ReadByte();
						int random = (int)this.reader.ReadSByte();
						int direction;
						if (this.reader.ReadBoolean())
						{
							direction = 1;
						}
						else
						{
							direction = -1;
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							Netplay.Clients[this.whoAmI].SpamAddBlock += 1f;
							if (!WorldGen.InWorld(x6, y6, 10) || !Netplay.Clients[this.whoAmI].TileSections[Netplay.GetSectionX(x6), Netplay.GetSectionY(y6)])
							{
								return;
							}
						}
						WorldGen.PlaceObject(x6, y6, (int)type12, false, style2, num174, random, direction);
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.SendObjectPlacment(this.whoAmI, x6, y6, (int)type12, style2, num174, random, direction);
							return;
						}
						return;
					}
				case 80:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						int num175 = (int)this.reader.ReadByte();
						int num176 = (int)this.reader.ReadInt16();
						if (num176 >= -3 && num176 < 8000)
						{
							LoadWorld.FakeMain.player[num175].chest = num176;
							Recipe.FindRecipes(true);
							return;
						}
						return;
					}
				case 81:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						int x7 = (int)this.reader.ReadSingle();
						int y7 = (int)this.reader.ReadSingle();
						Color color3 = this.reader.ReadRGB();
						int amount = this.reader.ReadInt32();
						CombatText.NewText(new Rectangle(x7, y7, 0, 0), color3, amount, false, false);
						return;
					}
				case 82:
					Terraria.Net.NetManager.Instance.Read(this.reader, this.whoAmI, length);
					return;
				case 83:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						int num177 = (int)this.reader.ReadInt16();
						int num178 = this.reader.ReadInt32();
						if (num177 >= 0 && num177 < 289)
						{
							NPC.killCount[num177] = num178;
							return;
						}
						return;
					}
				case 84:
					{
						int num179 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num179 = this.whoAmI;
						}
						float stealth = this.reader.ReadSingle();
						LoadWorld.FakeMain.player[num179].stealth = stealth;
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(84, -1, this.whoAmI, null, num179, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 85:
					{
						int num180 = this.whoAmI;
						byte b15 = this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2 && num180 < 255 && b15 < 58)
						{
							Chest.ServerPlaceItem(this.whoAmI, (int)b15);
							return;
						}
						return;
					}
				case 86:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						int num181 = this.reader.ReadInt32();
						if (this.reader.ReadBoolean())
						{
							TileEntity tileEntity = TileEntity.Read(this.reader, true);
							tileEntity.ID = num181;
							TileEntity.ByID[tileEntity.ID] = tileEntity;
							TileEntity.ByPosition[tileEntity.Position] = tileEntity;
							return;
						}
						TileEntity tileEntity2;
						if (TileEntity.ByID.TryGetValue(num181, out tileEntity2))
						{
							TileEntity.ByID.Remove(num181);
							TileEntity.ByPosition.Remove(tileEntity2.Position);
							return;
						}
						return;
					}
				case 87:
					{
						if (LoadWorld.FakeMain.netMode != 2)
						{
							return;
						}
						int x8 = (int)this.reader.ReadInt16();
						int y8 = (int)this.reader.ReadInt16();
						int type13 = (int)this.reader.ReadByte();
						if (!WorldGen.InWorld(x8, y8, 0))
						{
							return;
						}
						if (TileEntity.ByPosition.ContainsKey(new Point16(x8, y8)))
						{
							return;
						}
						TileEntity.PlaceEntityNet(x8, y8, type13);
						return;
					}
				case 88:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						int num182 = (int)this.reader.ReadInt16();
						if (num182 < 0 || num182 > 400)
						{
							return;
						}
						Item item4 = LoadWorld.FakeMain.item[num182];
						BitsByte bitsByte23 = this.reader.ReadByte();
						if (bitsByte23[0])
						{
							item4.color.PackedValue = this.reader.ReadUInt32();
						}
						if (bitsByte23[1])
						{
							item4.damage = (int)this.reader.ReadUInt16();
						}
						if (bitsByte23[2])
						{
							item4.knockBack = this.reader.ReadSingle();
						}
						if (bitsByte23[3])
						{
							item4.useAnimation = (int)this.reader.ReadUInt16();
						}
						if (bitsByte23[4])
						{
							item4.useTime = (int)this.reader.ReadUInt16();
						}
						if (bitsByte23[5])
						{
							item4.shoot = (int)this.reader.ReadInt16();
						}
						if (bitsByte23[6])
						{
							item4.shootSpeed = this.reader.ReadSingle();
						}
						if (!bitsByte23[7])
						{
							return;
						}
						bitsByte23 = this.reader.ReadByte();
						if (bitsByte23[0])
						{
							item4.width = (int)this.reader.ReadInt16();
						}
						if (bitsByte23[1])
						{
							item4.height = (int)this.reader.ReadInt16();
						}
						if (bitsByte23[2])
						{
							item4.scale = this.reader.ReadSingle();
						}
						if (bitsByte23[3])
						{
							item4.ammo = (int)this.reader.ReadInt16();
						}
						if (bitsByte23[4])
						{
							item4.useAmmo = (int)this.reader.ReadInt16();
						}
						if (bitsByte23[5])
						{
							item4.notAmmo = this.reader.ReadBoolean();
							return;
						}
						return;
					}
				case 89:
					{
						if (LoadWorld.FakeMain.netMode != 2)
						{
							return;
						}
						int x9 = (int)this.reader.ReadInt16();
						int y9 = (int)this.reader.ReadInt16();
						int netid = (int)this.reader.ReadInt16();
						int prefix = (int)this.reader.ReadByte();
						int stack5 = (int)this.reader.ReadInt16();
						TEItemFrame.TryPlacing(x9, y9, netid, prefix, stack5);
						return;
					}
				case 91:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						int num183 = this.reader.ReadInt32();
						int num184 = (int)this.reader.ReadByte();
						if (num184 != 255)
						{
							int num185 = (int)this.reader.ReadUInt16();
							int num186 = (int)this.reader.ReadUInt16();
							int num187 = (int)this.reader.ReadByte();
							int metadata = 0;
							if (num187 < 0)
							{
								metadata = (int)this.reader.ReadInt16();
							}
							WorldUIAnchor worldUIAnchor = EmoteBubble.DeserializeNetAnchor(num184, num185);
							if (num184 == 1)
							{
								LoadWorld.FakeMain.player[num185].emoteTime = 360;
							}
							Dictionary<int, EmoteBubble> byID = EmoteBubble.byID;
							lock (byID)
							{
								if (!EmoteBubble.byID.ContainsKey(num183))
								{
									EmoteBubble.byID[num183] = new EmoteBubble(num187, worldUIAnchor, num186);
								}
								else
								{
									EmoteBubble.byID[num183].lifeTime = num186;
									EmoteBubble.byID[num183].lifeTimeStart = num186;
									EmoteBubble.byID[num183].emote = num187;
									EmoteBubble.byID[num183].anchor = worldUIAnchor;
								}
								EmoteBubble.byID[num183].ID = num183;
								EmoteBubble.byID[num183].metadata = metadata;
								EmoteBubble.OnBubbleChange(num183);
								return;
							}
							goto IL_73EF;
						}
						if (EmoteBubble.byID.ContainsKey(num183))
						{
							EmoteBubble.byID.Remove(num183);
							return;
						}
						return;
					}
				case 92:
					goto IL_73EF;
				case 94:
				case 138:
					goto IL_9134;
				case 95:
					{
						ushort num188 = this.reader.ReadUInt16();
						int num189 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode != 2)
						{
							return;
						}
						for (int num190 = 0; num190 < 1000; num190++)
						{
							if (LoadWorld.FakeMain.projectile[num190].owner == (int)num188 && LoadWorld.FakeMain.projectile[num190].active && LoadWorld.FakeMain.projectile[num190].type == 602 && LoadWorld.FakeMain.projectile[num190].ai[1] == (float)num189)
							{
								LoadWorld.FakeMain.projectile[num190].Kill();
								NetMessage.TrySendData(29, -1, -1, null, LoadWorld.FakeMain.projectile[num190].identity, (float)num188, 0f, 0f, 0, 0, 0);
								return;
							}
						}
						return;
					}
				case 96:
					{
						int num191 = (int)this.reader.ReadByte();
						Player player16 = LoadWorld.FakeMain.player[num191];
						int num192 = (int)this.reader.ReadInt16();
						Vector2 vector5 = this.reader.ReadVector2();
						Vector2 velocity4 = this.reader.ReadVector2();
						int lastPortalColorIndex = num192 + ((num192 % 2 == 0) ? 1 : -1);
						player16.lastPortalColorIndex = lastPortalColorIndex;
						player16.Teleport(vector5, 4, num192);
						player16.velocity = velocity4;
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.SendData(96, -1, -1, null, num191, vector5.X, vector5.Y, (float)num192, 0, 0, 0);
							return;
						}
						return;
					}
				case 97:
					if (LoadWorld.FakeMain.netMode != 1)
					{
						return;
					}
					AchievementsHelper.NotifyNPCKilledDirect(LoadWorld.FakeMain.player[LoadWorld.FakeMain.myPlayer], (int)this.reader.ReadInt16());
					return;
				case 98:
					if (LoadWorld.FakeMain.netMode != 1)
					{
						return;
					}
					AchievementsHelper.NotifyProgressionEvent((int)this.reader.ReadInt16());
					return;
				case 99:
					{
						int num193 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num193 = this.whoAmI;
						}
						LoadWorld.FakeMain.player[num193].MinionRestTargetPoint = this.reader.ReadVector2();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(99, -1, this.whoAmI, null, num193, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 100:
					{
						int num194 = (int)this.reader.ReadUInt16();
						NPC npc3 = LoadWorld.FakeMain.npc[num194];
						int num195 = (int)this.reader.ReadInt16();
						Vector2 newPos = this.reader.ReadVector2();
						Vector2 velocity5 = this.reader.ReadVector2();
						int lastPortalColorIndex2 = num195 + ((num195 % 2 == 0) ? 1 : -1);
						npc3.lastPortalColorIndex = lastPortalColorIndex2;
						npc3.Teleport(newPos, 4, num195);
						npc3.velocity = velocity5;
						npc3.netOffset *= 0f;
						return;
					}
				case 101:
					if (LoadWorld.FakeMain.netMode == 2)
					{
						return;
					}
					NPC.ShieldStrengthTowerSolar = (int)this.reader.ReadUInt16();
					NPC.ShieldStrengthTowerVortex = (int)this.reader.ReadUInt16();
					NPC.ShieldStrengthTowerNebula = (int)this.reader.ReadUInt16();
					NPC.ShieldStrengthTowerStardust = (int)this.reader.ReadUInt16();
					if (NPC.ShieldStrengthTowerSolar < 0)
					{
						NPC.ShieldStrengthTowerSolar = 0;
					}
					if (NPC.ShieldStrengthTowerVortex < 0)
					{
						NPC.ShieldStrengthTowerVortex = 0;
					}
					if (NPC.ShieldStrengthTowerNebula < 0)
					{
						NPC.ShieldStrengthTowerNebula = 0;
					}
					if (NPC.ShieldStrengthTowerStardust < 0)
					{
						NPC.ShieldStrengthTowerStardust = 0;
					}
					if (NPC.ShieldStrengthTowerSolar > NPC.LunarShieldPowerExpert)
					{
						NPC.ShieldStrengthTowerSolar = NPC.LunarShieldPowerExpert;
					}
					if (NPC.ShieldStrengthTowerVortex > NPC.LunarShieldPowerExpert)
					{
						NPC.ShieldStrengthTowerVortex = NPC.LunarShieldPowerExpert;
					}
					if (NPC.ShieldStrengthTowerNebula > NPC.LunarShieldPowerExpert)
					{
						NPC.ShieldStrengthTowerNebula = NPC.LunarShieldPowerExpert;
					}
					if (NPC.ShieldStrengthTowerStardust > NPC.LunarShieldPowerExpert)
					{
						NPC.ShieldStrengthTowerStardust = NPC.LunarShieldPowerExpert;
						return;
					}
					return;
				case 102:
					{
						int num196 = (int)this.reader.ReadByte();
						ushort num197 = this.reader.ReadUInt16();
						Vector2 vector6 = this.reader.ReadVector2();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num196 = this.whoAmI;
							NetMessage.TrySendData(102, -1, -1, null, num196, (float)num197, vector6.X, vector6.Y, 0, 0, 0);
							return;
						}
						Player player17 = LoadWorld.FakeMain.player[num196];
						for (int num198 = 0; num198 < 255; num198++)
						{
							Player player18 = LoadWorld.FakeMain.player[num198];
							if (player18.active && !player18.dead && (player17.team == 0 || player17.team == player18.team) && player18.Distance(vector6) < 700f)
							{
								Vector2 value2 = player17.Center - player18.Center;
								Vector2 vector7 = Vector2.Normalize(value2);
								if (!vector7.HasNaNs())
								{
									int type14 = 90;
									float num199 = 0f;
									float num200 = 0.209439516f;
									Vector2 spinningpoint = new Vector2(0f, -8f);
									Vector2 value3 = new Vector2(-3f);
									float num201 = 0f;
									float num202 = 0.005f;
									if (num197 != 173)
									{
										if (num197 != 176)
										{
											if (num197 == 179)
											{
												type14 = 86;
											}
										}
										else
										{
											type14 = 88;
										}
									}
									else
									{
										type14 = 90;
									}
									int num203 = 0;
									while ((float)num203 < value2.Length() / 6f)
									{
										Vector2 position2 = player18.Center + 6f * (float)num203 * vector7 + spinningpoint.RotatedBy((double)num199, default(Vector2)) + value3;
										num199 += num200;
										int num204 = Dust.NewDust(position2, 6, 6, type14, 0f, 0f, 100, default(Color), 1.5f);
										LoadWorld.FakeMain.dust[num204].noGravity = true;
										LoadWorld.FakeMain.dust[num204].velocity = Vector2.Zero;
										num201 = (LoadWorld.FakeMain.dust[num204].fadeIn = num201 + num202);
										LoadWorld.FakeMain.dust[num204].velocity += vector7 * 1.5f;
										num203++;
									}
								}
								player18.NebulaLevelup((int)num197);
							}
						}
						return;
					}
				case 103:
					if (LoadWorld.FakeMain.netMode == 1)
					{
						NPC.MoonLordCountdown = this.reader.ReadInt32();
						return;
					}
					return;
				case 104:
					{
						if (LoadWorld.FakeMain.netMode != 1 || LoadWorld.FakeMain.npcShop <= 0)
						{
							return;
						}
						Item[] item5 = LoadWorld.FakeMain.instance.shop[LoadWorld.FakeMain.npcShop].item;
						int num205 = (int)this.reader.ReadByte();
						int type15 = (int)this.reader.ReadInt16();
						int stack6 = (int)this.reader.ReadInt16();
						int pre3 = (int)this.reader.ReadByte();
						int value4 = this.reader.ReadInt32();
						BitsByte bitsByte24 = this.reader.ReadByte();
						if (num205 < item5.Length)
						{
							item5[num205] = new Item();
							item5[num205].netDefaults(type15);
							item5[num205].stack = stack6;
							item5[num205].Prefix(pre3);
							item5[num205].value = value4;
							item5[num205].buyOnce = bitsByte24[0];
							return;
						}
						return;
					}
				case 105:
					{
						if (LoadWorld.FakeMain.netMode == 1)
						{
							return;
						}
						int i3 = (int)this.reader.ReadInt16();
						int j3 = (int)this.reader.ReadInt16();
						bool on = this.reader.ReadBoolean();
						WorldGen.ToggleGemLock(i3, j3, on);
						return;
					}
				case 106:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						HalfVector2 halfVector = default(HalfVector2);
						halfVector.PackedValue = this.reader.ReadUInt32();
						Utils.PoofOfSmoke(halfVector.ToVector2());
						return;
					}
				case 107:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						Color c = this.reader.ReadRGB();
						string text2 = NetworkText.Deserialize(this.reader).ToString();
						int widthLimit = (int)this.reader.ReadInt16();
						LoadWorld.FakeMain.NewTextMultiline(text2, false, c, widthLimit);
						return;
					}
				case 108:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						int damage2 = (int)this.reader.ReadInt16();
						float knockBack2 = this.reader.ReadSingle();
						int x10 = (int)this.reader.ReadInt16();
						int y10 = (int)this.reader.ReadInt16();
						int angle = (int)this.reader.ReadInt16();
						int ammo = (int)this.reader.ReadInt16();
						int num206 = (int)this.reader.ReadByte();
						if (num206 != LoadWorld.FakeMain.myPlayer)
						{
							return;
						}
						WorldGen.ShootFromCannon(x10, y10, angle, ammo, damage2, knockBack2, num206);
						return;
					}
				case 109:
					{
						if (LoadWorld.FakeMain.netMode != 2)
						{
							return;
						}
						int x11 = (int)this.reader.ReadInt16();
						int y11 = (int)this.reader.ReadInt16();
						int x12 = (int)this.reader.ReadInt16();
						int y12 = (int)this.reader.ReadInt16();
						WiresUI.Settings.MultiToolMode toolMode = (WiresUI.Settings.MultiToolMode)this.reader.ReadByte();
						int num207 = this.whoAmI;
						WiresUI.Settings.MultiToolMode toolMode2 = WiresUI.Settings.ToolMode;
						WiresUI.Settings.ToolMode = toolMode;
						Wiring.MassWireOperation(new Point(x11, y11), new Point(x12, y12), LoadWorld.FakeMain.player[num207]);
						WiresUI.Settings.ToolMode = toolMode2;
						return;
					}
				case 110:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						int type16 = (int)this.reader.ReadInt16();
						int num208 = (int)this.reader.ReadInt16();
						int num209 = (int)this.reader.ReadByte();
						if (num209 != LoadWorld.FakeMain.myPlayer)
						{
							return;
						}
						Player player19 = LoadWorld.FakeMain.player[num209];
						for (int num210 = 0; num210 < num208; num210++)
						{
							player19.ConsumeItem(type16, false);
						}
						player19.wireOperationsCooldown = 0;
						return;
					}
				case 111:
					if (LoadWorld.FakeMain.netMode != 2)
					{
						return;
					}
					BirthdayParty.ToggleManualParty();
					return;
				case 112:
					{
						int num211 = (int)this.reader.ReadByte();
						int num212 = this.reader.ReadInt32();
						int num213 = this.reader.ReadInt32();
						int num214 = (int)this.reader.ReadByte();
						int num215 = (int)this.reader.ReadInt16();
						if (num211 == 1)
						{
							if (LoadWorld.FakeMain.netMode == 1)
							{
								WorldGen.TreeGrowFX(num212, num213, num214, num215, false);
							}
							if (LoadWorld.FakeMain.netMode == 2)
							{
								NetMessage.TrySendData((int)b, -1, -1, null, num211, (float)num212, (float)num213, (float)num214, num215, 0, 0);
								return;
							}
							return;
						}
						else
						{
							if (num211 == 2)
							{
								NPC.FairyEffects(new Vector2((float)num212, (float)num213), num214);
								return;
							}
							return;
						}
						break;
					}
				case 113:
					{
						int x13 = (int)this.reader.ReadInt16();
						int y13 = (int)this.reader.ReadInt16();
						if (LoadWorld.FakeMain.netMode == 2 && !LoadWorld.FakeMain.snowMoon && !LoadWorld.FakeMain.pumpkinMoon)
						{
							if (DD2Event.WouldFailSpawningHere(x13, y13))
							{
								DD2Event.FailureMessage(this.whoAmI);
							}
							DD2Event.SummonCrystal(x13, y13);
							return;
						}
						return;
					}
				case 114:
					if (LoadWorld.FakeMain.netMode != 1)
					{
						return;
					}
					DD2Event.WipeEntities();
					return;
				case 115:
					{
						int num216 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num216 = this.whoAmI;
						}
						LoadWorld.FakeMain.player[num216].MinionAttackTargetNPC = (int)this.reader.ReadInt16();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(115, -1, this.whoAmI, null, num216, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 116:
					if (LoadWorld.FakeMain.netMode != 1)
					{
						return;
					}
					DD2Event.TimeLeftBetweenWaves = this.reader.ReadInt32();
					return;
				case 117:
					{
						int num217 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2 && this.whoAmI != num217 && (!LoadWorld.FakeMain.player[num217].hostile || !LoadWorld.FakeMain.player[this.whoAmI].hostile))
						{
							return;
						}
						PlayerDeathReason playerDeathReason = PlayerDeathReason.FromReader(this.reader);
						int damage3 = (int)this.reader.ReadInt16();
						int num218 = (int)(this.reader.ReadByte() - 1);
						BitsByte bitsByte25 = this.reader.ReadByte();
						bool flag14 = bitsByte25[0];
						bool pvp = bitsByte25[1];
						int num219 = (int)this.reader.ReadSByte();
						LoadWorld.FakeMain.player[num217].Hurt(playerDeathReason, damage3, num218, pvp, true, flag14, num219);
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.SendPlayerHurt(num217, playerDeathReason, damage3, num218, flag14, pvp, num219, -1, this.whoAmI);
							return;
						}
						return;
					}
				case 118:
					{
						int num220 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num220 = this.whoAmI;
						}
						PlayerDeathReason playerDeathReason2 = PlayerDeathReason.FromReader(this.reader);
						int num221 = (int)this.reader.ReadInt16();
						int num222 = (int)(this.reader.ReadByte() - 1);
						bool pvp2 = this.reader.ReadByte() == 1;
						LoadWorld.FakeMain.player[num220].KillMe(playerDeathReason2, (double)num221, num222, pvp2);
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.SendPlayerDeath(num220, playerDeathReason2, num221, num222, pvp2, -1, this.whoAmI);
							return;
						}
						return;
					}
				case 119:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						int x14 = (int)this.reader.ReadSingle();
						int y14 = (int)this.reader.ReadSingle();
						Color color4 = this.reader.ReadRGB();
						NetworkText networkText = NetworkText.Deserialize(this.reader);
						CombatText.NewText(new Rectangle(x14, y14, 0, 0), color4, networkText.ToString(), false, false);
						return;
					}
				case 120:
					{
						int num223 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num223 = this.whoAmI;
						}
						int num224 = (int)this.reader.ReadByte();
						if (num224 >= 0 && num224 < 145 && LoadWorld.FakeMain.netMode == 2)
						{
							EmoteBubble.NewBubble(num224, new WorldUIAnchor(LoadWorld.FakeMain.player[num223]), 360);
							EmoteBubble.CheckForNPCsToReactToEmoteBubble(num224, LoadWorld.FakeMain.player[num223]);
							return;
						}
						return;
					}
				case 121:
					{
						int num225 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num225 = this.whoAmI;
						}
						int num226 = this.reader.ReadInt32();
						int num227 = (int)this.reader.ReadByte();
						bool flag15 = false;
						if (num227 >= 8)
						{
							flag15 = true;
							num227 -= 8;
						}
						TileEntity tileEntity3;
						if (!TileEntity.ByID.TryGetValue(num226, out tileEntity3))
						{
							this.reader.ReadInt32();
							this.reader.ReadByte();
							return;
						}
						if (num227 >= 8)
						{
							tileEntity3 = null;
						}
						TEDisplayDoll tedisplayDoll = tileEntity3 as TEDisplayDoll;
						if (tedisplayDoll != null)
						{
							tedisplayDoll.ReadItem(num227, this.reader, flag15);
						}
						else
						{
							this.reader.ReadInt32();
							this.reader.ReadByte();
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData((int)b, -1, num225, null, num225, (float)num226, (float)num227, (float)flag15.ToInt(), 0, 0, 0);
							return;
						}
						return;
					}
				case 122:
					{
						int num228 = this.reader.ReadInt32();
						int num229 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num229 = this.whoAmI;
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							if (num228 == -1)
							{
								LoadWorld.FakeMain.player[num229].tileEntityAnchor.Clear();
								NetMessage.TrySendData((int)b, -1, -1, null, num228, (float)num229, 0f, 0f, 0, 0, 0);
								return;
							}
							int num230;
							TileEntity tileEntity4;
							if (!TileEntity.IsOccupied(num228, out num230) && TileEntity.ByID.TryGetValue(num228, out tileEntity4))
							{
								LoadWorld.FakeMain.player[num229].tileEntityAnchor.Set(num228, (int)tileEntity4.Position.X, (int)tileEntity4.Position.Y);
								NetMessage.TrySendData((int)b, -1, -1, null, num228, (float)num229, 0f, 0f, 0, 0, 0);
							}
						}
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						if (num228 == -1)
						{
							LoadWorld.FakeMain.player[num229].tileEntityAnchor.Clear();
							return;
						}
						TileEntity tileEntity5;
						if (TileEntity.ByID.TryGetValue(num228, out tileEntity5))
						{
							TileEntity.SetInteractionAnchor(LoadWorld.FakeMain.player[num229], (int)tileEntity5.Position.X, (int)tileEntity5.Position.Y, num228);
							return;
						}
						return;
					}
				case 123:
					{
						if (LoadWorld.FakeMain.netMode != 2)
						{
							return;
						}
						int x15 = (int)this.reader.ReadInt16();
						int y15 = (int)this.reader.ReadInt16();
						int netid2 = (int)this.reader.ReadInt16();
						int prefix2 = (int)this.reader.ReadByte();
						int stack7 = (int)this.reader.ReadInt16();
						TEWeaponsRack.TryPlacing(x15, y15, netid2, prefix2, stack7);
						return;
					}
				case 124:
					{
						int num231 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num231 = this.whoAmI;
						}
						int num232 = this.reader.ReadInt32();
						int num233 = (int)this.reader.ReadByte();
						bool flag16 = false;
						if (num233 >= 2)
						{
							flag16 = true;
							num233 -= 2;
						}
						TileEntity tileEntity6;
						if (!TileEntity.ByID.TryGetValue(num232, out tileEntity6))
						{
							this.reader.ReadInt32();
							this.reader.ReadByte();
							return;
						}
						if (num233 >= 2)
						{
							tileEntity6 = null;
						}
						TEHatRack tehatRack = tileEntity6 as TEHatRack;
						if (tehatRack != null)
						{
							tehatRack.ReadItem(num233, this.reader, flag16);
						}
						else
						{
							this.reader.ReadInt32();
							this.reader.ReadByte();
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData((int)b, -1, num231, null, num231, (float)num232, (float)num233, (float)flag16.ToInt(), 0, 0, 0);
							return;
						}
						return;
					}
				case 125:
					{
						int num234 = (int)this.reader.ReadByte();
						int num235 = (int)this.reader.ReadInt16();
						int num236 = (int)this.reader.ReadInt16();
						int num237 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num234 = this.whoAmI;
						}
						if (LoadWorld.FakeMain.netMode == 1)
						{
							LoadWorld.FakeMain.player[LoadWorld.FakeMain.myPlayer].GetOtherPlayersPickTile(num235, num236, num237);
						}
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.TrySendData(125, -1, num234, null, num234, (float)num235, (float)num236, (float)num237, 0, 0, 0);
							return;
						}
						return;
					}
				case 126:
					if (LoadWorld.FakeMain.netMode != 1)
					{
						return;
					}
					NPC.RevengeManager.AddMarkerFromReader(this.reader);
					return;
				case 127:
					{
						int markerUniqueID = this.reader.ReadInt32();
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						NPC.RevengeManager.DestroyMarker(markerUniqueID);
						return;
					}
				case 128:
					{
						int num238 = (int)this.reader.ReadByte();
						int num239 = (int)this.reader.ReadUInt16();
						int num240 = (int)this.reader.ReadUInt16();
						int num241 = (int)this.reader.ReadUInt16();
						int num242 = (int)this.reader.ReadUInt16();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.SendData(128, -1, num238, null, num238, (float)num241, (float)num242, 0f, num239, num240, 0);
							return;
						}
						GolfHelper.ContactListener.PutBallInCup_TextAndEffects(new Point(num239, num240), num238, num241, num242);
						return;
					}
				case 129:
					if (LoadWorld.FakeMain.netMode != 1)
					{
						return;
					}
					LoadWorld.FakeMain.FixUIScale();
					LoadWorld.FakeMain.TrySetPreparationState(LoadWorld.FakeMain.WorldPreparationState.ProcessingData);
					return;
				case 130:
					{
						if (LoadWorld.FakeMain.netMode != 2)
						{
							return;
						}
						int num243 = (int)this.reader.ReadUInt16();
						int num244 = (int)this.reader.ReadUInt16();
						int type17 = (int)this.reader.ReadInt16();
						int x16 = num243 * 16;
						num244 *= 16;
						NPC npc4 = new NPC();
						npc4.SetDefaults(type17, default(NPCSpawnParams));
						int type18 = npc4.type;
						int netID = npc4.netID;
						int num245 = NPC.NewNPC(x16, num244, type17, 0, 0f, 0f, 0f, 0f, 255);
						if (netID != type18)
						{
							LoadWorld.FakeMain.npc[num245].SetDefaults(netID, default(NPCSpawnParams));
							NetMessage.TrySendData(23, -1, -1, null, num245, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 131:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						int num246 = (int)this.reader.ReadUInt16();
						NPC npc5;
						if (num246 < 200)
						{
							npc5 = LoadWorld.FakeMain.npc[num246];
						}
						else
						{
							npc5 = new NPC();
						}
						int num247 = (int)this.reader.ReadByte();
						if (num247 == 1)
						{
							int time2 = this.reader.ReadInt32();
							int fromWho = (int)this.reader.ReadInt16();
							npc5.GetImmuneTime(fromWho, time2);
							return;
						}
						return;
					}
				case 132:
					{
						if (LoadWorld.FakeMain.netMode != 1)
						{
							return;
						}
						Point point = this.reader.ReadVector2().ToPoint();
						ushort key = this.reader.ReadUInt16();
						LegacySoundStyle legacySoundStyle = SoundID.SoundByIndex[key];
						BitsByte bitsByte26 = this.reader.ReadByte();
						int style3;
						if (bitsByte26[0])
						{
							style3 = this.reader.ReadInt32();
						}
						else
						{
							style3 = legacySoundStyle.Style;
						}
						float volumeScale;
						if (bitsByte26[1])
						{
							volumeScale = MathHelper.Clamp(this.reader.ReadSingle(), 0f, 1f);
						}
						else
						{
							volumeScale = legacySoundStyle.Volume;
						}
						float pitchOffset;
						if (bitsByte26[2])
						{
							pitchOffset = MathHelper.Clamp(this.reader.ReadSingle(), -1f, 1f);
						}
						else
						{
							pitchOffset = legacySoundStyle.GetRandomPitch();
						}
						SoundEngine.PlaySound(legacySoundStyle.SoundId, point.X, point.Y, style3, volumeScale, pitchOffset);
						return;
					}
				case 133:
					{
						if (LoadWorld.FakeMain.netMode != 2)
						{
							return;
						}
						int x17 = (int)this.reader.ReadInt16();
						int y16 = (int)this.reader.ReadInt16();
						int netid3 = (int)this.reader.ReadInt16();
						int prefix3 = (int)this.reader.ReadByte();
						int stack8 = (int)this.reader.ReadInt16();
						TEFoodPlatter.TryPlacing(x17, y16, netid3, prefix3, stack8);
						return;
					}
				case 134:
					{
						int num248 = (int)this.reader.ReadByte();
						int ladyBugLuckTimeLeft = this.reader.ReadInt32();
						float torchLuck = this.reader.ReadSingle();
						byte luckPotion = this.reader.ReadByte();
						bool hasGardenGnomeNearby = this.reader.ReadBoolean();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							num248 = this.whoAmI;
						}
						Player player20 = LoadWorld.FakeMain.player[num248];
						player20.ladyBugLuckTimeLeft = ladyBugLuckTimeLeft;
						player20.torchLuck = torchLuck;
						player20.luckPotion = luckPotion;
						player20.HasGardenGnomeNearby = hasGardenGnomeNearby;
						player20.RecalculateLuck();
						if (LoadWorld.FakeMain.netMode == 2)
						{
							NetMessage.SendData(134, -1, num248, null, num248, 0f, 0f, 0f, 0, 0, 0);
							return;
						}
						return;
					}
				case 135:
					{
						int num249 = (int)this.reader.ReadByte();
						if (LoadWorld.FakeMain.netMode == 1)
						{
							LoadWorld.FakeMain.player[num249].immuneAlpha = 255;
							return;
						}
						return;
					}
				case 136:
					for (int num250 = 0; num250 < 2; num250++)
					{
						for (int num251 = 0; num251 < 3; num251++)
						{
							NPC.cavernMonsterType[num250, num251] = (int)this.reader.ReadUInt16();
						}
					}
					return;
				case 137:
					{
						if (LoadWorld.FakeMain.netMode != 2)
						{
							return;
						}
						int num252 = (int)this.reader.ReadInt16();
						int buffTypeToRemove = (int)this.reader.ReadUInt16();
						if (num252 >= 0 && num252 < 200)
						{
							LoadWorld.FakeMain.npc[num252].RequestBuffRemoval(buffTypeToRemove);
							return;
						}
						return;
					}
				case 139:
					if (LoadWorld.FakeMain.netMode != 2)
					{
						int num253 = (int)this.reader.ReadByte();
						bool flag17 = this.reader.ReadBoolean();
						LoadWorld.FakeMain.countsAsHostForGameplay[num253] = flag17;
						return;
					}
					return;
				default:
					goto IL_9134;
			}
			if (LoadWorld.FakeMain.netMode != 2)
			{
				return;
			}
			if (Netplay.Clients[this.whoAmI].State == 1)
			{
				Netplay.Clients[this.whoAmI].State = 2;
			}
			NetMessage.TrySendData(7, this.whoAmI, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
			LoadWorld.FakeMain.SyncAnInvasion(this.whoAmI);
			return;
		IL_51B2:
			if (Netplay.Connection.State == 6)
			{
				Netplay.Connection.State = 10;
				LoadWorld.FakeMain.ActivePlayerFileData.StartPlayTimer();
				Player.Hooks.EnterWorld(LoadWorld.FakeMain.myPlayer);
				LoadWorld.FakeMain.player[LoadWorld.FakeMain.myPlayer].Spawn(PlayerSpawnContext.SpawningIntoWorld);
				return;
			}
			return;
		IL_73EF:
			int num254 = (int)this.reader.ReadInt16();
			int num255 = this.reader.ReadInt32();
			float num256 = this.reader.ReadSingle();
			float num257 = this.reader.ReadSingle();
			if (num254 < 0 || num254 > 200)
			{
				return;
			}
			if (LoadWorld.FakeMain.netMode == 1)
			{
				LoadWorld.FakeMain.npc[num254].moneyPing(new Vector2(num256, num257));
				LoadWorld.FakeMain.npc[num254].extraValue = num255;
				return;
			}
			LoadWorld.FakeMain.npc[num254].extraValue += num255;
			NetMessage.TrySendData(92, -1, -1, null, num254, (float)LoadWorld.FakeMain.npc[num254].extraValue, num256, num257, 0, 0, 0);
			return;
		IL_9134:
			if (Netplay.Clients[this.whoAmI].State == 0)
			{
				NetMessage.BootPlayer(this.whoAmI, Lang.mp[2].ToNetworkText());
			}
		}

		public const int readBufferMax = 131070;

		public const int writeBufferMax = 131070;

		public bool broadcast;

		public byte[] readBuffer = new byte[131070];

		public bool writeLocked;

		public int messageLength;

		public int totalData;

		public int whoAmI;

		public int spamCount;

		public int maxSpam;

		public bool checkBytes;

		public MemoryStream readerStream;

		public MemoryStream writerStream;

		public BinaryReader reader;

		public BinaryWriter writer;

		public PacketHistory History = new PacketHistory(100, 65535);
	}*/
}
