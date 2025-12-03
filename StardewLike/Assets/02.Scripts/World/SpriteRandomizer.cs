using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRandomizer : MonoBehaviour
{
    public Sprite[] variantSprites;

    void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;
        if (variantSprites == null || variantSprites.Length == 0) return;

        // 랜덤 스프라이트 선택
        sr.sprite = variantSprites[Random.Range(0, variantSprites.Length)];
    }
}
