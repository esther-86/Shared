using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Common.Utilities
{
    public abstract class SplitInfo
    {
        public string MatchRegex { get; private set; }
        public string SplitBy { get; private set; }
        public SplitInfo(string matchRegex, string splitBy)
        {
            this.MatchRegex = matchRegex;
            this.SplitBy = splitBy;
        }
    }

    public class LineSplitInfo : SplitInfo
    {
        public List<ValueSplitInfo> ValueSplitInfos { get; private set; }

        public LineSplitInfo(string matchRegex, string splitBy, List<ValueSplitInfo> valueSplitInfos) : base(matchRegex, splitBy)
        {
            this.ValueSplitInfos = valueSplitInfos;
        }
    }

    public class ValueSplitInfo : SplitInfo
    {
        public Action<string> ProcessSplitValue { get; private set; }

        public ValueSplitInfo(string matchRegex, string splitBy, Action<string> action) : base(matchRegex, splitBy)
        {
            this.ProcessSplitValue = action;
        }
    }

    public static class StringExtension
    {
        public static List<Exception> Parse(this string info, params LineSplitInfo[] splitInfos)
        {
            List<Exception> exceptions = new List<Exception>();
            StringReader sr = new StringReader(info);
            string line = null;
            while ((line = sr.ReadLine()) != null)
            {
                foreach (LineSplitInfo lsi in splitInfos)
                {
                    if (!Regex.IsMatch(line, lsi.MatchRegex.EscapeSpecialRegexCharacters()))
                        continue;

                    string[] split = Regex.Split(line, lsi.SplitBy);
                    foreach (string kv in split)
                    {
                        foreach (ValueSplitInfo valueSplitInfo in lsi.ValueSplitInfos)
                        {
                            if (!Regex.IsMatch(kv, valueSplitInfo.MatchRegex.EscapeSpecialRegexCharacters()))
                                continue;

                            // Doing substring instead of split because in the case of duration (Duration: 00:00:30.03), 
                            // The split char is :, however, : also occurs in the value, so it's not split to key [0], value [1]
                            // Substring works better
                            string trimmedValue = kv.Substring(kv.IndexOf(valueSplitInfo.SplitBy) + 1).Trim();
                            try { valueSplitInfo.ProcessSplitValue.Invoke(trimmedValue); }
                            catch (Exception ex) { exceptions.Add(new Exception("Cannot parse " + trimmedValue, ex)); }
                        }
                    }
                }
            }
            return exceptions;
        }

        public static string AddQuotes(this string str)
        {
            return string.Format("\"{0}\"", str);
        }

        public static string AddValueToString(this string str, string splitValue, object value)
        {
            string valueStr = value.GetValueToString();
            if (string.IsNullOrEmpty(valueStr))
                return valueStr;

            if (!string.IsNullOrEmpty(str))
                str += splitValue;
            str += valueStr;
            return str;
        }

        public static string EscapeSpecialRegexCharacters(this string value)
        {
            if (value == null)
                return null;

            value = value.Replace("?", Regex.Escape("?"));
            value = value.Replace("+", Regex.Escape("+"));
            value = value.Replace(" ", "(\\s)*");
            return value;
        }
    }
}
