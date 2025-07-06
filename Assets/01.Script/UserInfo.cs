using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LitJson;

using UserInfo_;
using SYU_Encryption;

public enum ETutorialFlag
{
    None = 0,

    /// <summary>
    /// 타워 배치 튜토리얼
    /// </summary>
    Ingame_1 = 1,

    /// <summary>
    /// 타워 효과부여 튜토리얼
    /// </summary>
    Ingame_2 = 2,

    /// <summary>
    /// 파괴자 튜토리얼
    /// </summary>
    Ingame_3 = 4,

    /// <summary>
    /// 공중 유닛 튜토리얼
    /// </summary>
    Ingame_4 = 8,

    /// <summary>
    /// 스킬 구매 튜토리얼
    /// </summary>
    Lobby_1 = 16,

    /// <summary>
    /// 쉴드 유닛 튜토리얼
    /// </summary>
    Ingame_5 = 32,

    Mask_Lobby = Lobby_1,
    Mask_Ingame = Ingame_1 + Ingame_2 + Ingame_3 + Ingame_4 + Ingame_5,
    Mask_All = Mask_Lobby + Mask_Ingame,
}

public class UserInfoData
{
    public UserInfoData()
    {
        stageClearInfo = new Dictionary<string, int>();
        newStageClearInfo = new Dictionary<string, StageClearInfo_NormalMode>();
        skillInventory = new Dictionary<string, bool>();

        infinityModeStageClaerInfo = new StageClearInfo_InfinityMode();
    }

    public ETutorialFlag tutorialFlag;

    public int starCount;
    public Dictionary<string, int> stageClearInfo;
    public Dictionary<string, StageClearInfo_NormalMode> newStageClearInfo;
    public Dictionary<string, bool> skillInventory;

    public StageClearInfo_InfinityMode infinityModeStageClaerInfo;

    public string lastRewardedTime;
}

public class StageClearInfo_NormalMode
{
    public int goldScore;
}

public class StageClearInfo_InfinityMode
{
    public int bestKillScore;
}

public class UserInfo
{
    public static UserInfo Instance { get { return AppManager.Instance.userInfo; } }

    public UserInfo()
    {
        skillInv = new SkillInventory(this);
    }

    public UserInfoData data = new UserInfoData();

    #region < Star Count >

    private int m_StarCount { get { return data.starCount; } set { data.starCount = value; } }
    public int starCount
    {
        get { return m_StarCount; }
        set
        {
            if (OnChangeStarCount != null) OnChangeStarCount(m_StarCount, value);
            m_StarCount = value;
        }
    }
    public void AddStarCount(int addCount, bool showParticle, Vector3 particlePos)
    {
        starCount += addCount;

        if(showParticle)
        {
            UIManager.Instance.ShowTakeStarParticle(particlePos, addCount);
        }
        else
        {
            UIManager.Instance.userResourcesUI.SetData(starCount);
        }
    }

    #endregion

    #region < Stage - Normal Mode >

    /// <summary>
    /// string : StageNo , int : StartCount
    /// </summary>
    public Dictionary<string, int> stageClearInfo { get { return data.stageClearInfo; } }
    public int GetStageStarCount(string stageId)
    {
        int starCount;
        if (stageClearInfo != null && stageClearInfo.TryGetValue(stageId, out starCount))
        {
            return starCount;
        }
        return 0;
    }
    public void SetStageStarCount(string stageId, int starCount)
    {
        int preStarCount;
        if(stageClearInfo == null)
        {
            return;
        }
        else if (stageClearInfo.TryGetValue(stageId, out preStarCount))
        {
            if (preStarCount < starCount)
            {
                stageClearInfo[stageId] = starCount;
            }
        }
        else
        {
            stageClearInfo.Add(stageId, starCount);
        }
    }
    
