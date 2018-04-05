using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Flekosoft.Common;
using Flekosoft.Common.Plugins;

namespace Flekosoft.UnitTests.Plugins
{
    interface ITestPluginType1 : IPlugin
    {

    }

    interface ITestPluginType2 : IPlugin
    {

    }

    class TestPluginType1 : Plugin, ITestPluginType1
    {
        public TestPluginType1(Guid guid, Type type, string name, string description, bool isSingleInstance) : base(guid, type, name, description, isSingleInstance)
        {
        }

        protected override object InternalGetInstance(object param)
        {
            return new TestPluginInstance1();
        }
    }

    class TestPluginType2 : Plugin, ITestPluginType2
    {
        public TestPluginType2(Guid guid, Type type, string name, string description, bool isSingleInstance) : base(guid, type, name, description, isSingleInstance)
        {
        }

        protected override object InternalGetInstance(object param)
        {
            return new TestPluginInstance2();
        }
    }

    class TestPluginInstance1 : DisposableBase
    {
        public string Name { get; set; }
    }

    class TestPluginInstance2 : DisposableBase
    {
        public string Name { get; set; }
    }

    class TestPluginProvider1 : PluginProvider
    {
        public int PluginsCount { get; set; } = 2;
        protected override ReadOnlyCollection<IPlugin> InternalGetPlugins()
        {
            var res = new List<IPlugin>();
            for (int i = 0; i < PluginsCount; i++)
            {
                res.Add(new TestPluginType1(Guid.NewGuid(), typeof(TestPluginInstance1), $"Plugin type 1 {i}", $"Description {i}", false));
            }
            return res.AsReadOnly();
        }
        public TestPluginProvider1(string name) : base(name)
        {
        }
    }

    class TestPluginProvider2 : PluginProvider
    {
        public int PluginsCount { get; set; } = 3;
        protected override ReadOnlyCollection<IPlugin> InternalGetPlugins()
        {
            var res = new List<IPlugin>();
            for (int i = 0; i < PluginsCount; i++)
            {
                res.Add(new TestPluginType2(Guid.NewGuid(), typeof(TestPluginInstance2), $"Plugin type 2 {i}", $"Description {i}", false));
            }
            return res.AsReadOnly();
        }
        public TestPluginProvider2(string name) : base(name)
        {
        }
    }

    class TestPluginProvider3 : PluginProvider
    {
        public int PluginsCount { get; set; } = 5;
        public int Plugins1Count { get; set; } = 2;
        public int Plugins2Count { get; set; } = 3;
        protected override ReadOnlyCollection<IPlugin> InternalGetPlugins()
        {
            var res = new List<IPlugin>();
            for (int i = 0; i < Plugins1Count; i++)
            {
                res.Add(new TestPluginType1(Guid.NewGuid(), typeof(TestPluginInstance1), $"Plugin type 1 {i}", $"Description {i}", false));
            }
            for (int i = 0; i < Plugins2Count; i++)
            {
                res.Add(new TestPluginType2(Guid.NewGuid(), typeof(TestPluginInstance2), $"Plugin type 2 {i}", $"Description {i}", false));
            }
            return res.AsReadOnly();
        }
        public TestPluginProvider3(string name) : base(name)
        {
        }
    }
}
