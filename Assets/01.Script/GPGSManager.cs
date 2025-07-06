using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class GPGSManager : BaseSingleton<GPGSManager>
{
    public bool isActivated => (Social.localUser != null && Social.localUser.authenticated);

    public void GPGSActivate()
    {
        // 소셜 로그인 준비
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            //.AddOauthScope("profile")
            //.RequestServerAuthCode(false)
            .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }

    public void GPGSLogin()
    {
        if (Social.localUser.authenticated)
        {
            Debug.LogFormat("이미 구글 플레이 서비스 로그인 되어있음");
        }
        else
        {
            Social.localUser.Authenticate(AuthenticateCallback);
        }
    }

    public void AuthenticateCallback(bool success)
    {
        if (success)
        {
            var serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
            Debug.LogFormat("구글 플레이 서비스 로그인 성공({0}), ServerAuthCode({1})", Social.localUser.userName, serverAuthCode);
        }
        else
        {
            Debug.LogFormat("구글 플레이 서비스 로그인 실패");
        }
    }

    public void ShowLeaderboardUI()
    {
        if(Social.localUser.authenticated)
        {
            Social.ShowLeaderboardUI();
        }

       //Social.Active.ReportScore
       //    ( (long)UserInfo.Instance.GetTotalStageGoldScore()
       //    , GPGSIds.leaderboard_high_gold_score_all_stage
       //    , (result)=> { });

       //Social.Active.ReportScore
       //    ((long)UserInfo.Instance.GetInfinityModeStageBestKillScore()
       //    , GPGSIds.leaderboard_high_score_infinity_mode
       //    , (result) => { });

       //Social.ShowLeaderboardUI();
    }
}
