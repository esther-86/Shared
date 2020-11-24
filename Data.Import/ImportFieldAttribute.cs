/*********************************************************************
 * Author: Benzi K. Ahamed
 *         www.benzi-ahamed.blogspot.com 
 * Date:   August 01 2007
 * 
 * Part of the Citrus framework
 * Copyright (C) 2007 - All rights reserved 
 * ********************************************************************/

using System;

namespace Data.Import
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ImportFieldAttribute : Attribute
    {
        /// <summary>
        /// The position of the field
        /// </summary>
        public int Position { get; protected set; }

        /// <summary>
        /// The regexp validation pattern for the field
        /// Validation happens only if EnableValidation is set to true
        /// </summary>
        public string ValidationPattern { get; set; }

        /// <summary>
        /// Set to true if validation is required
        /// </summary>
        public bool EnableValidation { get; set; }

        /// <summary>
        /// Determines whether input should be trimmed
        /// </summary>
        public bool EnableTrimming { get; set; }

        public ImportFieldAttribute(int position)
        {
            this.Position = position;
            this.EnableTrimming = true;
        }
    }
}
