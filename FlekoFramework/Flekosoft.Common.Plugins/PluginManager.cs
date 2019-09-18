using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        public PluginManager() : base("PluginManager")
        {
        }

        public PluginManager(string instanceName) : base(instanceName)
        {
        }

        public ListCollection<IPluginProvider> PluginProviders { get; } = new ListCollection<IPluginProvider>("Plugin providers collection", true);

        /// <summary>
        /// Get all IPlugin instances
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<IPlugin> GetPlugins()
        {
            if (_plugins.Count == 0) ReloadPlugins();
            return _plugins.AsReadOnly();
        }

        /// <summary>
        /// Get all instances of type T where T inherits IPlugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ReadOnlyCollection<T> GetPlugins<T>()
        {
            var res = new List<T>();
            if (typeof(T) == typeof(IPlugin) || typeof(T).GetInterfaces().Contains(typeof(IPlugin)))
            {
                var pl = GetPlugins();
                foreach (IPlugin plugin in pl)
                {
                    var interfaces = plugin.GetType().GetInterfaces();
                    if (interfaces.Contains(typeof(T)) || plugin is T) res.Add((T)plugin);
                }
            }
            return res.AsReadOnly();
        }

        /// <summary>
        /// Get all instances of type T where T inherits IPlugin and IPlugin.Type equals type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instanceType"></param>
        /// <returns></returns>
        public ReadOnlyCollection<T> GetPlugins<T>(Type instanceType)
        {
            var res = new List<T>();
            var pl = GetPlugins<T>();
            foreach (var unknown in pl)
            {
                if (unknown is IPlugin plugin)
                {
                    if (plugin.Type == instanceType || plugin.Type.GetInterfaces().Contains(instanceType))
                        res.Add(unknown);
                }
            }
            return res.AsReadOnly();
        }

        public ReadOnlyCollection<IPlugin> GetPlugins(Type instanceType)
        {
            return GetPlugins<IPlugin>(instanceType);
        }

        public T GetPlugin<T>(string name)
        {
            var pl = GetPlugins<T>();

            foreach (var unknown in pl)
            {
                var plugin = unknown as IPlugin;
                if (plugin?.Name == name) return unknown;
            }
            return default(T);
        }

        public IPlugin GetPlugin(string name)
        {
            return GetPlugin<IPlugin>(name);
        }

        public T GetPlugin<T>(Guid guid)
        {
            var pl = GetPlugins<T>();

            foreach (var unknown in pl)
            {
                var plugin = unknown as IPlugin;
                if (plugin?.Guid == guid) return unknown;
            }
            return default(T);
        }

        public IPlugin GetPlugin(Guid guid)
        {
            return GetPlugin<IPlugin>(guid);
        }


        /// <summary>
        /// Reload all IPlugin instances from all providers
        /// </summary>
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
