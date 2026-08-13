
using System;

namespace EasyFramework.Editor
{
    [Serializable]
    public class AppConfig
    {
        public string DefineSymbolsStr
        { 
            get => defineSymbols != null ? string.Join(";", defineSymbols) : string.Empty;
            set => defineSymbols = !string.IsNullOrEmpty(value) ? value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
        }
        public string appName;
        public string[] defineSymbols;
    }
}
