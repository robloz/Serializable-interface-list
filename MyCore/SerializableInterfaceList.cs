using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace MyCore.Serializable
{
    public class SerializableInterfaceList<T> : List<T>, IXmlSerializable
        where T : class
    {
        public System.Xml.Schema.XmlSchema GetSchema() { return null; }
        public void ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
                return;

            reader.ReadStartElement();//root
            var listOfTypes = new Dictionary<string, string>();
            reader.ReadStartElement("Types");
            while (reader.Name.Equals("Type"))
            {
                listOfTypes.Add(reader.GetAttribute("Name"), reader.GetAttribute("AssemblyQualifiedName"));
                reader.Read();
            }

            if (listOfTypes.Any())
            {
                reader.ReadEndElement();
            }

            reader.ReadStartElement("Items");

            if (!listOfTypes.Any())
            {
                return;
            }

            while (!reader.Name.Equals("Items"))
            {
                var assembly = listOfTypes[reader.Name];
                Type type = Type.GetType(assembly);
                XmlSerializer serial = new XmlSerializer(type);
                this.Add((T)serial.Deserialize(reader));
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            var listOfTypes = new Dictionary<string, string>();

            foreach (var item in this)
            {
                if (!listOfTypes.ContainsKey(item.GetType().Name))
                {
                    listOfTypes.Add(item.GetType().Name, item.GetType().AssemblyQualifiedName);
                }

            }

            var ns = new XmlSerializerNamespaces();
            // add an empty namespace and empty value
            ns.Add(string.Empty, string.Empty);

            writer.WriteStartElement("Types");
            foreach (var listOfType in listOfTypes)
            {
                writer.WriteStartElement("Type");
                writer.WriteAttributeString("Name", listOfType.Key);
                writer.WriteAttributeString("AssemblyQualifiedName", listOfType.Value);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Items");
            foreach (T item in this)
            {
                var xmlSerializer = new XmlSerializer(item.GetType());
                xmlSerializer.Serialize(writer, item, ns);
            }
            writer.WriteEndElement();
        }

        private string GetNameWithoutGenericArity(Type t)
        {
            string name = t.Name;
            int index = name.IndexOf('`');
            return index == -1 ? name : name.Substring(0, index);
        }
    }
}