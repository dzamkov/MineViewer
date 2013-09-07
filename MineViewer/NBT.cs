using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Cubia;
using zlib;

namespace MineViewer
{
    /// <summary>
    /// Helper class for manipulating the minecraft named binary tag format. The specification for this format can be found in
    /// http://www.minecraft.net/docs/NBT.txt
    /// </summary>
    public static class NBT
    {
        /// <summary>
        /// Reads NBT data from the specified (compressed) stream.
        /// </summary>
        public static NBTNamedTag<INBTData> ReadChunk(Stream fs, Point<int> Pos)
        {
            // Read the chunk from the file.

            byte[] buf = new byte[5];
            long seekOffset = 0;
            int sectorNumber = 0;
            int offset = 0;
            // Read the chunk offset.
            Point<int> chunkOffsetInRegion;
            chunkOffsetInRegion.X = Pos.X % 32;
            if (chunkOffsetInRegion.X < 0)
            {
                chunkOffsetInRegion.X += 32;
            }
            chunkOffsetInRegion.Y = Pos.Y % 32;
            if (chunkOffsetInRegion.Y < 0)
            {
                chunkOffsetInRegion.Y += 32;
            }

            seekOffset = 4 * (chunkOffsetInRegion.X + chunkOffsetInRegion.Y * 32);
            fs.Position = seekOffset;
            fs.Read(buf, 0, 4);
            sectorNumber = (int)buf[3];
            offset = (int)buf[0] << 16 | (int)buf[1] << 8 | (int)buf[2];

            if (offset == 0)
            {
                throw new ArithmeticException();
            }

            // Get the chunk length and version.
            int chunkLength = 0;
            fs.Position = offset * 4096;
            fs.Read(buf, 0, 5);
            chunkLength = (int)buf[0] << 24 | (int)buf[1] << 14 | (int)buf[2] << 8 | (int)buf[3];

            if (chunkLength > sectorNumber * 4096 || chunkLength > CHUNK_DEFLATE_MAX)
            {
                throw new ArithmeticException();
            }

            if (buf[4] != 2)
            {
                throw new ArithmeticException();
            }

            // Read compressed chunk data.
            byte[] inChunk = new byte[CHUNK_DEFLATE_MAX];
            byte[] outChunk = new byte[CHUNK_INFLATE_MAX];

            fs.Read(inChunk, 0, chunkLength - 1);

            fs.Close();

            // Decompress it.
            ZStream z = new ZStream();
            z.next_out = outChunk;
            z.avail_out = CHUNK_INFLATE_MAX;
            z.next_in = inChunk;
            z.avail_in = chunkLength - 1;
            z.inflateInit();
            z.inflate(zlibConst.Z_NO_FLUSH);
            z.inflateEnd();

            System.IO.MemoryStream msUncompressed = new System.IO.MemoryStream(outChunk);

            return NBT.ReadUncompressed(msUncompressed);
        }

        /// <summary>
        /// Reads NBT data from the specified (compressed) stream.
        /// </summary>
        public static NBTNamedTag<INBTData> Read(Stream Stream)
        {
            GZipStream unzipper = new GZipStream(Stream, CompressionMode.Decompress);
            return ReadUncompressed(unzipper);
        }

