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
using zlib;

namespace MineViewer.SMPPackets
{
    public static class PreChunk
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.PreChunk, Call);
        }

        private static void Call()
        {
            int x = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadInt32());
            int y = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadInt32());
            byte load = SMPInterface.Reader.ReadByte();
        }
    }

    public static class BlockChange
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.BlockChange, Call);
        }
        public static int mod(int a, int b)
        {
            return ((a % b) + b) % b;
        }
        public static void Call()
        {
            BinaryReader r = SMPInterface.Reader;
            int x = SMPInterface.SwapByteOrder(r.ReadInt32());
            byte y = r.ReadByte();
            int z = SMPInterface.SwapByteOrder(r.ReadInt32());

            byte type = r.ReadByte();
            byte meta = r.ReadByte();

            int ChunkPosX = -1 + x / MinecraftLevel.ChunkXSize;
            int ChunkPosY = y / MinecraftLevel.ChunkYSize;
            int ChunkPosZ = z / MinecraftLevel.ChunkZSize;

            int cx = (ChunkPosX * MinecraftLevel.ChunkXSize);
            int cy = (ChunkPosY * MinecraftLevel.ChunkYSize);
            int cz = (ChunkPosZ * MinecraftLevel.ChunkZSize);

            MinecraftLevel.Chunk c;
            MinecraftLevel.Chunk ct;
            Cubia.Point<int> p = new Cubia.Point<int>(ChunkPosX, ChunkPosZ);

            if (!SMPInterface.Chunks.TryGetValue(p, out c))
            {
                c = MinecraftLevel.Chunk.EmptyChunk();
                SMPInterface.Chunks.Add(p, c);
            }

            if (!SMPInterface.TransChunks.TryGetValue(p, out ct))
            {
                ct = MinecraftLevel.Chunk.EmptyChunk();
                ct.Trans = true;
                SMPInterface.TransChunks.Add(p, ct);
            }

            MinecraftBlock b = new MinecraftBlock()
            {
                BlockLight = 0,
                SkyLight = 0,
                Type = type
            };

            int rx = mod((x - cx), MinecraftLevel.ChunkXSize); // fixes the negatives
            int ry = mod((y - cy), MinecraftLevel.ChunkYSize);
            int rz = mod((z - cz), MinecraftLevel.ChunkZSize);
            if (GCScheme.IsTrans(type))
                ct.UpdateBlock(new Cubia.Vector<int>(rx, ry, rz), b);
            else
                c.UpdateBlock(new Cubia.Vector<int>(rx, ry, rz), b);
        }
    }

    public static class MultiBlockChange
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.MultiBlockChange, Call);
        }

        private static void Call()
        {
            
            int bx = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadInt32());
            int bz = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadInt32());

            MinecraftLevel.Chunk c;
            MinecraftLevel.Chunk ct;
            Cubia.Point<int> p = new Cubia.Point<int>(bx,bz);
            if (!SMPInterface.Chunks.TryGetValue(p, out c))
            {
                c = MinecraftLevel.Chunk.EmptyChunk();
                SMPInterface.Chunks.Add(p, c);
            }
            if (!SMPInterface.TransChunks.TryGetValue(p, out ct))
            {
                ct = MinecraftLevel.Chunk.EmptyChunk();
                ct.Trans = true;
                SMPInterface.TransChunks.Add(p, ct);
            }

            short arraysize = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadInt16());

            LinkedList<Cubia.Vector<int>> vec = new LinkedList<Cubia.Vector<int>>();
            byte[] blocks = new byte[arraysize];


            for (int i = 0; i < arraysize; i++)
            {
                MapChunkReader r = new MapChunkReader(SMPInterface.Reader.ReadBytes(2)); // 2 for coords, 1 for type
                byte x = (byte)r.Read(4);
                byte z = (byte)r.Read(4);
                byte y = (byte)r.Read(8);
                vec.AddLast(new Cubia.Vector<int>(x, y, z));
            }

            for (int i = 0; i < arraysize; i++)
                blocks[i] = SMPInterface.Reader.ReadByte();

            SMPInterface.Reader.ReadBytes(arraysize); // discard metadata

            for (int t = 0; t < arraysize; t++)
            {
                MinecraftBlock block = new MinecraftBlock()
                {
                    BlockLight = 0,
                    SkyLight = 0,
                    Type = blocks[t]
                };
                if (GCScheme.IsTrans(blocks[t]))
                    ct.UpdateBlock(vec.ElementAt<Cubia.Vector<int>>(t), block);
                else
                    c.UpdateBlock(vec.ElementAt<Cubia.Vector<int>>(t), block);
            }

            
        }
    }

    public static class Chunk
    {
        public static void Init()
        {
            SMPInterface.Subscribe(SMPInterface.PacketTypes.Chunk, Call);
        }

        private static void Call()
        {
            int cx = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadInt32());
            short cy = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadInt16());
            int cz = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadInt32());
            byte sizex = SMPInterface.Reader.ReadByte();
            byte sizey = SMPInterface.Reader.ReadByte();
            byte sizez = SMPInterface.Reader.ReadByte();
            int sizedata = SMPInterface.SwapByteOrder(SMPInterface.Reader.ReadInt32());

            byte[] data = SMPInterface.Reader.ReadBytes(sizedata);
            byte[] ddata;
            DecompressData(data, out ddata);

            //SMPInterface.Debug(string.Format("Got chunk: {0}, {1}, {2}\n", cx, cy, cz));
            ProccesChunk(ddata, cx, cy, cz, sizex, sizey, sizez);
        }

        private static void ProccesChunk(byte[] data, int x, short y, int z, byte sizex, byte sizey, byte sizez)
        {
            MapChunkReader r = new MapChunkReader(data);

            int ChunkPosX = x / MinecraftLevel.ChunkXSize;
            int ChunkPosY = y / MinecraftLevel.ChunkYSize;
            int ChunkPosZ = z / MinecraftLevel.ChunkZSize;

            int cx = (ChunkPosX * MinecraftLevel.ChunkXSize);
            int cy = (ChunkPosY * MinecraftLevel.ChunkYSize);
            int cz = (ChunkPosZ * MinecraftLevel.ChunkZSize);

            MinecraftLevel.Chunk c;
            Cubia.Point<int> p = new Cubia.Point<int>(ChunkPosX, ChunkPosZ);
            if (!SMPInterface.Chunks.TryGetValue(p, out c))
            {
                c = MinecraftLevel.Chunk.EmptyChunk();
                SMPInterface.Chunks.Add(p, c);
            }
            MinecraftLevel.Chunk ct;
            if (!SMPInterface.TransChunks.TryGetValue(p, out ct))
            {
                ct = MinecraftLevel.Chunk.EmptyChunk();
                ct.Trans = true;
                SMPInterface.TransChunks.Add(p, ct);
            }

            for (int ix = x; ix < x + sizex + 1; ix++)
                for (int iz = z; iz < z + sizez + 1; iz++)
                    for (int iy = y; iy < y + sizey + 1; iy++)
                    {
                        int rx = mod((ix - cx), MinecraftLevel.ChunkXSize); // fixes the negatives
                        int ry = mod((iy - cy), MinecraftLevel.ChunkYSize);
                        int rz = mod((iz - cz), MinecraftLevel.ChunkZSize);

                        Cubia.Vector<int> vec = new Cubia.Vector<int>(rx, ry, rz);
                        byte bType = (byte)r.Read(8); // 8 bits in byte
                        MinecraftBlock block = new MinecraftBlock()
                        {
                            BlockLight = 0,
                            SkyLight = 0,
                            Type = bType
                        };
                        if (GCScheme.IsTrans(bType))
                            ct.UpdateBlock(vec, block);
                        else
                            c.UpdateBlock(vec, block);
                    }

        }

        public static int mod(int a, int b)
        {
            return ((a % b) + b) % b;
        }

        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                try
                {
                    output.Write(buffer, 0, len);
                }
                catch { SMPInterface.Debug("Error deflating\n"); }
            }
            output.Flush();
        }

        public static void DecompressData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                try
                {
                    CopyStream(inMemoryStream, outZStream);
                    outZStream.finish();
                    outData = outMemoryStream.ToArray();
                }
                catch { outData = new byte[0]; }
            }
        }
    }
}