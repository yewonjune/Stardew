using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class FirebaseLoginManager : MonoBehaviour
{
    public InputField IDInputField;
    public InputField PWInputField;
    public Button CreateID_Btn;
    public Button EmailLogin_Btn;
    public Text MessageText;

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

                if (CreateID_Btn != null)
                {
                    CreateID_Btn.onClick.AddListener(CreateIDBtnClick);
                }

                if (EmailLogin_Btn != null)
                {
                    EmailLogin_Btn.onClick.AddListener(EmailLoginBtnClick);
                }

                if (GuestLogin_Btn != null)
                {
                    GuestLogin_Btn.onClick.AddListener(GuestLoginBtnClick);
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

    void CreateIDBtnClick()
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

        firebaseAuth.CreateUserWithEmailAndPasswordAsync(ID, PW)
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                MessageOnOff("가입 취소");
                return;
            }
            if (task.IsFaulted)
            {
                MessageOnOff("가입 실패");
                return;
            }
            MessageOnOff("가입 성공");
        });
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

    void MessageOnOff(string Mess = "", bool isOn = true)
    {
        if (isOn == true)
        {
            MessageText.text = Mess;
            MessageText.gameObject.SetActive(true);
            ShowMsTimer = MessageDuration;
        }
        else
        {
            MessageText.text = "";
            MessageText.gameObject.SetActive(false);
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
