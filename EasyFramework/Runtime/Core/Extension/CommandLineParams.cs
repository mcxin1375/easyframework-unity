using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyFramework
{
    public class CommandLineParams
    {
        public string[] Keys => _dict.Keys.ToArray();
        private readonly Dictionary<string, List<string>> _dict = new();

        public CommandLineParams(string[] lines) 
        {
            string key = string.Empty;

            foreach (string line in lines)
            {
                if (line.StartsWith("-"))
                {
                    key = line;
                    if ( !_dict.ContainsKey(key)) _dict[key] = new();
                    continue;
                }
                if (string.IsNullOrEmpty(key)) continue;
                
                if (!_dict.TryGetValue(key, out var list))
                {
                    list = new();
                    _dict[key] = list;
                }
                list.Add(line);
            }
        }
        
        public bool HasKey(string key) => _dict.ContainsKey(key);
        public string GetFirstOrEmpty(string key)
        {
            if (_dict.TryGetValue(key, out var list) && list.Count > 0) return list[0];
            return string.Empty;
        }
        public string[] GetValues(string key)
        {
            if (_dict.TryGetValue(key, out var list) && list.Count > 0) return list.ToArray();
            return Array.Empty<string>();
        }

    }
}
