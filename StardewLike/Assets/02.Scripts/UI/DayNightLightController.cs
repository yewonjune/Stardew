using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightLightController : MonoBehaviour
{
    [SerializeField] Light2D globalLight;

    public AnimationCurve intensityCurve = AnimationCurve.Linear(0, 0.2f, 1, 0.2f);

    public Gradient colorOverDay;

    [SerializeField] float lerpSpeed = 5f;

    TimeManager _timeManager;
    float _nextFindTime = 0f;

    float DayRatio()
    {
        // TimeManager가 없으면 0.5초에 한 번만 다시 찾기(매 프레임 탐색 방지)
        if (_timeManager == null && Time.unscaledTime >= _nextFindTime)
        {
            _timeManager = FindObjectOfType<TimeManager>(true);
            _nextFindTime = Time.unscaledTime + 0.5f;
        }

        if (_timeManager == null) return 0f;

        float totalMin = _timeManager.hour * 60f + _timeManager.minute;
        return Mathf.Repeat(totalMin / 1440f, 1f);
    }

    void Reset()
    {
        intensityCurve = new AnimationCurve(
        new Keyframe(0.000f, 0.90f), // 06:00
        new Keyframe(0.111f, 1.00f), // 08:00
        new Keyframe(0.333f, 1.10f), // 12:00
        new Keyframe(0.611f, 1.00f), // 17:00 여전히 밝음
        new Keyframe(0.667f, 0.95f),
        new Keyframe(0.722f, 0.75f), // 19:00 서서히 노을
        new Keyframe(0.833f, 0.35f), // 21:00 꽤 어두움
        new Keyframe(1.000f, 0.20f)  // 24:00 밤
        );

        // 기본 색 그라디언트(밤=남보라, 낮=연노랑, 노을=주황)
        GradientColorKey[] ck = new GradientColorKey[] {
        new (new Color(0.95f, 0.95f, 0.90f), 0.000f), // 06:00 밝은 중립광
        new (new Color(1.00f, 0.98f, 0.90f), 0.333f), // 12:00 약간 따뜻한 낮
        new (new Color(1.00f, 0.85f, 0.65f), 0.722f), // 19:00 노을 시작
        new (new Color(0.18f, 0.20f, 0.30f), 1.000f)  // 24:00 밤(남청)
        };
        GradientAlphaKey[] ak = new GradientAlphaKey[] {
            new (1f, 0f), new (1f, 1f)
        };
        colorOverDay = new Gradient() { colorKeys = ck, alphaKeys = ak };
    }

    void Update()
    {
        if (!globalLight) return;

        float t = DayRatio();
        float targetIntensity = intensityCurve.Evaluate(t);
        Color targetColor = colorOverDay.Evaluate(t);

        globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, Time.deltaTime * lerpSpeed);
        globalLight.color = Color.Lerp(globalLight.color, targetColor, Time.deltaTime * lerpSpeed);
    }
}
