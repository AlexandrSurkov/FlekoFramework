using System;
using System.ComponentModel;

namespace Flekosoft.Common.Collection.ComponentModel
{
    class DictionaryCollectionItemPropertyDescription<TK, TV> : PropertyDescriptor
    {
        private readonly DictionaryCollectionBase<TK,TV> _collection;
        private readonly TK _key;

        public DictionaryCollectionItemPropertyDescription(DictionaryCollectionBase<TK, TV> coll,
            TK key) : base("#" + key, null)
        {
            _collection = coll;
            _key = key;
        }

        public override AttributeCollection Attributes => new AttributeCollection(null);

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override Type ComponentType => _collection.GetType();

        public override string DisplayName => $"{_key}";

        public override string Description => string.Empty;

        public override object GetValue(object component)
        {
            if (_collection.ContainsKey(_key))
                return _collection[_key];
            return null;
        }

        public override bool IsReadOnly => true;

        public override string Name => "#" + _key;

        public override Type PropertyType
        {
            get {
                if (_collection.ContainsKey(_key))
                    return _collection[_key].GetType();
                return null;
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
