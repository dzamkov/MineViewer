using System;
using System.Collections.Generic;

namespace Cubia
{
    /// <summary>
    /// Caches the results of a mapping from an argument to a result. The contents of a cache are not
    /// guranteed as they may be removed to free memory.
    /// </summary>
    public class Cache<A, R>
        where A : struct, IEquatable<A>
    {
        public Cache()
        {
            this._Dict = new Dictionary<A, R>(_EqualityComparer.Singleton);
        }

        /// <summary>
        /// Looks up an argument to see if it is in the cache. If so, the result is returned.
        /// </summary>
        public bool Lookup(A Argument, out R Result)
        {
            return this._Dict.TryGetValue(Argument, out Result);
        }

        /// <summary>
        /// Stores an argument result pair.
        /// </summary>
        public void Store(A Argument, R Result)
        {
            this._Dict.Add(Argument, Result);
        }

        /// <summary>
        /// Equality comparer for arguments.
        /// </summary>
        private struct _EqualityComparer : IEqualityComparer<A>
        {
            public bool Equals(A x, A y)
            {
                return x.Equals(y);
            }

            public int GetHashCode(A obj)
            {
                return obj.GetHashCode();
            }

            public static readonly _EqualityComparer Singleton = new _EqualityComparer();
        }
       
        private Dictionary<A, R> _Dict;
    }
}