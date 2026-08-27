/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public interface IDLCManager
    {
        ETask UpdateAsync();
        // ETask UpdateAsync(string dlcVersion);

        void DownloadFile(string fileName, Action<bool> callback = null);
        void DownloadFiles(string[] fileNames, Action<bool> callback = null);
        ETask<bool> DownloadFileAsync(string fileName);
        // ETask<bool> DownloadFilesAsync(string[] fileNames);
        // string GetFileHashName(string fileName);
    }
}