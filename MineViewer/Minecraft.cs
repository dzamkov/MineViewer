using System;
using System.Collections.Generic;
using System.IO;
using Cubia;

namespace MineViewer
{
    /// <summary>
    /// Information about a minecraft block.
    /// </summary>
    public struct MinecraftBlock
    {
        /// <summary>
        /// The type of this block.
        /// </summary>
        public byte Type;

        /// <summary>
        /// How much light the block receives from the sun.
        /// </summary>
        public byte SkyLight;

        /// <summary>
        /// How much light the block receives from lights.
        /// </summary>
        public byte BlockLight;

        /// <summary>
        /// Gets the type for the specified block.
        /// </summary>
        public static byte GetType(MinecraftBlock? Block)
        {
            if (Block.HasValue)
            {
                return Block.Value.Type;
            }
            else
            {
                return DefaultType;
            }
        }

        /// <summary>
        /// Type given to unknown blocks.
        /// </summary>
        public const byte DefaultType = 255;
    }

    /// <summary>
    /// An alpha level from minecraft. The shape gives the minecraft block id's for the blocks
    /// at particular locations. Gives -1 for regions without data. Strangely, minecraft levels
    /// are oriented with the Y axis going up and down (who does that?).
    /// </summary>
    public class MinecraftLevel : IInfiniteShape<MinecraftBlock?>
    {
        public MinecraftLevel(string SourceDirectory, bool Trans)
        {
            this._Source = SourceDirectory;
            this._Chunks = new Dictionary<Point<int>, Chunk?>(Point<int>.EqualityComparer);
            this._Trans = Trans;
        }





        /// <summary>
        /// Represents a chunk of data in the minecraft format.
        /// </summary>
        public struct Chunk : IBoundedShape<MinecraftBlock>
        {
            public Chunk(byte[] Blocks, byte[] BlockLight, byte[] SkyLight, bool bTrans)
            {
                this._Blocks = Blocks;
                this._BlockLight = BlockLight;
                this._SkyLight = SkyLight;
                this.Trans = bTrans;
            }

            public static Chunk EmptyChunk()
            {
                int size = ChunkXSize * ChunkYSize * ChunkZSize;
                Chunk c = new Chunk(
                    new byte[size],
                    null,  // new byte[size],
                    null,
                    false
                );
                return c;
            }


            static public long MakeIndex(Vector<int> pos)
            {
                return pos.X + (pos.Z * ChunkXSize) + (pos.Y * ChunkXSize * ChunkZSize);
            }


            public void UpdateBlock(Vector<int> pos, MinecraftBlock block)
            {
                long ind = MakeIndex(pos);
                this._Blocks[ind] = block.Type;
                //this._BlockLight = block.BlockLight;
                //this._SkyLight = block.SkyLight;
            }


            public bool Trans;


            public MinecraftBlock Lookup(Vector<int> Location)
            {
                long ind = MakeIndex(Location);
                if (_Blocks == null)
                    return new MinecraftBlock();
                byte type = this._Blocks[ind];

                bool isTrans = GCScheme.IsTrans(type);

                if (isTrans == this.Trans)
                    return new MinecraftBlock()
                    {
                        Type = type
                    };
                return new MinecraftBlock()
                {
                    Type = 0
                };
            }


            public Vector<int> Bound
            {
                get
                {
                    return new Vector<int>(ChunkXSize, ChunkYSize, ChunkZSize);
                }
            }


            public T Extend<T>() where T : class
            {
                return this as T;
            }


            private byte[] _Blocks;
            private byte[] _BlockLight;
            private byte[] _SkyLight;
        }





        /// <summary>
        /// Tries loading the chunk at the specified position. This is done automatically
        /// when an area is requested.
        /// </summary>
        public Chunk? LoadChunk(Point<int> Pos)
        {
            Chunk? chunk;
            if (SMPInterface.IsSMP)
            {
                chunk = SMPInterface.GetChunk(Pos, this._Trans);
            }
            else if (!this._Chunks.TryGetValue(Pos, out chunk))
            {
                long regionX = (long)Math.Floor((decimal)Pos.X / 32);
                long regionZ = (long)Math.Floor((decimal)Pos.Y / 32);
                string file = this._Source + Path.DirectorySeparatorChar + "region" + Path.DirectorySeparatorChar + "r." +
                    Convert.ToString(regionX) + "." + Convert.ToString(regionZ) + ".mca";

                if (File.Exists(file))
                {
                    try
                    {
                        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) //File.OpenRead(file))
                        {
                            NBTNamedTag<INBTData> dat = NBT.ReadChunk(fs, Pos);
                            NBTCompound level = (NBTCompound)(((NBTCompound)dat.Data).Data["Level"]).Data;
                            this._Chunks.Add(Pos,
                                chunk = ParseChunkFromNBT(level)
                            );
                        }
                    }
                    catch
                    {
                        this._Chunks.Add(Pos, null);
                    }
                }
                else
                {
                    this._Chunks.Add(Pos, null);
                }
            }
            return chunk;
        }




