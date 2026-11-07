/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/8/15
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    [CustomEditor(typeof(UIScrollRectBehaviour))]
    public class UIScrollRectBehaviourInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var behaviour = target as UIScrollRectBehaviour;
            behaviour.RefreshOnEditorMode();
        }
    }
}