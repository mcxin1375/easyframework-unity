// using System.Collections.Generic;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// namespace EasyFramework.Editor
// {
//     public class ResDebugListViewEx : UIToolkitListViewEx<ResDebugWindow, string>
//     {
//         protected override string ViewName => "TabListView";
//
//         protected override VisualElement MakeItem()
//         {
//             var btn = new Button()
//             {
//                 style = { unityTextAlign = TextAnchor.MiddleLeft },
//             };
//             btn.clicked += () =>
//             {
//                 ListView.selectedIndex = int.Parse(btn.name);
//             };
//             return btn;
//             // return new Label()
//             // {
//             //     style = { unityTextAlign = TextAnchor.MiddleLeft },
//             // };
//         }
//         protected override void BindItem(VisualElement ve, int index)
//         {
//             var btn = ve as Button;
//             btn.name = index.ToString();
//             btn.text = Items[index];
//         }
//
//         protected override void OnSelectionChanged(IEnumerable<object> obj)
//         {
//             base.OnSelectionChanged(obj);
//
//             // Debug.Log(ListView.selectedIndex);
//             var s = Items[ListView.selectedIndex];
//             Window.SelectTab(s);
//         }
//     }
// }