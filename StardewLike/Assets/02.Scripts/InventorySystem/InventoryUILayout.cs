using UnityEngine;

public class InventoryUILayout : MonoBehaviour
{
    public RectTransform target;
    public RectTransform presetShop;    // 상점용 위치/사이즈

    Vector2 originalAnchorMin, originalAnchorMax, originalPivot, originalPos, originalSize;

    void Awake()
    {
        originalAnchorMin = target.anchorMin;
        originalAnchorMax = target.anchorMax;
        originalPivot = target.pivot;
        originalPos = target.anchoredPosition;
        originalSize = target.sizeDelta;
    }

    public void ApplyShop()
    {
        if (!presetShop) return;
        CopyNow(presetShop, target);
    }

    public void ApplyOriginal()
    {
        target.anchorMin = originalAnchorMin;
        target.anchorMax = originalAnchorMax;
        target.pivot = originalPivot;
        target.anchoredPosition = originalPos;
        target.sizeDelta = originalSize;
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
