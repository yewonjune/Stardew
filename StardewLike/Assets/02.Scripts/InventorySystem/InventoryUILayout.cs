using UnityEngine;
using UnityEngine.UI;

public class InventoryUILayout : MonoBehaviour
{
    public RectTransform target;
    public RectTransform presetShop;    // 상점용 위치/사이즈 프리셋

    [Header("Grid 축소 옵션 (상점 모드에서 적용)")]
    public GridLayoutGroup grid;        // 슬롯 그리드(없으면 자동 탐색)
    public Vector2 shopCellSize = new Vector2(64, 64);  // 상점에서 적용할 크기
    public Vector2 shopSpacing = new Vector2(4, 4);    // 상점에서 적용할 간격

    // 원래 값 백업
    Vector2 originalAnchorMin, originalAnchorMax, originalPivot, originalPos, originalSize;
    Vector2 originalCellSize, originalSpacing;
    Vector3 originalLocalScale;

    void Awake()
    {
        if (!grid) grid = target ? target.GetComponentInChildren<GridLayoutGroup>(true) : null;

        originalAnchorMin = target.anchorMin;
        originalAnchorMax = target.anchorMax;
        originalPivot = target.pivot;
        originalPos = target.anchoredPosition;
        originalSize = target.sizeDelta;
        originalLocalScale = target.localScale;

        if (grid)
        {
            originalCellSize = grid.cellSize;
            originalSpacing = grid.spacing;
        }
    }

    public void ApplyShop()
    {
        if (presetShop) CopyNow(presetShop, target);

        // 슬롯 사이즈/간격을 상점 전용 값으로 적용
        if (grid)
        {
            grid.cellSize = shopCellSize;
            grid.spacing = shopSpacing;
        }
    }

    public void ApplyOriginal()
    {
        target.anchorMin = originalAnchorMin;
        target.anchorMax = originalAnchorMax;
        target.pivot = originalPivot;
        target.anchoredPosition = originalPos;
        target.sizeDelta = originalSize;
        target.localScale = originalLocalScale;

        if (grid)
        {
            grid.cellSize = originalCellSize;
            grid.spacing = originalSpacing;
        }
    }

    static void CopyNow(RectTransform from, RectTransform to)
    {
        to.anchorMin = from.anchorMin;
        to.anchorMax = from.anchorMax;
        to.pivot = from.pivot;
        to.anchoredPosition = from.anchoredPosition;
        to.sizeDelta = from.sizeDelta;
    }
}
