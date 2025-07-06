using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using DG.Tweening;
using UnityEngine.Advertisements;

public class IngameUI_GameResult : UIBase
{
    [SerializeField]
    private TextMeshProUGUI m_InfinityModeTxt;
    public TextMeshProUGUI infinityModeTxt => m_InfinityModeTxt;

    [SerializeField]
    private List<Image> m_StarImages;
    public List<Image> starImages { get { return m_StarImages; } }

    private List<string> starImageFxKeys = new List<string>();

    [SerializeField]
    private TextMeshProUGUI m_ResultTxt;
    public TextMeshProUGUI resultTxt { get { return m_ResultTxt; } }

    [SerializeField]
    private TextMeshProUGUI m_ScoreLabelTxt;
    public TextMeshProUGUI scoreLabelTxt => m_ScoreLabelTxt;

    [SerializeField]
    private TextMeshProUGUI m_ScoreTxt;
    public TextMeshProUGUI scoreTxt => m_ScoreTxt;

    [SerializeField]
    private TextMeshProUGUI m_ScoreDesTxt;
    public TextMeshProUGUI scoreDesTxt => m_ScoreDesTxt;

    [SerializeField]
    private CustomButton m_ShowADBtn;
    public CustomButton showADBtn { get { return m_ShowADBtn; } }

    [SerializeField]
    private CustomButton m_ConfirmBtn;
    public CustomButton confirmBtn { get { return m_ConfirmBtn; } }

    public EStageMode stageMode { get; private set; }
    public bool win { get; private set; }
    public bool isGiveUp { get; private set; }
    public int takeStarCount { get; private set; }
    public int goldScore { get; private set; }
    public int killScore { get; private set; }

    private void Awake()
    {
        showADBtn.OnClick += OnClickShowADBtn;
        confirmBtn.OnClick += OnClickConfirmBtn;
    }

    public void SetData(bool win, int starCount, int takeStarCount, bool isGiveUp, int goldScore)
    {
        infinityModeTxt.gameObject.SetActive(false);
        foreach (var i in starImages) i.gameObject.SetActive(true);

        stageMode = EStageMode.Normal;
        this.win = win;
        this.isGiveUp = isGiveUp;
        this.takeStarCount = takeStarCount;
        this.goldScore = goldScore;
        this.killScore = 0;

        scoreTxt.text = (isGiveUp || !win) ? "0" : goldScore.ToString();
        scoreDesTxt.text = UITextManager.GetText("00039");

        Color bottomColor, topColor;
        if (ColorUtility.TryParseHtmlString("#EFC0FEFF", out topColor)) { }
        if (ColorUtility.TryParseHtmlString("#9BA7FFFF", out bottomColor)) { }

        scoreTxt.colorGradient = new VertexGradient()
        {
            topLeft = topColor,
            topRight = topColor,
            bottomLeft = bottomColor,
            bottomRight = bottomColor,
        };

        starImageFxKeys.Clear();
        for (int i = 0; i < starImages.Count; ++i)
        {
            Sprite sprite;
            if (!ResourceManager.Instance.TryGetSprite
                ((starCount > i) ? "Icon_Star_Enabled" : "Icon_Star_Disabled"
                , out sprite))
            {
                sprite = null;
            }

            starImageFxKeys.Add((starCount > i) ? "Fx_Result_Star_Enabled" : "Fx_Result_Star_Disabled");
            starImages[i].sprite = sprite;
        }
    }

    public void SetIntinityModeData(int takeStarCount, bool isGiveUp, int killScore)
    {
        infinityModeTxt.gameObject.SetActive(true);
        foreach(var i in starImages) i.gameObject.SetActive(false);

        stageMode = EStageMode.Infinity;
        this.win = true;
        this.isGiveUp = isGiveUp;
        this.takeStarCount = takeStarCount;
        this.goldScore = 0;
        this.killScore = killScore;

        scoreTxt.text = killScore.ToString();
        scoreDesTxt.text = UITextManager.GetText("00040");

        Color bottomColor, topColor;
        if (ColorUtility.TryParseHtmlString("#EE8871FF", out topColor))
        {

        }
        if (ColorUtility.TryParseHtmlString("#D6549CFF", out bottomColor))
        {

        }

        scoreTxt.colorGradient = new VertexGradient()
        {
            topLeft = topColor,
            topRight = topColor,
            bottomLeft = bottomColor,
            bottomRight = bottomColor,
        };

        starImageFxKeys.Clear();
        for (int i = 0; i < starImages.Count; ++i)
        {
            Sprite sprite;
            if (!ResourceManager.Instance.TryGetSprite
                ((!isGiveUp) ? "Icon_Star_Enabled" : "Icon_Star_Disabled"
                , out sprite))
            {
                sprite = null;
            }

            starImageFxKeys.Add((!isGiveUp) ? "Fx_Result_Star_Enabled" : "Fx_Result_Star_Disabled");
            starImages[i].sprite = sprite;
        }
    }

