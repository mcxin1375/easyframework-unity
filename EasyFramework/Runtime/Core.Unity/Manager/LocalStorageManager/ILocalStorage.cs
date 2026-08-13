
namespace EasyFramework
{
    public interface ILocalStorage
    {
        string DataPath { get; }
        
        bool Exists(string path);
        void Delete(string path);
        void ClearDirectory(string path);
        
        void WriteAllBytes(string path, byte[] bytes);
        void WriteAllText(string path, string contents);
        
        byte[] ReadAllBytes(string path);
        string ReadAllText(string path);
    }
}