        /// <summary>
        /// Reads NBT data from an uncompressed stream. Returns null if no data remains.
        /// </summary>
        public static NBTNamedTag<INBTData> ReadUncompressed(Stream Stream)
        {
            byte? type = ReadByte(Stream);
            if (type != null)
            {
                return ReadAsType(Stream, type.Value);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Interprets the remaining stream as a named NBT datum with the specified type.
        /// </summary>
        public static NBTNamedTag<INBTData> ReadAsType(Stream Stream, byte Type)
        {
            string name = ReadString(Stream);
            return new NBTNamedTag<INBTData>() { Data = ReadUnamedAsType(Stream, Type), Name = name };
        }

        /// <summary>
        /// Interprets the remaining stream as an unamed NBT datum with the specified type.
        /// </summary>
        public static INBTData ReadUnamedAsType(Stream Stream, byte Type)
        {
            switch (Type)
            {
                case NBTByte.TypeID:
                    return new NBTByte() { Data = ReadByte(Stream).GetValueOrDefault(0) };
                case NBTShort.TypeID:
                    return new NBTShort() { Data = ReadShort(Stream).GetValueOrDefault(0) };
                case NBTInt.TypeID:
                    return new NBTInt() { Data = ReadInt(Stream).GetValueOrDefault(0) };
                case NBTLong.TypeID:
                    return new NBTLong() { Data = ReadLong(Stream).GetValueOrDefault(0) };
                case NBTFloat.TypeID:
                    return new NBTFloat() { Data = ReadFloat(Stream).GetValueOrDefault(0.0f) };
                case NBTDouble.TypeID:
                    return new NBTDouble() { Data = ReadDouble(Stream).GetValueOrDefault(0.0) };
                case NBTByteArray.TypeID:
                    {
                        int balen = ReadInt(Stream).GetValueOrDefault(0);
                        byte[] buffer = new byte[balen];
                        Stream.Read(buffer, 0, balen);
                        return new NBTByteArray() { Data = buffer };
                    }
                case NBTString.TypeID:
                    {
                        return new NBTString() { Data = ReadString(Stream) };
                    }
                case NBTList.TypeID:
                    {
                        byte subtype = ReadByte(Stream).GetValueOrDefault(0);
                        int len = ReadInt(Stream).GetValueOrDefault(0);
                        List<INBTData> data = new List<INBTData>();
                        for (int t = 0; t < len; t++)
                        {
                            data.Add(ReadUnamedAsType(Stream, subtype));
                        }
                        return new NBTList() { Data = data, SubType = subtype };
                    }
                case NBTCompound.TypeID:
                    {
                        Dictionary<string, NBTNamedTag<INBTData>> children = new Dictionary<string, NBTNamedTag<INBTData>>();
                        while (true)
                        {
                            byte typeid = ReadByte(Stream).GetValueOrDefault(0);
                            if (typeid != 0)
                            {
                                NBTNamedTag<INBTData> datum = ReadAsType(Stream, typeid);
                                children.Add(datum.Name, datum);
                            }
                            else
                            {
                                break;
                            }
                        }
                        return new NBTCompound() { Data = children };
                    }
                case NBTIntArray.TypeID:
                    {
                        int ialen = ReadInt(Stream).GetValueOrDefault(0);
                        Int32[] buffer = new Int32[ialen];
                        for (int i = 0; i < ialen; i++)
                        {
                            buffer[i] = ReadInt(Stream).GetValueOrDefault(0);
                        }
                        return new NBTIntArray() { Data = buffer };
                    }
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a byte of data from the specified stream. This will return null when there is an
        /// end of stream.
        /// </summary>
        public static byte? ReadByte(Stream Stream)
        {
            byte[] buffer = new byte[1];
            int r = Stream.Read(buffer, 0, 1);
            if (r == 1)
            {
                return buffer[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a short of data from the specified stream. This will return null when there is an
        /// end of stream.
        /// </summary>
        public static short? ReadShort(Stream Stream)
        {
            byte[] buffer = new byte[2];
            int r = Stream.Read(buffer, 0, 2);
            if (r == 2)
            {
                if (BitConverter.IsLittleEndian)
                {
                    ByteReverse(buffer);
                }
                return BitConverter.ToInt16(buffer, 0);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a int of data from the specified stream. This will return null when there is an
        /// end of stream.
        /// </summary>
        public static int? ReadInt(Stream Stream)
        {
            byte[] buffer = new byte[4];
            int r = Stream.Read(buffer, 0, 4);
            if (r == 4)
            {
                if (BitConverter.IsLittleEndian)
                {
                    ByteReverse(buffer);
                }
                return BitConverter.ToInt32(buffer, 0);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a long of data from the specified stream. This will return null when there is an
        /// end of stream.
        /// </summary>
        public static long? ReadLong(Stream Stream)
        {
            byte[] buffer = new byte[8];
            int r = Stream.Read(buffer, 0, 8);
            if (r == 8)
            {
                if (BitConverter.IsLittleEndian)
                {
                    ByteReverse(buffer);
                }
                return BitConverter.ToInt64(buffer, 0);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a float of data from the specified stream. This will return null when there is an
        /// end of stream.
        /// </summary>
        public static float? ReadFloat(Stream Stream)
        {
            byte[] buffer = new byte[4];
            int r = Stream.Read(buffer, 0, 4);
            if (r == 4)
            {
                if (BitConverter.IsLittleEndian)
                {
                    ByteReverse(buffer);
                }
                return BitConverter.ToSingle(buffer, 0);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a double of data from the specified stream. This will return null when there is an
        /// end of stream.
        /// </summary>
        public static double? ReadDouble(Stream Stream)
        {
            byte[] buffer = new byte[8];
            int r = Stream.Read(buffer, 0, 8);
            if (r == 8)
            {
                if (BitConverter.IsLittleEndian)
                {
                    ByteReverse(buffer);
                }
                return BitConverter.ToDouble(buffer, 0);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a UTF-8 string from the stream.
        /// </summary>
        public static string ReadString(Stream Stream)
        {
            short len = (short)ReadShort(Stream).GetValueOrDefault(0);
            byte[] buffer = new byte[len];
            Stream.Read(buffer, 0, len);
            return UTF8Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Reverses all bytes in the specified buffer.
        /// </summary>
        public static void ByteReverse(byte[] Buffer)
        {
            int len = Buffer.Length;
            for (int t = 0; t < len / 2; t++)
            {
                byte temp = Buffer[t];
                Buffer[t] = Buffer[len - t - 1];
                Buffer[len - t - 1] = temp;
            }
        }

        public const int CHUNK_DEFLATE_MAX = 1024 * 512;   // 512 KiB limit for compressed chunks
        public const int CHUNK_INFLATE_MAX = 1024 * 1024;  // 1024 KiB limit for inflated chunks
    }

    /// <summary>
    /// Data that can be stored in the NBT format.
    /// </summary>
    public interface INBTData
    {

    }

    /// <summary>
    /// Signed byte.
    /// </summary>
    public struct NBTByte : INBTData
    {
        public byte Data;
        public const int TypeID = 1;
    }

    /// <summary>
    /// Signed short.
    /// </summary>
    public struct NBTShort : INBTData
    {
        public short Data;
        public const int TypeID = 2;
    }

    /// <summary>
    /// Signed int.
    /// </summary>
    public struct NBTInt : INBTData
    {
        public int Data;
        public const int TypeID = 3;
    }

    /// <summary>
    /// Signed long.
    /// </summary>
    public struct NBTLong : INBTData
    {
        public long Data;
        public const int TypeID = 4;
    }

    /// <summary>
    /// 32 bit float.
    /// </summary>
    public struct NBTFloat : INBTData
    {
        public float Data;
        public const int TypeID = 5;
    }

    /// <summary>
    /// 64 bit float.
    /// </summary>
    public struct NBTDouble : INBTData
    {
        public double Data;
        public const int TypeID = 6;
    }

    /// <summary>
    /// Byte array.
    /// </summary>
    public struct NBTByteArray : INBTData
    {
        public byte[] Data;
        public const int TypeID = 7;
    }

    /// <summary>
    /// Int32 array.
    /// </summary>
    public struct NBTIntArray : INBTData
    {
        public Int32[] Data;
        public const int TypeID = 11;
    }

    /// <summary>
    /// UTF-8 string.
    /// </summary>
    public struct NBTString : INBTData
    {
        public string Data;
        public const int TypeID = 8;
    }

    /// <summary>
    /// Homogenous list of unamed data.
    /// </summary>
    public struct NBTList : INBTData
    {
        public byte SubType;
        public List<INBTData> Data;
        public const int TypeID = 9;
    }

    /// <summary>
    /// Heterogenous set of named data.
    /// </summary>
    public struct NBTCompound : INBTData
    {
        public Dictionary<string, NBTNamedTag<INBTData>> Data;
        public const int TypeID = 10;
    }

    /// <summary>
    /// Data wrapped in a named tag with the NBT format.
    /// </summary>
    /// <typeparam name="D">The type of data stored.</typeparam>
    public class NBTNamedTag<D>
        where D : INBTData
    {
        /// <summary>
        /// The actual data in this tag.
        /// </summary>
        public D Data;

        /// <summary>
        /// The name of this data.
        /// </summary>
        public string Name;
    }
}