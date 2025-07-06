using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using OPS.AntiCheat;
using OPS.AntiCheat.Speed;

public enum EIngameObjectClass
{
    Normal = 0,
    Boss = 2,
}

public class IngameObjectSpawnInfo
{
    public IngameObjectSpawnInfo(int wave, string prefabKey, string dataKey, int spawnCount, float spawnDelay, float startSpawnDelay, float powerRatio = 1f, float costRatio = 1f, float defenseRatio = 1f, IngameObject.EClass unitClass = IngameObject.EClass.Normal, float maxShieldRatio = 1f)
    {
        this.wave = wave;
        this.prefabKey = prefabKey;
        this.dataKey = dataKey;
        this.spawnCount = spawnCount;
        this.spawnDelay = spawnDelay;
        spawnDelayTimer = 0;

        this.startSpawnDelay = startSpawnDelay;
        startSpawnDelayTimer = 0;

        this.powerRatio = powerRatio;
        this.costRatio = costRatio;
        this.maxShieldRatio = maxShieldRatio;

        if (!DataManager.Instance.TryGetIngameObjectData(dataKey, level, unitClass, spawnCount, out ingameObjData, powerRatio, costRatio, defenseRatio, maxShieldRatio))
        {
            Debug.LogErrorFormat("IngameObjectData({0} / {1} / {2} / {3} / {4}) not found.", dataKey, level, spawnCount, powerRatio, costRatio);
        }
    }

    public int wave;
    public int level => wave;
    public string prefabKey;
    public string dataKey;
    public float powerRatio;
    public float costRatio;
    public float maxShieldRatio;

    public IngameObjectData ingameObjData;
    public int spawnCount;

    public float spawnDelay;
    public float spawnDelayTimer;

    public float startSpawnDelay;
    public float startSpawnDelayTimer;
}

public class IngameWaveInfo
{
    public IngameWaveInfo()
    {
        spawnInfos = new List<IngameObjectSpawnInfo>();
    }
    
    public List<IngameObjectSpawnInfo> spawnInfos;
}

public class IngameManager : MonoBehaviour {
    
    public enum EStatus
    {
        None = 0,
        Initialized = 1,
        Playing = 2,
        Ending = 3,
        Ended = 4
    }

    public static IngameManager Instance { get { return AppManager.Instance.ingameManager; } }
    public static int OBJECT_ID = 0;
    
    public EStatus status { get; private set; }

    [SerializeField]
    private GameObject m_GridCenterGobj;
    public GameObject gridCenterGobj { get { return m_GridCenterGobj; } }

    #region < IngameObject >

    public Dictionary<string, IngameObject> ingameObjects { get; private set; }
    public void RegistObject(IngameObject ingameObject)
    {
        if(!ingameObjects.ContainsKey(ingameObject.name))
        {
            ingameObjects.Add(ingameObject.name, ingameObject);
        }
    }
    public bool UnregistObject(IngameObject ingameObject)
    {
        return ingameObjects.Remove(ingameObject.name);
    }
    public List<IngameObject> FindIngameObject(Func<IngameObject, bool> findCondition)
    {
        List<IngameObject> objs = new List<IngameObject>();
        foreach(var pair in ingameObjects)
        {
            if(findCondition(pair.Value))
            {
                objs.Add(pair.Value);
            }
        }
        return objs;
    }
    public bool CheckIngameObjectCondition(Func<IngameObject, bool> checkCondition)
    {
        List<IngameObject> objs = new List<IngameObject>();
        foreach (var pair in ingameObjects)
        {
            if (checkCondition(pair.Value))
            {
                return true;
            }
        }
        return false;
    }

    #endregion
    
    #region < KillScore >

    [SerializeField]
    private int m_KillScore;
    public int killScore
    {
        get { return m_KillScore; }
        private set { OnChangeKillScore?.Invoke(value, value - m_KillScore); m_KillScore = value; }
    }
    public void SetKillScore(int killScore)
    {
        this.killScore = killScore;
    }
    public void AddKillScore(int killScore)
    {
        this.killScore = Mathf.Max(0, this.killScore + killScore);
    }

    #endregion

    #region < Cost >

    [SerializeField]
    private int m_CurCost;
    public int curCost
    {
        get { return m_CurCost; }
        private set { OnChangeCost?.Invoke(value, value - m_CurCost); m_CurCost = value; }
    }
    public void SetCost(int cost, bool showFx = false)
    {
        curCost = Mathf.Max(0, cost);

        UIManager_Ingame.Instance.topUI.costUI.UpdateCost(curCost);
    }
    public void AddCost(int cost, bool showFx = false)
    {
        curCost = Mathf.Max(0, curCost + cost);

        UIManager_Ingame.Instance.topUI.costUI.UpdateCost(curCost);
    }

    [SerializeField]
    private int m_BuildCost;
    public int buildCost { get { return m_BuildCost; } private set { m_BuildCost = value; } }
    public void SetBuildCost(int buildCost, bool showFx = false)
    {
        this.buildCost = buildCost;
    }

    #endregion

    #region < Life >

    public int startLife { get; private set; }

    /// <summary>
    /// 라이프가 없을 때 광고 시청 후 일정량 충전한 횟 수
    /// </summary>
    public int lifeRefillCount { get; private set; }
    
    [SerializeField]
    private int m_CurLife;
    public int curLife
    {
        get { return m_CurLife; }
        private set
        {
            var nxtLife = Mathf.Max(0, value);
            OnChangeLife?.Invoke(nxtLife, m_CurLife - nxtLife);
            m_CurLife = nxtLife;

            if (m_CurLife == 0 && status == EStatus.Playing)
            {
                if(lifeRefillCount < GameSettingsManager.LimitLifeRefillCount)
                {
                    UIManager.ShowMessageBoxUI
                    (UITextManager.GetText("알림")
                    , UITextManager.GetText("00027")
                    , string.Format(UITextManager.GetText("00030"), Mathf.RoundToInt(startLife * GameSettingsManager.LifeRefilePercOfStartLife).ToString())
                    , () => {
                        if(UserInfo.Instance.IsPurchasedRemoveAdsContinue || UserInfo.Instance.IsPurchasedRemoveAdsAll)
                        {
                            isGameStopped = false;
                            UIManager.Instance.messageBoxUI.Close();

                            ++lifeRefillCount;
                            curLife += Mathf.RoundToInt(startLife * GameSettingsManager.LifeRefilePercOfStartLife);
                        }
                        else
                        {
                            AdsManager.Instance.ShowRewardedVideoAds((result2) =>
                            {
                                UnityEngine.Analytics.AnalyticsEvent.Custom("ads_video_end", new Dictionary<string, object>
                                {
                                    { "add_life" , result2.ToString() }
                                });

                                isGameStopped = false;
                                UIManager.Instance.messageBoxUI.Close();

                                if (result2 == UnityEngine.Advertisements.ShowResult.Finished)
                                {
                                    ++lifeRefillCount;
                                    curLife += Mathf.RoundToInt(startLife * GameSettingsManager.LifeRefilePercOfStartLife);
                                }
                                else
                                {
                                    ReadyEndStage(false);
                                }
                            });

                            //GoogleAdmobManager.Instance.ShowRewardedVideo((result) =>
                            //{
                            //    isGameStopped = false;
                            //    UIManager.Instance.messageBoxUI.Close();

                            //    if (result == UnityEngine.Advertisements.ShowResult.Finished)
                            //    {
                            //        ++lifeRefillCount;
                            //        curLife += Mathf.RoundToInt(startLife * GameSettingsManager.LifeRefilePercOfStartLife);
                            //    }
                            //    else if(result == UnityEngine.Advertisements.ShowResult.Failed)
                            //    {
                            //    }
                            //    else
                            //    {
                            //        ReadyEndStage(false);
                            //    }
                            //});
                        }
                    }
                    , UITextManager.GetText("패배")
                    , () => {

                        int leftValue = ((AppManager.Instance.curStageIdx / 10) * 10) + 1;
                        int rightValue = leftValue + 9;
                        UnityEngine.Analytics.AnalyticsEvent.Custom(string.Format("normal_gameover_{0}_{1}", leftValue, rightValue), new Dictionary<string, object>
                        {
                            { (AppManager.Instance.curStageIdx + 1).ToString(), string.Empty }
                        });

                        isGameStopped = false;
                        UIManager.Instance.messageBoxUI.Close();
                        ReadyEndStage(false);
                    });

                    isGameStopped = true;
                }
                else
                {
                    ReadyEndStage(false);
                }
            }
        }
    }

    public void SetLife(int life, bool showFx = false)
    {
        curLife = life;
    }
    public void AddLife(int addLife, bool showFx = false)
    {
        curLife += addLife;
    }

    #endregion

    #region < Event >

    public UnityAction<int, int> OnChangeKillScore;
    public UnityAction<int, int> OnChangeCost;
    public UnityAction<int, int> OnChangeLife;

    #endregion

    #region < Wave & Spawn >

