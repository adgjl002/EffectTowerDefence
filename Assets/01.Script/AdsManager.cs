using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Events;

using GoogleMobileAds.Api;

public enum EAdsType
{
    None = 0,
    Banner = 1,
    Interstital = 2,
    RewardedVideo = 4
}

public enum EInitStatus
{
    Initializing = 0,
    InitSuccess = 1,
    InitFailure = 2,
}

public class AdsManager : BaseMonoSingleton<AdsManager>, IUnityAdsListener
{
    public bool firstReadyInterstitalAds = false;
    public bool firstReadyBannerAds = false;
    public string gameId = "3408937";

    [Header("배너 광고 ID")]
    public string placementId_Banner = "banner";

    [Header("전면 광고 ID")]
    public string placementId_Interstital = "Interstital";

    [Header("보상형 광고 ID")]
    public string placementId_Rewarded = "rewardedVideo";

    public bool isTestMode = false;
    public bool isSkipMode = false;

    private UnityAction<ShowResult> OnRewardedAdsFinished;
    
    private void Start()
    {
        Debug.LogFormat("ADS Manager Initialize on Start");
        Advertisement.AddListener(this);
        Advertisement.Initialize(gameId, isTestMode);
        GoogleAdmobManager.Instance.Initialize();
    }

    public void ShowInterstitalAds(UnityAction<ShowResult> OnRewardedAdsFinished)
    {
        Debug.LogFormat("{0}", MethodBase.GetCurrentMethod().Name);

#if UNITY_EDITOR
        if(isSkipMode)
        {
            OnRewardedAdsFinished?.Invoke(ShowResult.Finished);
        }
        else
#endif
        {
            this.OnRewardedAdsFinished = OnRewardedAdsFinished;
            Advertisement.Show(placementId_Interstital);
        }
    }
    
    public void ShowRewardedVideoAds(UnityAction<ShowResult> OnRewardedAdsFinished)
    {
        Debug.LogFormat("{0}", MethodBase.GetCurrentMethod().Name);

#if UNITY_EDITOR
        if (isSkipMode)
        {
            OnRewardedAdsFinished?.Invoke(ShowResult.Finished);
        }
        else
#endif
        {
            this.OnRewardedAdsFinished = OnRewardedAdsFinished;
            Advertisement.Show(placementId_Rewarded);
        }
    }
    
    public void OnUnityAdsReady(string placementId)
    {
        // 첫 접속 후 광고 준비가 되면 전면 광고를 1회 띄운다.
        //if (!firstReadyInterstitalAds && placementId.Equals(placementId_Interstital))
        //{
        //    StartCoroutine(WaitForInitializeForShowAds());
        //}

        if (!firstReadyBannerAds && placementId.Equals(placementId_Banner))
        {
            StartCoroutine(ShowBannerWhenReady());
        }
    }

    private IEnumerator ShowBannerWhenReady()
    {
        if (firstReadyBannerAds) yield break;
        
        // 앱의 초기화 후 광고 출력
        //yield return new WaitWhile(() => {
        //    return MyIAPManager.Instance.initStatus == MyIAPManager.EInitStatus.Initializing;
        //});

        //while (!Advertisement.IsReady(placementId_Banner))
        //{
        //    yield return new WaitForSeconds(0.5f);
        //}

        //Debug.LogFormat("{0} 3 (IsReady)", MethodBase.GetCurrentMethod().Name);

        // 광고 제거 결재했다면 스킵
        if (!UserInfo.Instance.IsPurchasedRemoveAdsAll && !UserInfo.Instance.IsPurchasedRemoveAds)
        {
            Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
            Advertisement.Banner.Show(placementId_Banner);
            //GoogleAdmobManager.Instance.ShowBanner();
        }
        else
        {
            Advertisement.Banner.Hide(true);
            //GoogleAdmobManager.Instance.HideBanner();
        }
        firstReadyBannerAds = true;
    }

    public void HideBannerAds()
    {
        Advertisement.Banner.Hide(true);
        //GoogleAdmobManager.Instance.HideBanner();
    }

    private IEnumerator WaitForInitializeForShowAds()
    {
        // 최초 1회 광고가 노출되었다면 스킵
        if (firstReadyInterstitalAds) yield break;
        
        // 앱의 초기화 후 광고 출력
        yield return new WaitWhile(() => {
            return MyIAPManager.Instance.initStatus == MyIAPManager.EInitStatus.Initializing;
        });
        
        // 광고 제거 결재했다면 스킵
        if (!UserInfo.Instance.IsPurchasedRemoveAdsAll && !UserInfo.Instance.IsPurchasedRemoveAds)
        {
            ShowInterstitalAds(null);
            //GoogleAdmobManager.Instance.ShowInterstital(null);
        }
        firstReadyInterstitalAds = true;
    }

    public void OnUnityAdsDidError(string message)
    {
        Debug.LogError(message);
    }

    public void OnUnityAdsDidStart(string placementId)
    {

    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        OnRewardedAdsFinished?.Invoke(showResult);
        OnRewardedAdsFinished = null;
    }
}