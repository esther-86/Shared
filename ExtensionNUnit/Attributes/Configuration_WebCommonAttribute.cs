using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace ExtensionNUnit
{
    [Flags]
    public enum Browser : short
    {
        IEXPLORE = 1,
        FIREFOX = 2,
        CHROME = 4,
        ALL = IEXPLORE | FIREFOX | CHROME
    }

    public enum Options_HandleWindow
    { IMPORT, OPEN, SAVE, CANCEL }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    /// : TestCaseSourceAttribute 
    /// 10/10/2012: Should only consider this a custom attribute and not a TestCaseSourceAttribute 
    /// i.e.: Do not inherit from TestCaseSourceAttribute, only Attribute
    /// because if inherit from TestCaseSourceAttribute, the core NUnit code will try to generate
    /// the test cases, in addition to the generation from our custom add-in
    /// Having this custom attribute should only let the add-in know that it should generate additional
    /// test cases. It shouldn't cause the core NUnit to do that also.
    public class Configuration_WebCommonAttribute : Attribute
    {
        public string Cfg_Domain { get; set; }
        public Browser Cfg_Browser { get; set; }
        public string Cfg_URL { get; set; }
        public string Cfg_Build { get; set; }
        public string Cfg_Comments { get; set; }

        public Dictionary<string, string> FileContent { get; protected set;}
        public Type DataGeneratorClass { get; protected set; }
        public string DataGeneratorMethod { get; protected set; }
        public bool PassDataGeneratorConfigurationObject { get; protected set; }

        // Set default values for test
        public Configuration_WebCommonAttribute(Type dataGeneratorClass, string dataGeneratorMethod,
            bool passDataGeneratorConfigurationObject)
        {
            InitializeNoOverride(dataGeneratorClass, dataGeneratorMethod, 
                passDataGeneratorConfigurationObject);
        }

        public Configuration_WebCommonAttribute(string directory, string filePattern, 
            Type dataGeneratorClass, string dataGeneratorMethod,
            bool passDataGeneratorConfigurationObject)
        {
            InitializeNoOverride(dataGeneratorClass, dataGeneratorMethod,
                passDataGeneratorConfigurationObject);

            try
            {
                LoadConfigurationFileIntoDictionary(directory, filePattern);
            }
            catch { }
        }

        void InitializeNoOverride(Type dataGeneratorClass, string dataGeneratorMethod,
            bool passDataGeneratorConfigurationObject)
        {
            this.FileContent = new Dictionary<string, string>();

            Initialize();

            this.DataGeneratorClass = dataGeneratorClass;
            this.DataGeneratorMethod = dataGeneratorMethod;
            this.PassDataGeneratorConfigurationObject = passDataGeneratorConfigurationObject;
        }

        void LoadConfigurationFileIntoDictionary(string directory, string filePattern)
        {
            Dictionary<string, PropertyInfo> thisClassProperties = 
                new Dictionary<string, PropertyInfo>();
            PropertyInfo[] properties = this.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                Type type = property.PropertyType;
                if (type != typeof(string))
                    continue;

                thisClassProperties.Add(property.Name, property);
            }

            // User have specified a configuration file
            // thus, need to read it and parse the file content
            IEnumerable<string> configFiles = Directory.EnumerateFiles(directory, filePattern);
            foreach (string configFilePath in configFiles)
            {
                XDocument xDoc = XDocument.Load(configFilePath);
                IEnumerable<XNode> xElements = xDoc.Root.Descendants();
                foreach (XElement xElement in xElements)
                {
                    string key = xElement.Name.ToString();
                    this.FileContent[key] = xElement.Value;

                    // If user specified the value for the property in the config file
                    // set it to the value of the property
                    if (thisClassProperties.ContainsKey(key))
                    {
                        PropertyInfo property = thisClassProperties[key];
                        property.SetValue(this, xElement.Value, null);
                    }
                }
            }
        }

        public virtual void Initialize()
        {
            this.Cfg_Browser = Browser.IEXPLORE;
            this.Cfg_URL = "http://www.google.com";
        }
    }

    public class Configuration
    {
        // Will only be initialized once
        // FullTestName: Configuration
        protected static Dictionary<string, Configuration_WebCommonAttribute> changes = new Dictionary<string, Configuration_WebCommonAttribute>();
        public static Dictionary<string, Configuration_WebCommonAttribute> Changes
        {
            get { return changes; }
        }
    }
}
