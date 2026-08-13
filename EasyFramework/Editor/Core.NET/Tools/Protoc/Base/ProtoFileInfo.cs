/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2020/4/14
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework.Editor
{
    public class ProtoFileInfo
    {
        public string FileName { get; private set; }
        public MessageInfo[] MessageInfos { get; private set; }
        public ProtoFileInfo(string fileName, MessageInfo[] messageInfos)
        {
            FileName = fileName;
            MessageInfos = messageInfos;
        }
    }
}
