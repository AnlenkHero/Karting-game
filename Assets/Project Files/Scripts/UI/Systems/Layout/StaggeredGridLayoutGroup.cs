using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Systems.Layout
{
    [AddComponentMenu("Layout/Staggered Grid Layout Group")]
    public class StaggeredGridLayoutGroup : LayoutGroup
    {
        [Header("Grid Settings")]
        [Tooltip("Number of columns in the grid.")]
        public int columnCount = 2;

        [Header("Cell Settings")]
        [Tooltip("Width and height of each cell.")]
        public Vector2 cellSize = new (100f, 50f);

        [Header("Spacing Settings")]
        [Tooltip("Spacing between cells (horizontal and vertical).")]
        public Vector2 spacing = new (0f, 50f);

        [Header("Stagger Settings")]
        [Tooltip("If true, odd columns will be offset vertically.")]
        public bool staggered = true;
        [Tooltip("Vertical offset added to odd columns.")]
        public float staggeredOffset = 50f;

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            int columns = Mathf.Max(1, columnCount);
            float totalWidth = padding.left + padding.right + columns * cellSize.x + (columns - 1) * spacing.x;
            SetLayoutInputForAxis(totalWidth, totalWidth, -1, 0);
        }

        public override void CalculateLayoutInputVertical()
        {
            float totalHeight = padding.top + padding.bottom + CalculateTotalHeight();
            SetLayoutInputForAxis(totalHeight, totalHeight, -1, 1);
        }

        public override void SetLayoutHorizontal()
        {
            int columns = Mathf.Max(1, columnCount);
            float totalWidth = columns * cellSize.x + (columns - 1) * spacing.x;
            float startX = GetStartOffset(0, totalWidth);
        
            for (int i = 0; i < rectChildren.Count; i++)
            {
                int column = i % columns;
                float posX = startX + column * (cellSize.x + spacing.x);
                SetChildAlongAxis(rectChildren[i], 0, posX, cellSize.x);
            }
        }

        public override void SetLayoutVertical()
        {
            int columns = Mathf.Max(1, columnCount);
            float totalHeight = CalculateTotalHeight();
            float startY = GetStartOffset(1, totalHeight);

            for (int i = 0; i < rectChildren.Count; i++)
            {
                int column = i % columns;
                int row = i / columns;
                float posY = startY + row * (cellSize.y + spacing.y);
                if (staggered && (column % 2 == 1))
                {
                    posY += staggeredOffset;
                }
                SetChildAlongAxis(rectChildren[i], 1, posY, cellSize.y);
            }
        }
        
        private float CalculateTotalHeight()
        {
            int columns = Mathf.Max(1, columnCount);
            int totalChildren = rectChildren.Count;
            float maxHeight = 0f;
            
            for (int col = 0; col < columns; col++)
            {
                int itemsInColumn = totalChildren / columns;
                if (col < totalChildren % columns)
                    itemsInColumn++;

                if (itemsInColumn > 0)
                {
                    float columnHeight = itemsInColumn * cellSize.y + (itemsInColumn - 1) * spacing.y;
                    if (staggered && (col % 2 == 1))
                    {
                        columnHeight += staggeredOffset;
                    }
                    maxHeight = Mathf.Max(maxHeight, columnHeight);
                }
            }
            return maxHeight;
        }
    }
}
