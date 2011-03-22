using System;
using System.Collections.Generic;
using OpenTK;

namespace Cubia
{
    /// <summary>
    /// A third-dimensional vector of some type.
    /// </summary>
    public struct Vector<T>
    {
        public Vector(T X, T Y, T Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public T X;
        public T Y;
        public T Z;

        /// <summary>
        /// Gets the component of the specified axis.
        /// </summary>
        public T this[Axis Axis]
        {
            get
            {
                switch (Axis)
                {
                    case Axis.X:
                        return this.X;
                    case Axis.Y:
                        return this.Y;
                    default:
                        return this.Z;
                }
            }
            set
            {
                switch (Axis)
                {
                    case Axis.X:
                        this.X = value;
                        break;
                    case Axis.Y:
                        this.Y = value;
                        break;
                    default:
                        this.Z = value;
                        break;
                }
            }
        }

        /// <summary>
        /// Orders the components of a vector so the component corresponding to
        /// the specified axis comes first.
        /// </summary>
        public Vector<T> AxisOrder(Axis Axis)
        {
            switch (Axis)
            {
                case Axis.X:
                    return new Vector<T>(this.X, this.Y, this.Z);
                case Axis.Y:
                    return new Vector<T>(this.Y, this.Z, this.X);
                default:
                    return new Vector<T>(this.Z, this.X, this.Y);
            }
        }

        /// <summary>
        /// Performs the inverse operation of axis order.
        /// </summary>
        public Vector<T> AxisUnorder(Axis Axis)
        {
            switch (Axis)
            {
                case Axis.X:
                    return new Vector<T>(this.X, this.Y, this.Z);
                case Axis.Y:
                    return new Vector<T>(this.Z, this.X, this.Y);
                default:
                    return new Vector<T>(this.Y, this.Z, this.X);
            }
        }

        /// <summary>
        /// Converts a vector of some type to another based on a conversion function.
        /// </summary>
        public void Convert<L>(Func<T, L> ConversionFunction, out Vector<L> Vec)
        {
            Vec = new Vector<L>(ConversionFunction(this.X), ConversionFunction(this.Y), ConversionFunction(this.Z));
        }
    }

