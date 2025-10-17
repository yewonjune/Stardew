using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static Unity.Collections.AllocatorManager;

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
        if (!fadeCanvasGroup) return;

        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.blocksRaycasts = true;

        var seq = DG.Tweening.DOTween.Sequence().SetUpdate(true);
        seq.Append(fadeCanvasGroup.DOFade(1f, fadeDuration))
           .AppendCallback(() => onFadeMiddle?.Invoke())
           .AppendInterval(0.5f)
           .Append(fadeCanvasGroup.DOFade(0f, fadeDuration))
           .OnComplete(() =>
           {
               fadeCanvasGroup.blocksRaycasts = false;
               fadeCanvasGroup.gameObject.SetActive(false);
           })
           .Play();
    }

    public IEnumerator FadeOut(float duration)
    {
        if (!fadeCanvasGroup) yield break;
        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.blocksRaycasts = true;

        float t = 0f;
        float start = fadeCanvasGroup.alpha;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(start, 1f, t / duration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;
    }

    public IEnumerator FadeIn(float duration)
    {
        if (!fadeCanvasGroup) yield break;

        float t = 0f;
        float start = fadeCanvasGroup.alpha;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(start, 0f, t / duration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.gameObject.SetActive(false);
    }

    public void FadeInCoroutine(float duration)
    {
        StartCoroutine(FadeIn(duration));
    }

}
