using System;
using System.Diagnostics;

namespace Common.Utilities
{
    public static class ExceptionExtension
    {
        // http://stackoverflow.com/questions/8338495/how-to-get-error-line-number-of-code-using-try-catch
        public static StackFrame GetStackFrame(this Exception e)
        {
            // Get stack trace for the exception with source file information
            var trace = new StackTrace(e, true);
            // Get the top stack frame
            return trace.GetFrame(0);
        }
    }
}
