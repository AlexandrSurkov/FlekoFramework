using System;
using System.IO;
using System.Xml;

namespace Flekosoft.Common.Serialization.Xml
{
    public abstract class SerializerXml<T> : Serializer<T>, ISerializerXml
    {
        private readonly object _lockObject = new object();
        private readonly string _rootName;
        private SerializerXml(T serializableObject, string rootName, string storagePath, string fileName, bool storeToFile) : base(serializableObject)
        {
            _rootName = rootName;
            XmlDocument = new XmlDocument();
            XmlDeclaration dec = XmlDocument.CreateXmlDeclaration("1.0", null, null);
            XmlDocument.AppendChild(dec);
            XmlDocument.AppendChild(XmlDocument.CreateElement(_rootName));

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

        protected SerializerXml(T serializableObject, string rootName) : this(serializableObject, rootName, string.Empty, string.Empty, false)
        {

        }

        protected SerializerXml(T serializableObject, string rootName, string storagePath, string fileName) : this(serializableObject, rootName, storagePath, fileName, true)
        {

        }

        public XmlDocument XmlDocument { get; }
        public XmlElement XmlRoot => (XmlElement)XmlDocument.ChildNodes[1];
        public string StoragePath { get; } = string.Empty;
        public string FileName { get; } = string.Empty;
        public bool StoreToFile { get; }

        public override void Serialize()
        {
            if (!IsEnabled) return;
            ClearXmlRoot();
            base.Serialize();
            SaveToFile();
        }

        public override void Deserialize()
        {
            if (!IsEnabled) return;
            LoadFromFile();
            if (XmlDocument.ChildNodes.Count == 0)
            {
                XmlDeclaration dec = XmlDocument.CreateXmlDeclaration("1.0", null, null);
                XmlDocument.AppendChild(dec);
                XmlDocument.AppendChild(XmlDocument.CreateElement(_rootName));
            }
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
            lock (_lockObject)
            {
                if (StoreToFile)
                {
                    if (!Directory.Exists(StoragePath))
                    {
                        Directory.CreateDirectory(StoragePath);
                    }
                    XmlDocument.Save(StoragePath + FileName);
                }
            }
        }

        protected void LoadFromFile()
        {
            if (StoreToFile)
            {
                ClearXmlRoot();
                try
                {
                    if (File.Exists(StoragePath + FileName))
                        XmlDocument.Load(StoragePath + FileName);
                }
                catch (Exception e)
                {
                    AppendExceptionLogMessage(e);
                }
            }
        }

        protected bool CheckFileExist()
        {
            if (!File.Exists(StoragePath + FileName))
            {
                //AppendLogMessage(new LogRecord(DateTime.Now, new List<string> { $"Failed to deserialize {SerializableObject.ToString()}: File doesn't exist" }, LogRecordLevel.Info));
                return false;
            }
            return true;
        }
    }
}
