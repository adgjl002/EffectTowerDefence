using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager_Lobby : UIBase {
    
    public static UIManager_Lobby Instance { get { return AppManager.Instance.lobbyUIManager; } }

    [SerializeField]
    private CustomButton m_GameStartBtn;
    public CustomButton gameStartBtn { get { return m_GameStartBtn; } }

    [SerializeField]
    private CustomButton m_TimeRewardBtn;
    public CustomButton timeRewardBtn => m_TimeRewardBtn; 

    [SerializeField]
    private CustomButton m_ShopBtn;
    public CustomButton shopBtn { get { return m_ShopBtn; } }

    [SerializeField]
    private CustomButton m_SkillMapBtn;
    public CustomButton skillMapBtn { get { return m_SkillMapBtn; } }

    [SerializeField]
    private CustomButton m_RankingBtn;
    public CustomButton rankingBtn => m_RankingBtn;

    [SerializeField]
    private CustomButton m_CommunityBtn;
    public CustomButton communityBtn => m_CommunityBtn;

    [SerializeField]
    private CustomButton m_StageChangeLeftBtn;
    public CustomButton stageChangeLeftBtn { get { return m_StageChangeLeftBtn; } }

    [SerializeField]
    private CustomButton m_StageChangeRightBtn;
    public CustomButton stageChangeRightBtn { get { return m_StageChangeRightBtn; } }

    [SerializeField]
    private RectTransform m_StageDirectionUIRtf;
    public RectTransform stageDirectionUIRtf { get { return m_StageDirectionUIRtf; } }

    [SerializeField]
    private List<Image> m_StageDirectionImages;
    public List<Image> stageDirectionImages { get { return m_StageDirectionImages; } }

    [SerializeField]
    private TextMeshProUGUI m_StageTitleTxt;
    public TextMeshProUGUI stageTitleTxt { get { return m_StageTitleTxt; } }

    [SerializeField]
    private CustomButton m_StageInfoBtn;
    public CustomButton stageInfoBtn => m_StageInfoBtn;

    [SerializeField]
    private RectTransform m_LockUIRtf;
    public RectTransform lockUIRtf { get { return m_LockUIRtf; } }

    [SerializeField]
    private TextMeshProUGUI m_ModeNameTxt;
    public TextMeshProUGUI modeNameTxt => m_ModeNameTxt;

    [SerializeField]
    private List<Image> m_StageStarImaes;
    public List<Image> stageStarImages { get { return m_StageStarImaes; } }
    
    [SerializeField]
    private LobbyUI_SkillMap m_SkillMapUI;
    public LobbyUI_SkillMap skillMapUI { get { return m_SkillMapUI; } }

    [SerializeField]
    private LobbyUI_Shop m_ShopUI;
    public LobbyUI_Shop shopUI => m_ShopUI;
    
    private int stageChangeBtnClickCnt;
    private float stageChangeBtnClickCntTimer;
    public float stageChangeBtnClickCntDelay = 0.8f;

    private float timer;

    private void Awake()
    {
        gameStartBtn.OnClick += OnClickGameStartBtn;
        shopBtn.OnClick += OnClickShopBtn;
        skillMapBtn.OnClick += OnClickSkillMapBtn;
        rankingBtn.OnClick += OnClickRankingBtn;
        communityBtn.OnClick += OnClickCommunityBtn;

        stageChangeLeftBtn.OnClick += OnClickStageChangeLeftBtn;
        stageChangeRightBtn.OnClick += OnClickStageChangeRightBtn;
        stageInfoBtn.OnClick += OnClickStageInfoBtn;

        timeRewardBtn.OnClick += OnClickTimeRewardBtn;
    }

    public void Initialize()
    {

    }

    public override void Open()
    {
        base.Open();

        var stageCnt = DataManager.Instance.GetStageCount();
        for(int i = 0; i< stageCnt; ++i)
        {
            if(stageDirectionImages.Count < i+1)
            {
                var newImg = Instantiate(stageDirectionImages[0], stageDirectionUIRtf);
                stageDirectionImages.Add(newImg);
            }

            Sprite sprite;
            if (!ResourceManager.Instance.TryGetSprite("Icon_StageDirectionIcon_Disabled", out sprite))
            {
                sprite = null;
            }
            stageDirectionImages[i].sprite = sprite;
        }

        stageChangeBtnClickCnt = 0;
        stageChangeBtnClickCntTimer = stageChangeBtnClickCntDelay;

        UpdateStageUI();

        timer = 1f;
        UpdateRemainTime();
    }

    public void UpdateRemainTime()
    {
        System.DateTime lastRewardedDateTime;

        // 로컬에 저장된 시간확인 체크
        var nowDateTime = System.DateTime.Now;

        if (System.DateTime.TryParse(UserInfo.Instance.lastRewardedTime, out lastRewardedDateTime))
        {
            var dateTime = nowDateTime - lastRewardedDateTime;
            //Debug.LogFormat("DATETIME >> {0} / Last({1}) / Now({2})", dateTime.TotalMinutes, lastRewardedDateTime.ToString(TimeManager.TimeFormat), nowDateTime.ToString(TimeManager.TimeFormat));

            int remainMinutes = Mathf.Max(0, GameSettingsManager.TimeAdsRewardTurm - (int)dateTime.TotalMinutes);
            if (remainMinutes == 0)
            {
                timeRewardBtn.label.text = UITextManager.GetText("보상받기");
                timeRewardBtn.image.material = null;
            }
            else
            {
                timeRewardBtn.label.text = string.Format("{0}h{1}m", remainMinutes / 60, remainMinutes % 60);
                timeRewardBtn.image.material = ResourceManager.GetSpriteGrayScaleMaterial();
            }
        }
        else
        {
            // 첫 보상
            timeRewardBtn.label.text = UITextManager.GetText("보상받기");
            timeRewardBtn.image.material = null;
        }
    }

    public void OnClickTimeRewardBtn()
    {
        System.DateTime lastRewardedDateTime;

        // 로컬에 저장된 시간확인 체크
        var nowDateTime = TimeManager.GetNetworkTime();

        if (System.DateTime.TryParse(UserInfo.Instance.lastRewardedTime, out lastRewardedDateTime))
        {
            var dateTime = nowDateTime - lastRewardedDateTime;
            //Debug.LogFormat("DATETIME >> {0} / Last({1}) / Now({2})", dateTime.TotalMinutes, lastRewardedDateTime.ToString(TimeManager.TimeFormat), nowDateTime.ToString(TimeManager.TimeFormat));

            if (dateTime.TotalMinutes >= GameSettingsManager.TimeAdsRewardTurm)
            {
                // 광고 재생 후 보상 지급
                AdsManager.Instance.ShowRewardedVideoAds((r) =>
                {
                    UnityEngine.Analytics.AnalyticsEvent.Custom("ads_video_end_2", new Dictionary<string, object>
                    {
                        { "time_reward" , r.ToString() }
                    });

                    if (r == UnityEngine.Advertisements.ShowResult.Finished)
                    {
                        // 보상 지급 후 시간 저장
                        UserInfo.Instance.lastRewardedTime = nowDateTime.ToString(TimeManager.TimeFormat);
                        UserInfo.Instance.AddStarCount(40, true, UIManager.Instance.foreCanvas.transform.position);
                        UserInfo.Instance.Save();
                    }
                    else if(r == UnityEngine.Advertisements.ShowResult.Failed)
                    {
                        // 광고가 준비되지 않았습니다.
                        UIManager.ShowMessageBoxUI
                            (UITextManager.GetText("알림")
                            , UITextManager.GetText("00045")
                            , UITextManager.GetText("확인")
                            , UIManager.GetMessageBoxUI().Close);
                    }
                    UpdateRemainTime();
                });
            }
            else
            {
                // 아직 보상을 받을 수 없습니다.
                UIManager.ShowMessageBoxUI
                    (UITextManager.GetText("알림")
                    , UITextManager.GetText("00044")
                    , UITextManager.GetText("확인")
                    , UIManager.GetMessageBoxUI().Close);
            }
        }
        else
        {
            // 첫 보상
            AdsManager.Instance.ShowRewardedVideoAds((r) =>
            {
                if (r == UnityEngine.Advertisements.ShowResult.Finished)
                {
                    UIManager.Instance.ShowTakeStarParticle(UIManager.Instance.foreCanvas.transform.position, 30);

                    // 보상 지급 후 시간 저장
                    UserInfo.Instance.lastRewardedTime = nowDateTime.ToString(TimeManager.TimeFormat);
                    UserInfo.Instance.Save();
                }
                else if (r == UnityEngine.Advertisements.ShowResult.Failed)
                {
                    // 아직 보상을 받을 수 없습니다.
                    UIManager.ShowMessageBoxUI
                        (UITextManager.GetText("알림")
                        , UITextManager.GetText("00045")
                        , UITextManager.GetText("확인")
                        , UIManager.GetMessageBoxUI().Close);
                }
                UpdateRemainTime();
            });
        }
    }

    public void OnClickGameStartBtn()
    {
        if(UserInfo.Instance.CheckLockStage(AppManager.Instance.curStageIdx))
        {
            UIManager.Instance.ShowNoticeUI("이전 스테이지를 클리어해야 잠금이 해제됩니다.");
        }
        else
        {
            AppManager.Instance.CloseLobby();
            AppManager.Instance.OpenIngame();
        }
    }

    public void OnClickShopBtn()
    {
        shopUI.Open();
    }

    public void OnClickSkillMapBtn()
    {
        skillMapUI.Open();
        skillMapUI.UpdateUI();
    }

    public void OnClickRankingBtn()
    {
#if UNITY_ANDROID
        if(GPGSManager.Instance.isActivated)
        {
            GPGSManager.Instance.ShowLeaderboardUI();
        }
        else
        {
            GPGSManager.Instance.GPGSLogin();
        }
#endif
    }

    public void OnClickCommunityBtn()
    {
        Application.OpenURL("https://cafe.naver.com/effecttd");
    }

    private void OnClickStageChangeLeftBtn()
    {
        stageChangeBtnClickCnt = Mathf.Clamp(stageChangeBtnClickCnt - 1, -5, 0);
        stageChangeBtnClickCntTimer = stageChangeBtnClickCntDelay;
        
        if (AppManager.Instance.ChangeStage(AppManager.Instance.curStageIdx - ((stageChangeBtnClickCnt > -5) ? 1 : 4)))
        {
            UpdateStageUI();
        }
        //else if (AppManager.Instance.ChangeStage(AppManager.Instance.curStageIdx - 1))
        //{
        //    UpdateStageUI(AppManager.Instance.curStageIdx);
        //}
    }

    private void OnClickStageChangeRightBtn()
    {
        stageChangeBtnClickCnt = Mathf.Clamp(stageChangeBtnClickCnt + 1, 0, +5);
        stageChangeBtnClickCntTimer = stageChangeBtnClickCntDelay;
        
        if (AppManager.Instance.ChangeStage(AppManager.Instance.curStageIdx + ((stageChangeBtnClickCnt < 5) ? 1 : 4)))
        {
            UpdateStageUI();
        }
        //else if (AppManager.Instance.ChangeStage(AppManager.Instance.curStageIdx + 1))
        //{
        //    UpdateStageUI(AppManager.Instance.curStageIdx);
        //}
    }

    private void OnClickStageInfoBtn()
    {

    }

    private void Update()
    {
        if (AppManager.Instance.appStatus != AppManager.EStatus.Initialized || IngameManager.Instance.status == IngameManager.EStatus.Playing)
        {
            return;
        }
        else if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            timer = 1f;
            UpdateRemainTime();
        }

        if (stageChangeBtnClickCnt != 0)
        {
            if(stageChangeBtnClickCntTimer > 0)
            {
                stageChangeBtnClickCntTimer -= Time.deltaTime;
            }
            else
            {
                stageChangeBtnClickCntTimer = stageChangeBtnClickCntDelay;
                stageChangeBtnClickCnt = 0;
            }
        }
    }

    public void UpdateStageUI()
    {
        Sprite sprite;
        if (!ResourceManager.Instance.TryGetSprite("Icon_StageDirectionIcon_Disabled", out sprite))
        {
            sprite = null;
        }
        stageDirectionImages[AppManager.Instance.preStageIdx].sprite = sprite;

        if (!ResourceManager.Instance.TryGetSprite("Icon_StageDirectionIcon_Enabled", out sprite))
        {
            sprite = null;
        }
        stageDirectionImages[AppManager.Instance.curStageIdx].sprite = sprite;

        StageData stageData;
        EStageMode stageMode = EStageMode.Normal;
        if (DataManager.Instance.TryStageData(AppManager.Instance.curStageIdx, out stageData))
        {
            stageMode = stageData.stageMode;
        }

        if(stageMode == EStageMode.Normal)
        {
            var goldScore = UserInfo.Instance.GetStageGoldScore((AppManager.Instance.curStageIdx + 1).ToString());
            if(goldScore > 0)
            {
                stageTitleTxt.text = string.Format("<b>Stage {0}</b>\n\r<font=BMHANNA_11yrs_ttf_OnlyNumbers><size=80%><color=#B5B5B5ff>{1}</color></size></font>", AppManager.Instance.curStageIdx + 1, goldScore);
            }
            else
            {
                stageTitleTxt.text = string.Format("<b>Stage {0}</b>", AppManager.Instance.curStageIdx + 1);
            }

            int starCount = UserInfo.Instance.GetStageStarCount(AppManager.Instance.curStageId);
            for (int i = 0; i < 3; ++i)
            {
                Sprite starIcon;
                if (!ResourceManager.Instance.TryGetSprite
                    ((starCount > i) ? "Icon_Star_Enabled" : "Icon_Star_Disabled"
                    , out starIcon))
                {
                    starIcon = null;
                }

                stageStarImages[i].gameObject.SetActive(true);
                stageStarImages[i].sprite = starIcon;
            }

            modeNameTxt.gameObject.SetActive(false);
            modeNameTxt.text = string.Empty;
        }
        else
        {
            var killScore = UserInfo.Instance.GetInfinityModeStageBestKillScore();
            if(killScore > 0)
            {
                stageTitleTxt.text = string.Format("<b>Stage {0}</b>\n\r<font=BMHANNA_11yrs_ttf_OnlyNumbers><size=80%><color=#B5B5B5ff>{1}</color></size></font>", AppManager.Instance.curStageIdx + 1, killScore);
            }
            else
            {
                stageTitleTxt.text = string.Format("<b>Stage {0}</b>", AppManager.Instance.curStageIdx + 1);
            }

            for (int i = 0; i < 3; ++i) stageStarImages[i].gameObject.SetActive(false);

            modeNameTxt.gameObject.SetActive(true);
            modeNameTxt.text = UITextManager.GetText("무한모드");
        }
        
        if(UserInfo.Instance.CheckLockStage(AppManager.Instance.curStageIdx))
        {
            lockUIRtf.gameObject.SetActive(true);
            gameStartBtn.image.material = ResourceManager.GetSpriteGrayScaleMaterial();
        }
        else
        {
            lockUIRtf.gameObject.SetActive(false);
            gameStartBtn.image.material = null;
        }
    }
    
    public void Release()
    {

    }
}
