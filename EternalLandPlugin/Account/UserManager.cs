using EternalLandPlugin.Game;
using EternalLandPlugin.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace EternalLandPlugin.Account
{
    class UserManager
    {
        public static EPlayer GetEPlayerFromID(int id)
        {
            EPlayer eplr = null;
            EternalLand.OnlineEPlayer.ForEach(e => { if (e.ID == id) eplr = e; });
            return eplr;
        }

        public static bool TryGetEPlayerFromID(int id, out EPlayer eplr)
        {
            eplr = null;
            var list = (from temp in EternalLand.OnlineEPlayer where temp.ID == id select temp).ToList();
            if (list.Any())
            {
                eplr = list[0];
                return true;
            }
            else
            {
                eplr = null;
                return false;
            }
        }

        public static bool TryGetEPlayeFuzzy(string name, out List<EPlayer> eplrs, bool offline = false)
        {
            var list = (from temp in (offline ? DataBase.GetEPlayerFuzzy(name).Result : EternalLand.OnlineEPlayer) where temp.Name.ToLower().Contains(name.ToLower()) select temp).ToList();
            if (list.Any())
            {
                eplrs = list;
                return true;
            }
            else
            {
                eplrs = null;
                return false;
            }
        }

        public static bool TryGetEPlayeFromName(string name, out EPlayer eplr, bool offline = false)
        {
            eplr = null;
            var list = (from temp in (offline ? DataBase.GetEPlayerFuzzy(name).Result : EternalLand.OnlineEPlayer) where temp.Name == name select temp).ToList();
            if (list.Any())
            {
                eplr = list[0];
                return true;
            }
            else
            {
                eplr = null;
                return false;
            }
        }

        public static bool GetTSPlayerFromName(string name, out TSPlayer tsp)
        {
            tsp = null;
            foreach (var t in from t in TShock.Players where t != null && t.Name == name select t)
            {
                tsp = t;
                return true;
            }

            return false;
        }

        public static bool GetTSPlayerFuzzy(string name, out List<TSPlayer> list)
        {
            list = new List<TSPlayer>();
            foreach (var tsp in EternalLand.OnlineTSPlayer)
            {
                if (tsp.Name.ToLower().Contains(name.ToLower()))
                {
                    list.Add(tsp);
                }
            }
            if (list.Any()) return true;
            else return false;
        }

        public static bool GetTSPlayerFromID(int id, out TSPlayer tsp)
        {
            foreach (var t in EternalLand.OnlineTSPlayer)
            {
                if (t.Account != null && t.Account.ID == id)
                {
                    tsp = t;
                    return true;
                }
            }
            tsp = null;
            return false;
        }

        public async static void UpdateInfoToOtherPlayers(EPlayer eplr)
        {
            await Task.Run(() =>
            {
                SetPlayerActive(eplr);
                System.Threading.Thread.Sleep(1000);
                SetCharacter(eplr);
                SetBag(eplr);

                if (eplr.IsInAnotherWorld)
                {
                    eplr.Map.GetAllPlayers().ForEach(e => { if (e != eplr) eplr.SendData(PacketTypes.PlayerUpdate, "", e.Index); });
                    NetMessage.SendData(13, -1, eplr.Index, null, eplr.Index);
                }
                else
                {
                    EternalLand.OnlineEPlayer.ForEach(e => { if (e != eplr) eplr.SendData(PacketTypes.PlayerUpdate, "", e.Index); });
                    NetMessage.SendData(13, -1, eplr.Index, null, eplr.Index);
                }
            });
        }

        public static void SetBag(EPlayer eplr)
        {
            var list = eplr.TempCharacter == null ? eplr.Character.Bag : eplr.TempCharacter.Bag;
            for (int i = 0; i < 260; i++)
            {
                var item = list[i] ?? new EItem();
                EternalLand.OnlineEPlayer.ForEach(e =>
                {
                    if (e != eplr) e.SendRawData(new RawDataWriter().SetType(PacketTypes.PlayerSlot).PackByte((byte)eplr.Index).PackInt16((short)i).PackInt16((short)item.stack).PackByte((byte)item.prefix).PackInt16((short)item.type).GetByteData());
                });
            }
        }

        public static void SetPlayerActive(EPlayer eplr)
        {
            if (MapManager.GetMapFromUUID(eplr.MapUUID, out var map) && eplr.MapUUID != Guid.Empty)
            {
                EternalLand.OnlineEPlayer.ForEach(e =>
                {
                    if (e != eplr)
                    {
                        e.tsp.SendData(PacketTypes.PlayerActive, "", eplr.Index, map.Player.Contains(e.ID) ? 1 : 0);
                        eplr.SendData(PacketTypes.PlayerActive, "", e.Index, map.Player.Contains(e.ID) ? 1 : 0);
                    }
                });
            }
            else //eplr 回到主世界
            {
                EternalLand.OnlineEPlayer.ForEach(e =>
                {
                    if (e != eplr)
                    {
                        e.tsp.SendData(PacketTypes.PlayerActive, "", eplr.Index, e.MapUUID == eplr.MapUUID ? 1 : 0);
                        eplr.SendData(PacketTypes.PlayerActive, "", e.Index, e.MapUUID == eplr.MapUUID ? 1 : 0);
                    }
                });
            }
        }

        public static void SetCharacter(EPlayer eplr)
        {
            NetMessage.SendData(4, -1, -1, null, eplr.Index);
            NetMessage.SendData(16, -1, -1, null, eplr.Index);
            NetMessage.SendData(42, -1, -1, null, eplr.Index);
        }
    }
}
