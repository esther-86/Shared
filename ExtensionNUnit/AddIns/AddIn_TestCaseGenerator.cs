using NUnit.Core.Extensibility;

namespace ExtensionNUnit
{
    #region "Useful Resources"
    // http://fzzd.blogspot.com/2011/12/blackbox-testing-with-nunit-using.html
    // Getting a method's attributes: http://social.msdn.microsoft.com/Forums/en-US/vsautotest/thread/95f9025e-f3e4-4dac-909d-f3d47836e43c
    // Attribute parameters: http://msdn.microsoft.com/en-us/library/aa664614%28v=VS.71%29.aspx
    // Core SampleFixture code with this change:
        //public Test BuildFrom(Type type)
        //{
        //    if (CanBuildFrom(type))
        //    { return new SampleFixtureExtension(type); }
        //    return null;
        //}
    // From most recent work: If extend SuiteBuilder: Test attribute and any other attributes related to it will no longer work
    #endregion

    [NUnitAddin(Description = "Custom test case provider addin", Name = "Custom test case provider addin", Type = ExtensionType.Core)]
    public class TestCaseProviderAddin : IAddin
    {
        public bool Install(IExtensionHost host)
        {
            host.GetExtensionPoint("TestCaseProviders").Install(new TestCaseProvider_WebCommon());
            return true;
        }
    }
}
