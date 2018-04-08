using System.Xml;

namespace Flekosoft.Common.Serialization.Xml
{
    public abstract class CollectionSerializerXml<T>: CollectionSerializer<T>, ISerializerXml
    {
        protected CollectionSerializerXml(T serialisableObject, string rootName) : base(serialisableObject)
        {
            XmlDocument = new XmlDocument();
            XmlDeclaration dec = XmlDocument.CreateXmlDeclaration("1.0", null, null);
            XmlDocument.AppendChild(dec);
            XmlRoot = XmlDocument.CreateElement(rootName);
            XmlDocument.AppendChild(XmlRoot);
        }

        public XmlDocument XmlDocument { get;}
        public XmlElement XmlRoot { get; }
    }
}
