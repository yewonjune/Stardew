using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

    Action onDialogueComplete;

    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] MonoBehaviour[] extraToDisable;

    public static bool IsBusy =>
        Instance != null && (Instance.isDialogueActive || Instance.modalOpen);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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

    // 일반 대화
    public void StartDialogue(DialogueData data, Action onComplete = null)
    {
        if (isDialogueActive) return;

        DialogueLine[] selected = PickSequenceLines(data);
        if (selected == null || selected.Length == 0) return;

        SetupDialogue(data, selected, onComplete);

        lastAdvanceTime = Time.unscaledTime - advanceCooldown;
    }

    // 선물 받았을 때
    public void StartDialogue(DialogueData data, DialogueSequence seq, Action onComplete = null)
    {
        if (data == null || seq == null) return;
        if (isDialogueActive) return;

        SetupDialogue(data, seq.lines, onComplete);

        lastAdvanceTime = Time.unscaledTime - advanceCooldown;
    }

    void SetupDialogue(DialogueData data, DialogueLine[] selectedLines, Action onComplete)
    {
        isDialogueActive = true;
        currentData = data;
        lines = selectedLines;
        index = 0;
        onDialogueComplete = onComplete;

        dialoguePanel.SetActive(true);

        // 이름
        if (nameText)
        {
            nameText.text = (data.npcData != null && !string.IsNullOrEmpty(data.npcData.displayName))
                ? data.npcData.displayName
                : "???";
        }

        // 기본 초상화
        if (portraitImage != null)
        {
            Sprite s = (data.npcData != null) ? data.npcData.defaultPortrait : null;
            portraitImage.sprite = s;
            portraitImage.enabled = (s != null);
        }

        ShowLine();

        //FreezePlayer(true);
        PlayerActionLock.Lock("Dialogue");
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
        if (modalOpen) return;
        if (!isDialogueActive) return;

        if (currentData != null && currentData.npcData != null)
        {
            QuestManager.I?.OnTalkedTo(currentData.npcData.npcId);
        }

        isDialogueActive = false;
        dialoguePanel.SetActive(false);

        onDialogueComplete?.Invoke();
        onDialogueComplete = null;

        PlayerActionLock.Unlock("Dialogue");


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

    public void Confirm(string message, Action onOK, Action onCancel=null, bool pauseGame=true)
    {
        if(confirmDialog == null)    return;

        modalOpen = true;
        //FreezePlayer(true);
        PlayerActionLock.Lock("DialogueModal");

        confirmDialog.Open(
            message,
            onOK: () =>
            {
                PlayerActionLock.Unlock("DialogueModal");
                modalOpen = false;

                onOK?.Invoke();
            },
            onCancel: () =>
            {
                PlayerActionLock.Unlock("DialogueModal");
                modalOpen = false;

                onCancel?.Invoke();
            },
            pauseGame: pauseGame
        );
    }

    void OnDisable()
    {
        if (isDialogueActive || modalOpen)
        {
            isDialogueActive = false;
            modalOpen = false;

            PlayerActionLock.Unlock("Dialogue");
            PlayerActionLock.Unlock("DialogueModal");

        }
    }
}
