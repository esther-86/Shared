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
    
    [global::System.Serializable]
    public class DataImportException : Exception
    {
        public DataImportException() { }
        public DataImportException(string message) : base(message) { }
        public DataImportException(string message, Exception inner) : base(message, inner) { }
        protected DataImportException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [global::System.Serializable]
    public class FieldValidationException : DataImportException
    {
        public FieldValidationException() { }
        public FieldValidationException(string message) : base(message) { }
        public FieldValidationException(string message, Exception inner) : base(message, inner) { }
        protected FieldValidationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
