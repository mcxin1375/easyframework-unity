/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework.Editor
{
    public enum EResLoaderMode
    {
        Editor,
        Runtime,
    }

    [ProjectSettings(ProjectSettingsAttribute.ETag.Editor)]
    public class EasyFrameworkEditorSettings : ProjectSettings<EasyFrameworkEditorSettings>
    {
        public EResLoaderMode resLoaderMode = EResLoaderMode.Editor;
    }
}