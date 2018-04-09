using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Flekosoft.Common.Logging;

namespace Flekosoft.Common.Serialization.Xml
{
    public abstract class CollectionSerializerXml<T>: CollectionSerializer<T>, ISerializerXml
    {
        private CollectionSerializerXml(T serialisableObject, string rootName, string storagePath, string fileName, bool storeToFile) : base(serialisableObject)
        {
            XmlDocument = new XmlDocument();
            XmlDeclaration dec = XmlDocument.CreateXmlDeclaration("1.0", null, null);
            XmlDocument.AppendChild(dec);
            XmlRoot = XmlDocument.CreateElement(rootName);
            XmlDocument.AppendChild(XmlRoot);

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
        public XmlElement XmlRoot { get; }
        public string StoragePath { get; } = string.Empty;
        public string FileName { get; } = string.Empty;
        public bool StoreToFile { get; }


        public override void ClearSerializedData()
        {
            if (StoreToFile) File.Delete(StoragePath + FileName);
            XmlRoot.RemoveAll();
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
