using System;
using Flekosoft.Common.Plugins;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Plugins
{
    [TestClass]
    public class PluginManagerTests
    {
        [TestMethod]
        public void GetPluginsTest()
        {
            var pm = new PluginManager();

            Assert.AreEqual(0, pm.PluginProviders.Count);
            var provider1 = new TestPluginProvider1("provider1");
            var provider2 = new TestPluginProvider2("provider2");

            pm.PluginProviders.Add(provider1);
            pm.PluginProviders.Add(provider2);
            Assert.AreEqual(2, pm.PluginProviders.Count);
            Assert.AreEqual(provider1, pm.PluginProviders[0]);
            Assert.AreEqual(provider2, pm.PluginProviders[1]);

            Assert.IsFalse(provider2.IsDisposed);
            pm.PluginProviders.Remove(provider2);
            Assert.IsTrue(provider2.IsDisposed);


            provider1.PluginsCount = 1;

            var plugins = pm.GetPlugins();
            Assert.AreEqual(provider1.PluginsCount, plugins.Count);
            Assert.AreEqual("Plugin type 1 0", plugins[0].Name);
            Assert.AreEqual("Description 0", plugins[0].Description);
            Assert.AreEqual(typeof(TestPluginInstance1), plugins[0].Type);
            var instance = plugins[0].GetInstance();
            Assert.AreEqual(plugins[0].Type, instance.GetType());

            var provider3 = new TestPluginProvider3("provider3");
            pm.PluginProviders.Add(provider3);

            plugins = pm.GetPlugins();
            Assert.AreEqual(provider1.PluginsCount, plugins.Count);
            var p = instance as TestPluginInstance1;
            Assert.IsNotNull(p);
            Assert.IsFalse(p.IsDisposed);


            pm.ReloadPlugins();
            Assert.IsTrue(p.IsDisposed);

            plugins = pm.GetPlugins();
            Assert.AreEqual(provider1.PluginsCount + provider3.PluginsCount, plugins.Count);

            var ps = pm.GetPlugins<IDisposable>();
            Assert.AreEqual(0, ps.Count);

            var psI1 = pm.GetPlugins<ITestPluginType1>();
            Assert.AreEqual(provider1.PluginsCount + provider3.Plugins1Count, psI1.Count);

            var pst = pm.GetPlugins(typeof(TestPluginInstance1));
            Assert.AreEqual(provider1.PluginsCount + provider3.Plugins1Count, pst.Count);

            pst = pm.GetPlugins(typeof(TestPluginInstance2));
            Assert.AreEqual(provider3.Plugins2Count, pst.Count);

            var ps1 = pm.GetPlugins<TestPluginType1>();
            Assert.AreEqual(provider1.PluginsCount + provider3.Plugins1Count, ps1.Count);

            ps1 = pm.GetPlugins<TestPluginType1>(typeof(TestPluginInstance1));
            Assert.AreEqual(provider1.PluginsCount + provider3.Plugins1Count, ps1.Count);

            ps1 = pm.GetPlugins<TestPluginType1>(typeof(TestPluginInstance2));
            Assert.AreEqual(0, ps1.Count);

            ps1 = pm.GetPlugins<TestPluginType1>(typeof(IDisposable));
            Assert.AreEqual(provider1.PluginsCount + provider3.Plugins1Count, ps1.Count);

            var psI2 = pm.GetPlugins<ITestPluginType2>();
            Assert.AreEqual(provider3.Plugins2Count, psI2.Count);

            var ps2 = pm.GetPlugins<TestPluginType2>();
            Assert.AreEqual(provider3.Plugins2Count, ps2.Count);

            ps2 = pm.GetPlugins<TestPluginType2>(typeof(TestPluginInstance2));
            Assert.AreEqual(provider3.Plugins2Count, ps2.Count);

            ps2 = pm.GetPlugins<TestPluginType2>(typeof(TestPluginInstance1));
            Assert.AreEqual(0, ps2.Count);

            var name = "Plugin type 1 0";
            var pi = pm.GetPlugin(name);
            Assert.AreEqual(name, pi?.Name);

            var pit1 = pm.GetPlugin<ITestPluginType1>(name);
            Assert.AreEqual(name, pit1?.Name);

            var pit2 = pm.GetPlugin<TestPluginType1>(name);
            Assert.AreEqual(name, pit2?.Name);

            var pit3 = pm.GetPlugin<ITestPluginType2>(name);
            Assert.IsNull(pit3);

            var pit4 = pm.GetPlugin<TestPluginType2>(name);
            Assert.IsNull(pit4);


            Assert.IsFalse(provider1.IsDisposed);
            Assert.IsFalse(provider3.IsDisposed);
            pm.Dispose();
            Assert.IsTrue(provider1.IsDisposed);
            Assert.IsTrue(provider3.IsDisposed);
        }

        [TestMethod]
        public void AddRemoveProvidersTest()
        {
            var pm = new PluginManager();

            Assert.AreEqual(0, pm.PluginProviders.Count);
            var provider1 = new TestPluginProvider1("provider1");
            var provider2 = new TestPluginProvider2("provider2");

            pm.PluginProviders.Add(provider1);
            pm.PluginProviders.Add(provider2);
            Assert.AreEqual(2, pm.PluginProviders.Count);
            Assert.AreEqual(provider1, pm.PluginProviders[0]);
            Assert.AreEqual(provider2, pm.PluginProviders[1]);

            Assert.IsFalse(provider2.IsDisposed);
            pm.PluginProviders.Remove(provider2);
            Assert.IsTrue(provider2.IsDisposed);

            var provider3 = new TestPluginProvider2("provider3");
            pm.PluginProviders.Add(provider3);

            Assert.IsFalse(provider1.IsDisposed);
            Assert.IsFalse(provider3.IsDisposed);
            pm.Dispose();
            Assert.IsTrue(provider1.IsDisposed);
            Assert.IsTrue(provider3.IsDisposed);
        }

        [TestMethod]
        public void FileProviderTest()
        {
            var str = "123";
            var fp = PluginManager.CreateFileSystemProvider(str);
            Assert.AreEqual(str, fp.FilesFolder);
        }
    }
}
