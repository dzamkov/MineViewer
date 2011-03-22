using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Cubia
{
    /// <summary>
    /// Renders a static shape of visual cubes.
    /// </summary>
    public class StaticRenderer : IRenderable
    {
        public StaticRenderer(IEnumerableSurface<Material> Source)
        {
            IIterator<ColorNormalVertex, VBO<ColorNormalVertex>> vboc = VBO<ColorNormalVertex>.Create();

            IOctreeInteriorSurface<Material> ois = Source.Extend<IOctreeInteriorSurface<Material>>();
            if (ois != null)
            {
                Quadtree<Material>[,] slices = ois.Slices; 
                for (int iax = 0; iax < 3; iax++)
                {
                    for (int l = 0; l < ois.Size - 1; l++)
                    {
                        Send(new QuadtreeSurface<Material>(slices[iax, l], Material.Default, (Axis)iax, l), vboc, vboc);
                    }
                }
                this._VBO = vboc.End();
                return;
            }

            Send(Source, vboc);
            this._VBO = vboc.End();
        }

        /// <summary>
        /// Sends all the vertices in the given surface to the specified iterator.
        /// </summary>
        public static void Send<R>(IEnumerableSurface<Material> Source, IIterator<ColorNormalVertex, R> Iterator)
        {
            foreach (Border<Material> bord in Source.Borders)
            {
                Send(Material.CreateRenderable(bord), Iterator);
            }
        }

        /// <summary>
        /// Sends the given renderable to the specified iterator.
        /// </summary>
        public static void Send<R>(IRenderable Renderable, IIterator<ColorNormalVertex, R> Iterator)
        {
            IVertexRenderable<ColorNormalVertex> vertexrenders = Renderable as IVertexRenderable<ColorNormalVertex>;
            if (vertexrenders != null)
            {
                Cubia.Iterator.Pipe(Iterator, vertexrenders.Vertices);
            }
        }

        /// <summary>
        /// Sends all the vertices in the given quadtree surface to the specified iterators.
        /// </summary>
        public static void Send<R>(QuadtreeSurface<Material> Source, IIterator<ColorNormalVertex, R> Positive, IIterator<ColorNormalVertex, R> Negative)
        {
            SendQuadtree(Source.Source, Source.PlaneAxis, Source.PlaneLevel, new Point<int>(0, 0), Positive, Negative);
        }

        /// <summary>
        /// Sends the vertices in a given quadtree of material information to the specified iterators.
        /// </summary>
        public static void SendQuadtree<R>(Quadtree<Material> Source, Axis PlaneAxis, int PlaneLevel, Point<int> Offset, 
            IIterator<ColorNormalVertex, R> Positive, IIterator<ColorNormalVertex, R> Negative)
        {
            Material val;
            if (Source.Homogenous(out val))
            {
                IMaterial m = val.Description;
                if (m != null)
                {
                    int s = Source.Size;
                    ITileableMaterial tm = m as ITileableMaterial;

                    if (tm != null)
                    {
                        Vector<double> normal;
                        var proj = Material.Project(PlaneAxis, val.Direction, PlaneLevel, new Point<int>(Offset.X, Offset.Y), new Point<int>(s, s), out normal);
                        Send(tm.CreateTileableRenderable(proj, normal, new Point<int>(s, s)), val.Direction == Polarity.Positive ? Positive : Negative);
                    }
                    else
                    {
                        for (int x = 0; x < s; x++)
                        {
                            for (int y = 0; y < s; y++)
                            {
                                Vector<double> normal;
                                var proj = Material.Project(PlaneAxis, val.Direction, PlaneLevel, new Point<int>(x + Offset.X, y + Offset.Y), new Point<int>(1, 1), out normal);
                                Send(m.CreateRenderable(proj, normal), val.Direction == Polarity.Positive ? Positive : Negative);
                            }
                        }
                    }
                }
            }
            else
            {
                int hs = 1 << (Source.Depth - 1);
                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        SendQuadtree(Source[x * 2 + y], PlaneAxis, PlaneLevel, new Point<int>(Offset.X + x * hs, Offset.Y + y * hs), Positive, Negative);
                    }
                }
            }
        }

        public void Render()
        {
            // Render the cache
            this._VBO.GetRenderable(BeginMode.Quads).Render();
        }

        public void RenderTrans()
        {
            this._VBOTrans.GetRenderable(BeginMode.Quads).Render();
        }

        /// <summary>
        /// Vertex buffer for the renderer.
        /// </summary>
        private VBO<ColorNormalVertex> _VBO;
        private VBO<ColorNormalVertex> _VBOTrans;
    }
}