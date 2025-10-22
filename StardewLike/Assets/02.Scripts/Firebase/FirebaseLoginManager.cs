using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using DG.Tweening.Plugins.Options;
using System;

public class FirebaseLoginManager : MonoBehaviour
{
    public InputField IDInputField;
    public InputField PWInputField;
    public Button EmailLogin_Btn;
    public Button OpenCreateID_Btn;
    public Text MessageText;

    [Header("--- Create ID ---")]
    public GameObject CreateIDPanel;
    public InputField New_IDInputField;
    public InputField New_PWInputField;
    public InputField New_NickInputField;
    public Button CreateAccountBtn;
    public Button CancelBtn;
   // public Button m_CancelBtn;

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
        if (CreateIDPanel != null)
        {
            CreateIDPanel.SetActive(true);
        }

        //if (!TryReadInputs(out ID, out PW))
        //    return;
        //if (firebaseAuth == null)
        //{
        //    MessageOnOff("초기화 중입니다. 잠시 후 다시 시도하세요.");
        //    return;
        //}

        //if (isBusy)
        //    return;

        //isBusy = true;

        //firebaseAuth.CreateUserWithEmailAndPasswordAsync(ID, PW)
        //.ContinueWithOnMainThread(task =>
        //{
        //    if (task.IsCanceled)
        //    {
        //        MessageOnOff("가입 취소");
        //        return;
        //    }
        //    if (task.IsFaulted)
        //    {
        //        MessageOnOff("가입 실패");
        //        return;
        //    }
        //    MessageOnOff("가입 성공");
        //});
    }

    void CancelBtnClick()
    {
        if (CreateIDPanel != null)
            CreateIDPanel.SetActive(false);

        New_IDInputField.text = "";
        New_PWInputField.text = "";
        New_NickInputField.text = "";
    }
    //async void CreateIDBtnClick()
    //{
    //    if (NetworkMgr.g_fAuth == null)
    //    {
    //        MessageOnOff("초기화 중입니다. 잠시 후 다시 시도하세요.");
    //        return;
    //    }

    //    string a_IdStr = New_IDInputField.text;
    //    string a_PwStr = New_PWInputField.text;
    //    string a_NickStr = New_NickInputField.text;

    //    a_IdStr = a_IdStr.Trim();
    //    a_PwStr = a_PwStr.Trim();
    //    a_NickStr = a_NickStr.Trim();

    //    if (string.IsNullOrEmpty(a_IdStr) == true ||
    //        string.IsNullOrEmpty(a_PwStr) == true ||
    //        string.IsNullOrEmpty(a_NickStr) == true)
    //    {
    //        MessageOnOff("Id, Pw, 별명은 빈칸 없이 입력해 주세요.");
    //        return;
    //    }

    //    if (!(6 <= a_IdStr.Length && a_IdStr.Length <= 20))  // 6 ~ 20
    //    {
    //        MessageOnOff("Id는 6글자부터 20글자까지 작성해 주세요.");
    //        return;
    //    }

    //    if (!(6 <= a_PwStr.Length && a_PwStr.Length <= 20))
    //    {
    //        MessageOnOff("비밀번호는 6글자부터 20글자까지 작성해 주세요.");
    //        return;
    //    }

    //    if (!(3 <= a_NickStr.Length && a_NickStr.Length <= 20))
    //    {
    //        MessageOnOff("별명은 3글자부터 20글자까지 작성해 주세요.");
    //        return;
    //    }

    //    if (!CheckEmailAddress(a_IdStr))
    //    {
    //        MessageOnOff("Email 형식이 맞지 않습니다.");
    //        return;
    //    }

    //    m_SvNewIdStr = a_IdStr;
    //    m_SvNewPwStr = a_PwStr;

    //    try
    //    {
    //        //1) 닉네임 중복 확인
    //        QuerySnapshot checkSnapshot = await NetworkMgr.g_fsRef
    //                                                .Collection("users")
    //                                                .WhereEqualTo("NickName", a_NickStr)
    //                                                .GetSnapshotAsync();

    //        if (checkSnapshot != null && checkSnapshot.Count > 0)
    //        {
    //            MessageOnOff("이미 존재하는 별명입니다.");
    //            return;
    //        }

    //        MessageOnOff("회원 가입 중... 잠시만 기다려 주세요.");
    //        ShowMsTimer = 300.0f;

    //        //2) 계정 생성
    //        var signUpResult =
    //            await NetworkMgr.g_fAuth.CreateUserWithEmailAndPasswordAsync(a_IdStr, a_PwStr);

    //        var newUser = signUpResult.User;
    //        string uid = newUser.UserId;

    //        //----- 3) 닉네임 저장
    //        Dictionary<string, object> userData = new Dictionary<string, object>()
    //        {
    //            { "NickName", a_NickStr },
    //            { "Email", a_IdStr },
    //        };

    //        await NetworkMgr.g_fsRef
    //                    .Collection("users")
    //        .Document(uid)
    //                    .SetAsync(userData, SetOptions.MergeAll);
    //        //----- 닉네임 저장

    //        //----- 체험 스킬 1개씩 주기
    //        Dictionary<string, object> rsData = new Dictionary<string, object>();
    //        rsData.Add("Gold", 0);
    //        for (int i = 0; i < (int)SkillType.SkCount; i++)
    //        {
    //            string key = ((SkillType)i).ToString(); //$"Skill_{i}";
    //            rsData.Add(key, 1);
    //        }//for (int i = 0; i < (int)SkillType.SkCount; i++)

    //        await NetworkMgr.g_RtdbRoot
    //                        .Child("users")
    //                        .Child(uid)
    //                        .UpdateChildrenAsync(rsData);  // 값 업데이트
    //        //----- 체험 스킬 1개씩 주기

    //        IdInputField.text = m_SvNewIdStr;
    //        PassInputField.text = m_SvNewPwStr;

    //        MessageOnOff("가입 성공");

    //    }
    //    catch (FirebaseException fe)
    //    {
    //        // Firebase 에러 코드 분기
    //        switch ((AuthError)fe.ErrorCode)
    //        {
    //            case AuthError.EmailAlreadyInUse:
    //                MessageOnOff("이미 존재하는 이메일(ID)입니다.");
    //                break;
    //            case AuthError.InvalidEmail:
    //                MessageOnOff("이메일 형식이 잘못되었습니다.");
    //                break;
    //            case AuthError.WeakPassword:
    //                MessageOnOff("비밀번호가 너무 약합니다. 더 강력한 비밀번호를 입력하세요.");
    //                break;
    //            default:
    //                MessageOnOff("가입 실패: " + fe.Message);
    //                break;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        // 일반적인 예외 (네트워크 오류 등)
    //        //Debug.Log("회원가입 처리 중 오류: " + ex.Message);
    //        MessageOnOff("가입 실패: " + ex.Message);
    //    }

    //}

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
