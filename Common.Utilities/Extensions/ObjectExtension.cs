using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace Common.Utilities
{
    public static class ObjectExtension
    {
        public static string GetValueToString(this object value, string defaultStr = "")
        {
            if (value == null)
                return defaultStr;
            return value.ToString();
        }

        /// <summary>
        /// Loop through each 
        /// </summary>
        public static void DoPerPropertyValueForObject(this object objectToGetPropertiesFor,
            Func<PropertyInfo, bool> shouldSkipProperty, Action<PropertyInfo, object> actionPerValue)
        {
            objectToGetPropertiesFor.GetType().DoPerPropertyValue(shouldSkipProperty,
                (pi) =>
                {
                    object value = pi.GetValue(objectToGetPropertiesFor, null);
                    actionPerValue.Invoke(pi, value);
                });
        }

        #region "XmlTools"
        // http://stackoverflow.com/questions/2434534/serialize-an-object-to-string
        public static string Serialize(this object toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }
        #endregion
    }
}
