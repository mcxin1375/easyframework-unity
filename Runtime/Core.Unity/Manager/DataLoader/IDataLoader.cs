

namespace EasyFramework
{
    public interface IDataLoader
    {
        // void LoadExcelData(string fileName);
        // Task LoadExcelDataAsync(string fileName);
        
        string GetDataFile(string fullName);
        string LoadDataAllText(string fullName);
        byte[] LoadDataAllBytes(string fullName);
        
        // T LoadAsset<T>(string assetName) where T : UnityEngine.Object;
        // T[] LoadAllAssets<T>(string assetName) where T : UnityEngine.Object;
        // Task<T> LoadAssetAsync<T>(string assetName) where T : UnityEngine.Object;
        // Task<T[]> LoadAllAssetsAsync<T>(string assetName) where T : UnityEngine.Object;
    }
}