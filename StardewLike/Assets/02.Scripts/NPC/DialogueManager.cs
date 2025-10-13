using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [SerializeField] GameObject dialoguePanel;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] Image portraitImage;

    [SerializeField] ConfirmDialog confirmDialog;

    DialogueLine[] lines;
    DialogueData currentData;

    int index;
    bool isDialogueActive;
    bool modalOpen;
    float lastAdvanceTime;
    const float advanceCooldown = 0.05f;

    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] MonoBehaviour[] extraToDisable;

    public static bool IsBusy =>
        Instance != null && (Instance.isDialogueActive || Instance.modalOpen);

    void Awake()
    {
        Instance = this;

        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (portraitImage) portraitImage.enabled = false;
    }
    void Update()
    {
        if (!isDialogueActive) return;
        if (modalOpen) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (Time.unscaledTime - lastAdvanceTime > advanceCooldown)
            {
                lastAdvanceTime = Time.unscaledTime;
                ShowLine();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            EndDialogue();
    }

    public void StartDialogue(DialogueData data)
    {
        DialogueLine[] selected = PickSequenceLines(data);
        if (selected == null || selected.Length == 0)
        {
            return;
        }

        isDialogueActive = true;
        currentData = data;
        lines = selected;
        index = 0;

        dialoguePanel.SetActive(true);

        nameText.text = (data.npcData != null && !string.IsNullOrEmpty(data.npcData.displayName))
                ? data.npcData.displayName
                : "???";

        if (portraitImage != null)
        {
            Sprite s = (data.npcData != null) ? data.npcData.defaultPortrait : null;
            portraitImage.sprite = s;
            portraitImage.enabled = (s != null);
        }

        ShowLine();

        lastAdvanceTime = Time.unscaledTime - advanceCooldown;

        FreezePlayer(true);
        GamePause.Pause();
    }

    DialogueLine[] PickSequenceLines(DialogueData data)
    {
        if (data.sequences != null && data.sequences.Length > 0)
        {
            DialogueSequence dialogueSequence = data.sequences[UnityEngine.Random.Range(0, data.sequences.Length)];
            if (dialogueSequence != null && dialogueSequence.lines != null && dialogueSequence.lines.Length > 0)
                return dialogueSequence.lines;
        }

        return null;
    }

    public void ShowLine()
    {
        if (index < lines.Length)
        {
            DialogueLine line = lines[index];

            dialogueText.text = line.text;

            if (portraitImage != null && currentData != null && currentData.npcData != null)
            {
                Sprite s = currentData.npcData.GetPortrait(line.emotion);
                if (s == null) s = currentData.npcData.defaultPortrait;
                portraitImage.sprite = s;
                portraitImage.enabled = (s != null);
            }

            index++;
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        Debug.Log("[DialogueManager] EndDialogue called");
        isDialogueActive = false;

        dialoguePanel.SetActive(false);

        FreezePlayer(false);
        GamePause.Resume();
        Debug.Log($"[DialogueManager] After EndDialogue: isPaused={GamePause.isPaused}, timeScale={Time.timeScale}");
    
}
    public void OnClickNext()
    {
        if (!isDialogueActive || modalOpen) return;
        if (Time.unscaledTime - lastAdvanceTime > advanceCooldown)
        {
            lastAdvanceTime = Time.unscaledTime;
            ShowLine();
        }
    }

    void FreezePlayer(bool freeze)
    {
        if (playerMovement) playerMovement.SetControl(!freeze);

        if (extraToDisable != null)
        {
            foreach (var comp in extraToDisable)
                if (comp) comp.enabled = !freeze;
        }
    }

    public void Confirm(string message, Action onOK, Action onCancel=null, bool pauseGame=true)
    {
        if(confirmDialog == null)
        {
            Debug.LogError("[DialogueManager] ConfirmDialog reference is missing.");
            return;
        }

        modalOpen = true;
        FreezePlayer(true);

        confirmDialog.Open(
            message,
            onOK: () =>
            {
                FreezePlayer(false);
                modalOpen = false;
                onOK?.Invoke();
            },
            onCancel: () =>
            {
                FreezePlayer(false);
                modalOpen = false;
                onCancel?.Invoke();
            },
            pauseGame: pauseGame
        );
    }

    void OnDisable()   //  예비 안전장치: 씬 전환 중 UI 닫히면서 멈춤 방지
    {
        if (isDialogueActive || modalOpen)
        {
            isDialogueActive = false;
            modalOpen = false;
            GamePause.ResetAll();
            FreezePlayer(false);
        }
    }
}
