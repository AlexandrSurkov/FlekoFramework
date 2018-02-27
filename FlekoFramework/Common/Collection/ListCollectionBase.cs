using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Flekosoft.Common.Logging;

namespace Flekosoft.Common.Collection
{
    public abstract class ListCollectionBase<T> : CollectionBase
    {
        private readonly List<T> _addUpdateList = new List<T>();
        private readonly List<string> _updateLogList = new List<string>();
        private readonly List<T> _removeUpdateList = new List<T>();

        protected ListCollectionBase(string collectionInstanceName, bool disposeItemsOnRemove) : base(collectionInstanceName, disposeItemsOnRemove)
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

        protected override void InternalBeginUpdate()
        {
            _addUpdateList.Clear();
            _removeUpdateList.Clear();
            _updateLogList.Clear();
        }

        protected override void InternalEndUpdate()
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _addUpdateList));
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _removeUpdateList));
            AppendLogMessage(new LogRecord(DateTime.Now, _updateLogList, LogRecordLevel.Info));
            _addUpdateList.Clear();
            _removeUpdateList.Clear();
            _updateLogList.Clear();
        }

        public bool Add(T item)
        {
            bool res;
            lock (LockObject)
            {
                res = InternalAdd(item);
            }
            if (res)
            {
                var logStr = $"{InstanceName}: The \"{item}\" was added";
                if (IsUpdateMode) _updateLogList.Add(logStr);
                else AppendLogMessage(new LogRecord(DateTime.Now, new List<string> { logStr }, LogRecordLevel.Info));
                
                if (IsUpdateMode) _addUpdateList.Add(item);
                else OnCollectionChanged(
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
                var logStr = $"{InstanceName}: The {item} was removed";
                if (IsUpdateMode) _updateLogList.Add(logStr);
                else AppendLogMessage(new LogRecord(DateTime.Now, new List<string> { logStr }, LogRecordLevel.Info));

                if (IsUpdateMode) _removeUpdateList.Add(item);
                else OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<T> { item }));
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
                    var logStr = $"{InstanceName}: The {item} was removed";
                    if (IsUpdateMode) _updateLogList.Add(logStr);
                    else AppendLogMessage(new LogRecord(DateTime.Now, new List<string> { logStr }, LogRecordLevel.Info));
                    
                    if (IsUpdateMode) _removeUpdateList.Add(item);
                    else  OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<T> { item }));
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
