using System;

namespace Flekosoft.Common.Plugins
{
    public interface IPlugin : IUniqueEntity
    {
        string Name { get; }
        string Description { get; }
        bool IsEnabled { get; set; }
        Type Type { get; }
        object GetInstance();
        bool IsSingleInstance { get; }
    }
}
