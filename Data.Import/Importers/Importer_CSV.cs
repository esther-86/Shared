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
using System.IO;

namespace Data.Import
{
    /// <summary>
    /// CSV file importer class
    /// Imports CSV files and returns an object model of records in the file
    /// </summary>
    /// <typeparam name="FileRecordClass"></typeparam>
    public class Importer_CSV<T, EntityClass> : Importer_Base<T, EntityClass>
        where EntityClass : class, new()
    {
        public Importer_CSV(string fileName, Func<T, bool> filterFunction)
            : base(fileName, filterFunction) { }

        /// <summary>
        /// Imports the CSV record and returns a list of objects
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, List<EntityClass>> Import()
        {
            string recordData;
            string[] dataElements;
            EntityClass fileRecord;

            // Category, Data
            Dictionary<string, List<EntityClass>> theDictionary = new Dictionary<string, List<EntityClass>>();

            StreamReader streamReader = File.OpenText(base.FileName);

            while (!streamReader.EndOfStream)
            {
                // Read in a line of the record data
                recordData = streamReader.ReadLine();
                
                // Filtering is enabled
                if (this.FilterFunction != null)
                {
                    bool matchesFilteringCriteria = this.FilterFunction.Invoke((T)(object)recordData);
                    if (!matchesFilteringCriteria)
                    { continue; }
                }

                try
                {
                    // Split the record data based on the column delimiter
                    dataElements = 
                        recordData.Split(
                        ImportFileSettings.FieldDelimiter.ToCharArray());

                    // Populate the record data elements into the object
                    // and add it to the list
                    fileRecord = new EntityClass();

                    // For every data elements we find
                    for (int i = 0; i < dataElements.Length; i++)
                    {
                        ReflectionHelper.SetPropertyValue(
                            fileRecord, 
                            dataElements[i], 
                            i);
                    }

                    string category = "NONE";
                    if (this.CategoryProperty != null)
                    { category = this.CategoryProperty.GetValue(fileRecord, null).ToString(); }

                    // If this category have not existed, create it
                    // Otherwise, add this item to the list
                    List<EntityClass> theList = null;
                    if (!theDictionary.TryGetValue(category, out theList))
                    {
                        theList = new List<EntityClass>();
                        theDictionary.Add(category, theList);
                    }

                    theList.Add(fileRecord);
                }
                catch (FieldValidationException)
                {
                    ErrorRecords.Add(recordData);
                }
            }

            streamReader.Close();
            return theDictionary;
        }
    }
}
