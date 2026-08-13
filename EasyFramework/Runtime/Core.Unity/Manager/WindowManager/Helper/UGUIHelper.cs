/*----------------------------------------------------------------
// author:meng cheng xin
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EasyFramework
{
    /// <summary>
    /// UGUI 工具类
    /// </summary>
    public static class UGUIHelper
    {
        /// <summary>
        /// 世界坐标 转 UGUI坐标
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        // public static Vector3 WorldPointToUGUIPoint(Camera worldCamera, Canvas canvas, Vector3 worldPosition)
        // {
        //     Vector3 v3 = worldCamera.WorldToScreenPoint(worldPosition);
        //     return ScreenPointToUGUIPoint(canvas, v3);
        // }

        /// <summary>
        /// 屏幕坐标 转 UGUI坐标
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="screenPosition"></param>
        /// <returns></returns>
        // public static Vector3 ScreenPointToUGUIPoint(Canvas canvas, Vector3 screenPosition)
        // {
        //     if (canvas.worldCamera.orthographic)
        //     {
        //         Vector3 v_ui = canvas.worldCamera.ScreenToWorldPoint(screenPosition);
        //         Vector3 v_new = new Vector3(v_ui.x, v_ui.y, canvas.GetComponent<RectTransform>().anchoredPosition3D.z);
        //         return v_new;
        //     }
        //     else
        //     {
        //         screenPosition.z = canvas.planeDistance;
        //         Vector3 v_ui = canvas.worldCamera.ScreenToWorldPoint(screenPosition);
        //         //Vector3 v_new = new Vector3(v_ui.x, v_ui.y, canvas.GetComponent<RectTransform>().anchoredPosition3D.z);
        //         return v_ui;
        //     }
        // }

        public static bool IsPointerOverUIObject(Vector2 screenPoint)
        {
            return RaycastUIObject(screenPoint, out var gameObject);
        }

        public static bool RaycastUIObject(Vector2 screenPos, out GameObject gameObject)
        {
            RaycastUIObjects(screenPos, out var results);
            gameObject = results.Count > 0 ? results[0].gameObject : null;
            return gameObject != null;
        }

        public static void RaycastUIObjects(Vector2 screenPos, out List<RaycastResult> results)
        {
            results = new();
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (eventSystem == null) return;

            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = screenPos;
            eventSystem.RaycastAll(pointerEventData, results);
        }

        public static Vector2 CalculateGridSizeDelta(RectTransform rectTransform, int itemNum)
        {
            GridLayoutGroup gridLayoutGroup = rectTransform.GetComponent<GridLayoutGroup>();
            
            int lineNum = Mathf.FloorToInt(itemNum / gridLayoutGroup.constraintCount);

            if (itemNum % gridLayoutGroup.constraintCount > 0)
                lineNum = lineNum + 1;

            if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {
                float height = gridLayoutGroup.cellSize.y * lineNum + gridLayoutGroup.padding.top;
                if (lineNum > 0)
                    height = height + gridLayoutGroup.spacing.y * (lineNum - 1);

                float width = gridLayoutGroup.padding.left + gridLayoutGroup.padding.right
                    + (gridLayoutGroup.constraintCount - 1) * gridLayoutGroup.spacing.x
                    + gridLayoutGroup.constraintCount * gridLayoutGroup.cellSize.x;

                return new Vector2(width, height);
            }
            else
            {
                float width = gridLayoutGroup.cellSize.x * lineNum + gridLayoutGroup.padding.left;
                if (lineNum > 0)
                {
                    width = width + gridLayoutGroup.spacing.x * (lineNum - 1);
                }

                float height = gridLayoutGroup.padding.top + gridLayoutGroup.padding.bottom
                    + (gridLayoutGroup.constraintCount - 1) * gridLayoutGroup.spacing.y
                    + gridLayoutGroup.constraintCount * gridLayoutGroup.cellSize.y;

                return new Vector2(width, height);
            }
        }
        
        /// <summary>
        /// 根据itemNumber 返回对应数量的位置信息
        /// </summary>
        /// <param name="gridRectTransform"></param>
        /// <param name="itemNumber"></param>
        /// <returns></returns>
        public static Vector2[] GetGridItemPosArr(RectTransform gridRectTransform, int itemNumber)
        {
            if (ReferenceEquals(gridRectTransform, null))
            {
                return null;
            }

            GridLayoutGroup layoutGroup = gridRectTransform.GetComponent<GridLayoutGroup>();
            if (ReferenceEquals(layoutGroup, null))
            {
                return null;
            }

            Vector2[] positionArr = new Vector2[itemNumber];

            float width = gridRectTransform.rect.size.x;
            float height = gridRectTransform.rect.size.y;

            int cellCountX = 1;
            int cellCountY = 1;

            if (layoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {
                cellCountX = layoutGroup.constraintCount;

                if (itemNumber > cellCountX)
                    cellCountY = itemNumber / cellCountX + (itemNumber % cellCountX > 0 ? 1 : 0);
            }
            else if (layoutGroup.constraint == GridLayoutGroup.Constraint.FixedRowCount)
            {
                cellCountY = layoutGroup.constraintCount;

                if (itemNumber > cellCountY)
                    cellCountX = itemNumber / cellCountY + (itemNumber % cellCountY > 0 ? 1 : 0);
            }
            else
            {
                if (layoutGroup.cellSize.x + layoutGroup.spacing.x <= 0)
                    cellCountX = int.MaxValue;
                else

                    cellCountX = Mathf.Max(1,
                        Mathf.FloorToInt(
                            (width - layoutGroup.padding.horizontal + layoutGroup.spacing.x + 0.001f) /
                            (layoutGroup.cellSize.x + layoutGroup.spacing.x)));

                if (layoutGroup.cellSize.y + layoutGroup.spacing.y <= 0)
                    cellCountY = int.MaxValue;
                else
                    cellCountY = Mathf.Max(1,
                        Mathf.FloorToInt(
                            (height - layoutGroup.padding.vertical + layoutGroup.spacing.y + 0.001f) /
                            (layoutGroup.cellSize.y + layoutGroup.spacing.y)));
            }


            int cornerX = (int)layoutGroup.startCorner % 2;
            int cornerY = (int)layoutGroup.startCorner / 2;

            int cellsPerMainAxis, actualCellCountX, actualCellCountY;
            if (layoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                cellsPerMainAxis = cellCountX;
                actualCellCountX = Mathf.Clamp(cellCountX, 1, itemNumber);
                actualCellCountY =
                    Mathf.Clamp(cellCountY, 1, Mathf.CeilToInt(itemNumber / (float)cellsPerMainAxis));
            }
            else
            {
                cellsPerMainAxis = cellCountY;
                actualCellCountY = Mathf.Clamp(cellCountY, 1, itemNumber);
                actualCellCountX =
                    Mathf.Clamp(cellCountX, 1, Mathf.CeilToInt(itemNumber / (float)cellsPerMainAxis));
            }

            Vector2 requiredSpace = new Vector2(
                actualCellCountX * layoutGroup.cellSize.x + (actualCellCountX - 1) * layoutGroup.spacing.x,
                actualCellCountY * layoutGroup.cellSize.y + (actualCellCountY - 1) * layoutGroup.spacing.y
            );
            Vector2 startOffset = new Vector2(
                GetStartOffset(0, requiredSpace.x),
                GetStartOffset(1, requiredSpace.y)
            );

            for (int i = 0; i < itemNumber; i++)
            {
                int positionX;
                int positionY;
                if (layoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal)
                {
                    positionX = i % cellsPerMainAxis;
                    positionY = i / cellsPerMainAxis;
                }
                else
                {
                    positionX = i / cellsPerMainAxis;
                    positionY = i % cellsPerMainAxis;
                }

                if (cornerX == 1)
                    positionX = actualCellCountX - 1 - positionX;
                if (cornerY == 1)
                    positionY = actualCellCountY - 1 - positionY;
                var posX = startOffset.x + (layoutGroup.cellSize[0] + layoutGroup.spacing[0]) * positionX;
                var sizeX = layoutGroup.cellSize[0];
                var posY = startOffset.y + (layoutGroup.cellSize[1] + layoutGroup.spacing[1]) * positionY;
                var sizeY = layoutGroup.cellSize[1];
                var x = posX + sizeX * 0.5f;
                var y = -posY - sizeY * (1f - 0.5f);
                var anchoredPosition = new Vector2(x, y);
                positionArr[i] = anchoredPosition;
            }

            return positionArr;
            
            
            #region Local Method

            float GetStartOffset(int axis, float requiredSpaceWithoutPadding)
            {
                float requiredSpaceTmp = requiredSpaceWithoutPadding +
                                      (axis == 0 ? layoutGroup.padding.horizontal : layoutGroup.padding.vertical);
                float availableSpace = gridRectTransform.rect.size[axis];
                float surplusSpace = availableSpace - requiredSpaceTmp;
                float alignmentOnAxis = GetAlignmentOnAxis(axis);
                return (axis == 0 ? layoutGroup.padding.left : layoutGroup.padding.top) +
                       surplusSpace * alignmentOnAxis;
            }

            float GetAlignmentOnAxis(int axis)
            {
                if (axis == 0)
                    return (int)layoutGroup.childAlignment % 3 * 0.5f;
                else
                    return (int)layoutGroup.childAlignment / 3 * 0.5f;
            }
            #endregion
        }
    }
}