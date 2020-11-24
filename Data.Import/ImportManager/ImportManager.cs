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

namespace Data.Import
{
    public class ImportManager<T, EntityClass>
            where EntityClass : class, new()
    {
        public string FileName { get; protected set; }
        protected Importer_Base<T, EntityClass> Importer { get; set; }
        protected Dictionary<string, List<EntityClass>> Configuration { get; set; }

        protected EntityClass entity = null;
        protected ImportFileAttribute importFileSettings;

        public ImportManager(string fileName, Func<string, bool> filterFunction)
        {
            FileName = fileName;
            entity = new EntityClass();

            // Get the import file attribute from the entity class
            importFileSettings = ReflectionHelper.GetImportFileAttribute(entity);
            Importer = (Importer_Base<T, EntityClass>) Activator.CreateInstance(importFileSettings.FileType, fileName, filterFunction);
        }

        public void Import()
        {
            this.Configuration = Importer.Import();            
        }
    }
}






