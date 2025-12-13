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
        if (isDialogueActive) return;

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
        PlayerActionLock.Lock("Dialogue");
    }

    public void StartDialogue(DialogueData data, DialogueSequence seq)
    {
        if (data == null || seq == null) return;
        if (isDialogueActive) return;

        isDialogueActive = true;
        currentData = data;
        lines = seq.lines;
        index = 0;

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

        lastAdvanceTime = Time.unscaledTime - advanceCooldown;

        FreezePlayer(true);
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
        isDialogueActive = false;

        dialoguePanel.SetActive(false);

        PlayerActionLock.Unlock("Dialogue");

        if (!PlayerActionLock.IsLocked)
            FreezePlayer(false);
        else
            FreezePlayer(true);

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
        if(confirmDialog == null)    return;

        modalOpen = true;
        FreezePlayer(true);
        PlayerActionLock.Lock("DialogueModal");

        confirmDialog.Open(
            message,
            onOK: () =>
            {
                PlayerActionLock.Unlock("DialogueModal");
                modalOpen = false;

                if (!PlayerActionLock.IsLocked)
                    FreezePlayer(false);
                else
                    FreezePlayer(true);

                onOK?.Invoke();
            },
            onCancel: () =>
            {
                PlayerActionLock.Unlock("DialogueModal");
                modalOpen = false;

                if (!PlayerActionLock.IsLocked)
                    FreezePlayer(false);
                else
                    FreezePlayer(true);

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

            if (!PlayerActionLock.IsLocked)
                FreezePlayer(false);
        }
    }
}
