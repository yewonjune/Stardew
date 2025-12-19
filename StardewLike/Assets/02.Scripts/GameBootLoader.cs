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
            var svc = FindObjectOfType<CloudSaveService>(true);
            if (svc == null) { Debug.LogError("[Boot] CloudSaveService missing"); return; }

            await svc.InitTask;

            var tm = FindObjectOfType<TimeManager>(true);
            if (tm == null) { Debug.LogError("[Boot] TimeManager missing"); return; }

            string slot = BootParam.Slot;

            if (BootParam.ForceNewGameReset)
            {
                BootParam.ForceNewGameReset = false;

                await svc.DeleteAsync(slot);

                var newData = SaveBuilder.BuildNewGameDefault();
                await svc.SaveAsync(slot, newData);
            }

            var data = await svc.LoadAsync(slot);
            if (data == null)
            {
                Debug.LogWarning($"[Boot] No save found at {slot}. Creating default.");
                data = SaveBuilder.BuildNewGameDefault();
                await svc.SaveAsync(slot, data);
            }

            SaveBuilder.Apply(data, tm);

            // 씬 로드 타이밍에 따라 없을 수 있으니 비활성 포함
            foreach (var soil in FindObjectsOfType<SoilTilemapController>(true))
            {
                soil.ForceRebuildFromState();
                soil.RestoreFromState();
            }
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
