using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Boot());
    }

    IEnumerator Boot()
    {
        var op = SceneManager.LoadSceneAsync("FarmScene", LoadSceneMode.Additive);
        yield return op;

        var farm = SceneManager.GetSceneByName("FarmScene");
        if (farm.IsValid())
            SceneManager.SetActiveScene(farm);
    }
}
