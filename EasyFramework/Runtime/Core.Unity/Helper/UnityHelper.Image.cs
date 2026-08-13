/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/23
// describe:
//----------------------------------------------------------------*/

using UnityEngine;
using UnityEngine.UI;

namespace EasyFramework
{
    public static partial class UnityHelper
    {
        public static void SetColorRGB(this Image image, Color color)
        {
            image.color = new Color(color.r, color.g, color.b, image.color.a);
        }
        
        public static void SetAlpha(this Image image, float alpha)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
        }
    }
}
