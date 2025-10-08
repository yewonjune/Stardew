using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [SerializeField] GameObject dialoguePanel;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] Image portraitImage;

    DialogueLine[] lines;
    DialogueData currentData;

    int index;
    bool isDialogueActive;
    float lastAdvanceTime;
    const float advanceCooldown = 0.05f;

    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] MonoBehaviour[] extraToDisable;

    void Awake()
    {
        Instance = this;

        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (portraitImage) portraitImage.enabled = false;
    }
    void Update()
    {
        if (!isDialogueActive) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (Time.time - lastAdvanceTime > advanceCooldown)
            {
                lastAdvanceTime = Time.time;
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

        FreezePlayer(true);
    }

    DialogueLine[] PickSequenceLines(DialogueData data)
    {
        if (data.sequences != null && data.sequences.Length > 0)
        {
            DialogueSequence dialogueSequence = data.sequences[Random.Range(0, data.sequences.Length)];
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
        isDialogueActive = false;

        dialoguePanel.SetActive(false);

        FreezePlayer(false);
    }
    public void OnClickNext()
    {
        if (!isDialogueActive) return;
        if (Time.time - lastAdvanceTime > advanceCooldown)
        {
            lastAdvanceTime = Time.time;
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
}