    public bool CheckLockStage(int stageIdx)
    {
#if !REAL
        if (GameSettingsManager.OpenAllStage) return false;
#endif

        StageData preStageData, curStageData;
        // 이전 스테이지가 없는 경우 (첫번째 스테이지는 항상 오픈)
        if (!DataManager.Instance.TryStageData(stageIdx - 1, out preStageData))
        {
            return false;   
        }
        // 이전 스테이지를 클리어한 적이 있는 경우
        else if (GetStageStarCount(preStageData.stageId) > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public int GetTotalStageStarCount()
    {
        int totalStarCount = 0;
        foreach(var pair in stageClearInfo)
        {
            totalStarCount += pair.Value;
        }
        return totalStarCount;
    }
    
    public Dictionary<string, StageClearInfo_NormalMode> newStageClearInfo { get { return data.newStageClearInfo; } }
    public int GetStageGoldScore(string stageId)
    {
        StageClearInfo_NormalMode clearInfo;
        if (newStageClearInfo != null && newStageClearInfo.TryGetValue(stageId, out clearInfo))
        {
            return clearInfo.goldScore;
        }
        return 0;
    }
    public void SetStageGoldScore(string stageId, int goldScore)
    {
        StageClearInfo_NormalMode clearInfo;
        {
            if (newStageClearInfo.TryGetValue(stageId, out clearInfo))
            {
                if(clearInfo.goldScore < goldScore)
                {
                    stageClearInfo[stageId] = goldScore;
                }
            }
            else
            {
                newStageClearInfo.Add(stageId, new StageClearInfo_NormalMode()
                {
                    goldScore = goldScore
                });
            }
        }
    }
    public int GetTotalStageGoldScore()
    {
        int totalGoldScore = 0;
        foreach (var pair in newStageClearInfo)
        {
            totalGoldScore += pair.Value.goldScore;
        }
        return totalGoldScore;
    }

    #endregion

    #region < Stage - Infinity Mode >

    public StageClearInfo_InfinityMode infinityModeStageClearInfo => data.infinityModeStageClaerInfo;
    public void SetInfinityModeStageBestKillScore(int killScore)
    {
        infinityModeStageClearInfo.bestKillScore = killScore;
    }
    public int GetInfinityModeStageBestKillScore()
    {
        return infinityModeStageClearInfo.bestKillScore;
    }
    

    #endregion

    /// <summary>
    /// UserInfoData와 매핑되어 SkillInventory의 값을 변경하면 UserInfoData도 함께 바뀐다.
    /// </summary>
    public SkillInventory skillInv { get; private set; }
    public void InitializeSkillInventory()
    {
        // 인벤토리 초기화 (기존에 배웠었던 스킬들 저장)
        skillInv.Initialize();

        // 공짜 스킬 습득
        var etor = DataManager.Instance.GetSkillDatasEtor();
        while (etor.MoveNext())
        {
#if REAL
            if (etor.Current.Value.starCost == 0)
            {
                skillInv.SetActiveSkill(etor.Current.Value.skillID, true);
            }
#else
            if (GameSettingsManager.OpenAllSkill || etor.Current.Value.starCost == 0)
            {
                skillInv.SetActiveSkill(etor.Current.Value.skillID, true);
            }
#endif

        }
    }

    #region < Event >

    /// <summary>
    /// int : preCash , int : curCash
    /// </summary>
    public UnityAction<int, int> OnChangeCash;
    public UnityAction<int, int> OnChangeStarCount;

    #endregion

    #region < Purchase >

    public bool IsPurchasedRemoveAds
    {
        get { return MyIAPManager.Instance.HasPurchased(IAPIDs.REMOVE_ADS); }
    }

    public bool IsPurchasedRemoveAdsContinue
    {
        get { return MyIAPManager.Instance.HasPurchased(IAPIDs.REMOVE_ADS_CONTINUE); }
    }

    public bool IsPurchasedRemoveAdsAll
    {
        get { return MyIAPManager.Instance.HasPurchased(IAPIDs.REMOVE_ADS_ALL) || MyIAPManager.Instance.HasPurchased(IAPIDs.STARTER_PACKAGE_1); }
    }

    #endregion

    #region < Tutorial >

    public void SetTutorialFlag(ETutorialFlag flag)
    {
        data.tutorialFlag |= flag;
    }
    public bool CheckTutorialCompleted(ETutorialFlag flag)
    {
        return (data.tutorialFlag & flag) == flag;
    }
    public bool IsTutorialCompletedAll { get { return (data.tutorialFlag & ETutorialFlag.Mask_All) == ETutorialFlag.Mask_All; } }

    #endregion

    #region < Ads >

    public System.DateTime lastAdsTime;
    public int adsPlayCount;

    #endregion

    public int normalModePlayCount;
    public int infinityModePlayCount;
    public string lastRewardedTime { get => data.lastRewardedTime; set => data.lastRewardedTime = value; }

    private static string sKey { get { return "!#%&(_USERINFO_@#(*&"; } }

    public void Save()
    {
        Save(this);
    }

    public static void Save(UserInfo userInfo)
    {
        var json = JsonMapper.ToJson(userInfo.data);
#if UNITY_EDITOR
        Debug.Log("SAVE \n\r" + json);
#endif

        var encryptedKey = AESCryptor.Encrypt("UserInfo", sKey);
        var encryptedJson = AESCryptor.Encrypt(json, sKey);
#if UNITY_EDITOR
        Debug.Log("SAVE(Encrypted) \n\r" + encryptedJson);
#endif

        PlayerPrefs.SetString(encryptedKey, encryptedJson);
        PlayerPrefs.Save();
    }

    public static bool TryLoad(out UserInfo userInfo)
    {
        userInfo = new UserInfo();
        UserInfoData data;

        var encryptedKey = AESCryptor.Encrypt("UserInfo", sKey);

        string decryptedJson = null;
        string encryptedJson = PlayerPrefs.GetString(encryptedKey);

#if UNITY_EDITOR
        Debug.Log("LOAD(Encrypted) \n\r" + encryptedJson);
#endif

        // 복호화 시도
        try
        {
            decryptedJson = AESCryptor.Decrypt(encryptedJson, sKey);
#if UNITY_EDITOR
            Debug.Log("LOAD(Decrypted) \n\r" + decryptedJson);
#endif
        }
        catch (System.Exception e)
        {
            decryptedJson = null;
        }

        // 복호화 성공 시
        if (!string.IsNullOrEmpty(decryptedJson))
        {
            data = JsonMapper.ToObject<UserInfoData>(decryptedJson);
            if (data != null)
            {
                userInfo.data = data;
#if UNITY_EDITOR
                Debug.Log("UserInfo Decrypt Success");
#endif
                return true;
            }
        }

        // 복호화 실패 시
        data = JsonMapper.ToObject<UserInfoData>(encryptedJson);
        if (data != null)
        {
            userInfo.data = data;
#if UNITY_EDITOR
            Debug.Log("UserInfo Decrypt Failure");
#endif
            return true;
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("UserInfo data not found");
#endif
            return false;
        }
    }

    public string GetLog()
    {
        return JsonMapper.ToJson(Instance.data);
    }
}
