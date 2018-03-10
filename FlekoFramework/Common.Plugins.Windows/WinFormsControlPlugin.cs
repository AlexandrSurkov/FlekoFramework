using System;
using System.Windows.Forms;
using Flekosoft.Common.Plugins;

namespace Common.Plugins.Windows
{
    public abstract class WinFormsControlPlugin : Plugin, IWinFormsControlPlugin
    {
        protected WinFormsControlPlugin(Guid guid, Type type, string name, string description, bool isSingleInstance) : base(guid, type, name, description, isSingleInstance)
        {
        }

        protected abstract ContainerControl InternalGetControl();


        public ContainerControl GetControl()
        {
            return InternalGetControl();
        }
    }
}
