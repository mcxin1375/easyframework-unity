/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2020/4/14
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework.Editor
{
    public class MessageIDInfo
    {
        public MessageIDInfo(string name, int msgId, string desc)
        {
            this.name = name;
            this.msgId = msgId;
            this.desc = desc;
        }

        public string name
        {
            get;
            private set;
        }
        public int msgId
        {
            get;
            private set;
        }
        public string desc
        {
            get;
            private set;
        }
        public override string ToString()
        {
            return $"name:{name}, msgId:{msgId}";
        }
    }
}
