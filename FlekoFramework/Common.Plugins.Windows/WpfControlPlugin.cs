using System;
using System.Windows.Controls;
using Flekosoft.Common.Plugins;

namespace Common.Plugins.Windows
{
    public abstract class WpfControlPlugin : Plugin, IWpfControlPlugin
    {
        protected WpfControlPlugin(Guid guid, Type type, string name, string description, bool isSingleInstance) : base(guid, type, name, description, isSingleInstance)
        {
        }

        protected abstract ContentControl InternalGetControl();


        public ContentControl GetControl()
        {
            return InternalGetControl();
        }
    }
}
