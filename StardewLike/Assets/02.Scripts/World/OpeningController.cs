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
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] string[] lines;

    int idx = 0;
    bool dialogueActive = false;
    bool isTransitioning = false;

    private void Awake()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (dialogueText) dialogueText.text = "";

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
            NextLine();
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

        dialogueText.text = lines[idx];
    }
    void NextLine()
    {
        idx++;

        if (lines == null || idx >= lines.Length)
        {
            GoManager();
            return;
        }

        dialogueText.text = lines[idx];
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
}
