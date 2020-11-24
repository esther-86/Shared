using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Web;
using System.Collections.Specialized;
using System.Linq;

namespace Common.Utilities
{
    public enum Format
    {
        JSON, URL, HEADER
    }

    public class RequestManipulator
    {
        protected static JavaScriptSerializer serializer = new JavaScriptSerializer();

        // TODO: Set when to extract and replace whole array, and when not to
        protected static bool extractReplaceBool_ReplaceWholeArray = false;
        protected static bool extractReplaceBool_ReplaceValueIsSerialized = false;

        public const string REGEXPATTERN_PREFIX_SUFFIX = "(\"|\\s|%22)*";
        public const string JSON_SERIALIZEDDICT = "serializedDict";
        public const string SPLITVALUE_JSON = ":";
        public const string SPLITVALUE_URL = "=";

        public static NameValueCollection nvc_InvalidJSON = HttpUtility.ParseQueryString("__type=DOUBLEUNDERSCOREtype&<=OPENBRACKET&>=CLOSEBRACKET");

        public static string AddToQueryString(string originalURL, string id, string toAdd)
        {
            string qsid = "ss" + id;
            string url = DeleteKeyValue(Format.URL, originalURL, qsid);
            if (url.Contains("?"))
            { url += "&" + qsid + "=" + toAdd; }
            else { url += "?" + qsid + "=" + toAdd; }
            return url;
        }

        public static string ExtractReplaceEntryId(Format format, string originalString, string entry_ies, Regex regexIsEntryId, string stringListRootId)
        {
            List<string> ret = RequestManipulator.ExtractReplaceKeyValue(format, originalString, regexIsEntryId, entry_ies, true, stringListRootId);
            // If nothing was replaced/returned, return the unmodified string
            if (ret.Count <= 0)
            { return originalString; }
            return ret[0].Trim();
        }
        
        public static string DeleteKeyValue(Format format, string originalString, string fullKeyName)
        {
            List<string> matches = ExtractReplaceKeyValue(format, originalString, fullKeyName, null);
            foreach (string match in matches)
            {
                originalString = Regex.Replace(originalString, match, "", RegexOptions.IgnoreCase);
            }
            originalString = originalString.Replace("&&", "&");
            originalString = originalString.Replace(",,", ",");
            if (originalString.EndsWith("&") || originalString.EndsWith(","))
            { originalString = originalString.Remove(originalString.Length - 1); }
            return originalString.Trim();
        }

        /// Out of the list of results, only return the first result
        public static string ExtractReplaceKeyValueEx(Format format, string originalString, string fullKeyName, string value)
        {
            if (string.IsNullOrEmpty(originalString))
            { return originalString; }

            List<string> ret = ExtractReplaceKeyValue(format, originalString, fullKeyName, value);
            // If nothing was replaced/returned, return the unmodified string
            if (ret.Count <= 0)
            { return originalString; }
            return ret[0].Trim();
        }

        /// Returns the value, not the key and value (from original string)
        public static string ExtractValue(Format format, string originalString, string fullKeyName)
        {
            List<string> ret = ExtractReplaceKeyValue(format, originalString, fullKeyName, null);
            // If nothing was replaced/returned, return the unmodified string
            if (ret.Count <= 0)
            { return originalString; }
            string retValue = ret[0].Trim();

            // Assume this is a key, value of each type string. Will not work if value is an array/object
            if (format == Format.JSON)
            {
                Dictionary<string, object> kv = serializer.Deserialize<Dictionary<string, object>>("{" + retValue + "}");
                return kv.Values.ElementAt(0).ToString();
            }
            if (format == Format.URL)
            {
                NameValueCollection kv = HttpUtility.ParseQueryString(retValue);
                return kv.GetValues(0)[0];
            }

            return retValue;
        }

        /// When added to the list, key and values are URLEncoded
        public static void URLDeserialize(ref Dictionary<string, string> keyValueList, string qs)
        {
            NameValueCollection nvc = HttpUtility.ParseQueryString(qs);
            foreach (string key in nvc.Keys)
            {
                // Because the code sometimes passes in request body, which might have URL or JSON format
                if (string.IsNullOrEmpty(key))
                { continue; }

                string decodedValue = nvc[key];
                string encodedValue = HttpUtility.UrlEncode(decodedValue);
                if (decodedValue.Contains("{"))
                { JSONDeserialize(Format.URL, ref keyValueList, decodedValue, false); }
                keyValueList.Add(HttpUtility.UrlEncode(key), SPLITVALUE_URL + encodedValue);
            }
        }

