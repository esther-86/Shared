using System;
using System.Collections.Generic;

namespace Common.Utilities
{
    public class Utilities_Arguments
    {
        /// <summary>
        /// Takes in an enum type that contains all of the available arguments for this program
        /// defaults for the values are stored in the EnumValue attribute
        /// Always need EnumValue so that a sample usage string can be created from this
        /// for the user to see how they should write the arguments
        /// Will return a dictionary of the enum name and the associated value for it
        /// Currently, supported value types are int[], string, and other convertable types
        /// Enum names starting with Range needs to have value in format [from]-[to]
        /// </summary>
        public static Dictionary<Enum, object> Parse(Type enumType, string[] args)
        {
            Dictionary<Enum, object> argumentDictionary = new Dictionary<Enum, object>();
            List<Enum> requiredArguments = new List<Enum>();

            const char KEYVALUE_SPLITTER = '=';

            const string RANGE_PREFIX_IDENTIFIER = "Range";
            const char RANGE_SPLITTER = '-';

            string availableParameterMessage = "AVAILABLE PARAMETERS:\n";
            string requiredParameterMessage = "REQUIRED PARAMETERS:\n";
            string userSpecifiedParameterMessage = "USER-SPECIFIED PARAMETER:\n";
            string userSpecifiedParameterMessage_formatted = "USER-SPECIFIED PARAMETER (formatted):\n";

            try
            {
                foreach (Enum enumObject in Enum.GetValues(enumType))
                {
                    // Always will need enum value, for demonstration purpose
                    EnumValue enumvalue = EnumValue.GetEnumValueObject(enumObject);

                    object value = enumvalue.Value;
                    argumentDictionary.Add(enumObject, value);

                    string enumObjectString = enumObject.ToString();
                    string valueStr = value.ToString();
                    if (enumObjectString.StartsWith(RANGE_PREFIX_IDENTIFIER))
                    {
                        int[] range = (int[])value;
                        valueStr = range[0] + RANGE_SPLITTER.ToString() + range[1];
                    }

                    if (enumvalue.IsRequired)
                    {
                        requiredParameterMessage += string.Format("\t{0}\n",
                            enumObjectString);
                        requiredArguments.Add(enumObject);
                    }

                    availableParameterMessage += string.Format("\t{0}:{1}\n",
                        enumObjectString, valueStr);
                }

                // Parse the argument into the desired object type
                foreach (string arg in args)
                {
                    userSpecifiedParameterMessage += arg + " ";
                    userSpecifiedParameterMessage_formatted += string.Format("\t{0}\n", arg);

                    string[] split = arg.Split(KEYVALUE_SPLITTER);
                    string key = split[0];
                    Enum enumObject = (Enum)Enum.Parse(enumType, key.Trim(), true);

                    // Replace the key portion because if user specified datasource connection string
                    // the connection string will contain equal sign in the value
                    // and this part should not be split
                    string valueStr = arg.Replace(key + KEYVALUE_SPLITTER.ToString(), "").Trim();
                    object value = valueStr;
                    if (enumObject.ToString().StartsWith(RANGE_PREFIX_IDENTIFIER))
                    {
                        string[] range = valueStr.Split(RANGE_SPLITTER);
                        int min = int.Parse(range[0]);
                        if (range.Length == 1)
                        {
                            value = new int[] { min, min };
                        }
                        else
                        {
                            int max = int.Parse(range[1]);
                            if (max < min)
                                value = new int[] { max, min };
                            else
                                value = new int[] { min, max };
                        }
                    }
                    else
                    {
                        try
                        {
                            // Try to cast it to the type specified in the default value
                            // if can't the value will be the valueStr 
                            Type defaultValueType = argumentDictionary[enumObject].GetType();
                            value = Convert.ChangeType(valueStr, defaultValueType);
                        }
                        catch { }
                    }

                    argumentDictionary[enumObject] = value;

                    if (requiredArguments.Contains(enumObject))
                        requiredArguments.Remove(enumObject);
                }

                if (requiredArguments.Count > 0)
                    throw new Exception("Please specify all required parameters");

            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(
                    "{0}\n{1}\n{2}\n{3}\n\n\t{4}",
                    availableParameterMessage, 
                    requiredParameterMessage,
                    userSpecifiedParameterMessage_formatted, userSpecifiedParameterMessage,
                    ex.Message));
            }

            return argumentDictionary;
        }
    }
}
