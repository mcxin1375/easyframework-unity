/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2022/6/1
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public enum EConfigEncrypt
    {
        None,
        DES
    }

    public enum EConfigType
    {
        Unity,
        Newtonsoft,
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ConfigSettingsAttribute : Attribute
    {
        public readonly EConfigType Type;
        public readonly EConfigEncrypt Encrypt;
        public readonly string Key;
        public ConfigSettingsAttribute(EConfigType type, EConfigEncrypt encrypt = EConfigEncrypt.None, string key = null)
        {
            Type = type;
            Encrypt = encrypt;
            Key = key;
        }
    }
}

