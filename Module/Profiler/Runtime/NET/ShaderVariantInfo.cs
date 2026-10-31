/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/1/30
// describe:
//----------------------------------------------------------------*/

using System.Linq;

namespace EasyFramework.Profiler
{
    public class ShaderVariantInfo
    {
        public const string LogStartMatchStr = "Shader ";
        public const string LogEndMatchStr = " not found.";
        
        public readonly string ShaderName;
        public readonly int Pass;
        public readonly string[] Keywords;
        public readonly string Log;

        public ShaderVariantInfo(string shaderName, int pass, string[] keywords, string log)
        {
            ShaderName = shaderName;
            Pass = pass;
            Keywords = keywords;
            Log = log;
        }

        public static bool TryParseFromLog(string log, out ShaderVariantInfo shaderVariantInfo)
        {
            shaderVariantInfo = null;

            if (!log.IsMatch(LogStartMatchStr, LogEndMatchStr)) return false;

            var shaderName = log.Extract("Shader ", ", subshader");
            var passStr = log.Extract("pass ", ", stage");
            var keywordsStr = log.Extract("variant ", " not found");
            if (string.IsNullOrEmpty(shaderName) || string.IsNullOrEmpty(passStr) || string.IsNullOrEmpty(keywordsStr)) return false;

            int pass = int.Parse(passStr);
            string[] keywords = keywordsStr == "<no keywords>" ? null : keywordsStr.Split(' ').Where(e => !string.IsNullOrEmpty(e)).ToArray();
            shaderVariantInfo = new ShaderVariantInfo(shaderName, pass, keywords, log);
            return true;
        }

    }
}