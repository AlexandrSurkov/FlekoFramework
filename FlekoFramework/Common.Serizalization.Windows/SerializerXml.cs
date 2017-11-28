//using System.Reflection;
//using Flekosoft.Common.Serialization;
//using System.Xml;

//namespace Common.Serizalization.Windows
//{
//    public class SerializerXml<T> : Serializer<T>
//    {
//        protected SerializerXml(T serialisableObject, XmlElement root) : base(serialisableObject)
//        {
//            Root = root;
//        }
//        protected XmlElement Root { get; }

//        public override void Serialize()
//        {
//            var pds = SerialisableObject.GetType().GetProperties();

//            foreach (PropertyInfo pd in pds)
//            {
//                var value = pd.GetValue(SerialisableObject, null);
//            }
//        }
//    }
//}
