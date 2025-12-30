using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFatigueController : MonoBehaviour
{
    public float maxFatigue = 100f;
    public float current = 0f;

    public float defaultToolFatigue = 2f;

    public struct ToolFatigue
    {
        public ToolType toolType;
        public float amount;
    }
    public List<ToolFatigue> toolFatigues = new()
    {
        new ToolFatigue{ toolType = ToolType.Hoe,         amount = 2f },
        new ToolFatigue{ toolType = ToolType.Pickaxe,     amount = 2f },
        new ToolFatigue{ toolType = ToolType.Axe,         amount = 2f },
        new ToolFatigue{ toolType = ToolType.Scythe,      amount = 0f },
        new ToolFatigue{ toolType = ToolType.WateringCan, amount = 1f },
        new ToolFatigue{ toolType = ToolType.Sword,       amount = 0f },
        new ToolFatigue{ toolType = ToolType.Fishingrod,  amount = 2f },
    };
    public bool IsFull => current >= maxFatigue;

    Dictionary<ToolType, float> dict;

    public event Action<float, float> OnFatigueChanged;

    void Awake()
    {
        dict = new Dictionary<ToolType, float>();
        foreach (var tf in toolFatigues) dict[tf.toolType] = tf.amount;
        ClampAndRaise();
    }

    float GetAmount(ToolType type)
    {
        if (dict != null && dict.TryGetValue(type, out float amount))
            return amount;

        return defaultToolFatigue;
    }

    public void AddByTool(ToolType t)
    {
        current += GetAmount(t);

        if (current > maxFatigue) current = maxFatigue;

        ClampAndRaise();
    }

    public void AddFatigue(float amount)
    {
        current += amount;
        ClampAndRaise();
    }

    public void ReduceFatigue(float amount)
    {
        current -= amount;
        ClampAndRaise();
    }

    public void RecoverOnSleep(float recoverAmount = 60f, bool fullRecover = false)
    {
        if (fullRecover) current = 0f;
        else current = Mathf.Max(0f, current - recoverAmount);
        ClampAndRaise();
    }
    void ClampAndRaise()
    {
        current = Mathf.Clamp(current, 0f, maxFatigue);
        OnFatigueChanged?.Invoke(current, maxFatigue);
    }

    public float Ratio => maxFatigue > 0 ? current / maxFatigue : 0f;
}
