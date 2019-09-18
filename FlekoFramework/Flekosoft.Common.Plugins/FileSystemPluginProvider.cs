using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Flekosoft.Common.Plugins
{
    public class FileSystemPluginProvider : PluginProvider
    {
        public FileSystemPluginProvider(string filesFolder) : base("File system plugin provider")
        {
            FilesFolder = filesFolder;
            if (!Directory.Exists(FilesFolder))
            {
                Directory.CreateDirectory(FilesFolder);
            }
        }

        protected override ReadOnlyCollection<IPlugin> InternalGetPlugins()
        {
            var result = new List<IPlugin>();
            try
            {
                var filesList = Directory.GetFiles(FilesFolder, "*.dll");
                foreach (string s in filesList)
                {
                    try
                    {
                        //var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                        //var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();
                        //var toLoad = filesList.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();
                        //toLoad.ForEach(path => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));

                        var dll = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(s));
                        foreach (Type type in dll.GetExportedTypes())
                        {
                            var interfaces = type.GetInterfaces();
                            if (interfaces.Contains(typeof(IPlugin)))
                            {
                                result.Add((IPlugin)Activator.CreateInstance(type));
                            }
                        }
                    }
#pragma warning disable 168
                    catch (Exception ex)
#pragma warning restore 168
                    {
                        AppendExceptionLogMessage(ex);
                    }
                }

            }
            catch (Exception e)
            {
                AppendExceptionLogMessage(e);
            }
            return result.AsReadOnly();
        }

        public string FilesFolder { get; }
    }
}
