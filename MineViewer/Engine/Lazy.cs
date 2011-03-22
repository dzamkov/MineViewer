using System;

namespace Cubia
{
    /// <summary>
    /// I hear that .net 4.0 would implement this, but i'm making my own until that happens. This class allows the
    /// creation of lazy values, values that are computed only when needed and only once.
    /// </summary>
    public class Lazy<V>
    {
        /// <summary>
        /// Creates a lazy value that always has the specified actual value.
        /// </summary>
        public Lazy(V Value)
        {
            this._Cache = Value;
            this._Generator = null;
        }

        /// <summary>
        /// Creates a lazy value based on its generator function (A function that will get the actual value).
        /// </summary>
        public Lazy(Func<V> Generator)
        {
            this._Cache = default(V);
            this._Generator = Generator;
        }

        /// <summary>
        /// Gets the actual value, either by computing it or getting a cached result.
        /// </summary>
        public V Value
        {
            get
            {
                if (this._Generator == null)
                {
                    return this._Cache;
                }
                else
                {
                    this._Cache = this._Generator();
                    this._Generator = null;
                    return this._Cache;
                }
            }
        }

        private V _Cache;
        private Func<V> _Generator;
    }
}