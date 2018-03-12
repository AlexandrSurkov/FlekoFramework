using System;
using System.Windows.Controls;

namespace Flekosoft.Common.Plugins.Windows
{
    public abstract class WpfControlPlugin : Plugin, IWpfControlPlugin
    {
        protected WpfControlPlugin(Guid guid, Type type, string name, string description, bool isSingleInstance) : base(guid, type, name, description, isSingleInstance)
        {
        }

        protected abstract ContentControl InternalGetControl(object instance);


        public ContentControl GetControl(object instance)
        {
            if (Type == instance.GetType())
            {
                return InternalGetControl(instance);
            }
            return null;
        }
    }
}
