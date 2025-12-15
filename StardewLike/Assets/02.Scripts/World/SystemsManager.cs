using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemsManager : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
