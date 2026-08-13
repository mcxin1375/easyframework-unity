using System;

namespace EasyFramework.Editor
{
    [Serializable]
    public class DingTalkConfig
    {
        public string tag;
        public string url;
        public string secret;
        public ETaskResultState state = ETaskResultState.Failed | ETaskResultState.Succeeded | ETaskResultState.Canceled;
    }
}
