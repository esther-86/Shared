using System;
using System.IO;
using System.Xml.Serialization;

namespace Common.Utilities
{
    /// <summary>
    /// Contains overwriteable method to allow users to specify custom post-processing for the deserialized object
    ///     If there is inheritance where B : A : CustomSerialization and A has custom DoPostProcessing code,
    ///     if there are array elements in B, need to call DoPostProcessing on each array element
    /// </summary>
    public abstract class CustomSerialization
    {
        public virtual void DoPostProcessing() { }
    }

    public class XmlTools
    {
        // http://stackoverflow.com/questions/8417225/parse-xml-using-linq-to-xml-to-class-objects
        public static T Deserialize<T>(string xml)
            where T : CustomSerialization
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return Deserialize<T>(serializer, xml);
        }

        public static T Deserialize<T>(Type type, string xml)
            where T : CustomSerialization
        {
            XmlSerializer serializer = new XmlSerializer(type);
            return Deserialize<T>(serializer, xml);
        }

        static T Deserialize<T>(XmlSerializer serializer, string xml)
            where T : CustomSerialization
        {
            try
            {
                using (var tr = new StringReader(xml))
                {
                    T deserializedObject = (T)serializer.Deserialize(tr);
                    deserializedObject.DoPostProcessing();
                    return (T)deserializedObject;
                }
            }
            catch (Exception ex)
            {
                // Usually, the inner exception for serialization is the more informative one
                throw new Exception(string.Format(
                    "Exception: {0}\r\nInner exception: {1}",
                    ex.Message, ex.InnerException)); 
            }
        }
    }
}
