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

    public RectTransform mainMenuPanel;
    public RectTransform newGamePanel;

    public float transitionDuration = 1f;
    public float slideOffsetX = 2080f;

    // Start is called before the first frame update
    void Start()
    {
        if (newBtn != null)
            newBtn.onClick.AddListener(OnNewGameClicked);

        if (startBtn != null)
            startBtn.onClick.AddListener(OnstartBtnClicked);
    }
    void OnNewGameClicked()
    {
        newGamePanel.gameObject.SetActive(true);

        mainMenuPanel.DOAnchorPosX(mainMenuPanel.anchoredPosition.x + slideOffsetX,
                                    transitionDuration);
        newGamePanel.DOAnchorPosX(newGamePanel.anchoredPosition.x + slideOffsetX,
                                    transitionDuration);
    }

    void OnstartBtnClicked()
    {
        SceneManager.LoadScene("FarmScene");
    }
}
