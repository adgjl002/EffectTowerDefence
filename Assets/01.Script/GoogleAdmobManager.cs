using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GoogleMobileAds.Api;
using System;
using UnityEngine.Advertisements;
using UnityEngine.Events;
using System.Reflection;

public class GoogleAdmobManager : BaseMonoSingleton<GoogleAdmobManager>
{
    public EInitStatus initStatus { get; private set; }

    private BannerView bannerView;
    private InterstitialAd interstitial;
    private RewardBasedVideoAd rewardedVideoAd;

    private IEnumerator showInterstitalEtor;
    private IEnumerator showRewardedVideoEtor;

    private UnityAction<ShowResult> OnRewardedAdsFinished;
    
    public void Initialize()
    {
        Debug.LogFormat("Google AdMob Initialize Start");
        MobileAds.Initialize(initStatus =>
        {
            Debug.LogFormat("Google AdMob Initialized >> {0}", initStatus.ToString());
            this.initStatus = EInitStatus.InitSuccess;
            
            //RequestBanner();
            RequestInterstitial();
            RequestRewardedVideo();
        });
    }
    
    private void RequestBanner()
    {
#if UNITY_ANDROID && !REAL
        string adUnitId = "ca-app-pub-3940256099942544/6300978111"; // 테스트 ID
#elif UNITY_ANDROID && REAL
        string adUnitId = "ca-app-pub-6492657678308461/7450642000";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/2934735716"; // 테스트 ID
#else
        string adUnitId = "unexpected_platform";
#endif

        // Create a 320x50 banner at the top of the screen.
        this.bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

        // Called when an ad request has successfully loaded.
        //this.bannerView.OnAdLoaded += this.HandleOnAdLoaded;
        // Called when an ad request failed to load.
        //this.bannerView.OnAdFailedToLoad += this.HandleOnAdFailedToLoad;
        // Called when an ad is clicked.
        //this.bannerView.OnAdOpening += this.HandleOnAdOpened;
        // Called when the user returned from the app after an ad click.
        //this.bannerView.OnAdClosed += this.HandleOnAdClosed;
        // Called when the ad click caused the user to leave the application.
        //this.bannerView.OnAdLeavingApplication += this.HandleOnAdLeavingApplication;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder()
#if !REAL
          .AddTestDevice("2077ef9a63d2b398840261c8221a0c9b") // 테스트 ID
#endif
          .Build();

        // Load the banner with the request.
        this.bannerView.LoadAd(request);
    }

    private void RequestInterstitial()
    {
#if UNITY_ANDROID && !REAL
        string adUnitId = "ca-app-pub-3940256099942544/1033173712"; // 테스트 ID
#elif UNITY_ANDROID && REAL
        string adUnitId = "ca-app-pub-6492657678308461/3702968681";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/4411468910"; // 테스트 ID
#else
        string adUnitId = "unexpected_platform";
#endif
        if (this.interstitial != null)
        {
            this.interstitial.Destroy();
            this.interstitial = null;
        }

        // Initialize an InterstitialAd.
        this.interstitial = new InterstitialAd(adUnitId);

        // Called when an ad request has successfully loaded.
        this.interstitial.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request failed to load.
        this.interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when an ad is shown.
        this.interstitial.OnAdOpening += HandleOnAdOpened;
        // Called when the ad is closed.
        this.interstitial.OnAdClosed += HandleOnAdClosed;
        // Called when the ad click caused the user to leave the application.
        this.interstitial.OnAdLeavingApplication += HandleOnAdLeavingApplication;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder()
#if !REAL
          .AddTestDevice("2077ef9a63d2b398840261c8221a0c9b") // 테스트 ID
#endif
          .Build();

        // Load the interstitial with the request.
        this.interstitial.LoadAd(request);
    }

#region < RewardedVideo >

    private void RequestRewardedVideo()
    {
#if UNITY_ANDROID && !REAL
        string adUnitId = "ca-app-pub-3940256099942544/5224354917"; // 테스트 ID
#elif UNITY_ANDROID && REAL
        string adUnitId = "ca-app-pub-6492657678308461/4445204712";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/4411468910"; // 테스트 ID
#else
        string adUnitId = "unexpected_platform";
#endif
        if (this.rewardedVideoAd == null)
        {
            this.rewardedVideoAd = RewardBasedVideoAd.Instance;

            // Called when an ad request has successfully loaded.
            rewardedVideoAd.OnAdLoaded += HandleRewardBasedVideoLoaded;
            // Called when an ad request failed to load.
            rewardedVideoAd.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
            // Called when an ad is shown.
            rewardedVideoAd.OnAdOpening += HandleRewardBasedVideoOpened;
            // Called when the ad starts to play.
            rewardedVideoAd.OnAdStarted += HandleRewardBasedVideoStarted;
            // Called when the user should be rewarded for watching a video.
            rewardedVideoAd.OnAdRewarded += HandleRewardBasedVideoRewarded;
            // Called when the ad is closed.
            rewardedVideoAd.OnAdClosed += HandleRewardBasedVideoClosed;
            // Called when the ad click caused the user to leave the application.
            rewardedVideoAd.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;
        }

        AdRequest request = new AdRequest.Builder()
#if !REAL
          .AddTestDevice("2077ef9a63d2b398840261c8221a0c9b") // 테스트 ID
#endif
          .Build();
        rewardedVideoAd.LoadAd(request, adUnitId);
    }

