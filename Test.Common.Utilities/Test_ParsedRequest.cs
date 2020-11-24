using System.Collections.Generic;
using NUnit.Framework;
using Common.Utilities;
using System.Text.RegularExpressions;
using System.Linq;

// http://www.roelvanlisdonk.nl/?p=1734
// TODO: 10/17/2012 The library is not handling binary data in requests and will return null when trying to parse requests for such instances. Need to add test and support for it, if needed.
namespace Test.Common.Utilities
{
    [TestFixture]
    public class Test_ParsedRequest
    {
        [Test]
        [TestCaseSource(typeof(Test_ParsedRequest), "GetTestArguments")]
        public void Test_UpdateMatchingEntryId(string value, string regexKey, string expectedMatch, int expectedMatchCount)
        {
            if (expectedMatchCount != 1 || !regexKey.Equals(Constants.REGEX_WAS_ENTRY_IES))
                return;

            expectedMatch = "-88888";
            ParsedRequest requestObject = ParsedRequest.GetParsedRequest(value);
            List<KeyValuePair<string, object>> matches = requestObject.GetMatches(regexKey);
            foreach (KeyValuePair<string, object> match in matches)
                requestObject.Update(match, expectedMatch, requestObject.Dictionary);

            AssertMatches(requestObject, regexKey, expectedMatch, expectedMatchCount);
        }

        [Test]
        [TestCaseSource(typeof(Test_ParsedRequest), "GetTestCookieArguments")]
        public void Test_ParsedRequest_Parsing_Building_Match_Cookie(string value, string expectedMatch, int expectedMatchCount)
        {
            string regexKey = "ASP.NET_SessionId";

            ParsedRequest requestObject = new Cookie();
            requestObject.AssignDictionary(value);
            // Not using the following regex because for cookies, 
            // might have a case where the values is not key=value state, thus, assertion will fail
            // i.e: ASP.NET_SessionId=s5uelshalwzyxk1j55qkmeqd; path=/; HttpOnly
            // The parsed object will not include HttpOnly because it's not in format
            // key=value and so it will be skipped
            // the regex we want to test against should not have HttpOnly as a criteria
            //// Escaping value because the value might contain spaces, 
            //// and we want to use that as the regex pattern
            //string matchRegex = Utilities_String.EscapeSpecialRegexCharacters(value);
            // TODO: Make this more comprehensive, without the value 
            string matchRegex = expectedMatch;
            Assert.IsTrue(Regex.IsMatch(
                requestObject.ToString(), matchRegex, 
                RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase),
                Utilities_Assertion.ToString(
                "Test_ParsedRequest_Parsing_Building_Match_Cookie", value, requestObject));

            AssertMatches(requestObject, regexKey, expectedMatch, expectedMatchCount);
            AssertParsedRequest_AddOrUpdate(requestObject, regexKey, expectedMatch, expectedMatchCount);
        }

        [Test]
        [TestCaseSource(typeof(Test_ParsedRequest), "GetTestArguments")]
        public void Test_ParsedRequest_Parsing_Building_Match(string value, string regexKey, string expectedMatch, int expectedMatchCount)
        {
            ParsedRequest requestObject = ParsedRequest.GetParsedRequest(value);
            Assert.AreEqual(value.ToLower(), requestObject.ToString().ToLower());

            AssertMatches(requestObject, regexKey, expectedMatch, expectedMatchCount);
        }

        [Test]
        [TestCaseSource(typeof(Test_ParsedRequest), "GetTestArguments")]
        public void Test_ParsedRequest_Delete(string value, string regexKey, string expectedMatch, int expectedMatchCount)
        {
            ParsedRequest requestObject = ParsedRequest.GetParsedRequest(value);
            requestObject.Delete(regexKey);

            AssertMatches(requestObject, regexKey, expectedMatch, 0);
        }

        [Test]
        [TestCaseSource(typeof(Test_ParsedRequest), "GetTestArguments")]
        public void Test_ParsedRequest_AddOrUpdate(string value, string regexKey, string expectedMatch, int expectedMatchCount)
        {
            ParsedRequest requestObject = ParsedRequest.GetParsedRequest(value);
            AssertParsedRequest_AddOrUpdate(requestObject, regexKey, expectedMatch, expectedMatchCount);
        }

