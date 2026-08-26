using System.IO;
using System.Text;

namespace EasyFramework
{
    public static class CRC32Helper
    {
        // 预计算的 CRC32 查表，提高性能
        private static readonly uint[] Table = CreateCRCTable();

        private static uint[] CreateCRCTable()
        {
            uint[] table = new uint[256];
            const uint poly = 0xEDB88320u;
            for (uint i = 0; i < table.Length; i++)
            {
                uint crc = i;
                for (int j = 0; j < 8; j++)
                    crc = (crc & 1) == 1 ? (crc >> 1) ^ poly : crc >> 1;
                table[i] = crc;
            }
            
            return table;
        }

        /// <summary>
        /// 计算字节的 CRC32
        /// </summary>
        public static uint HashBytes(byte[] bytes)
        {
            uint crc = 0xFFFFFFFFu;
            foreach (byte b in bytes)
            {
                uint index = (crc ^ b) & 0xFF;
                crc = (crc >> 1) ^ Table[index];
            }
            return ~crc;
        }

        /// <summary>
        /// 计算字符串 UTF8 的 CRC32
        /// </summary>
        public static uint HashString(string str)
        {
            return HashBytes(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// 计算文件 CRC32（流式读取，不占用内存）
        /// </summary>
        public static uint HashFile(string path, int bufferSize = 64 * 1024)
        {
            uint crc = 0xFFFFFFFFu;
            byte[] buffer = new byte[bufferSize];

            using (var stream = File.OpenRead(path))
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < read; i++)
                    {
                        uint index = (crc ^ buffer[i]) & 0xFF;
                        crc = (crc >> 1) ^ Table[index];
                    }
                }
            }

            return ~crc;
        }
        
        public static bool IsFileMatch(string filePath, uint crc32, long fileSize)
        {
            if (!File.Exists(filePath)) return false;
            FileInfo fi = new FileInfo(filePath);
            if (fi.Length != fileSize) return false;
            return HashFile(filePath) == crc32;
        }
        
    }
}
