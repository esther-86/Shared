using System.Reflection;
using NUnit.Core.Extensibility;
using NUnit.Core;

namespace ExtensionNUnit
{
    public abstract class TestCaseProvider_Test : ITestCaseProvider
    {
        public abstract System.Collections.IEnumerable GetTestCasesFor(MethodInfo method);

        public bool HasTestCasesFor(MethodInfo method)
        {
            return (method.GetCustomAttributes(typeof(Configuration_WebCommonAttribute), false).Length > 0);
        }

        public string SetTestName(NUnitTestMethod test, string newName)
        {
            string previousName = test.TestName.Name;
            test.TestName.Name = newName;
            // Just in case the class name is the same as the test name, we only want to replace the test name portion
            // test.TestName.FullName = test.TestName.FullName.Replace(previousName, newName);
            int indexOfTestName = test.TestName.FullName.LastIndexOf(previousName);
            test.TestName.FullName = test.TestName.FullName.Remove(indexOfTestName);
            test.TestName.FullName += newName;
            return test.TestName.FullName;
        }

        public string GetArgumentsAsString(System.Array arguments)
        {
            string retArgumentAsString = "";
            if (arguments == null)
                return retArgumentAsString;

            foreach (object arg in arguments)
            {
                if (arg == null)
                    continue;
                
                if (!string.IsNullOrEmpty(retArgumentAsString))
                    retArgumentAsString += ", ";

                // If array, get the parameters as values and append it to 
                // this string to return
                if (arg is System.Array)
                {
                    retArgumentAsString += GetArgumentsAsString((System.Array)arg);
                    continue;
                }

                string format = "{0}";
                if (arg.GetType() == typeof(string))
                    format = "\"{0}\"";
                
                retArgumentAsString += string.Format(format, arg.ToString());
            }

            return retArgumentAsString;
        }
    }
}
