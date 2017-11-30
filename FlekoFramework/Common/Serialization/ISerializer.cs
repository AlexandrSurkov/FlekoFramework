namespace Flekosoft.Common.Serialization
{
    public interface ISerializer
    {
        void Serialize();
        void Deserialize();
        void ClearSerializedData();
    }
}
