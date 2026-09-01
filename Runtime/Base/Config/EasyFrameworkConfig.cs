
using System;

namespace EasyFramework
{
    [Serializable]
    public class EasyFrameworkConfig : SingletonConfig<EasyFrameworkConfig>
    {
        public string DLCVersionInfoUid => dlcVersion?.dlcVersionInfoUid;
        public string DLCServerUrl { get; private set; }
        
        public string mainResUid;
        public DLCVersion dlcVersion;

        public void Refresh()
        {
            if (dlcVersion != null)
            {
                DLCServerUrl = DLCHelper.GetDLCResListURL(dlcVersion.versionName);
            }
        }

        protected override void OnCreate() => Refresh();
        protected override void OnSave() => Refresh();
    }
}