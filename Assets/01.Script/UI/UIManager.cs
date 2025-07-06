using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIManager : UIBase
{
    public static UIManager Instance { get { return AppManager.Instance.uiManager; } }
    
    [SerializeField]
    private Canvas m_ForeCanvas;
    public Canvas foreCanvas { get { return m_ForeCanvas; } }

    [SerializeField]
    private Canvas m_MidCanvas;
    public Canvas midCanvas { get { return m_MidCanvas; } }

    [SerializeField]
    private Canvas m_BackCanvas;
    public Canvas backCanvas { get { return m_BackCanvas; } }
    
    [SerializeField]
    private UIManager_Common_NoticeUI m_NoticeUI;
    public UIManager_Common_NoticeUI noticeUI { get { return m_NoticeUI; } }

    [SerializeField]
    private UIManager_Common_TakeRewardUI m_TakeRewardUI;
    public UIManager_Common_TakeRewardUI takeRewardUI { get { return m_TakeRewardUI; } }

    [SerializeField]
    private CustomButton m_SettingBtn;
    public CustomButton settingBtn { get { return m_SettingBtn; } }
    
    [SerializeField]
    private CommonUI_UserResources m_UserResourcesUI;
    public CommonUI_UserResources userResourcesUI { get { return m_UserResourcesUI; } }

    [SerializeField]
    private CommonUI_MessageBox m_MessageBoxUI;
    public CommonUI_MessageBox messageBoxUI { get { return m_MessageBoxUI; } }

    [SerializeField]
    private CommonUI_Setting m_SettingUI;
    public CommonUI_Setting settingUI { get { return m_SettingUI; } }

    [SerializeField]
    private CommonUI_Hand m_HandUI;
    public CommonUI_Hand handUI => m_HandUI;

    [SerializeField]
    private RectTransform m_LockPannelRtf;
    public RectTransform lockPannelRtf => m_LockPannelRtf;

    private void Awake()
    {
        settingBtn.OnClick += () => { settingUI.Open(); };
    }

    public void Initialize()
    {
        userResourcesUI.SetData(UserInfo.Instance.starCount);
    }

    public void Release()
    {

    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ShowNoticeUI("아무말 대잔치 LEVEL 2 !!!", Random.Range(2, 5));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UserInfo.Instance.AddStarCount(10, true, foreCanvas.transform.position);
            UserInfo.Instance.Save();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            UserInfo.Instance.AddStarCount(70, true, foreCanvas.transform.position);
            UserInfo.Instance.Save();
        }
#endif
    }
    
    public void ShowNoticeUI(string message, float duration = 1f)
    {
        noticeUI.SetData(message, duration);
        noticeUI.Open();
    }

    public void ShowNoticeUIByTexTNo(string textNo, float duration = 1f)
    {
        noticeUI.SetData(UITextManager.GetText(textNo), duration);
        noticeUI.Open();
    }

    public void ShowTakeStarParticle(Vector3 worldPos, int starCount)
    {
        int bigParticleCnt = starCount / 5;
        int smallParticleCnt = (starCount % 5);

        Fx_ParticleTarget fx;
        if (smallParticleCnt > 0 && SpawnMaster.TrySpawnFx("Fx_TakeStarParticle", worldPos, Quaternion.identity, out fx))
        {
            fx.mainPs.emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst() { count = 1, maxCount = 1, minCount = 1, cycleCount = smallParticleCnt, probability = 1f, time = 0f, repeatInterval = 0.05f }
            });

            var psMain = fx.mainPs.main;
            psMain.maxParticles = smallParticleCnt;
            psMain.startSize = 0.6f;

            fx.SetFx
                (userResourcesUI.starIconImg.gameObject
                , false
                , () => { return !IngameManager.Instance.gameObject.activeInHierarchy; }
                , (des) => {
                    userResourcesUI.AddData(1);
                });

            fx.On(() =>
            {
                //userResourcesUI.SetData(UserInfo.Instance.starCount);
            });
        }
        
        if (bigParticleCnt > 0 && SpawnMaster.TrySpawnFx("Fx_TakeStarParticle", worldPos, Quaternion.identity, out fx))
        {
            fx.mainPs.emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst() { count = 1, maxCount = 1, minCount = 1, cycleCount = bigParticleCnt, probability = 1f, time = 0f, repeatInterval = 0.05f }
            });

            var psMain = fx.mainPs.main;
            psMain.maxParticles = bigParticleCnt;
            psMain.startSize = 1.2f;

            fx.SetFx
                (userResourcesUI.starIconImg.gameObject
                , false
                , () => { return !IngameManager.Instance.gameObject.activeInHierarchy; }
                , (des) => {
                    userResourcesUI.AddData(5);
                });

            fx.On(() =>
            {
                //userResourcesUI.SetData(UserInfo.Instance.starCount);
                //UserInfo.Instance.Save();
            });
        }
    }

    public void ShowCompleteBuyProduct(string productID)
    {
        ShowMessageBoxUI
            ( UITextManager.GetText("알림")
            , UITextManager.GetText("00037")
            , UITextManager.GetText("확인")
            , Instance.messageBoxUI.Close);
    }
    
    public void ShowFailureBuyProduct(string productID)
    {
        ShowMessageBoxUI
            (UITextManager.GetText("알림")
            , UITextManager.GetText("00038")
            , UITextManager.GetText("확인")
            , Instance.messageBoxUI.Close);
    }

    public static CommonUI_MessageBox GetMessageBoxUI()
    {
        return Instance.messageBoxUI;
    }

    public static void ShowMessageBoxUI(string title, string message, string yesBtnLabel, UnityAction onClickYesBtn, string noBtnLabel = null, UnityAction onClickNoBtn = null)
    {
        Instance.messageBoxUI.SetData
                    (title
                    , message
                    , yesBtnLabel
                    , onClickYesBtn
                    , noBtnLabel
                    , onClickNoBtn);
        Instance.messageBoxUI.Open();

    }

    public static void OpenLockPannel()
    {
        Instance.lockPannelRtf.gameObject.SetActive(true);
    }

    public static void CloseLockPannel()
    {
        Instance.lockPannelRtf.gameObject.SetActive(false);
    }
}
