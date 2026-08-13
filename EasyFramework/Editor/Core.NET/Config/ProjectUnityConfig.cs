using System;
using System.Collections.Generic;

namespace EasyFramework.Editor
{
    [Serializable]
    public class ProjectUnityConfig
    {
        public List<AppConfig> appList = new();
        public List<DingTalkConfig> dingTalkConfigList = new();
        
        public ExcelCommandSettings excelCommandSettings = new();
        public ProtocCommandSettings protocCommandSettings = new();
        
        // public List<SvnConfig> svnConfigList = new();
        // public List<FTPSyncConfig> ftpSyncConfigList = new();
        // public List<TimerConfig> timerConfigList = new();
        // public UnityConfig unityConfig = new();

        // public void Add(string appName)
        // {
        //     foreach (var appConfig in AppList) if (appConfig.AppName == appName) return;
        //
        //     var newAppConfig = new AppConfig
        //     {
        //         AppName = appName
        //     };
        //     AppList.Add(newAppConfig);
        // }
        //
        // public AppConfig GetAppConfig(string appName)
        // {
        //     for (int i = 0; i < appList.Count; i++)
        //     {
        //         var config = appList[i];
        //         if (config.appName == appName) return config;
        //     }
        //     return null;
        // }
    }
}
