using System.Collections.Generic;

namespace Flekosoft.Common.Serialization
{
    public interface ISerializabe
    {
        List<ISerializer> Serializers { get; }
        string InstanceName { get; }
    }
}
