using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Flekosoft.Common.Logging;

namespace Flekosoft.Common.Collection
{
    public abstract class CollectionBase: PropertyChangedErrorNotifyDisposableBase, IEnumerable,INotifyCollectionChanged
    {
        protected readonly object LockObject = new object();

        protected CollectionBase(string collectionName)
        {
            CollectionName = collectionName;
        }

        public string CollectionName { get;}
        protected abstract IEnumerator InternalGetEnumerator();
        protected abstract void InternalClear();
        protected abstract int InternalGetCount();

        public override string ToString()
        {
            return CollectionName;
        }

        public int Count
        {
            get
            {
                lock (LockObject)
                {
                    return InternalGetCount();
                }
            }
        }
        public void Clear()
        {
            lock (LockObject)
            {
                InternalClear();
            }
            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"{CollectionName} was cleared" }, LogRecordLevel.Info));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public IEnumerator GetEnumerator()
        {
            lock (LockObject)
            {
                return InternalGetEnumerator();
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs eventArgs)
        {
            // ReSharper disable once UseNullPropagation
            CollectionChanged?.Invoke(this, eventArgs);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clear();
                CollectionChanged = null;
            }
            base.Dispose(disposing);
        }
    }
}