        /// <summary>
        /// Parses the NBT compound "level" tag into a chunk
        /// </summary>
        Chunk? ParseChunkFromNBT(NBTCompound level)
        {
            byte[] Blocks = new byte[16 * 1256 * 16];
            byte[] BlockLight = new byte[16 * 256 * 16 / 2];
            byte[] SkyLight = new byte[16 * 256 * 16 / 2];
            const int BLOCKS_PER_SECTION = 16 * 16 * 16;
            const int NIBBLES_PER_SECTION = 16 * 16 * 16 / 2;

            // Parse each section:
            NBTList Sections = (NBTList)(level.Data["Sections"].Data);
            foreach (INBTData Section in Sections.Data)
            {
                NBTCompound CompS = (NBTCompound)Section;
                int y = ((NBTByte)(CompS.Data["Y"].Data)).Data;
                byte[] SectionBlocks = ((NBTByteArray)(CompS.Data["Blocks"].Data)).Data;
                Buffer.BlockCopy(SectionBlocks, 0, Blocks, y * BLOCKS_PER_SECTION, BLOCKS_PER_SECTION);
                byte[] SectionBlockLight = ((NBTByteArray)(CompS.Data["BlockLight"].Data)).Data;
                Buffer.BlockCopy(SectionBlockLight, 0, BlockLight, y * NIBBLES_PER_SECTION, NIBBLES_PER_SECTION);
                byte[] SectionSkyLight = ((NBTByteArray)(CompS.Data["BlockLight"].Data)).Data;
                Buffer.BlockCopy(SectionSkyLight, 0, SkyLight, y * NIBBLES_PER_SECTION, NIBBLES_PER_SECTION);
            }  // foreach Section
            return new Chunk(Blocks, BlockLight, SkyLight, this._Trans);
        }





        /// <summary>
        /// Unloads a chunk from memory.
        /// </summary>
        public void UnloadChunk(Point<int> Pos)
        {
            this._Chunks.Remove(Pos);
        }

        /// <summary>
        /// Grabs an area from the level based on the specified chunk locations. This method is usually faster than
        /// using the minecraft level directly as it preloads the required chunks.
        /// </summary>
        public IBoundedShape<MinecraftBlock?> ChunkArea(Point<int> ChunkStart, Point<int> ChunkAmount)
        {
            return new _ChunkAreaShape(this, ChunkStart, ChunkAmount);
        }

        /// <summary>
        /// Loads all the chunks in the specified area.
        /// </summary>
        public void LoadArea(Point<int> ChunkStart, Point<int> ChunkAmount)
        {
            for (int x = 0; x < ChunkAmount.X; x++)
            {
                for (int y = 0; y < ChunkAmount.Y; y++)
                {
                    this.LoadChunk(new Point<int>(x + ChunkStart.X, y + ChunkStart.Y));
                }
            }
        }

        /// <summary>
        /// Unloads all the chunks in the specified area.
        /// </summary>
        public void UnloadArea(Point<int> ChunkStart, Point<int> ChunkAmount)
        {
            for (int x = 0; x < ChunkAmount.X; x++)
            {
                for (int y = 0; y < ChunkAmount.Y; y++)
                {
                    this.UnloadChunk(new Point<int>(x + ChunkStart.X, y + ChunkStart.Y));
                }
            }
        }

        /// <summary>
        /// A shape for a chunk area.
        /// </summary>
        private class _ChunkAreaShape : IBoundedShape<MinecraftBlock?>
        {
            public _ChunkAreaShape(MinecraftLevel Level, Point<int> ChunkStart, Point<int> ChunkAmount)
            {
                this._Chunks = new Chunk?[ChunkAmount.X, ChunkAmount.Y];
                for (int x = 0; x < ChunkAmount.X; x++)
                {
                    for (int y = 0; y < ChunkAmount.Y; y++)
                    {
                        this._Chunks[x, y] = Level.LoadChunk(new Point<int>(x + ChunkStart.X, y + ChunkStart.Y));
                    }
                }
            }

            public MinecraftBlock? Lookup(Vector<int> Location)
            {
                int cx = Location.X / ChunkXSize;
                int cz = Location.Z / ChunkZSize;
                int cix = Location.X - (cx * ChunkXSize);
                int ciz = Location.Z - (cz * ChunkZSize);
                Chunk? chunk = this._Chunks[cx, cz];
                if (chunk.HasValue)
                {
                    return chunk.Value.Lookup(new Vector<int>(cix, Location.Y, ciz));
                }
                else
                {
                    return null;
                }
            }

            public Vector<int> Bound
            {
                get
                {
                    return new Vector<int>(
                            this._Chunks.GetLength(0) * ChunkXSize,
                            ChunkYSize,
                            this._Chunks.GetLength(1) * ChunkZSize);
                }
            }

            public E Extend<E>() where E : class
            {
                return this as E;
            }

            private Chunk?[,] _Chunks;
        }

        public MinecraftBlock? Lookup(Vector<int> Location)
        {
            if (Location.Y < 0 || Location.Y >= ChunkYSize)
            {
                return null;
            }
            else
            {
                int chunkxloc = (Location.X % ChunkXSize + ChunkXSize) % ChunkXSize;
                int chunkyloc = (Location.Z % ChunkZSize + ChunkZSize) % ChunkZSize;

                Point<int> chunkkey = new Point<int>((Location.X - chunkxloc) / ChunkXSize, (Location.Z - chunkyloc) / ChunkZSize);
                Chunk? chunk = this.LoadChunk(chunkkey);
                if (chunk == null)
                {
                    return null;
                }
                else
                {
                    return chunk.Value.Lookup(new Vector<int>(
                            chunkxloc,
                            Location.Y,
                            chunkyloc));

                }
            }
        }

        public T Extend<T>() where T : class
        {
            return this as T;
        }

        private string _Source;
        private Dictionary<Point<int>, Chunk?> _Chunks;
        private bool _Trans;

        public const int ChunkXSize = 1 << ChunkADepth;
        public const int ChunkYSize = 1 << ChunkYDepth;
        public const int ChunkZSize = 1 << ChunkADepth;
        public const int ChunkADepth = 4;
        public const int ChunkYDepth = 8;
    }

}