    public int curWave { get; private set; }
    public Dictionary<int, IngameWaveInfo> waveInfos { get; private set; }
    public List<IngameObjectSpawnInfo> ingameObjectSpawnInfos { get; private set; }

    #endregion

    #region < TowerAdditionalEffect >

    public int refreshCount = 0;
    public bool isCanRefreshTowerAdditionalEffects;
    public int GetRefreshPrice()
    {
        return GameSettingsManager.TAERefreshPrice + (refreshCount * GameSettingsManager.TAERefreshPriceIncrease);
    }
    public void InitTAERefreshCount()
    {
        refreshCount = 0;
        isCanRefreshTowerAdditionalEffects = true;
    }

    public List<TowerAdditionalEffect> randomTowerAdditionalEffects { get; private set; }
    public void SetRandomTowerAdditionalEffects(int count = 3)
    {
        randomTowerAdditionalEffects.Clear();

        TowerAdditionalEffect TAE;
        for(int i = count; i>0; --i)
        {
            if(TryGetRandomTAE(i, out TAE))
            {
                randomTowerAdditionalEffects.Add(TAE);
            }
            else
            {
                Debug.LogErrorFormat("TryGetRandomTAE is failure");
            }
        }
    }
    public bool TryGetRandomTAE(int tier, out TowerAdditionalEffect tae)
    {
        SkillData sData;
        List<int> skillTypes;

        if (GameSettingsManager.UseFixedTAE)
        {
            int skillType = 0;

            if (tier == 1) skillType = (int)GameSettingsManager.tier1FixedTAE;
            else if(tier == 2) skillType = (int)GameSettingsManager.tier2FixedTAE;
            else if(tier == 3) skillType = (int)GameSettingsManager.tier3FixedTAE;

            if (skillType == 0 && UserInfo.Instance.skillInv.TryGetTierByActiveSkillTypes(tier, out skillTypes))
            {
                // FixedTAE가 None으로 설정되어 있으면 랜덤
            }

            string skillID = UserInfo.Instance.skillInv.GetHighestLevelSkillID(skillType);
            if (DataManager.Instance.TryGetSkillData(skillID, out sData))
            {
                tae = TowerAdditionalEffect.Create(sData);
                return true;
            }
        }
        
        if (UserInfo.Instance.skillInv.TryGetTierByActiveSkillTypes(tier, out skillTypes))
        {
            int skillType = skillTypes[UnityEngine.Random.Range(0, skillTypes.Count)];
            string skillID = UserInfo.Instance.skillInv.GetHighestLevelSkillID(skillType);
            if(DataManager.Instance.TryGetSkillData(skillID, out sData))
            {
                tae = TowerAdditionalEffect.Create(sData);
                return true;
            }
        }

        tae = null;
        return false;
    }

    #endregion  

    #region < Select Grid Block >

    [SerializeField]
    private IngameSelectFrame m_IngameSelectFrame;
    public IngameSelectFrame ingameSelectFrame { get { return m_IngameSelectFrame; } }
    
    public GridBlock selectedGridBlock { get; private set; }
    public void SelectGridBlock(GridBlock gridBlock)
    {
        UnselectIngameObject();
        UnselectGridBlock();

        gridBlock.Select();
        selectedGridBlock = gridBlock;

        ingameSelectFrame.transform.position = selectedGridBlock.transform.position;
        ingameSelectFrame.SetGridFrame((gridBlock.isBuildTower) ? gridBlock.tower.attackRange : 0);
        ingameSelectFrame.Open();
    }
    public void UnselectGridBlock()
    {
        if (selectedGridBlock != null)
        {
            selectedGridBlock.Unselect();
        }
        selectedGridBlock = null;

        ingameSelectFrame.Close();
    }

    public IngameObject selectedIngmaeObject { get; private set; }
    public void SelectIngameObject(IngameObject ingameObject)
    {
        UnselectIngameObject();
        UnselectGridBlock();

        selectedIngmaeObject = ingameObject;

        ingameSelectFrame.transform.position = selectedIngmaeObject.transform.position;
        ingameSelectFrame.SetIngameObjectFrame(ingameObject);
        ingameSelectFrame.Open();
    }
    public void UnselectIngameObject()
    {
        //if (selectedIngmaeObject != null)
        //{
        //    selectedIngmaeObject.Unselect();
        //}
        selectedIngmaeObject = null;

        ingameSelectFrame.Close();
    }

    #endregion

    #region < Ingame Settings >

    private bool m_IsOnIngameInfo = false;
    public bool isOnIngameInfo { get { return m_IsOnIngameInfo; } set { m_IsOnIngameInfo = value; } }

    private bool m_IsGameStopped;
    public bool isGameStopped {
        get { return m_IsGameStopped; }
        set
        {
            m_IsGameStopped = value;
            Time.timeScale = (m_IsGameStopped) ? 0 : curGameSpeed;
        }
    }

    public int curGameSpeed = 1;
    public void ChangeGameSpeed()
    {
        curGameSpeed = ++curGameSpeed;
        if(curGameSpeed > 2)
        {
            curGameSpeed = 1;
        }
        Time.timeScale = curGameSpeed;
    }

    private bool m_IsLockWaveStart;
    public bool isLockWaveStart
    {
        get { return m_IsLockWaveStart; }
        set { m_IsLockWaveStart = value; }
    }

    #endregion

    private IEnumerator etorStageCycle;
    public StageData curStageData { get; private set; }
    public EStageMode curStageMode => curStageData.stageMode;

    public void Initialize()
    {
        ingameObjects = new Dictionary<string, IngameObject>();
        randomTowerAdditionalEffects = new List<TowerAdditionalEffect>();

        waveInfos = new Dictionary<int, IngameWaveInfo>();
        ingameObjectSpawnInfos = new List<IngameObjectSpawnInfo>();

        status = EStatus.Initialized;
    }

    public void Release()
    {
        status = EStatus.None;

        if (ingameObjectSpawnInfos != null) ingameObjectSpawnInfos.Clear();
        ingameObjectSpawnInfos = null;

        if (waveInfos != null) waveInfos.Clear();
        waveInfos = null;

        if (randomTowerAdditionalEffects != null) randomTowerAdditionalEffects.Clear();
        randomTowerAdditionalEffects = null;

        if (ingameObjects != null) ingameObjects.Clear();
        ingameObjects = null;

        OnChangeCost = null;
        OnChangeLife = null;
    }

    #region < Stage >

    public void StartStage(StageData stageData)
    {
        UserInfo.Instance.adsPlayCount++;

        status = EStatus.Playing;
        curStageData = stageData;
        isGameStopped = false;
        
        InitTAERefreshCount();

        startLife = stageData.startLife;
        lifeRefillCount = 0;

        SetLife(stageData.startLife);
        SetCost((GameSettingsManager.UseFixedStageCost) ? GameSettingsManager.fixedStageCost : stageData.startCost);
        SetBuildCost(100);
        SetRandomTowerAdditionalEffects();
        SetKillScore(0);
        
        AppManager.Instance.cameraController.SetZoomSize(stageData.ingameZoomSize);

        InputManager.RegistInputEvent(OnInputEvent);

        // 몬스터 스폰 정보 생성
        waveInfos.Clear();
        foreach (var d in stageData.waveDatas)
        {
            IngameWaveInfo waveInfo;
            if (!waveInfos.TryGetValue(d.wave, out waveInfo))
            {
                waveInfo = new IngameWaveInfo();
                waveInfos.Add(d.wave, waveInfo);
            }
            waveInfo.spawnInfos.Add(d);
        }

        UIManager_Ingame.Instance.Open();

        if (etorStageCycle != null)
        {
            StopCoroutine(etorStageCycle);
        }

        if(stageData.stageMode == EStageMode.Infinity)
        {
            UserInfo.Instance.infinityModePlayCount++;
            etorStageCycle = StageCycle_InfinityMode();
        }
        else
        {
            UserInfo.Instance.normalModePlayCount++;
            etorStageCycle = StageCycle();
        }

        StartTutorial();
        StartCoroutine(etorStageCycle);
    }

