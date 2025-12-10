using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BombGoblinAttack : MonoBehaviour
{
    public GameObject bombPrefab;
    public Transform throwPoint;
    public float throwDuration = 0.6f;  // 폭탄 날아가는 시간
    public float jumpPower = 1.2f;      // 포물선 높이
    public int bombDamage = 1;

    public void ThrowBomb(Vector2 playerPos)
    {
        if (bombPrefab == null || throwPoint == null) return;

        GameObject bomb = Instantiate(bombPrefab, throwPoint.position, Quaternion.identity);

        Bomb bombBehavior = bomb.GetComponent<Bomb>();
        if (bombBehavior == null) bombBehavior = bomb.AddComponent<Bomb>();
        bombBehavior.damage = bombDamage;

        bomb.transform
           .DOJump(
               playerPos,      // 도착 위치
               jumpPower,      // 점프 높이
               1,              // 점프 횟수(1번)
               throwDuration   // 이동 시간
           )
           .SetEase(Ease.OutQuad);
    }
}

