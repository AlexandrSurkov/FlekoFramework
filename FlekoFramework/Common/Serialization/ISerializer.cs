using System;

namespace Flekosoft.Common.Serialization
{
    public interface ISerializer : IDisposable
    {
        void Serialize();
        void Deserialize();
        void ClearSerializedData();
    }
}
