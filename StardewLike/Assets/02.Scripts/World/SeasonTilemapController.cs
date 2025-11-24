using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonTilemapController : MonoBehaviour
{
    [Header("Seasonal Tilemaps For This Scene")]
    public GameObject springTilemap;
    public GameObject summerTilemap;
    public GameObject fallTilemap;
    public GameObject winterTilemap;

    void OnEnable()
    {
        if (SeasonManager.Instance != null)
        {
            // 씬이 켜질 때, 현재 계절에 맞게 한 번 적용
            ApplySeason(SeasonManager.Instance.currentSeason);
            // 이후 계절 변경 이벤트 구독
            SeasonManager.Instance.OnSeasonChanged += ApplySeason;
        }
    }

    void OnDisable()
    {
        if (SeasonManager.Instance != null)
        {
            SeasonManager.Instance.OnSeasonChanged -= ApplySeason;
        }
    }
    void ApplySeason(Season season)
    {
        if (springTilemap) springTilemap.SetActive(season == Season.Spring);
        if (summerTilemap) summerTilemap.SetActive(season == Season.Summer);
        if (fallTilemap) fallTilemap.SetActive(season == Season.Fall);
        if (winterTilemap) winterTilemap.SetActive(season == Season.Winter);
    }
}
