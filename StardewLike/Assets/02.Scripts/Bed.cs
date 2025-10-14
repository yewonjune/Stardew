using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : MonoBehaviour
{
    [SerializeField] KeyCode interactKey = KeyCode.E;
    [SerializeField] float interactCooldown = 0.3f;

    bool playerInRange;
    float lastInteractTime;

    void Update()
    {
        if(playerInRange && Time.time - lastInteractTime >= interactCooldown)
        {
            if (Input.GetKeyDown(interactKey))
            {
                lastInteractTime = Time.time;

                DialogueManager.Instance.Confirm(
                   "잠을 자면 다음날 아침 6시가 됩니다.\n잘까요?",
                   onOK: () =>
                   {
                       var timeManager = FindObjectOfType<TimeManager>();
                       if (timeManager != null)
                       {
                           FadeManager.Instance.FadeOutIn(() =>
                           {
                               timeManager.EndDay();

                               Debug.Log("플레이어가 잠을 자서 다음날 아침 6시가 되었습니다!");
                           });

                       }
                   },
                   onCancel: () =>
                   {
                       Debug.Log("잠자기 취소");
                   },
                   pauseGame: true
               );
            }
        }
        
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
