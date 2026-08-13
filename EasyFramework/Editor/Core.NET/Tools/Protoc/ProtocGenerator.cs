using System;
using System.Collections.Generic;
using System.Text;

namespace EasyFramework.Editor
{
    public static class ProtocGenerator
    {
        public static string CreateCmd(ProtocolInfo info, int svnRevision = 0, string nameSpace = "EasyFramework")
        {
            List<string> contentList = new List<string>();

            // contentList.Add($"// DateTime: {DateTime.Now.ToShortDateString()}\n");
            contentList.Add("public static class CMD\n");
            contentList.Add("{\n");

            contentList.Add($"    public const int SvnRevision = {svnRevision}; // \n\n");
            foreach (MessageInfo messageInfo in info.MessageInfos)
            {
                if (string.IsNullOrEmpty(messageInfo.cmd))
                    continue;
                //content += string.Format("    public const short {0} = {1}; // {2}\n", messageInfo.name, messageInfo.cmd, messageInfo.desc);
                contentList.Add($"    public const short {messageInfo.cmd}; // {messageInfo.desc}\n");
            }

            contentList.Add("\n");
            contentList.Add("    public static List<short> RepList { get; } = new List<short>()\n");
            contentList.Add("    {\n");
            foreach (MessageInfo messageInfo in info.MessageInfos)
            {
                if (string.IsNullOrEmpty(messageInfo.rep))
                    continue;
                string[] tmpArr = messageInfo.rep.Split('=');
                if (tmpArr.Length != 2)
                    continue;
                contentList.Add($"        {tmpArr[0].Trim()},\n");
            }
            contentList.Add("    };\n");
            contentList.Add("}\n");
            contentList.Add("public enum ECMD\n");
            contentList.Add("{\n");
            foreach (MessageInfo messageInfo in info.MessageInfos)
            {
                if (string.IsNullOrEmpty(messageInfo.cmd))
                    continue;
                //content += string.Format("    public const short {0} = {1}; // {2}\n", messageInfo.name, messageInfo.cmd, messageInfo.desc);
                contentList.Add($"    {messageInfo.cmd},     // {messageInfo.desc}\n");
            }
            contentList.Add("}\n");

            StringBuilder sb = new StringBuilder();
            sb.Append("using System.Collections.Generic;\n\n");

            string preStr = "";
            if (!string.IsNullOrWhiteSpace(nameSpace))
            {
                preStr = "    ";
                sb.Append($"namespace {nameSpace}\n");
                sb.Append("{\n");
            }
            foreach (string s in contentList)
            {
                sb.Append($"{preStr}{s}");
            }
            if (!string.IsNullOrWhiteSpace(nameSpace))
            {
                sb.Append("}");
            }

            return sb.ToString();
        }
        