    public override void Open()
    {
        base.Open();
        StartCoroutine(OpenAnimating());
    }

    IEnumerator OpenAnimating()
    {
        confirmBtn.Close();
        showADBtn.Close();

        scoreLabelTxt.color = new Color(scoreLabelTxt.color.r, scoreLabelTxt.color.g, scoreLabelTxt.color.b, 0);
        scoreDesTxt.color = new Color(scoreDesTxt.color.r, scoreDesTxt.color.g, scoreDesTxt.color.b, 0);
        scoreTxt.color = new Color(scoreTxt.color.r, scoreTxt.color.g, scoreTxt.color.b, 0);

        if(stageMode == EStageMode.Normal)
        {
            foreach (var img in starImages)
            {
                img.color = new Color(1, 1, 1, 0);
                img.transform.localScale = new Vector3(3, 3, 3);
            }
        }

        float totalTime = 0;
        yield return new WaitForSeconds(0.2f);
        totalTime += 0.2f;

        if (stageMode == EStageMode.Normal)
        {
            for (int i = 0; i < starImages.Count; ++i)
            {
                starImages[i].DOColor(new Color(1, 1, 1, 1), 0.4f).SetEase(Ease.InSine);
                starImages[i].transform.DOScale(new Vector3(1, 1, 1), 0.4f).SetEase(Ease.InSine);

                Fx_OneTime fx;
                if (SpawnMaster.TrySpawnFx(starImageFxKeys[i], starImages[i].transform.position, Quaternion.identity, out fx))
                {
                    fx.On();
                }

                yield return new WaitForSeconds(0.3f);
                totalTime += 0.3f;
            }

            yield return new WaitForSeconds(0.2f);
            totalTime += 0.2f;
        }

        scoreLabelTxt.DOColor(new Color(scoreLabelTxt.color.r, scoreLabelTxt.color.g, scoreLabelTxt.color.b, 1), 0.3f);
        scoreTxt.DOColor(new Color(scoreTxt.color.r, scoreTxt.color.g, scoreTxt.color.b, 1), 0.3f);
        scoreDesTxt.DOColor(new Color(scoreDesTxt.color.r, scoreDesTxt.color.g, scoreDesTxt.color.b, 1), 0.3f);
        
        if (win)
        {
            yield return new WaitForSeconds(0.2f);
            totalTime += 0.2f;

            showADBtn.label.text = UITextManager.GetText("00028");
            showADBtn.Open();

            showADBtn.transform.localScale = Vector3.zero;
            showADBtn.transform.DOScale(1, 0.4f).SetEase(Ease.OutElastic);

            yield return new WaitForSeconds(0.4f);
            totalTime += 0.4f;

            showADBtn.transform.localScale = Vector3.one;
            showADBtn.transform.DOScale(1.05f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutCubic).Restart();

            float timer = 3f;
            do
            {
                yield return null;
                timer -= Time.deltaTime;
            }
            while (timer > 0 && gameObject.activeInHierarchy);

            confirmBtn.gameObject.SetActive(true);
            confirmBtn.transform.localScale = Vector3.zero;
            confirmBtn.transform.DOScale(1, 0.4f).SetEase(Ease.OutCubic);
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
            totalTime += 0.2f;

            confirmBtn.gameObject.SetActive(true);
            confirmBtn.transform.localScale = Vector3.zero;
            confirmBtn.transform.DOScale(1, 0.4f).SetEase(Ease.OutElastic);
        }
    }

