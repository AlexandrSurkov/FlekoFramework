using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Flekosoft.Common.Logging;

namespace Flekosoft.Common.Collection
{
    public abstract class ListCollectionBase<T> : CollectionBase
    {
        protected ListCollectionBase(string collectionName, bool disposeItemsOnRemove) : base(collectionName, disposeItemsOnRemove)
        {
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected abstract bool InternalAdd(T item);
        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected abstract bool InternalContains(T item);
        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected abstract bool InternalRemove(T item);
        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected abstract T InternalGetItem(int index);
        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected abstract int InternalIndexOf(T item);
        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected abstract bool InternalRemoveAt(int index);
        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <returns></returns>
        protected abstract ReadOnlyCollection<T> InternalAsReadOnly();

        public bool Add(T item)
        {
            bool res;
            lock (LockObject)
            {
                res = InternalAdd(item);
            }
            if (res)
            {
                AppendLogMessage(new LogRecord(DateTime.Now,
                    new List<string> { $"{Name}: The \"{item}\" was added" }, LogRecordLevel.Info));
                OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<T> { item }));
            }
            return res;
        }

        public bool Contains(T item)
        {
            lock (LockObject)
            {
                return InternalContains(item);
            }
        }

        public bool Remove(T item)
        {
            bool res;
            lock (LockObject)
            {
                res = InternalRemove(item);
            }
            if (res)
            {
                AppendLogMessage(new LogRecord(DateTime.Now,
                    new List<string> { $"{Name}: The {item} was removed" }, LogRecordLevel.Info));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<T> { item }));
                TryToDispose(item);
            }
            return res;
        }

        public T this[int index]
        {
            get
            {
                lock (LockObject)
                {
                    return InternalGetItem(index);
                }
            }
        }

        public int IndexOf(T item)
        {
            lock (LockObject)
            {
                return InternalIndexOf(item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (LockObject)
            {
                var item = InternalGetItem(index);
                bool res;
                lock (LockObject)
                {
                    res = InternalRemoveAt(index);
                }
                if (res)
                {
                    AppendLogMessage(new LogRecord(DateTime.Now,
                        new List<string> { $"{Name}: The {item} was removed" }, LogRecordLevel.Info));
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<T> { item }));
                    TryToDispose(item);
                }
            }
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            lock (LockObject)
            {
                return InternalAsReadOnly();
            }
        }
    }
}
