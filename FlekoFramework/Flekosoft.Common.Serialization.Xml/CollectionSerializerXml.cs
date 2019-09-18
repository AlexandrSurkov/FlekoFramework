using System.IO;
using System.Xml;

namespace Flekosoft.Common.Serialization.Xml
{
    public abstract class CollectionSerializerXml<T> : CollectionSerializer<T>, ISerializerXml
    {
        private CollectionSerializerXml(T serialisableObject, string rootName, string storagePath, string fileName, bool storeToFile) : base(serialisableObject)
        {
            XmlDocument = new XmlDocument();
            XmlDeclaration dec = XmlDocument.CreateXmlDeclaration("1.0", null, null);
            XmlDocument.AppendChild(dec);
            XmlDocument.AppendChild(XmlDocument.CreateElement(rootName));

            StoreToFile = storeToFile;

            if (StoreToFile)
            {
                StoragePath = storagePath;
                if (StoragePath[StoragePath.Length - 1] != Path.DirectorySeparatorChar)
                    StoragePath += Path.DirectorySeparatorChar;

                FileName = $"{fileName}.xml";

                if (!Directory.Exists(StoragePath))
                {
                    Directory.CreateDirectory(StoragePath);
                }
            }
        }

        protected CollectionSerializerXml(T serialisableObject, string rootName) : this(serialisableObject, rootName, string.Empty, string.Empty, false)
        {

        }

        protected CollectionSerializerXml(T serialisableObject, string rootName, string storagePath, string fileName) : this(serialisableObject, rootName, storagePath, fileName, true)
        {

        }

        public XmlDocument XmlDocument { get; }
        public XmlElement XmlRoot => (XmlElement)XmlDocument.ChildNodes[1];

        public string StoragePath { get; } = string.Empty;
        public string FileName { get; } = string.Empty;
        public bool StoreToFile { get; }

        public override void Serialize()
        {
            ClearXmlRoot();
            base.Serialize();
            SaveToFile();
        }

        public override void Deserialize()
        {
            LoadFromFile();
            base.Deserialize();
        }


        public override void ClearSerializedData()
        {
            if (StoreToFile)
            {
                if (File.Exists(StoragePath + FileName)) File.Delete(StoragePath + FileName);
            }
            ClearXmlRoot();
        }
        protected void ClearXmlRoot()
        {
            XmlRoot.RemoveAll();
        }

        protected void SaveToFile()
        {
            if (StoreToFile) XmlDocument.Save(StoragePath + FileName);
        }

        protected void LoadFromFile()
        {
            if (StoreToFile)
            {
                ClearXmlRoot();

                if (File.Exists(StoragePath + FileName))
                    XmlDocument.Load(StoragePath + FileName);
            }
        }

        protected bool CheckFileExist()
        {
            if (!File.Exists(StoragePath + FileName))
            {
                //AppendLogMessage(new LogRecord(DateTime.Now, new List<string> { $"Faled to deserialize {SerialisableObject.ToString()}: File doesn't exist" }, LogRecordLevel.Info));
                return false;
            }
            return true;
        }
    }
}
