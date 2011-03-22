using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Cubia;

namespace MineViewer
{
    /// <summary>
    /// Renders an infinite minecraft level, allowing slicing.
    /// </summary>
    public class MinecraftRenderer
    {

        public MinecraftRenderer(MinecraftLevel Source)
        {
            this._Source = Source;
            this.Chunks = new Dictionary<Point<int>, Chunk>(Point<int>.EqualityComparer);
        }

        public E Extend<E>() 
            where E : class
        {
            return this as E;
        }

        /// <summary>
        /// Gets the surface to use to test mouse selecting on.
        /// </summary>
        public ISurface<HitBorder> GetHitSurface(Slice? Slice)
        {
            return new _HitSurface() { Source = this, Slice = Slice };
        }

        private class _HitSurface : ISurface<HitBorder>
        {
            public HitBorder Lookup(Vector<int> Location, Axis Direction)
            {
                Point<int> c;
                Point<int> o;
                Chunk chunk;
                byte lower = this.Source.Lookup(Location, this.Slice, out c, out o, out chunk);
                if (Location.Z < 0 && Direction == Axis.Z)
                {
                    return new HitBorder() { Chunk = c, Polarity = Polarity.Positive };
                }
                Point<int> hc;
                Point<int> ho;
                Chunk hchunk;
                byte higher = this.Source.Lookup(Vector.Add(Location, new Vector<int>(1, 0, 0).AxisUnorder(Direction)), this.Slice, out hc, out ho, out hchunk);
                Material resl = this.Source.CurrentScheme.MaterialBorder(lower, higher, Direction);
                if (resl.Equals(Material.Default))
                {
                    return HitBorder.Default;
                }
                else
                {
                    if (resl.Direction == Polarity.Positive)
                    {
                        return new HitBorder() { Chunk = c, Loaded = chunk != null, Polarity = Polarity.Positive };
                    }
                    else
                    {
                        return new HitBorder() { Chunk = hc, Loaded = hchunk != null, Polarity = Polarity.Negative };
                    }
                }
            }

            public E Extend<E>() 
                where E : class
            {
                return this as E;
            }

            public Slice? Slice;
            public MinecraftRenderer Source;
        }

        /// <summary>
        /// Looks up the block type at the specified location.
        /// </summary>
        public byte Lookup(Vector<int> Location, Slice? Slice, out Point<int> ChunkKey, out Point<int> Offset, out Chunk Chunk)
        {
            ChunkOffset(new Point<int>(Location.X, Location.Y), out ChunkKey, out Offset);
            Chunk = null;
            if (Location.Z >= 0 && Location.Z < ChunkSize)
            {
                Slice? cs;
                if (ChunkInSlice(ChunkKey, Slice, out cs) && this.Chunks.TryGetValue(ChunkKey, out Chunk))
                {
                    Vector<int> vec = new Vector<int>(Offset.X, Offset.Y, Location.Z);
                    if (cs.HasValue)
                    {
                        Slice css = cs.Value;
                        if (vec[css.Axis] <= css.EndLevel && vec[css.Axis] >= css.StartLevel)
                        {
                            return Chunk.Data.Lookup(vec);
                        }
                    }
                    else
                    {
                        return Chunk.Data.Lookup(vec);
                    }
                }
            }
            return MinecraftBlock.DefaultType;
        }

        /// <summary>
        /// A border that may be hit by tracing.
        /// </summary>
        public struct HitBorder : IEquatable<HitBorder>
        {
            /// <summary>
            /// The chunk the border belongs to.
            /// </summary>
            public Point<int>? Chunk;

            /// <summary>
            /// Gets if the associated chunk is currently loaded.
            /// </summary>
            public bool Loaded;

            /// <summary>
            /// The polarity of this border.
            /// </summary>
            public Polarity Polarity;

            public bool Equals(HitBorder other)
            {
                return this.Polarity == other.Polarity && this.Chunk.Equals(other.Chunk);
            }

            public static readonly HitBorder Default = new HitBorder() { Chunk = null, Polarity = Polarity.Negative };
        }

