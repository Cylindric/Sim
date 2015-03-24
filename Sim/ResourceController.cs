using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Sim
{
    class ResourceController
    {
        public static string GetDataFilename(string format, params object[] args)
        {
            return Path.Combine("Resources", "Data", string.Format(format, args));
        }

        public static string GetDataFilename(string name)
        {
            return Path.Combine("Resources", "Data", name);
        }

        public static string GetSpriteFilename(string name)
        {
            return Path.Combine("Resources", "Sprites", name);
        }

        public static void Save<T>(T obj, string filename)
        {
            try
            {
                var document = new XmlDocument();
                var serialiser = new XmlSerializer(obj.GetType());
                using (var stream = new MemoryStream())
                {
                    serialiser.Serialize(stream, obj);
                    stream.Position = 0;
                    document.Load(stream);
                    document.Save(filename);
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Unable to save data file.", e);
            }
        }

        public static T Load<T>(string filename)
        {
            var obj = default(T);

            try
            {
                var attributeXml = string.Empty;
                var document = new XmlDocument();
                document.Load(filename);
                var xml = document.OuterXml;

                using (var read = new StringReader(xml))
                {
                    var outType = typeof(T);

                    var serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        obj = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Unable to load data file.", e);
            }

            return obj;
        }
    }
}
