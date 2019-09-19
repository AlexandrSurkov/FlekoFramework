using System;

namespace Flekosoft.Common.Serialization
{
    public abstract class Serializer<T> : PropertyChangedErrorNotifyDisposableBase, ISerializer
    {
        readonly PropertyChangedErrorNotifyDisposableBase _propertyChangedObject;
        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged(nameof(_isEnabled));
                }
            }
        }

        protected Serializer(T serializableObject)
        {
            if (!(serializableObject is ISerializable)) throw new ArgumentException("serializableObject must implement ISerializable");
            SerializableObject = serializableObject;

            _propertyChangedObject = serializableObject as PropertyChangedErrorNotifyDisposableBase;
            if (_propertyChangedObject != null)
            {
                _propertyChangedObject.PropertyChanged += SerializableObject_PropertyChanged;
            }
        }

        protected virtual bool CheckPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            return true;
        }

        private void SerializableObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (IsDisposed) return;
            if (CheckPropertyChanged(sender, e)) Serialize();
        }

        public virtual void Serialize()
        {
            if (IsDisposed) return;
            if (!IsEnabled) return;
            InternalSerialize();
            AppendDebugLogMessage(!string.IsNullOrEmpty(_propertyChangedObject?.InstanceName)
                ? $"{_propertyChangedObject.InstanceName}: Serialized"
                : $"{_propertyChangedObject}: Serialized");
        }

        public virtual void Deserialize()
        {
            if (IsDisposed) return;
            if (!IsEnabled) return;
            if (_propertyChangedObject != null) _propertyChangedObject.PropertyChanged -= SerializableObject_PropertyChanged;
            InternalDeserialize();
            if (_propertyChangedObject != null)
            {
                _propertyChangedObject.PropertyChanged += SerializableObject_PropertyChanged;
                AppendDebugLogMessage(!string.IsNullOrEmpty(_propertyChangedObject.InstanceName)
                    ? $"{_propertyChangedObject.InstanceName}: Deserialized"
                    : $"{_propertyChangedObject}: Deserialized");
            }
        }

        protected T SerializableObject { get; }

        public abstract void InternalSerialize();
        public abstract void InternalDeserialize();
        public abstract void ClearSerializedData();
    }
}
