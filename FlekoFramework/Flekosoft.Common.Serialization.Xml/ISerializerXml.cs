using System.Xml;

namespace Flekosoft.Common.Serialization.Xml
{
    public interface ISerializerXml : ISerializer
    {
        XmlDocument XmlDocument { get; }
        XmlElement XmlRoot { get; }
    }
}
