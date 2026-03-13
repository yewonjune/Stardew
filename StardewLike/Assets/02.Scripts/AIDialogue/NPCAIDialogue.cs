using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Text;

/// <summary>
/// NPC GameObject에 부착.
/// Anthropic Claude API를 호출해 플레이어 입력에 맞는 NPC 응답을 생성합니다.
/// </summary>
/// 
public class NPCAIDialogue : MonoBehaviour
{
    [Header("필수 설정")]
    public NPCAIPersonality personality;
    public NPCData npcData;

    [Header("연동 (자동 탐색 가능)")]
    public NPCAffection affection;

    [Header("이벤트")]
    public UnityEvent<string, EmotionType> OnNPCResponded;   // 응답 텍스트, 감정
    public UnityEvent OnThinkingStart;
    public UnityEvent OnThinkingEnd;
    public UnityEvent<string> OnErrorOccurred;

    private readonly List<(string role, string content)> _history = new();
    private bool _isBusy = false;
    public bool IsBusy => _isBusy;

    void Awake()
    {
        if (affection == null)
            affection = GetComponent<NPCAffection>();
    }

    /// <summary>NPC에게 다가갈 때 호출 — 대화 기록 초기화</summary>
    public void ResetHistory() => _history.Clear();

    /// <summary>플레이어 메시지 전송</summary>
    public void SendPlayerMessage(string playerInput)
    {
        if (_isBusy || string.IsNullOrWhiteSpace(playerInput)) return;
        StartCoroutine(CoRequest(playerInput.Trim()));
    }

    IEnumerator CoRequest(string playerInput)
    {
        _isBusy = true;
        OnThinkingStart?.Invoke();

        _history.Add(("user", playerInput));

        string requestJson = BuildRequestJson();
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestJson);

        var uwr = new UnityEngine.Networking.UnityWebRequest(
            "https://api.anthropic.com/v1/messages", "POST");
        uwr.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
        uwr.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("x-api-key", NPCAIConfig.Instance.ApiKey);
        uwr.SetRequestHeader("anthropic-version", "2023-06-01");

        yield return uwr.SendWebRequest();

        _isBusy = false;
        OnThinkingEnd?.Invoke();

        if (uwr.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[NPCAIDialogue] 오류: {uwr.error}");
            // 실패한 user 메시지 제거
            if (_history.Count > 0 && _history[^1].role == "user")
                _history.RemoveAt(_history.Count - 1);
            OnErrorOccurred?.Invoke("잠깐 연결이 끊겼어요. 다시 말해줘요!");
            yield break;
        }

        string responseText = ParseResponse(uwr.downloadHandler.text);

        // 토큰 사용량 측정
        string inputTokens = ExtractJson(uwr.downloadHandler.text, "input_tokens");
        string outputTokens = ExtractJson(uwr.downloadHandler.text, "output_tokens");
        Debug.Log($"[토큰 사용량] 입력: {inputTokens} / 출력: {outputTokens}");

        if (string.IsNullOrEmpty(responseText))
        {
            OnErrorOccurred?.Invoke("응답을 받지 못했어요.");
            yield break;
        }

        _history.Add(("assistant", responseText));
        TrimHistory();

        EmotionType emotion = DetectEmotion(responseText);
        OnNPCResponded?.Invoke(responseText, emotion);
    }

    string BuildRequestJson()
    {
        int currentAff = affection != null ? affection.currentAffection : 0;
        string affStyle = personality != null ? personality.GetAffectionStyle(currentAff) : "";
        string npcName = personality != null ? personality.npcDisplayName
                                                   : (npcData != null ? npcData.displayName : "NPC");
        string personalityDesc = personality != null ? personality.personalityPrompt
                                                     : $"당신은 {npcName}입니다.";

        var sys = new StringBuilder();
        sys.AppendLine(personalityDesc);
        sys.AppendLine($"현재 플레이어와의 호감도: {currentAff}점.");
        sys.AppendLine($"말투 지침: {affStyle}");
        sys.AppendLine("답변은 2~3문장 이내로 자연스럽게 말하세요.");
        sys.AppendLine("게임 NPC처럼 자신의 이름이나 역할을 먼저 밝히지 말고 자연스럽게 대화하세요.");

        var msgs = new StringBuilder("[");
        for (int i = 0; i < _history.Count; i++)
        {
            if (i > 0) msgs.Append(",");
            msgs.Append($"{{\"role\":\"{_history[i].role}\",\"content\":{JsonEscape(_history[i].content)}}}");
        }
        msgs.Append("]");

        int maxTok = personality != null ? personality.maxTokens : 150;

        return $"{{" +
               $"\"model\":\"claude-sonnet-4-20250514\"," +
               $"\"max_tokens\":{maxTok}," +
               $"\"system\":{JsonEscape(sys.ToString())}," +
               $"\"messages\":{msgs}" +
               $"}}";
    }

    string ParseResponse(string json)
    {
        const string marker = "\"text\":\"";
        int start = json.IndexOf(marker);
        if (start < 0) return null;
        start += marker.Length;

        var sb = new StringBuilder();
        for (int i = start; i < json.Length; i++)
        {
            char c = json[i];
            if (c == '"' && json[i - 1] != '\\') break;

            if (c == '\\' && i + 1 < json.Length)
            {
                char next = json[++i];
                switch (next)
                {
                    case 'n': sb.Append('\n'); break;
                    case 't': sb.Append('\t'); break;
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    default: sb.Append(c); break;
                }
            }
            else sb.Append(c);
        }
        return sb.ToString().Trim();
    }


    EmotionType DetectEmotion(string text)
    {
        if (personality?.emotionKeywords == null) return EmotionType.Default;
        string lower = text.ToLower();
        foreach (var ek in personality.emotionKeywords)
            foreach (var kw in ek.keywords)
                if (lower.Contains(kw.ToLower())) return ek.emotion;
        return EmotionType.Default;
    }

    void TrimHistory()
    {
        int max = (personality != null ? personality.maxHistoryTurns : 6) * 2;
        while (_history.Count > max)
            _history.RemoveAt(0);
    }

    static string JsonEscape(string s)
    {
        if (s == null) return "\"\"";
        var sb = new StringBuilder("\"");
        foreach (char c in s)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default: sb.Append(c); break;
            }
        }
        return sb.Append('"').ToString();
    }

    string ExtractJson(string json, string key)
    {
        string marker = $"\"{key}\":";
        int start = json.IndexOf(marker);
        if (start < 0) return "?";
        start += marker.Length;
        int end = json.IndexOfAny(new char[] { ',', '}' }, start);
        return json.Substring(start, end - start).Trim();
    }
}
