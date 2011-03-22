using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.IO;
using Cubia;
using System.Windows.Forms;
using System.Globalization;

namespace MineViewer
{
    /// <summary>
    /// Main window for the program.
    /// </summary>
    
    public class Window : GameWindow
    {
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this._Bookmarks.Save();
            base.OnClosing(e);
        }
        private Bookmarks _Bookmarks;

        public Window(string World, bool Nether , Dictionary<string, Scheme> Schemes)
            : base(640, 480, GraphicsMode.Default, DefaultTitle)
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmHelp));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend); // support transparencey
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            MinecraftLevel mcl;
            MinecraftLevel mclt;
            if (Nether)
            {
                mcl = new MinecraftLevel(World + Path.DirectorySeparatorChar + "DIM-1", false);
                mclt = new MinecraftLevel(World + Path.DirectorySeparatorChar + "DIM-1", true);
            }
            else
            {
                mcl = new MinecraftLevel(World, false);
                mclt = new MinecraftLevel(World, true);
            }
            int s = 128;
            
            DateTime starttime = DateTime.Now;
            string leveldat = World + Path.DirectorySeparatorChar + "level.dat"; // we still dont change this, level.dat is still used
            this._Renderer = new MinecraftRenderer(mcl);
            this._RendererTrans = new MinecraftRenderer(mclt);


            Scheme schm;
            Schemes.TryGetValue("Default", out schm);

            this._Renderer.CurrentScheme = schm;
            this._RendererTrans.CurrentScheme = schm;

            Vector<double> acpos = new Vector<double>();
            if (File.Exists(leveldat) && !SMPInterface.IsSMP)
            {
                try
                {
                    NBTCompound data = ((NBT.Read(File.OpenRead(leveldat)).Data as NBTCompound?).Value.Data["Data"].Data as NBTCompound?).Value;
                    NBTNamedTag<INBTData> player = null;

                    double hellscale = 8.0;

                    bool tryget = data.Data.TryGetValue("Player", out player);
                    bool nether = !(((player.Data as NBTCompound?).Value.Data["Dimension"].Data as NBTInt?).Value.Data == 0);
                    
                    if (tryget & Nether == nether)
                    {
                        NBTList pos = (((player.Data as NBTCompound?).Value.Data["Pos"]).Data as NBTList?).Value;
                        acpos = new Vector<double>(
                          (pos.Data[0] as NBTDouble?).Value.Data,
                          (pos.Data[1] as NBTDouble?).Value.Data,
                          (pos.Data[2] as NBTDouble?).Value.Data);
                    }
                    else if (tryget && !Nether) // the correct positions are not in the correct places
                    {                           // Not the nether, but player location is in the nether
                        // Lets make the view 16 times further away, this is the chunk they will come up on.
                        NBTList pos = (((player.Data as NBTCompound?).Value.Data["Pos"]).Data as NBTList?).Value;
                        acpos = new Vector<double>(
                          (pos.Data[0] as NBTDouble?).Value.Data * hellscale,
                          (pos.Data[1] as NBTDouble?).Value.Data,
                          (pos.Data[2] as NBTDouble?).Value.Data * hellscale);
                    }
                    else if (tryget && Nether) // Lets see where we will end up on the nether from up top
                    {
                        NBTList pos = (((player.Data as NBTCompound?).Value.Data["Pos"]).Data as NBTList?).Value;
                        acpos = new Vector<double>(
                          (pos.Data[0] as NBTDouble?).Value.Data / hellscale,
                          (pos.Data[1] as NBTDouble?).Value.Data,
                          (pos.Data[2] as NBTDouble?).Value.Data / hellscale);
                    }
                    else
                    {
                        acpos = new Vector<double>(
                            ((data.Data["SpawnX"]).Data as NBTInt?).Value.Data,
                            ((data.Data["SpawnY"]).Data as NBTInt?).Value.Data,
                            ((data.Data["SpawnZ"]).Data as NBTInt?).Value.Data);
                    }
                }
                catch
                {
                   
                }
            }
            else if (SMPInterface.IsSMP)
            {
                MineViewer.SMPPackets.PlayerSpawnPos.UpdateSpawnPos = new Action<Vector<int>>(delegate(Vector<int> pos)
                    {
                        Point<int> c;
                        Point<int> o; MinecraftRenderer.ChunkOffset(new Point<int>((int)pos.Z, (int)pos.X), out c, out o);
                        this._Renderer.GetChunk(c);
                        this._RendererTrans.GetChunk(c);
                        this._CamPos = new Vector<double>(s + 10.1 + c.X * MinecraftRenderer.ChunkSize, s + 10.1 + c.Y * MinecraftRenderer.ChunkSize, s + 10.1);

                    });
            }
            Point<int> chunk;
            Point<int> offset; MinecraftRenderer.ChunkOffset(new Point<int>((int)acpos.Z, (int)acpos.X), out chunk, out offset);
            if (!SMPInterface.IsSMP)
            {
                this._Renderer.GetChunk(chunk);
                this._RendererTrans.GetChunk(chunk);
            }
            this._CamPos = new Vector<double>(s + 10.1 + chunk.X * MinecraftRenderer.ChunkSize, s + 10.1 + chunk.Y * MinecraftRenderer.ChunkSize, s + 10.1);

            this._LookZAngle = 4.0;
            this._LookXAngle = -0.6;
            DateTime endtime = DateTime.Now;
            TimeSpan loadtime = endtime - starttime;
            this._LastHit = null;

            _Bookmarks = new Bookmarks(World + Path.DirectorySeparatorChar, delegate(Vector<double> vec, Vector<double> ang)
            {
                this._CamPos = vec;
                //this._LookXAngle = ang.X;
                //this._LookZAngle = ang.Z;

                Point<int> dchunk;
                Point<int> doffset;
                MinecraftRenderer.ChunkOffset(new Point<int>((int)vec.X, (int)vec.Y), out dchunk, out doffset);
                this._Renderer.GetChunk(dchunk);
                this._RendererTrans.GetChunk(dchunk);

            });

            this.KeyPress += delegate(object sender, OpenTK.KeyPressEventArgs e)
            {
                char key = e.KeyChar.ToString().ToLower()[0];
                if (key == 'b')
                {
                    this._Bookmarks.ShowForm(this._CamPos, new Vector<double>(this._LookXAngle, 0.0, this._LookZAngle));
                }
                if (key == ',')
                {
                    this._SliceLevel = this._SlicePolarity == Polarity.Negative ? this._SliceLevel - 1 : this._SliceLevel + 1;
                }
                if (key == '.')
                {
                    this._SliceLevel = this._SlicePolarity == Polarity.Negative ? this._SliceLevel + 1 : this._SliceLevel - 1;
                }
                if (key == 'h')
                {
                    frmHelp frm = new frmHelp();
                    frm.Show();
                }
                if (key == 'k')
                {
                    frmKey k = new frmKey(this._Renderer.CurrentScheme);
                    k.Show();
                }
                if (key == 't')
                {
                    frmSchemes schemeform = new frmSchemes(delegate(string item)
                    {
                        if (item.Length < 1)
                            return;
                        this._Renderer.CurrentScheme = this._Schemes[item];
                        this._RendererTrans.CurrentScheme = this._Schemes[item];
                    });

                    int index = 0;
                    foreach(string k in this._Schemes.Keys)
                    {
                        ListViewItem item = new ListViewItem(k);
                        schemeform.Schemes.Items.Add(item);

                        Scheme o;
                        this._Schemes.TryGetValue(k, out o);
                        if (this._Renderer.CurrentScheme == o)
                            schemeform.Schemes.Items[index].Selected = true;
                        index++;
                    }

                    schemeform.Show();
                    
                }
            };

            this.Keyboard.KeyDown += delegate(object sender, KeyboardKeyEventArgs e)
            {
                if (e.Key == Key.Escape)
                {
                    this.Close();
                }
                if (e.Key == Key.R)
                {
                    this._SliceAxis = null;
                }
            };

            this.Mouse.ButtonDown += delegate(object sender, MouseButtonEventArgs bea)
            {
                if (bea.Button == MouseButton.Left)
                {
                    if (this._LastHit != null)
                    {

                        Border<MinecraftRenderer.HitBorder> hit = this._LastHit.Value;
                        MinecraftRenderer.Chunk c;
                        bool loaded = this._Renderer.Chunks.TryGetValue(hit.Value.Chunk.Value, out c);
                        if (loaded)
                        {
                            if (this.Keyboard[Key.Q])
                            {
                                this._Renderer.UnloadChunk(hit.Value.Chunk.Value);
                                this._RendererTrans.UnloadChunk(hit.Value.Chunk.Value);
                            }
                            else if (this.Keyboard[Key.I])
                            {
                                // TODO!!!!!!!!!!!!!!!!!!
                                Point<int> pos = hit.Value.Chunk.Value;


                                long regionX = (long)Math.Floor((decimal)pos.X / 32);
                                long regionZ = (long)Math.Floor((decimal)pos.Y / 32);

                                string file = Path.DirectorySeparatorChar + "region" + Path.DirectorySeparatorChar + "r." +
                                    Convert.ToString(regionX) + "." + Convert.ToString(regionZ) + ".mcr";

                                string msg = String.Format("Chunk: X {0} Y {1}\nFile: {2}", pos.X, pos.Y, file);
                                msg += String.Format("\nX {0} Y {1} Z {2}", hit.Position.Y, hit.Position.Z, hit.Position.X);

                                MessageBox.Show(msg);
                            }
                            else
                            {
                                this._SliceAxis = hit.Direction;
                                this._SliceLevel = hit.Position[hit.Direction] + (hit.Value.Polarity == Polarity.Negative ? 1 : 0);
                                this._SlicePolarity = hit.Value.Polarity;
                            }
                        }
                        else
                        {
                            if (this.Keyboard[Key.E])
                            {
                                bool a;
                                Point<int> pos = hit.Value.Chunk.Value;

                                if (this._Renderer.ChunksSelected.TryGetValue(pos, out a))
                                    this._Renderer.ChunksSelected.Remove(pos);
                                else
                                    this._Renderer.ChunksSelected.Add(pos, true);
                            }
                            else
                            {
                                if (this._Renderer.ChunksSelected.Count > 0)
                                {
                                    foreach (KeyValuePair<Point<int>, bool> kv in this._Renderer.ChunksSelected)
                                    {
                                        this._Renderer.GetChunk(kv.Key);
                                        this._RendererTrans.GetChunk(kv.Key);
                                    }
                                    this._Renderer.ChunksSelected.Clear();
                                }
                                else
                                {
                                    this._Renderer.GetChunk(hit.Value.Chunk.Value);
                                    this._RendererTrans.GetChunk(hit.Value.Chunk.Value);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (this._Renderer.ChunksSelected.Count > 0 && this.Keyboard[Key.E])
                        this._Renderer.ChunksSelected.Clear();
                }
            };

            

            this._Schemes = Schemes;
            this._Renderer.CurrentScheme = this._Schemes["Default"];
            this._AverageFPS = 60.0;

            
        }


        private Dictionary<string,Scheme> _Schemes;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            double fpsstability = 10.0;
            this._AverageFPS = (this.RenderFrequency + this._AverageFPS * fpsstability) / (fpsstability + 1);
            this.Title = DefaultTitle + " - Press H for help!"; // fps messurement is off

            GL.ClearColor(0.3f, 0.5f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4d proj = Matrix4d.CreatePerspectiveFieldOfView(0.9, (double)this.Width / (double)this.Height, 0.2, 600.0);
            GL.LoadMatrix(ref proj);
            Matrix4d view = Matrix4d.LookAt(
                Vector.FromDoubleVector(this._CamPos),
                Vector.FromDoubleVector(Vector.Add(this._CamPos, this.CamDirection)),
                new Vector3d(0.0, 0.0, 1.0));
            GL.MultMatrix(ref view);

            // Test picking
            int mx = this.Mouse.X;
            int my = this.Mouse.Y;
            int sx = this.ClientSize.Width;
            int sy = this.ClientSize.Height;
            Vector3d p = Unproject(new Vector3d(-1.0 + 2.0 * ((double)mx / (double)sx), 1.0 - 2.0 * ((double)my / (double)sy), 0.995), view, proj);
            Vector<double> tracestart = this._CamPos;
            Vector<double> traceend = new Vector<double>(p.X, p.Y, p.Z);
            Vector<double> dif = Vector.Normalize(Vector.Subtract(traceend, tracestart));
            traceend = Vector.Add(Vector.Multiply(dif, 400), tracestart);
            foreach (TraceHit<MinecraftRenderer.HitBorder> hb in Trace.TraceRay(this._Renderer.GetHitSurface(this.Slice), tracestart, traceend, MinecraftRenderer.HitBorder.Default))
            {
                if ((hb.Border.Value.Polarity == Polarity.Positive && tracestart[hb.Border.Direction] > hb.Border.Position[hb.Border.Direction]) ||
                    (hb.Border.Value.Polarity == Polarity.Negative && tracestart[hb.Border.Direction] < hb.Border.Position[hb.Border.Direction]))
                {
                    this._LastHit = hb.Border;
                    break;
                }
            }

            Point<int>? highlight = this._LastHit.HasValue ? this._LastHit.Value.Value.Chunk : null;
            if(this._SliceAxis.HasValue)
            {
                if (this._SliceAxis.Value == Axis.Z)
                {
                    this._SliceLevel = this._SliceLevel < 1 ? 1 : this._SliceLevel;
                    this._SliceLevel = this._SliceLevel > MinecraftRenderer.ChunkSize - 1 ? MinecraftRenderer.ChunkSize - 1 : this._SliceLevel;
                }
            }
            this._Renderer.Render(this._CamPos, this.Slice, highlight);
            this._RendererTrans.Render(this._CamPos, this.Slice, highlight);
            this.SwapBuffers();
        }

        /// <summary>
        /// Gets the current slice description.
        /// </summary>
        public MinecraftRenderer.Slice? Slice
        {
            get
            {
                if (this._SliceAxis.HasValue)
                {
                    Axis sliceax = this._SliceAxis.Value;
                    
                    if (this._SlicePolarity == Polarity.Negative)
                    {
                        return new MinecraftRenderer.Slice() { Axis = sliceax, StartLevel = this._SliceLevel, EndLevel = int.MaxValue / 2 };
                    }
                    else
                    {
                        return new MinecraftRenderer.Slice() { Axis = sliceax, StartLevel = int.MinValue / 2, EndLevel = this._SliceLevel };
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Projects a 2d screenspace coordinate to world coordinates.
        /// </summary>
        public static Vector3d Unproject(Vector3d Point, Matrix4d View, Matrix4d Proj)
        {
            Matrix4d ma = Matrix4d.Mult(View, Proj);
            Matrix4d ima = Matrix4d.Invert(ma);
            Vector4d coord = new Vector4d(Point.X, Point.Y, Point.Z, 1.0);
            Vector4d res = Vector4d.Transform(coord, ima);
            return new Vector3d(res.X / res.W, res.Y / res.W, res.Z / res.W);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            System.Windows.Forms.Application.DoEvents();

            // Mouse movements
            if (this.Mouse[MouseButton.Right])
            {
                var cursorpos = System.Windows.Forms.Cursor.Position;
                var pos = new Point<int>(cursorpos.X, cursorpos.Y);
                if (this._MouseHold.HasValue)
                {
                    Point<int> mousehold = this._MouseHold.Value;
                    Point<int> delta = Point.Subtract(pos, mousehold);

                    this._LookXAngle += (double)delta.Y * -0.005;
                    this._LookZAngle += (double)delta.X * 0.005;
                    
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point(mousehold.X, mousehold.Y);
                }
                else
                {
                    System.Windows.Forms.Cursor.Hide();
                    this._MouseHold = pos;
                }
            }
            else
            {
                if (this._MouseHold.HasValue)
                {
                    System.Windows.Forms.Cursor.Show();
                    this._MouseHold = null;
                }
            }

            int mdelta = this.Mouse.Wheel - this._MouseWheelPos;
            this._MouseWheelPos = this.Mouse.Wheel;
            this._SliceLevel = this._SlicePolarity == Polarity.Negative ? this._SliceLevel - mdelta : this._SliceLevel + mdelta;

            if (this.Keyboard[Key.Up]) this._LookXAngle += e.Time * 2;
            if (this.Keyboard[Key.Down]) this._LookXAngle -= e.Time * 2;
            if (this.Keyboard[Key.Left]) this._LookZAngle -= e.Time * 2;
            if (this.Keyboard[Key.Right]) this._LookZAngle += e.Time * 2;
            double camspeed = 32;
            Vector<double> movement = new Vector<double>();
            
            if (this.Keyboard[Key.W])
            {
                movement = Vector.Add(movement, this.CamDirection);
            }
            if (this.Keyboard[Key.S])
            {
                movement = Vector.Subtract(movement, this.CamDirection);
            }
            if (this.Keyboard[Key.A])
            {
                movement = Vector.Subtract(movement, Vector.Normalize(Vector.Cross(this.CamDirection, new Vector<double>(0.0, 0.0, 1.0))));
            }
            if (this.Keyboard[Key.D])
            {
                movement = Vector.Add(movement, Vector.Normalize(Vector.Cross(this.CamDirection, new Vector<double>(0.0, 0.0, 1.0))));
            }
            if (this.Keyboard[Key.Space])
            {
                movement = Vector.Add(movement, Vector.Normalize(new Vector<double>(0.0, 0.0, 1.0)));
            }

            bool cammove = false;
            cammove = Vector.Length(movement) > 0.0;

            if (cammove)
            {
                movement = Vector.Normalize(movement); // Diag doesent go faster...
                this._CamSpeedMultiplier *= Math.Pow(2.0, e.Time);
            }
            else
            {
                this._CamSpeedMultiplier = 1.0;
            }
            if (this.Keyboard[Key.ShiftLeft] || this.Keyboard[Key.ShiftRight])
            {
                camspeed *= 4;
                this._CamSpeedMultiplier = 1.0;
            }
            if (this.Keyboard[Key.ControlLeft] || this.Keyboard[Key.ControlRight])
            {
                camspeed /= 4;
                this._CamSpeedMultiplier = 1.0;
            }
            camspeed *= this._CamSpeedMultiplier;

            this._CamPos = Vector.Add(this._CamPos, Vector.Multiply(movement, e.Time * camspeed));

            this._LookXAngle = Math.Min(Math.PI / 2.01, this._LookXAngle);
            this._LookXAngle = Math.Max(-Math.PI / 2.01, this._LookXAngle);
            this._LookZAngle = this._LookZAngle % (Math.PI * 2.0);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(this.ClientSize);
        }

        /// <summary>
        /// Gets the direction vector of where the camera is facing.
        /// </summary>
        public Vector<double> CamDirection
        {
            get
            {
                double cosx = Math.Cos(this._LookXAngle);
                return new Vector<double>(Math.Sin(this._LookZAngle) * cosx, Math.Cos(this._LookZAngle) * cosx, Math.Sin(this._LookXAngle));
            }
        }

        /// <summary>
        /// The main portion of the window's title.
        /// </summary>
        public static string DefaultTitle = "MineViewer Alpha";

        private Point<int>? _MouseHold;
        private double _LookXAngle;
        private double _LookZAngle;
        private Vector<double> _CamPos;

        private int _MouseWheelPos;

        private Border<MinecraftRenderer.HitBorder>? _LastHit;
        private double _CamSpeedMultiplier;
        private double _AverageFPS;
        private int _SliceLevel;
        private Axis? _SliceAxis;
        private Polarity _SlicePolarity;
        private MinecraftRenderer _Renderer;
        private MinecraftRenderer _RendererTrans;
    }
}