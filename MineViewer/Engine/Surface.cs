using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cubia
{
    /// <summary>
    /// Function used to produce a border from two adjacent units in a shape.
    /// </summary>
    public delegate F Surfacize<T, F>(T Lower, T Higher, Axis Axis);

    /// <summary>
    /// A complementary collection to an infinite shape. A surface stores a single value
    /// for all borders between the units in an infinite space. In fact, a single unit
    /// in a surface is called a border, so... there.
    /// </summary>
    public interface ISurface<T> : IImmutable
    {
        /// <summary>
        /// Gets a single border at the specified positive direction from the location. The negative
        /// borders can be obtained by subtracting 1 from the location in the required axis.
        /// </summary>
        T Lookup(Vector<int> Location, Axis Direction);
    }

    /// <summary>
    /// A shape that can produce a surface given a surfacization function.
    /// </summary>
    public interface ISurfaceFormableShape<T> : IInfiniteShape<T>
    {
        /// <summary>
        /// Creates a surface from this shape using the specified surfacization function.
        /// </summary>
        ISurface<F> FormSurface<F>(Surfacize<T, F> Surfacization);
    }

    /// <summary>
    /// A surface where the borders that are not of the default value can be enumerated.
    /// </summary>
    public interface IEnumerableSurface<T> : ISurface<T>
    {
        /// <summary>
        /// Gets all non-default borders.
        /// </summary>
        IEnumerable<Border<T>> Borders { get; }

        /// <summary>
        /// The default value for borders.
        /// </summary>
        T Default { get; }
    }

    /// <summary>
    /// A surface that may be converted to an enumerable surface given a default value.
    /// </summary>
    public interface IMultiEnumerableSurface<T> : ISurface<T>
        where T : IEquatable<T>
    {
        /// <summary>
        /// Creates an enumerable surface of all borders that are not the specified default value. This will be null
        /// if there is an infinite amount of such borders.
        /// </summary>
        IEnumerableSurface<T> EnumerateBorders(T Default);
    }

    /// <summary>
    /// Description of a border in a surface.
    /// </summary>
    public struct Border<T>
    {
        /// <summary>
        /// The location of the border.
        /// </summary>
        public Vector<int> Position;

        /// <summary>
        /// The direction of the border.
        /// </summary>
        public Axis Direction;

        /// <summary>
        /// The border's value.
        /// </summary>
        public T Value;

        /// <summary>
        /// Gets a border on a surface.
        /// </summary>
        public static Border<T> Get(ISurface<T> Surface, Vector<int> Position, Axis Direction)
        {
            return new Border<T>() { Position = Position, Direction = Direction, Value = Surface.Lookup(Position, Direction) };
        }
    }

    /// <summary>
    /// An enumerable surface that is contained in a bounded plain.
    /// </summary>
    public interface IBoundedPlaneSurface<T> : IEnumerableSurface<T>
    {
        /// <summary>
        /// The axis of the plane the surface is on.
        /// </summary>
        Axis PlaneAxis { get; }

        /// <summary>
        /// The level of the plane on its axis.
        /// </summary>
        int PlaneLevel { get; }

        /// <summary>
        /// The size, or bounds of the plane.
        /// </summary>
        Point<int> PlaneBound { get; }

        /// <summary>
        /// Looks up a border on the plane.
        /// </summary>
        T PlaneLookup(Point<int> PlaneLocation);
    }

    /// <summary>
    /// Surface related functions.
    /// </summary>
    public static class Surface
    {
        /// <summary>
        /// Produces a surface using the specified source shape and a surfacization function.
        /// </summary>
        public static ISurface<F> Form<T, F>(IInfiniteShape<T> Source, Surfacize<T, F> Surfacization)
            where F : IEquatable<F>
        {
            ISurfaceFormableShape<T> sfs = Source.Extend<ISurfaceFormableShape<T>>();
            if (sfs != null)
            {
                return sfs.FormSurface<F>(Surfacization);
            }

            return new FormSurface<T, F>() { Source = Source, Surfacization = Surfacization };
        }

        /// <summary>
        /// Wrapper used to form a surface from a shape without its own capability to do so.
        /// </summary>
        public class FormSurface<T, F> : ISurface<F>
            where F : IEquatable<F>
        {
            public F Lookup(Vector<int> Location, Axis Direction)
            {
                return this.Surfacization(
                   this.Source.Lookup(Location),
                   this.Source.Lookup(Vector.Add(Location, new Vector<int>(1, 0, 0).AxisOrder(Direction))),
                   Direction);
            }

            public E Extend<E>() 
                where E : class
            {
                if (typeof(E) == typeof(IMultiEnumerableSurface<F>))
                {
                    IEnumerableSurfaceShape<T> ess = this.Source.Extend<IEnumerableSurfaceShape<T>>();
                    if (ess != null)
                    {
                        return (E)(object)new _MultiEnumerable() { Source = ess, Surface = this };
                    }
                }

                return this as E;
            }

            private class _MultiEnumerable : IMultiEnumerableSurface<F>
            {
                public IEnumerableSurface<F> EnumerateBorders(F Default)
                {
                    return this.Source.EnumerateBorders<F>(this.Surface.Surfacization, Default);
                }

                public F Lookup(Vector<int> Location, Axis Direction)
                {
                    return this.Surface.Lookup(Location, Direction);
                }

                public E Extend<E>() 
                    where E : class
                {
                    return this as E;
                }

                public FormSurface<T, F> Surface;
                public IEnumerableSurfaceShape<T> Source;
            }

            public IInfiniteShape<T> Source;
            public Surfacize<T, F> Surfacization; 
        }
    }

    /// <summary>
    /// A finite collection of non-default borders. All borders not in the collection are default. Lookup times are abysmal making this
    /// class not much good for anything other than enumerability.
    /// </summary>
    public class FiniteBorderSurface<T> : ISurface<T>, IEnumerableSurface<T>
    {
        public FiniteBorderSurface(IEnumerable<Border<T>> Borders, T Default)
        {
            this._Borders = Borders;
            this._Default = Default;
        }  

        public IEnumerable<Border<T>> Borders
        {
            get 
            {
                return this._Borders;
            }
        }

        public T Lookup(Vector<int> Location, Axis Direction)
        {
            foreach (Border<T> bord in this._Borders)
            {
                if (Vector.Equal(Location, bord.Position) && bord.Direction == Direction)
                {
                    return bord.Value;
                }
            }
            return this._Default;
        }

        public T Default
        {
            get
            {
                return this._Default;
            }
        }

        public E Extend<E>()
             where E : class
        {
            return this as E;
        }

        private IEnumerable<Border<T>> _Borders;
        private T _Default;
    }
}