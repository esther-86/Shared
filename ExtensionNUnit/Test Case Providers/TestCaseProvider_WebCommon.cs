using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Reflection;
using NUnit.Core;

namespace ExtensionNUnit
{
    public class TestCaseProvider_WebCommon : TestCaseProvider_Test
    {
        public override System.Collections.IEnumerable GetTestCasesFor(MethodInfo method)
        {
            Configuration_WebCommonAttribute assignedAttributeForMethod =
                Attribute.GetCustomAttribute(method, typeof(Configuration_WebCommonAttribute), false)
                as Configuration_WebCommonAttribute;

            List<TestCaseData> returnTestCases = new List<TestCaseData>();
            List<TestCaseData> generatedTestCases = null;
            if (assignedAttributeForMethod.DataGeneratorClass != null && !string.IsNullOrEmpty(assignedAttributeForMethod.DataGeneratorMethod))
            {
                ConstructorInfo methodsClass = assignedAttributeForMethod.DataGeneratorClass.GetConstructor(Type.EmptyTypes);
                MethodInfo methodInfo = assignedAttributeForMethod.DataGeneratorClass.GetMethod(assignedAttributeForMethod.DataGeneratorMethod);

                object methodsClassObject = methodsClass.Invoke(new object[] { });
                object[] methodParameter = new object[] { };
                if (assignedAttributeForMethod.PassDataGeneratorConfigurationObject)
                    methodParameter = new object[] { assignedAttributeForMethod };
                generatedTestCases = (List<TestCaseData>)methodInfo.Invoke(methodsClassObject, methodParameter);
            }

            // If user did not specify a method to generate test data from
            // We should just go on with the test and set up configuration information
            if (generatedTestCases == null)
                generatedTestCases = new List<TestCaseData>() { new TestCaseData() };

            // Browser will always add the method to the changed list
            foreach (Browser browser in Enum.GetValues(typeof(Browser)))
            {
                // Try to split up Browser.ALL to separate test cases
                if (!assignedAttributeForMethod.Cfg_Browser.HasFlag(browser) || browser == Browser.ALL)
                { continue; }

                foreach (TestCaseData arguments in generatedTestCases)
                {
                    NUnitTestMethod test = new NUnitTestMethod(method);

                    Configuration_WebCommonAttribute configurationForTestCase = Attribute.GetCustomAttribute(method,
                        typeof(Configuration_WebCommonAttribute), false) as Configuration_WebCommonAttribute;
                    configurationForTestCase.Cfg_Browser = browser;

                    string newTestName = string.Format("{0}({1})_{2}",
                            test.TestName.Name, GetArgumentsAsString(arguments.Arguments), browser.ToString());
                    SetTestName(test, newTestName);

                    TestCaseData browserTestData = new TestCaseData(arguments.Arguments);
                    browserTestData.SetName(test.TestName.Name);

                    Configuration.Changes.Add(test.TestName.FullName, configurationForTestCase);
                    returnTestCases.Add(browserTestData);
                }
            }

            return returnTestCases;
        }
    }
}
