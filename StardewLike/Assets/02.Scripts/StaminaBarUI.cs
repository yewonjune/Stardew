using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    [SerializeField] PlayerStaminaController playerStaminaController;
    [SerializeField] Image staminaFill;
    [SerializeField] bool hideWhenFull = true;

    void LateUpdate()
    {
        if (!playerStaminaController || !staminaFill) return;

        float fillValue = playerStaminaController.stamina / playerStaminaController.maxStamina;
        staminaFill.fillAmount = fillValue;

        if (hideWhenFull)
            gameObject.SetActive(fillValue < 0.999f);
    }
}
