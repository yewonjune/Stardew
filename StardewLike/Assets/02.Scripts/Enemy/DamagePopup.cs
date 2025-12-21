using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    [SerializeField] float riseY = 0.6f;
    [SerializeField] float duration = 0.7f;
    [SerializeField] float punchScale = 0.18f;

    [SerializeField] float randomX = 0.25f;

    Sequence seq;
    Color baseColor;
    Vector3 baseScale;

    private void Awake()
    {
        if (text == null) text = GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null) baseColor = text.color;

        baseScale = transform.localScale;
    }

    public void Play(int damage, bool isCrit = false)
    {
        if (text == null) return;

        text.text = damage.ToString();

        float critScaleMul = isCrit ? 1.15f : 1.0f;

        var tr = transform;
        var startPos = tr.position;
        tr.position = startPos + new Vector3(Random.Range(-randomX, randomX), 0f, 0f);

        transform.localScale = baseScale * critScaleMul;

        var c = baseColor;
        c.a = 1f;
        text.color = c;

        seq?.Kill();

        seq = DOTween.Sequence();

        // 통통 (타격감)
        seq.Join(tr.DOPunchScale(baseScale * punchScale, 0.18f, 8, 0.9f));

        // 위로 슥
        seq.Join(tr.DOMoveY(tr.position.y + riseY, duration).SetEase(Ease.OutCubic));

        // 페이드 아웃
        seq.Join(text.DOFade(0f, duration).SetEase(Ease.InQuad));

        // 제거
        seq.OnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
        seq?.Kill();
    }
}
