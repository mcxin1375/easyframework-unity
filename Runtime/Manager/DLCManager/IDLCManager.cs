/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public interface IDLCManager
    {
        ETask<EResult> UpdateAsync();
        // ETask UpdateAsync(string dlcVersion);

        string GetFileName(string resName);
        string GetFilePath(string resName);
        bool Exists(string resName);
        ETask<bool> DownloadAsync(string resName);
        ETask<bool> DownloadAsync(string resName, out string filePath);
        ETask<string> DownloadAndReturnFileAsync(string resName);
        
        public enum EResult
        {
            Success,
            InitVersionError,
            DLCUpdaterError,
        }
    }
}