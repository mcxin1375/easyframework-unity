/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public class DLCDownloader : Singleton<DLCDownloader>
    {
        public async ETask<bool> DownloadAsync(string fileName)
        {
            bool result = false;
            bool isCompleted = false;
            F.DLCManager.DownloadFile(fileName, b =>
            {
                isCompleted = true;
                result = b;
            });

            await ETask.WaitUntil(() => isCompleted);
            return result;
        }
        
        public async ETask<bool> DownloadAsync(string[] fileNames)
        {
            bool result = false;
            bool isCompleted = false;
            F.DLCManager.DownloadFiles(fileNames, b =>
            {
                isCompleted = true;
                result = b;
            });

            await ETask.WaitUntil(() => isCompleted);
            return result;
        }
    }
}