
using System.Collections.Generic;

namespace EasyFramework.Editor
{
    public class ExcelEnumInfo
    {
        public class KeyValueInfo
        {
            public string Key;
            public int Value;
            public string Description;
        }
        
        public readonly string TypeName;
        public readonly List<KeyValueInfo> ValueList = new();

        public ExcelEnumInfo(string typeName)
        {
            TypeName = typeName;
        }

        public void Add(string key, int value, string description)
        {
            ValueList.Add(new KeyValueInfo()
            {
                Key = key,
                Value = value,
                Description = description
            });
        }
    }
}
