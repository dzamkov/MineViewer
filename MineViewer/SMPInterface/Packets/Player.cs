using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.IO.Compression;
using System.Threading;
using MineViewer;
using MineViewer.SMPPackets;

namespace MineViewer.SMPPackets
{
    public static class PlayerPosLook
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.PlayerPosLook, Call);
        }

        private static void Call()
        {
            SMPInterface.PlayerX = SMPInterface.Reader.ReadDouble();
            double stance = SMPInterface.Reader.ReadDouble();
            SMPInterface.PlayerY = SMPInterface.Reader.ReadDouble();
            SMPInterface.PlayerZ = SMPInterface.Reader.ReadDouble();
            float yaw = SMPInterface.Reader.ReadSingle();
            float pitch = SMPInterface.Reader.ReadSingle();
            bool onground = SMPInterface.Reader.ReadBoolean();
            SMPInterface.Handler.SetOperationCode(SMPInterface.PacketTypes.PlayerPosLook);
            SMPInterface.Handler.Write(SMPInterface.PlayerX);
            SMPInterface.Handler.Write(SMPInterface.PlayerY);
            SMPInterface.Handler.Write(stance);
            SMPInterface.Handler.Write(SMPInterface.PlayerZ);
            SMPInterface.Handler.Write(yaw);
            SMPInterface.Handler.Write(pitch);
            SMPInterface.Handler.Write(onground);
            SMPInterface.Handler.Flush();

            string.Format("Player Pos Look recived @ ({0}, {1}, {2})\n", SMPInterface.PlayerX, SMPInterface.PlayerY, SMPInterface.PlayerZ);
            
        }
    }

    public static class PlayerSpawnPos
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.PlayerSpawnPos, Call);
        }
        public static Action<Cubia.Vector<int>> UpdateSpawnPos;

        private static void Call()
        {
            SMPInterface.PlayerX = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadInt32());
            SMPInterface.PlayerY = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadInt32());
            SMPInterface.PlayerZ = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadInt32());
            SMPInterface.Debug(string.Format("Player Spawn Pos recived @ ({0}, {1}, {2})\n", SMPInterface.PlayerX, SMPInterface.PlayerY, SMPInterface.PlayerZ));
            if (UpdateSpawnPos != null)
            {
                UpdateSpawnPos.Invoke(new Cubia.Vector<int>((int)SMPInterface.PlayerX,
                    (int)SMPInterface.PlayerY,
                    (int)SMPInterface.PlayerZ));
            }
        }
    }

    public static class UpdateHealth
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.UpdateHealth, Call);
        }
        public static Action<Cubia.Vector<int>> UpdateSpawnPos;

        private static void Call()
        {
            short h = SMPInterface.SwapByteOrder((short)SMPInterface.Reader.ReadInt16());
            if (h == 0)
            {
                SMPInterface.Debug("You are now dead");
            }
            else
            {
                SMPInterface.Debug("Health is now " + h.ToString());
            }
        }
    }
}