using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Food", menuName = "Inventory/Food")]
public class Food : Item
{
    public int restoreStamina = 20;
    public int restoreHealth = 0;

    public bool givesBuff;
    public float buffDuration = 60f;

    public float moveSpeedMultiplier = 1.0f; // 1이면 변화 없음
}
