using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Flekosoft.Common.Logging;

namespace Flekosoft.Common.Collection
{
    public abstract class DictionaryCollectionBase<TK, TV> : CollectionBase
    {
        private readonly List<TV> _addUpdateList = new List<TV>();
        private readonly List<string> _updateLogList = new List<string>();
        private readonly List<TV> _removeUpdateList = new List<TV>();

        protected DictionaryCollectionBase(string collectionInstanceName, bool disposeItemsOnRemove) : base(collectionInstanceName, disposeItemsOnRemove)
        {
        }

        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <returns></returns>
        protected abstract bool InternalAdd(TK key, TV value);
        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <returns></returns>
        protected abstract bool InternalContainsKey(TK key);
        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <returns></returns>
        protected abstract bool InternalContainsValue(TV value);
        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <returns></returns>
        protected abstract bool InternalRemove(TK key);
        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <returns></returns>
        protected abstract TV InternalGetValue(TK key);
        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <returns></returns>
        protected abstract TK InternalKeyOf(TV value);
        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// Be careful. This method will be called into lock statement!
        /// </summary>
        /// <returns></returns>
        protected abstract ReadOnlyCollection<TV> InternalAsReadOnly();

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

        public bool Add(TK key, TV value)
        {
            bool res;
            if (InternalContainsKey(key))
            {
                var logStr = $"{InstanceName}: Key \"{key}\" allready exist";
                if (IsUpdateMode) _updateLogList.Add(logStr);
                else AppendLogMessage(new LogRecord(DateTime.Now, new List<string> { logStr }, LogRecordLevel.Info));
                return false;
            }
            lock (LockObject)
            {
                res = InternalAdd(key, value);
            }
            if (res)
            {
                var logStr = $"{InstanceName}: Item \"{value}\" with key \"{key}\" was added";
                if (IsUpdateMode) _updateLogList.Add(logStr);
                else AppendLogMessage(new LogRecord(DateTime.Now, new List<string> { logStr }, LogRecordLevel.Info));
                if (IsUpdateMode) _addUpdateList.Add(value);
                else OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<TV> { value }));
            }
            return res;
        }

        public bool ContainsValue(TV value)
        {
            lock (LockObject)
            {
                return InternalContainsValue(value);
            }
        }

        public bool ContainsKey(TK key)
        {
            lock (LockObject)
            {
                return InternalContainsKey(key);
            }
        }

        public bool Remove(TK key)
        {
            bool res;
            if (!InternalContainsKey(key))
            {
                var logStr = $"{InstanceName}: Key \"{key}\" does not exist";
                if (IsUpdateMode) _updateLogList.Add(logStr);
                else AppendLogMessage(new LogRecord(DateTime.Now, new List<string> { logStr }, LogRecordLevel.Info));
                return false;
            }
            var value = this[key];

            lock (LockObject)
            {
                res = InternalRemove(key);
            }
            if (res)
            {
                var logStr = $"{InstanceName}: Item \"{value}\" with key \"{key}\" was removed";
                if (IsUpdateMode) _updateLogList.Add(logStr);
                else AppendLogMessage(new LogRecord(DateTime.Now, new List<string> { logStr }, LogRecordLevel.Info));

                if (IsUpdateMode) _addUpdateList.Add(value);
                else OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<TV> { value }));
                TryToDispose(value);
            }
            return res;
        }

        public TV this[TK key]
        {
            get
            {
                lock (LockObject)
                {
                    return InternalGetValue(key);
                }
            }
        }

        public TK KeyOf(TV value)
        {
            lock (LockObject)
            {
                return InternalKeyOf(value);
            }
        }

        public ReadOnlyCollection<TV> AsReadOnly()
        {
            lock (LockObject)
            {
                return InternalAsReadOnly();
            }
        }
    }
}
