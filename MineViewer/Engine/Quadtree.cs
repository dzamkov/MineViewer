using System;
using System.Collections.Generic;

namespace Cubia
{
    /// <summary>
    /// The lower half of an octree, alternatively, a recursive data structure that efficently organizes values
    /// in 2-d space.
    /// </summary>
    public class Quadtree<T> : HashedRecursiveSpatialStructure<T, Quadtree<T>>
        where T : IEquatable<T>
    {
        private Quadtree(int Depth, int ID, Quadtree<T>[] Children, T Value)
            : base(Depth, ID, Children, Value)
        {

        }

        private class _Setup : HashedRecursiveSpatialStructure<T, Quadtree<T>>.ISetup
        {
            public Quadtree<T> Create(int Depth, int ID, Quadtree<T>[] Children, T Value)
            {
                return new Quadtree<T>(Depth, ID, Children, Value);
            }

            public int Dimension
            {
                get
                {
                    return 2;
                }
            }
        }

        static Quadtree()
        {
            HashedRecursiveSpatialStructure<T, Quadtree<T>>.Setup(new _Setup());
        }

        /// <summary>
        /// Gets the sub quadtree unit at the specified index.
        /// </summary>
        public Quadtree<T> this[int Index]
        {
            get
            {
                return this.Children[Index];
            }
        }

        /// <summary>
        /// Enumerates all units that are not excluded.
        /// </summary>
        public IEnumerable<KeyValuePair<Point<int>, T>> Enumerate(T Excluded)
        {
            return _EnumerateDesc.Get(new _EnumerateDesc() { Excluded = Excluded, Quadtree = this });
        }

        /// <summary>
        /// Description of a subset of values in a quadtree.
        /// </summary>
        private struct _EnumerateDesc
        {
            /// <summary>
            /// The quadtree to search in.
            /// </summary>
            public Quadtree<T> Quadtree;

            /// <summary>
            /// The value to exclude from results.
            /// </summary>
            public T Excluded;

            public override int GetHashCode()
            {
                return this.Quadtree.ID ^ this.Excluded.GetHashCode();
            }

            private struct _EqualityComparer : IEqualityComparer<_EnumerateDesc>
            {
                public bool Equals(_EnumerateDesc x, _EnumerateDesc y)
                {
                    return x.Quadtree == y.Quadtree && x.Excluded.Equals(y.Excluded);
                }

                public int GetHashCode(_EnumerateDesc obj)
                {
                    return obj.GetHashCode();
                }
            }

            /// <summary>
            /// Gets all the values in that satisfy the EnumerateDesc.
            /// </summary>
            public static LinkedList<KeyValuePair<Point<int>, T>> Get(_EnumerateDesc Desc)
            {
                if (Desc.Quadtree.Depth == 0)
                {
                    T val = Desc.Quadtree.Value;
                    if (!val.Equals(Desc.Excluded))
                    {
                        LinkedList<KeyValuePair<Point<int>, T>> li = new LinkedList<KeyValuePair<Point<int>, T>>();
                        li.AddLast(new KeyValuePair<Point<int>, T>(new Point<int>(0, 0), val));
                        return li;
                    }
                    else
                    {
                        return new LinkedList<KeyValuePair<Point<int>, T>>();
                    }
                }
                else
                {
                    LinkedList<KeyValuePair<Point<int>, T>> res;
                    if (!_Cache.TryGetValue(Desc, out res))
                    {
                        res = new LinkedList<KeyValuePair<Point<int>, T>>();
                        int hsize = 1 << (Desc.Quadtree.Depth - 1);
                        for (int x = 0; x < 2; x++)
                        {
                            for (int y = 0; y < 2; y++)
                            {
                                Quadtree<T> child = Desc.Quadtree.Children[x * 2 + y];
                                _EnumerateDesc desc = new _EnumerateDesc() { Quadtree = child, Excluded = Desc.Excluded };
                                LinkedList<KeyValuePair<Point<int>, T>> sublist = Get(desc);
                                foreach (KeyValuePair<Point<int>, T> it in sublist)
                                {
                                    res.AddLast(new KeyValuePair<Point<int>, T>(Point.Add(it.Key, new Point<int>(x * hsize, y * hsize)), it.Value));
                                }
                            }
                        }
                        _Cache[Desc] = res;
                    }
                    else
                    {

                    }
                    return res;
                }
            }

            private static Dictionary<_EnumerateDesc, LinkedList<KeyValuePair<Point<int>, T>>> _Cache = new Dictionary<_EnumerateDesc, LinkedList<KeyValuePair<Point<int>, T>>>(new _EqualityComparer());
        }

        /// <summary>
        /// Gets the size of the quadtree in units.
        /// </summary>
        public int Size
        {
            get
            {
                return 1 << this.Depth;
            }
        }

        /// <summary>
        /// Creates a depth-0 quadtree with a single value.
        /// </summary>
        public static Quadtree<T> Singleton(T Value)
        {
            return HashedRecursiveSpatialStructure<T, Quadtree<T>>.Get(Value);
        }

        /// <summary>
        /// Creates a quadtree with the specified quadtree children.
        /// </summary>
        public static Quadtree<T> Create(int Depth, Quadtree<T>[] Children)
        {
            return HashedRecursiveSpatialStructure<T, Quadtree<T>>.Get(Depth, Children);
        }
    }

    /// <summary>
    /// A bounded enumerable surface create from a quadtree placed on an axis at the specified level.
    /// </summary>
    public class QuadtreeSurface<T> : IBoundedPlaneSurface<T>
        where T : IEquatable<T>
    {
        public QuadtreeSurface(Quadtree<T> Source, T Default, Axis Axis, int Level)
        {
            this._Source = Source;
            this._Axis = Axis;
            this._Level = Level;
            this._Default = Default;
        }

        /// <summary>
        /// Gets the source quadtree for this surface.
        /// </summary>
        public Quadtree<T> Source
        {
            get
            {
                return this._Source;
            }
        }

        public Axis PlaneAxis
        {
            get 
            {
                return this._Axis;
            }
        }

        public int PlaneLevel
        {
            get
            {
                return this._Level;
            }
        }

        public Point<int> PlaneBound
        {
            get 
            {
                int size = this._Source.Size;
                return new Point<int>(size, size);
            }
        }


        public IEnumerable<Border<T>> Borders
        {
            get 
            {
                foreach (KeyValuePair<Point<int>, T> kvp in this._Source.Enumerate(this._Default))
                {
                    yield return new Border<T>()
                    {
                        Position = new Vector<int>(this.PlaneLevel, kvp.Key.X, kvp.Key.Y).AxisUnorder((Axis)this.PlaneAxis),
                        Direction = this.PlaneAxis,
                        Value = kvp.Value
                    };
                }
            }
        }

        public T Default
        {
            get 
            {
                return this._Default;
            }
        }

        public T PlaneLookup(Point<int> PlaneLocation)
        {
            throw new NotImplementedException();
        }

        public T Lookup(Vector<int> Location, Axis Direction)
        {
            throw new NotImplementedException();
        }

        public E Extend<E>()
            where E : class
        {
            return this as E;
        }

        private Quadtree<T> _Source;
        private T _Default;
        private Axis _Axis;
        private int _Level;
    }
}