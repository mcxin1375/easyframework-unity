/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace EasyFramework.Editor
{
    public class SvnStatusInfo
    {
        public List<string> Unversioned { get; } = new List<string>(); // 状态 '?'
        public List<string> Missing { get; } = new List<string>();     // 状态 '!'
        public List<string> Modified { get; } = new List<string>();    // 状态 'M'
        public List<string> Deleted { get; } = new List<string>();     // 状态 'D'
        public List<string> Added { get; } = new List<string>();       // 状态 'A'
        public List<string> Conflicted { get; } = new List<string>();  // 状态 'C'
                                                                       // 其他状态可以按需添加

        public static SvnStatusInfo Parse(string output)
        {
            SvnStatusInfo result = new SvnStatusInfo();
            foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var status = line.Substring(0, 1);
                var filePath = line.Substring(1).Trim();

                switch (status)
                {
                    case "?": result.Unversioned.Add(filePath); break;
                    case "!": result.Missing.Add(filePath); break;
                    case "M": result.Modified.Add(filePath); break;
                    case "D": result.Deleted.Add(filePath); break;
                    case "A": result.Added.Add(filePath); break;
                    case "C": result.Conflicted.Add(filePath); break;
                        // 添加更多状态的处理逻辑
                }
            }
            return result;
        }
    }

    public class SvnVersionInfo
    {
        public int Revision { get; private set; }
        public string Author { get; private set; }
        public string Date { get; private set; }
        // public int LastChangedRev { get; private set; }
        // public string Message { get; private set; }

        // 解析单个版本信息
        public static SvnVersionInfo Parse(string data)
        {
            // Debug.Log(data);

            try
            {
                var doc = XDocument.Parse(data);
                var entry = doc.Root?.Element("entry");
                if (entry == null) throw new InvalidOperationException("Invalid SVN info XML format");

                return new SvnVersionInfo
                {
                    // Revision = int.Parse(entry.Attribute("revision")?.Value ?? "0"),
                    // Message = entry.Element("commit")?.Element("msg")?.Value,
                    Revision = int.Parse(entry.Element("commit")?.Attribute("revision")?.Value ?? "0"),
                    Author = entry.Element("commit")?.Element("author")?.Value,
                    Date = entry.Element("commit")?.Element("date")?.Value
                };
            }
            catch (Exception e)
            {
                //Debug.LogWarning(e);
                return null;
            }
        }
    }

    public class SvnVersionLog
    {
        public int Revision { get; private set; }
        public string Author { get; private set; }
        public string Date { get; private set; }
        public string Message { get; private set; }

        // 解析单个版本信息
        public static SvnVersionLog Parse(string data)
        {
            // Debug.Log(data);

            try
            {
                var doc = XDocument.Parse(data);
                var entry = doc.Root?.Element("logentry");
                if (entry == null) throw new InvalidOperationException("Invalid SVN info XML format");

                return new SvnVersionLog
                {
                    Revision = int.Parse(entry.Attribute("revision")?.Value ?? "0"),
                    Author = entry.Element("author")?.Value,
                    Date = entry.Element("date")?.Value,
                    Message = entry.Element("msg")?.Value
                };
            }
            catch (Exception e)
            {
                //Debug.LogWarning(e);
                return null;
            }
        }

        // 解析多个版本信息
        public static SvnVersionLog[] ParseMultiple(string data)
        {
            // Debug.Log(data);
            try
            {
                var doc = XDocument.Parse(data);
                return doc.Descendants("logentry")
                    .Select(entry => new SvnVersionLog
                    {
                        Revision = int.Parse(entry.Attribute("revision")?.Value ?? "0"),
                        Author = entry.Element("author")?.Value,
                        Date = entry.Element("date")?.Value,
                        Message = entry.Element("msg")?.Value
                    })
                    .ToArray();
            }
            catch (Exception e)
            {
                //Debug.LogWarning(e);
                return null;
            }
        }
    }

}