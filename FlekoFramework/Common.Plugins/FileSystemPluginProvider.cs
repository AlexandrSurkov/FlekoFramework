using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

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
                var filesList = Directory.GetFiles(FilesFolder);
                foreach (string s in filesList)
                {
                    try
                    {
                        var dll = System.Reflection.Assembly.LoadFile(s);
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
