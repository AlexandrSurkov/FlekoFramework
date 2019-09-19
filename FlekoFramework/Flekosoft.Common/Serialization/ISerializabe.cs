using System.Collections.Generic;

namespace Flekosoft.Common.Serialization
{
    public interface ISerializable
    {
        List<ISerializer> Serializers { get; }
    }
}
