using UnityEngine;
using DG.Tweening;

public class ChestRewardFx : MonoBehaviour
{
    [SerializeField] SpriteRenderer icon;
    [SerializeField] float jumpY = 0.7f;
    [SerializeField] float jumpDur = 0.35f;
    [SerializeField] float hold = 0.35f;
    [SerializeField] float fadeOut = 0.25f;
    [SerializeField] float popScale = 1.15f;

    Tween t;

    void OnDisable() => t?.Kill();

    public void Play(Sprite sprite)
    {
        if (icon) icon.sprite = sprite;

        var tr = icon ? icon.transform : transform;
        tr.localScale = Vector3.one * 0.85f;

        // 시작 알파
        if (icon)
        {
            var c = icon.color;
            c.a = 1f;
            icon.color = c;
        }

        Vector3 basePos = transform.position;

        // timeScale 영향 안 받게 (보통 연출은 pause에도 보이게)
        Sequence seq = DOTween.Sequence().SetUpdate(true);

        seq.Append(tr.DOScale(popScale, 0.1f).SetEase(Ease.OutQuad));
        seq.Append(tr.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));

        seq.Join(transform.DOMoveY(basePos.y + jumpY, jumpDur).SetEase(Ease.OutQuad));

        seq.AppendInterval(hold);

        if (icon)
            seq.Append(icon.DOFade(0f, fadeOut).SetEase(Ease.OutQuad));
        else
            seq.AppendInterval(fadeOut);

        seq.OnComplete(() => Destroy(gameObject));

        t = seq;
    }
}
