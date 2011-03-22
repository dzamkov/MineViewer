using System;
using System.Collections.Generic;

namespace Cubia
{
    /// <summary>
    /// A infinite spatially-organized collection of the specified type.
    /// </summary>
    public interface IInfiniteShape<T> : IImmutable
    {
        /// <summary>
        /// Gets the value at the specified location.
        /// </summary>
        T Lookup(Vector<int> Location);
    }

    /// <summary>
    /// A bounded spatially-organized collection of the specified type.
    /// </summary>
    public interface IBoundedShape<T> : IImmutable
    {
        /// <summary>
        /// Gets the value at the specified location.
        /// </summary>
        T Lookup(Vector<int> Location);

        /// <summary>
        /// Gets the maximum position for which this shape has data.
        /// </summary>
        Vector<int> Bound { get; }
    }

    /// <summary>
    /// A shape that can enumerate all its borders (connections between units) that are not of a certain exluded value.
    /// based on a given function.
    /// </summary>
    public interface IEnumerableSurfaceShape<T> : IInfiniteShape<T>
    {
        /// <summary>
        /// As previously stated, enumerates all non-excluded borders based on a border generation function. Returns
        /// null if the amount of borders are infinite.
        /// </summary>
        IEnumerableSurface<F> EnumerateBorders<F>(Surfacize<T, F> SurfacizeFunc, F Exluded) 
            where F : IEquatable<F>;
    }

    /// <summary>
    /// A bounded shape that can enumerate all its borders (connections between units) that are not of a certain excluded value. Technically, all
    /// bounded shapes should have this functionality but OH WELL.
    /// </summary>
    public interface IBoundedEnumerableSurfaceShape<T> : IBoundedShape<T>
    {
        /// <summary>
        /// Enumerates all non-null borders based on a border generation function. All values outside the bounds
        /// of the shape should be assumed to be Default.
        /// </summary>
        IEnumerableSurface<F> EnumerateBorders<F>(Surfacize<T, F> SurfacizeFunc, T Default, F Excluded)
            where F : IEquatable<F>;
    }

    /// <summary>
    /// A bounded shape that can enumerate all its interior borders that are not of a certain excluded value.
    /// </summary>
    public interface IEnumerableInteriorSurfaceShape<T> : IBoundedShape<T>
    {
        /// <summary>
        /// Enumerates all interior borders that are not excluded within the bounded shape.
        /// </summary>
        IEnumerableSurface<F> EnumerateInteriorBorders<F>(Surfacize<T, F> SurfacizeFunc, F Excluded)
            where F : IEquatable<F>;
    }

    /// <summary>
    /// A bounded shape that can have slices made into surfaces. A slice is a parallel plane between units that extends
    /// the entirety of the bounded shape on two axies.
    /// </summary>
    public interface IBoundedSliceableSurfaceShape<T> : IBoundedShape<T>
    {
        /// <summary>
        /// Slices the bounded shape on a plane perpendicular to the specified axis with that axis having a value of Level. A 
        /// surfacize function is used to create the borders. All borders outside the plain are given the default value. The return
        /// value will be a surface that can enumerate all borders that are not the default value. This means every enumerated
        /// border will have a non-default value, have the specified axis, and has a position whose component on the specified axis
        /// equals Level.
        /// </summary>
        IBoundedPlaneSurface<F> Slice<F>(Surfacize<T, F> SurfacizeFunc, F Default, Axis Axis, int Level)
            where F : IEquatable<F>;
    }

    /// <summary>
    /// A shape that can be oriented on an axis.
    /// </summary>
    public interface IOrientableInfiniteShape<T> : IInfiniteShape<T>
    {
        /// <summary>
        /// Orients the shape so that the x axis is transformed to the specified axis.
        /// </summary>
        IInfiniteShape<T> Orient(Axis Axis);
    }

    /// <summary>
    /// A shape where a mapping of values can be applied.
    /// </summary>
    public interface ITransformableInfiniteShape<T> : IInfiniteShape<T>
    {
        /// <summary>
        /// Transforms the shape based on a mapping function.
        /// </summary>
        IInfiniteShape<F> Transform<F>(Func<T, F> Mapping);
    }

    /// <summary>
    /// A bounded shape that can be "filled" and converted into a filled infinite shape.
    /// </summary>
    public interface IFillableBoundedShape<T> : IBoundedShape<T>
    {
        /// <summary>
        /// Creates a filled infinite shape using this bounded shape and the specified default
        /// value.
        /// </summary>
        IFilledInfiniteShape<T> Fill(T Default);
    }

