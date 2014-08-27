using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.IO.Compression;

namespace Wx.Common.Utils
{
    public static class WxXml
    {
        public static string XmlSerialize(Encoding encoding, object obj)
        {
            if (obj == null)
                return string.Empty;

            using (MemoryStream ms = new MemoryStream())
            {
                new XmlSerializer(obj.GetType()).Serialize(ms, obj);
                return encoding.GetString(ms.ToArray());
            }
        }

        public static T XmlDeserialize<T>(Encoding encoding, string xmlString)
        {
            using (Stream stream = new MemoryStream(encoding.GetBytes(xmlString)))
            using (XmlReader xr = XmlReader.Create(stream))
            {
                Object obj = new XmlSerializer(typeof(T)).Deserialize(xr);
                return (T)obj;
            }
        }
    }
}
