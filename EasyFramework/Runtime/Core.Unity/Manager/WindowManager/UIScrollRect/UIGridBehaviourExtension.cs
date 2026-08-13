using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EasyFramework
{
    public static class UIGridBehaviourExtension
    {
        
        public static void CalculateDynamicItemDisplayRange(this UIGridBehaviour uiGridBehaviour, int itemCount, out int startIndex, out int endIndex)
        {
            startIndex = 0;
            endIndex = -1;

            var content = uiGridBehaviour.rectTransform;
            var viewport = content.parent as RectTransform;
            if (viewport == null)
            {
                FDebug.LogError("UIGridLayoutGroupEx.CalculateItemDisplayRange content parent get rectTransform failed!");
                return;
            }

            var padding = uiGridBehaviour.padding;

            var direction = uiGridBehaviour.startAxis == GridLayoutGroup.Axis.Horizontal ? GridLayoutGroup.Axis.Vertical : GridLayoutGroup.Axis.Horizontal;
            switch (direction)
            {
                case GridLayoutGroup.Axis.Horizontal:

                    var gridX = Mathf.Min(0, content.anchoredPosition.x);
                    var displayLeftX = -gridX;
                    var displayRightX = -gridX + viewport.rect.width;
                    
                    // Debug.Log($"{displayLeftX}, {displayRightX}");
                    
                    for (int i = 0; i < itemCount; i++)
                    {
                        var itemPos = uiGridBehaviour.GetItemPos(i);
                        var itemSize = uiGridBehaviour.GetItemSize(i);
                    
                        if (endIndex == -1)
                        {
                            if (itemPos.x + itemSize.x / 2 > displayLeftX)
                            {
                                startIndex = i;
                                endIndex = i;
                            }
                        }
                        else
                        {
                            // Debug.Log($"{i}, {itemPos.x - itemSize.x / 2}, {displayRightX}");
                            if (itemPos.x - itemSize.x / 2 > displayRightX) break;
                            endIndex = i;
                        }
                    }
                    
                    break;
                case GridLayoutGroup.Axis.Vertical:
                    
                    var gridY = Mathf.Max(0, content.anchoredPosition.y);
                    var displayTopY = -gridY;
                    var displayBottomY = -gridY - viewport.rect.height;
                    
                    // Debug.Log($"{displayTopY}, {displayBottomY}");

                    for (int i = 0; i < itemCount; i++)
                    {
                        var itemPos = uiGridBehaviour.GetItemPos(i);
                        var itemSize = uiGridBehaviour.GetItemSize(i);

                        if (endIndex == -1)
                        {
                            if (itemPos.y - itemSize.y / 2 < displayTopY)
                            {
                                startIndex = i;
                                endIndex = i;
                            }
                        }
                        else
                        {
                            if (itemPos.y + itemSize.y / 2 < displayBottomY) break;
                            endIndex = i;
                        }
                    }
                    break;
            }
        }
        // public static void CalculateDynamicItemDisplayRange(this UIGridLayoutGroup uiGridLayoutGroup, out int startIndex, out int endIndex)
        // {
        //     startIndex = 0;
        //     endIndex = -1;
        //
        //     var content = uiGridLayoutGroup.rectTransform;
        //     var viewport = content.parent as RectTransform;
        //     if (viewport == null)
        //     {
        //         Debug.LogWarning("UIGridLayoutGroupEx.CalculateItemDisplayRange: 父容器RectTransform为空");
        //         return;
        //     }
        //     
        //     // ====== 1. 计算viewport在content局部空间下的矩形范围 ======
        //     Vector3[] worldCorners = new Vector3[4];
        //     viewport.GetWorldCorners(worldCorners);
        //
        //     // 把viewport的世界坐标转到content的局部坐标
        //     Vector3 min = content.InverseTransformPoint(worldCorners[0]); // 左下角
        //     Vector3 max = content.InverseTransformPoint(worldCorners[2]); // 右上角
        //     Rect viewRect = new Rect(min, max - min);
        //
        //     // ====== 2. 遍历posList，计算可见索引 ======
        //     for (int i = 0; i < uiGridLayoutGroup.ItemPosList.Count; i++)
        //     {
        //         Vector2 center = uiGridLayoutGroup.ItemPosList[i];
        //         Vector2 size = uiGridLayoutGroup.GetItemSize(i);
        //
        //         // item 的局部 Rect
        //         Rect itemRect = new Rect(
        //             center.x - size.x / 2f,
        //             center.y - size.y / 2f,
        //             size.x,
        //             size.y
        //         );
        //
        //         if (itemRect.Overlaps(viewRect))
        //         {
        //             if (endIndex == -1) // 第一次命中
        //                 startIndex = i;
        //             endIndex = i;
        //         }
        //     }
        // }
        
        public static void CalculateDynamicItemSize(this UIGridBehaviour uiGridBehaviour, List<Vector2> sizeList, int itemCount)
        {
            if (!uiGridBehaviour.SRBehaviour.HasSizeDelegate) return;
            for (int i = 0; i < itemCount; i++)
            {
                var itemSize = uiGridBehaviour.SRBehaviour.CalculateDynamicItemSize(i);
                sizeList.Add(itemSize);
            }
        }

        public static void CalculateDynamicItemPosition(this UIGridBehaviour uiGridBehaviour, List<Vector2> posList, int itemNumber, out Vector2 sizeDelta)
        {
            sizeDelta = Vector2.zero;

            var gridRect = uiGridBehaviour.rectTransform;
            var parentRect = gridRect.parent as RectTransform;
            if (parentRect == null)
            {
                FDebug.LogError("CalculateDynamicItemPosition content parent get rectTransform failed!");
                return;
            }

            if (itemNumber <= 0) return;

            float parentWidth = parentRect.rect.width;
            float parentHeight = parentRect.rect.height;

            if (uiGridBehaviour.SRBehaviour.HasPositionDelegate)
            {
                float width = parentWidth, height = parentHeight;
                
                for (int i = 0; i < itemNumber; i++)
                {
                    var itemPos = uiGridBehaviour.SRBehaviour.CalculateDynamicItemPosition(i);
                    posList.Add(itemPos);
                    
                    var itemSize = uiGridBehaviour.GetItemSize(i);

                    if (uiGridBehaviour.startAxis == GridLayoutGroup.Axis.Horizontal)
                    {
                        var y = itemSize.y / 2 - itemPos.y;
                        if (y > height) height = y;
                    }
                    else
                    {
                        var x = itemSize.x / 2 + itemPos.x;
                        if (x > width) width = x;
                    }
                }
                sizeDelta = new Vector2(width, height);
                
                return;
            }

            var padding = uiGridBehaviour.padding;
            var spacing = uiGridBehaviour.spacing;

            float contentWidth = 0f;
            float contentHeight = 0f;

            // -------------------------
            // 根据排列方向分别处理
            // -------------------------
            if (uiGridBehaviour.startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                // 垂直优先：从上到下，换行向右
                float x = padding.left;
                float y = -padding.top;
                float rowMaxHeight = 0f;

                for (int i = 0; i < itemNumber; i++)
                {
                    var itemSize = uiGridBehaviour.GetItemSize(i);

                    // 判断是否需要换行
                    if (x + itemSize.x + padding.right > parentWidth && x > padding.left)
                    {
                        // 换行
                        x = padding.left;
                        y -= (rowMaxHeight + spacing.y);
                        contentHeight += rowMaxHeight + spacing.y;
                        rowMaxHeight = 0f;
                    }

                    // 添加位置（以中心点为准）
                    float centerX = x + itemSize.x / 2f;
                    float centerY = y - itemSize.y / 2f;
                    posList.Add(new Vector2(centerX, centerY));

                    x += itemSize.x + spacing.x;
                    rowMaxHeight = Mathf.Max(rowMaxHeight, itemSize.y);

                    contentWidth = Mathf.Max(contentWidth, x + padding.right);
                }

                contentHeight += rowMaxHeight + padding.top + padding.bottom;

                sizeDelta.x = parentWidth;
                sizeDelta.y = Mathf.Max(contentHeight, parentHeight);
            }
            else // Horizontal
            {
                // 水平优先：从左到右，换列向下
                float x = padding.left;
                float y = -padding.top;
                float colMaxWidth = 0f;

                for (int i = 0; i < itemNumber; i++)
                {
                    Vector2 itemSize = uiGridBehaviour.GetItemSize(i);

                    // 判断是否需要换列
                    if (y - itemSize.y - padding.bottom < -parentHeight && y < -padding.top)
                    {
                        // 换列
                        y = -padding.top;
                        x += (colMaxWidth + spacing.x);
                        contentWidth += colMaxWidth + spacing.x;
                        colMaxWidth = 0f;
                    }

                    // 添加位置
                    float centerX = x + itemSize.x / 2f;
                    float centerY = y - itemSize.y / 2f;
                    posList.Add(new Vector2(centerX, centerY));

                    y -= itemSize.y + spacing.y;
                    colMaxWidth = Mathf.Max(colMaxWidth, itemSize.x);

                    contentHeight = Mathf.Max(contentHeight, Mathf.Abs(y) + padding.bottom);
                }

                contentWidth += colMaxWidth + padding.left + padding.right;

                sizeDelta.x = Mathf.Max(contentWidth, parentWidth);
                sizeDelta.y = parentHeight;
            }

            // -------------------------
            // TODO: 处理 childAlignment / startCorner
            // -------------------------
            // 这里只是实现了 "左上角起点" 的版本，如果需要支持 UpperRight/LowerLeft/LowerRight，
            // 可以在 posList 计算完成后，统一做一次坐标翻转。
        }

        // public static void CalculateDynamicItemPosition(this UIGridLayoutGroup uiGridLayoutGroup, int itemNumber, out Vector2 sizeDelta)
        // {
        //     uiGridLayoutGroup.ItemPosList.Clear();
        //     sizeDelta = Vector2.zero;
        //
        //     var gridRect = uiGridLayoutGroup.rectTransform;
        //     var parentRect = gridRect.parent as RectTransform;
        //     if (parentRect == null)
        //     {
        //         Debug.LogWarning("CalculateDynamicItemPosition: 父容器RectTransform为空");
        //         return;
        //     }
        //
        //     if (itemNumber <= 0) return;
        //     
        //     var padding = uiGridLayoutGroup.padding;
        //     var spacing = uiGridLayoutGroup.spacing;
        //
        //     float parentWidth = parentRect.rect.width;
        //     float parentHeight = parentRect.rect.height;
        //
        //     // 当前排列状态
        //     float x = padding.left;
        //     float y = -padding.top;
        //     float rowMaxHeight = 0f; // 当前行最大高度
        //     float contentWidth = 0f;
        //     float contentHeight = 0f;
        //
        //     for (int i = 0; i < itemNumber; i++)
        //     {
        //         Vector2 itemSize = uiGridLayoutGroup.GetItemSize(i);
        //
        //         // 判断当前行是否还能放下该元素（超出宽度则换行）
        //         if (x + itemSize.x + padding.right > parentWidth && x > padding.left)
        //         {
        //             // 换行
        //             x = padding.left;
        //             y -= (rowMaxHeight + spacing.y);
        //             contentHeight += rowMaxHeight + spacing.y;
        //             rowMaxHeight = 0f;
        //         }
        //
        //         // 计算该 item 的位置（以中心点为基准）
        //         float centerX = x + itemSize.x / 2f;
        //         float centerY = y - itemSize.y / 2f;
        //         uiGridLayoutGroup.ItemPosList.Add(new Vector2(centerX, centerY));
        //
        //         // 更新行状态
        //         x += itemSize.x + spacing.x;
        //         rowMaxHeight = Mathf.Max(rowMaxHeight, itemSize.y);
        //
        //         // 更新内容宽度
        //         contentWidth = Mathf.Max(contentWidth, x + padding.right);
        //     }
        //
        //     // 最后一行高度加入总内容高度
        //     contentHeight += rowMaxHeight + padding.bottom;
        //
        //     var direction = uiGridLayoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal ? GridLayoutGroup.Axis.Vertical : GridLayoutGroup.Axis.Horizontal;
        //     if (direction == GridLayoutGroup.Axis.Horizontal)
        //     {
        //         // 水平滚动：宽度取内容宽与父宽的最大值，高度强制与父高一致
        //         sizeDelta.x = Mathf.Max(contentWidth, parentWidth);
        //         sizeDelta.y = parentHeight;
        //     }
        //     else // Vertical
        //     {
        //         // 垂直滚动：高度取内容高与父高的最大值，宽度强制与父宽一致
        //         sizeDelta.y = Mathf.Max(contentHeight, parentHeight);
        //         sizeDelta.x = parentWidth;
        //     }
        // }

        public static void CalculateFixedItemDisplayRange(this UIGridBehaviour uiGridBehaviour, int itemCount, out int startIndex, out int endIndex)
        {
            startIndex = 0;
            endIndex = -1;

            var content = uiGridBehaviour.rectTransform;
            var viewport = content.parent as RectTransform;
            if (viewport == null)
            {
                FDebug.LogError("CalculateItemDisplayRange content parent get rectTransform failed!");
                return;
            }

            var padding = uiGridBehaviour.padding;
            var cellSize = uiGridBehaviour.CellSize;
            var spacing = uiGridBehaviour.spacing;

            float tileW = cellSize.x + spacing.x; // 每列总宽度（单元格+列间距）
            float tileH = cellSize.y + spacing.y;   // 每行总高度（单元格+行间距）
            
            int rows    = Mathf.Max(1, uiGridBehaviour.rows);
            int columns = Mathf.Max(1, uiGridBehaviour.columns);

            var direction = uiGridBehaviour.startAxis == GridLayoutGroup.Axis.Horizontal ? GridLayoutGroup.Axis.Vertical : GridLayoutGroup.Axis.Horizontal;
            
            // Debug.Log($"{viewport.name}, {direction}, {rows}, {columns}");
            
            switch (direction)
            {
                case GridLayoutGroup.Axis.Horizontal:

                    var gridX = Mathf.Min(0, content.anchoredPosition.x);
                    var displayLeftX = gridX + padding.left;
                    var displayRightX = gridX - viewport.rect.width + padding.left;
                    
                    int leftCol = Mathf.FloorToInt(Mathf.Abs(displayLeftX) / tileW );
                    int rightCol = Mathf.CeilToInt(Mathf.Abs(displayRightX) / tileW );
                    
                    startIndex = leftCol * rows;
                    endIndex = rightCol * rows - 1;
                    
                    // Debug.Log($"{displayLeftX}, {displayRightX}, {leftCol}, {rightCol}");
                    
                    break;
                case GridLayoutGroup.Axis.Vertical:
                    
                    var gridY = Mathf.Max(0, content.anchoredPosition.y);
                    var displayTopY = gridY - padding.top;
                    var displayBottomY = gridY + viewport.rect.height - padding.top;
                    
                    int topRow = Mathf.FloorToInt(Mathf.Abs(displayTopY) / tileH);
                    int bottomRow = Mathf.CeilToInt(Mathf.Abs(displayBottomY) / tileH);
                    
                    startIndex = topRow * columns;
                    endIndex = bottomRow * columns - 1;
                    
                    break;
            }
            endIndex = Mathf.Min(itemCount - 1, endIndex);
        }
        
        public static void CalculateFixedItemPosition(this UIGridBehaviour uiGridBehaviour, List<Vector2> posList, int itemNumber, out Vector2 sizeDelta, out int rows, out int columns)
        {
            rows = 0;
            columns = 0;
            posList.Clear();
            sizeDelta = Vector2.zero;
            
            var gridRect = uiGridBehaviour.rectTransform;
            var parentRect = gridRect.parent as RectTransform;
            if (parentRect == null)
            {
                FDebug.LogError("CalculateItemPosition content parent get rectTransform failed!");
                return;
            }

            if (itemNumber <= 0) return;
            
            // 获取布局参数
            var padding = uiGridBehaviour.padding;
            var cellSize = uiGridBehaviour.CellSize;
            var spacing = uiGridBehaviour.spacing;
            var startCorner = uiGridBehaviour.startCorner;
            var startAxis = uiGridBehaviour.startAxis;
            var childAlignment = uiGridBehaviour.childAlignment;
            var direction = uiGridBehaviour.startAxis == GridLayoutGroup.Axis.Horizontal ? GridLayoutGroup.Axis.Vertical : GridLayoutGroup.Axis.Horizontal;

            // 父容器原始尺寸（未扣除内边距）
            float parentWidth = parentRect.rect.width;
            float parentHeight = parentRect.rect.height;

            // 计算行列数量(已扣除内边距)
            CalculateFixedGridDimensions(startAxis, parentWidth - (padding.left + padding.right), parentHeight - (padding.top + padding.bottom), cellSize, spacing, itemNumber, out rows, out columns);

            // 计算单个单元格总宽度（尺寸+间距）
            float cellTotalWidth = cellSize.x + spacing.x;
            float cellTotalHeight = cellSize.y + spacing.y;

            // 计算内容总尺寸（含内边距）
            float contentWidth = padding.left + (columns > 0 ? columns * cellSize.x + (columns - 1) * spacing.x : 0) + padding.right;
            float contentHeight = padding.top + (rows > 0 ? rows * cellSize.y + (rows - 1) * spacing.y : 0) + padding.bottom;

            // 根据滚动类型设置sizeDelta
            if (direction == GridLayoutGroup.Axis.Horizontal)
            {
                // 水平滚动：宽度取内容宽与父宽的最大值，高度强制与父高一致
                sizeDelta.x = Mathf.Max(contentWidth, parentWidth);
                sizeDelta.y = parentHeight;
            }
            else // Vertical
            {
                // 垂直滚动：高度取内容高与父高的最大值，宽度强制与父宽一致
                sizeDelta.y = Mathf.Max(contentHeight, parentHeight);
                sizeDelta.x = parentWidth;
            }

            // 计算基于对齐方式的内容偏移
            float contentOffsetX = CalculateContentOffsetX(childAlignment, parentWidth, contentWidth - padding.left - padding.right);
            float contentOffsetY = CalculateContentOffsetY(childAlignment, parentHeight, contentHeight - padding.top - padding.bottom);

            // 计算起始位置（基于左上角的相对位置）
            Vector2 startPosition = new Vector2(
                padding.left + contentOffsetX,
                -padding.top - contentOffsetY // Y轴向上为负
            );

            // 计算每个Item的位置
            for (int i = posList.Count; i < itemNumber; i++)
            {
                // 计算原始行列索引
                int row, col;
                CalculateFixedRowColumn(i, startAxis, rows, columns, out row, out col);

                // 根据起始角落修正列/行索引
                int adjustedCol = col;
                int adjustedRow = row;

                switch (startCorner)
                {
                    case GridLayoutGroup.Corner.UpperRight:
                        adjustedCol = columns - 1 - col;
                        break;
                    case GridLayoutGroup.Corner.LowerLeft:
                        adjustedRow = rows - 1 - row;
                        break;
                    case GridLayoutGroup.Corner.LowerRight:
                        adjustedCol = columns - 1 - col;
                        adjustedRow = rows - 1 - row;
                        break;
                }

                // 计算单元格左上角坐标
                float cellX = startPosition.x + adjustedCol * cellTotalWidth;
                float cellY = startPosition.y - adjustedRow * cellTotalHeight;

                // 偏移到单元格中心点（加上自身尺寸的一半）
                float centerX = cellX + cellSize.x / 2f;
                float centerY = cellY - cellSize.y / 2f;

                posList.Add(new Vector2(centerX, centerY));
            }
        }

        // 其他方法保持不变...
        private static float CalculateContentOffsetX(TextAnchor alignment, float parentWidth, float contentWidth)
        {
            switch (alignment)
            {
                case TextAnchor.UpperLeft:
                case TextAnchor.MiddleLeft:
                case TextAnchor.LowerLeft:
                    return 0;
                case TextAnchor.UpperCenter:
                case TextAnchor.MiddleCenter:
                case TextAnchor.LowerCenter:
                    return (parentWidth - contentWidth) / 2f;
                case TextAnchor.UpperRight:
                case TextAnchor.MiddleRight:
                case TextAnchor.LowerRight:
                    return parentWidth - contentWidth;
                default:
                    return 0;
            }
        }

        private static float CalculateContentOffsetY(TextAnchor alignment, float parentHeight, float contentHeight)
        {
            switch (alignment)
            {
                case TextAnchor.UpperLeft:
                case TextAnchor.UpperCenter:
                case TextAnchor.UpperRight:
                    return 0;
                case TextAnchor.MiddleLeft:
                case TextAnchor.MiddleCenter:
                case TextAnchor.MiddleRight:
                    return (parentHeight - contentHeight) / 2f;
                case TextAnchor.LowerLeft:
                case TextAnchor.LowerCenter:
                case TextAnchor.LowerRight:
                    return parentHeight - contentHeight;
                default:
                    return 0;
            }
        }

        private static void CalculateFixedGridDimensions(GridLayoutGroup.Axis startAxis, float availableWidth, float availableHeight,
                                                  Vector2 cellSize, Vector2 spacing, int itemCount, out int rows, out int columns)
        {
            if (startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                // 正确计算列数：额外 +spacing.x 避免少算一列
                columns = Mathf.Max(1, Mathf.FloorToInt((availableWidth + spacing.x) / (cellSize.x + spacing.x)));
                rows = Mathf.CeilToInt((float)itemCount / columns);
            }
            else
            {
                // 正确计算行数：额外 +spacing.y 避免少算一行
                rows = Mathf.Max(1, Mathf.FloorToInt((availableHeight + spacing.y) / (cellSize.y + spacing.y)));
                columns = Mathf.CeilToInt((float)itemCount / rows);
            }
        }

        private static void CalculateFixedRowColumn(int index, GridLayoutGroup.Axis startAxis, int rows, int columns, out int row, out int column)
        {
            if (startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                row = index / columns;
                column = index % columns;
            }
            else
            {
                row = index % rows;
                column = index / rows;
            }
        }
    }
}