    /// <summary>
    /// A shape where every value outside a bounded shape will be the default value.
    /// </summary>
    public interface IFilledInfiniteShape<T> : IInfiniteShape<T>
    {
        /// <summary>
        /// The source bounded shape for the filled infinite shape.
        /// </summary>
        IBoundedShape<T> Source { get; }

        /// <summary>
        /// The default value assigned to all locations not in the bounded shape.
        /// </summary>
        T Default { get; }
    }

    /// <summary>
    /// An infinite shape where a finite portion can be examined as a bounded shape.
    /// </summary>
    public interface IDivisibleInfiniteShape<T> : IInfiniteShape<T>
    {
        /// <summary>
        /// Gets the bounded shape at the starting position with the specified size.
        /// </summary>
        IBoundedShape<T> Subsection(Vector<int> Size, Vector<int> Start);
    }

    /// <summary>
    /// Shape related functions.
    /// </summary>
    public static class Shape
    {
        /// <summary>
        /// "Slices" the specified shape, returning a plane of the borders on the specified axis
        /// and level.
        /// </summary>
        public static IBoundedPlaneSurface<F> Slice<T, F>(IBoundedShape<T> Source, Axis Axis, int Level, Surfacize<T, F> SurfacizeFunc, F Excluded)
            where F : IEquatable<F>
        {
            IBoundedSliceableSurfaceShape<T> bsss = Source.Extend<IBoundedSliceableSurfaceShape<T>>();
            if (bsss != null)
            {
                return bsss.Slice(SurfacizeFunc, Excluded, Axis, Level);
            }

            // Too sleepy to do this right now...
            throw new NotImplementedException();
        }

        /// <summary>
        /// Enumerates the interior surfaces in a bounded shape.
        /// </summary>
        public static IEnumerableSurface<F> EnumerateInterior<T, F>(IBoundedShape<T> Source, Surfacize<T, F> SurfacizeFunc, F Excluded)
            where F : IEquatable<F>
        {
            IEnumerableInteriorSurfaceShape<T> eiss = Source.Extend<IEnumerableInteriorSurfaceShape<T>>();
            if (eiss != null)
            {
                return eiss.EnumerateInteriorBorders(SurfacizeFunc, Excluded);
            }

            return new EnumerableInteriorSurface<T, F>(Source, SurfacizeFunc, Excluded); 
        }

