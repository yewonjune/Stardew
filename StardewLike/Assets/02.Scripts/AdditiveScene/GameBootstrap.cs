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
        AsyncOperation op = SceneManager.LoadSceneAsync("FarmScene", LoadSceneMode.Additive);

        while (!op.isDone)
            yield return null;

        Scene farm = SceneManager.GetSceneByName("FarmScene");
        if (farm.IsValid())
            SceneManager.SetActiveScene(farm);
    }
}
