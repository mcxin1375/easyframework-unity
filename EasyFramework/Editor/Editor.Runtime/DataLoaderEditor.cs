using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class DataLoaderEditor : IDataLoader
    {
        private readonly Dictionary<string, string> _dataMap;

        public DataLoaderEditor()
        {
            var dataFiles = DataBuilder.Instance.GetDataFiles();
            _dataMap = dataFiles.ToDictionary(Path.GetFileName, item => item);
        }

        public string GetDataFile(string fullName) => _dataMap.ContainsKey(fullName) ? _dataMap[fullName] : string.Empty;
        public string LoadDataAllText(string fullName)
        {
            string file = GetDataFile(fullName);
            if (!File.Exists(file))
            {
                Debug.LogError($"LoadDataAllText({fullName}) Error. File: {file}");
                return string.Empty;
            }
            return File.ReadAllText(file);
        }
        public byte[] LoadDataAllBytes(string fullName)
        {
            string file = GetDataFile(fullName);
            if (!File.Exists(file))
            {
                Debug.LogError($"LoadDataAllBytes({fullName}) Error. File: {file}");
                return null;
            }
            return File.ReadAllBytes(file);
        }
        
    }
}