    public IEnumerator StageCycle()
    {
        GridInfo startGridInfo, endGridInfo;
        if (!AppManager.Instance.gridMap.TryFindGridInfo
            ((info) => { return info.gridBlock.gridBlockType == GridBlock.EType.StartPoint; }
            , out startGridInfo))
        {
            Debug.LogErrorFormat("Start Grid Info not found.");
            yield break;
        }
        else if (!AppManager.Instance.gridMap.TryFindGridInfo
             ((info) => { return info.gridBlock.gridBlockType == GridBlock.EType.EndPoint; }
             , out endGridInfo))
        {
            Debug.LogErrorFormat("End Grid Info not found.");
            yield break;
        }

        var ingameUIManager = UIManager_Ingame.Instance;

        yield return new WaitForSeconds(1f);

        curWave = 0;
        // 다음 웨이브 생성
        do
        {
            UIManager_Ingame.Instance.topUI.SetStageTxt(AppManager.Instance.curStageIdx, ++curWave);

            yield return new WaitWhile(() => { return isLockWaveStart; });

            ingameUIManager.stageStartUI.SetText(string.Format("WAVE {0}", curWave));
            ingameUIManager.stageStartUI.Open();

            yield return new WaitForSeconds(1f);

            ingameUIManager.stageStartUI.SetText("3");
            ingameUIManager.stageStartUI.Open();

            yield return new WaitForSeconds(1f);

            ingameUIManager.stageStartUI.SetText("2");
            ingameUIManager.stageStartUI.Open();

            yield return new WaitForSeconds(1f);

            ingameUIManager.stageStartUI.SetText("1");
            ingameUIManager.stageStartUI.Open();

            yield return new WaitForSeconds(1f);

            ingameUIManager.stageStartUI.SetText("START!!");
            ingameUIManager.stageStartUI.Open();

            yield return new WaitForSeconds(1f);

            ingameUIManager.stageStartUI.Close();

            yield return null;

            IngameWaveInfo curWaveInfo;
            if (waveInfos.TryGetValue(curWave, out curWaveInfo))
            {
                bool endSpawnWave = true;
                int[] curSpawnCounts = new int[curWaveInfo.spawnInfos.Count];
                do
                {
                    endSpawnWave = true;

                    for (int i = 0; i < curWaveInfo.spawnInfos.Count; ++i)
                    {
                        var info = curWaveInfo.spawnInfos[i];
                        if (curSpawnCounts[i] < info.spawnCount)
                        {
                            endSpawnWave = false;
                            if (info.startSpawnDelayTimer > 0)
                            {
                                info.startSpawnDelayTimer -= Time.deltaTime;
                            }
                            else if (info.spawnDelayTimer > 0)
                            {
                                info.spawnDelayTimer -= Time.deltaTime;
                            }
                            else
                            {
                                IngameObject enemy = null;
                                IngameUI_UnitHp hpUI = null;
                                if (SpawnMaster.TrySpawnIngameObject(info.prefabKey, startGridInfo.gridBlock.transform.position, Quaternion.identity, out enemy)
                                    && SpawnMaster.TrySpawnUI(IngameUI_UnitHp.PrefabKey, UIManager_Ingame.Instance.middleUI.transform, out hpUI))
                                {
                                    hpUI.SetData(enemy);
                                    hpUI.Open();

                                    enemy.SetData(new IngameObjectData
                                        (info.ingameObjData.unitType
                                        , info.ingameObjData.maxHp
                                        , info.ingameObjData.moveSpeed
                                        , info.ingameObjData.cost
                                        , info.ingameObjData.defense
                                        , info.level
                                        , info.ingameObjData.unitClass
                                        , info.ingameObjData.regeneration
                                        , info.ingameObjData.resistance
                                        , info.ingameObjData.maxShield
                                        , info.ingameObjData.skillIds)
                                        , hpUI);
                                    enemy.SetMoveRoot(startGridInfo.gridBlock, endGridInfo.gridBlock);
                                    enemy.gameObject.name = "IngameObject" + OBJECT_ID++;

                                    enemy.Ready();
                                    RegistObject(enemy);

                                    info.spawnDelayTimer = info.spawnDelay;
                                    ++curSpawnCounts[i];
                                }
                                else
                                {
                                    curSpawnCounts[i] = info.spawnCount;
                                    Debug.LogErrorFormat("IngameManager :: Can't spawned enemy({0}) or hpUI({1})", info.prefabKey, hpUI);
                                }
                            }
                        }
                    }

                    yield return null;
                }
                while (!endSpawnWave);
            }

            yield return new WaitForSeconds(1f);

            float delay = GameSettingsManager.NextWaveDelayTime;
            // 모든 유닛이 죽거나 5초가 지나면 다음 웨이브 진행
            do
            {
                yield return null;

                if(delay > 0)
                {
                    delay -= Time.deltaTime;
                }
                else
                {
                    break;
                }
            }
            while (CheckIngameObjectCondition((obj) => { return !obj.isDie; }));
        }
        while (waveInfos.Count > curWave);

        yield return new WaitForSeconds(1f);
        
        // 모든 유닛들이 죽을 때까지 대기
        while (CheckIngameObjectCondition((obj) => { return !obj.isDie; }))
        {
            yield return null;
        }

        ReadyEndStage(true);
    }
    
