using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    [SerializeField] PlayerStaminaController playerStaminaController;
    [SerializeField] Image staminaFill;

    [SerializeField] GameObject uiRoot;
    [SerializeField] bool showOnlyWhileRunning = true;

    private void Awake()
    {
        if(playerStaminaController == null)
            playerStaminaController = FindObjectOfType<PlayerStaminaController>();

        if (uiRoot == null)
            uiRoot = staminaFill != null ? staminaFill.transform.parent.gameObject : gameObject;

        if(uiRoot) uiRoot.SetActive(false);
    }

    void LateUpdate()
    {
        if (!playerStaminaController || !staminaFill || !uiRoot) return;

        float fillValue = playerStaminaController.stamina / playerStaminaController.maxStamina;
        staminaFill.fillAmount = fillValue;

        if(showOnlyWhileRunning )
        {
            bool isRunning = playerStaminaController.IsRunningUI;
            bool isFull = fillValue > 0.999f;

            uiRoot.SetActive(!isFull && (isRunning || fillValue < 0.999f));
        }
        else
        {
            uiRoot.SetActive(fillValue < 0.999f);
        }

    }
}
