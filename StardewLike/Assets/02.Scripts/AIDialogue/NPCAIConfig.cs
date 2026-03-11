using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[DefaultExecutionOrder(-999)]
public class NPCAIConfig : MonoBehaviour
{
    public static NPCAIConfig Instance { get; private set; }

    [Header("Anthropic API Key")]
    [Tooltip("Anthropic Console에서 발급받은 API 키를 입력하세요.")]
    [SerializeField] private string apiKey = "";

    public string ApiKey => apiKey;
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (string.IsNullOrEmpty(apiKey))
            Debug.LogWarning("[NPCAIConfig] API 키가 설정되지 않았습니다. Inspector에서 입력하세요.");
    }
}