        public void AssertParsedRequest_AddOrUpdate(ParsedRequest requestObject, string regexKey, string expectedMatch, int expectedMatchCount)
        {
            // Add (the regexKey in the parameter might be an entry id regex key, we don't want to use that as a key)
            string newKey = "NEW_STRING_KEY";
            requestObject.AddOrUpdate(newKey, newKey, expectedMatch);
            AssertMatches(requestObject, newKey, expectedMatch, 1);

            // Because if the previous value was not there, AddOrUpdate will Add it.
            // Make sure that regexKey for this, if entry id, that it exists in the test
            if (expectedMatchCount == 0)
                expectedMatchCount = 1;
            // Update the newKey's value
            expectedMatch += "NEW";
            requestObject.AddOrUpdate(regexKey, regexKey, expectedMatch);
            AssertMatches(requestObject, regexKey, expectedMatch, expectedMatchCount);
        }

        public void AssertMatches(ParsedRequest requestObject, string regexKey, string expectedMatch, int expectedMatchCount)
        {
            string value = requestObject.ToString();
            expectedMatch = Utilities_String.EscapeSpecialRegexCharacters(expectedMatch);
            List<KeyValuePair<string, object>> matches = requestObject.GetMatches(regexKey);
            Assert.AreEqual(expectedMatchCount, matches.Count, string.Format("Count for value of {0} in {1}", regexKey, value));
            if (matches.Count <= 0)
                return;
            KeyValuePair<string, object> match = matches.First();

            bool isNull = (expectedMatch == null);
            if (isNull)
                Assert.IsNull(match.Value);
            else
            {
                Assert.IsTrue(Regex.IsMatch(match.Value.ToString(), expectedMatch, RegexOptions.IgnoreCase),
                    Utilities_Assertion.ToString(string.Format("Value of {0} in {1}", regexKey, value), expectedMatch, match.Value));
            }

            System.Console.WriteLine(string.Format("HTTP_Request:\n\tType: {0}\tToString: {1}",
                requestObject.GetType(), requestObject.ToString()));
        }

        public List<TestCaseData> GetTestCookieArguments()
        {
            return new List<TestCaseData>()
                {
                    new TestCaseData(
                        "ASP.NET_SessionId=s5uelshalwzyxk1j55qkmeqd; path=/; HttpOnly",
                        "s5uelshalwzyxk1j55qkmeqd", 1),
                    new TestCaseData(
                        "ASP.NET_SessionId=ojkmd42wcejg2kayi12sgi55; ClaimDexBanner=AAFES; AuthProfile=Public; MachineTag=ee50d397-bd10-425e-9713-d71c800e0f56; LastUsername-PRGXLOADTEST=Vendor_0001; AuthCode-PRGXLOADTEST=9H0d+zc3o0cHelsQWonwsuI3F1ncqj65vXYiIregWinhDXCXD3nySedm; UseWinsAuth-PRGXLOADTEST=False; LastRegionSelected=United States",
                        "ojkmd42wcejg2kayi12sgi55", 1)
                };
        }

