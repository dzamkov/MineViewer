using System;
using System.Collections.Generic;

namespace Cubia
{
    /// <summary>
    /// A structure that stores spatially-organized data recursively. An octree perhaps?
    /// </summary>
    public interface IRecursiveSpatialStructure<T>
    {
        /// <summary>
        /// Gets the depth of the recursive structure. Depth-0 indicates that the
        /// structure only has a single unit of data. A Depth-1 structure is made up of
        /// Depth-0 structures. A Depth-2 structure is made up of Depth-1 structures. etc.
        /// </summary>
        int Depth { get; }

        /// <summary>
        /// Gets the dimension the spatial structure is for.
        /// </summary>
        int Dimension { get; }

        /// <summary>
        /// Gets the value of this structure if it is Depth-0. At any other depth, the value of
        /// this property is undefined.
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Gets the child structure at the specified index. Only valid on Depth-1 and higher
        /// structures.
        /// </summary>
        IRecursiveSpatialStructure<T> GetChild(int Index);
    }

    /// <summary>
    /// A recursive structure using hashing to reduce memory on common patterns.
    /// </summary>
    public abstract class HashedRecursiveSpatialStructure<T, R> : IRecursiveSpatialStructure<T>
        where T : IEquatable<T>
        where R : HashedRecursiveSpatialStructure<T, R>
    {
        protected HashedRecursiveSpatialStructure(int Depth, int ID, R[] Children, T Value)
        {
            this._Depth = Depth;
            this._ID = ID;
            this._Children = Children;
            this._Value = Value;
        }

        public int Dimension
        {
            get
            {
                return _Dimension;
            }
        }

        public int Depth
        {
            get
            {
                return this._Depth;
            }
        }

        public T Value
        {
            get
            {
                return this._Value;
            }
        }

        public IRecursiveSpatialStructure<T> GetChild(int Index)
        {
            return this._Children[Index];
        }

        /// <summary>
        /// Gets the children for this structure.
        /// </summary>
        protected R[] Children
        {
            get
            {
                return this._Children;
            }
        }

        /// <summary>
        /// Gets the ID of this octree unit (which also serves as its hashcode).
        /// </summary>
        public int ID
        {
            get
            {
                return this._ID;
            }
        }

        /// <summary>
        /// Gets if this recursive structure is homogenous (has the same value on all children) and returns
        /// the value if so.
        /// </summary>
        public bool Homogenous(out T Value)
        {
            if (this._Depth == 0)
            {
                Value = this._Value;
                return true;
            }
            for (int t = 1; t < _ChildAmount; t++)
            {
                if (this._Children[t] != this._Children[0])
                {
                    Value = this._Value;
                    return false;
                }
            }
            if (!this._Children[0].Homogenous(out Value))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Setup information for the hash system.
        /// </summary>
        public interface ISetup
        {
            /// <summary>
            /// Creates a new structure.
            /// </summary>
            R Create(int Depth, int ID, R[] Children, T Value);

            /// <summary>
            /// Gets the dimension of the structure.
            /// </summary>
            int Dimension { get; }
        }

        /// <summary>
        /// Gets the depth-0 structure with the specified value.
        /// </summary>
        protected static R Get(T Value)
        {
            int hash = _SingletonValue.ComputeHash(new _SingletonValue() { Value = Value });
            R val = CacheRetreive(Value);
            if (val == null)
            {
                val = _Setup.Create(0, hash, null, Value);
                CacheStore(val);
            }
            return val;
        }

        /// <summary>
        /// Gets a structure with the specified children. The specified array should not change after this call.
        /// </summary>
        protected static R Get(int Depth, R[] Children)
        {
            int hash = _ChildArray.ComputeHash(new _ChildArray() { Children = Children });
            R val = CacheRetreive(Depth, Children);
            if (val == null)
            {
                val = _Setup.Create(Depth, hash, Children, default(T));
                CacheStore(val);
            }
            return val;
        }

        /// <summary>
        /// Sets up the hashed structure.
        /// </summary>
        protected static void Setup(ISetup Info)
        {
            _LowCache = new Dictionary<_SingletonValue, R>(_SingletonValue.EqualityComparer.Singleton);
            _HighCache = new Dictionary<int, Dictionary<_ChildArray, R>>();
            _Setup = Info;
            _Dimension = Info.Dimension;
            _ChildAmount = 1 << _Dimension;
        }

        /// <summary>
        /// Stores a structure in the cache.
        /// </summary>
        protected static void CacheStore(R Structure)
        {
            if (Structure.Depth == 0)
            {
                _LowCache[new _SingletonValue() { Value = Structure._Value }] = Structure;
            }
            else
            {
                Dictionary<_ChildArray, R> hcache;
                if (!_HighCache.TryGetValue(Structure._Depth, out hcache))
                {
                    _HighCache[Structure._Depth] = hcache = new Dictionary<_ChildArray, R>(_ChildArray.EqualityComparer.Singleton);
                }
                hcache[new _ChildArray() { Children = Structure._Children }] = Structure;
            }
        }

        /// <summary>
        /// Tries getting the structure at the specified depth (over 0) with the specified children.
        /// </summary>
        protected static R CacheRetreive(int Depth, R[] Children)
        {
            Dictionary<_ChildArray, R> hcache;
            if (!_HighCache.TryGetValue(Depth, out hcache))
            {
                return null;
            }
            R val;
            if (hcache.TryGetValue(new _ChildArray() { Children = Children }, out val))
            {
                return val;
            }
            return null;
        }

        /// <summary>
        /// Tries getting a depth-0 structure from the cache.
        /// </summary>
        protected static R CacheRetreive(T Value)
        {
            R val;
            if (!_LowCache.TryGetValue(new _SingletonValue() { Value = Value }, out val))
            {
                return null;
            }
            return val;
        }

        /// <summary>
        /// Contains an array of children, for use as a key in caches.
        /// </summary>
        private struct _ChildArray
        {
            public R[] Children;

            /// <summary>
            /// Gets the hash for a child array.
            /// </summary>
            public static int ComputeHash(_ChildArray ChildArray)
            {
                int a = 0x1337BED5;
                for (int t = 0; t < _ChildAmount; t++)
                {
                    int id = ChildArray.Children[t]._ID;
                    a = a ^ id;
                    a = a ^ (a << 3) ^ (a >> 3);
                    a += id;
                }
                return a;
            }

            /// <summary>
            /// Compares equality and stuff.
            /// </summary>
            public class EqualityComparer : IEqualityComparer<_ChildArray>
            {
                private EqualityComparer()
                {

                }

                public bool Equals(_ChildArray x, _ChildArray y)
                {
                    for (int t = 0; t < _ChildAmount; t++)
                    {
                        if (x.Children[t] != y.Children[t])
                        {
                            // put breakpoint here. If it ever triggers, you found yourself a hash collision.
                            // Suggested course of action is to fix the hashing function.
                            return false;
                        }
                    }
                    return true;
                }

                public int GetHashCode(_ChildArray obj)
                {
                    return ComputeHash(obj);
                }

                /// <summary>
                /// Singleton for this class.
                /// </summary>
                public static readonly EqualityComparer Singleton = new EqualityComparer();
            }
        }

        /// <summary>
        /// A single value, for use as keys in a cache.
        /// </summary>
        private struct _SingletonValue
        {
            public T Value;

            /// <summary>
            /// Gets the hash for a singleton value.
            /// </summary>
            public static int ComputeHash(_SingletonValue Value)
            {
                return Value.Value == null ? 0 : Value.Value.GetHashCode();
            }

            /// <summary>
            /// Compares equality and stuff.
            /// </summary>
            public class EqualityComparer : IEqualityComparer<_SingletonValue>
            {
                private EqualityComparer()
                {

                }

                public bool Equals(_SingletonValue x, _SingletonValue y)
                {
                    if (x.Value == null && y.Value == null)
                    {
                        return true;
                    }
                    if (x.Value == null || y.Value == null)
                    {
                        return false;
                    }
                    return x.Value.Equals(y.Value);
                }

                public int GetHashCode(_SingletonValue obj)
                {
                    return ComputeHash(obj);
                }

                /// <summary>
                /// Singleton for this class.
                /// </summary>
                public static readonly EqualityComparer Singleton = new EqualityComparer();
            }
        }

        private int _ID;
        private int _Depth;
        private T _Value;
        private R[] _Children;

        private static int _Dimension;
        private static int _ChildAmount;
        private static ISetup _Setup;
        private static Dictionary<_SingletonValue, R> _LowCache;
        private static Dictionary<int, Dictionary<_ChildArray, R>> _HighCache;
    }
}