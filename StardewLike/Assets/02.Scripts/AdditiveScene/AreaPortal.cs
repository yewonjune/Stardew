using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaPortal : MonoBehaviour
{
    public string targetScene;
    public string spawnPointId;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        FadeManager.Instance.FadeOutIn(() =>
        {
            StartCoroutine(SwitchArea());
        });
    }

    System.Collections.IEnumerator SwitchArea()
    {
        string current = SceneManager.GetActiveScene().name;

        var load = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
        yield return load;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(targetScene));

        PlayerSpawnManager.NextSpawnPointId = spawnPointId;
        PlayerSpawnManager.Instance.PlacePlayerAtSpawn();

        var unload = SceneManager.UnloadSceneAsync(current);
        yield return unload;
    }
}
