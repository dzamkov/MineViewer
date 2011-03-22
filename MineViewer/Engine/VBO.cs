using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Cubia
{
    /// <summary>
    /// Data for a single vertex in a VBO.
    /// </summary>
    public interface IVertex
    {
        /// <summary>
        /// Writes the vertex data directly (using GL.Vertex3, GL.Color4, etc).
        /// </summary>
        void Write();

        /// <summary>
        /// Writes the vertex data to the specified memory address.
        /// </summary>
        unsafe void Write(void* Memory);

        /// <summary>
        /// Sets the current vertex format.
        /// </summary>
        void Set();

        /// <summary>
        /// Gets the amount of bytes per vertex.
        /// </summary>
        int Size { get; }
    }

    /// <summary>
    /// A vertex with a color and a normal.
    /// </summary>
    public struct ColorNormalVertex : IVertex
    {
        /// <summary>
        /// The position of the vertex.
        /// </summary>
        public Vector<double> Position;

        /// <summary>
        /// The normal of the vertex.
        /// </summary>
        public Vector<double> Normal;

        /// <summary>
        /// The color of the vertex.
        /// </summary>
        public Color Color;

        void IVertex.Write()
        {
            GL.Color4(this.Color);
            GL.Normal3(Vector.FromDoubleVector(this.Normal));
            GL.Vertex3(Vector.FromDoubleVector(this.Position));
        }

        unsafe void IVertex.Write(void* Memory)
        {
            float* mem = (float*)Memory;
            mem[0] = (float)this.Color.R;
            mem[1] = (float)this.Color.G;
            mem[2] = (float)this.Color.B;
            mem[3] = (float)this.Color.A;
            mem[4] = (float)this.Normal.X;
            mem[5] = (float)this.Normal.Y;
            mem[6] = (float)this.Normal.Z;
            mem[7] = (float)this.Position.X;
            mem[8] = (float)this.Position.Y;
            mem[9] = (float)this.Position.Z;
        }

        void IVertex.Set()
        {
            GL.InterleavedArrays(InterleavedArrayFormat.C4fN3fV3f, sizeof(float) * 10, IntPtr.Zero);
        }

        int IVertex.Size
        {
            get
            {
                return sizeof(float) * 10;
            }
        }
    }

    /// <summary>
    /// A vertex with a texture coordinate and a normal.
    /// </summary>
    public struct TextureNormalVertex : IVertex
    {
        /// <summary>
        /// The position of the vertex.
        /// </summary>
        public Vector<double> Position;

        /// <summary>
        /// The normal of the vertex.
        /// </summary>
        public Vector<double> Normal;

        /// <summary>
        /// The texture coordinate for the vertex.
        /// </summary>
        public Point<double> UV;

        void IVertex.Write()
        {
            GL.TexCoord2(this.UV.X, this.UV.Y);
            GL.Normal3(Vector.FromDoubleVector(this.Normal));
            GL.Vertex3(Vector.FromDoubleVector(this.Position));
        }

        unsafe void IVertex.Write(void* Memory)
        {
            float* mem = (float*)Memory;
            mem[0] = (float)this.UV.X;
            mem[1] = (float)this.UV.Y;
            mem[2] = (float)this.Normal.X;
            mem[3] = (float)this.Normal.Y;
            mem[4] = (float)this.Normal.Z;
            mem[5] = (float)this.Position.X;
            mem[6] = (float)this.Position.Y;
            mem[7] = (float)this.Position.Z;
        }

        void IVertex.Set()
        {
            GL.InterleavedArrays(InterleavedArrayFormat.T2fN3fV3f, sizeof(float) * 8, IntPtr.Zero);
        }

        int IVertex.Size
        {
            get 
            {
                return sizeof(float) * 8;
            }
        }
    }

    /// <summary>
    /// A vertex buffer containing vertices of different types and modes.
    /// </summary>
    public class HeterogenousVertexBuffer : IRenderable
    {
        private HeterogenousVertexBuffer(Dictionary<KeyValuePair<Type, BeginMode>, IRenderable> VBOs)
        {
            this._VBOs = VBOs;
        }

        public HeterogenousVertexBuffer(IEnumerable<IVertexRenderable> Source)
        {
            var it = Create();
            Iterator.Pipe(it, Source);
            this._VBOs = it.End()._VBOs;
        }

        /// <summary>
        /// Gets an iterator that can be used to create a heterogenous vertex buffer.
        /// </summary>
        public static IIterator<IVertexRenderable, HeterogenousVertexBuffer> Create()
        {
            return new _IteratorConstructor();
        }

        private class _IteratorConstructor : IIterator<IVertexRenderable, HeterogenousVertexBuffer>
        {
            public _IteratorConstructor()
            {
                this.Selector = new _Selector() { VertexStore = this.VertexStore = new Dictionary<KeyValuePair<Type, BeginMode>, _UntypedVertexList>() };
            }

            public void Next(IVertexRenderable Input)
            {
                Input.Select<object>(this.Selector);
            }

            public HeterogenousVertexBuffer End()
            {
                Dictionary<KeyValuePair<Type, BeginMode>, IRenderable> vbos = new Dictionary<KeyValuePair<Type, BeginMode>, IRenderable>();
                foreach (KeyValuePair<KeyValuePair<Type, BeginMode>, _UntypedVertexList> vbo in this.VertexStore)
                {
                    vbos.Add(vbo.Key, vbo.Value.CreateVBO());
                }
                return new HeterogenousVertexBuffer(vbos);
            }

            public _Selector Selector;
            public Dictionary<KeyValuePair<Type, BeginMode>, _UntypedVertexList> VertexStore;
        }

        /// <summary>
        /// Used to select vertex renderables based on type.
        /// </summary>
        private class _Selector : IVertexRenderableTypeSelector<object>
        {
            public object Select<T>(IVertexRenderable<T> Renderable) 
                where T : struct, IVertex
            {
                Type verttype = typeof(T);
                BeginMode mode = Renderable.Mode;
                KeyValuePair<Type, BeginMode> key = new KeyValuePair<Type,BeginMode>(verttype, mode);
                _UntypedVertexList uvl;
                if (this.VertexStore.TryGetValue(key, out uvl))
                {
                    _VertexList<T> vl = (_VertexList<T>)uvl;
                    foreach (T v in Renderable.Vertices)
                    {
                        vl.Iterator.Next(v);
                    }
                }
                else
                {
                    _VertexList<T> vl = new _VertexList<T>() { Mode = mode, Iterator = VBO<T>.Create() };
                    this.VertexStore[key] = vl;
                }
                return null;
            }

            /// <summary>
            /// Dictionary of lists storing vertices.
            /// </summary>
            public Dictionary<KeyValuePair<Type, BeginMode>, _UntypedVertexList> VertexStore;
        }

        /// <summary>
        /// A homogenous list of vertices based on type.
        /// </summary>
        private interface _UntypedVertexList
        {
            /// <summary>
            /// Creates a vertex buffer object for the vertex list.
            /// </summary>
            IRenderable CreateVBO();
        }

        private class _VertexList<T> : _UntypedVertexList
            where T : struct, IVertex
        {
            /// <summary>
            /// Iterator used to create the vertex buffer.
            /// </summary>
            public IIterator<T, VBO<T>> Iterator;

            /// <summary>
            /// Begin mode for this set of vertices.
            /// </summary>
            public BeginMode Mode;

            public IRenderable CreateVBO()
            {
                return this.Iterator.End().GetRenderable(Mode);
            }
        }

        public void Render()
        {
            foreach (IRenderable ren in this._VBOs.Values)
            {
                ren.Render();
            }
        }

        private Dictionary<KeyValuePair<Type, BeginMode>, IRenderable> _VBOs;
    }

    /// <summary>
    /// A vertex buffer object, which can store and manipulate vertex data in graphics memory. The vertex data
    /// may also be rendered directly (which is much slower).
    /// </summary>
    /// <typeparam name="V">The type of vertex for the vertex buffer.</typeparam>
    public class VBO<V> : IDisposable
        where V : struct, IVertex
    {
        public VBO(IEnumerable<V> Vertices, int Count)
        {
            this._Count = Count;
            if (this._Count > 0)
            {
                this._First = Vertices.GetEnumerator().Current;
                GL.GenBuffers(1, out this._ArrayBuffer);
                if (this._ArrayBuffer != 0)
                {
                    int vertsize = this._First.Size;
                    GL.BindBuffer(BufferTarget.ArrayBuffer, this._ArrayBuffer);
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(this._Count * vertsize), IntPtr.Zero, BufferUsageHint.StaticDraw);
                    unsafe
                    {
                        byte* mem = (byte*)(GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly).ToPointer());
                        foreach (V vert in Vertices)
                        {
                            vert.Write(mem);
                            mem += vertsize;
                        }
                        GL.UnmapBuffer(BufferTarget.ArrayBuffer);
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        /// <summary>
        /// Returns an iterator that can be used to create a VBO given its vertices.
        /// </summary>

        public static IIterator<V, VBO<V>> Create()
        {
            return new _IteratorConstructor();
        }

        private class _IteratorConstructor : IIterator<V, VBO<V>>
        {
            public _IteratorConstructor()
            {
                this._Vertices = new List<V>();
            }

            public void Next(V Input)
            {
                this._Vertices.Add(Input);
            }

            public VBO<V> End()
            {
                return new VBO<V>(this._Vertices, this._Vertices.Count);
            }

            private List<V> _Vertices;
        }

        /// <summary>
        /// Gets a renderable to render the specified subset of vertices in the VBO with the given mode.
        /// </summary>
        public IVertexRenderable<V> GetRenderable(BeginMode Mode, int VertexStart, int Amount)
        {
            return new _VertexRenderable() { VBO = this, BeginMode = Mode, Amount = Amount, VertexStart = VertexStart };
        }

        /// <summary>
        /// Gets a renderable to render all the vertices in the VBO.
        /// </summary>
        public IVertexRenderable<V> GetRenderable(BeginMode Mode)
        {
            return new _VertexRenderable() { VBO = this, BeginMode = Mode, Amount = this._Count, VertexStart = 0 };
        }

        /// <summary>
        /// Gets the amount of vertices in the vertex buffer.
        /// </summary>
        public int Count
        {
            get
            {
                return this._Count;
            }
        }

        /// <summary>
        /// Vertex renderable that renders from this vbo.
        /// </summary>
        private class _VertexRenderable : IVertexRenderable<V>
        {
            public IEnumerable<V> Vertices
            {
                get 
                {
                    // This would involve reading from the graphics card, a little tricky.
                    throw new NotImplementedException();
                }
            }

            public BeginMode Mode
            {
                get 
                {
                    return this.BeginMode;
                }
            }

            public void Render()
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, this.VBO._ArrayBuffer);
                this.VBO._First.Set();
                GL.DrawArrays(this.BeginMode, this.VertexStart, this.Amount);
            }

            public R Select<R>(IVertexRenderableTypeSelector<R> Selector)
            {
                return Selector.Select<V>(this);
            }

            public VBO<V> VBO;
            public BeginMode BeginMode;
            public int VertexStart;
            public int Amount;   
        }

        public void Dispose()
        {
            GL.DeleteBuffers(1, ref this._ArrayBuffer);
        }

        private V _First;
        private int _Count;
        private uint _ArrayBuffer;
    }
}