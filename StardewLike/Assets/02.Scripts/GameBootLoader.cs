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
        
        string slot = BootParam.Slot;

        // 1) 처음부터: 리셋(삭제 + 기본 저장)
        if (BootParam.ForceNewGameReset)
        {
            BootParam.ForceNewGameReset = false;

            await svc.DeleteAsync(slot);

            var newData = SaveBuilder.BuildNewGameDefault();
            await svc.SaveAsync(slot, newData);
        }

        // 2) 이어하기: 로드(없으면 기본 생성)
        var data = await svc.LoadAsync(slot);
        if (data == null)
        {
            Debug.LogWarning($"[Boot] No save found at {slot}. Creating default.");
            data = SaveBuilder.BuildNewGameDefault();
            await svc.SaveAsync(slot, data);
        }

        SaveBuilder.Apply(data, tm);

        foreach (var soil in FindObjectsOfType<SoilTilemapController>())
        {
            soil.ForceRebuildFromState();
            soil.RestoreFromState();
        }
    }

}