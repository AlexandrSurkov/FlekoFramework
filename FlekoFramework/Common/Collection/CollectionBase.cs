using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Flekosoft.Common.Logging;

namespace Flekosoft.Common.Collection
{
    public abstract class CollectionBase : PropertyChangedErrorNotifyDisposableBase, IEnumerable, INotifyCollectionChanged
    {
        protected readonly object LockObject = new object();

        protected CollectionBase(string collectionName, bool disposeItemsOnRemove):base(collectionName)
        {
            DisposeItemsOnRemove = disposeItemsOnRemove;
        }

        protected bool DisposeItemsOnRemove { get; }
        protected abstract IEnumerator InternalGetEnumerator();
        protected abstract void InternalClear();
        protected abstract int InternalGetCount();

        protected void TryToDispose(object obj)
        {
            if (DisposeItemsOnRemove)
            {
                var disposableItem = obj as IDisposable;
                disposableItem?.Dispose();
            }
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
            InternalClear(true);
        }

        private void InternalClear(bool sendCollectionChanged)
        {
            lock (LockObject)
            {
                InternalClear();
            }
            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"{Name} was cleared" }, LogRecordLevel.Info));
            if (sendCollectionChanged) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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
                SendPropertyChangedEvent = false;
                //Because of serialisation we should send CollectionChnagedEvent only on Clean not on Dispose
                InternalClear(false);
                CollectionChanged = null;
            }
            base.Dispose(disposing);
            AppendDebugMessage($"{Name} was Disposed");
        }
    }
}
