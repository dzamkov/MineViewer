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

namespace MineViewer.SMPInsan
{
    public static class Weird
    {
        static public void UnpackMetadata()
        {
            BinaryReader r = SMPInterface.Reader;
            byte i = r.ReadByte();

            while (i != 127)
            {
                int j = (i & 0xE0) >> 5;
                int k = i & 0x1F;

                switch (j)
                {
                    case 0:
                        r.ReadByte();
                        break;
                    case 1:
                        r.ReadInt16();
                        break;
                    case 2:
                        r.ReadInt32();
                        break;
                    case 3:
                        r.ReadSingle();
                        break;
                    case 4:
                        r.ReadByte();
                        r.ReadString();
                        break;
                    case 5:
                        r.ReadInt16();
                        r.ReadByte();
                        r.ReadInt16();
                        break;
                }

                i = r.ReadByte();
            }
        }
    }
}

namespace MineViewer.SMPPackets
{

    public static class KeepAlive
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.KeepAlive, Call);
        }

        private static void Call()
        {
            SMPInterface.Handler.SetOperationCode(SMPInterface.PacketTypes.KeepAlive);
            SMPInterface.Handler.Flush();
        }
    }

    public static class MobSpawn
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.MobSpawn, Call);
        }

        private static void Call()
        {
            BinaryReader r = SMPInterface.Reader;
            r.ReadBytes(sizeof(int) + sizeof(byte) + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(byte) + sizeof(byte));
            MineViewer.SMPInsan.Weird.UnpackMetadata();
        }
    }

    public static class Unknown2
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.Unknown2, Call);
        }

        private static void Call()
        {
            BinaryReader r = SMPInterface.Reader;
            r.ReadInt32();
            r.ReadByte(); r.ReadString();
            r.ReadBytes(sizeof(int) * 4);
        }
    }

    public static class UnknownMetadata
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.UnknownMetadata, Call);
        }

        private static void Call()
        {
            BinaryReader r = SMPInterface.Reader;
            r.ReadInt32();
            MineViewer.SMPInsan.Weird.UnpackMetadata();
        }
    }

    public static class SetSlot
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.SetSlot, Call);
        }

        private static void Call()
        {
            BinaryReader r = SMPInterface.Reader;
            byte windowid = r.ReadByte();
            short slot = SMPInterface.SwapByteOrder(r.ReadInt16());
            short itemid = SMPInterface.SwapByteOrder(r.ReadInt16());
            if (itemid != -1)
            {
                r.ReadBytes(3); // count and uses
            }
        }
    }

    public static class OpenWindow
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.OpenWindow, Call);
        }

        private static void Call()
        {
            SMPInterface.Reader.ReadBytes(3);
            SMPInterface.Reader.ReadString();
            SMPInterface.Reader.ReadByte();
        }
    }

    public static class WindowItems
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.WindowItems, Call);
        }

        private static void Call()
        {
            byte type = SMPInterface.Reader.ReadByte();
            short count = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadInt16());
            int totalBlocks = 0;
            for (int i = 0; i < count; i++)
            {
                int bid = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadInt16());
                if (bid != -1)
                {
                    byte amm = SMPInterface.Reader.ReadByte();
                    totalBlocks += amm;
                    SMPInterface.Reader.ReadInt16(); // uses
                }
            }
            //SMPInterface.Reader.ReadBytes(2);
            SMPInterface.Debug("Got Inv, " + totalBlocks.ToString() + " blocks in total\n");
        }
    }

    public static class Asploide
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.Explodie, Call);
        }

        private static void Call()
        {
            double exx = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadDouble());
            double exy = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadDouble());
            double exz = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadDouble());
            SMPInterface.Reader.ReadSingle();
            int blocksaffected = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadInt32());
            SMPInterface.Reader.ReadBytes(blocksaffected * 3);
            SMPInterface.Debug(string.Format("ASPLODIE AT: {0}, {1}, {2}\n", exx, exy, exz));
        }
    }

    public static class NamedEntitySpawned
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.NamedEntitySpawn, Call);
        }

        private static void Call()
        {
            int pid = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadInt32());
            SMPInterface.Reader.ReadByte();
            string name = SMPInterface.Reader.ReadString();
            SMPInterface.Reader.ReadInt32(); //x
            SMPInterface.Reader.ReadInt32();
            SMPInterface.Reader.ReadInt32(); // z
            SMPInterface.Reader.ReadByte(); // rotation
            SMPInterface.Reader.ReadByte();
            SMPInterface.Reader.ReadInt16(); // current item
            SMPInterface.Debug("Player: " + name + "\n");
        }
    }

    public static class UpdateSign
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.UpdateSign, Call);
        }

        private static void Call()
        {
            SMPInterface.Reader.ReadInt32();
            SMPInterface.Reader.ReadInt16();
            SMPInterface.Reader.ReadInt32();
            SMPInterface.Reader.ReadByte(); SMPInterface.Reader.ReadString();
            SMPInterface.Reader.ReadByte(); SMPInterface.Reader.ReadString();
            SMPInterface.Reader.ReadByte(); SMPInterface.Reader.ReadString();
            SMPInterface.Reader.ReadByte(); SMPInterface.Reader.ReadString();
        }
    }
}