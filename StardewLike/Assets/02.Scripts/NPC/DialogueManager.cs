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

    string[] lines;
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
        isDialogueActive = true;

        dialoguePanel.SetActive(true);
        nameText.text = data.npcName;
        lines = data.lines;
        index = 0;
        ShowLine();

        FreezePlayer(true);
    }

    public void ShowLine()
    {
        if (index < lines.Length)
        {
            dialogueText.text = lines[index];
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