    public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoLoaded event received");
    }

    public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print(
            "HandleRewardBasedVideoFailedToLoad event received with message: "
                             + args.Message);
        OnRewardedAdsFinished?.Invoke(ShowResult.Failed);
        OnRewardedAdsFinished = null;
    }

    public void HandleRewardBasedVideoOpened(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoOpened event received");
    }

    public void HandleRewardBasedVideoStarted(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoStarted event received");
    }

    public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoClosed event received");
        OnRewardedAdsFinished?.Invoke(ShowResult.Skipped);
        OnRewardedAdsFinished = null;
    }

    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        MonoBehaviour.print(
            "HandleRewardBasedVideoRewarded event received for "
                        + amount.ToString() + " " + type);

        OnRewardedAdsFinished?.Invoke(ShowResult.Finished);
        OnRewardedAdsFinished = null;
    }

    public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoLeftApplication event received");
    }

    public void ShowRewardedVideo(UnityAction<ShowResult> OnRewardedAdsFinished)
    {
        if (initStatus != EInitStatus.InitSuccess || interstitial == null)
        {
            Debug.LogFormat("{0} Failure", MethodBase.GetCurrentMethod().Name);
            OnRewardedAdsFinished?.Invoke(ShowResult.Failed);
            return;
        }

        this.OnRewardedAdsFinished = OnRewardedAdsFinished;

        if (showRewardedVideoEtor != null)
        {
            StopCoroutine(showRewardedVideoEtor);
            showRewardedVideoEtor = null;
        }

        showRewardedVideoEtor = ShowRewardedVideoWhenReady();
        StartCoroutine(showRewardedVideoEtor);

        Debug.LogFormat("{0} Success", MethodBase.GetCurrentMethod().Name);
    }

    private IEnumerator ShowRewardedVideoWhenReady()
    {
        Debug.LogFormat("{0} Google REWARDED VIDEO Wait for isloaded", MethodBase.GetCurrentMethod().Name);

        float timer = 5f;
        // 광고가 로드될때까지 대기
        while (!rewardedVideoAd.IsLoaded())
        {
            yield return null;

            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                // 일정시간동안 광고가 준비되지 않는다면 실패 콜백을 날린다.
                OnRewardedAdsFinished?.Invoke(ShowResult.Failed);
                OnRewardedAdsFinished = null;
                yield break;
            }
        }

        this.rewardedVideoAd.Show();
        Debug.LogFormat("{0} Google INTERSTITIAL SHOW!!", MethodBase.GetCurrentMethod().Name);
        yield break;
    }


#endregion

    public void ShowBanner()
    {
        //if (initStatus == EInitStatus.InitSuccess)
        //{
        //    Debug.LogFormat("{0} Success", MethodBase.GetCurrentMethod().Name);
        //    this.bannerView?.Show();
        //}
        //else
        //{
        //    Debug.LogFormat("{0} Failure", MethodBase.GetCurrentMethod().Name);
        //}
    }
    
    public void HideBanner()
    {
        //if (initStatus == EInitStatus.InitSuccess)
        //{
        //    Debug.LogFormat("{0} Success", MethodBase.GetCurrentMethod().Name);
        //    this.bannerView?.Hide();
        //}
        //else
        //{
        //    Debug.LogFormat("{0} Failure", MethodBase.GetCurrentMethod().Name);
        //}
    }

#region < Interstital >

    public void ShowInterstital(UnityAction<ShowResult> OnRewardedAdsFinished)
    {
        if (initStatus != EInitStatus.InitSuccess || interstitial == null)
        {
            Debug.LogFormat("{0} Failure", MethodBase.GetCurrentMethod().Name);
            OnRewardedAdsFinished?.Invoke(ShowResult.Failed);
            return;
        }

        this.OnRewardedAdsFinished = OnRewardedAdsFinished;

        if (showInterstitalEtor != null)
        {
            StopCoroutine(showInterstitalEtor);
            showInterstitalEtor = null;
        }

        showInterstitalEtor = ShowInterstitalWhenReady();
        StartCoroutine(showInterstitalEtor);

        Debug.LogFormat("{0} Success", MethodBase.GetCurrentMethod().Name);
    }

    private IEnumerator ShowInterstitalWhenReady()
    {
        Debug.LogFormat("{0} Google INTERSTITIAL Wait for isloaded", MethodBase.GetCurrentMethod().Name);

        float timer = 5f;
        // 광고가 로드될때까지 대기
        while (!interstitial.IsLoaded())
        {
            yield return null;
             
            if(timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                // 일정시간동안 광고가 준비되지 않는다면 실패 콜백을 날린다.
                OnRewardedAdsFinished?.Invoke(ShowResult.Failed);
                yield break;
            }
        }
         
        this.interstitial.Show();
        Debug.LogFormat("{0} Google INTERSTITIAL SHOW!!", MethodBase.GetCurrentMethod().Name);
        yield break;
    }

    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdLoaded event received : " + args.ToString());
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print("HandleFailedToReceiveAd event received with message: "
                            + args.Message);

        OnRewardedAdsFinished?.Invoke(ShowResult.Failed);
        OnRewardedAdsFinished = null;

        RequestInterstitial();
    }

    public void HandleOnAdOpened(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdOpened event received");
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdClosed event received");

        OnRewardedAdsFinished?.Invoke(ShowResult.Finished);
        OnRewardedAdsFinished = null;

        RequestInterstitial();
    }

    public void HandleOnAdLeavingApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdLeavingApplication event received");
    }

#endregion

}
