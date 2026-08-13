/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public interface IDLCManager
    {
        ETask EnterAsync();
        ETask EnterAsync(string dlcVersion);

        ETask DownloadFileAsync(string fileName);
        ETask DownloadFilesAsync(string[] fileNames);

        string GetFileHashName(string fileName);
    }
}