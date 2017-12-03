using RestSharp;
using RestSharp.Deserializers;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Fapl.Flashcards.Shared.Helpers
{
    public class CustomXmlDeserializer : IDeserializer
    {
        public string DateFormat { get; set; }
        public string Namespace { get; set; }
        public string RootElement { get; set; }

        public T Deserialize<T>(IRestResponse response)
        {
            if (DateFormat != null) throw new NotImplementedException();
            if (Namespace != null) throw new NotImplementedException();
            if (RootElement != null) throw new NotImplementedException();

            using (TextReader contentReader = new StringReader(response.Content))
            using (XmlReader xmlReader = XmlReader.Create(contentReader))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(petfinder));
                return (T)serializer.Deserialize(xmlReader);
            }
        }
    }
}
