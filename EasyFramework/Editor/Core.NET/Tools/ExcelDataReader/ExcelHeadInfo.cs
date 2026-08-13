using System;

namespace EasyFramework.Editor
{
    public class ExcelDataHeadInfo
    {
        public readonly int Index;
        public readonly string Key;
        public readonly string Type;
        public readonly string WriteType;
        public readonly string Description;

        public ExcelDataHeadInfo(int index, string key, string type, string writeType, string description)
        {
            Index = index;
            Key = key;
            Type = type;
            WriteType = writeType;
            Description = description;
        }
    }
}
