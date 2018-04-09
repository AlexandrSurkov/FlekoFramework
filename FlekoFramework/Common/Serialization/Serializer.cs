using System;

namespace Flekosoft.Common.Serialization
{
    public abstract class Serializer<T> : DisposableBase, ISerializer
    {
        readonly PropertyChangedErrorNotifyDisposableBase _propertyChangedObject;

        protected Serializer(T serialisableObject)
        {
            if (!(serialisableObject is ISerializabe)) throw new ArgumentException("serialisableObject must implement ISerializabe");
            SerialisableObject = serialisableObject;

            _propertyChangedObject = serialisableObject as PropertyChangedErrorNotifyDisposableBase;
            if (_propertyChangedObject != null)
            {
                _propertyChangedObject.PropertyChanged += SerialisableObject_PropertyChanged;
            }
        }

        protected virtual bool CheckPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            return true;
        }

        private void SerialisableObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (IsDisposed) return;
            if (CheckPropertyChanged(sender, e)) Serialize();
        }

        public virtual void Serialize()
        {
            if (IsDisposed) return;
            InternalSerialize();
            AppendDebugLogMessage(!string.IsNullOrEmpty(_propertyChangedObject?.InstanceName)
                ? $"{_propertyChangedObject.InstanceName}: Serialized"
                : $"{_propertyChangedObject}: Serialized");
        }

        public virtual void Deserialize()
        {
            if (IsDisposed) return;
            if (_propertyChangedObject != null) _propertyChangedObject.PropertyChanged -= SerialisableObject_PropertyChanged;
            InternalDeserialize();
            if (_propertyChangedObject != null)
            {
                _propertyChangedObject.PropertyChanged += SerialisableObject_PropertyChanged;
                AppendDebugLogMessage(!string.IsNullOrEmpty(_propertyChangedObject.InstanceName)
                    ? $"{_propertyChangedObject.InstanceName}: Deserialized"
                    : $"{_propertyChangedObject}: Deserialized");
            }
        }

        protected T SerialisableObject { get; }

        public abstract void InternalSerialize();
        public abstract void InternalDeserialize();
        public abstract void ClearSerializedData();
    }
}
