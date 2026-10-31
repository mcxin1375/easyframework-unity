/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/1/30
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace EasyFramework.Editor
{
    public static class SVCHelper
    {
        
        public static string ShaderVariantToString(Shader shader, ShaderSnippetData snippet, ShaderCompilerData compilerData)
        {
            return $"{shader.name}|{snippet.passType}|{KeywordsToString(compilerData.shaderKeywordSet.GetShaderKeywords())}";
        }
        public static string ShaderVariantToString(string shaderName, string passType, ShaderKeyword[] keywords)
        {
            return $"{shaderName}|{passType}|{KeywordsToString(keywords)}";
        }
        public static string ShaderVariantToString(string shaderName, string passType, string[] keywords)
        {
            return $"{shaderName}|{passType}|{KeywordsToString(keywords)}";
        }
        public static string ShaderVariantToString(ShaderVariantCollection.ShaderVariant shaderVariant)
        {
            return $"{shaderVariant.shader.name}|{shaderVariant.passType}|{KeywordsToString(shaderVariant.keywords)}";
        }
        
        public static string KeywordsToString(ShaderKeyword[] keywords)
        {
            if (keywords?.Length > 0)
            {
                StringBuilder sb = new StringBuilder();

                var arr = keywords.OrderBy(item => item.name);
                foreach (var keyword in arr)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append($" {keyword.name}");
                    }
                    else
                    {
                        sb.Append(keyword.name);
                    }
                }
                // for (int i = 0; i < keywords.Length; i++)
                // {
                //     sb.Append(i > 0 ? $" {keywords[i].name}" : $"{keywords[i].name}");
                // }

                return sb.ToString();
            }
            return "<no keywords>";
        }
        
        public static string KeywordsToString(string[] keywords)
        {
            if (keywords?.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                
                var arr = keywords.OrderBy(item => item);
                foreach (var keyword in arr)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append($" {keyword}");
                    }
                    else
                    {
                        sb.Append(keyword);
                    }
                }
                // for (int i = 0; i < keywords.Length; i++)
                // {
                //     sb.Append(i > 0 ? $" {keywords[i]}" : $"{keywords[i]}");
                // }

                return sb.ToString();
            }
            return "<no keywords>";
        }
        
        public static ShaderVariantCollection.ShaderVariant[] GetShaderVariants(ShaderVariantCollection shaderVariantCollection)
        {
            List<ShaderVariantCollection.ShaderVariant> tmpList = new();
            
            SerializedObject serializedObject = new SerializedObject(shaderVariantCollection);
            SerializedProperty shaderProperty = serializedObject.FindProperty("m_Shaders");
            for (int i = 0; i < shaderProperty.arraySize; i++)
            {
                SerializedProperty sp = shaderProperty.GetArrayElementAtIndex(i);
                SerializedProperty first = sp.FindPropertyRelative("first");
                SerializedProperty second = sp.FindPropertyRelative("second"); //ShaderInfo

                Shader shader = first.objectReferenceValue as Shader;
                if (shader == null) continue;

                SerializedProperty variants = second.FindPropertyRelative("variants");
                for (var vi = 0; vi < variants.arraySize; ++vi)
                {
                    SerializedProperty variantInfo = variants.GetArrayElementAtIndex(vi);
                    ShaderVariantCollection.ShaderVariant variant = PropToVariantObject(shader, variantInfo);
                    tmpList.Add(variant);
                }
            }

            return tmpList.ToArray();
        }
        
        //将SerializedProperty转化为ShaderVariant
        public static ShaderVariantCollection.ShaderVariant PropToVariantObject(Shader shader, SerializedProperty variantInfo)
        {
            PassType passType = (PassType)variantInfo.FindPropertyRelative("passType").intValue;
            string keywords = variantInfo.FindPropertyRelative("keywords").stringValue;
            string[] keywordSet = keywords.Split(' ');
            keywordSet = (keywordSet.Length == 1 && keywordSet[0] == "") ? new string[0] : keywordSet;

            ShaderVariantCollection.ShaderVariant newVariant = new ShaderVariantCollection.ShaderVariant()
            {
                shader = shader,
                keywords = keywordSet,
                passType = passType
            };

            return newVariant;
        }
    }
}