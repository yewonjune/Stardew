using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.VisualScripting;

public class ConfirmDialog : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] Button okButton;
    [SerializeField] Button cancelButton;
    [SerializeField] GameObject dimPanel;
    [SerializeField] GameObject dialogPanel;

    Action _onOK;
    Action _onCancel;
    bool isOpen;

    void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        HideImmediate();
    }

    public void Open(string message, Action onOK, Action onCancel = null, bool pauseGame = true)
    {
        if (isOpen) return;
        isOpen = true;

        descriptionText.text = message;
        _onOK = onOK;
        _onCancel = onCancel;

        dimPanel.SetActive(true);
        dialogPanel.SetActive(true);

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        okButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        okButton.onClick.AddListener(() =>
        {
            Close(pauseGame);
            _onOK?.Invoke();
        });

        cancelButton.onClick.AddListener(() =>
        {
            Close(pauseGame);
            _onCancel?.Invoke();
        });

        if (pauseGame) GamePause.Pause();
    }

    public void Close(bool resumeGame = true)
    {
        if (!isOpen) return;
        isOpen = false;

        HideImmediate();
        if (resumeGame) GamePause.Resume();
    }

    void HideImmediate()
    {
        if (dimPanel) dimPanel.SetActive(false);
        if (dialogPanel) dialogPanel.SetActive(false);

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0f;
    }
}
