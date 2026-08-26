/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    [Serializable]
    public class ToolVersion
    {
        public string DateTimeStr => DateTime.ToString("yyyy_MMd_HHmm");
        public DateTime DateTime => DateTime.FromFileTime(dateTime);
        
        public int buildIndex;
        public int revision;
        public long dateTime;
        
        public void SetToolVersion(ToolVersion toolVersion)
        {
            buildIndex = toolVersion.buildIndex;
            revision = toolVersion.revision;
            dateTime = toolVersion.dateTime;
        }
    }
}