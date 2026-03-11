using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Create > NPC > AI Personality 로 생성
/// NPC마다 하나씩 만들어서 NPCAIDialogue에 연결하세요.
/// </summary>

[CreateAssetMenu(menuName = "NPC/AI Personality")]
public class NPCAIPersonality : ScriptableObject
{
    [Header("NPC 기본 정보")]
    [Tooltip("NPCData의 npcId와 동일하게 입력")]
    public string npcId;
    public string npcDisplayName = "미나";

    [Header("성격 설정")]
    [TextArea(3, 6)]
    public string personalityPrompt =
       "당신은 마을의 꽃집을 운영하는 미나입니다. " +
       "밝고 친절하며 꽃과 자연을 사랑합니다. " +
       "항상 따뜻하게 대화하며 짧고 자연스럽게 답변합니다.";

    [Header("호감도별 말투 (NPCAffection 연동)")]
    [TextArea(2, 4)]
    public string lowAffectionStyle = "아직 친하지 않아서 다소 조심스럽고 격식 있게 말합니다.";
    [TextArea(2, 4)]
    public string midAffectionStyle = "어느 정도 친해져서 편하고 친근하게 말합니다.";
    [TextArea(2, 4)]
    public string highAffectionStyle = "매우 친한 친구처럼 애정이 담기고 편하게 말합니다.";

    [Header("호감도 구간 기준")]
    public int lowAffectionThreshold = 30;
    public int highAffectionThreshold = 70;

    [Header("대화 제한")]
    [Tooltip("AI에게 전달하는 최근 대화 기록 수 (토큰 절약)")]
    public int maxHistoryTurns = 6;
    [Tooltip("AI 응답 최대 토큰")]
    public int maxTokens = 150;

    [Header("감정 감지 키워드")]
    public EmotionKeywords[] emotionKeywords;

    public string GetAffectionStyle(int affection)
    {
        if (affection >= highAffectionThreshold) return highAffectionStyle;
        if (affection >= lowAffectionThreshold) return midAffectionStyle;
        return lowAffectionStyle;
    }
}

[System.Serializable]
public class EmotionKeywords
{
    public EmotionType emotion;
    [Tooltip("AI 응답에 이 키워드가 포함되면 해당 감정 초상화로 전환")]
    public string[] keywords;
}

