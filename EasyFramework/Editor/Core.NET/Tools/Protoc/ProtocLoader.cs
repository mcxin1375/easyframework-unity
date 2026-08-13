/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2020/4/14
// describe:
//----------------------------------------------------------------*/

using System.IO;
using System.Collections.Generic;

namespace EasyFramework.Editor
{
    public static class ProtocolLoader
    {

        public static ProtocolInfo Load(string path)
        {
            ProtocolInfo pbInfo = new ProtocolInfo();
            //pbInfo.messageIDInfos =  LoadMsgId(StringEasyCHelper.Combine(path, "event.lua"));
            //pbInfo.msgInfoArray = LoadProtocol(StringEasyCHelper.Combine(path, "protocol.proto"));

            List<ProtoFileInfo> tmpList = new List<ProtoFileInfo>();
            string[] files = Directory.GetFiles(path, "*.proto", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                tmpList.Add(LoadProtocol(file));
            }

            pbInfo.RefreshData(tmpList.ToArray());
            return pbInfo;
        }

        private static MessageIDInfo[] LoadMsgId(string file)
        {
            List<MessageIDInfo> list = new List<MessageIDInfo>();
            int msgStartIndex = 0;
            string[] arr = File.ReadAllLines(file);
            foreach (string tmpStr in arr)
            {
                if (tmpStr.StartsWith("--"))
                {
                    string[] arr1 = tmpStr.Split('=');
                    if (arr1.Length == 2)
                    {
                        int result;
                        if (int.TryParse(arr1[1], out result))
                        {
                            msgStartIndex = result;
                            //Log.Info(msgStartIndex);
                        }
                    }
                    continue;
                }
                //string str = tmpStr.Replace("\"", "@");
                string[] arr2 = tmpStr.Split('"');
                if (arr2.Length > 1)
                {
                    string name = arr2[1].Trim();
                    string[] descArr = tmpStr.Split('-');
                    list.Add(new MessageIDInfo(name, msgStartIndex++, descArr[descArr.Length - 1]));
                }
            }
            return list.ToArray();
        }

        private static ProtoFileInfo LoadProtocol(string file)
        {
            List<MessageInfo> list = new List<MessageInfo>();

            MessageInfo tempInfo = new MessageInfo();
            List<MessageFiledInfo> filedInfoList = new List<MessageFiledInfo>();

            string fileName = Path.GetFileNameWithoutExtension(file);
            tempInfo.moduleName = fileName;

            string[] arr = File.ReadAllLines(file);
            for (int i = 0; i < 5; i++)
            {
                arr[i] = "";
            }
            foreach (var tmpStr in arr)
            {
                string fieldDes = "";
                //去掉//注释
                string[] desArr = tmpStr.TrimStart().Split('/');
                if (desArr.Length > 1 && !string.IsNullOrEmpty(desArr[0]))
                {//字段后面带注释
                    fieldDes = desArr[desArr.Length - 1];
                }

                string lineStr = tmpStr.Replace("//", "");

                string[] tagArr = lineStr.Split(':');
                if (tagArr.Length > 1)
                {
                    switch (tagArr[0])
                    {
                        case "[DES]":
                            tempInfo.desc = tagArr[1];
                            break;
                        case "[CMD]":
                            tempInfo.cmd = tagArr[1];
                            break;
                        case "[REP]":
                            tempInfo.rep = tagArr[1];
                            break;
                        case "[OPTION]":
                            tempInfo.option = tagArr[1];
                            break;
                    }
                }

                //获取message消息名
                string[] lineArr = lineStr.Split(' ', '{');
                //Log.Info("lineStr. ", lineStr, lineArr.Length);
                if (lineArr.Length > 1 && lineArr[0] == "message")
                {
                    filedInfoList.Clear();
                    tempInfo.name = lineArr[1].Trim();
                    //Log.Info("message. ", lineArr[1], tempInfo.name);
                    continue;
                }

                //获取字段
                lineArr = lineStr.Split('=');
                if (lineArr.Length > 1)
                {
                    string[] fileArr = lineArr[0].Trim().Split(' ', ',');
                    if (fileArr.Length == 2)
                    {
                        filedInfoList.Add(new MessageFiledInfo("", fileArr[0], fileArr[1], filedInfoList.Count + 1, fieldDes));
                    }
                    else if (fileArr.Length == 3)
                    {
                        filedInfoList.Add(new MessageFiledInfo(fileArr[0], fileArr[1], fileArr[2], filedInfoList.Count + 1, fieldDes));
                    }
                }

                //协议读取完毕
                lineArr = lineStr.Split('}');
                if (lineArr.Length > 1)
                {
                    //Log.Info(name, module, des, cmd, rep);
                    list.Add(new MessageInfo(tempInfo.name?.Trim(), tempInfo.moduleName?.Trim(), tempInfo.desc?.Trim(), tempInfo.cmd?.Trim(), tempInfo.rep?.Trim(), tempInfo.option?.Trim(), filedInfoList.ToArray()));
                    filedInfoList.Clear();
                    tempInfo.Clear();
                }
            }

            return new ProtoFileInfo(fileName, list.ToArray());
            //return list.ToArray();
        }

    }
}
