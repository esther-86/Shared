/*********************************************************************
 * Author: Benzi K. Ahamed
 *         www.benzi-ahamed.blogspot.com 
 * Date:   August 01 2007
 * 
 * Part of the Citrus framework
 * Copyright (C) 2007 - All rights reserved 
 * ********************************************************************/

using System;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Data.Import
{
    public class ReflectionHelper
    {
        /// <summary>
        /// Gets the import file attribute settings 
        /// that have been marked for a class
        /// </summary>
        /// <param name="entity">The object
        /// whose attribute will be returned</param>
        /// <returns></returns>
        public static ImportFileAttribute GetImportFileAttribute(
            object entity)
        {
            object[] attributes = 
                entity.GetType().GetCustomAttributes(false);
            foreach (object attribute in attributes)
            {
                if (attribute is ImportFileAttribute)
                {
                    return (ImportFileAttribute)attribute;
                }
            }
            return null;
        }

        public static void SetPropertyValue(
            object entity, 
            object value, 
            int fieldIndex)
        {
            object[] attributes;

            // Search the properties for the correct position and fill the appropriate value
            foreach (PropertyInfo property in entity.GetType().GetProperties())
            {
                attributes = property.GetCustomAttributes(
                    typeof(ImportFieldAttribute), false);
                foreach (object attribute in attributes)
                {
                    ImportFieldAttribute field = (ImportFieldAttribute)attribute;
                    if (field.Position == fieldIndex)
                    {                        
                        if (IsFieldValueValid(field, value))
                        {
                            value = PrepareFieldValue(field, property, value);
                            property.SetValue(entity, value, null);
                        }
                        else
                        {
                            throw new FieldValidationException(
                                string.Format(
                                "Validation of field '{0}' failed with value " +
                                "'{1}'\nShould match pattern '{2}'", 
                                property.Name, 
                                value, 
                                field.ValidationPattern)
                                );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines if a field value to be populated to a field is valid or not
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsFieldValueValid(
            ImportFieldAttribute field, 
            object value)
        {
            if (field.EnableValidation && 
                field.ValidationPattern != null && 
                field.ValidationPattern.Length > 0)
            {
                if (Regex.IsMatch((string)value, field.ValidationPattern))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        
        /// <summary> 
        /// Sets up the value object for setting to the property
        /// </summary> 
        /// <param name="field"></param> 
        /// <param name="value"></param> 
        /// <returns></returns> 
        public static object PrepareFieldValue(
            ImportFieldAttribute field,
            PropertyInfo property,
            object value)
        {
            if (property.PropertyType == typeof(string))
            {
                if (field.EnableTrimming)
                    value = ((string)value).Trim();
            }
            else if (value is IConvertible) // Convert to the type specified by the property
            {
                // Try to convert the input string value to the proper type 
                // of the data, only if data type is not string (save extra processing power)
                if (property.PropertyType != typeof(string))
                { value = Convert.ChangeType(value, property.PropertyType); }
            }
            else { } // Custom conversion types

            return value;
        }
    }
}
