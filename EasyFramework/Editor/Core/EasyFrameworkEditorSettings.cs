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

    [ProjectSettings("ProjectSettings/EasyFramework")]
    public class EasyFrameworkEditorSettings : ProjectSettingsEditor<EasyFrameworkEditorSettings>
    {
        public EResLoaderMode resLoaderMode = EResLoaderMode.Editor;
    }
}