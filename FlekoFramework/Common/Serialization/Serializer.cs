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
