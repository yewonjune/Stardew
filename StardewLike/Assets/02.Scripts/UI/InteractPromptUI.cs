using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractPromptUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI label;   // "대화하기" 등 텍스트
    Camera _cam;

    void Awake()
    {
        _cam = Camera.main;
        Hide();
    }

    void LateUpdate()
    {
        // 항상 카메라를 바라보게 (2D에서 빌보드 효과)
        transform.rotation = _cam.transform.rotation;
    }

    public void Show(Vector3 worldPos, string text)
    {
        transform.position = worldPos;
        label.text = text;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
