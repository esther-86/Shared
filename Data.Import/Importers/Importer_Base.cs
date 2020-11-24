/*********************************************************************
 * Author: Benzi K. Ahamed
 *         www.benzi-ahamed.blogspot.com 
 * Date:   August 01 2007
 * 
 * Part of the Citrus framework
 * Copyright (C) 2007 - All rights reserved 
 * ********************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Data.Import
{
    public abstract class Importer_Base<T, EntityClass>
        where EntityClass : class, new()
    {
        public ImportFileAttribute ImportFileSettings { get; protected set; }   
        public string FileName { get; protected set; }
        public PropertyInfo CategoryProperty { get; protected set; }
        public Func<T, bool> FilterFunction { get; protected set; }

        public Importer_Base(string fileName, Func<T, bool> filterFunction)
        {
            this.FileName = fileName;

            // Instantiate an object to get the related attributes
            EntityClass fileRecord = new EntityClass();
            this.ImportFileSettings = ReflectionHelper.GetImportFileAttribute(fileRecord);
            
            // Will be used later to retrieve property's value, if property exists
            this.CategoryProperty = fileRecord.GetType().GetProperty("Category");

            // For data filtering at import
            this.FilterFunction = filterFunction;
        }

        private List<string> _errorRecords =
            new List<string>();
        /// <summary>
        /// The list of failed records
        /// </summary>
        public List<string> ErrorRecords
        {
            get { return _errorRecords; }
            set { _errorRecords = value; }
        }

        /// <summary>
        /// Determines if an import was successful
        /// </summary>
        public bool ImportSuccess
        {
            get
            {
                return (_errorRecords.Count == 0);
            }
        }

        public abstract Dictionary<string, List<EntityClass>> Import();
    }
}
