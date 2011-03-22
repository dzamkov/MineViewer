using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Cubia
{
    /// <summary>
    /// A sqaure section that describes a material (color/texturing) that can be applied
    /// to a border in a surface.
    /// </summary>
    public interface IMaterial
    {
        /// <summary>
        /// Creates a renderable which can render this material. The mapping function is used to convert coordinates
        /// in ((0.0, 0.0), (1.0, 1.0)) to coordinates in the target render space. The normal of the material is provided.
        /// </summary>
        IRenderable CreateRenderable(Func<Point<double>, Vector<double>> Mapping, Vector<double> Normal);

        /// <summary>
        /// Creates a bitmap image (Size x Size pixels) of the material.
        /// </summary>
        Bitmap MakeBitmap(int Size);

        Color GetColor();
    }

    /// <summary>
    /// A description of a material in a surface.
    /// </summary>
    public struct Material : IEquatable<Material>
    {
        /// <summary>
        /// Gets the material used to render this.
        /// </summary>
        public IMaterial Description;

        /// <summary>
        /// Gets the direction the material is facing.
        /// </summary>
        public Polarity Direction;

        /// <summary>
        /// Creates a function which maps (x, y) to a point on the specified plane corresponding to (x + PlaneStart.X, y + PlaneStart.Y). May be
        /// inverted depending on polarity.
        /// </summary>
        public static Func<Point<double>, Vector<double>> Project(Axis PlaneAxis, Polarity Polarity, int PlaneLevel, Point<int> PlaneStart, Point<int> PlaneSize, out Vector<double> Normal)
        {
            if (Polarity == Polarity.Positive)
            {
                Normal = new Vector<double>(1, 0, 0).AxisUnorder(PlaneAxis);
                return p => new Vector<double>(PlaneLevel + 0.5, p.X + PlaneStart.X - 0.5, p.Y + PlaneStart.Y - 0.5).AxisUnorder(PlaneAxis);
            }
            else
            {
                Normal = new Vector<double>(-1, 0, 0).AxisUnorder(PlaneAxis);
                return p => new Vector<double>(PlaneLevel + 0.5, PlaneStart.X + PlaneSize.X - p.X - 0.5, p.Y + PlaneStart.Y - 0.5).AxisUnorder(PlaneAxis);
            }
        }

        /// <summary>
        /// Gets the renderable needed to render a material in a surface.
        /// </summary>
        public static IRenderable CreateRenderable(Border<Material> Border)
        {
            Vector<int> ordpos = Border.Position.AxisOrder(Border.Direction);
            Vector<double> normal;
            Func<Point<double>, Vector<double>> proj = Project(
                Border.Direction, Border.Value.Direction, 
                ordpos.X, new Point<int>(ordpos.Y, ordpos.Z), new Point<int>(1, 1),
                out normal);
            return Border.Value.Description.CreateRenderable(proj, normal);
        }

        /// <summary>
        /// The default material, signifying no rendering is needed.
        /// </summary>
        public static readonly Material Default = new Material() { Description = null, Direction = Polarity.Negative };

        public bool Equals(Material other)
        {
            return this.Description == other.Description && this.Direction == other.Direction;
        }

        public override int GetHashCode()
        {
            if (this.Description != null)
            {
                return this.Description.GetHashCode() ^ (int)this.Direction;
            }
            else
            {
                return (int)this.Direction;
            }
        }
    }

    /// <summary>
    /// A material that can be applied as tiles to a rectangular area.
    /// </summary>
    public interface ITileableMaterial : IMaterial
    {
        /// <summary>
        /// Creates a renderable which can render a rectangular area of this material. Similar to GetRenderable but renders
        /// coordinate in ((0.0, 0.0), (Size.X, Size.Y)) space.
        /// </summary>
        IRenderable CreateTileableRenderable(Func<Point<double>, Vector<double>> Mapping, Vector<double> Normal, Point<int> Size);
    }

    /// <summary>
    /// A cube surface made from a single solid color.
    /// </summary>
    public class SolidColorMaterial : ITileableMaterial
    {
        public SolidColorMaterial(Color Color)
        {
            this._Color = Color;
        }

        public IRenderable CreateRenderable(Func<Point<double>, Vector<double>> Mapping, Vector<double> Normal)
        {
            return CreateTileableRenderable(Mapping, Normal, new Point<int>(1, 1));
        }

        public IRenderable CreateTileableRenderable(Func<Point<double>, Vector<double>> Mapping, Vector<double> Normal, Point<int> Size)
        {
            return new EnumerableVertexRenderable<ColorNormalVertex>(
                new ColorNormalVertex[]
                {
                    new ColorNormalVertex() { Position = Mapping(new Point<double>(0.0, -0.0)), Color = this._Color, Normal = Normal },
                    new ColorNormalVertex() { Position = Mapping(new Point<double>((double)Size.X, 0.0)), Color = this._Color, Normal = Normal },
                    new ColorNormalVertex() { Position = Mapping(new Point<double>((double)Size.X, (double)Size.Y)), Color = this._Color, Normal = Normal },
                    new ColorNormalVertex() { Position = Mapping(new Point<double>(0.0, (double)Size.Y)), Color = this._Color, Normal = Normal }
                }, BeginMode.Quads);
        }

        public Bitmap MakeBitmap(int Size)
        {
            Bitmap bm = new Bitmap(Size, Size);
            using (Graphics g = Graphics.FromImage(bm))
            {
                g.FillRectangle(new SolidBrush(this.Color), 0, 0, Size, Size);
            }
            return bm;
        }

        public Color GetColor()
        {
            return this._Color;
        }

        /// <summary>
        /// Gets the color of the surface.
        /// </summary>
        public Color Color
        {
            get
            {
                return this._Color;
            }
        }

        private Color _Color;
    }

    /// <summary>
    /// A cube surface made from another, 
    /// </summary>
    public class BorderedMaterial : IMaterial
    {
        public BorderedMaterial(Color BorderColor, double BorderFalloff, IMaterial Base)
        {
            this._BorderColor = BorderColor;
            this._BorderFalloff = BorderFalloff;
            this._Base = Base;
        }

        public Color GetColor()
        {
            return this._Base.GetColor();
        }

        public IRenderable CreateRenderable(Func<Point<double>, Vector<double>> Mapping, Vector<double> Normal)
        {
            SolidColorMaterial sccs = this._Base as SolidColorMaterial;
            if (sccs != null)
            {
                LinkedList<ColorNormalVertex> verts = new LinkedList<ColorNormalVertex>();
                double falloff = this._BorderFalloff / 2;
                double[] faceposes = new double[]
                {
                    1.0,
                    1.0 - falloff,
                    falloff,
                    0.0
                };
                Vector<double>[,] actposes = new Vector<double>[4, 4];
                Color[,] actcols = new Color[4, 4];
                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        double xpos = faceposes[x];
                        double ypos = faceposes[y];
                        Color col = (x == 1 || x == 2) && (y == 1 || y == 2) ? sccs.Color : this._BorderColor;
                        actcols[x, y] = col;
                        actposes[x, y] = Mapping(new Point<double>(xpos, ypos));
                    }
                }
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        // Flipping some corners is required to avoid making a big colored triangle on
                        // the corner.
                        int[,] vertorder;
                        if ((x == 2 && y == 0) || (x == 0 && y == 2))
                        {
                            vertorder = new int[,] {
                                            {x + 1, y},
                                            {x + 1, y + 1},
                                            {x, y + 1},
                                            {x, y}
                                        };
                        }
                        else
                        {
                            vertorder = new int[,] {
                                            {x, y},
                                            {x + 1, y},
                                            {x + 1, y + 1},
                                            {x, y + 1}
                                        };
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            verts.AddLast(new ColorNormalVertex()
                            {
                                Color = actcols[vertorder[i, 0], vertorder[i, 1]],
                                Position = actposes[vertorder[i, 0], vertorder[i, 1]],
                                Normal = Normal
                            });
                        }
                    }
                }

                return new EnumerableVertexRenderable<ColorNormalVertex>(verts, BeginMode.Quads);
            }

            throw new NotImplementedException();
        }

        public Bitmap MakeBitmap(int Size)
        {
            Bitmap bm = this._Base.MakeBitmap(Size);
            System.Drawing.Color col = bm.GetPixel(0, 0);
            using (Graphics g = Graphics.FromImage(bm))
            {
                GraphicsPath path = new GraphicsPath();

                path.AddRectangle(new Rectangle(0, 0, Size, Size));
                

                PathGradientBrush brush = new PathGradientBrush(path);
                brush.WrapMode = WrapMode.Clamp;
                brush.SurroundColors = new System.Drawing.Color[]{this.BorderColor};
                brush.CenterColor = col;
                float falloff = 1f - (float)this.BorderFalloff;
                brush.FocusScales = new PointF(falloff, falloff);
                g.FillRectangle(brush, 0, 0, Size, Size);

                path.Dispose();
                brush.Dispose();
                
            }
            return bm;
        }

        /// <summary>
        /// Gets the color of the border.
        /// </summary>
        public Color BorderColor
        {
            get
            {
                return this._BorderColor;
            }
        }

        /// <summary>
        /// Gets the size of the falloff portion of the border (where the border blends into the base) relative
        /// to the size of the cube. (0.0 - 1.0)
        /// </summary>
        public double BorderFalloff
        {
            get
            {
                return this._BorderFalloff;
            }
        }

        /// <summary>
        /// Gets the surface to draw the border on.
        /// </summary>
        public IMaterial Base
        {
            get
            {
                return this._Base;
            }
        }

        private double _BorderFalloff;
        private Color _BorderColor;
        private IMaterial _Base;
    }

    /// <summary>
    /// A cube surface that displays a texture.
    /// </summary>
    public class TexturedMaterial : IMaterial
    {
        public TexturedMaterial(Texture Texture)
        {
            this._Texture = Texture;
        }

        public Color GetColor()
        {
            return Color.RGB(0.0,0.0,0.0);
        }

        public IRenderable CreateRenderable(Func<Point<double>, Vector<double>> Mapping, Vector<double> Normal)
        {
            return new TextureRenderable(this._Texture,
                new EnumerableVertexRenderable<TextureNormalVertex>(
                    new TextureNormalVertex[]
                    {
                        new TextureNormalVertex() { Position = Mapping(new Point<double>(0.0, 0.0)), UV = new Point<double>(0.0, 0.0), Normal = Normal },
                        new TextureNormalVertex() { Position = Mapping(new Point<double>(1.0, 0.0)), UV = new Point<double>(0.0, 1.0), Normal = Normal },
                        new TextureNormalVertex() { Position = Mapping(new Point<double>(1.0, 1.0)), UV = new Point<double>(1.0, 1.0), Normal = Normal },
                        new TextureNormalVertex() { Position = Mapping(new Point<double>(0.0, 1.0)), UV = new Point<double>(1.0, 0.0), Normal = Normal }
                    }, BeginMode.Quads));
        }

        public Bitmap MakeBitmap(int Size)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the texture displayed on the cube surface.
        /// </summary>
        public Texture Texture
        {
            get
            {
                return this._Texture;
            }
        }

        private Texture _Texture;
    }
}