    public IEnumerator StageCycle_InfinityMode()
    {
        // 랜덤으로 맵을 생성한다.
        var stageMapData = DataManager.Instance.GetRandomStageMapData();
        AppManager.Instance.gridMap.Claer();
        AppManager.Instance.gridMap.Create(stageMapData.mapData
            , stageMapData.mapSkyData
            , AppManager.Instance.gridMapCenterTf.position
            , stageMapData.tileSize
            , stageMapData.tileSize
            , stageMapData.wallData);

        AppManager.Instance.cameraController.SetZoomSize(stageMapData.ingameZoomSize);

        GridInfo startGridInfo, endGridInfo;
        if (!AppManager.Instance.gridMap.TryFindGridInfo
            ((info) => { return info.gridBlock.gridBlockType == GridBlock.EType.StartPoint; }
            , out startGridInfo))
        {
            Debug.LogErrorFormat("Start Grid Info not found.");
            yield break;
        }
        else if (!AppManager.Instance.gridMap.TryFindGridInfo
             ((info) => { return info.gridBlock.gridBlockType == GridBlock.EType.EndPoint; }
             , out endGridInfo))
        {
            Debug.LogErrorFormat("End Grid Info not found.");
            yield break;
        }

        var ingameUIManager = UIManager_Ingame.Instance;

        yield return new WaitForSeconds(1f);

        curWave = 0;
        // 다음 웨이브 생성
        do
        {
            if (++curWave % 10 == 1)
            {
                var prefabType1 = DataManager.Instance.GetRandomPrefabKey(IngameObject.EType.Walker);
                var prefabType2 = DataManager.Instance.GetRandomPrefabKey(IngameObject.EType.Destroyer);
                var prefabType3 = DataManager.Instance.GetRandomPrefabKey(IngameObject.EType.Flyer);

#if UNITY_EDITOR
                Debug.LogFormat("P1({0}) P2({1}) P3({2})", prefabType1, prefabType2, prefabType3);
#endif

                int wave10 = curWave / 10;
                List<IngameObjectSpawnInfo> spawnInfos;

                #region < Create Random SpawnInfos >
                switch (UnityEngine.Random.Range(0, 4))
                {
                    default:
                    case 0:
                        spawnInfos = new List<IngameObjectSpawnInfo>()
                        {
                            new IngameObjectSpawnInfo(curWave + 0, prefabType1, "IOD-1-2-2", 20, 1.0f, 00, 0.30f + ((curWave + 0) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 0) / 4 * 0.1f)), 0.85f + ((curWave + 0) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 0 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 0, prefabType3, "IOD-3-3-2", 40, 0.5f, 01, 0.30f + ((curWave + 0) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 0) / 4 * 0.1f)), 0.85f + ((curWave + 0) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 0 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 1, prefabType1, "IOD-1-2-2", 20, 1.0f, 00, 0.40f + ((curWave + 1) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 1) / 4 * 0.1f)), 0.95f + ((curWave + 1) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 1 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 1, prefabType3, "IOD-3-3-2", 40, 0.5f, 01, 0.40f + ((curWave + 1) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 1) / 4 * 0.1f)), 0.95f + ((curWave + 1) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 1 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 2, prefabType1, "IOD-1-2-2", 20, 1.0f, 00, 0.50f + ((curWave + 2) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 2) / 4 * 0.1f)), 1.00f + ((curWave + 2) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 2 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 2, prefabType3, "IOD-3-3-2", 40, 0.5f, 01, 0.50f + ((curWave + 2) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 2) / 4 * 0.1f)), 1.00f + ((curWave + 2) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 2 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 3, prefabType2, "IOD-2-2-2", 20, 1.0f, 00, 0.50f + ((curWave + 3) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 3) / 4 * 0.1f)), 1.05f + ((curWave + 3) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 3 - 10) * 0.01f)),// Destroy
                            new IngameObjectSpawnInfo(curWave + 3, prefabType3, "IOD-3-3-2", 40, 0.5f, 01, 0.50f + ((curWave + 3) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 3) / 4 * 0.1f)), 1.05f + ((curWave + 3) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 3 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 4, prefabType1, "IOD-1-2-2", 02, 1.0f, 00, 0.50f + ((curWave + 4) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 4) / 4 * 0.1f)), 1.92f + ((curWave + 4) * 0.02f), IngameObject.EClass.Hero  , Mathf.Max(0, (curWave + 4 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 4, prefabType3, "IOD-3-3-2", 40, 0.5f, 01, 0.50f + ((curWave + 4) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 4) / 4 * 0.1f)), 1.46f + ((curWave + 4) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 4 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 5, prefabType1, "IOD-1-2-2", 20, 1.0f, 00, 0.50f + ((curWave + 5) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 5) / 4 * 0.1f)), 1.10f + ((curWave + 5) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 5 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 5, prefabType3, "IOD-3-3-2", 40, 0.5f, 01, 0.50f + ((curWave + 5) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 5) / 4 * 0.1f)), 1.10f + ((curWave + 5) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 5 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 6, prefabType1, "IOD-1-2-2", 20, 1.0f, 00, 0.50f + ((curWave + 6) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 6) / 4 * 0.1f)), 1.15f + ((curWave + 6) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 6 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 6, prefabType3, "IOD-3-3-2", 40, 0.5f, 01, 0.50f + ((curWave + 6) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 6) / 4 * 0.1f)), 1.15f + ((curWave + 6) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 6 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 7, prefabType1, "IOD-1-2-2", 20, 1.0f, 00, 0.50f + ((curWave + 7) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 7) / 4 * 0.1f)), 1.20f + ((curWave + 7) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 7 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 7, prefabType3, "IOD-3-3-2", 40, 0.5f, 01, 0.50f + ((curWave + 7) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 7) / 4 * 0.1f)), 1.20f + ((curWave + 7) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 7 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 8, prefabType1, "IOD-1-2-2", 20, 1.0f, 00, 0.50f + ((curWave + 8) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 8) / 4 * 0.1f)), 1.25f + ((curWave + 8) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 8 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 8, prefabType3, "IOD-3-3-2", 40, 0.5f, 01, 0.50f + ((curWave + 8) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 8) / 4 * 0.1f)), 1.25f + ((curWave + 8) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 8 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 9, prefabType1, "IOD-1-2-2", 01, 2.0f, 00, 0.50f + ((curWave + 9) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 9) / 4 * 0.1f)), 1.84f + ((curWave + 9) * 0.02f), IngameObject.EClass.Boss  , Mathf.Max(0, (curWave + 9 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 9, prefabType3, "IOD-3-3-2", 40, 0.5f, 01, 0.50f + ((curWave + 9) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 9) / 4 * 0.1f)), 1.42f + ((curWave + 9) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 9 - 10) * 0.01f)),
                        };
                        break;

                    case 1:
                        spawnInfos = new List<IngameObjectSpawnInfo>()
                        {
                            new IngameObjectSpawnInfo(curWave + 0, prefabType1, "IOD-1-2-2", 10, 2, 00, 0.30f + ((curWave + 0) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 0) / 4 * 0.1f)), 0.85f + ((curWave + 0) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 0 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 0, prefabType3, "IOD-3-2-2", 20, 1, 01, 0.30f + ((curWave + 0) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 0) / 4 * 0.1f)), 0.85f + ((curWave + 0) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 0 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 1, prefabType1, "IOD-1-2-2", 10, 2, 00, 0.40f + ((curWave + 1) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 1) / 4 * 0.1f)), 0.95f + ((curWave + 1) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 1 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 1, prefabType3, "IOD-3-2-2", 20, 1, 01, 0.40f + ((curWave + 1) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 1) / 4 * 0.1f)), 0.95f + ((curWave + 1) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 1 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 2, prefabType1, "IOD-2-1-2", 05, 4, 00, 0.50f + ((curWave + 2) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 2) / 4 * 0.1f)), 1.00f + ((curWave + 2) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 2 - 10) * 0.01f)), // Destroy
                            new IngameObjectSpawnInfo(curWave + 2, prefabType3, "IOD-3-2-2", 20, 1, 01, 0.50f + ((curWave + 2) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 2) / 4 * 0.1f)), 1.00f + ((curWave + 2) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 2 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 3, prefabType2, "IOD-2-2-2", 10, 2, 00, 0.50f + ((curWave + 3) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 3) / 4 * 0.1f)), 1.05f + ((curWave + 3) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 3 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 3, prefabType3, "IOD-3-2-2", 20, 1, 01, 0.50f + ((curWave + 3) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 3) / 4 * 0.1f)), 1.05f + ((curWave + 3) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 3 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 4, prefabType1, "IOD-1-2-2", 01, 2, 00, 0.50f + ((curWave + 4) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 4) / 4 * 0.1f)), 1.92f + ((curWave + 4) * 0.02f), IngameObject.EClass.Hero  , Mathf.Max(0, (curWave + 4 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 4, prefabType3, "IOD-3-2-2", 20, 1, 01, 0.50f + ((curWave + 4) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 4) / 4 * 0.1f)), 1.46f + ((curWave + 4) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 4 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 5, prefabType2, "IOD-2-1-2", 05, 4, 00, 0.52f + ((curWave + 5) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 5) / 4 * 0.1f)), 1.10f + ((curWave + 5) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 5 - 10) * 0.01f)), // Destroy
                            new IngameObjectSpawnInfo(curWave + 5, prefabType3, "IOD-3-2-2", 20, 1, 01, 0.52f + ((curWave + 5) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 5) / 4 * 0.1f)), 1.10f + ((curWave + 5) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 5 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 6, prefabType1, "IOD-1-2-2", 10, 2, 00, 0.54f + ((curWave + 6) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 6) / 4 * 0.1f)), 1.15f + ((curWave + 6) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 6 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 6, prefabType3, "IOD-3-2-2", 20, 1, 01, 0.54f + ((curWave + 6) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 6) / 4 * 0.1f)), 1.15f + ((curWave + 6) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 6 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 7, prefabType1, "IOD-1-2-2", 10, 2, 00, 0.56f + ((curWave + 7) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 7) / 4 * 0.1f)), 1.20f + ((curWave + 7) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 7 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 7, prefabType3, "IOD-3-2-2", 20, 1, 01, 0.56f + ((curWave + 7) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 7) / 4 * 0.1f)), 1.20f + ((curWave + 7) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 7 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 8, prefabType2, "IOD-2-1-2", 05, 4, 00, 0.58f + ((curWave + 8) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 8) / 4 * 0.1f)), 1.25f + ((curWave + 8) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 8 - 10) * 0.01f)), // Destroy
                            new IngameObjectSpawnInfo(curWave + 8, prefabType3, "IOD-3-2-2", 20, 1, 01, 0.58f + ((curWave + 8) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 8) / 4 * 0.1f)), 1.25f + ((curWave + 8) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 8 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 9, prefabType1, "IOD-1-2-2", 01, 2, 00, 0.50f + ((curWave + 9) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 9) / 4 * 0.1f)), 1.84f + ((curWave + 9) * 0.02f), IngameObject.EClass.Boss  , Mathf.Max(0, (curWave + 9 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 9, prefabType3, "IOD-3-2-2", 20, 1, 01, 0.50f + ((curWave + 9) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 9) / 4 * 0.1f)), 1.42f + ((curWave + 9) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 9 - 10) * 0.01f)),
                        };
                        break;

                    case 2:
                        spawnInfos = new List<IngameObjectSpawnInfo>()
                        {
                            new IngameObjectSpawnInfo(curWave + 0, prefabType1, "IOD-1-1-2", 10, 2, 00, 0.30f + ((curWave + 0) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 0) / 4 * 0.1f)), 0.85f + ((curWave + 0) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 0 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 0, prefabType3, "IOD-3-1-2", 10, 2, 01, 0.30f + ((curWave + 0) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 0) / 4 * 0.1f)), 0.85f + ((curWave + 0) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 0 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 1, prefabType1, "IOD-1-1-2", 10, 2, 00, 0.40f + ((curWave + 1) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 1) / 4 * 0.1f)), 0.95f + ((curWave + 1) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 1 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 1, prefabType3, "IOD-3-1-2", 10, 2, 01, 0.40f + ((curWave + 1) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 1) / 4 * 0.1f)), 0.95f + ((curWave + 1) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 1 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 2, prefabType1, "IOD-1-1-2", 10, 2, 00, 0.50f + ((curWave + 2) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 2) / 4 * 0.1f)), 1.00f + ((curWave + 2) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 2 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 2, prefabType3, "IOD-3-1-2", 10, 2, 01, 0.50f + ((curWave + 2) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 2) / 4 * 0.1f)), 1.00f + ((curWave + 2) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 2 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 3, prefabType2, "IOD-2-1-2", 10, 2, 00, 0.50f + ((curWave + 3) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 3) / 4 * 0.1f)), 1.05f + ((curWave + 3) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 3 - 10) * 0.01f)),// Destroy
                            new IngameObjectSpawnInfo(curWave + 3, prefabType3, "IOD-3-1-2", 10, 2, 01, 0.50f + ((curWave + 3) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 3) / 4 * 0.1f)), 1.05f + ((curWave + 3) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 3 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 4, prefabType1, "IOD-1-1-2", 01, 2, 00, 0.50f + ((curWave + 4) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 4) / 4 * 0.1f)), 1.92f + ((curWave + 4) * 0.02f), IngameObject.EClass.Hero  , Mathf.Max(0, (curWave + 4 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 4, prefabType3, "IOD-3-1-2", 10, 2, 01, 0.50f + ((curWave + 4) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 4) / 4 * 0.1f)), 1.46f + ((curWave + 4) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 4 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 5, prefabType1, "IOD-1-1-2", 10, 2, 00, 0.50f + ((curWave + 5) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 5) / 4 * 0.1f)), 1.10f + ((curWave + 5) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 5 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 5, prefabType3, "IOD-3-1-2", 10, 2, 01, 0.50f + ((curWave + 5) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 5) / 4 * 0.1f)), 1.10f + ((curWave + 5) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 5 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 6, prefabType1, "IOD-1-1-2", 10, 2, 00, 0.50f + ((curWave + 6) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 6) / 4 * 0.1f)), 1.15f + ((curWave + 6) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 6 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 6, prefabType3, "IOD-3-1-2", 10, 2, 01, 0.50f + ((curWave + 6) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 6) / 4 * 0.1f)), 1.15f + ((curWave + 6) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 6 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 7, prefabType1, "IOD-1-1-2", 10, 2, 00, 0.50f + ((curWave + 7) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 7) / 4 * 0.1f)), 1.20f + ((curWave + 7) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 7 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 7, prefabType3, "IOD-3-1-2", 10, 2, 01, 0.50f + ((curWave + 7) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 7) / 4 * 0.1f)), 1.20f + ((curWave + 7) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 7 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 8, prefabType1, "IOD-1-1-2", 10, 2, 00, 0.50f + ((curWave + 8) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 8) / 4 * 0.1f)), 1.25f + ((curWave + 8) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 8 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 8, prefabType3, "IOD-3-1-2", 10, 2, 01, 0.50f + ((curWave + 8) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 8) / 4 * 0.1f)), 1.25f + ((curWave + 8) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 8 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 9, prefabType1, "IOD-1-1-2", 01, 2, 00, 0.50f + ((curWave + 9) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 9) / 4 * 0.1f)), 1.84f + ((curWave + 9) * 0.02f), IngameObject.EClass.Boss  , Mathf.Max(0, (curWave + 9 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 9, prefabType3, "IOD-3-1-2", 10, 2, 01, 0.50f + ((curWave + 9) * 0.003f), Mathf.Max(0.1f, 0.5f - ((curWave + 9) / 4 * 0.1f)), 1.42f + ((curWave + 9) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 9 - 10) * 0.01f))
                        };
                        break;

                    case 3:
                        spawnInfos = new List<IngameObjectSpawnInfo>()
                        {
                            new IngameObjectSpawnInfo(curWave + 0, prefabType1, "IOD-1-1-2", 10, 2, 00, 0.30f + ((curWave + 0) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 0) / 4 * 0.1f)), 0.85f + ((curWave + 0) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 0 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 0, prefabType3, "IOD-3-2-2", 10, 2, 01, 0.30f + ((curWave + 0) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 0) / 4 * 0.1f)), 0.85f + ((curWave + 0) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 0 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 1, prefabType1, "IOD-1-1-2", 10, 2, 00, 0.40f + ((curWave + 1) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 1) / 4 * 0.1f)), 0.95f + ((curWave + 1) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 1 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 1, prefabType3, "IOD-3-2-2", 10, 2, 01, 0.40f + ((curWave + 1) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 1) / 4 * 0.1f)), 0.95f + ((curWave + 1) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 1 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 2, prefabType1, "IOD-1-1-2", 10, 2, 00, 0.50f + ((curWave + 2) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 2) / 4 * 0.1f)), 1.00f + ((curWave + 2) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 2 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 2, prefabType3, "IOD-3-2-2", 10, 2, 01, 0.50f + ((curWave + 2) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 2) / 4 * 0.1f)), 1.00f + ((curWave + 2) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 2 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 3, prefabType2, "IOD-2-1-2", 10, 2, 00, 0.50f + ((curWave + 3) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 3) / 4 * 0.1f)), 1.05f + ((curWave + 3) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 3 - 10) * 0.01f)),// Destroy
                            new IngameObjectSpawnInfo(curWave + 3, prefabType3, "IOD-3-2-2", 10, 2, 01, 0.50f + ((curWave + 3) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 3) / 4 * 0.1f)), 1.05f + ((curWave + 3) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 3 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 4, prefabType1, "IOD-1-1-2", 01, 2, 00, 0.50f + ((curWave + 4) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 4) / 4 * 0.1f)), 1.92f + ((curWave + 4) * 0.02f), IngameObject.EClass.Hero  , Mathf.Max(0, (curWave + 4 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 4, prefabType3, "IOD-3-2-2", 10, 2, 01, 0.50f + ((curWave + 4) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 4) / 4 * 0.1f)), 1.46f + ((curWave + 4) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 4 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 5, prefabType1, "IOD-1-1-2", 10, 2, 00, 0.50f + ((curWave + 5) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 5) / 4 * 0.1f)), 1.10f + ((curWave + 5) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 5 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 5, prefabType3, "IOD-3-2-2", 10, 2, 01, 0.50f + ((curWave + 5) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 5) / 4 * 0.1f)), 1.10f + ((curWave + 5) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 5 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 6, prefabType1, "IOD-1-1-2", 10, 2, 00, 0.50f + ((curWave + 6) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 6) / 4 * 0.1f)), 1.15f + ((curWave + 6) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 6 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 6, prefabType3, "IOD-3-2-2", 10, 2, 01, 0.50f + ((curWave + 6) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 6) / 4 * 0.1f)), 1.15f + ((curWave + 6) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 6 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 7, prefabType1, "IOD-1-1-2", 10, 2, 00, 0.50f + ((curWave + 7) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 7) / 4 * 0.1f)), 1.20f + ((curWave + 7) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 7 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 7, prefabType3, "IOD-3-2-2", 10, 2, 01, 0.50f + ((curWave + 7) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 7) / 4 * 0.1f)), 1.20f + ((curWave + 7) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 7 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 8, prefabType1, "IOD-1-1-2", 10, 2, 00, 0.50f + ((curWave + 8) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 8) / 4 * 0.1f)), 1.25f + ((curWave + 8) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 8 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 8, prefabType3, "IOD-3-2-2", 10, 2, 01, 0.50f + ((curWave + 8) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 8) / 4 * 0.1f)), 1.25f + ((curWave + 8) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 8 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 9, prefabType1, "IOD-1-1-2", 01, 2, 00, 0.50f + ((curWave + 9) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 9) / 4 * 0.1f)), 1.84f + ((curWave + 9) * 0.02f), IngameObject.EClass.Boss  , Mathf.Max(0, (curWave + 9 - 10) * 0.01f)),
                            new IngameObjectSpawnInfo(curWave + 9, prefabType3, "IOD-3-2-2", 10, 2, 01, 0.50f + ((curWave + 9) * 0.002f), Mathf.Max(0.1f, 0.5f - ((curWave + 9) / 4 * 0.1f)), 1.42f + ((curWave + 9) * 0.02f), IngameObject.EClass.Normal, Mathf.Max(0, (curWave + 9 - 10) * 0.01f)),
                        };
                        break;
                }
                #endregion

                // 몬스터 스폰 정보 추가
                foreach (var info in spawnInfos)
                {
                    IngameWaveInfo waveInfo;
                    if (!waveInfos.TryGetValue(info.wave, out waveInfo))
                    {
                        waveInfo = new IngameWaveInfo();
                        waveInfos.Add(info.wave, waveInfo);
                    }
                    waveInfo.spawnInfos.Add(info);
                }
            }

            UIManager_Ingame.Instance.topUI.SetStageTxt(AppManager.Instance.curStageIdx, curWave);

            yield return new WaitWhile(() => { return isLockWaveStart; });

            ingameUIManager.stageStartUI.SetText(string.Format("WAVE {0}", curWave));
            ingameUIManager.stageStartUI.Open();

            yield return new WaitForSeconds(1f);

            ingameUIManager.stageStartUI.SetText("3");
            ingameUIManager.stageStartUI.Open();

            yield return new WaitForSeconds(1f);

            ingameUIManager.stageStartUI.SetText("2");
            ingameUIManager.stageStartUI.Open();

            yield return new WaitForSeconds(1f);

            ingameUIManager.stageStartUI.SetText("1");
            ingameUIManager.stageStartUI.Open();

            yield return new WaitForSeconds(1f);

            ingameUIManager.stageStartUI.SetText("START!!");
            ingameUIManager.stageStartUI.Open();

            yield return new WaitForSeconds(1f);

            ingameUIManager.stageStartUI.Close();

            yield return null;

            IngameWaveInfo curWaveInfo;
            if (waveInfos.TryGetValue(curWave, out curWaveInfo))
            {
                bool endSpawnWave = true;
                int[] curSpawnCounts = new int[curWaveInfo.spawnInfos.Count];
                do
                {
                    endSpawnWave = true;

                    for(int i = 0; i< curWaveInfo.spawnInfos.Count; ++i)
                    {
                        var info = curWaveInfo.spawnInfos[i];
                        if(curSpawnCounts[i] < info.spawnCount)
                        {
                            endSpawnWave = false;
                            if (info.startSpawnDelayTimer > 0)
                            {
                                info.startSpawnDelayTimer -= Time.deltaTime;
                            }
                            else if (info.spawnDelayTimer > 0)
                            {
                                info.spawnDelayTimer -= Time.deltaTime;
                            }
                            else
                            {
                                IngameObject enemy = null;
                                IngameUI_UnitHp hpUI = null;
                                if (SpawnMaster.TrySpawnIngameObject(info.prefabKey, startGridInfo.gridBlock.transform.position, Quaternion.identity, out enemy)
                                    && SpawnMaster.TrySpawnUI(IngameUI_UnitHp.PrefabKey, UIManager_Ingame.Instance.middleUI.transform, out hpUI))
                                {
                                    hpUI.SetData(enemy);
                                    hpUI.Open();

                                    enemy.SetData(new IngameObjectData
                                        (info.ingameObjData.unitType
                                        , info.ingameObjData.maxHp
                                        , info.ingameObjData.moveSpeed
                                        , info.ingameObjData.cost
                                        , info.ingameObjData.defense
                                        , info.level
                                        , info.ingameObjData.unitClass
                                        , info.ingameObjData.regeneration
                                        , info.ingameObjData.resistance
                                        , info.ingameObjData.maxShield
                                        , info.ingameObjData.skillIds)
                                        , hpUI);
                                    enemy.SetMoveRoot(startGridInfo.gridBlock, endGridInfo.gridBlock);
                                    enemy.gameObject.name = "IngameObject" + OBJECT_ID++;

                                    enemy.Ready();
                                    RegistObject(enemy);

                                    info.spawnDelayTimer = info.spawnDelay;

                                    ++curSpawnCounts[i];
                                }
                                else
                                {
                                    curSpawnCounts[i] = info.spawnCount;
                                    Debug.LogErrorFormat("IngameManager :: Can't spawned enemy({0}) or hpUI({1})", info.prefabKey, hpUI);
                                }
                            }
                        }
                    }

                    yield return null;
                }
                while (!endSpawnWave);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogErrorFormat("IngameWaveInfo({0}) not found.", curWave);
#endif
            }

            yield return new WaitForSeconds(1f);

            float delay = GameSettingsManager.NextWaveDelayTime;
            // 모든 유닛이 죽거나 5초가 지나면 다음 웨이브 진행
            do
            {
                yield return null;

                if (delay > 0)
                {
                    delay -= Time.deltaTime;
                }
                else
                {
                    break;
                }
            }
            while (CheckIngameObjectCondition((obj) => { return !obj.isDie; }));
        }
        while (true);
    }

    public void ReadyEndStage(bool win, bool isGiveUp = false)
    {
        StopAllCoroutines();
        StartCoroutine(ReadyingEndStage(win, isGiveUp));
    }

    public IEnumerator ReadyingEndStage(bool win, bool isGiveUp)
    {
        status = EStatus.Ending;

        if (etorStageCycle != null)
        {
            StopCoroutine(etorStageCycle);
        }

        foreach (var obj in ingameObjects)
        {
            obj.Value.Die();
        }

        yield return new WaitForSeconds(1.5f);

        UnselectGridBlock();
        UnselectIngameObject();

        ingameObjectSpawnInfos.Clear();
        randomTowerAdditionalEffects.Clear();

        AppManager.Instance.gridMap.Claer();

        int curStarCount = 0;
        StageData stageData;
        if (DataManager.Instance.TryStageData(AppManager.Instance.curStageIdx, out stageData))
        {
            int needLifeForOneStar = stageData.startLife / 2;
            if (!win)
            {
                curStarCount = 0;
            }
            else if (stageData.startLife == curLife)
            {
                curStarCount = 3;
            }
            else if ((stageData.startLife / 2) < curLife)
            {
                curStarCount = 2;
            }
            else
            {
                curStarCount = 1;
            }
        }

        if (curStageMode == EStageMode.Normal)
        {
            var preStarCount = UserInfo.Instance.GetStageStarCount(AppManager.Instance.curStageId);

            // 최초 획득 별에 따른 보상
            var takeStarCount = (win) ? Mathf.Max(0, (curStarCount - preStarCount) * 10) : 0;

            // 웨이브 진행도에 따른 보상
            takeStarCount += curWave / 6;

            // 매 플레이마다 달성한 별에 따른 보상
            takeStarCount += curStarCount;

            UserInfo.Instance.SetStageGoldScore(AppManager.Instance.curStageId, curCost);
            UserInfo.Instance.SetStageStarCount(AppManager.Instance.curStageId, (isGiveUp) ? 0 : curStarCount);

#if UNITY_ANDROID
            Social.Active.ReportScore
                ( (long)UserInfo.Instance.GetTotalStageGoldScore()
                , GPGSIds.leaderboard_high_gold_scoreall_stage
                , (result) => { });
#endif

            UIManager_Ingame.Instance.gameResultUI.SetData(win, (isGiveUp) ? 0 : curStarCount, takeStarCount, isGiveUp, curCost);
            UIManager_Ingame.Instance.gameResultUI.Open();
        }
        else if (curStageMode == EStageMode.Infinity)
        {
            // 웨이브 진행도에 따른 보상
            var takeStarCount = curWave / 6;

            UserInfo.Instance.SetStageStarCount(AppManager.Instance.curStageId, (isGiveUp) ? 0 : curStarCount);
            UserInfo.Instance.SetInfinityModeStageBestKillScore(killScore);

#if UNITY_ANDROID
            Social.Active.ReportScore
                ( (long)killScore
                , GPGSIds.leaderboard_high_scoreinfinity_mode
                , (result) => { });
#endif

            UIManager_Ingame.Instance.gameResultUI.SetIntinityModeData(takeStarCount, isGiveUp, killScore);
            UIManager_Ingame.Instance.gameResultUI.Open();
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogErrorFormat("EStageMode({0}) is not implemented.", curStageMode);
#endif

            UIManager_Ingame.Instance.gameResultUI.SetData(win, 0, 0, isGiveUp, 0);
            UIManager_Ingame.Instance.gameResultUI.Open();
        }
    }

    public void EndStage()
    {
        StopTutorial();

        curWave = 0;
        curGameSpeed = 1;
        Time.timeScale = curGameSpeed;

        UIManager_Ingame.Instance.Close();

        InputManager.UnregistInputEvent(OnInputEvent);

        status = EStatus.Ended;
    }