    /// <summary>
    /// The two-dimensional analogy of a vector.
    /// </summary>
    public struct Point<T> : IEquatable<Point<T>>
        where T : IEquatable<T>
    {
        public Point(T X, T Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public bool Equals(Point<T> other)
        {
            return this.X.Equals(other.X) && this.Y.Equals(other.Y);
        }

        private struct _EqualityComparer : IEqualityComparer<Point<T>>
        {
            public bool Equals(Point<T> x, Point<T> y)
            {
                return x.Equals(y);
            }

            public int GetHashCode(Point<T> obj)
            {
                return obj.X.GetHashCode() ^ obj.Y.GetHashCode();
            }
        }

        /// <summary>
        /// An equality comparer for points.
        /// </summary>
        public static readonly IEqualityComparer<Point<T>> EqualityComparer = new _EqualityComparer();

        public T X;
        public T Y;
    }

    /// <summary>
    /// Misc point functions.
    /// </summary>
    public static class Point
    {
        public static Point<int> Add(Point<int> A, Point<int> B)
        {
            return new Point<int>(A.X + B.X, A.Y + B.Y);
        }

        public static Point<int> Subtract(Point<int> A, Point<int> B)
        {
            return new Point<int>(A.X - B.X, A.Y - B.Y);
        }
    }

    /// <summary>
    /// Misc vector functions.
    /// </summary>
    public static class Vector
    {
        /// <summary>
        /// Converts an integer vector to one usable by opentk.
        /// </summary>
        public static Vector3d FromIntVector(Vector<int> Vec)
        {
            return new Vector3d((double)Vec.X, (double)Vec.Y, (double)Vec.Z);
        }

        public static Vector3d FromDoubleVector(Vector<double> Vec)
        {
            return new Vector3d(Vec.X, Vec.Y, Vec.Z);
        }

        public static Vector<double> IntToDoubleVector(Vector<int> Vec)
        {
            return new Vector<double>((double)Vec.X, (double)Vec.Y, (double)Vec.Z);
        }

        /// <summary>
        /// Gets the distance between two vectors.
        /// </summary>
        public static double Distance(Vector<double> A, Vector<double> B)
        {
            return Length(Subtract(A, B));
        }

        /// <summary>
        /// Gets the length of the specified vector.
        /// </summary>
        public static double Length(Vector<double> A)
        {
            return Math.Sqrt(
                (A.X * A.X) +
                (A.Y * A.Y) +
                (A.Z * A.Z));
        }

        /// <summary>
        /// Interpolates between two vectors by the specified amount.
        /// </summary>
        public static Vector<double> Interpolate(Vector<double> A, Vector<double> B, double Amount)
        {
            return Add(A, Multiply(Subtract(B, A), Amount));
        }

        public static Vector<double> Add(Vector<double> A, Vector<double> B)
        {
            return new Vector<double>(A.X + B.X, A.Y + B.Y, A.Z + B.Z);
        }

        public static Vector<int> Add(Vector<int> A, Vector<int> B)
        {
            return new Vector<int>(A.X + B.X, A.Y + B.Y, A.Z + B.Z);
        }

        public static Vector<double> Subtract(Vector<double> A, Vector<double> B)
        {
            return new Vector<double>(A.X - B.X, A.Y - B.Y, A.Z - B.Z);
        }

        public static Vector<int> Subtract(Vector<int> A, Vector<int> B)
        {
            return new Vector<int>(A.X - B.X, A.Y - B.Y, A.Z - B.Z);
        }

        public static Vector<double> Multiply(Vector<double> Vec, double Amount)
        {
            return new Vector<double>(Vec.X * Amount, Vec.Y * Amount, Vec.Z * Amount);
        }

        /// <summary>
        /// Gets if the specified vector is within the bounds.
        /// </summary>
        public static bool Bounded(Vector<int> Vector, Vector<int> Bounds)
        {
            return Vector.X >= 0 && Vector.Y >= 0 && Vector.Z >= 0 &&
                Vector.X < Bounds.X && Vector.Y < Bounds.Y && Vector.Z < Bounds.Z;
        }

        /// <summary>
        /// Computes the cross product between two vectors, with the resulting
        /// vector being the normal.
        /// </summary>
        public static Vector<double> Cross(Vector<double> A, Vector<double> B)
        {
            return new Vector<double>(
                (A.Y * B.Z) - (A.Z * B.Y),
                (A.Z * B.X) - (A.X * B.Z),
                (A.X * B.Y) - (A.Y * B.X));
        }

        /// <summary>
        /// Normalizes the specified vector.
        /// </summary>
        public static Vector<double> Normalize(Vector<double> A)
        {
            return Multiply(A, 1.0 / Length(A));
        }

        /// <summary>
        /// Gets a vector representing one of the 6 axies.
        /// </summary>
        public static Vector<int> AxisIndex(int Index)
        {
            return (new Vector<int>[] {
                new Vector<int>(1, 0, 0),
                new Vector<int>(-1, 0, 0),
                new Vector<int>(0, 1, 0),
                new Vector<int>(0, -1, 0),
                new Vector<int>(0, 0, 1),
                new Vector<int>(0, 0, -1)
            })[Index];
        }

        /// <summary>
        /// Gets if two vectors are equal.
        /// </summary>
        public static bool Equal(Vector<int> A, Vector<int> B)
        {
            return 
                A.X == B.X &&
                A.Y == B.Y &&
                A.Z == B.Z;
        }
    }

    /// <summary>
    /// One of the 3 axies in a vector.
    /// </summary>
    public enum Axis
    {
        X,
        Y,
        Z
    }

    /// <summary>
    /// Positive or negative.
    /// </summary>
    public enum Polarity
    {
        Positive,
        Negative
    }
}