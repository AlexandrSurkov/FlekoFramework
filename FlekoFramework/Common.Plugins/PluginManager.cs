using System.Collections.Generic;
using System.Collections.ObjectModel;
using Flekosoft.Common.Collection;

namespace Flekosoft.Common.Plugins
{
    public class PluginManager : ErrorNotifyDisposableBase
    {
        /// <summary>
        /// Create file system plugin provider
        /// </summary>
        /// <param name="path">path to folder with plugin files</param>
        /// <returns></returns>
        public static FileSystemPluginProvider CreateFileSystemProvider(string path)
        {
            return new FileSystemPluginProvider(path);
        }

        //#region Singleton part
        //// ReSharper disable once InconsistentNaming
        //public static PluginManager Instance { get; } = new PluginManager();
        //#endregion

        private readonly List<IPlugin> _plugins = new List<IPlugin>();
        public ListCollection<IPluginProvider> PluginProviders { get; } = new ListCollection<IPluginProvider>("Plugin providers collection", true);

        public PluginManager()
        {
        }

        public ReadOnlyCollection<IPlugin> GetPlugins()
        {
            if (_plugins.Count == 0) ReloadPlugins();
            return _plugins.AsReadOnly();
        }

        public void ReloadPlugins()
        {
            ClearPlugins();
            foreach (IPluginProvider pluginProvider in PluginProviders)
            {
                var plugins = pluginProvider.GetPlugins();
                foreach (IPlugin plugin in plugins)
                {
                    AppendDebugLogMessage($"Plugin {plugin.Name} ({plugin.Guid}) was loaded by {pluginProvider.Name} ");
                }
                _plugins.AddRange(plugins);
            }
        }

        void ClearPlugins()
        {
            foreach (IPlugin plugin in _plugins)
            {
                plugin.Dispose();
            }
            _plugins.Clear();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearPlugins();
            }
            foreach (IPluginProvider pluginProvider in PluginProviders)
            {
                pluginProvider.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
