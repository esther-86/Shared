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
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ImportFileAttribute : Attribute
    {
        /// <summary>
        /// The row delimiter
        /// </summary>
        public string RowDelimiter { get; protected set; }

        /// <summary>
        /// The type of file we are importing
        /// </summary>
        public Type FileType { get; set; }

        /// <summary>
        /// The field delimiter
        /// </summary>
        public string FieldDelimiter { get; protected set; }

        /// <summary>
        /// Indicates whether the first row is a header row
        /// </summary>
        public bool HasFirstRowAsHeader { get; protected set; }

        public ImportFileAttribute()
        {
            this.FieldDelimiter = ",";
            this.HasFirstRowAsHeader = false;
            this.RowDelimiter = "\n";
        }

        public ImportFileAttribute(Type fileType)
        {
            this.FileType = fileType;
        }
    }
}
