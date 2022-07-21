using System;
using TrueCraft.Client.Rendering;

namespace TrueCraft.Client.Modelling
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ModellerEventArgs<T> : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public T Item { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public MeshBase Result { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsPriority { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="result"></param>
        /// <param name="isPriority"></param>
        public ModellerEventArgs(T item, MeshBase result, bool isPriority)
        {
            Item = item;
            Result = result;
            IsPriority = isPriority;
        }
    }
}
