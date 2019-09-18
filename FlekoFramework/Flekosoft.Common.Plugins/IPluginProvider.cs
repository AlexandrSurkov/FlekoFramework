using System;
using System.Collections.ObjectModel;

namespace Flekosoft.Common.Plugins
{
    public interface IPluginProvider : IDisposable
    {
        ReadOnlyCollection<IPlugin> GetPlugins();
        string Name { get; }
    }
}
