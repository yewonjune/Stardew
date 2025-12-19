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
    public Button exitBtn;

    public RectTransform mainMenuPanel;
    public RectTransform newGamePanel;

    [Header("New Game Slide")]
    public float mainMenuHideX = 2000f;
    public float newGameTargetX = 0f;

    public float transitionDuration = 1f;

    Vector2 mainMenuStartPos;
    Vector2 newGameStartPos;

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

        if (newBtn != null)
            newBtn.onClick.AddListener(OnNewGameClicked);

        if (startBtn != null)
            startBtn.onClick.AddListener(OnStartBtnClicked);
        
        if (exitBtn != null)
            exitBtn.onClick.AddListener(OnExitClicked);

    }
    void OnNewGameClicked()
    {
        //newGamePanel.gameObject.SetActive(true);

        //mainMenuPanel.DOAnchorPosX(mainMenuHideX, transitionDuration);
        //newGamePanel.DOAnchorPosX(newGameTargetX, transitionDuration);

        BootParam.ForceNewGameReset = true;
        BootParam.PlayIntroCutscene = true;
        SceneManager.LoadScene("ManagerScene");
    }

    void OnStartBtnClicked()
    {
        BootParam.ForceNewGameReset = false;
        BootParam.PlayIntroCutscene = false;

        SceneManager.LoadScene("ManagerScene");
    }

    void OnExitClicked()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
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
    }
}
