/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using EasyFramework.Editor;
using UnityEngine.Rendering;

namespace EasyFramework.Server.Editor
{
    [ProjectSettings("ProjectSettings/EasyFramework")]
    public class SVCErrorLogCollectorSettings : ProjectSettingsEditor<SVCErrorLogCollectorSettings>
    {
        public bool enabled = true;
        public bool svnCommitEnabled = true;
        public ShaderTagToPassType[] tagToPass = new []
        {
            new ShaderTagToPassType()
            {
                shaderTagName = "UniversalForward",
                passType = PassType.ScriptableRenderPipeline
            }
        };
        
        [Serializable]
        public class ShaderTagToPassType
        {
            public string shaderTagName;
            public PassType passType;
        }
    }
}