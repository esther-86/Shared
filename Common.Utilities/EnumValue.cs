using System.Text.RegularExpressions;
using System;

namespace Common.Utilities
{
    public class EnumValue : System.Attribute
    {
        public bool IsRequired { get; protected set; }
        public object Value { get; protected set; }

        public EnumValue(object value)
        {
            this.Value = value;
            this.IsRequired = false;
        }

        public EnumValue(bool isRequired, object sampleValueIfRequired)
        {
            this.Value = sampleValueIfRequired;
            this.IsRequired = isRequired;
        }

        public static string GetEnumValueAsString(Enum enumObject)
        {
            object retValue = GetEnumValue(enumObject);
            if (retValue == null)
                return "";
            return retValue.ToString();
        }

        public static EnumValue GetEnumValueObject(Enum enumObject)
        {
            EnumValue retValue = null;
            Type type = enumObject.GetType();
            //Look for the value in the field's custom attributes          
            System.Reflection.FieldInfo fi = type.GetField(enumObject.ToString());
            EnumValue[] attrs =
                fi.GetCustomAttributes(typeof(EnumValue), false) as EnumValue[];
            if (attrs.Length > 0)
                retValue = attrs[0];
            return retValue;
        }

        public static object GetEnumValue(Enum enumObject)
        {
            EnumValue enumValue = GetEnumValueObject(enumObject);
            if (enumValue == null)
                return null;
            else
                return enumValue.Value;
        }

        /// <summary>
        /// Get the Enum, per specified enum value and enum type
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="enumValueToMatch"></param>
        /// <returns></returns>
        public static object GetEnumValue(Type enumType, string enumValueToMatch)
        {
            string[] enumNames = Enum.GetNames(enumType);
            foreach (string enumName in enumNames)
            {
                string checkingEnumValue = GetEnumValueAsString((Enum)Enum.Parse(enumType, enumName));
                if (string.IsNullOrEmpty(checkingEnumValue))
                    continue;
                if (Regex.IsMatch(enumValueToMatch, checkingEnumValue, RegexOptions.IgnoreCase))
                    return Enum.Parse(enumType, enumName);
            }
            return null;
        }
    }
}
