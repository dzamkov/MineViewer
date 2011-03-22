using System;
using System.Collections.Generic;

namespace Cubia
{
    /// <summary>
    /// Produces an object after being given a collection of values.
    /// </summary>
    /// <typeparam name="I">The type for the input values.</typeparam>
    /// <typeparam name="O">The output value.</typeparam>
    public interface IIterator<I, O>
    {
        /// <summary>
        /// Gives the next input to the iterator.
        /// </summary>
        void Next(I Input);

        /// <summary>
        /// Signifies the end of the input stream and gets the output value.
        /// </summary>
        O End();
    }

    /// <summary>
    /// Iterator helper functions.
    /// </summary>
    public static class Iterator
    {
        /// <summary>
        /// Descructively gives the values in the specified collection to the iterator without requesting its output.
        /// </summary>
        public static void Pipe<I, O>(IIterator<I, O> Iterator, IEnumerable<I> Collection)
        {
            foreach (I val in Collection)
            {
                Iterator.Next(val);
            }
        }

        /// <summary>
        /// Creates an iterator to add items to a list as they are added to the iterator.
        /// </summary>
        public static IIterator<I, List<I>> ListIterator<I>(List<I> List)
        {
            return new _ListIterator<I>() { List = List };
        }

        private class _ListIterator<I> : IIterator<I, List<I>>
        {
            public void Next(I Input)
            {
                this.List.Add(Input);
            }

            public List<I> End()
            {
                return this.List;
            }

            public List<I> List;
        }

        /// <summary>
        /// Transforms the output of an iterator with a mapping.
        /// </summary>
        public static IIterator<I, TO> TransformOutput<I, O, TO>(IIterator<I, O> Iterator, Func<O, TO> Mapping)
        {
            return new _TransformOutputIterator<I, O, TO>() { Source = Iterator, Mapping = Mapping };
        }

        private class _TransformOutputIterator<I, O, TO> : IIterator<I, TO>
        {
            public void Next(I Input)
            {
                this.Source.Next(Input);
            }

            public TO End()
            {
                return this.Mapping(this.Source.End());
            }

            public Func<O, TO> Mapping;
            public IIterator<I, O> Source;
        }
    }
}