        public static string CreateMessageSenderEx(ProtocolInfo info, string nameSpace = "EasyFramework")
        {
            StringBuilder fileBuilder = new StringBuilder();
            // fileBuilder.Append($"// DateTime: {DateTime.Now.ToShortDateString()}\n");
            fileBuilder.Append(@"
using EasyFramework;

namespace NAMESPACE_NAME
{
    public interface IMessageSender
    {
        void Send(short msgId, Google.Protobuf.IMessage message);
    }
     public static class IMessageSenderEx
     {
//C2S_EVENT_FUNC
     }
}
");

            StringBuilder c2sEventFunc = new StringBuilder();

            foreach (ProtoFileInfo protoFileInfo in info.FileInfos)
            {
                string fileName = protoFileInfo.FileName[0].ToString().ToUpper() + protoFileInfo.FileName.Substring(1);

                foreach (MessageInfo messageInfo in protoFileInfo.MessageInfos)
                {
                    if (string.IsNullOrEmpty(messageInfo.name) || string.IsNullOrEmpty(messageInfo.cmd)) continue;
                    string[] nameArr = messageInfo.name.Split('_');
                    if (nameArr.Length < 2) continue;
                    string[] tmpArr = messageInfo.cmd.Split('=');
                    if (tmpArr.Length != 2) continue;

                    if (nameArr[0] == "C2S")
                    {
                        if (messageInfo.messageFiledInfos?.Length > 0)
                        {
                            MessageFiledInfo messageFiledInfo = messageInfo.messageFiledInfos[0];
                            string nameEx = messageFiledInfo.Tag == "repeated" ? "[]" : string.Empty;
                            string valueEx = messageFiledInfo.Name == "noUse" ? " = false" : string.Empty;
                            string funcArgs = $"{ParseType(messageFiledInfo)}{nameEx} {messageFiledInfo.Name}{valueEx}";
                            for (int i = 1; i < messageInfo.messageFiledInfos.Length; i++)
                            {
                                messageFiledInfo = messageInfo.messageFiledInfos[i];
                                nameEx = messageFiledInfo.Tag == "repeated" ? "[]" : string.Empty;
                                funcArgs += $", {ParseType(messageFiledInfo)}{nameEx} {messageFiledInfo.Name}";
                            }

                            c2sEventFunc.Append($"          // {messageInfo.desc}\n");
                            c2sEventFunc.Append($"          public static void {messageInfo.name}(this IMessageSender handler, {funcArgs})\n");
                            c2sEventFunc.Append("          {\n");
                            c2sEventFunc.Append($"               var packet = Singleton<{messageInfo.name}>.Instance;\n");
                            foreach (MessageFiledInfo filedInfo in messageInfo.messageFiledInfos)
                            {
                                string propertyName = filedInfo.Name[0].ToString().ToUpper() + filedInfo.Name.Substring(1);

                                if (filedInfo.Tag == "repeated")
                                {
                                    c2sEventFunc.Append($"               packet.{propertyName}.Clear(); // {filedInfo.Des}\n");
                                    c2sEventFunc.Append($"               packet.{propertyName}.AddRange({filedInfo.Name}); // {filedInfo.Des}\n");
                                }
                                else
                                    c2sEventFunc.Append($"               packet.{propertyName} = {filedInfo.Name}; // {filedInfo.Des}\n");
                            }
                            c2sEventFunc.Append($"               {messageInfo.name}(handler, packet);\n");
                            c2sEventFunc.Append("          }\n");
                        }


                        c2sEventFunc.Append($"          public static void {messageInfo.name}(this IMessageSender handler, {messageInfo.name} packet)\n");
                        c2sEventFunc.Append("          {\n");
                        c2sEventFunc.Append($"               handler.Send(CMD.{messageInfo.name}, packet);\n");
                        c2sEventFunc.Append("          }\n");
                    }
                }
            }

            string content = fileBuilder.ToString();
            content = content.Replace("NAMESPACE_NAME", nameSpace);
            content = content.Replace("//C2S_EVENT_FUNC", c2sEventFunc.ToString());

            return content;
        }

        public static string CreateMessageProxyDeserialize(ProtocolInfo info, string nameSpace = "EasyFramework")
        {
            StringBuilder fileBuilder = new StringBuilder();
            // fileBuilder.Append($"// DateTime: {DateTime.Now.ToShortDateString()}\n");
            fileBuilder.Append(@"
using System;
using System.Reflection;
using Google.Protobuf;
using EasyFramework;

namespace NAMESPACE_NAME
{
     public partial class MessageProxy
     {
          public static IMessage Deserialize(short msgId, ReadOnlySpan<byte> span)
          {
               switch (msgId)
               {
//PARSE_FROM_BYTES
               }
               return null;
          }
          public static T Deserialize<T>(byte[] data) where T : IMessage<T>
          {
               if (data == null || data.Length == 0)
               {
                    throw new ArgumentNullException(nameof(data));
               }
               var parser = GetParser<T>();
               return parser.ParseFrom(data);
          }
          public static MessageParser<T> GetParser<T>() where T : IMessage<T>
          {
               var parser = typeof(T).GetProperty(""Parser"", BindingFlags.Public | BindingFlags.Static);
               return (MessageParser<T>)parser?.GetValue(null, null);
          }
          public static MessageParser GetParser(short msgId)
          {
               switch (msgId)
               {
//GET_PARSER
               }
               return null;
          }
     }
}
");
            StringBuilder parseFromBytesPacket = new StringBuilder();
            StringBuilder getParsePacket = new StringBuilder();

            foreach (ProtoFileInfo protoFileInfo in info.FileInfos)
            {
                foreach (MessageInfo messageInfo in protoFileInfo.MessageInfos)
                {
                    if (string.IsNullOrEmpty(messageInfo.name) || string.IsNullOrEmpty(messageInfo.cmd))
                        continue;
                    string[] nameArr = messageInfo.name.Split('_');
                    //if (nameArr.Length < 2 || nameArr[0] != "S2C")
                    if (nameArr.Length < 2)
                        continue;
                    string[] tmpArr = messageInfo.cmd.Split('=');
                    if (tmpArr.Length != 2)
                        continue;

                    if (nameArr[0] == "S2C")
                    {
                        parseFromBytesPacket.Append($"                    case CMD.{messageInfo.name}: return {messageInfo.name}.Parser.ParseFrom(span);\n");
                        getParsePacket.Append($"                    case CMD.{messageInfo.name}: return {messageInfo.name}.Parser;\n");
                    }
                }
            }

            string content = fileBuilder.ToString();
            content = content.Replace("NAMESPACE_NAME", nameSpace);
            content = content.Replace("//PARSE_FROM_BYTES", parseFromBytesPacket.ToString());
            content = content.Replace("//GET_PARSER", getParsePacket.ToString());

            return content;
        }

        public static string CreateMessageHandler(ProtocolInfo info, string nameSpace = "EasyFramework")
        {
            string proxyTemp = @"
using EasyFramework;

namespace NAMESPACE_NAME
{
     public interface IMessageHandler
     {
          void OnClear() { }

//S2C_EVENT_FUNC     }
}";

            StringBuilder s2cBuilder = new StringBuilder();

            foreach (ProtoFileInfo protoFileInfo in info.FileInfos)
            {
                string fileName = protoFileInfo.FileName[0].ToString().ToUpper() + protoFileInfo.FileName.Substring(1);
                s2cBuilder.Append($"          // {fileName}\n");
                
                foreach (MessageInfo messageInfo in protoFileInfo.MessageInfos)
                {
                    if (string.IsNullOrEmpty(messageInfo.name) || string.IsNullOrEmpty(messageInfo.cmd)) continue;
                    string[] nameArr = messageInfo.name.Split('_');
                    //if (nameArr.Length < 2 || nameArr[0] != "S2C")
                    if (nameArr.Length < 2) continue;
                    string[] tmpArr = messageInfo.cmd.Split('=');
                    if (tmpArr.Length != 2) continue;

                    if (nameArr[0] == "S2C")
                    {
                        s2cBuilder.Append($"          void {messageInfo.name}({messageInfo.name} packet) {{ }}\n");
                    }
                }
            }

            var content = proxyTemp.Replace("NAMESPACE_NAME", nameSpace);
            content = content.Replace("//S2C_EVENT_FUNC", s2cBuilder.ToString());
            return content;
        }

        public static string CreateMessageProxy(ProtocolInfo info, string nameSpace = "EasyFramework")
        {
            StringBuilder fileBuilder = new StringBuilder();
            // fileBuilder.Append($"// DateTime: {DateTime.Now.ToShortDateString()}\n");
            fileBuilder.Append(@"
using System;
using EasyFramework;

namespace NAMESPACE_NAME
{
     public partial class MessageProxy : IMessageHandler
     {
          public readonly IMessageHandler MessageHandler;
//PACKET_PROPERTY
//PACKET_ACTION

          public MessageProxy(IMessageHandler messageHandler = null)
          {
               MessageHandler = messageHandler ?? this;
          }
          public void Clear()
          {
//CLEANUP_PACKET
               MessageHandler.OnClear();
          }

          public void Dispatch(short msgId, Google.Protobuf.IMessage message)
          {
               switch (msgId)
               {
//DISPATCH_PACKET
               }
          }
     }
}
");

            StringBuilder packetAction = new StringBuilder();
            StringBuilder packetProperty = new StringBuilder();
            StringBuilder cleanupProperty = new StringBuilder();
            StringBuilder dispatchPacket = new StringBuilder();

            //const string getStr = "{ get; }";
            foreach (ProtoFileInfo protoFileInfo in info.FileInfos)
            {
                string fileName = protoFileInfo.FileName[0].ToString().ToUpper() + protoFileInfo.FileName.Substring(1);
                string proxyName = $"{fileName}";

                foreach (MessageInfo messageInfo in protoFileInfo.MessageInfos)
                {
                    if (string.IsNullOrEmpty(messageInfo.name) || string.IsNullOrEmpty(messageInfo.cmd))
                        continue;
                    string[] nameArr = messageInfo.name.Split('_');
                    //if (nameArr.Length < 2 || nameArr[0] != "S2C")
                    if (nameArr.Length < 2)
                        continue;
                    string[] tmpArr = messageInfo.cmd.Split('=');
                    if (tmpArr.Length != 2)
                        continue;

                    bool cachePacket = true;
                    // string[] options = !string.IsNullOrEmpty(messageInfo.option) ? messageInfo.option.Split('|') : null;
                    // if (options != null)
                    // {
                    //     foreach (string option in options)
                    //     {
                    //         string[] arr = option.Split('=');
                    //         cachePacket = arr.Length == 2 && arr[0].Trim() == "CACHE_PACKET" && arr[1].Trim() == "1";
                    //     }
                    // }

                    if (nameArr[0] == "S2C")
                    {
                        if (cachePacket)
                        {
                            cleanupProperty.Append($"               {messageInfo.name} = null;\n");
                            packetProperty.Append($"          public {messageInfo.name} {messageInfo.name} " + "{ get; private set; }\n");
                        }

                        packetAction.Append($"          public event Action<{messageInfo.name}> {messageInfo.name}_ACTION;\n");

                        dispatchPacket.Append($"                    case CMD.{messageInfo.name}:\n");
                        // dispatchPacket.Append($"                    {messageInfo.name} _{messageInfo.name} = {messageInfo.name}.Parser.ParseFrom(bytes);\n");
                        dispatchPacket.Append($"                         {messageInfo.name} = message as {messageInfo.name};\n");
                        // dispatchPacket.Append($"                         Log.Warning(\"[ {messageInfo.cmd} ] = \", _{messageInfo.name}.ToString());\n");
                        // dispatchPacket.Append($"                         _{proxyName}?.{messageInfo.name}(_{messageInfo.name});\n");
                        dispatchPacket.Append($"                         MessageHandler.{messageInfo.name}({messageInfo.name});\n");
                        // dispatchPacket.Append($"                         {messageInfo.name}_EX(_{messageInfo.name});\n");
                        dispatchPacket.Append($"                         {messageInfo.name}_ACTION?.Invoke({messageInfo.name});\n");
                        dispatchPacket.Append("                    break;\n");
                    }
                }
            }

            string content = fileBuilder.ToString();
            content = content.Replace("NAMESPACE_NAME", nameSpace);
            content = content.Replace("//PACKET_PROPERTY", packetProperty.ToString());
            content = content.Replace("//CLEANUP_PACKET", cleanupProperty.ToString());
            content = content.Replace("//PACKET_ACTION", packetAction.ToString());
            content = content.Replace("//DISPATCH_PACKET", dispatchPacket.ToString());


            return content;
        }

        public static string ParseType(MessageFiledInfo file)
        {
            string type = file.Type.Trim();
            switch (type)
            {
                case "int32": return "int";
                case "int64": return "long";
            }
            return type;
        }

    }
}
