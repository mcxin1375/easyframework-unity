/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/



namespace EasyFramework.Editor
{
    public static class DLCBuilderHelper
    {
        public static DLCBuilderVersionList LoadDLCBuilderVersionList()
        {
            var file = $"{DLCBuilder.Instance.ProjectDataPath}/{nameof(DLCBuilderVersionList)}.json";
            return UnityJsonHelper.LoadOrCreate<DLCBuilderVersionList>(file);
        }
    }
}