        // Extract replace key value.
        // One caviat: This should not be entry id string value because entry id regex is in a different format
        // Should call the entry id - specific functions if you want that done
        protected static List<string> ExtractReplaceKeyValue(Format format, string originalString, string matchKey, string toReplaceValue)
        {
            Regex matchRegex = new Regex(string.Format("^{1}{0}.*{1}$", matchKey, REGEXPATTERN_PREFIX_SUFFIX), RegexOptions.IgnoreCase);
            return ExtractReplaceKeyValue(format, originalString, matchRegex, toReplaceValue, false, "");
        }

        public static List<string> ExtractReplaceKeyValue(Format format, string originalString, Regex matchRegex, string toReplaceValue, bool isReplacingEntryIds, string stringListRootId)
        {
            string ret_str = originalString;
            foreach (string s in nvc_InvalidJSON)
            { ret_str = ret_str.Replace(s, nvc_InvalidJSON[s]); }

            Dictionary<string, string> keyValueList = new Dictionary<string, string>();
            List<string> matches = new List<string>();

            bool returnExtractedValue = (toReplaceValue == null);

            // key, value are JSON serialized/URL encoded
            // First index for the value contains unserialized/unencoded splitValue character
            switch (format)
            {
                case Format.HEADER:
                    string[] splitted = Regex.Split(ret_str, ";");
                    foreach (string str in splitted)
                    {
                        string[] keyValue = Regex.Split(str, "=");
                        string key = keyValue[0];
                        string value = "";
                        try { value = keyValue[1]; }
                        catch (Exception) { } // Content-Type: multipart/form-data; // Does not have value part
                        keyValueList.Add(key, SPLITVALUE_URL + value);
                    }
                    break;
                case Format.URL:
                    string qs = ret_str;
                    // Query string, or request body will most likely use '?' as a query string separator
                    // If it exists, look only at the query portion
                    int indexOfQueryStrSeparator = qs.IndexOf('?');
                    if (indexOfQueryStrSeparator > 0)
                        qs = qs.Substring(indexOfQueryStrSeparator);
                    URLDeserialize(ref keyValueList, qs);
                    break;
                case Format.JSON:
                    JSONDeserialize(format, ref keyValueList, ret_str, false);
                    break;
            }

            for (int i = 0; i < keyValueList.Keys.Count; i++)
            {
                string key = keyValueList.Keys.ElementAt(i);
                if (!matchRegex.IsMatch(key))
                { continue; }

                // keyValueList[key] should already have the split symbol (= or :, etc.)
                string value = keyValueList[key];
                string splitValue = value.Substring(0, 1);
                // Don't do this because if the split value is :, and the string has :, 
                // it will replace the colon in the string, causing search/replace to fail
                // Need to do a substring instead
                // value = value.Replace(splitValue, ""); 
                value = value.Substring(1);

                string prefix_suffix = "";
                if (key.Contains(JSON_SERIALIZEDDICT))
                { prefix_suffix = "\\\\"; }

                string extracted_value = value;
                // Sometimes, values is null. In this case, should not replace anything
                if (Regex.IsMatch(extracted_value, "null", RegexOptions.IgnoreCase))
                { continue; }

                string replaced_value = toReplaceValue;
                switch (format)
                {
                    case Format.HEADER:
                        break;
                    case Format.URL:
                        // HttpUtility.ParseQueryString decoded the query string. 
                        // Need to re-encode it for search and replace to be correct
                        if (splitValue.Equals(SPLITVALUE_JSON))
                        { splitValue = HttpUtility.UrlEncode(splitValue); }
                        break;
                    case Format.JSON:
                        if (replaced_value == null)
                        { break; }
                        // If the extracted_value is not empty, or is not an empty array
                        // The content might contain int, or string, so serialize the replaced_value
                        // OR if the extracted_value contains quotes, then we know to serialize it
                        if (!extractReplaceBool_ReplaceValueIsSerialized &&
                            (string.IsNullOrEmpty(extracted_value) || extracted_value.Contains("[]")
                            || extracted_value.Contains("\"")))
                        {
                            replaced_value = serializer.Serialize(replaced_value);
                            replaced_value = replaced_value.Replace("\\u003c", "<");
                            replaced_value = replaced_value.Replace("\\u003e", ">");
                        }
                        // If empty bracket, and there are more than one occurrence of these empty brackets
                        // We need to be able to replace it correctly only for the key that we are working with
                        if (extracted_value.Contains("[") && !replaced_value.Contains("["))
                        {
                            if (extractReplaceBool_ReplaceWholeArray) { replaced_value = string.Format("[{0}]", replaced_value); }
                            else { replaced_value = string.Format("[{0}", replaced_value); }
                        }
                        break;
                }

                extracted_value = string.Format("{3}{0}{3}{1}{3}{2}{3}", key, splitValue, extracted_value, prefix_suffix);
                // Will determine if the whole JSON list [1,2,3] should be replaced with toReplaceValue
                // Or only the first item in the list
                if (!extractReplaceBool_ReplaceWholeArray && extracted_value.Contains("["))
                {
                    int endOfFirstItemIndex = extracted_value.IndexOf(',');
                    if (endOfFirstItemIndex < 0)
                    { endOfFirstItemIndex = extracted_value.IndexOf(']'); }
                    extracted_value = extracted_value.Substring(0, endOfFirstItemIndex);
                }

                if (returnExtractedValue) { ret_str = extracted_value; }
                else
                {
                    // If we are trying to make entryId replacement, do not replace
                    // if the extracted id is not a numeric value, or if the id is in a list of root ids, 
                    if (isReplacingEntryIds)
                    {
                        // If format is JSON, value is deserialized. 
                        // We care about this case because the id can be captured in quotes (i.e.: "247")
                        // Or it can be a list of ids. We need to deserialize it to check (i.e: [1, 2])
                        string maybeEntryId = value;
                        if (format == Format.JSON)
                        {
                            // If id in quotes, deserialize will make it correct without quotes,
                            // Then, parsing the id will be good
                            object deserializedValue = serializer.Deserialize<object>(maybeEntryId);
                            if (maybeEntryId.Contains("[")) 
                            {
                                object[] entriesInRequest = (object[])deserializedValue;
                                // There's no matching entries to replace
                                if (entriesInRequest.Length <= 0)
                                { continue; }
                                else { maybeEntryId = entriesInRequest[0].ToString(); }
                            }
                            else { maybeEntryId = deserializedValue.ToString(); }
                        }
                        if (format == Format.URL)
                        {
                            try
                            {
                                string originalValue = maybeEntryId;
                                maybeEntryId = HttpUtility.UrlDecode(maybeEntryId);
                                maybeEntryId = maybeEntryId.Replace("[", "");
                                maybeEntryId = maybeEntryId.Replace("]", "");
                                replaced_value = originalValue.Replace(maybeEntryId, replaced_value);
                            }
                            catch { }
                        }
                        long tryParseId = 0;
                        bool isId = long.TryParse(maybeEntryId, out tryParseId);
                        if (stringListRootId.Contains("|" + maybeEntryId + "|") || !isId)
                        { continue; }
                    }

                    replaced_value = string.Format("{3}{0}{3}{1}{3}{2}{3}", key, splitValue, replaced_value, prefix_suffix);
                    // Try to do a regular replace first. If it succeeds, the regex replace will not work
                    // This is needed for when the extracted_value contains brackets, etc., which are special
                    // characters in Regex.Replace
                    ret_str = ret_str.Replace(extracted_value, replaced_value);
                    // Regex.Replace because HTTPUtility.UrlEncode, ParseQueryString will convert capitalized letters to lowercase.
                    // Not spending time to get the rules for this, so just use Regex.Replace
                    try { ret_str = Regex.Replace(ret_str, extracted_value, replaced_value, RegexOptions.IgnoreCase); }
                    catch (Exception) { } // If extracted_value has brackets, it may cause Regex.Replace to fail
                }

                foreach (string s in nvc_InvalidJSON)
                { ret_str = ret_str.Replace(nvc_InvalidJSON[s], s); }
                matches.Insert(0, ret_str);
            }

            return matches;
        }
    }
}
