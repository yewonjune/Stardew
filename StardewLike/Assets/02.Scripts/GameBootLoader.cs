using System;
using UnityEngine;

public class GameBootLoader : MonoBehaviour
{
    static bool ran;

    async void Start()
    {
        if (ran) { Destroy(gameObject); return; }
        ran = true;

        try
        {
            Debug.Log("[Boot] Start begin");

            var svc = FindObjectOfType<CloudSaveService>(true);
            if (svc == null) { Debug.LogError("[Boot] CloudSaveService missing"); return; }

            await svc.InitTask;
            if (this == null) return;   // BootLoader가 파괴됐으면 중단
            if (svc == null) return;    // svc가 파괴됐으면 중단
            Debug.Log("[Boot] Firebase ready");

            var tm = FindObjectOfType<TimeManager>(true);
            if (tm == null) { Debug.LogError("[Boot] TimeManager missing"); return; }

            string slot = BootParam.Slot;

            if (BootParam.ForceNewGameReset)
            {
                BootParam.ForceNewGameReset = false;

                Debug.Log("[Boot] ForceNewGameReset -> delete & create default");
                await svc.DeleteAsync(slot);

                var newData = SaveBuilder.BuildNewGameDefault();
                await svc.SaveAsync(slot, newData);
            }

            var data = await svc.LoadAsync(slot);
            Debug.Log("[Boot] Load done. data=" + (data != null));

            if (data == null)
            {
                Debug.LogWarning($"[Boot] No save found at {slot}. Creating default.");
                data = SaveBuilder.BuildNewGameDefault();
                await svc.SaveAsync(slot, data);
            }

            Debug.Log("[Boot] Apply begin");
            SaveBuilder.Apply(data, tm);
            Debug.Log("[Boot] Apply end");

            // 씬/오브젝트 교체가 있을 수 있어서 한 프레임 늦추면 더 안전
            await System.Threading.Tasks.Task.Yield();

            Debug.Log("[Boot] Soil rebuild begin");
            foreach (var soil in FindObjectsOfType<SoilTilemapController>(true))
            {
                if (!soil) continue;
                soil.ForceRebuildFromState();
                soil.RestoreFromState();
            }
            Debug.Log("[Boot] Soil rebuild end");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            Destroy(gameObject);
        }
    }
}
