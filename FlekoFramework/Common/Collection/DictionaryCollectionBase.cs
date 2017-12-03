using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Flekosoft.Common.Logging;

namespace Flekosoft.Common.Collection
{
    public abstract class DictionaryCollectionBase<TK, TV> : CollectionBase
    {
        protected DictionaryCollectionBase(string collectionName, bool disposeItemsOnRemove) : base(collectionName, disposeItemsOnRemove)
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

        public bool Add(TK key, TV value)
        {
            bool res;
            if (InternalContainsKey(key))
            {
                Logger.Instance.AppendLog(new LogRecord(DateTime.Now,
                    new List<string> { $"{Name}: Key \"{key}\" allready exist" }, LogRecordLevel.Info));
                return false;
            }
            lock (LockObject)
            {
                res = InternalAdd(key, value);
            }
            if (res)
            {
                Logger.Instance.AppendLog(new LogRecord(DateTime.Now,
                    new List<string> { $"{Name}: Item \"{value}\" with key \"{key}\" was added" }, LogRecordLevel.Info));
                OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<TV> { value }));
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
                Logger.Instance.AppendLog(new LogRecord(DateTime.Now,
                    new List<string> { $"{Name}: Key \"{key}\" does not exist" }, LogRecordLevel.Info));
                return false;
            }
            var value = this[key];

            lock (LockObject)
            {
                res = InternalRemove(key);
            }
            if (res)
            {
                Logger.Instance.AppendLog(new LogRecord(DateTime.Now,
                    new List<string> { $"{Name}: Item \"{value}\" with key \"{key}\" was removed" }, LogRecordLevel.Info));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<TV> { value }));
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
