using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public DialogueData dialogueData;
    Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Interact()
    {
        DialogueManager.Instance.StartDialogue(dialogueData);
    }
}
