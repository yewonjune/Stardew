using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootLoader : MonoBehaviour
{
    async void Start()
    {
        var svc = FindObjectOfType<CloudSaveService>();
        if (svc == null) { Debug.LogError("CloudSaveService missing"); return; }

        await svc.InitTask;

        var tm = FindObjectOfType<TimeManager>();

        var data = await svc.LoadAsync("slot1");
        if (data != null)
        {
            SaveBuilder.Apply(data, tm);

            // 현재 씬의 타일/오브젝트를 즉시 반영하고 싶다면:
            //   - 씬 리로드
            //   - 또는 SoilTilemapController에 "ForceRebuildFromState()" 같은 메서드를 만들어 호출
            foreach (var soil in FindObjectsOfType<SoilTilemapController>())
            {
                soil.ForceRebuildFromState();
                soil.RestoreFromState();
            }
        }
    }

}