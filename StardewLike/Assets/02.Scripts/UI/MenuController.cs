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

    public float transitionDuration = 1f;
    public float slideOffsetX = 2080f;
    public float slideOffsetY = 540f;

    // Start is called before the first frame update
    void Start()
    {
        if (newBtn != null)
            newBtn.onClick.AddListener(OnNewGameClicked);

        if (startBtn != null)
            startBtn.onClick.AddListener(OnStartBtnClicked);

        if (coopBtn != null)
            coopBtn.onClick.AddListener(OnCoopBtnClicked);
    }
    void OnNewGameClicked()
    {
        newGamePanel.gameObject.SetActive(true);

        mainMenuPanel.DOAnchorPosX(mainMenuPanel.anchoredPosition.x + slideOffsetX,
                                    transitionDuration);
        newGamePanel.DOAnchorPosX(newGamePanel.anchoredPosition.x + slideOffsetX,
                                    transitionDuration);
    }

    void OnStartBtnClicked()
    {
        SceneManager.LoadScene("ManagerScene");
    }

    void OnCoopBtnClicked()
    {
        coopGamePanel.gameObject.SetActive(true);

        mainMenuPanel.DOAnchorPosY(mainMenuPanel.anchoredPosition.y + slideOffsetY,
                            transitionDuration);

        coopGamePanel.DOAnchorPosY(coopGamePanel.anchoredPosition.y + slideOffsetY,
                            transitionDuration);

        //SceneManager.LoadScene("CoopScene");
    }
}
