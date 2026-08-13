/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2020/4/14
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;

namespace EasyFramework.Editor
{
    public class ProtocolInfo
    {
        public ProtoFileInfo[] FileInfos { get; private set; }
        public MessageInfo[] MessageInfos { get; private set; }

        public void RefreshData(ProtoFileInfo[] fileInfos)
        {
            FileInfos = fileInfos;

            List<MessageInfo> tmp = new List<MessageInfo>();
            if (fileInfos != null)
            {
                foreach (ProtoFileInfo fileInfo in fileInfos)
                {
                    tmp.AddRange(fileInfo.MessageInfos);
                }
            }
            MessageInfos = tmp.ToArray();
        }
    }
}
