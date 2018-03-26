using System;
using System.Collections.Generic;

namespace Flekosoft.Common.Plugins
{
    public abstract class Plugin : PropertyChangedErrorNotifyDisposableBase, IPlugin
    {
        readonly List<object> _instancesList = new List<object>();

        protected Plugin(Guid guid, Type type, string name, string description, bool isSingleInstance) : base($"{name} ({guid})")
        {
            Guid = guid;
            Type = type;
            Name = name;
            Description = description;
            IsSingleInstance = isSingleInstance;
        }

        public Guid Guid { get; }

        public string Name { get; }

        public string Description { get; }

        public Type Type { get; }

        public object GetInstance()
        {
            if (!IsSingleInstance)
            {
                var instance = InternalGetInstance();
                _instancesList.Add(instance);
                return instance;
            }
            if (_instancesList.Count == 0)
            {
                var instance = InternalGetInstance();
                _instancesList.Add(instance);
                return instance;
            }
            return _instancesList[0];
        }

        public bool IsSingleInstance { get; }

        protected abstract object InternalGetInstance();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (object o in _instancesList)
                {
                    if (o is IDisposable d)
                    {
                        d.Dispose();
                    }
                }
            }
            base.Dispose(disposing);
        }
    }
}
