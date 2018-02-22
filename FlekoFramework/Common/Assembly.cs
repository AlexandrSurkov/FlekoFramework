using System.Diagnostics;

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
    }
}
