
//using System.Collections.ObjectModel;
//using static EasyFramework.ExcelData.ExcelDataLoader;



//namespace EasyFramework.ExcelData
//{
//    public static class ExcelDataLoader
//    {
//        public static void Load(string file)
//        {
//            using (FileStream fileStream = new FileStream(file, FileMode.Open))
//            using (BinaryReader reader = new BinaryReader(fileStream))
//            {
//                ExcelActiveDailySignData.InitInternal(reader);
//            }
//        }
//    }
//}

//namespace EasyFramework
//{
//    public class Audio
//    {
//        public int Id { get; private set; }
//        public string AssetName { get; private set; }
//        public string Who { get; private set; }
//        public byte Coexistence { get; private set; }

//        public static Audio[] Items { get; private set; }
//        public static IReadOnlyDictionary<int, Audio> ItemReadOnlyDict => _itemDict;
//        private static Dictionary<int, Audio> _itemDict = new();

//        public static Audio? Get(int id) => _itemDict.ContainsKey(id) ? _itemDict[id] : null;
//    }
//}

//namespace EasyFramework.ExcelData
//{

//    public static class ExcelDataLoader
//    {
//        public delegate string GetExcelDataFile(string typeName);
//        public static void LoadAll(GetExcelDataFile del)
//        {
//            ExcelAudioData.Load(del(typeof(ExcelAudioData).Name));
//        }
//    }

//    public class ExcelAudioData
//    {
//        public int Id { get; private set; }
//        public string AssetName { get; private set; }
//        public string Who { get; private set; }
//        public byte Coexistence { get; private set; }


//        public static ExcelAudioData[] Items { get; private set; }
//        public static IReadOnlyDictionary<int, ExcelAudioData> ItemReadOnlyDict => _itemDict;
//        private static readonly Dictionary<int, ExcelAudioData> _itemDict = new();

//        public static ExcelAudioData? Get(int Id) => _itemDict.ContainsKey(Id) ? _itemDict[Id] : null;

//        internal static void Load(string file)
//        {
//            Items = LoadFromFile(file);
//            _itemDict.Clear();
//            for (int i = 0; i < Items.Length; i++)
//            {
//                var item = Items[i];
//                _itemDict[item.Id] = item;
//            }
//        }

//        private static ExcelAudioData[] LoadFromMemory(byte[] binary)
//        {
//            using (MemoryStream memoryStream = new MemoryStream(binary))
//            using (BinaryReader reader = new BinaryReader(memoryStream))
//            {
//                int count = reader.ReadInt32();
//                var arr = new ExcelAudioData[count];
//                for (int i = 0; i < count; i++)
//                {
//                    arr[i] = new ExcelAudioData
//                    {
//                        Id = reader.ReadInt32(),
//                        AssetName = reader.ReadString(),
//                        Who = reader.ReadString(),
//                        Coexistence = reader.ReadByte(),

//                    };
//                }
//                return arr;
//            }
//        }

//        private static ExcelAudioData[] LoadFromFile(string file)
//        {
//            using (FileStream fileStream = new FileStream(file, FileMode.Open))
//            using (BinaryReader reader = new BinaryReader(fileStream))
//            {
//                int count = reader.ReadInt32();
//                var arr = new ExcelAudioData[count];
//                for (int i = 0; i < count; i++)
//                {
//                    arr[i] = new ExcelAudioData
//                    {
//                        Id = reader.ReadInt32(),
//                        AssetName = reader.ReadString(),
//                        Who = reader.ReadString(),
//                        Coexistence = reader.ReadByte(),

//                    };
//                }
//                return arr;
//            }
//        }

//    }
//}