        /// <summary>
        /// Gets a chunk and an offset based on an actual position in the world.
        /// </summary>
        public static void ChunkOffset(Point<int> Location, out Point<int> Chunk, out Point<int> Offset)
        {
            Offset = new Point<int>((Location.X % ChunkSize + ChunkSize) % ChunkSize, (Location.Y % ChunkSize + ChunkSize) % ChunkSize);
            Chunk = new Point<int>((Location.X - Offset.X) / ChunkSize, (Location.Y - Offset.Y) / ChunkSize);
        }
        
        /// <summary>
        /// Unloads the specified chunk and all data associated with it.
        /// </summary>
        public void UnloadChunk(Point<int> ChunkKey)
        {
            Chunk c;
            if (this.Chunks.TryGetValue(ChunkKey, out c))
            {
                c.Dispose();
                this.Chunks.Remove(ChunkKey);
            }
        }

        /// <summary>
        /// Gets or loads the chunk at the specified location.
        /// </summary>
        public Chunk GetChunk(Point<int> ChunkKey)
        {
            Chunk c;
            if (this.Chunks.TryGetValue(ChunkKey, out c))
            {
                return c;
            }
            else
            {
                int xchunk = ChunkSize / MinecraftLevel.ChunkXSize;
                int ychunk = ChunkSize / MinecraftLevel.ChunkZSize;
                Point<int> areastart = new Point<int>(ChunkKey.Y * ychunk, ChunkKey.X * xchunk);
                Point<int> areasize = new Point<int>(ychunk, xchunk);
                c = new Chunk(Octree<byte>.Create(
                        Shape.Transform<MinecraftBlock?, byte>(
                            Shape.Orient<MinecraftBlock?>(
                                Shape.Fill(this._Source.ChunkArea(areastart, areasize), null),
                                Axis.Y), MinecraftBlock.GetType), ChunkDepth));
                this._Source.UnloadArea(areastart, areasize);
                this.Chunks[ChunkKey] = c;
                return c;
            }
        }

        /// <summary>
        /// Renders the current minecraft level.
        /// </summary>
        public void Render(Vector<double> CameraPos, Slice? Slice, Point<int>? Highlight)
        {
            GL.Enable(EnableCap.Light0);
//            GL.Enable(EnableCap.Blend);
            GL.Light(LightName.Light0, LightParameter.Ambient, Color.RGB(0.2, 0.2, 0.2));
            GL.Light(LightName.Light0, LightParameter.Diffuse, Color.RGB(0.6, 0.6, 0.6));
            GL.Light(LightName.Light0, LightParameter.Position, new Vector4(2.0f, 5.0f, 7.8f, 0.0f));

            const int ChunkViewMax = 3;
            int camchunkx = (int)Math.Floor(CameraPos.X / (double)ChunkSize);
            int camchunky = (int)Math.Floor(CameraPos.Y / (double)ChunkSize);
            for (int x = camchunkx - ChunkViewMax; x <= camchunkx + ChunkViewMax; x++)
            {
                for (int y = camchunky - ChunkViewMax; y <= camchunky + ChunkViewMax; y++)
                {
                    Point<int> chunkkey = new Point<int>(x, y);
                    Slice? chunkslice;
                    if (ChunkInSlice(chunkkey, Slice, out chunkslice))
                    {
                        if (Highlight.HasValue && Highlight.Value.Equals(chunkkey))
                        {
                            GL.Light(LightName.Light0, LightParameter.Ambient, Color.RGB(0.3, 0.3, 0.3));
                            GL.Light(LightName.Light0, LightParameter.Diffuse, Color.RGB(0.7, 0.7, 0.7));
                        }
                        else
                        {
                            GL.Light(LightName.Light0, LightParameter.Ambient, Color.RGB(0.2, 0.2, 0.2));
                            GL.Light(LightName.Light0, LightParameter.Diffuse, Color.RGB(0.6, 0.6, 0.6));
                        }
                        Chunk chunk;
                        if (this.Chunks.TryGetValue(chunkkey, out chunk))
                        {
                            GL.PushMatrix();
                            GL.Translate(chunkkey.X * ChunkSize, chunkkey.Y * ChunkSize, 0.0);

                            Chunk[] sides = new Chunk[4];
                            for (int t = 0; t < 4; t++)
                            {
                                Point<int> sidechunkkey = Point.Add(chunkkey, Chunk.SideOffsets[t]);
                                Slice? dummy;
                                Chunk sidechunk = null;
                                if (ChunkInSlice(sidechunkkey, Slice, out dummy) && this.Chunks.TryGetValue(sidechunkkey, out sidechunk))
                                {
                                    sides[t] = sidechunk;
                                }
                            }
                            chunk.Render(sides, chunkslice, this._CurrentScheme);
                            GL.PopMatrix();

                            GL.Begin(BeginMode.Quads);
                            GL.Color4(Color.RGB(0.0, 0.3, 0.6));
                            double xn = chunkkey.X * ChunkSize - 0.5;
                            double xm = (chunkkey.X + 1) * ChunkSize - 0.5;
                            double yn = chunkkey.Y * ChunkSize - 0.5;
                            double ym = (chunkkey.Y + 1) * ChunkSize - 0.5;
                            GL.Vertex3(xn, yn, -0.5);
                            GL.Vertex3(xm, yn, -0.5);
                            GL.Vertex3(xm, ym, -0.5);
                            GL.Vertex3(xn, ym, -0.5);
                            GL.End();
                        }
                        else
                        {
                            GL.Begin(BeginMode.Quads);
                            bool b;
                            if (ChunksSelected.TryGetValue(chunkkey, out b))
                                GL.Color4(Color.RGB(0.2, 0.5, 0.4));
                            else
                                GL.Color4(Color.RGB(0.0, 0.7, 0.6));
                            
                            
                            double xn = chunkkey.X * ChunkSize - 0.5;
                            double xm = (chunkkey.X + 1) * ChunkSize - 0.5;
                            double yn = chunkkey.Y * ChunkSize - 0.5;
                            double ym = (chunkkey.Y + 1) * ChunkSize - 0.5;
                            GL.Vertex3(xn, yn, -0.5);
                            GL.Vertex3(xm, yn, -0.5);
                            GL.Vertex3(xm, ym, -0.5);
                            GL.Vertex3(xn, ym, -0.5);
                            GL.End();
                        }
                    }
                }
            }
        }

