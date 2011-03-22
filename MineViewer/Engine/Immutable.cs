using System;

namespace Cubia
{
    /// <summary>
    /// An immutable object.
    /// </summary>
    public interface IImmutable
    {
        /// <summary>
        /// Tries "extending" the functionality of the object. For example, some configurations
        /// of shapes are enumberable (have a finite amount of units) while many others aren't. By calling
        /// extend with IMyAwesomeEnumberableShape, you can determine if the shape is enumerable and if
        /// so, use the methods associated with it.
        /// </summary>
        T Extend<T>()
            where T : class;
    }
}