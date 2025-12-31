using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Video;
using TMPro;
using UnityEngine.SceneManagement;

public class OpeningController : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayer;

    [SerializeField] Image fadeOverlay;
    [SerializeField] Image fadeUIOverlay;
    [SerializeField] float fadeDuration = 1f;

    [SerializeField] GameObject dialoguePanel;
    [SerializeField] Image arrowIcon;
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] string[] lines;

    int idx = 0;
    bool dialogueActive = false;
    bool isTransitioning = false;

    [SerializeField] float charInterval = 0.03f;      // 타이핑 속도
    [SerializeField] float lineStartDelay = 0.05f;    // 줄 시작 딜레이

    Coroutine typingCo;
    bool isTyping = false;
    bool canNext = false;

    Tween arrowTween;
    Vector3 arrowBasePos;
    Vector3 arrowBaseScale;

    private void Awake()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (dialogueText) dialogueText.text = "";

        if(arrowIcon)
        {
            arrowIcon.gameObject.SetActive(false);
            arrowBasePos = arrowIcon.rectTransform.anchoredPosition3D;
            arrowBaseScale = arrowIcon.rectTransform.localScale;
        }

        if (fadeOverlay)
        {
            var c = fadeOverlay.color;
            c.a = 0f;
            fadeOverlay.color = c;
            fadeOverlay.gameObject.SetActive(true);
        }

        if (fadeUIOverlay)
        {
            var c = fadeUIOverlay.color;
            c.a = 0f;
            fadeUIOverlay.color = c;
            fadeUIOverlay.gameObject.SetActive(true);
        }

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (videoPlayer != null)
            videoPlayer.Play();
        else
            StartDialogue();
    }

    // Update is called once per frame
    void Update()
    {
        if (!dialogueActive) return;

        if(Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (canNext) ArrowPressFeedback();

            NextLineOrFinishTyping();
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (isTransitioning) return;

        if (fadeOverlay == null)
        {
            StartDialogue();
            return;
        }

        fadeOverlay.DOKill();
        fadeOverlay
            .DOFade(1f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (videoPlayer != null) videoPlayer.Stop();

                StartDialogue();
            });

    }

    void StartDialogue()
    {
        if (isTransitioning) return;
        dialogueActive = true;

        if (dialoguePanel) dialoguePanel.SetActive(true);

        idx = 0;
        if (lines == null || lines.Length == 0)
        {
            GoManager();
            return;
        }

        StartTyping(lines[idx]);
        //dialogueText.text = lines[idx];
    }

    void GoManager()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        dialogueActive = false;

        if (fadeUIOverlay == null)
        {
            SceneManager.LoadScene("ManagerScene");
            return;
        }

        fadeUIOverlay.DOKill();
        fadeUIOverlay
            .DOFade(1f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() => SceneManager.LoadScene("ManagerScene"));
      
    }
    void StartTyping(string fullLine)
    {
        StopTyping();
        HideArrow();

        typingCo = StartCoroutine(TypeLine(fullLine));
    }

    IEnumerator TypeLine(string fullLine)
    {
        isTyping = true;
        canNext = false;

        if (dialogueText) dialogueText.text = "";

        if (lineStartDelay > 0f)
            yield return new WaitForSecondsRealtime(lineStartDelay);

        // 한 글자씩 출력
        for (int i = 0; i < fullLine.Length; i++)
        {
            if (!isTyping) yield break;

            dialogueText.text += fullLine[i];
            yield return new WaitForSecondsRealtime(charInterval);
        }

        isTyping = false;
        canNext = true;

        // 타이핑 끝나면 화살표 표시 + 반복 애니
        ShowArrowWithLoop();
    }

    void StopTyping()
    {
        if (typingCo != null)
        {
            StopCoroutine(typingCo);
            typingCo = null;
        }
        isTyping = false;
    }

    void NextLineOrFinishTyping()
    {
        if (isTransitioning) return;

        if (isTyping)
        {
            StopTyping();
            dialogueText.text = lines[idx];

            canNext = true;
            ShowArrowWithLoop();
            return;
        }

        idx++;

        if (lines == null || idx >= lines.Length)
        {
            GoManager();
            return;
        }

        StartTyping(lines[idx]);
    }


    // 화살표 바운스 + 페이드 반복
    void ShowArrowWithLoop()
    {
        if (!arrowIcon) return;

        arrowIcon.gameObject.SetActive(true);

        arrowIcon.DOKill();
        var rt = arrowIcon.rectTransform;
        rt.anchoredPosition3D = arrowBasePos;
        rt.localScale = arrowBaseScale;

        var c = arrowIcon.color;
        c.a = 1f;
        arrowIcon.color = c;

        arrowTween?.Kill();

        // 바운스(위아래) + 페이드(yoyo) 동시에
        Sequence seq = DOTween.Sequence().SetUpdate(true);

        // 위로 6px
        seq.Join(rt.DOAnchorPosY(arrowBasePos.y + 6f, 0.45f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo));
        seq.Join(arrowIcon.DOFade(0.35f, 0.45f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo));

        arrowTween = seq;
    }

    void HideArrow()
    {
        canNext = false;

        if (!arrowIcon) return;

        arrowTween?.Kill();
        arrowTween = null;

        arrowIcon.DOKill();
        arrowIcon.gameObject.SetActive(false);

        arrowIcon.rectTransform.anchoredPosition3D = arrowBasePos;
        arrowIcon.rectTransform.localScale = arrowBaseScale;

        var c = arrowIcon.color;
        c.a = 1f;
        arrowIcon.color = c;
    }

    // 화살표 "툭" 반응
    void ArrowPressFeedback()
    {
        if (!arrowIcon || !arrowIcon.gameObject.activeSelf) return;

        var rt = arrowIcon.rectTransform;
        rt.DOKill();

        // 짧게 눌리는 느낌
        rt
            .DOScale(arrowBaseScale * 0.9f, 0.07f)
            .SetUpdate(true)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                rt.DOScale(arrowBaseScale, 0.10f).SetUpdate(true).SetEase(Ease.OutBack);
            });
    }

}
