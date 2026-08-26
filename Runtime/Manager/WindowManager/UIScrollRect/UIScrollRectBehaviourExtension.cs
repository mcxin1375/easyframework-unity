using UnityEngine;

namespace EasyFramework
{
    public static class UIScrollRectBehaviourExtension
    {
        public static Vector2 CalculateDynamicItemSize(this UIScrollRectBehaviour srBehaviour, int itemIndex)
        {
            if (srBehaviour.GetItemSizeDelegate != null) return srBehaviour.GetItemSizeDelegate(itemIndex);
            return srBehaviour.Grid.CellSize;
        }
        public static Vector2 CalculateDynamicItemPosition(this UIScrollRectBehaviour srBehaviour, int itemIndex)
        {
            if (srBehaviour.GetItemPositionDelegate != null) return srBehaviour.GetItemPositionDelegate(itemIndex);
            return Vector2.zero;
        }
    }
}