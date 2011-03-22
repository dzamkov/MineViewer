using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Cubia
{
    /// <summary>
    /// A command that can be used to draw graphics.
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        /// Renders to the current graphics context.
        /// </summary>
        void Render();
    }

    /// <summary>
    /// Used to determine the vertex type of a vertex renderable.
    /// </summary>
    /// <typeparam name="R">The return type from selection.</typeparam>
    public interface IVertexRenderableTypeSelector<R>
    {
        R Select<T>(IVertexRenderable<T> Renderable)
            where T : struct, IVertex;
    }

    /// <summary>
    /// Untyped vertex renderable.
    /// </summary>
    public interface IVertexRenderable
    {
        /// <summary>
        /// Selects based on the type of the vertex. This is not difficult.
        /// </summary>
        R Select<R>(IVertexRenderableTypeSelector<R> Selector);
    }

    /// <summary>
    /// A renderable that renders a list of vertices.
    /// </summary>
    public interface IVertexRenderable<T> : IRenderable, IVertexRenderable
        where T : struct, IVertex
    {
        /// <summary>
        /// Gets the vertices rendered.
        /// </summary>
        IEnumerable<T> Vertices { get; }

        /// <summary>
        /// Gets the rendering mode.
        /// </summary>
        BeginMode Mode { get; }
    }

    /// <summary>
    /// Nothing special, stores and renders (slowly) a collection of vertices.
    /// </summary>
    public sealed class EnumerableVertexRenderable<T> : IVertexRenderable<T>
        where T : struct, IVertex
    {
        public EnumerableVertexRenderable(IEnumerable<T> Vertices, BeginMode Mode)
        {
            this._Vertices = Vertices;
            this._Mode = Mode;
        }

        public IEnumerable<T> Vertices
        {
            get 
            {
                return this._Vertices;
            }
        }

        public BeginMode Mode
        {
            get 
            {
                return this._Mode;
            }
        }

        public void Render()
        {
            // The inefficency here shames me, promise to never use this?
            GL.Begin(this._Mode);
            foreach (IVertex vert in this._Vertices)
            {
                vert.Write();
            }
            GL.End();
        }

        public R Select<R>(IVertexRenderableTypeSelector<R> Selector)
        {
            return Selector.Select<T>(this);
        }

        private IEnumerable<T> _Vertices;
        private BeginMode _Mode;
    }

    /// <summary>
    /// A renderable that renders another renderable in between other rendering commands.
    /// </summary>
    public abstract class NestedRenderable : IRenderable
    {
        public NestedRenderable(IRenderable Base)
        {
            this._Base = Base;
        }

        /// <summary>
        /// Gets the main portion of what is rendered.
        /// </summary>
        public IRenderable Base
        {
            get
            {
                return this._Base;
            }
        }

        /// <summary>
        /// Does all prerendering commands before the main render.
        /// </summary>
        protected abstract void PreRender();

        /// <summary>
        /// Does all the postrendering commands after the main render. Usually to
        /// reset the state.
        /// </summary>
        protected abstract void PostRender();

        public void Render()
        {
            this.PreRender();
            this._Base.Render();
            this.PostRender();
        }

        private IRenderable _Base;
    }

    /// <summary>
    /// Renders a collection of renderables in no particular order.
    /// </summary>
    public sealed class UnionRenderable : IRenderable
    {
        public UnionRenderable(IEnumerable<IRenderable> Renderables)
        {
            this._Renderables = Renderables;
        }

        public void Render()
        {
            foreach (IRenderable renderable in this._Renderables)
            {
                renderable.Render();
            }
        }

        private IEnumerable<IRenderable> _Renderables;
    }
}