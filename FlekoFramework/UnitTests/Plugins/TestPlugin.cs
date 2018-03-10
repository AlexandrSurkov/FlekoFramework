using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Flekosoft.Common;
using Flekosoft.Common.Plugins;

namespace Flekosoft.UnitTests.Plugins
{
    class TestPlugin : Plugin
    {
        public TestPlugin(Guid guid, Type type, string name, string description, bool isSingleInstance) : base(guid, type, name, description, isSingleInstance)
        {
        }

        protected override object InternalGetInstance()
        {
            return new TestPluginInstance();
        }
    }

    class TestPluginInstance : DisposableBase
    {
        public string Name { get; set; }
    }

    class TestPluginProvider : PluginProvider
    {
        public int PluginsCount { get; set; } = 1;
        protected override ReadOnlyCollection<IPlugin> InternalGetPlugins()
        {
            var res = new List<IPlugin>();
            for (int i = 0; i < PluginsCount; i++)
            {
                res.Add(new TestPlugin(Guid.NewGuid(), typeof(TestPluginInstance), $"Plugin {i}", $"Description {i}", false));
            }
            return res.AsReadOnly();
        }
        public TestPluginProvider(string name) : base(name)
        {
        }
    }
}
