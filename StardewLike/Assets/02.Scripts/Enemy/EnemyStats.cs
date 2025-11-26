using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Stats")]
public class EnemyStats : ScriptableObject
{
    public int maxHp = 20;
    public float moveSpeed = 2f;
    public float detectRadius = 5f;
    public int attackPower = 5;

    public float deathDestroyDelay = 0.6f;
}
