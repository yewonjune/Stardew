using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PickupToastItemUI : MonoBehaviour
{
    public Image iconImage;
    public Text nameText;
    public Text countText;

    public float inDuration = 0.18f;
    public float holdDuration = 1.15f;
    public float outDuration = 0.25f;
    public float slideX = 28f;
    public float popScale = 1.06f;

    CanvasGroup cg;
    public RectTransform animTarget;

    int amount = 1;
    Sequence seq;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();

        if(cg) cg.alpha = 0f;
    }

    private void OnDisable()
    {
        seq?.Kill();
    }

    public void Setup(Sprite icon, string itemName, int startAmount)
    {
        if (iconImage) iconImage.sprite = icon;
        if(nameText) nameText.text = itemName;

        amount = Mathf.Max(1, startAmount);
        RefreshCount();

        PlaySequence(resetPos: true);
    }

    public void AddAmount(int add)
    {
        amount += Mathf.Max(1, add);
        RefreshCount() ;

        // 같은 아이템이 또 들어오면
        PlaySequence(resetPos: false);
    }

    void RefreshCount()
    {
        if (!countText) return;
        countText.text = amount > 0 ? $"x{amount}" : "";
    }

    void PlaySequence(bool resetPos)
    {
        seq?.Kill();

        Vector2 basePos = animTarget.anchoredPosition;

        if (resetPos)
            animTarget.anchoredPosition = basePos + new Vector2(-slideX, 0f);

        animTarget.localScale = Vector3.one;

        // DOTween은 기본이 Time.timeScale 영향을 받음.
        // UI는 보통 pause 중에도 뜨게 하고 싶으니 SetUpdate(true)로 unscaled로 돌림.
        seq = DOTween.Sequence().SetUpdate(true);

        // IN
        seq.Append(cg.DOFade(1f, inDuration).SetEase(Ease.OutQuad));
        seq.Join(animTarget.DOAnchorPos(basePos, inDuration).SetEase(Ease.OutCubic));

        // 살짝 “팝”
        seq.Join(animTarget.DOScale(popScale, 0.10f).SetEase(Ease.OutQuad));
        seq.Append(animTarget.DOScale(1f, 0.10f).SetEase(Ease.OutQuad));

        // HOLD
        seq.AppendInterval(holdDuration);

        // OUT
        seq.Append(cg.DOFade(0f, outDuration).SetEase(Ease.OutQuad));

        seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}
