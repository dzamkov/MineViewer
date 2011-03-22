using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System;

namespace Cubia
{
    /// <summary>
    /// Represents a two-dimensional image loaded into the graphics device.
    /// </summary>
    public class Texture
    {
        public Texture(Bitmap Source)
        {
            this._Source = Source;
            try
            {
                this._Load();
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Loads a texture by a name.
        /// </summary>
        public static Texture Load(string Name)
        {
            string cd = Directory.GetCurrentDirectory();
            cd = cd.Replace("\\bin\\Debug", "");
            cd = cd.Replace("\\bin\\Release", "");
            cd += "\\textures\\";


            Bitmap b = null;
            try
            {
                b = new Bitmap(cd + Name + ".png");
            }
            catch (Exception)
            {
                try
                {
                    b = new Bitmap(cd + Name + ".bmp");
                }
                catch (Exception)
                {

                }
            }


            if (b != null)
            {
                return new Texture(b);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Binds the texture for rendering.
        /// </summary>
        public void Bind()
        {
            if (this._Source != null)
            {
                this._Load();
            }
            GL.BindTexture(TextureTarget.Texture2D, this._TextureID);
        }

        /// <summary>
        /// Loads the texture into GL.
        /// </summary>
        private void _Load()
        {
            GL.GenBuffers(1, out this._TextureID);
            GL.BindTexture(TextureTarget.Texture2D, this._TextureID);

            BitmapData bd = this._Source.LockBits(
                new Rectangle(0, 0, this._Source.Width, this._Source.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexEnv(TextureEnvTarget.TextureEnv,
                TextureEnvParameter.TextureEnvMode,
                (float)TextureEnvMode.Modulate);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (float)TextureMagFilter.Linear);

            GL.TexImage2D(TextureTarget.Texture2D,
                0, PixelInternalFormat.Rgba,
                bd.Width, bd.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bd.Scan0);

            this._Source.UnlockBits(bd);
            this._Source = null;
        }

        private Bitmap _Source;
        private uint _TextureID;
    }

    /// <summary>
    /// Renders another renderable with a texture applied.
    /// </summary>
    public sealed class TextureRenderable : NestedRenderable
    {
        public TextureRenderable(Texture Texture, IRenderable Base) :
            base(Base)
        {
            this._Texture = Texture;
        }

        /// <summary>
        /// Gets the texture used.
        /// </summary>
        public Texture Texture
        {
            get
            {
                return this._Texture;
            }
        }

        protected override void PreRender()
        {
            GL.Enable(EnableCap.Texture2D);
            this._Texture.Bind();
        }

        protected override void PostRender()
        {
            GL.Disable(EnableCap.Texture2D);
        }

        public override int GetHashCode()
        {
            return this._Texture.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            TextureRenderable tr = obj as TextureRenderable;
            if (tr != null)
            {
                return tr._Texture == this._Texture;
            }
            return false;
        }

        private Texture _Texture;
    }
}