using System;
using System.ComponentModel;

namespace Flekosoft.Common.Collection.ComponentModel
{
    class ListCollectionItemPropertyDescription<T> : PropertyDescriptor
    {
        private readonly ListCollectionBase<T> _collection;
        private readonly int _index;

        public ListCollectionItemPropertyDescription(ListCollectionBase<T> coll,
            int idx) : base("#" + idx, null)
        {
            _collection = coll;
            _index = idx;
        }

        public override AttributeCollection Attributes => new AttributeCollection(null);

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override Type ComponentType => _collection.GetType();

        public override string DisplayName => $"{_index}";

        public override string Description
        {
            get
            {
                var tp = _collection[_index];
                return tp.ToString();
            }
        }

        public override object GetValue(object component)
        {
            if (_index >= _collection.Count) return null;
            return _collection[_index];
        }

        public override bool IsReadOnly => true;

        public override string Name => "#" + _index;

        public override Type PropertyType
        {
            get
            {
                if (_index >= _collection.Count) return null;
                return _collection[_index].GetType();
            }
        }

        public override void ResetValue(object component) { }

        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }

        public override void SetValue(object component, object value)
        {
            // this.collection[index] = value;
        }
    }
}