        public Dictionary<Point<int>, bool> ChunksSelected = new Dictionary<Point<int>, bool>();

        /// <summary>
        /// Gets if the given chunk is shown in the specified slice. Outputs the chunk-oriented slice
        /// if so.
        /// </summary>
        public bool ChunkInSlice(Point<int> Chunk, Slice? Slice, out Slice? ChunkSlice)
        {
            if (Slice == null)
            {
                ChunkSlice = null;
                return true;
            }
            Slice s = Slice.Value;
            if (s.Axis == Axis.Z)
            {
                ChunkSlice = Slice;
                return true;
            }
            else
            {
                int d = (s.Axis == Axis.X ? Chunk.X : Chunk.Y) * ChunkSize;
                s.StartLevel -= d;
                s.EndLevel -= d;
                if (s.StartLevel < ChunkSize && s.EndLevel >= 0)
                {
                    if (s.StartLevel < 0 && s.EndLevel >= ChunkSize)
                    {
                        ChunkSlice = null;
                    }
                    else
                    {
                        ChunkSlice = new Slice() { Axis = s.Axis, StartLevel = s.StartLevel, EndLevel = s.EndLevel };
                    }
                    return true;
                }
                else
                {
                    ChunkSlice = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// Information of a loaded cubical section in the renderer.
        /// </summary>
        public class Chunk : IDisposable
        {
            public Chunk(Octree<byte> Octree)
            {
                this._DataOctree = Octree;
                this._Cache = new Dictionary<Scheme, RenderCache>();
            }

            /// <summary>
            /// Gets the data for this chunk.
            /// </summary>
            public Octree<byte> Data
            {
                get
                {
                    return this._DataOctree;
                }
            }

            /// <summary>
            /// Gets the render cache for the specified scheme.
            /// </summary>
            private RenderCache _GetCache(Scheme Scheme)
            {
                RenderCache rc;
                if (!this._Cache.TryGetValue(Scheme, out rc))
                {
                    rc = new RenderCache(Scheme, this._DataOctree);
                    this._Cache[Scheme] = rc;
                }
                return rc;
            }

            /// <summary>
            /// Renders the chunk with the specified parameters.
            /// </summary>
            public void Render(Chunk[] Sides, Slice? Slice, Scheme Scheme)
            {
                RenderCache rc = this._GetCache(Scheme);
                Slice s = Slice.GetValueOrDefault(new Slice() { Axis = Axis.Z, StartLevel = 0, EndLevel = ChunkSize });
                if (s.StartLevel < 0)
                {
                    s.StartLevel = 0;
                }
                if (s.EndLevel >= ChunkSize)
                {
                    s.EndLevel = ChunkSize - 1;
                }
                if (s.EndLevel >= s.StartLevel)
                {
                    if (s.StartLevel > 0 || s.EndLevel < ChunkSize - 1)
                    {
                        rc.InteriorRenderer.Render(s.Axis, s.StartLevel, s.EndLevel);
                    }
                    else
                    {
                        rc.InteriorRenderer.Render();
                    }

                    // Here marks the worst code in the program
                    if (s.Axis == Axis.Z)
                    {
                        if (s.EndLevel == ChunkSize - 1)
                        {
                            rc.GetSide(Axis.Z, Polarity.Positive, Axis.Z, this._DataOctree).Render();
                        }
                        if (s.StartLevel == 0 && s.EndLevel == ChunkSize - 1)
                        {
                            for (int t = 0; t < 4; t++)
                            {
                                Axis ax = (Axis)(t % 2);
                                Polarity pol = (Polarity)(t / 2);
                                if (Sides[t] == null)
                                {
                                    rc.GetSide(ax, pol, ax, this._DataOctree).Render();
                                }
                                else
                                {
                                    if (t < 2)
                                    {
                                        rc.GetInteriorSide(ax, Sides[t], ax, this._DataOctree).Render();
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int t = 0; t < 4; t++)
                            {
                                Axis ax = (Axis)(t % 2);
                                Polarity pol = (Polarity)(t / 2);
                                if (Sides[t] == null)
                                {
                                    rc.GetSide(ax, pol, Axis.Z, this._DataOctree).Render(s.StartLevel, 0, s.EndLevel, 2);
                                }
                                else
                                {
                                    if (t < 2)
                                    {
                                        rc.GetInteriorSide(ax, Sides[t], Axis.Z, this._DataOctree).Render(s.StartLevel, 0, s.EndLevel, 2);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Axis oppose = (Axis)(((int)s.Axis + 1) % 2);
                        if (s.StartLevel == 0)
                        {
                            if (Sides[(int)s.Axis + 2] == null)
                            {
                                rc.GetSide(s.Axis, Polarity.Negative, s.Axis, this._DataOctree).Render();
                            }
                        }
                        if (s.EndLevel == ChunkSize - 1)
                        {
                            if (Sides[(int)s.Axis] == null)
                            {
                                rc.GetSide(s.Axis, Polarity.Positive, s.Axis, this._DataOctree).Render();
                            }
                            else
                            {
                                rc.GetInteriorSide(s.Axis, Sides[(int)s.Axis], s.Axis, this._DataOctree).Render();
                            }
                        }
                        if (s.StartLevel == 0 && s.EndLevel == ChunkSize - 1)
                        {
                            rc.GetSide(Axis.Z, Polarity.Positive, Axis.Z, this._DataOctree).Render();
                            if (Sides[(int)oppose + 2] == null)
                            {
                                rc.GetSide(oppose, Polarity.Negative, oppose, this._DataOctree).Render();
                            }
                            if (Sides[(int)oppose] == null)
                            {
                                rc.GetSide(oppose, Polarity.Positive, oppose, this._DataOctree).Render();
                            }
                            else
                            {
                                rc.GetInteriorSide(oppose, Sides[(int)oppose], oppose, this._DataOctree).Render();
                            }
                        }
                        else
                        {
                            rc.GetSide(Axis.Z, Polarity.Positive, s.Axis, this._DataOctree).Render(s.StartLevel, 0, s.EndLevel, 2);
                            if (Sides[(int)oppose + 2] == null)
                            {
                                rc.GetSide(oppose, Polarity.Negative, s.Axis, this._DataOctree).Render(s.StartLevel, 0, s.EndLevel, 2);
                            }
                            if (Sides[(int)oppose] == null)
                            {
                                rc.GetSide(oppose, Polarity.Positive, s.Axis, this._DataOctree).Render(s.StartLevel, 0, s.EndLevel, 2);
                            }
                            else
                            {
                                rc.GetInteriorSide(oppose, Sides[(int)oppose], s.Axis, this._DataOctree).Render(s.StartLevel, 0, s.EndLevel, 2);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// The offsets for each side in a chunk.
            /// </summary>
            public static readonly Point<int>[] SideOffsets = new Point<int>[]
            {
                new Point<int>(1, 0),
                new Point<int>(0, 1),
                new Point<int>(-1, 0),
                new Point<int>(0, -1)
            };

            public void Dispose()
            {
                foreach (KeyValuePair<Scheme, RenderCache> rcs in this._Cache)
                {
                    rcs.Value.Dispose();
                }
            }

            /// <summary>
            /// Data used to render a chunk.
            /// </summary>
            public struct RenderCache : IDisposable
            {
                public RenderCache(Scheme Scheme, Octree<byte> Data)
                {
                    this.Scheme = Scheme;
                    this.InteriorRenderer = new StratifiedRenderer<byte>(Data, Scheme.MaterialBorder, MinecraftBlock.DefaultType);
                    this.InteriorSides = new OrientedPartRenderer[2, 2];
                    this.Sides = new OrientedPartRenderer[5, 2];
                }

                /// <summary>
                /// The block material scheme used.
                /// </summary>
                public Scheme Scheme;

                /// <summary>
                /// Renders the interior of the chunk.
                /// </summary>
                public StratifiedRenderer<byte> InteriorRenderer;

                /// <summary>
                /// Renderers for the interior sides of the chunk. Interior sides are common
                /// between this chunk and its neighbors to (+1, 0) and (0, +1).
                /// </summary>
                public OrientedPartRenderer[,] InteriorSides;

                /// <summary>
                /// Renderers for the side of the chunk that are not occupied by another chunk.
                /// </summary>
                public OrientedPartRenderer[,] Sides;

                /// <summary>
                /// Gets the renderer for a side of the chunk.
                /// </summary>
                public OrientedPartRenderer GetSide(Axis MainDirection, Polarity Polarity, Axis SliceDirection, Octree<byte> Data)
                {
                    if (MainDirection == Axis.Z)
                    {
                        if (Polarity == Polarity.Positive)
                        {
                            if (SliceDirection == Axis.Z)
                            {
                                if (this.Sides[4, 1] != null)
                                {
                                    return this.Sides[4, 1];
                                }
                                return this.GetSide(Axis.Z, Polarity.Positive, Axis.X, Data);
                            }
                            Surfacize<byte, Material> mb = this.Scheme.MaterialBorder;
                            Func<QuadtreeSurface<Material>> createsurface = delegate
                            {
                                Octree<byte> lower = Data;
                                Octree<byte> higher = Octree<byte>.Solid(MinecraftBlock.DefaultType, ChunkDepth);
                                return new QuadtreeSurface<Material>(
                                    Octree.GetSurface<byte, Material>(mb, lower, higher, Axis.Z),
                                    Material.Default, Axis.Z, ChunkSize - 1);
                            };
                            if (SliceDirection == Axis.X)
                            {
                                if (this.Sides[4, 0] == null)
                                {
                                    this.Sides[4, 0] = new OrientedPartRenderer(createsurface(), Axis.X, ChunkSize);
                                }
                                return this.Sides[4, 0];
                            }
                            else
                            {
                                if (this.Sides[4, 1] == null)
                                {
                                    this.Sides[4, 1] = new OrientedPartRenderer(createsurface(), Axis.Y, ChunkSize);
                                }
                                return this.Sides[4, 1];
                            }
                        }
                    }
                    else
                    {
                        int mi = 0;
                        if (MainDirection == Axis.Y)
                        {
                            mi += 1;
                        }
                        if (Polarity == Polarity.Negative)
                        {
                            mi += 2;
                        }
                        if (SliceDirection == MainDirection)
                        {
                            if (this.Sides[mi, 1] != null)
                            {
                                return this.Sides[mi, 1];
                            }
                            else
                            {
                                return this.GetSide(MainDirection, Polarity, Axis.Z, Data);
                            }
                        }
                        Surfacize<byte, Material> mb = this.Scheme.MaterialBorder;
                        Func<QuadtreeSurface<Material>> createsurface = delegate
                        {
                            Octree<byte> lower = Octree<byte>.Solid(MinecraftBlock.DefaultType, ChunkDepth);
                            Octree<byte> higher = Data;
                            if (Polarity == Polarity.Positive)
                            {
                                Octree<byte> temp = lower;
                                lower = higher;
                                higher = temp;
                            }

                            return new QuadtreeSurface<Material>(
                                Octree.GetSurface<byte, Material>(mb, lower, higher, MainDirection),
                                Material.Default, MainDirection, Polarity == Polarity.Positive ? ChunkSize - 1 : -1);
                        };
                        if (SliceDirection == Axis.Z)
                        {
                            if (this.Sides[mi, 0] == null)
                            {
                                this.Sides[mi, 0] = new OrientedPartRenderer(createsurface(), Axis.Z, ChunkSize);
                            }
                            return this.Sides[mi, 0];
                        }
                        else
                        {
                            if (this.Sides[mi, 1] == null)
                            {
                                this.Sides[mi, 1] = new OrientedPartRenderer(createsurface(), (Axis)(((int)MainDirection + 1) % 2), ChunkSize);
                            }
                            return this.Sides[mi, 1];
                        }
                    }

                    return null;
                }

                /// <summary>
                /// Gets the renderer between this and another chunk.
                /// </summary>
                public OrientedPartRenderer GetInteriorSide(Axis Direction, Chunk Side, Axis SliceDirection, Octree<byte> Data)
                {
                    int i = Direction == Axis.X ? 0 : 1;
                    if (SliceDirection == Direction)
                    {
                        if (this.InteriorSides[i, 1] != null)
                        {
                            return this.InteriorSides[i, 1];
                        }
                        else
                        {
                            return GetInteriorSide(Direction, Side, Axis.Z, Data);
                        }
                    }
                    Surfacize<byte, Material> mb = this.Scheme.MaterialBorder;
                    Func<QuadtreeSurface<Material>> createsurface = delegate
                    {
                        Octree<byte> lower = Data;
                        Octree<byte> higher = Side._DataOctree;
                        return new QuadtreeSurface<Material>(
                            Octree.GetSurface<byte, Material>(mb, lower, higher, Direction),
                            Material.Default, Direction, ChunkSize - 1);
                    };
                    if (SliceDirection == Axis.Z)
                    {
                        if (this.InteriorSides[i, 0] == null)
                        {
                            this.InteriorSides[i, 0] = new OrientedPartRenderer(createsurface(), Axis.Z, ChunkSize);
                        }
                        return this.InteriorSides[i, 0];
                    }
                    else
                    {
                        if (this.InteriorSides[i, 1] == null)
                        {
                            this.InteriorSides[i, 1] = new OrientedPartRenderer(createsurface(), (Axis)(((int)Direction + 1) % 2), ChunkSize);
                        }
                        return this.InteriorSides[i, 1];
                    }
                }

                public void Dispose()
                {
                    for (int t = 0; t < 5; t++)
                    {
                        for (int l = 0; l < 2; l++)
                        {
                            if (this.Sides[t, l] != null)
                            {
                                this.Sides[t, l].Dispose();
                            }
                            if (t < 2 && this.InteriorSides[t, l] != null)
                            {
                                this.InteriorSides[t, l].Dispose();
                            }
                        }
                    }
                    this.InteriorRenderer.Dispose();
                }
            }

            private Dictionary<Scheme, RenderCache> _Cache;
            private Octree<byte> _DataOctree;
        }

        /// <summary>
        /// Describes a slicing configuration.
        /// </summary>
        public struct Slice
        {
            /// <summary>
            /// Axis the slice is on.
            /// </summary>
            public Axis Axis;

            /// <summary>
            /// Level the slice starts on.
            /// </summary>
            public int StartLevel;

            /// <summary>
            /// Level the slice ends on.
            /// </summary>
            public int EndLevel;
        }

        /// <summary>
        /// Gets or sets the current scheme used with the minecraft renderer.
        /// </summary>
        public Scheme CurrentScheme
        {
            get
            {
                return this._CurrentScheme;
            }
            set
            {
                this._CurrentScheme = value;
                GCScheme.Schm = this._CurrentScheme;
            }
        }


        public const int ChunkDepth = 7;
        public const int ChunkSize = 1 << ChunkDepth;

        private Scheme _CurrentScheme;
        private MinecraftLevel _Source;
        public Dictionary<Point<int>, Chunk> Chunks;
    }
}