using System;
using System.Collections;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Cubia
{
    /// <summary>
    /// Renders a shape in such a way that fast cross-sections at different levels can be created. Only the interior is rendered.
    /// </summary>
    public class StratifiedRenderer<T> : IRenderable, IDisposable
        where T : IEquatable<T>
    {
        public StratifiedRenderer(IBoundedShape<T> Source, Surfacize<T, Material> SurfacizeFunc, T Default)
        {
            this._Default = Default;
            this._Source = Source;
            this._SurfacizeFunc = SurfacizeFunc;
            this._OrientedRenderers = new _OrientedRenderer[3];
        }

        public void  Render()
        {
            foreach (_OrientedRenderer or in this._OrientedRenderers)
            {
                if (or != null)
                {
                    or.Render(0, or.DirectionBound);
                    return;
                }
            }
            this._OrientedRenderers[0] = new _OrientedRenderer(this, Axis.X);
            this._OrientedRenderers[0].Render(0, this._OrientedRenderers[0].DirectionBound);
        }

        public void Render(Axis Axis, int StartLevel, int EndLevel)
        {
            int iax = (int)Axis;
            _OrientedRenderer or = this._OrientedRenderers[iax];
            if (or == null)
            {
                or = this._OrientedRenderers[iax] = new _OrientedRenderer(this, Axis);
            }
            or.Render(StartLevel, EndLevel);
        }

        /// <summary>
        /// Generates oriented renderers for the specified axies.
        /// </summary>
        public void GenerateOrientedRenderers(bool[] Axies)
        {
            for (int iax = 0; iax < 3; iax++)
            {
                if (Axies[iax] && this._OrientedRenderers[iax] == null)
                {
                    this._OrientedRenderers[iax] = new _OrientedRenderer(this, (Axis)iax);
                }
            }
        }

        /// <summary>
        /// Renders a shape oriented in one direction, to allow fast slicing.
        /// </summary>
        private class _OrientedRenderer : IDisposable
        {
            public _OrientedRenderer(StratifiedRenderer<T> StratifiedRenderer, Axis Axis)
            {
                this._StratifiedRenderer = StratifiedRenderer;
                this._Axis = Axis;
                IBoundedShape<T> source = this._StratifiedRenderer._Source;
                Vector<int> bounds = source.Bound;
                this._DirBounds = bounds[Axis];
                this._Interior = new OrientedPartRenderer(Shape.EnumerateInterior(source, this._StratifiedRenderer._SurfacizeFunc, Material.Default), Axis, this._DirBounds);
                this._Levels = new VBO<ColorNormalVertex>[this._DirBounds - 1, 2];
            }

            /// <summary>
            /// Gets the bound of the renderer in the direction its oriented to.
            /// </summary>
            public int DirectionBound
            {
                get
                {
                    return this._DirBounds;
                }
            }

            public void Render(int StartLevel, int EndLevel)
            {
                if (StartLevel > 0)
                {
                    this.GetLevel(StartLevel, Polarity.Negative).GetRenderable(BeginMode.Quads).Render();
                }
                if (EndLevel < this._DirBounds - 1)
                {
                    this.GetLevel(EndLevel, Polarity.Positive).GetRenderable(BeginMode.Quads).Render();
                }
                this._Interior.Render(StartLevel, 0, EndLevel, 2);
            }

            /// <summary>
            /// Gets the vertex buffer for the specified level.
            /// </summary>
            public VBO<ColorNormalVertex> GetLevel(int Level, Polarity Direction)
            {
                if (Direction == Polarity.Negative)
                {
                    Level--;
                }
                VBO<ColorNormalVertex> c = this._Levels[Level, (int)Direction];
                if (c == null)
                {
                    IIterator<ColorNormalVertex, VBO<ColorNormalVertex>> vboc = VBO<ColorNormalVertex>.Create();
                    IBoundedPlaneSurface<Material> bps = Shape.Slice(this._StratifiedRenderer._Source, this._Axis, Level, delegate(T Lower, T Higher, Axis Axis)
                    {
                        if (Direction == Polarity.Positive && !Higher.Equals(this._StratifiedRenderer._Default))
                        {
                            return this._StratifiedRenderer._SurfacizeFunc(Lower, this._StratifiedRenderer._Default, Axis);
                        }
                        if (Direction == Polarity.Negative && !Lower.Equals(this._StratifiedRenderer._Default))
                        {
                            return this._StratifiedRenderer._SurfacizeFunc(this._StratifiedRenderer._Default, Higher, Axis);
                        }
                        return Material.Default;
                    }, Material.Default);

                    QuadtreeSurface<Material> qs = bps.Extend<QuadtreeSurface<Material>>();
                    if (qs != null)
                    {
                        StaticRenderer.Send(qs, vboc, vboc);
                    }
                    else
                    {
                        StaticRenderer.Send(bps, vboc);
                    }

                    this._Levels[Level, (int)Direction] = c = vboc.End();
                }
                return c;
            }

            public void Dispose()
            {
                for (int t = 0; t < this._Levels.GetLength(0); t++)
                {
                    for (int l = 0; l < this._Levels.GetLength(1); l++)
                    {
                        if (this._Levels[t, l] != null)
                        {
                            this._Levels[t, l].Dispose();
                        }
                    }
                }
                this._Interior.Dispose();
            }

            private int _DirBounds;
            private Axis _Axis;
            private StratifiedRenderer<T> _StratifiedRenderer;
            private VBO<ColorNormalVertex>[,] _Levels;
            private OrientedPartRenderer _Interior;
        }

        public void Dispose()
        {
            for (int iax = 0; iax < 3; iax++)
            {
                _OrientedRenderer or = this._OrientedRenderers[iax];
                if (or != null)
                {
                    or.Dispose();
                }
            }
        }

        private T _Default;
        private _OrientedRenderer[] _OrientedRenderers;
        private Surfacize<T, Material> _SurfacizeFunc;
        private IBoundedShape<T> _Source;
    }

    /// <summary>
    /// Renders an enumerable surface oriented in one direction, to allow parts at different levels along that
    /// axis to be rendered selectively.
    /// </summary>
    public class OrientedPartRenderer : IRenderable, IDisposable
    {
        public OrientedPartRenderer(IEnumerableSurface<Material> Source, Axis Direction, int DirectionBound)
        {
            var accum = _CreateAccum(DirectionBound);

            IOctreeInteriorSurface<Material> ois = Source.Extend<IOctreeInteriorSurface<Material>>();
            if (ois != null)
            {
                _SendVertices(ois.Slices, Direction, DirectionBound, accum);
            }
            else
            {
                _SendVertices(Source.Borders, Direction, DirectionBound, accum);
            }

            int Size;
            this._Layers = _CreateLayers(accum, DirectionBound, out Size);
            this._VBO = new VBO<ColorNormalVertex>(_Concat(accum), Size);
        }

        /// <summary>
        /// Sends the vertices in the specified surface to the accumulator.
        /// </summary>
        private static void _SendVertices(IEnumerable<Border<Material>> Source, Axis Direction, int DirectionBound, List<ColorNormalVertex>[,] Accum)
        {
            foreach (Border<Material> bord in Source)
            {
                int l = bord.Position[Direction];
                int d = 1;
                if (bord.Direction == Direction)
                {
                    if (bord.Value.Direction == Polarity.Positive)
                    {
                        d++;
                    }
                    else
                    {
                        l++;
                        d--;
                    }
                }

                IRenderable ren = Material.CreateRenderable(bord);
                IVertexRenderable<ColorNormalVertex> cnv = ren as IVertexRenderable<ColorNormalVertex>;
                if (cnv != null)
                {
                    Accum[l, d].AddRange(cnv.Vertices);
                }
            }
        }

        /// <summary>
        /// Sends the vertices in the set of slices to the accumulator.
        /// </summary>
        private static void _SendVertices(Quadtree<Material>[,] Slices, Axis Direction, int DirectionBound, List<ColorNormalVertex>[,] Accum)
        {
            for (int iax = 0; iax < 3; iax++)
            {
                Axis ax = (Axis)iax;
                if (ax != Direction)
                {
                    for (int l = 0; l < DirectionBound - 1; l++)
                    {
                        foreach (KeyValuePair<Point<int>, Material> bord in Slices[iax, l].Enumerate(Material.Default))
                        {
                            Vector<int> pos = new Vector<int>(l, bord.Key.X, bord.Key.Y).AxisUnorder(ax);
                            IRenderable ren = Material.CreateRenderable(new Border<Material>() { Direction = ax, Position = pos, Value = bord.Value });
                            IVertexRenderable<ColorNormalVertex> cnv = ren as IVertexRenderable<ColorNormalVertex>;
                            if (cnv != null)
                            {
                                Accum[pos[Direction], 1].AddRange(cnv.Vertices);
                            }
                        }
                    }
                }
                else
                {
                    for (int l = 0; l < DirectionBound - 1; l++)
                    {
                        var posit = Iterator.ListIterator(Accum[l, 2]);
                        var negit = Iterator.ListIterator(Accum[l + 1, 0]);
                        StaticRenderer.SendQuadtree(Slices[iax, l], Direction, l, new Point<int>(0, 0), posit, negit);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a vertice accumulation buffer.
        /// </summary>
        private static List<ColorNormalVertex>[,] _CreateAccum(int DirectionBound)
        {
            List<ColorNormalVertex>[,] accum = new List<ColorNormalVertex>[DirectionBound, 3];
            for (int l = 0; l < DirectionBound; l++)
            {
                for (int t = 0; t < 3; t++)
                {
                    accum[l, t] = new List<ColorNormalVertex>();
                }
            }
            return accum;
        }

        /// <summary>
        /// Creates a set of layers based on an accumulator. Outputs the total amount of vertices.
        /// </summary>
        private static int[,] _CreateLayers(List<ColorNormalVertex>[,] Accum, int DirectionBound, out int Size)
        {
            int[,] layers = new int[DirectionBound, 3];
            Size = 0;
            for (int l = 0; l < DirectionBound; l++)
            {
                for (int t = 0; t < 3; t++)
                {
                    Size += Accum[l, t].Count;
                    layers[l, t] = Size;
                }
            }
            return layers;
        }

        /// <summary>
        /// Creates a vertex buffer object from an accumulator with the specified size.
        /// </summary>
        private static VBO<ColorNormalVertex> _CreateVBO(int Size, List<ColorNormalVertex>[,] Accum)
        {
            return new VBO<ColorNormalVertex>(_Concat(Accum), Size);
        }

        private static IEnumerable<T> _Concat<T>(IEnumerable<T>[,] Source)
        {
            foreach (IEnumerable<T> e in Source)
            {
                foreach (T t in e)
                {
                    yield return t;
                }
            }
        }

        public void Render()
        {
            this._VBO.GetRenderable(BeginMode.Quads).Render();
        }

        /// <summary>
        /// Renders a part of the scene specified to this oriented part renderer. Major indices specify
        /// which "layer" to start and stop rendering on. Minor indices specify which part of the layer
        /// to start and stop on, 0 indicating positive facing borders, 1 indicating borders parallel to the axis,
        /// 2 indicating negative facing borders.
        /// </summary>
        public void Render(int MajorStart, int MinorStart, int MajorEnd, int MinorEnd)
        {
            MinorStart--;
            if (MinorStart < 0)
            {
                MajorStart--;
                MinorStart = 2;
            }
            int max = this._Layers.GetLength(0);
            int sv = MajorStart >= 0 ? this._Layers[MajorStart, MinorStart] : 0;
            int ev = MajorEnd < max ? this._Layers[MajorEnd, MinorEnd] : this._Layers[max - 1, 2];
            this._VBO.GetRenderable(BeginMode.Quads, sv, ev - sv).Render();
        }

        public void Dispose()
        {
            this._VBO.Dispose();
        }

        private VBO<ColorNormalVertex> _VBO;
        private int[,] _Layers; 
    }
}