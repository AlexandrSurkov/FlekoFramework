using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Flekosoft.Common.Collection
{
    public class ListCollection<T> : ListCollectionBase<T>
    {
        protected List<T> InternalCollection { get; } = new List<T>();

        public ListCollection(string collectionName, bool disposeItemsOnRemove) : base(collectionName, disposeItemsOnRemove)
        {
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override bool InternalAdd(T item)
        {
            InternalCollection.Add(item);
            if (item is INotifyPropertyChanged ipc) ipc.PropertyChanged += Item_PropertyChanged;
            return true;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged($"Collection[{InternalIndexOf((T)sender)}]." + e.PropertyName);
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override bool InternalContains(T item)
        {
            return InternalCollection.Contains(item);
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override bool InternalRemove(T item)
        {
            return InternalCollection.Remove(item);
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override T InternalGetItem(int index)
        {
            return InternalCollection[index];
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override int InternalIndexOf(T item)
        {
            return InternalCollection.IndexOf(item);
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
                InternalCollection.RemoveAt(index);
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
            return new List<T>(InternalCollection).AsReadOnly();
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator InternalGetEnumerator()
        {
            return InternalCollection.GetEnumerator();
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        protected override void InternalClear()
        {
            foreach (T item in InternalCollection)
            {
                TryToDispose(item);
            }
            InternalCollection.Clear();
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <returns></returns>
        protected override int InternalGetCount()
        {
            return InternalCollection.Count;
        }
    }
}
