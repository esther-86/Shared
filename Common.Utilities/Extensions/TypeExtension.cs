using System;
using System.Reflection;

namespace Common.Utilities
{
    public static class TypeExtension
    {
        public static void DoPerPropertyValue(this Type typeToGetPropertiesFor,
            Func<PropertyInfo, bool> shouldSkipProperty, Action<PropertyInfo> actionPerProperty)
        {
            PropertyInfo[] props = typeToGetPropertiesFor.GetProperties();
            foreach (PropertyInfo pi in props)
            {
                if (shouldSkipProperty.Invoke(pi))
                    continue;
                actionPerProperty.Invoke(pi);
            }
        }
    }
}
