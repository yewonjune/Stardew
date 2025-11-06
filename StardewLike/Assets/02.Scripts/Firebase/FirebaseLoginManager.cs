using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public class FirebaseLoginManager : MonoBehaviour
{
    public InputField IDInputField;
    public InputField PWInputField;
    public Button EmailLogin_Btn;
    public Button OpenCreateID_Btn;
    public Text MessageText;
    public Text LoginMessageText;

    [Header("--- Create ID ---")]
    public GameObject CreateIDPanel;
    public InputField New_IDInputField;
    public InputField New_PWInputField;
    public InputField New_NickInputField;
    public Button CreateAccountBtn;
    public Button CancelBtn;
    public Text CreateIDMessageText;

    [Header("--- Guest Login ---")]
    public Button GuestLogin_Btn;
    private bool isInitialized = false;
    private bool isBusy = false;

   // public Button GoogleLogin_Btn;

    private FirebaseAuth firebaseAuth;

    private bool didStartScene = false;

    string ID;
    string PW;

    float ShowMsTimer = 0.0f;
    const float MessageDuration = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                firebaseAuth = FirebaseAuth.DefaultInstance;

                if (OpenCreateID_Btn != null)
                {
                    OpenCreateID_Btn.onClick.AddListener(OpenCreateIDBtnClick);
                }

                if (CancelBtn != null)
                {
                    CancelBtn.onClick.AddListener(CancelBtnClick);
                }

                if (EmailLogin_Btn != null)
                {
                    EmailLogin_Btn.onClick.AddListener(EmailLoginBtnClick);
                }

                if (GuestLogin_Btn != null)
                {
                    GuestLogin_Btn.onClick.AddListener(GuestLoginBtnClick);
                }

                if (CreateAccountBtn != null)
                {
                    CreateAccountBtn.onClick.AddListener(CreateAccountBtnClick);
                }
                isInitialized = true;

                MessageOnOff("Firebase 준비 완료");
            }
            else
            {
                Debug.LogError("Firebase 초기화 실패: " + dependencyStatus);
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (0.0f < ShowMsTimer)
        {
            ShowMsTimer -= Time.deltaTime;
            if (ShowMsTimer <= 0.0f)
            {
                MessageOnOff("", false);
            }
        }
    }

    void OpenCreateIDBtnClick()
    {
        MessageOnOff("", false);

        if (CreateIDPanel != null)
        {
            CreateIDPanel.SetActive(true);
        }
    }

    void CancelBtnClick()
    {
        MessageOnOff("", false);

        if (CreateIDPanel != null)
            CreateIDPanel.SetActive(false);

        New_IDInputField.text = "";
        New_PWInputField.text = "";
        New_NickInputField.text = "";
    }
   
    void EmailLoginBtnClick()
    {
        if (!TryReadInputs(out ID, out PW))
            return;

        if (firebaseAuth == null)
        {
            MessageOnOff("초기화 중입니다. 잠시 후 다시 시도하세요.");
            return;
        }
        
        if (isBusy)
            return;
        
        isBusy = true;

        firebaseAuth.SignInWithEmailAndPasswordAsync(ID, PW).ContinueWithOnMainThread(task =>
        {
            isBusy = false;

            if (task.IsCanceled)
            {
                MessageOnOff("로그인 취소");
                return;
            }
            if (task.IsFaulted)
            {
                MessageOnOff("로그인 실패");
                return;
            }
            MessageOnOff("로그인 성공");

            StartSceneOnce();
        });
    }
    void GuestLoginBtnClick()
    {
        if (!isInitialized || firebaseAuth == null)
        {
            MessageOnOff("초기화 중입니다. 잠시 후 다시 시도하세요.");
            return;
        }
        if (isBusy)
            return;
        isBusy = true;
        firebaseAuth.SignInAnonymouslyAsync().ContinueWithOnMainThread
            (task =>
            {
                isBusy = false;
                if (task.IsCanceled)
                {
                    MessageOnOff("게스트 로그인 취소");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError(task.Exception);
                    MessageOnOff("게스트 로그인 실패");
                    return;
                }
                FirebaseUser newUser = task.Result.User;
                if (newUser != null)
                {
                    MessageOnOff("게스트 로그인 성공 : " + newUser.UserId);

                    StartSceneOnce();
                }
                else
                {
                    MessageOnOff("게스트 로그인 성공 (User 정보 없음)");

                    StartSceneOnce();
                }
            });
    }
    bool TryReadNewInputs(out string email, out string pw, out string nick)
    {
        email = (New_IDInputField != null) ? New_IDInputField.text.Trim() : "";
        pw = (New_PWInputField != null) ? New_PWInputField.text.Trim() : "";
        nick = (New_NickInputField != null) ? New_NickInputField.text.Trim() : "";

        if (string.IsNullOrEmpty(email))
        {
            MessageOnOff("이메일을 입력하세요.");
            return false;
        }
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            MessageOnOff("이메일 형식이 올바르지 않습니다.");
            return false;
        }
        if (string.IsNullOrEmpty(pw) || pw.Length < 6)
        {
            MessageOnOff("비밀번호는 6자 이상 입력하세요.");
            return false;
        }
        if (string.IsNullOrEmpty(nick) || nick.Length < 2)
        {
            MessageOnOff("닉네임을 2자 이상 입력하세요.");
            return false;
        }
        return true;
    }

    void CreateAccountBtnClick()
    {
        if (!isInitialized || firebaseAuth == null)
        {
            MessageOnOff("초기화 중입니다. 잠시 후 다시 시도하세요.");
            return;
        }
        if (isBusy) return;

        if (!TryReadNewInputs(out string newEmail, out string newPw, out string newNick))
            return;

        isBusy = true;
        MessageOnOff("계정 생성 중...");

        firebaseAuth.CreateUserWithEmailAndPasswordAsync(newEmail, newPw)
            .ContinueWithOnMainThread(task =>
            {
                isBusy = false;

                if (task.IsCanceled)
                {
                    MessageOnOff("가입 취소");
                    return;
                }
                if (task.IsFaulted)
                {
                    var fe = task.Exception?.Flatten()?.InnerExceptions
                                .OfType<FirebaseException>().FirstOrDefault();
                    if (fe != null)
                    {
                        switch ((AuthError)fe.ErrorCode)
                        {
                            case AuthError.EmailAlreadyInUse:
                                MessageOnOff("이미 존재하는 이메일(ID)입니다.");
                                return;
                            case AuthError.InvalidEmail:
                                MessageOnOff("이메일 형식이 잘못되었습니다.");
                                return;
                            case AuthError.WeakPassword:
                                MessageOnOff("비밀번호가 너무 약합니다.");
                                return;
                        }
                        MessageOnOff("가입 실패: " + fe.Message);
                        return;
                    }
                    MessageOnOff("가입 실패: 알 수 없는 오류");
                    return;
                }

                var user = task.Result.User;

                // 닉네임(표시 이름) 반영 - 선택 사항
                if (user != null && !string.IsNullOrEmpty(newNick))
                {
                    var profile = new UserProfile { DisplayName = newNick };
                    user.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(_ => { });

                    if (PlayerRankManager.Instance != null)
                    {
                        PlayerRankManager.Instance.SaveNickname(newNick);
                    }
                }

                // 로그인창에 자동 채움 & 패널 닫기
                if (IDInputField) IDInputField.text = newEmail;
                if (PWInputField) PWInputField.text = newPw;

                if (CreateIDPanel) CreateIDPanel.SetActive(false);
                if (New_IDInputField) New_IDInputField.text = "";
                if (New_PWInputField) New_PWInputField.text = "";
                if (New_NickInputField) New_NickInputField.text = "";

                MessageOnOff("가입 성공! 로그인 버튼을 눌러주세요.");
            });
    }
    Text ResolveTargetMessageText()
    {
        // CreateIDPanel이 켜져있으면 Create 패널쪽 우선
        if (CreateIDPanel != null && CreateIDPanel.activeSelf)
        {
            if (CreateIDMessageText != null) return CreateIDMessageText;
            // 패널 전용 Text가 없으면 공용으로 폴백
            if (MessageText != null) return MessageText;
        }
        else
        {
            // 로그인 패널 쪽
            if (LoginMessageText != null) return LoginMessageText;
            if (MessageText != null) return MessageText;
        }

        return null; // 모두 없으면 null
    }

    Text _currentMsgTarget;  // 현재 표시 중인 대상(타이머로 끌 때 사용)

    // 기존 함수 대체
    void MessageOnOff(string Mess = "", bool isOn = true)
    {
        if (isOn)
        {
            _currentMsgTarget = ResolveTargetMessageText();

            if (_currentMsgTarget != null)
            {
                _currentMsgTarget.text = Mess;
                _currentMsgTarget.gameObject.SetActive(true);
            }
            // 공용(기존) MessageText만 쓰는 경우도 대비
            if (_currentMsgTarget == null && MessageText != null)
            {
                MessageText.text = Mess;
                MessageText.gameObject.SetActive(true);
                _currentMsgTarget = MessageText;
            }

            ShowMsTimer = MessageDuration;
        }
        else
        {
            // 마지막으로 켠 대상만 끔
            if (_currentMsgTarget != null)
            {
                _currentMsgTarget.text = "";
                _currentMsgTarget.gameObject.SetActive(false);
            }
            else if (MessageText != null)
            {
                MessageText.text = "";
                MessageText.gameObject.SetActive(false);
            }
            ShowMsTimer = 0.0f;
        }
    }

    bool TryReadInputs(out string id, out string pw)
    {
        id = (IDInputField != null) ? IDInputField.text.Trim() : "";
        pw = (PWInputField != null) ? PWInputField.text.Trim() : "";
        if (string.IsNullOrEmpty(id))
        {
            MessageOnOff("이메일을 입력하세요.");
            return false;
        }
        if (string.IsNullOrEmpty(pw))
        {
            MessageOnOff("비밀번호를 입력하세요.");
            return false;
        }
        return true;
    }

    void StartSceneOnce()
    {
        if (didStartScene)
            return;

        didStartScene = true;
        SceneManager.LoadScene("StartScene");
    }

}
