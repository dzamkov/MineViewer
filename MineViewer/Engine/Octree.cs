using System;
using System.Collections.Generic;

namespace Cubia
{
    /// <summary>
    /// The surface formed by the interior of an octree.
    /// </summary>
    public interface IOctreeInteriorSurface<T> : IEnumerableSurface<T>
        where T : IEquatable<T>
    {
        /// <summary>
        /// Gets the size of the octree that produced this surface.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Gets the "slices" of the interior. The resulting array has the size of
        /// [3 (Axies), this.Size - 1].
        /// </summary>
        Quadtree<T>[,] Slices { get; }
    }

    /// <summary>
    /// You know, an octree, but this one is hashed and works for the specified type.
    /// </summary>
    /// <remarks>Octree's do not like big value types.</remarks>
    /// <typeparam name="T">Data stored with depth 0 octree's.</typeparam>
    public class Octree<T> : 
        HashedRecursiveSpatialStructure<T, Octree<T>>, 
        IBoundedEnumerableSurfaceShape<T>,
        IEnumerableInteriorSurfaceShape<T>,
        IBoundedSliceableSurfaceShape<T>,
        IBoundedShape<T>
        where T : IEquatable<T>
    {
        private Octree(int Depth, int ID, Octree<T>[] Children, T Value)
            : base(Depth, ID, Children, Value)
        {

        }

        static Octree()
        {
            HashedRecursiveSpatialStructure<T, Octree<T>>.Setup(new _Setup());
        }

        private class _Setup : HashedRecursiveSpatialStructure<T, Octree<T>>.ISetup
        {
            public Octree<T> Create(int Depth, int ID, Octree<T>[] Children, T Value)
            {
                return new Octree<T>(Depth, ID, Children, Value);
            }

            public int Dimension
            {
                get
                {
                    return 3;
                }
            }
        }

        /// <summary>
        /// Gets a depth-0 octree with the specified value.
        /// </summary>
        public static Octree<T> Singleton(T Value)
        {
            return HashedRecursiveSpatialStructure<T, Octree<T>>.Get(Value);
        }

        /// <summary>
        /// Creates a solid octree (all depth-0 octree's have the same value).
        /// </summary>
        public static Octree<T> Solid(T Value, int Depth)
        {
            if (Depth == 0)
            {
                return Singleton(Value);
            }
            else
            {
                Octree<T> prev = Solid(Value, Depth - 1);
                Octree<T>[] childrens = new Octree<T>[8];
                for (int t = 0; t < 8; t++)
                {
                    childrens[t] = prev;
                }
                return HashedRecursiveSpatialStructure<T, Octree<T>>.Get(Depth, childrens);
            }
        }

        /// <summary>
        /// Creates an octree representation of the data specified in "Shape" with the
        /// specified depth.
        /// </summary>
        public static Octree<T> Create(IInfiniteShape<T> Shape, int Depth)
        {
            return Create(Shape, new Vector<int>(0, 0, 0), Depth);
        }

        /// <summary>
        /// Creates an octree representation of the data specified in "Shape". Since the size
        /// of an octree must be a power of 2, all extra space is filled with the specified
        /// default value.
        /// </summary>
        public static Octree<T> Create(IBoundedShape<T> Shape, T Default)
        {
            Vector<int> bound = Shape.Bound;
            int mbound = bound.X;
            mbound = mbound < bound.Y ? bound.Y : mbound;
            mbound = mbound < bound.Z ? bound.Z : mbound;
            mbound -= 1;

            int d = 0;
            while (mbound > 0)
            {
                mbound /= 2;
                d++;
            }

            return Octree<T>.Create(Cubia.Shape.Fill(Shape, Default), d);
        }

        /// <summary>
        /// Similar to create, with an additional Starting vector (to indicate the lowest position used from Shape).
        /// </summary>
        public static Octree<T> Create(IInfiniteShape<T> Shape, Vector<int> Start, int Depth)
        {
            if (Depth == 0)
            {
                return Singleton(Shape.Lookup(Start));
            }
            else
            {
                int hsize = _Pow2(Depth - 1);
                Octree<T>[] childrens = new Octree<T>[8];
                int i = 0;
                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        for (int z = 0; z < 2; z++)
                        {
                            childrens[i] = Create(Shape, Vector.Add(Start, new Vector<int>(x * hsize, y * hsize, z * hsize)), Depth - 1);
                            i++;
                        }
                    }
                }
                return HashedRecursiveSpatialStructure<T, Octree<T>>.Get(Depth, childrens);
            }
        }

        /// <summary>
        /// 1 to the power of the specified number.
        /// </summary>
        private static int _Pow2(int Num)
        {
            return (1 << Num);
        }

        /// <summary>
        /// Gets the sub octree unit at the specified index.
        /// </summary>
        public Octree<T> this[int Index]
        {
            get
            {
                return this.Children[Index];
            }
        }

        /// <summary>
        /// Converts a coordinate in this octree to one in one of its children. Only valid for octree's whose depth is over 0.
        /// </summary>
        public void SubCoord(Vector<int> AbsoluteCoord, out Vector<int> ChildCoord, out int ChildIndex)
        {
            _SubCoord(AbsoluteCoord, _Pow2(this.Depth - 1), out ChildCoord, out ChildIndex);
        }

        private void _SubCoord(Vector<int> AbsoluteCoord, int HSize, out Vector<int> ChildCoord, out int ChildIndex)
        {
            ChildIndex = 0;
            ChildCoord = AbsoluteCoord;
            if (AbsoluteCoord.X >= HSize)
            {
                ChildIndex += 4;
                ChildCoord = Vector.Subtract(ChildCoord, new Vector<int>(HSize, 0, 0));
            }
            if (AbsoluteCoord.Y >= HSize)
            {
                ChildIndex += 2;
                ChildCoord = Vector.Subtract(ChildCoord, new Vector<int>(0, HSize, 0));
            }
            if (AbsoluteCoord.Z >= HSize)
            {
                ChildIndex += 1;
                ChildCoord = Vector.Subtract(ChildCoord, new Vector<int>(0, 0, HSize));
            }
        }

        /// <summary>
        /// Gets the size of the octree in units.
        /// </summary>
        public int Size
        {
            get
            {
                return _Pow2((int)this.Depth);
            }
        }

        public T Lookup(Vector<int> Location)
        {
            if (this.Depth == 0)
            {
                return this.Value;
            }
            else
            {
                int ci;
                Vector<int> cc;
                this.SubCoord(Location, out cc, out ci);
                return this[ci].Lookup(cc);
            }
        }

        public Vector<int> Bound
        {
            get
            {
                int size = this.Size;
                return new Vector<int>(size, size, size);
            }
        }

        public IEnumerableSurface<F> EnumerateBorders<F>(Surfacize<T, F> SurfacizeFunc, T Default, F Excluded) 
            where F : IEquatable<F>
        {
            return new OctreeSurface<T, F>(this, SurfacizeFunc, Default, Excluded);
        }

        public IEnumerableSurface<F> EnumerateInteriorBorders<F>(Surfacize<T, F> SurfacizeFunc, F Excluded) 
            where F : IEquatable<F>
        {
            return new OctreeInteriorSurface<T, F>(this, SurfacizeFunc, Excluded);
        }

        public IBoundedPlaneSurface<F> Slice<F>(Surfacize<T, F> SurfacizeFunc, F Default, Axis Axis, int Level) 
            where F : IEquatable<F>
        {
            return new QuadtreeSurface<F>(
                    Octree.GetSlice<T, F>(SurfacizeFunc, this, Axis, Level),
                    Default, Axis, Level);
        }

        public E Extend<E>()
            where E : class
        {
            return this as E;
        }
    }

    /// <summary>
    /// Octree-related functionality.
    /// </summary>
    public static class Octree
    {
        /// <summary>
        /// Array of octree indices such that the child at Indices[Axis, I, 0] is lower in Axis than child at Indices[Axis, I, 1].
        /// </summary>
        public static readonly int[,,] Indices = new int[,,] 
        {
            {
                { 0, 4 },
                { 1, 5 },
                { 2, 6 },
                { 3, 7 }
            },
            {
                { 0, 2 },
                { 4, 6 },
                { 1, 3 },
                { 5, 7 },
            },
            {
                { 0, 1 },
                { 2, 3 },
                { 4, 5 },
                { 6, 7 }
            }
        };

        /// <summary>
        /// The spatial locations of children in an octree.
        /// </summary>
        public static readonly Vector<Polarity>[] Locations = new Vector<Polarity>[]
        {
            new Vector<Polarity>(Polarity.Negative, Polarity.Negative, Polarity.Negative),
            new Vector<Polarity>(Polarity.Negative, Polarity.Negative, Polarity.Positive),
            new Vector<Polarity>(Polarity.Negative, Polarity.Positive, Polarity.Negative),
            new Vector<Polarity>(Polarity.Negative, Polarity.Positive, Polarity.Positive),
            new Vector<Polarity>(Polarity.Positive, Polarity.Negative, Polarity.Negative),
            new Vector<Polarity>(Polarity.Positive, Polarity.Negative, Polarity.Positive),
            new Vector<Polarity>(Polarity.Positive, Polarity.Positive, Polarity.Negative),
            new Vector<Polarity>(Polarity.Positive, Polarity.Positive, Polarity.Positive),
        };

        /// <summary>
        /// Gets the quadtree surface formed by the common sides of two octrees merged together.
        /// </summary>
        public static Quadtree<F> GetSurface<T, F>(Surfacize<T, F> SurfaceizeFunc, Octree<T> Lower, Octree<T> Higher, Axis Axis)
            where T : IEquatable<T>
            where F : IEquatable<F>
        {
            return _SurfaceDesc<T, F>.Get(new _SurfaceDesc<T, F>() { SurfaceizeFunc = SurfaceizeFunc, Lower = Lower, Higher = Higher, Axis = Axis });
        }

        /// <summary>
        /// Gets a quadtree that represents the surface produced by a "slice" of an octree, at the specified level and axis.
        /// </summary>
        public static Quadtree<F> GetSlice<T, F>(Surfacize<T, F> SurfaceizeFunc, Octree<T> Octree, Axis Axis, int Level)
            where T : IEquatable<T>
            where F : IEquatable<F>
        {
            return _SliceDesc<T, F>.Get(new _SliceDesc<T, F>() { SurfaceizeFunc = SurfaceizeFunc, Axis = Axis, Level = Level, Octree = Octree });
        }

        /// <summary>
        /// Gets the interior borders of a surface produced from an octree. This returns an array with size
        /// [3, Octree.Size - 1] for each "slice" through the octree on each axis.
        /// </summary>
        public static Quadtree<F>[,] GetInterior<T, F>(Surfacize<T, F> SurfaceizeFunc, Octree<T> Octree)
            where T : IEquatable<T>
            where F : IEquatable<F>
        {
            if (Octree.Depth == 0)
            {
                return new Quadtree<F>[3, 0];
            }
            else
            {
                _InteriorArg<T, F> arg = new _InteriorArg<T, F>() { Octree = Octree, SurfacizeFunc = SurfaceizeFunc };
                Quadtree<F>[,] res;
                if (!_InteriorArg<T, F>.Cache.Lookup(arg, out res))
                {
                    int s = Octree.Size;
                    int a = s - 1;
                    int h = a / 2;

                    res = new Quadtree<F>[3, a];

                    // Get child interiors
                    var childinteriors = new Quadtree<F>[8][,];
                    for (int c = 0; c < 8; c++)
                    {
                        childinteriors[c] = GetInterior<T, F>(SurfaceizeFunc, Octree[c]);
                    }

                    for (int iax = 0; iax < 3; iax++)
                    {
                        Axis ax = (Axis)iax;

                        // Middle
                        Quadtree<F>[] children = new Quadtree<F>[4];
                        for (int t = 0; t < 4; t++)
                        {
                            children[t] = GetSurface<T, F>(SurfaceizeFunc, Octree[Indices[iax, t, 0]], Octree[Indices[iax, t, 1]], ax);
                        }
                        res[iax, h] = Quadtree<F>.Create(children[0].Depth + 1, children);

                        // Lower
                        for (int l = 0; l < h; l++)
                        {
                            Quadtree<F>[] childrenl = new Quadtree<F>[4];
                            for (int t = 0; t < 4; t++)
                            {
                                childrenl[t] = childinteriors[Indices[iax, t, 0]][iax, l];
                            }
                            res[iax, l] = Quadtree<F>.Create(children[0].Depth + 1, childrenl);
                        }

                        // Higher
                        for (int l = 0; l < h; l++)
                        {
                            Quadtree<F>[] childrenh = new Quadtree<F>[4];
                            for (int t = 0; t < 4; t++)
                            {
                                childrenh[t] = childinteriors[Indices[iax, t, 1]][iax, l];
                            }
                            res[iax, h + l + 1] = Quadtree<F>.Create(children[0].Depth + 1, childrenh);
                        }
                    }

                    _InteriorArg<T, F>.Cache.Store(arg, res);
                }
                return res;
            }
        }

        private struct _InteriorArg<T, F> : IEquatable<_InteriorArg<T, F>>
            where T : IEquatable<T>
            where F : IEquatable<F>
        {
            public Surfacize<T, F> SurfacizeFunc;
            public Octree<T> Octree;

            public override int GetHashCode()
            {
                return this.SurfacizeFunc.GetHashCode() ^ this.Octree.ID;
            }

            public bool Equals(_InteriorArg<T, F> other)
            {
                return this.SurfacizeFunc == other.SurfacizeFunc && this.Octree == other.Octree;
            }

            public static readonly Cache<_InteriorArg<T, F>, Quadtree<F>[,]> Cache = new Cache<_InteriorArg<T, F>, Quadtree<F>[,]>();   
        }

        /// <summary>
        /// Enumerates the borders in the interior of an octree, excluding the specified value.
        /// </summary>
        public static IEnumerable<Border<F>> EnumerateInterior<T, F>(Surfacize<T, F> SurfaceizeFunc, Octree<T> Octree, F Excluded)
            where T : IEquatable<T>
            where F : IEquatable<F>
        {
            var interior = GetInterior<T, F>(SurfaceizeFunc, Octree);
            for (int iax = 0; iax < 3; iax++)
            {
                Axis ax = (Axis)iax;
                for (int l = 0; l < Octree.Size - 1; l++)
                {
                    if (interior[iax, l] != null)
                    {
                        foreach (KeyValuePair<Point<int>, F> kvp in interior[iax, l].Enumerate(Excluded))
                        {
                            yield return new Border<F>() { Position = new Vector<int>(l, kvp.Key.X, kvp.Key.Y).AxisUnorder(ax), Direction = ax, Value = kvp.Value };
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Describes the surface created between two Octrees on a certain axis.
        /// </summary>
        private struct _SurfaceDesc<T, F>
            where T : IEquatable<T>
            where F : IEquatable<F>
        {
            /// <summary>
            /// Function used to produce the surface.
            /// </summary>
            public Surfacize<T, F> SurfaceizeFunc;

            /// <summary>
            /// Lower octree.
            /// </summary>
            public Octree<T> Lower;

            /// <summary>
            /// Higher octree. Must have the same depth as lower.
            /// </summary>
            public Octree<T> Higher;

            /// <summary>
            /// The axis the octrees meet at.
            /// </summary>
            public Axis Axis;

            public override int GetHashCode()
            {
                // Why can't c# just do some bit twiddling and do this itself?
                // Took a while to find a performance problem, wonder what it was?
                // Oh right, dictionaries are reduced to linear complexity with unhashed values, making the get function
                // quadratic instead of sublinear as expected.
                // </comp sci rant>
                int hash = this.SurfaceizeFunc.GetHashCode();
                hash += this.Lower.GetHashCode() ^ (hash << 7) ^ (hash >> 7);
                hash += this.Higher.GetHashCode() ^ (hash << 13) ^ (hash >> 13);
                hash += (int)Axis;
                return hash;
            }

            private struct _EqualityComparer : IEqualityComparer<_SurfaceDesc<T, F>>
            {
                public bool Equals(_SurfaceDesc<T, F> x, _SurfaceDesc<T, F> y)
                {
                    return x.Axis == y.Axis && x.Higher == y.Higher && x.Lower == y.Lower && x.SurfaceizeFunc == y.SurfaceizeFunc;
                }

                public int GetHashCode(_SurfaceDesc<T, F> obj)
                {
                    return obj.GetHashCode();
                }
            }

            /// <summary>
            /// Gets the surface with the specified description.
            /// </summary>
            public static Quadtree<F> Get(_SurfaceDesc<T, F> Surface)
            {
                Quadtree<F> surf;
                if (Surface.Lower.Depth == 0)
                {
                    surf = Quadtree<F>.Singleton(Surface.SurfaceizeFunc(Surface.Lower.Value, Surface.Higher.Value, Surface.Axis));
                }
                else
                {
                    if (!_SurfaceCache.TryGetValue(Surface, out surf))
                    {
                        // RECURSE!
                        int axisind = (int)Surface.Axis;
                        Quadtree<F>[] children = new Quadtree<F>[4];
                        for (int t = 0; t < 4; t++)
                        {
                            _SurfaceDesc<T, F> subsurf = new _SurfaceDesc<T, F>()
                            {
                                Axis = Surface.Axis,
                                Lower = Surface.Lower[Octree.Indices[axisind, t, 1]],
                                Higher = Surface.Higher[Octree.Indices[axisind, t, 0]],
                                SurfaceizeFunc = Surface.SurfaceizeFunc
                            };
                            children[t] = Get(subsurf);
                        }
                        surf = _SurfaceCache[Surface] = Quadtree<F>.Create(Surface.Lower.Depth, children);
                    }
                }
                return surf;
            }

            /// <summary>
            /// Cache of different surfaces.
            /// </summary>
            private static Dictionary<_SurfaceDesc<T, F>, Quadtree<F>> _SurfaceCache = new Dictionary<_SurfaceDesc<T, F>, Quadtree<F>>(new _EqualityComparer());
        }

        /// <summary>
        /// Describes a slice in an octree.
        /// </summary>
        private struct _SliceDesc<T, F>
            where T : IEquatable<T>
            where F : IEquatable<F>
        {
            /// <summary>
            /// Function used to produce the surface.
            /// </summary>
            public Surfacize<T, F> SurfaceizeFunc;

            /// <summary>
            /// The axis the slice is on.
            /// </summary>
            public Axis Axis;

            /// <summary>
            /// The level to make the slice on.
            /// </summary>
            public int Level;

            /// <summary>
            /// The octree to slice on.
            /// </summary>
            public Octree<T> Octree;

            public override int GetHashCode()
            {
                return
                    this.SurfaceizeFunc.GetHashCode() ^
                    ((int)(this.Axis)) ^
                    this.Level ^
                    this.Octree.ID;
            }

            private struct _EqualityComparer : IEqualityComparer<_SliceDesc<T, F>>
            {
                public bool Equals(_SliceDesc<T, F> x, _SliceDesc<T, F> y)
                {
                    return
                        x.SurfaceizeFunc == y.SurfaceizeFunc &&
                        x.Axis == y.Axis &&
                        x.Level == y.Level &&
                        x.Octree == y.Octree;
                }

                public int GetHashCode(_SliceDesc<T, F> obj)
                {
                    return obj.GetHashCode();
                }
            }

            // <summary>
            /// Gets the slice borders for the specified slice description.
            /// </summary>
            public static Quadtree<F> Get(_SliceDesc<T, F> Slice)
            {
                if (Slice.Octree.Depth == 0)
                {
                    throw new ArgumentException();
                }

                Quadtree<F> tree;
                if (!_SliceCache.TryGetValue(Slice, out tree))
                {
                    Axis a = Slice.Axis;
                    int ai = (int)a;
                    int l = Slice.Level;
                    int hs = 1 << (Slice.Octree.Depth - 1);

                    if (l == hs - 1)
                    {
                        Quadtree<F>[] children = new Quadtree<F>[4];
                        for (int r = 0; r < 4; r++)
                        {
                            children[r] = _SurfaceDesc<T, F>.Get(new _SurfaceDesc<T, F>()
                            {
                                Axis = a,
                                Lower = Slice.Octree[Cubia.Octree.Indices[ai, r, 0]],
                                Higher = Slice.Octree[Cubia.Octree.Indices[ai, r, 1]],
                                SurfaceizeFunc = Slice.SurfaceizeFunc
                            });
                        }
                        tree = Quadtree<F>.Create(children[0].Depth + 1, children);
                    }
                    else
                    {
                        int i = l < hs ? 0 : 1;
                        Quadtree<F>[] children = new Quadtree<F>[4];
                        for (int r = 0; r < 4; r++)
                        {
                            children[r] = _SliceDesc<T, F>.Get(new _SliceDesc<T, F>()
                            {
                                Axis = a,
                                Level = l - (i * hs),
                                SurfaceizeFunc = Slice.SurfaceizeFunc,
                                Octree = Slice.Octree[Cubia.Octree.Indices[ai, r, i]]
                            });
                        }
                        tree = Quadtree<F>.Create(children[0].Depth + 1, children);
                    }

                    _SliceCache[Slice] = tree;
                }
                return tree;
            }

            private static Dictionary<_SliceDesc<T, F>, Quadtree<F>> _SliceCache = new Dictionary<_SliceDesc<T, F>, Quadtree<F>>(new _EqualityComparer());
        }
    }

    /// <summary>
    /// The enumerable surface produces by an octree given a surfacization funtion and default value.
    /// </summary>
    public class OctreeSurface<T, F> : IEnumerableSurface<F>
        where T : IEquatable<T>
        where F : IEquatable<F>
    {
        public OctreeSurface(Octree<T> Source, Surfacize<T, F> SurfacizeFunc, T Default, F Excluded)
        {
            this._Source = Source;
            this._SurfacizeFunc = SurfacizeFunc;
            this._Default = Default;
            this._Excluded = Excluded;
        }

        public IEnumerable<Border<F>> Borders
        {
            get 
            {
                // Exterior
                for (int axis = 0; axis < 3; axis++)
                {
                    Quadtree<F> sl = Octree.GetSurface(this._SurfacizeFunc, Octree<T>.Solid(this._Default, this._Source.Depth), this._Source, (Axis)axis);
                    Quadtree<F> sh = Octree.GetSurface(this._SurfacizeFunc, this._Source, Octree<T>.Solid(this._Default, this._Source.Depth), (Axis)axis);
                    foreach (Border<F> b in new QuadtreeSurface<F>(sl, this._Excluded, (Axis)axis, -1).Borders)
                    {
                        yield return b;
                    }
                    foreach (Border<F> b in new QuadtreeSurface<F>(sh, this._Excluded, (Axis)axis, this._Source.Size - 1).Borders)
                    {
                        yield return b;
                    }
                }

                // Interior
                foreach (Border<F> b in Octree.EnumerateInterior<T, F>(this._SurfacizeFunc, this._Source, this._Excluded))
                {
                    yield return b;
                }
            }
        }

        public F Default
        {
            get
            {
                return this._Excluded;
            }
        }

        public F Lookup(Vector<int> Location, Axis Direction)
        {
            throw new NotImplementedException();
        }

        public E Extend<E>() where E : class
        {
            return this as E;
        }  

        private Surfacize<T, F> _SurfacizeFunc;
        private Octree<T> _Source;
        private T _Default;
        private F _Excluded;
    }

    /// <summary>
    /// An enumerable surface produced by the interior borders of an octree.
    /// </summary>
    public class OctreeInteriorSurface<T, F> : IEnumerableSurface<F>, IOctreeInteriorSurface<F>
        where T : IEquatable<T>
        where F : IEquatable<F>
    {
        public OctreeInteriorSurface(Octree<T> Source, Surfacize<T, F> SurfacizeFunc, F Excluded)
        {
            this._SurfacizeFunc = SurfacizeFunc;
            this._Excluded = Excluded;
            this._Source = Source;
        }

        public IEnumerable<Border<F>> Borders
        {
            get 
            {
                return Octree.EnumerateInterior<T, F>(this._SurfacizeFunc, this._Source, this._Excluded);
            }
        }

        public F Default
        {
            get 
            {
                return this._Excluded;
            }
        }

        public int Size
        {
            get 
            {
                return this._Source.Size;
            }
        }

        public Quadtree<F>[,] Slices
        {
            get 
            {
                return Octree.GetInterior<T, F>(this._SurfacizeFunc, this._Source);
            }
        }

        public F Lookup(Vector<int> Location, Axis Direction)
        {
            int s = this._Source.Size;
            if (Location.X >= 0 && Location.Y >= 0 && Location.Z >= 0 &&
                Location.X < s && Location.Y < s && Location.Z < s)
            {
                T l = this._Source.Lookup(Location);
                T h = this._Source.Lookup(Vector.Add(Location, new Vector<int>(1, 0, 0).AxisUnorder(Direction)));
                return this._SurfacizeFunc(l, h, Direction);
            }
            return this._Excluded;
        }

        public E Extend<E>() 
            where E : class
        {
            return this as E;
        }

        private Surfacize<T, F> _SurfacizeFunc;
        private Octree<T> _Source;
        private F _Excluded;
    }
}