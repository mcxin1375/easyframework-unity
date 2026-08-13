/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2020/4/14
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework.Editor
{
    public class MessageInfo
    {
        public MessageInfo()
        {

        }
        public MessageInfo(string name, string moduleName, string desc, string cmd, string rep, string option, MessageFiledInfo[] messageFiledInfos)
        {
            this.name = name;
            this.moduleName = moduleName;
            this.desc = desc;
            this.cmd = cmd;
            this.rep = rep;
            this.option = option;
            this.messageFiledInfos = messageFiledInfos;
        }
        //public MessageInfo(string name, string rep, string descr, MessageFiledInfo[] messageFiledInfos)
        //{
        //    this.name = name;
        //    this.desc = descr;
        //    this.rep = rep;
        //    this.messageFiledInfos = messageFiledInfos;
        //}

        public string moduleName;
        public string name;
        public string desc;
        public string cmd;
        public string rep;
        public string option;
        public MessageFiledInfo[] messageFiledInfos
        {
            get;
            private set;
        }

        private string FiledInfoArrayToString()
        {
            string content = "";
            foreach (MessageFiledInfo info in messageFiledInfos)
            {
                content += info.ToString() + ";";
            }
            return content;
        }

        public void Clear()
        {
            name = "";
            desc = "";
            cmd = "";
            rep = "";
            option = "";
        }

        public override string ToString()
        {
            //return string.Format("name:{0}, module:{1}, descr:{2}, cmd:{3}, rep:{4}, filedInfoArray:{5}", Name, Module, Des, Cmd, Rep, FiledInfoArrayToString());
            string content = $"message {name}\n";
            content += "{\n";
            if (messageFiledInfos != null)
            {
                foreach (MessageFiledInfo filedInfo in messageFiledInfos)
                {
                    if (string.IsNullOrEmpty(filedInfo.Des))
                    {
                        content += string.Format("	{0} {1} {2} = {3};\n", filedInfo.Tag, filedInfo.Type, filedInfo.Name, filedInfo.index);
                    }
                    else
                    {
                        content += string.Format("	{0} {1} {2} = {3};          // {4}\n", filedInfo.Tag, filedInfo.Type, filedInfo.Name, filedInfo.index, filedInfo.Des);
                    }
                }
            }
            content += "}\n";
            return content;
        }
    }
}
