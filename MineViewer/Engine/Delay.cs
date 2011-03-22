namespace Cubia
{
    /// <summary>
    /// Another experiment in functional c#, Things that inherit this class do not apply
    /// changes directly. Instead, their operations create a new instance of this class. Usually the new
    /// instance will store the original instance and the parameters for the "change" applied to it.
    /// </summary>
    /// <typeparam name="B">The base class of the delayed system, represents an object.</typeparam>
    /// <typeparam name="D">The data class for the system, where the data is actually stored. Instances of B that
    /// do not inherit D are assumed to just store a "change" and an instance of D.</typeparam>
    /// <remarks>This is confusing, therfore, you get an example. Say you want to store a delayed number. Just wrap
    /// it in a Delay class. B should be something like DelayedNumber while D would be DelayedNumberData. instances of D would store
    /// an actual number while other instances of B would just store modifications on other DelayedNumbers. An example instance of B
    /// would be IncrementDelayedNumber, which would store an instance of D (The last data representation of it). When this instance is
    /// flushed, it would return an instance of D with the number stored in it being one more than the DelayedNumber stored in
    /// IncrementDelayedNumber.
    /// </remarks>
    public abstract class Delay<B, D>
        where B : Delay<B, D>
        where D : B
    {
        /// <summary>
        /// Gets a data representation of the object. This usually involves applying all changes to the
        /// last data representation. You can go ahead and assume that a data representation (instance of D) will
        /// always be faster than a non-data instance of B.
        /// </summary>
        public D Data 
        {
            get
            {
                if (this._Cached != null)
                {
                    return this._Cached;
                }
                else
                {
                    return this._Cached = this.Compute;
                }
            }
        }

        /// <summary>
        /// Destructively applies the changes to the last data representation. This should be tested before
        /// being used, as it breaks functional principles (along with thread safety and possibly absence of bugs) 
        /// in favor of performance.
        /// </summary>
        public D DataUnsafe
        {
            get
            {
                if (this._Cached != null)
                {
                    return this._Cached;
                }
                else
                {
                    return this._Cached = this.ComputeUnsafe;
                }
            }
        }

        /// <summary>
        /// Computes a data representation of the object.
        /// </summary>
        protected abstract D Compute { get; }

        /// <summary>
        /// Computes a data representation of the object, possibly by destructively altering another data object.
        /// </summary>
        protected abstract D ComputeUnsafe { get; }

        private D _Cached;
    }


}