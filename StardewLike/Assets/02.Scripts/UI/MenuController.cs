using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public Button newBtn;
    public Button startBtn;
    public Button coopBtn;

    public RectTransform mainMenuPanel;
    public RectTransform newGamePanel;
    public RectTransform coopGamePanel;

    public Button coopPanelCancelBtn;

    [Header("New Game Slide")]
    public float mainMenuHideX = 2000f;
    public float newGameTargetX = 0f;

    [Header("Coop Slide")]
    public float coopTargetY = 0f;
    public float mainMenuHideY = 1080f;

    public float transitionDuration = 1f;

    Vector2 mainMenuStartPos;
    Vector2 newGameStartPos;
    Vector2 coopStartPos;

    // Start is called before the first frame update
    void Start()
    {
        if (mainMenuPanel != null)
            mainMenuStartPos = mainMenuPanel.anchoredPosition;

        if (newGamePanel != null)
        {
            newGameStartPos = newGamePanel.anchoredPosition;
            newGamePanel.gameObject.SetActive(false);
        }

        if (coopGamePanel != null)
        {
            coopStartPos = coopGamePanel.anchoredPosition;
            coopGamePanel.gameObject.SetActive(false);
        }

        if (newBtn != null)
            newBtn.onClick.AddListener(OnNewGameClicked);

        if (startBtn != null)
            startBtn.onClick.AddListener(OnStartBtnClicked);

        if (coopBtn != null)
            coopBtn.onClick.AddListener(OnCoopBtnClicked);

        if (coopPanelCancelBtn != null)
            coopPanelCancelBtn.onClick.AddListener(OnCoopPanelCancelBtnClicked);
    }
    void OnNewGameClicked()
    {
        newGamePanel.gameObject.SetActive(true);

        mainMenuPanel.DOAnchorPosX(mainMenuHideX, transitionDuration);
        newGamePanel.DOAnchorPosX(newGameTargetX, transitionDuration);
    }

    void OnStartBtnClicked()
    {
        SceneManager.LoadScene("ManagerScene");
    }

    void OnCoopBtnClicked()
    {
        coopGamePanel.gameObject.SetActive(true);

        mainMenuPanel.DOAnchorPosY(mainMenuHideY, transitionDuration);
        coopGamePanel.DOAnchorPosY(coopTargetY, transitionDuration);
    }

    public void ResetToDefault()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.anchoredPosition = mainMenuStartPos;

        if (newGamePanel != null)
        {
            newGamePanel.anchoredPosition = newGameStartPos;
            newGamePanel.gameObject.SetActive(false);
        }

        if (coopGamePanel != null)
        {
            coopGamePanel.anchoredPosition = coopStartPos;
            coopGamePanel.gameObject.SetActive(false);
        }
    }

    void OnCoopPanelCancelBtnClicked()
    {
        ResetToDefault();
    }
}
