using System;

namespace Flekosoft.Common.Plugins
{
    public interface IPlugin : IUniqueEntity
    {
        string Name { get; }
        string Description { get; }
        Type Type { get; }
        object GetInstance();
        bool IsSingleInstance { get; }
    }
}
