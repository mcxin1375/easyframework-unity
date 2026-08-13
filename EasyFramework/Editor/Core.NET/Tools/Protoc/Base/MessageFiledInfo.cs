/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2020/4/14
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework.Editor
{

    public class MessageFiledInfo
    {
        public MessageFiledInfo(string tag, string type, string name, int index, string descr)
        {
            this.Tag = tag;
            this.Type = type;
            this.Name = name;
            this.index = index;
            this.Des = descr;
        }

        public string Tag
        {
            get;
            private set;
        }
        public string Type
        {
            get;
            private set;
        }
        public string Name
        {
            get;
            private set;
        }
        public int index
        {
            get;
            private set;
        }
        public string Des
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return $"tag:{Tag}, type:{Type}, name:{Name}, index:{index}, descr:{Des}";
        }
    }
}
