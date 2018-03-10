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
            var provider1 = new TestPluginProvider("provider1");
            var provider2 = new TestPluginProvider("provider2");

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
            Assert.AreEqual("Plugin 0", plugins[0].Name);
            Assert.AreEqual("Description 0", plugins[0].Description);
            Assert.AreEqual(typeof(TestPluginInstance), plugins[0].Type);
            var instance = plugins[0].GetInstance();
            Assert.AreEqual(plugins[0].Type, instance.GetType());

            var provider3 = new TestPluginProvider("provider3");
            pm.PluginProviders.Add(provider3);

            provider3.PluginsCount = 4;

            plugins = pm.GetPlugins();
            Assert.AreEqual(provider1.PluginsCount, plugins.Count);
            var p = instance as TestPluginInstance;
            Assert.IsNotNull(p);
            Assert.IsFalse(p.IsDisposed);


            pm.ReloadPlugins();
            Assert.IsTrue(p.IsDisposed);

            plugins = pm.GetPlugins();
            Assert.AreEqual(provider1.PluginsCount + provider3.PluginsCount, plugins.Count);

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
            var provider1 = new TestPluginProvider("provider1");
            var provider2 = new TestPluginProvider("provider2");

            pm.PluginProviders.Add(provider1);
            pm.PluginProviders.Add(provider2);
            Assert.AreEqual(2, pm.PluginProviders.Count);
            Assert.AreEqual(provider1, pm.PluginProviders[0]);
            Assert.AreEqual(provider2, pm.PluginProviders[1]);

            Assert.IsFalse(provider2.IsDisposed);
            pm.PluginProviders.Remove(provider2);
            Assert.IsTrue(provider2.IsDisposed);

            var provider3 = new TestPluginProvider("provider3");
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