        /// <summary>
        /// Interior surface of a bounded shape.
        /// </summary>
        public class EnumerableInteriorSurface<T, F> : IEnumerableSurface<F>
            where F : IEquatable<F>
        {
            public EnumerableInteriorSurface(IBoundedShape<T> Source, Surfacize<T, F> SurfacizeFunc, F Excluded)
            {
                this._Source = Source;
                this._SurfacizeFunc = SurfacizeFunc;
                this._Excluded = Excluded;
            }

            public IEnumerable<Border<F>> Borders
            {
                get 
                {
                    Vector<int> bounds = this._Source.Bound;
                    for (int x = 0; x < bounds.X - 1; x++)
                    {
                        for (int y = 0; y < bounds.Y - 1; y++)
                        {
                            for (int z = 0; z < bounds.Z - 1; z++)
                            {
                                Vector<int> pos = new Vector<int>(x, y, z);
                                for (int iax = 0; iax < 3; iax++)
                                {
                                    Axis ax = (Axis)iax;
                                    F s = this._SurfacizeFunc(this._Source.Lookup(pos), this._Source.Lookup(Vector.Add(pos, new Vector<int>(1, 0, 0).AxisUnorder(ax))), ax);
                                    if (!s.Equals(this._Excluded))
                                    {
                                        yield return new Border<F>() { Position = pos, Direction = ax, Value = s };
                                    }
                                }
                            }
                        }
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
                Vector<int> bounds = this._Source.Bound;
                if (Location.X >= 0 && Location.Y >= 0 && Location.Z >= 0 &&
                    Location.X < bounds.X && Location.Y < bounds.Y && Location.Z < bounds.Z &&
                    Location[Direction] < bounds[Direction] - 1)
                {
                    return this._SurfacizeFunc(this._Source.Lookup(Location), this._Source.Lookup(Vector.Add(new Vector<int>(1, 0, 0).AxisUnorder(Direction), Location)), Direction);
                }
                else
                {
                    return this._Excluded;
                }
            }

            public E Extend<E>() 
                where E : class
            {
                return this as E;
            }

            private IBoundedShape<T> _Source;
            private Surfacize<T, F> _SurfacizeFunc;
            private F _Excluded;
        }

        /// <summary>
        /// Reorients a shape so that the x axis is transformed to the specified axis.
        /// </summary>
        public static IInfiniteShape<T> Orient<T>(IInfiniteShape<T> Source, Axis Axis)
        {
            IOrientableInfiniteShape<T> ois = Source.Extend<IOrientableInfiniteShape<T>>();
            if (ois != null)
            {
                return ois.Orient(Axis);
            }

            return new OrientedInfiniteShape<T>() { Source = Source, Axis = Axis };
        }

        /// <summary>
        /// Gets the unit for a spatial location.
        /// </summary>
        public static Vector<int> Unit(Vector<double> Space)
        {
            return new Vector<int>((int)Math.Round(Space.X), (int)Math.Round(Space.Y), (int)Math.Round(Space.Z));
        }

        /// <summary>
        /// Wrapper used to orient a shape without its own capability to do so.
        /// </summary>
        public class OrientedInfiniteShape<T> : IOrientableInfiniteShape<T>
        {
            public IInfiniteShape<T> Orient(Axis Axis)
            {
                return new OrientedInfiniteShape<T>() { Axis = (Axis)(((int)this.Axis + (int)Axis) % 3), Source = this.Source };
            }

            public T Lookup(Vector<int> Location)
            {
                return this.Source.Lookup(Location.AxisOrder(this.Axis));
            }

            public E Extend<E>() where E : class
            {
                return this as E;
            }

            public Axis Axis;
            public IInfiniteShape<T> Source;
        }

        /// <summary>
        /// Transforms a shape by applying a mapping on all its values.
        /// </summary>
        public static IInfiniteShape<F> Transform<T, F>(IInfiniteShape<T> Source, Func<T, F> Mapping)
        {
            ITransformableInfiniteShape<T> tis = Source.Extend<ITransformableInfiniteShape<T>>();
            if (tis != null)
            {
                return tis.Transform<F>(Mapping);
            }

            return new TransformedInfiniteShape<T, F>() { Source = Source, Mapping = Mapping };
        }

        /// <summary>
        /// Wrapper used to transform a shape without its own capability to do so.
        /// </summary>
        public class TransformedInfiniteShape<T, F> : ITransformableInfiniteShape<F>
        {
            public IInfiniteShape<N> Transform<N>(Func<F, N> Mapping)
            {
                return new TransformedInfiniteShape<T, N>() { Mapping = x => Mapping(this.Mapping(x)), Source = this.Source };
            }

            public F Lookup(Vector<int> Location)
            {
                return this.Mapping(this.Source.Lookup(Location));
            }

            public E Extend<E>() where E : class
            {
                return this as E;
            }

            public Func<T, F> Mapping;
            public IInfiniteShape<T> Source;
        }

        /// <summary>
        /// Creates a filled infinite shape with the specified parameters.
        /// </summary>
        public static IFilledInfiniteShape<T> Fill<T>(IBoundedShape<T> Source, T Default)
        {
            IFillableBoundedShape<T> fbs = Source.Extend<IFillableBoundedShape<T>>();
            if (fbs != null)
            {
                return fbs.Fill(Default);
            }

            return new FilledInfiniteShape<T>() { Source = Source, Default = Default };
        }

        /// <summary>
        /// Wrapper used to Fill a shape without its own capability to do so.
        /// </summary>
        public class FilledInfiniteShape<T> : IFilledInfiniteShape<T>, IEnumerableSurfaceShape<T>
        {
            IBoundedShape<T> IFilledInfiniteShape<T>.Source
            {
                get
                {
                    return this.Source;
                }
            }

            T IFilledInfiniteShape<T>.Default
            {
                get
                {
                    return this.Default;
                }
            }

            public T Lookup(Vector<int> Location)
            {
                Vector<int> sourcebounds = this.Source.Bound;
                if (Location.X >= 0 && Location.Y >= 0 && Location.Z >= 0 &&
                    Location.X < sourcebounds.X &&
                    Location.Y < sourcebounds.Y &&
                    Location.Z < sourcebounds.Z)
                {
                    return this.Source.Lookup(Location);
                }
                else
                {
                    return this.Default;
                }
            }

            public IEnumerableSurface<F> EnumerateBorders<F>(Surfacize<T, F> SurfacizeFunc, F Excluded)
                where F : IEquatable<F>
            {
                Axis[] axies = new Axis[] { Axis.X, Axis.Y, Axis.Z };

                // Check to insure noninfinince.
                foreach (Axis a in axies)
                {
                    if (!SurfacizeFunc(this.Default, this.Default, a).Equals(Excluded))
                    {
                        return null;
                    }
                }

                // Could save some time if the source already knows how to do this.
                IBoundedEnumerableSurfaceShape<T> bess = this.Source as IBoundedEnumerableSurfaceShape<T>;
                if (bess != null)
                {
                    // Hooray, less work for us.
                    return bess.EnumerateBorders<F>(SurfacizeFunc, this.Default, Excluded);
                }

                // Check main area
                List<Border<F>> borders = new List<Border<F>>();
                Vector<int> sourcebounds = this.Source.Bound;
                for (int x = 0; x < sourcebounds.X; x++)
                {
                    for (int y = 0; y < sourcebounds.Y; y++)
                    {
                        for (int z = 0; z < sourcebounds.Z; z++)
                        {
                            Vector<int> pos = new Vector<int>(x, y, z);
                            foreach (Axis a in axies)
                            {
                                Vector<int> norm = new Vector<int>(1, 0, 0).AxisUnorder(a);
                                Vector<int> upperpos = Vector.Add(pos, norm);
                                T low = this.Source.Lookup(pos);
                                T high = (upperpos.X >= sourcebounds.X || upperpos.Y >= sourcebounds.Y || upperpos.Z >= sourcebounds.Z) ?
                                    this.Default : this.Source.Lookup(upperpos);
                                F val = SurfacizeFunc(low, high, a);
                                if (!val.Equals(Default))
                                {
                                    borders.Add(new Border<F>()
                                    {
                                        Position = pos,
                                        Direction = a,
                                        Value = val
                                    });
                                }
                            }
                        }
                    }
                }

                // Check negative area.
                foreach (Axis a in axies)
                {
                    Vector<int> unorderedbounds = sourcebounds.AxisUnorder(a);
                    for (int y = 0; y < unorderedbounds.Y; y++)
                    {
                        for (int z = 0; z < unorderedbounds.Z; z++)
                        {
                            Vector<int> posneg = new Vector<int>(0, y, z).AxisUnorder(a);
                            T highneg = this.Source.Lookup(posneg);
                            F val = SurfacizeFunc(this.Default, highneg, a);
                            if (val != null)
                            {
                                borders.Add(new Border<F>()
                                {
                                    Position = new Vector<int>(-1, y, z).AxisUnorder(a),
                                    Direction = a,
                                    Value = val
                                });
                            }
                        }
                    }
                }

                return new FiniteBorderSurface<F>(borders, Excluded);
            }

            public E Extend<E>() where E : class
            {
                return this as E;
            }

            public T Default;
            public IBoundedShape<T> Source;
        }

        /// <summary>
        /// Gets a subsection of the specified shape.
        /// </summary>
        public static IBoundedShape<T> Subsection<T>(IInfiniteShape<T> Source, Vector<int> Size, Vector<int> Start)
        {
            IDivisibleInfiniteShape<T> dis = Source.Extend<IDivisibleInfiniteShape<T>>();
            if (dis != null)
            {
                return dis.Subsection(Start, Size);
            }

            return new SubsectionShape<T>() { Source = Source, Start = Start, Size = Size };
        }

        /// <summary>
        /// Wrapper used to get a subsection of a shape without its own capability to do so.
        /// </summary>
        public class SubsectionShape<T> : IBoundedShape<T>
        {
            public T Lookup(Vector<int> Location)
            {
                return this.Source.Lookup(Vector.Add(Location, Start));
            }

            public Vector<int> Bound
            {
                get 
                {
                    return this.Size;
                }
            }

            public E Extend<E>() 
                where E : class
            {
                return this as E;
            }

            public Vector<int> Size;
            public Vector<int> Start;
            public IInfiniteShape<T> Source;
        }

        /// <summary>
        /// Creates a cubiod shape.
        /// </summary>
        public static CuboidShape<T> Cubiod<T>(Vector<int> Start, Vector<int> Size, T Interior, T Default)
        {
            return new CuboidShape<T>(Interior, Default, Start, Size);
        }

        /// <summary>
        /// A cubiod in infinite space.
        /// </summary>
        public class CuboidShape<T> : IInfiniteShape<T>
        {
            public CuboidShape(T Interior, T Default, Vector<int> Start, Vector<int> Size)
            {
                this._Interior = Interior;
                this._Default = Default;
                this._Start = Start;
                this._Size = Size;
            }

            public T Lookup(Vector<int> Location)
            {
                Location = Vector.Subtract(Location, this._Start);
                if (Location.X >= 0 && Location.Y >= 0 && Location.Z >= 0 &&
                    Location.X < this._Size.X && Location.Y < this._Size.Y && Location.Z < this._Size.Z)
                {
                    return this._Interior;
                }
                else
                {
                    return this._Default;
                }
            }

            public E Extend<E>() where E : class
            {
                return this as E;
            }

            private T _Interior;
            private T _Default;
            private Vector<int> _Start;
            private Vector<int> _Size;
        }
    }
}