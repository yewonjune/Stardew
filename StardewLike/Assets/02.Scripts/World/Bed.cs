// 클래스 선언 변경
using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
    // interactKey, interactCooldown, lastInteractTime 필드 제거
    bool playerInRange;

    public string InteractLabel => "잠자기";

    public void Interact()
    {
        DialogueManager.Instance.Confirm(
            "잠을 자면 다음날 아침 6시가 됩니다.\n자겠어요?",
            onOK: () =>
            {
                var timeManager = FindObjectOfType<TimeManager>();
                if (timeManager != null)
                {
                    FadeManager.Instance.FadeOutIn(() =>
                    {
                        timeManager.EndDay();
                    });
                }
            },
            onCancel: () => { },
            pauseGame: true
        );
    }

    // Update() 전체 제거

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }
}