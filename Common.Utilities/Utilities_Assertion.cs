using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Common.Utilities
{
    public class Utilities_Assertion
    {
        public static string ToString(string comment, object expected, object actual)
        {
            string expectedStr = "";
            string actualStr = "";
            if (expected != null)
                expectedStr = expected.ToString();
            if (actualStr != null)
                actualStr = actual.ToString();
            return string.Format("\tComment:{0}\n\tExpected: {1}\n\tActual: {2}", comment, expectedStr, actualStr);
        }

        public static bool AssertAndInform(List<Exception> exceptions, string assertionInformationPrefix, Action assertAction, params Func<bool>[] backupAssertions)
        {
            try
            {
                assertAction.Invoke();
                Console.WriteLine(assertionInformationPrefix + ": {0}", true);
                return true;
            }
            catch (Exception ex)
            {
                // If the assertion throws an error, we will need to add the exception and run the backup assertions
                exceptions.Add(ex);
                Console.WriteLine(assertionInformationPrefix + ": {0}", false);

                // Once a backup assertion returns success, don't need to run the others anymore
                foreach (Func<bool> backupAssertion in backupAssertions)
                {
                    bool isSuccess = backupAssertion.Invoke();
                    if (isSuccess)
                        return true;
                }
            }

            return false;
        }

        // http://stackoverflow.com/questions/2834717/nunit-is-it-possible-to-continue-executing-test-after-assert-fails
        public static void AssertAll(params Action[] assertions)
        {
            List<Exception> exceptions = new List<Exception>();

            foreach (Action assertion in assertions)
            {
                try { assertion(); }
                catch (Exception ex) { exceptions.Add(ex); }
            }

            if (exceptions.Any())
            {
                var ex = new Exception(
                    string.Join(Environment.NewLine, exceptions.Select(e => e.Message)),
                    exceptions.First());

                // Use stack trace from the first exception to ensure first
                // failed Assert is one click away
                ReplaceStackTrace(ex, exceptions.First().StackTrace);

                throw ex;
            }
        }

        static void ReplaceStackTrace(Exception exception, string stackTrace)
        {
            var remoteStackTraceString = typeof(Exception)
                .GetField("_remoteStackTraceString",
                    BindingFlags.Instance | BindingFlags.NonPublic);

            remoteStackTraceString.SetValue(exception, stackTrace);
        }
    }
}
