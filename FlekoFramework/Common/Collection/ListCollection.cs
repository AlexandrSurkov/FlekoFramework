using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Flekosoft.Common.Collection
{
    public class ListCollection<T> : ListCollectionBase<T>
    {
        protected List<T> InternalList { get; } = new List<T>();

        public ListCollection(string collectionName) : base(collectionName)
        {
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override bool InternalAdd(T item)
        {
            InternalList.Add(item);
            return true;
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override bool InternalContains(T item)
        {
            return InternalList.Contains(item);
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override bool InternalRemove(T item)
        {
            return InternalList.Remove(item);
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override T InternalGetItem(int index)
        {
            return InternalList[index];
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override int InternalIndexOf(T item)
        {
            return InternalList.IndexOf(item);
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override bool InternalRemoveAt(int index)
        {
            try
            {
                InternalList.RemoveAt(index);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <returns></returns>
        protected override ReadOnlyCollection<T> InternalAsReadOnly()
        {
            return new List<T>(InternalList).AsReadOnly();
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator InternalGetEnumerator()
        {
            return InternalList.GetEnumerator();
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        protected override void InternalClear()
        {
            foreach (T item in InternalList)
            {
                var ds = item as IDisposable;
                ds?.Dispose();
            }
            InternalList.Clear();
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <returns></returns>
        protected override int InternalGetCount()
        {
            return InternalList.Count;
        }
    }
}
