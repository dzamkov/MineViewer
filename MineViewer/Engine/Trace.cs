using System;
using System.Collections.Generic;

namespace Cubia
{
    /// <summary>
    /// A surface where traces (ray casting) can be computed against it.
    /// </summary>
    public interface ITraceableSurface<T> : ISurface<T>
        where T : IEquatable<T>
    {
        /// <summary>
        /// Traces a ray (directional, but bounded line) through the shape and returns all surfaces that are not
        /// excluded. Results should be in order of distance along the ray.
        /// </summary>
        IEnumerable<TraceHit<T>> TraceRay(Vector<double> RayStart, Vector<double> RayEnd, T Excluded);

    }

    /// <summary>
    /// Functions related to tracing.
    /// </summary>
    public static class Trace
    {
        /// <summary>
        /// Gets the unit that corresponds the specified point on a grid.
        /// </summary>
        public static Point<int> GridPoint(Point<double> GridOffset, out Point<double> Offset)
        {
            Point<int> u = new Point<int>((int)Math.Round(GridOffset.X), (int)Math.Round(GridOffset.Y));
            Offset = new Point<double>(GridOffset.X - (double)u.X, GridOffset.Y - (double)u.Y);
            return u;
        }

        /// <summary>
        /// Traces a ray (directional, but bounded line) through the shape and returns all surfaces that are not
        /// excluded.
        /// </summary>
        public static IEnumerable<TraceHit<T>> TraceRay<T>(ISurface<T> Surface, Vector<double> RayStart, Vector<double> RayEnd, T Excluded)
            where T : struct, IEquatable<T>
        {
            ITraceableSurface<T> ts = Surface as ITraceableSurface<T>;
            if (ts != null)
            {
                return ts.TraceRay(RayStart, RayEnd, Excluded);
            }

            return TraceRayDefault<T>(Surface, RayStart, RayEnd, Excluded);
        }

        /// <summary>
        /// Traces a surface using a (slow) method applicable to all surfaces.
        /// </summary>
        public static IEnumerable<TraceHit<T>> TraceRayDefault<T>(ISurface<T> Surface, Vector<double> RayStart, Vector<double> RayEnd, T Excluded)
            where T : struct, IEquatable<T>
        {
            Vector<int> start = Shape.Unit(RayStart);
            Vector<int> stop = Shape.Unit(RayEnd);
            TraceHit<T>[][] hits = new TraceHit<T>[3][];
            for (int iax = 0; iax < 3; iax++)
            {
                Axis ax = (Axis)iax;
                int ss = start[ax];
                int se = stop[ax];
                int dif = se - ss;
                
                if (dif > 0)
                {
                    hits[iax] = new TraceHit<T>[dif];
                    for (int t = 0; t < dif; t++)
                    {
                        double dis;
                        Point<double> phit = PlaneIntersect(RayStart, RayEnd, ax, (double)ss + (double)t + 0.5, out dis);
                        Point<double> offset;
                        Point<int> ghit = GridPoint(phit, out offset);
                        hits[iax][t] = new TraceHit<T>()
                        {
                            Distance = dis,
                            Offset = offset,
                            Border = Border<T>.Get(Surface, new Vector<int>(ss + t, ghit.X, ghit.Y).AxisUnorder(ax), ax)
                        };
                    }
                }
                else
                {
                    hits[iax] = new TraceHit<T>[-dif];
                    for (int t = 0; t < -dif; t++)
                    {
                        double dis;
                        Point<double> phit = PlaneIntersect(RayStart, RayEnd, ax, (double)se + (double)t + 0.5, out dis);
                        Point<double> offset;
                        Point<int> ghit = GridPoint(phit, out offset);
                        hits[iax][-dif - t - 1] = new TraceHit<T>()
                        {
                            Distance = dis,
                            Offset = offset,
                            Border = Border<T>.Get(Surface, new Vector<int>(se + t, ghit.X, ghit.Y).AxisUnorder(ax), ax)
                        };
                    }
                }
            }

            // Sort and return
            Vector<int?> curs = new Vector<int?>(
                hits[0].Length > 0 ? (int?)0 : null, 
                hits[1].Length > 0 ? (int?)0 : null, 
                hits[2].Length > 0 ? (int?)0 : null);
            while (true)
            {
                Axis? naxis = null;
                double closedis = 0.0;
                for (int iax = 0; iax < 3; iax++)
                {
                    Axis ax = (Axis)iax;
                    int? cur = curs[ax];
                    if (cur != null)
                    {
                        double dis = hits[iax][cur.Value].Distance;
                        if (naxis == null || dis < closedis)
                        {
                            naxis = ax;
                            closedis = dis;
                        }
                    }
                }

                if (naxis != null)
                {
                    // Increment
                    Axis ax = naxis.Value;
                    int c = curs[ax].Value;
                    int l = c + 1;
                    if (l >= hits[(int)(ax)].Length)
                    {
                        curs[ax] = null;
                    }
                    else
                    {
                        curs[ax] = l;
                    }

                    // Return
                    TraceHit<T> hit = hits[(int)(ax)][c];
                    if(!hit.Border.Value.Equals(Excluded))
                    {
                        yield return hit;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Intersects a plane on the specified axis and level with a ray and returns where the ray hit.
        /// </summary>
        public static Point<double> PlaneIntersect(Vector<double> RayStart, Vector<double> RayEnd, Axis PlaneAxis, double PlaneLevel, out Double RayDis)
        {
            Vector<double> oraystart = RayStart.AxisOrder(PlaneAxis); oraystart.X -= PlaneLevel;
            Vector<double> orayend = RayEnd.AxisOrder(PlaneAxis); orayend.X -= PlaneLevel;
            Vector<double> delta = Vector.Subtract(orayend, oraystart);
            RayDis = -(oraystart.X) / (delta.X);
            return new Point<double>(RayDis * delta.Y + oraystart.Y, RayDis * delta.Z + oraystart.Z);
        }
    }

    /// <summary>
    /// Result from a trace hitting a border.
    /// </summary>
    public struct TraceHit<T>
    {
        /// <summary>
        /// The offset of the trace from the lower corner of the border. Between (-0.5, -0.5) and (0.5, 0.5).
        /// </summary>
        public Point<double> Offset;

        /// <summary>
        /// How far along the ray the hit is at. Between 0.0 and 1.0.
        /// </summary>
        public double Distance;

        /// <summary>
        /// The border that was hit.
        /// </summary>
        public Border<T> Border;

        /// <summary>
        /// Gets the actual position of the hit. Note that a cube at (0, 0, 0) would have a center of (0.0, 0.0, 0.0) an edge length
        /// of 1.0 and extend into 0.5 and -0.5 in each axis. 
        /// </summary>
        public Vector<double> ActualPosition
        {
            get
            {
                Vector<double> bpos = Vector.IntToDoubleVector(Border.Position.AxisOrder(Border.Direction));
                return new Vector<double>(bpos.X + 0.5, bpos.Y + Offset.X, bpos.Z + Offset.Y).AxisUnorder(Border.Direction);
            }
        }
    }
}