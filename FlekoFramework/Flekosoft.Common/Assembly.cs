using System.Diagnostics;
using System.IO;

namespace Flekosoft.Common
{
    public static class Assembly
    {
        public static bool IsAssemblyDebugBuild(System.Reflection.Assembly assembly)
        {
            foreach (var attribute in assembly.GetCustomAttributes(false))
            {
                if (attribute is DebuggableAttribute debuggableAttribute)
                {
                    return debuggableAttribute.IsJITTrackingEnabled;
                }
            }
            return false;
        }

        public static string GetPath(System.Reflection.Assembly assembly)
        {
            var file = assembly.Location;
            return Path.GetDirectoryName(file);
        }
    }
}
