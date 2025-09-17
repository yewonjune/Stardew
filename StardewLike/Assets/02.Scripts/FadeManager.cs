using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void FadeOutIn(System.Action onFadeMiddle)
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(fadeCanvasGroup.DOFade(1f, fadeDuration))   // 검게
           .AppendCallback(() => onFadeMiddle?.Invoke())       // 위치 이동, 카메라 전환 등
           .AppendInterval(0.5f)                                // 아주 살짝 쉼 (안정용)
           .Append(fadeCanvasGroup.DOFade(0f, fadeDuration));   // 밝게

        seq.Play();
    }
}