        public List<TestCaseData> GetTestArguments()
        {
            // TODO: JSON status is in 1 nested arrays, on level 1, another nested array, on level 2
            return new List<TestCaseData>()
                {
                    // Empty
                    new TestCaseData(
                        "{\"parameters\":{\"Repository\":\"ciga-test83\",\"Entries\":[7471],\"Versions\":[],\"Flags\":3,\"CheckValue\":true,\"Cached\":0,\"RefreshEntryReference\":true}}",
                        Constants.REGEX_WAS_ENTRY_IES, "7471", 1),
                    new TestCaseData(
                        "/laserfiche8/Helper/TileData.aspx?repoName=ciga-test83&documentId=1471&versionNum=0&x=0&y=0&pageNum=1&scale=31&ro=0&time=1343943381980",
                        Constants.REGEX_WAS_ENTRY_IES, "1471", 1),
                    new TestCaseData(
                        "", "status", "", 0),
                    // Has LF search query
                    new TestCaseData(
                        "__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE=%2FwEPDwULLTE2MDc2NzYxNTIPFgIeDlJlcG9zaXRvcnlOYW1lBQxQUkdYTG9hZFRlc3QWAgIBD2QWEgICDw8WBh8ABQxQUkdYTG9hZFRlc3QeDkRlZmluaXRpb25GaWxlBU1DOlxQcm9ncmFtIEZpbGVzICh4ODYpXExhc2VyZmljaGVcV2ViIEFjY2VzcyA4XFdlYiBGaWxlc1xDb25maWdcUXVpY2tCYXJzLnhtbB4PRGVmaW5pdGlvblhQYXRoBSMvUXVpY2tCYXJEZWZpbml0aW9ucy9Ccm93c2VRdWlja0JhcmRkAgYPDxYEHhBTdGFydGluZ0ZvbGRlcklEBQExHwAFDFBSR1hMb2FkVGVzdGRkAgcPDxYEHwMFATAfAAUMUFJHWExvYWRUZXN0ZGQCCA8PFgQfAwUBMR8ABQxQUkdYTG9hZFRlc3RkZAIJDw8WAh8ABQxQUkdYTG9hZFRlc3RkZAIKDw8WAh4MUFJHWFVzZXJUeXBlCylqUFJHWENvbmZpZ3VyYXRpb24uUFJHWF9Vc2VyX1R5cGUsIFBSR1hDb25maWd1cmF0aW9uLCBWZXJzaW9uPTEuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49bnVsbBBkZAIPD2QWAmYPZBYCZg9kFgICAg8WAh4Dc3JjBRlBc3NldHMvaW1hZ2VzL2xvYWRpbmcuZ2lmZAIXD2QWAmYPZBYCZg9kFgICAg8WAh8FBRlBc3NldHMvaW1hZ2VzL2xvYWRpbmcuZ2lmZAIbDxAWAh4HVmlzaWJsZWgQFQEHRW5nbGlzaBUBBWVuLVVTFCsDAWcUKwEBZmRk12T5JgEmLUrxXZYv%2ByXbjsy9JGk%3D&=&__CALLBACKID=mySearchResultListingControl&__CALLBACKPARAM=%7B%22start%22%3A1%2C%22focusentryid%22%3A-1%2C%22sortcolumnname%22%3A%22Hit%20Count%22%2C%22sortisasc%22%3Afalse%2C%22dbname%22%3A%22PRGXLoadTest%22%2C%22search%22%3A%22%7BLF%3AName%3D%5C%22*%5C%22%2C%20Type%3D%5C%22F%5C%22%7D%20%26%20(%7B%5B%5D%3A%5BStatus%5D%3D%5C%22XPV%5C%22%7D%20%7C%20%7B%5B%5D%3A%5BStatus%5D%3D%5C%22XPA%5C%22%7D%20%7C%20%7B%5B%5D%3A%5BStatus%5D%3D%5C%22CDL%5C%22%7D%20%7C%20%7B%5B%5D%3A%5BStatus%5D%3D%5C%22VPR%5C%22%7D)%20%26%20%7B%5B%5D%3A%5BClient%20Name%5D%3D%5C%22AAFES%5C%22%7D%20%26%20%7B%5B%5D%3A%5BClient%20Category%5D%3D%5C%22Search%20Result%5C%22%7D%22%2C%22count%22%3A100%2C%22dateFormat%22%3A%22MDY%22%2C%22listingid%22%3A-1%7D",
                        "sortisasc", "false", 1),
                    new TestCaseData("/laserfiche8/js-src/dojo/resources/iframe_history.html?1336162440115", 
                        "", "", 0),
                    new TestCaseData("{\"utcOffset\":-480,\"dstOffset\":60,\"dbname\":null}", 
                        "dbname", null, 1),
                    // JSON
                    new TestCaseData(
                        "{\"repository\":\"PRGXLoadTest\",\"id\":1007,\"serializedDict\":\"{\\\"Status\\\":\\\"VPR\\\"}\",\"maxTimeout\":120000}",
                        "status", "VPR", 1),
                    new TestCaseData(
                        "http://v-dev-2k8r2-11/laserfiche?a=1&b=2#id%3D1%3Bview%3Dbrowse",
                        Constants.REGEX_WAS_ENTRY_IES, "1", 1),
                    // The right hand side is not a string, but is actually a JSON object
                    new TestCaseData(
                        "__CALLBACKPARAM=%7B%22id%22%3A144506%2C%22repository%22%3A%22PRGXLoadTest%22%7D",
                        Constants.REGEX_WAS_ENTRY_IES, "144506", 1),
                    new TestCaseData(
                        "__EVENTTARGET=LoginButton&__EVENTARGUMENT=&__VIEWSTATE=%2FwEPDwUKMTAwMzEyODQ3Nw8WBB4HRGVzdFVSTGQeDlNob3dXaW5BdXRoQm94BQR0cnVlFgICAQ9kFhBmDxYCHglpbm5lcmh0bWwFFkxvZyBJbiB0byBQUkdYTG9hZFRlc3RkAgEPFgIfAmRkAgIPZBYEZg8WAh4Fc3R5bGUFDGRpc3BsYXk6bm9uZRYCZg9kFgJmDxBkDxYBZhYBEAUMUFJHWExvYWRUZXN0BQ0xUFJHWExvYWRUZXN0Z2RkAgUPFgIfAwUOZGlzcGxheTppbmxpbmVkAgMPFgIeB1Zpc2libGVnFgRmDxAPFgIeBFRleHQFI1RoaXMgaXMgYSBwdWJsaWMgb3Igc2hhcmVkIGNvbXB1dGVyZGRkZAIBDxAPFgIfBQUaVGhpcyBpcyBhIHByaXZhdGUgY29tcHV0ZXJkZGRkAgQPFgIfAwUtbWFyZ2luLXRvcDoxNnB4O3BhZGRpbmc6MHB4IDNweDtkaXNwbGF5Om5vbmU7FgICAw8QFgIfBGgQFQEHRW5nbGlzaBUBBWVuLVVTFCsDAWcUKwEBZmQCBQ8WAh8EaGQCBw8WAh8EaGQCCA8WAh8EaGQYAQUeX19Db250cm9sc1JlcXVpcmVQb3N0QmFja0tleV9fFgYFD1VzZVdpbmRvd3NDaGVjawUMVXNlQXV0b0xvZ2luBRVVc2VDdXN0b21Qcm9maWxlTG9naW4FEFB1YmxpY01vZGVCdXR0b24FEVByaXZhdGVNb2RlQnV0dG9uBRFQcml2YXRlTW9kZUJ1dHRvbg%3D%3D&SelectedRepo=0prgxloadtest&UserNameBox=auditor_2433&PasswordBoxDisplay=&PasswordBox=&SelectedProfile=&SecurityMode=PublicModeButton&HashInput=",
                        "__VIEWSTATE", "EPDwUKMTAwMzEyOD", 1),
                    new TestCaseData(
                        "__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE=%2FwEPDwUKMTc1MzM2Mzg4Ng8WAh4OUmVwb3NpdG9yeU5hbWUFDFBSR1hMb2FkVGVzdBYEZg9kFgICBA8WAh4EaHJlZgUlLi9Bc3NldHMvcHJneC9iYW5uZXJzL0FBRkVTL3N0eWxlLmNzc2QCAQ9kFhQCAg8WAh4HVmlzaWJsZWhkAgMPZBYCAgEPFgQfAQUVUHJvamVjdFNlbGVjdGlvbi5hc3B4Hglpbm5lcmh0bWwFHCZsdDsgR28gQmFjayBUbyBBbGwgUHJvamVjdHNkAgQPDxYGHwAFDFBSR1hMb2FkVGVzdB4ORGVmaW5pdGlvbkZpbGUFTUM6XFByb2dyYW0gRmlsZXMgKHg4NilcTGFzZXJmaWNoZVxXZWIgQWNjZXNzIDhcV2ViIEZpbGVzXENvbmZpZ1xRdWlja0JhcnMueG1sHg9EZWZpbml0aW9uWFBhdGgFIy9RdWlja0JhckRlZmluaXRpb25zL0Jyb3dzZVF1aWNrQmFyZGQCCA8PFgQeEFN0YXJ0aW5nRm9sZGVySUQFATEfAAUMUFJHWExvYWRUZXN0ZGQCCQ8PFgQfBgUBMB8ABQxQUkdYTG9hZFRlc3RkZAIKDw8WBB8GBQExHwAFDFBSR1hMb2FkVGVzdGRkAgsPDxYCHwAFDFBSR1hMb2FkVGVzdGRkAgwPDxYCHgxQUkdYVXNlclR5cGULKXJMYXNlcmZpY2hlLldlYkFjY2Vzcy5Db21tb24uUFJHWF9Vc2VyX1R5cGUsIFdlYkFjY2Vzc0NvbW1vbiwgVmVyc2lvbj04LjEuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPW51bGwBZGQCEQ9kFgJmD2QWAmYPZBYCAgIPFgIeA3NyYwUZQXNzZXRzL2ltYWdlcy9sb2FkaW5nLmdpZmQCGA8QFgIfAmgQFQEHRW5nbGlzaBUBBWVuLVVTFCsDAWcUKwEBZmRk&=&__CALLBACKID=myThumbnailControl&__CALLBACKPARAM=%7B%22id%22%3A144506%2C%22repository%22%3A%22PRGXLoadTest%22%7D",
                        "repository", "PRGXLoadTest", 1),
                    new TestCaseData(
                        "http://v-dev-2k8r2-11/laserfiche",
                        "laserfiche", "", 0),
                    new TestCaseData(
                        "http://v-dev-2k8r2-11/laserfiche?a=1",
                        "b", "", 0),
                    new TestCaseData(
                        "http://v-dev-2k8r2-11/laserfiche?a=1&b=2",
                        "b", "2", 1),
                    new TestCaseData(
                        "http://v-dev-2k8r2-11/laserfiche?a=1&b=2#id%3D1",
                        "a", "1", 1),
                    new TestCaseData(
                        "http://v-dev-2k8r2-11/laserfiche#id%3D1%3Bview%3Dbrowse",
                        Constants.REGEX_WAS_ENTRY_IES, "1", 1),
                    new TestCaseData(
                        "http://v-dev-2k8r2-11/laserfiche?a=1#id%3D1%3Bview%3Dbrowse",
                        "view", "browse", 1),
                    new TestCaseData(
                        "http://v-dev-2k8r2-11/laserfiche?i=29&tid=40&id=30",
                        Constants.REGEX_WAS_ENTRY_IES, "29|30", 2),
                    // This one also replaced page id. It looks like some web method actually treats the page id as document id.
                    new TestCaseData(
                        "http://v-dev-2k8r2-11/laserfiche8/Helper/TileData.aspx?reposName=PRGXLoadTest&docID=144506&x=3&y=1&pageNum=1&scale=35&ro=0&time=1305269978248&pageID=2480",
                        Constants.REGEX_WAS_ENTRY_IES, "144506|2480", 2),
                    new TestCaseData(
                        "{\"repository\":\"PRGXLoadTest\",\"id\":29,\"message\":\"Topic: Please review the attached claim support before approval: Auditor\",\"linkedIds\":[],\"tokens\":[],\"zipToOneFile\":false,\"sendAttachmentsToRecipents\":false,\"visibility\":0,\"recipents\":[]}",
                        "recipents", "", 2),
                    new TestCaseData(
                        "{\"repoName\":\"PRGXLoadTest\",\"entryIds\":[119307],\"metadataFlags\":15,\"ticks\":1305269541107,\"checkValue\":true}",
                        Constants.REGEX_WAS_ENTRY_IES, "119307", 1),
                    new TestCaseData(
                        "{\"repoName\":\"PRGXLoadTest\",\"entryIds\":[2747],\"changes\":{\"__type\":\"Laserfiche.WebAccess.Common.MetadataChanges\",\"newTemplateId\":3,\"removeTemplate\":false,\"fieldChanges\":[{\"__type\":\"Laserfiche.WebAccess.Common.FieldChangeInfo\",\"fieldId\":44,\"value\":\"AAFES Buyer 634659397298133853\",\"remove\":false,\"fieldIndex\":0},{\"__type\":\"Laserfiche.WebAccess.Common.FieldChangeInfo\",\"fieldId\":22,\"value\":\"Claim metadata edited by Auditor_0001 634659397298133853\",\"remove\":false,\"fieldIndex\":0},{\"__type\":\"Laserfiche.WebAccess.Common.FieldChangeInfo\",\"fieldId\":86,\"value\":\"Editted 634659397298133853\",\"remove\":false,\"fieldIndex\":0},{\"__type\":\"Laserfiche.WebAccess.Common.FieldChangeInfo\",\"fieldId\":47,\"value\":\"2012/02/27 00:00:0\",\"remove\":false,\"fieldIndex\":0},{\"__type\":\"Laserfiche.WebAccess.Common.FieldChangeInfo\",\"fieldId\":126,\"value\":53,\"remove\":false,\"fieldIndex\":0}],\"tagChanges\":[],\"linkChanges\":[]}}",
                        Constants.REGEX_WAS_ENTRY_IES, "2747", 1)
                };
        }
    }
}
