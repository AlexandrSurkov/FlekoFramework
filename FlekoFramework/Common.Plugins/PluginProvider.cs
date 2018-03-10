using System.Collections.ObjectModel;

namespace Flekosoft.Common.Plugins
{
    public abstract class PluginProvider : PropertyChangedErrorNotifyDisposableBase, IPluginProvider
    {
        protected PluginProvider(string name) : base(name)
        {
            Name = name;
        }

        protected abstract ReadOnlyCollection<IPlugin> InternalGetPlugins();

        public ReadOnlyCollection<IPlugin> GetPlugins()
        {
            return InternalGetPlugins();
        }

        public string Name { get; }
    }
}
