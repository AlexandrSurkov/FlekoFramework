using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Flekosoft.Common.Collection
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public partial class DictionaryCollection<TK, TV> : DictionaryCollectionBase<TK, TV>, ICustomTypeDescriptor
    {
        protected Dictionary<TK, TV> InternalCollection { get; } = new Dictionary<TK, TV>();
        public DictionaryCollection(string collectionInstanceName, bool disposeItemsOnRemove) : base(collectionInstanceName, disposeItemsOnRemove)
        {
        }

        protected override IEnumerator InternalGetEnumerator()
        {
            return InternalCollection.GetEnumerator();
        }

        protected override void InternalClear()
        {
            foreach (TV value in InternalCollection.Values)
            {
                TryToDispose(value);
            }
            InternalCollection.Clear();
        }

        protected override int InternalGetCount()
        {
            return InternalCollection.Count;
        }

        protected override bool InternalAdd(TK key, TV value)
        {
            InternalCollection.Add(key, value);
            if (value is INotifyPropertyChanged ipc) ipc.PropertyChanged += Item_PropertyChanged;
            return true;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged($"Collection[{InternalKeyOf((TV)sender)}]." + e.PropertyName);
        }

        protected override bool InternalContainsKey(TK key)
        {
            return InternalCollection.ContainsKey(key);
        }

        protected override bool InternalContainsValue(TV value)
        {
            return InternalCollection.ContainsValue(value);
        }

        protected override bool InternalRemove(TK key)
        {
            return InternalCollection.Remove(key);
        }

        protected override TV InternalGetValue(TK key)
        {
            return InternalCollection[key];
        }

        protected override TK InternalKeyOf(TV value)
        {
            foreach (KeyValuePair<TK, TV> pair in InternalCollection)
            {
                if (pair.Value.Equals(value)) return pair.Key;
            }
            return default(TK);
        }

        protected override ReadOnlyCollection<TV> InternalAsReadOnly()
        {
            return new List<TV>(InternalCollection.Values).AsReadOnly();
        }
    }
}