    public void OnClickShowADBtn()
    {
        if(UserInfo.Instance.IsPurchasedRemoveAds || UserInfo.Instance.IsPurchasedRemoveAdsAll)
        {
            Close();

            AppManager.Instance.CloseIngame();
            AppManager.Instance.OpenLobby(!isGiveUp);

            UserInfo.Instance.AddStarCount(takeStarCount * 2, true, transform.position);
            UserInfo.Instance.Save();

            takeStarCount = 0;
        }
        else
        {
            #region < Lagacy >
            //GoogleAdmobManager.Instance.ShowRewardedVideo((result) =>
            //{
            //    UserInfo.Instance.adsPlayCount = 0;

            //    Close();

            //    AppManager.Instance.CloseIngame();
            //    AppManager.Instance.OpenLobby(!isGiveUp);

            //    switch (result)
            //    {
            //        case ShowResult.Finished:
            //            UserInfo.Instance.AddStarCount(takeStarCount * 2, true, transform.position);
            //            UserInfo.Instance.Save();
            //            takeStarCount = 0;
            //            break;

            //        case ShowResult.Skipped:
            //            UserInfo.Instance.AddStarCount(takeStarCount, true, transform.position);
            //            UserInfo.Instance.Save();
            //            takeStarCount = 0;
            //            break;

            //        default:
            //        case ShowResult.Failed:
            //            // 구글 광고 재생에 실패하면 유니티 광고를 보여준다.
            //            AdsManager.Instance.ShowRewardedVideoAds((result2) =>
            //            {
            //                Close();

            //                AppManager.Instance.CloseIngame();
            //                AppManager.Instance.OpenLobby();

            //                switch (result2)
            //                {
            //                    case ShowResult.Finished:
            //                        UserInfo.Instance.AddStarCount(takeStarCount * 2, true, transform.position);
            //                        UserInfo.Instance.Save();
            //                        break;

            //                    default:
            //                    case ShowResult.Failed:
            //                    case ShowResult.Skipped:
            //                        UserInfo.Instance.AddStarCount(takeStarCount, true, transform.position);
            //                        UserInfo.Instance.Save();
            //                        break;
            //                }
            //                takeStarCount = 0;
            //            });
            //            break;
            //    }
            //});
            #endregion

            AdsManager.Instance.ShowRewardedVideoAds((result2) =>
            {
                UnityEngine.Analytics.AnalyticsEvent.Custom("ads_video_end", new Dictionary<string, object>
                {
                    { "rewsult_reward_x2" , result2.ToString() }
                });

                Close();

                AppManager.Instance.CloseIngame();
                AppManager.Instance.OpenLobby();

                switch (result2)
                {
                    case ShowResult.Finished:
                        UserInfo.Instance.AddStarCount(takeStarCount * 2, true, transform.position);
                        UserInfo.Instance.Save();
                        break;

                    default:
                    case ShowResult.Failed:
                    case ShowResult.Skipped:
                        UserInfo.Instance.AddStarCount(takeStarCount, true, transform.position);
                        UserInfo.Instance.Save();
                        break;
                }
                takeStarCount = 0;
            });
        }
    } 

    public void OnClickConfirmBtn()
    {
        Debug.LogFormat("TOTAL MINUTES ({0}) PLAY COUNT ({1})"
            , (System.DateTime.Now - UserInfo.Instance.lastAdsTime).TotalMinutes
            , UserInfo.Instance.adsPlayCount);

        if(UserInfo.Instance.IsPurchasedRemoveAds || UserInfo.Instance.IsPurchasedRemoveAdsAll)
        {
            Close();

            AppManager.Instance.CloseIngame();
            AppManager.Instance.OpenLobby(!isGiveUp);

            UserInfo.Instance.AddStarCount(takeStarCount, true, transform.position);
            UserInfo.Instance.Save();

            takeStarCount = 0;
        }
        else if ((System.DateTime.Now - UserInfo.Instance.lastAdsTime).TotalMinutes >= GameSettingsManager.AdsTimeCycle)
        {
            UserInfo.Instance.adsPlayCount = 0;
            UserInfo.Instance.lastAdsTime = System.DateTime.Now;

            AdsManager.Instance.ShowRewardedVideoAds((result) =>
            {
                UnityEngine.Analytics.AnalyticsEvent.Custom("ads_video_end_2", new Dictionary<string, object>
                {
                    { "result_play_time" , result.ToString() }
                });

                Close();

                AppManager.Instance.CloseIngame();
                AppManager.Instance.OpenLobby(!isGiveUp);

                UserInfo.Instance.AddStarCount(takeStarCount, true, transform.position);
                UserInfo.Instance.Save();

                takeStarCount = 0;
            });

        }
        else if (UserInfo.Instance.adsPlayCount == GameSettingsManager.AdsPlayCount / 2)
        {
            AdsManager.Instance.ShowInterstitalAds((result) =>
            {
                UnityEngine.Analytics.AnalyticsEvent.Custom("ads_interstital_end", new Dictionary<string, object>
                {
                    { "result_play_count" , result.ToString() }
                });

                Close();

                AppManager.Instance.CloseIngame();
                AppManager.Instance.OpenLobby(!isGiveUp);

                UserInfo.Instance.AddStarCount(takeStarCount, true, transform.position);
                UserInfo.Instance.Save();

                takeStarCount = 0;
            });
        }
        else if (UserInfo.Instance.adsPlayCount >= GameSettingsManager.AdsPlayCount)
        {
            UserInfo.Instance.adsPlayCount = 0;
            UserInfo.Instance.lastAdsTime = System.DateTime.Now;

            AdsManager.Instance.ShowRewardedVideoAds((result) =>
            {
                UnityEngine.Analytics.AnalyticsEvent.Custom("ads_video_end_2", new Dictionary<string, object>
                {
                    { "result_play_count" , result.ToString() }
                });

                Close();

                AppManager.Instance.CloseIngame();
                AppManager.Instance.OpenLobby(!isGiveUp);

                UserInfo.Instance.AddStarCount(takeStarCount, true, transform.position);
                UserInfo.Instance.Save();

                takeStarCount = 0;
            });
        }
        else
        {
            Close();

            AppManager.Instance.CloseIngame();
            UserInfo.Instance.AddStarCount(takeStarCount, true, transform.position);
            UserInfo.Instance.Save();

            AppManager.Instance.OpenLobby(!isGiveUp);

            takeStarCount = 0;
        }
    }
}
