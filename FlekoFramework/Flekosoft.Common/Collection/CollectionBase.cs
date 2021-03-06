﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Flekosoft.Common.Logging;

namespace Flekosoft.Common.Collection
{
    public abstract class CollectionBase : PropertyChangedErrorNotifyDisposableBase, IEnumerable, INotifyCollectionChanged
    {
        protected readonly object LockObject = new object();

        protected CollectionBase(string collectionInstanceName, bool disposeItemsOnRemove) : base(collectionInstanceName)
        {
            DisposeItemsOnRemove = disposeItemsOnRemove;
            IsUpdateMode = false;
        }

        protected bool DisposeItemsOnRemove { get; }
        protected abstract IEnumerator InternalGetEnumerator();
        protected abstract void InternalClear();
        protected abstract int InternalGetCount();

        protected abstract void InternalBeginUpdate();
        protected abstract void InternalEndUpdate();

        protected bool IsUpdateMode { get; private set; }

        public void BeginUpdate()
        {
            IsUpdateMode = true;
            InternalBeginUpdate();
        }

        public void EndUpdate()
        {
            IsUpdateMode = false;
            InternalEndUpdate();
        }

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
            lock (LockObject)
            {
                InternalClear();
            }
            AppendLogMessage(new LogRecord(DateTime.Now, new List<string> { $"{InstanceName} was cleared" }, LogRecordLevel.Info));
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
            if (!IsDisposing) CollectionChanged?.Invoke(this, eventArgs);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clear();
                CollectionChanged = null;
            }
            base.Dispose(disposing);
            AppendDebugLogMessage($"{InstanceName} was Disposed");
        }
    }
}