#endregion
    
    public void OnInputEvent(InputData data, EInputType inputType)
    {
        if (inputType == EInputType.PointDown)
        {
            if (data.clickObj != null)
            {
                switch (data.clickObj.tag)
                {
                    case "IngameObject":
                        UnselectGridBlock();
                        UnselectIngameObject();
                        var ingameObject = data.clickObj.GetComponent<IngameObject>();
                        if (ingameObject != null && !ingameObject.isDie)
                        {
                            SelectIngameObject(ingameObject);

                            UIManager_Ingame.Instance.gridInfoUI.SetEnemyInfoData(ingameObject);
                            UIManager_Ingame.Instance.gridInfoUI.Open();
                        }
                        break;

                    case "GridBlock":
                        if (InputManager.Instance.currentState == InputManager.EState.NONE)
                        {
                            var gridBlock = data.clickObj.GetComponent<GridBlock>();
                            if (gridBlock != null)
                            {
                                if (selectedGridBlock != null && selectedGridBlock.gridPos == gridBlock.gridPos)
                                {
                                    UnselectGridBlock();
                                    UnselectIngameObject();
                                    UIManager_Ingame.Instance.gridInfoUI.Close();
                                }
                                else
                                {
                                    SelectGridBlock(gridBlock);
                                    if (gridBlock.isBuildTower)
                                    {
                                        // 타워가 설치된 그리드
                                        UIManager_Ingame.Instance.gridInfoUI.Open();
                                        UIManager_Ingame.Instance.gridInfoUI.SetTowerInfoData(gridBlock.tower);
                                    }
                                    else if (gridBlock.EqualGridType(GridBlock.EType.StartPoint))
                                    {
                                        // 웨이브 시작 그리드
                                        UIManager_Ingame.Instance.gridInfoUI.Open();
                                        UIManager_Ingame.Instance.gridInfoUI.SetStartInfoData(curWave);
                                    }
                                    else if (!gridBlock.EqualGridType(GridBlock.EType.None))
                                    {
                                        // 기타
                                        UIManager_Ingame.Instance.gridInfoUI.Open();
                                        UIManager_Ingame.Instance.gridInfoUI.SetMessageData(gridBlock);
                                    }
                                    else
                                    {
                                        UnselectGridBlock();
                                        UnselectIngameObject();
                                        UIManager_Ingame.Instance.gridInfoUI.Close();
                                    }
                                }
                            }
                            else
                            {
                                UnselectGridBlock();
                                UnselectIngameObject();
                                UIManager_Ingame.Instance.gridInfoUI.Close();
                            }
                        }
                        break;

                    default:
                        UIManager_Ingame.Instance.gridInfoUI.Close();
                        UnselectGridBlock();
                        UnselectIngameObject();
                        break;
                }
            }
            else
            {
                UIManager_Ingame.Instance.gridInfoUI.Close();
                UnselectGridBlock();
                UnselectIngameObject();
            }
        }
        else if (inputType == EInputType.PointUp && data.clickObj != null)
        {
            //switch (data.clickObj.tag)
            //{
            //    case "IngameObject":
            //        UnselectGridBlock();
            //        var ingameObject = data.clickObj.GetComponent<IngameObject>();
            //        if (ingameObject != null && !ingameObject.isDie)
            //        {
            //            ingameObject.Attacked(new IngameAttackInfo()
            //            {
            //                attaker = null,
            //                damage = 1,
            //                damageType = EDamageType.Touch,
            //                isCritical = false,
            //                isPenetrate = false
            //            });
            //        }
            //        break;

            //    case "GridBlock":
            //        if (InputManager.Instance.currentState == InputManager.EState.NONE)
            //        {
            //            var gridBlock = data.clickObj.GetComponent<GridBlock>();
            //            if (gridBlock != null && gridBlock.isBuildTower)
            //            {
            //                SelectGridBlock(gridBlock);
            //                UIManager_Ingame.Instance.towerInfoUI.SetData(gridBlock.tower);
            //                UIManager_Ingame.Instance.towerInfoUI.Open();
            //                UIManager_Ingame.Instance.towerInfoUI.UpdateUI();
            //            }
            //        }
            //        break;
            //}
        }
        else if (inputType == EInputType.Drag && data.clickObj == null)
        {
            var dragDis = Vector3.Distance(data.clickPos, data.deltaPos);
            if (dragDis > 5f)
            {
                // 화면 이동
                AppManager.Instance.cameraController.Move(data.deltaPos * Time.deltaTime * GameSettingsManager.CameraScrollSensitivity);
            }
        }
    }

    #region < Tutorial >

    public void StartTutorial()
    {
        if (!UserInfo.Instance.CheckTutorialCompleted(ETutorialFlag.Mask_Ingame))
        {
            StartCoroutine(TutorialProc());
        }
    }

    private IEnumerator TutorialProc()
    {
        yield return Tutorial_1_Proc();
        yield return Tutorial_2_Proc();
        yield return Tutorial_3_Proc();
        yield return Tutorial_4_Proc();
        yield return Tutorial_5_Proc();
    }

    private IEnumerator Tutorial_1_Proc()
    {
        // 이미 튜토리얼은 진행한 경우
        if (UserInfo.Instance.CheckTutorialCompleted(ETutorialFlag.Ingame_1))
        {
            yield break;
        }
        // 이미 첫번째 스테이지를 클리어한 경우
        else if (UserInfo.Instance.GetStageStarCount("SN_1") > 0)
        {
            UserInfo.Instance.SetTutorialFlag(ETutorialFlag.Ingame_1);
            UserInfo.Instance.Save();
            yield break;
        }
        // 아직 첫번째 스테이지를 클리어하지 못한 경우
        else if(curStageData.stageId.Equals("SN_1"))
        {
            // 1. 그리드를 터치하도록 유도
            // 2. 스킬 맵 UI에서 꽃게 타워 스킬 터치 유도
            // 3. 스킬 구매 버튼 터치 유도
            // 4. 튜토리얼 완료 팝업

            isLockWaveStart = true;

            var msgPopUp = UIManager.GetMessageBoxUI();
            UIManager.ShowMessageBoxUI
                (UITextManager.GetText("튜토리얼")
                , UITextManager.GetText("튜토리얼_Ingame_1")
                , UITextManager.GetText("확인")
                , ()=>
                {
                    UIManager.GetMessageBoxUI().Close();
                    IngameManager.Instance.isGameStopped = false;
                });
            
            yield return null;

            isGameStopped = true;

            yield return new WaitWhile(() => { return UIManager.GetMessageBoxUI().isOpened; });

            var ingameUIManager = UIManager_Ingame.Instance;
            UIManager.Instance.handUI.Open();

            while (UserInfo.Instance.GetStageStarCount("SN_1") < 1)
            {
                if (status == EStatus.Ended) break;

                if(ingameUIManager.gridInfoUI.groundInfoUI.isOpened)
                {
                    if (ingameUIManager.gridInfoUI.groundInfoUI.towerAbilityUIRtf.gameObject.activeInHierarchy)
                    {
                        UIManager.Instance.handUI.SetPosition(ingameUIManager.gridInfoUI.groundInfoUI.buildConfirmBtn.transform.position);
                    }
                    else
                    {
                        IngameUI_GridInfo_Ground_BuildItem buildItem;
                        if (ingameUIManager.gridInfoUI.groundInfoUI.TryGetGroundBuildItem(0, out buildItem))
                        {
                            UIManager.Instance.handUI.SetPosition(buildItem.transform.position);
                        }
                        else
                        {
                            Debug.LogErrorFormat("BuildItem({0}) not found.", 0);
                            break;
                        }
                    }
                }
                else if(curCost >= 100)
                {
                    GridInfo gInfo;
                    if(AppManager.Instance.gridMap.TryGetGridInfo(new IntVector2(2, 2), out gInfo) && !gInfo.gridBlock.isBuildTower)
                    {
                        UIManager.Instance.handUI.SetPosition(gInfo.gridBlock.transform.position);
                    }
                    else if(AppManager.Instance.gridMap.TryGetGridInfo(new IntVector2(6, 2), out gInfo) && !gInfo.gridBlock.isBuildTower)
                    {
                        UIManager.Instance.handUI.SetPosition(gInfo.gridBlock.transform.position);
                    }
                    else
                    {
                        Debug.LogErrorFormat("GridInfo({0}, {1}) not found.", 2, 2);
                        break;
                    }
                }
                else
                {
                    // 타워를 배치할 자원이 없을 때
                    break;
                }

                yield return null;
            }

            isLockWaveStart = false;

            UIManager.Instance.handUI.Close();
            UserInfo.Instance.SetTutorialFlag(ETutorialFlag.Ingame_1);
            UserInfo.Instance.Save();
            yield break;
        }

        yield break;
    }

    private IEnumerator Tutorial_2_Proc()
    {
        // 이미 튜토리얼은 진행한 경우
        if (UserInfo.Instance.CheckTutorialCompleted(ETutorialFlag.Ingame_2))
        {
            yield break;
        }
        // 이미 첫번째 스테이지를 클리어한 경우
        else if (UserInfo.Instance.GetStageStarCount("SN_1") > 0)
        {
            UserInfo.Instance.SetTutorialFlag(ETutorialFlag.Ingame_2);
            UserInfo.Instance.Save();
            yield break;
        }
        else if (curStageData.stageId.Equals("SN_1") && UserInfo.Instance.CheckTutorialCompleted(ETutorialFlag.Ingame_1))
        {
            bool showMsg = false;

            // 효과가 부여된 타워가 없는지 확인
            IntVector2 towerGridPos = new IntVector2(-1, -1);
            var gridMap = AppManager.Instance.gridMap;
            List<GridInfo> buildTowerGridInfos;
            if(gridMap.gridInfoByType.TryGetValue(GridBlock.EType.Ground, out buildTowerGridInfos))
            {
                foreach(var info in buildTowerGridInfos)
                {
                    if(info.gridBlock.isBuildTower)
                    {
                        if (info.gridBlock.tower.GetAdditionalEffectCount() > 0)
                        {
                            // 추가된 효과가 하나라도 있다면 튜토리얼 생략
                            goto SKIP;
                        }
                        else
                        {
                            // 타워가 설치된 그리드를 찾아서 등록
                            towerGridPos = info.gridPos;
                        }
                    }
                }
            }

            GridInfo gInfo;
            if(towerGridPos.x != -1 && gridMap.TryGetGridInfo(towerGridPos, out gInfo) && gInfo.gridBlock.isBuildTower)
            {
                // 추가 효과의 가격만큼 돈이 모일때까지 대기
                yield return new WaitWhile(() => { return (curCost < randomTowerAdditionalEffects[0].cost); });

                while (UserInfo.Instance.GetStageStarCount("SN_1") < 1 || gInfo.gridBlock.tower.GetAdditionalEffectCount() == 0)
                {
                    if (status == EStatus.Ended) break;

                    var ingameUIManager = UIManager_Ingame.Instance;

                    if(!showMsg)
                    {
                        UIManager.ShowMessageBoxUI
                            (UITextManager.GetText("튜토리얼")
                            , UITextManager.GetText("튜토리얼_Ingame_2_1")
                            , UITextManager.GetText("확인")
                            , () =>
                            {
                                UIManager.GetMessageBoxUI().Close();
                                IngameManager.Instance.isGameStopped = false;
                                showMsg = true;
                            });

                        yield return null;

                        isGameStopped = true;

                        yield return new WaitWhile(() => { return UIManager.GetMessageBoxUI().isOpened; });
                    }

                    if (!UIManager.Instance.handUI.isOpened)
                    {
                        UIManager.Instance.handUI.Open();
                    }

                    // 배치된 타워에 추가 효과가 하나라도 있는 경우
                    if (gInfo.gridBlock.tower.GetAdditionalEffectCount() > 0)
                    {
                        // 추가된 효과가 하나라도 있다면 튜토리얼 생략
                        goto SKIP;
                    }
                    else if (ingameUIManager.gridInfoUI.towerInfoUI.isOpened)
                    {
                        if (ingameUIManager.gridInfoUI.towerInfoUI.selectTAEUI.isOpened)
                        {
                            if (ingameUIManager.gridInfoUI.towerInfoUI.selectTAEUI.selectedFrameBtn.isOpened)
                            {
                                UIManager.Instance.handUI.SetPosition(ingameUIManager.gridInfoUI.towerInfoUI.selectTAEUI.selectedFrameBtn.transform.position);
                            }
                            else
                            {
                                UIManager.Instance.handUI.SetPosition(ingameUIManager.gridInfoUI.towerInfoUI.selectTAEUI.taeInfos[0].btn.transform.position);
                            }
                        }
                        else
                        {
                            UIManager.Instance.handUI.SetPosition(ingameUIManager.gridInfoUI.towerInfoUI.openSelectTAEBtn.transform.position);
                        }
                    }
                    else
                    {
                        UIManager.Instance.handUI.SetPosition(gInfo.gridBlock.transform.position);
                    }

                    yield return null;
                }
            }

            SKIP:;

            UIManager.Instance.handUI.Close();
            UserInfo.Instance.SetTutorialFlag(ETutorialFlag.Ingame_2);
            UserInfo.Instance.Save();
            
            UIManager.ShowMessageBoxUI
                (UITextManager.GetText("튜토리얼")
                , UITextManager.GetText("튜토리얼_Ingame_2_2")
                , UITextManager.GetText("확인")
                , () =>
                {
                    UIManager.GetMessageBoxUI().Close();
                    IngameManager.Instance.isGameStopped = false;
                    showMsg = true;
                });

            yield return null;

            isGameStopped = true;

            yield return new WaitWhile(() => { return UIManager.GetMessageBoxUI().isOpened; });
        }

        yield break;
    }

    private IEnumerator Tutorial_3_Proc()
    {        
        // 이미 튜토리얼은 진행한 경우
        if (UserInfo.Instance.CheckTutorialCompleted(ETutorialFlag.Ingame_3))
        {
            yield break;
        }
        // 이미 네번째 스테이지를 클리어한 경우
        else if (UserInfo.Instance.GetStageStarCount("SN_4") > 0)
        {
            UserInfo.Instance.SetTutorialFlag(ETutorialFlag.Ingame_3);
            UserInfo.Instance.Save();
            yield break;
        }
        // 네번째 스테이지를 진행하고 있는 경우
        else if(curStageData.stageId.Equals("SN_4"))
        {
            isLockWaveStart = true;

            GridInfo gInfo;
            if(AppManager.Instance.gridMap.TryGetGridInfo(new IntVector2(5, 2), out gInfo) 
                && gInfo.gridBlock.gridBlockType == GridBlock.EType.Wall)
            {
                GridBlock_Wall gridBlockWall = gInfo.gridBlock as GridBlock_Wall;
                
                // 1. 첫번째 멘트 출력
                // 2. 벽 블럭을 터치하도록 유도 GridPos(5, 2)
                // 3. 벽 블럭에 대한 설명 멘트 출력
                // 4. 벽 블럭에 대한 설명 멘트 2 출력

                UIManager.ShowMessageBoxUI
                    (UITextManager.GetText("튜토리얼")
                    , UITextManager.GetText("튜토리얼_Ingame_3_1")
                    , UITextManager.GetText("확인")
                    , () =>
                    {
                        UIManager.GetMessageBoxUI().Close();
                        IngameManager.Instance.isGameStopped = false;
                    });

                yield return null;

                isGameStopped = true;

                yield return new WaitWhile(() => { return UIManager.GetMessageBoxUI().isOpened; });

                bool showMsg1 = false;
                bool showMsg2 = false;
                var ingameUIManager = UIManager_Ingame.Instance;
                UIManager.Instance.handUI.Open();

                while (!gridBlockWall.isWallOpened && (!showMsg1 || !showMsg2))
                {
                    if(ingameUIManager.gridInfoUI.messageUIRtf.gameObject.activeInHierarchy)
                    {
                        if(!showMsg1)
                        {
                            UIManager.ShowMessageBoxUI
                                (UITextManager.GetText("튜토리얼")
                                , UITextManager.GetText("튜토리얼_Ingame_3_2")
                                , UITextManager.GetText("확인")
                                , () =>
                                {
                                    UIManager.GetMessageBoxUI().Close();
                                    IngameManager.Instance.isGameStopped = false;
                                    showMsg1 = true;
                                });

                            yield return null;

                            isGameStopped = true;

                            yield return new WaitWhile(() => { return UIManager.GetMessageBoxUI().isOpened; });
                        }

                        if (!showMsg2)
                        {
                            UIManager.ShowMessageBoxUI
                                (UITextManager.GetText("튜토리얼")
                                , UITextManager.GetText("튜토리얼_Ingame_3_3")
                                , UITextManager.GetText("확인")
                                , () =>
                                {
                                    UIManager.GetMessageBoxUI().Close();
                                    IngameManager.Instance.isGameStopped = false;
                                    showMsg2 = true;
                                });

                            yield return null;

                            isGameStopped = true;

                            yield return new WaitWhile(() => { return UIManager.GetMessageBoxUI().isOpened; });

                            break;
                        }
                    }
                    else
                    {
                        UIManager.Instance.handUI.SetPosition(gInfo.gridBlock.transform.position);
                    }

                    yield return null;
                }
            }
            else
            {
                Debug.LogErrorFormat("GridInfo({0}, {1}) not found or GridBlockType is not Wall.", 5, 2);
            }

            isLockWaveStart = false;

            UIManager.Instance.handUI.Close();
            UserInfo.Instance.SetTutorialFlag(ETutorialFlag.Ingame_3);
            UserInfo.Instance.Save();

        }
        yield break;
    }

    private IEnumerator Tutorial_4_Proc()
    {        
        // 이미 튜토리얼은 진행한 경우
        if (UserInfo.Instance.CheckTutorialCompleted(ETutorialFlag.Ingame_4))
        {
            yield break;
        }
        // 이미 일곱번째 스테이지를 클리어한 경우
        else if (UserInfo.Instance.GetStageStarCount("SN_7") > 0)
        {
            UserInfo.Instance.SetTutorialFlag(ETutorialFlag.Ingame_4);
            UserInfo.Instance.Save();
            yield break;
        }
        // 일곱번째 스테이지를 진행하고 있는 경우
        else if (curStageData.stageId.Equals("SN_7"))
        {
            isLockWaveStart = true;

            UIManager.ShowMessageBoxUI
                (UITextManager.GetText("튜토리얼")
                , UITextManager.GetText("튜토리얼_Ingame_4_1")
                , UITextManager.GetText("확인")
                , () =>
                {
                    UIManager.GetMessageBoxUI().Close();
                    IngameManager.Instance.isGameStopped = false;
                });

            yield return null;

            isGameStopped = true;

            yield return new WaitWhile(() => { return UIManager.GetMessageBoxUI().isOpened; });

            isLockWaveStart = false;

            UIManager.Instance.handUI.Close();
            UserInfo.Instance.SetTutorialFlag(ETutorialFlag.Ingame_4);
            UserInfo.Instance.Save();
        }
        yield break;
    }

    private IEnumerator Tutorial_5_Proc()
    {
        // 이미 튜토리얼은 진행한 경우
        if (UserInfo.Instance.CheckTutorialCompleted(ETutorialFlag.Ingame_5))
        {
            yield break;
        }
        // 이미 40번째 스테이지를 클리어한 경우
        else if (UserInfo.Instance.GetStageStarCount("SN_40") > 0)
        {
            UserInfo.Instance.SetTutorialFlag(ETutorialFlag.Ingame_5);
            UserInfo.Instance.Save();
            yield break;
        }
        // 40번째 스테이지를 진행하고 있는 경우
        else if (curStageData.stageId.Equals("SN_40"))
        {
            isLockWaveStart = true;

            UIManager.ShowMessageBoxUI
                (UITextManager.GetText("튜토리얼")
                , UITextManager.GetText("튜토리얼_Ingame_5_1")
                , UITextManager.GetText("확인")
                , () =>
                {
                    UIManager.GetMessageBoxUI().Close();
                    IngameManager.Instance.isGameStopped = false;
                });

            yield return null;

            isGameStopped = true;

            yield return new WaitWhile(() => { return UIManager.GetMessageBoxUI().isOpened; });

            isLockWaveStart = false;

            UIManager.Instance.handUI.Close();
            UserInfo.Instance.SetTutorialFlag(ETutorialFlag.Ingame_5);
            UserInfo.Instance.Save();
        }
        yield break;
    }

    public void StopTutorial()
    {
        isLockWaveStart = false;
        StopAllCoroutines();
        UIManager.Instance.handUI.Close();
    }

#endregion
}
