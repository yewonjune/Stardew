using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCAIDialogueUI : MonoBehaviour
{
    [Header("UI 참조")]
    public GameObject dialogueBox;          // DialogueBox
    public TMP_Text npcNameText;            // NPCNameBox > NPCNameText
    public TMP_Text npcDialogueText;        // NPCNameBox > NPCAIDialogueText
    public Image npcPortrait;               // NPCNameBox > NPCPortrait
    public TMP_Text prevPlayerMessageText;  // PrevPlayerMessageText
    public TMP_InputField inputField;       // InputRow > InputField
    public Button sendButton;               // InputRow > SendButton
    public Button closeButton;              // CloseButton
    public GameObject thinkingIndicator;    // 있으면 연결, 없으면 비워도 됨

    private NPCAIDialogue _currentNPC;
    private NPCData _currentNPCData;

    void Awake()
    {
        sendButton.onClick.AddListener(OnSendClicked);
        closeButton.onClick.AddListener(CloseDialogue);
        inputField.onSubmit.AddListener(_ => OnSendClicked());
        gameObject.SetActive(false);
    }

    // ─────────────────────── Public API ────────────────────────

    public void OpenDialogue(NPCAIDialogue npc, NPCData npcData)
    {
        PlayerActionLock.Lock("AIDialogue");

        _currentNPC = npc;
        _currentNPCData = npcData;

        // 초기화
        inputField.text = "";
        prevPlayerMessageText.text = "";
        npcDialogueText.text = "";
        SetThinking(false);

        if (npcData != null)
        {
            if (npcNameText) npcNameText.text = npcData.displayName;
            if (npcPortrait) npcPortrait.sprite = npcData.GetPortrait(EmotionType.Default);
        }

        _currentNPC.OnNPCResponded.AddListener(OnNPCResponded);
        _currentNPC.OnThinkingStart.AddListener(() => SetThinking(true));
        _currentNPC.OnThinkingEnd.AddListener(() => SetThinking(false));
        _currentNPC.OnErrorOccurred.AddListener(OnError);
        _currentNPC.ResetHistory();

        gameObject.SetActive(true);
        inputField.ActivateInputField();

        // 첫 인사
        npcDialogueText.text = "안녕하세요! 무슨 일로 오셨나요?";
    }

    public void CloseDialogue()
    {
        PlayerActionLock.Unlock("AIDialogue");

        if (_currentNPC != null)
        {
            _currentNPC.OnNPCResponded.RemoveListener(OnNPCResponded);
            _currentNPC.OnThinkingStart.RemoveAllListeners();
            _currentNPC.OnThinkingEnd.RemoveAllListeners();
            _currentNPC.OnErrorOccurred.RemoveListener(OnError);
            _currentNPC = null;
        }
        gameObject.SetActive(false);
    }

    // ─────────────────────── 버튼 이벤트 ───────────────────────

    void OnSendClicked()
    {
        if (_currentNPC == null || _currentNPC.IsBusy) return;
        string msg = inputField.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;

        // 플레이어 입력 표시
        prevPlayerMessageText.text = msg;

        inputField.text = "";
        inputField.ActivateInputField();

        _currentNPC.SendPlayerMessage(msg);
    }

    // ─────────────────────── NPC 이벤트 ────────────────────────

    void OnNPCResponded(string text, EmotionType emotion)
    {
        npcDialogueText.text = text;

        if (npcPortrait && _currentNPCData != null)
            npcPortrait.sprite = _currentNPCData.GetPortrait(emotion);
    }

    void OnError(string msg)
    {
        npcDialogueText.text = msg;
    }

    // ─────────────────────── 유틸 ──────────────────────────────

    void SetThinking(bool on)
    {
        if (thinkingIndicator) thinkingIndicator.SetActive(on);
        sendButton.interactable = !on;
        if (on) npcDialogueText.text = "...";
    }
}