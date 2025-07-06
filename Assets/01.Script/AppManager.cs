 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class AppManager : BaseMonoSingleton<AppManager> {

    public enum EStatus
    {
        None,
        Initializing,
        Initialized,
        Releasing,
        Released
    }

    public EStatus appStatus;
    public bool isPlayingIngame { get; private set; }
    public bool isInitialized { get { return appStatus == EStatus.Initialized; } }

    #region < Common >
    
    public DataManager dataManager { get; private set; }
    public ResourceManager resourceManager { get; private set; }
    public SpawnMaster spawnMaster { get; private set; }
    public InputManager inputManager { get; private set; }

    public UserInfo userInfo { get; private set; }

    [SerializeField]
    private CameraController m_CameraController;
    public CameraController cameraController { get { return m_CameraController; } }

    [SerializeField]
    private AudioManager m_AudioManager;
    public AudioManager audioManager { get { return m_AudioManager; } }

    public int preStageIdx { get; private set; }
    public int curStageIdx { get; private set; }

    public string curStageId
    {
        get
        {
            StageData sData;
            if (DataManager.Instance.TryStageData(curStageIdx, out sData))
            {
                return sData.stageId;
            }
            return string.Empty;
        }
    }

    public GridMap gridMap { get; private set; }

    [SerializeField]
    private Transform m_GridMapCenterTf;
    public Transform gridMapCenterTf => m_GridMapCenterTf;

    #endregion

    #region < Lobby >

    [SerializeField]
    private LobbyManager m_LobbyManager;
    public LobbyManager lobbyManager { get { return m_LobbyManager; } }

    [SerializeField]
    private UIManager_Lobby m_LobbyUIManager;
    public UIManager_Lobby lobbyUIManager { get { return m_LobbyUIManager; } }

    #endregion

    #region < Ingame >

    [SerializeField]
    private UIManager m_UIManager;
    public UIManager uiManager { get { return m_UIManager; } }
    
    [SerializeField]
    private IngameManager m_IngameManager;
    public IngameManager ingameManager { get { return m_IngameManager; } }

    [SerializeField]
    private UIManager_Ingame m_IngameUIManager;
    public UIManager_Ingame ingameUIManager { get { return m_IngameUIManager; } }

    #endregion

    #region < Event >

    public UnityAction<float> OnUpdate;

    #endregion  

    protected override void Awake()
    {
#if UNITY_STANDALONE
        Screen.SetResolution(540, 960, false);
#endif

#if REAL
        Debug.unityLogger.logEnabled = false;
#endif

        base.Awake();

        isPlayingIngame = false;

        StartCoroutine(Initialize());
    }

    public IEnumerator Initialize()
    {
        if(appStatus == EStatus.Initializing || appStatus == EStatus.Initialized)
        {
            yield break;
        }
        appStatus = EStatus.Initializing;

#if UNITY_ANDROID
        GPGSManager.CreateInstance();
        GPGSManager.Instance.GPGSActivate();
        GPGSManager.Instance.GPGSLogin();
#endif

        GameSettingsManager.CreateInstance();
        GameSettingsManager.Load();

        if (dataManager == null) dataManager = new DataManager();
        dataManager.Initialize();

#if UNITY_EDITOR
        Debug.Log(dataManager.GetLog());
#endif

        UserInfo tUserInfo;
#if UNITY_EDITOR
        if (!GameSettingsManager.CreateNewUser && UserInfo.TryLoad(out tUserInfo))
        {
            userInfo = tUserInfo;
        }
        else
        {
            userInfo = new UserInfo();
            userInfo.starCount = GameSettingsManager.StartStarCount;
        }
        userInfo.InitializeSkillInventory();
        userInfo.lastAdsTime = System.DateTime.Now;
        userInfo.adsPlayCount = 0;
        userInfo.normalModePlayCount = 0;
        userInfo.infinityModePlayCount = 0;
        userInfo.Save();
#else
        if (UserInfo.TryLoad(out tUserInfo))
        {
            userInfo = tUserInfo;
        }
        else
        {
            userInfo = new UserInfo();
            userInfo.starCount = GameSettingsManager.StartStarCount;
        }
        userInfo.InitializeSkillInventory();
        userInfo.lastAdsTime = System.DateTime.Now;
        userInfo.adsPlayCount = 0;
#endif


        //userInfo.skillInv.Initialize();

        //// 공짜 스킬 습득
        //var etor = dataManager.GetSkillDatasEtor();
        //while (etor.MoveNext())
        //{
        //    if (GameSettingsManager.OpenAllSkill || etor.Current.Value.starCost == 0)
        //    {
        //        userInfo.skillInv.SetActiveSkill(etor.Current.Value.skillID, true);
        //    }
        //}

        if (resourceManager == null) resourceManager = new ResourceManager();
        resourceManager.Initialize();

        if (spawnMaster == null) spawnMaster = new SpawnMaster();
        spawnMaster.Initialize();

        if (inputManager == null) inputManager = new InputManager();
        inputManager.Initialize();

        audioManager.Initialize();

        cameraController.Initialize();

        uiManager.Initialize();
        uiManager.Open();

        lobbyManager.Initialize();

        lobbyUIManager.Initialize();
        lobbyUIManager.Close();

        ingameManager.Initialize();

        ingameUIManager.Initialize();
        ingameUIManager.Close();

        gridMap = new GridMap();

        // 클리어한 스테이지 중 가장 늦은 스테이지를 찾아서 등록
        curStageIdx = -1;
        foreach(var info in UserInfo.Instance.stageClearInfo)
        {
            int stageIdx;
            if(info.Value > 0 && DataManager.Instance.stageIdxById.TryGetValue(info.Key, out stageIdx))
            {
                if(stageIdx > curStageIdx)
                {
                    preStageIdx = curStageIdx = stageIdx;
                }
            }
        }
        // 마지막에 클리어한 스테이지의 다음 스테이지를 선택
        //SetStageIdx(curStageIdx + 1);

        yield return null;

        OpenLobby();

        appStatus = EStatus.Initialized;
    }

    public void OpenLobby(bool openNextStage = true)
    {
        UIManager.Instance.userResourcesUI.Open();

        lobbyManager.gameObject.SetActive(true);
        lobbyUIManager.gameObject.SetActive(true);

        isPlayingIngame = false;

        cameraController.SetDes(new Vector3(0, 0.35f, 0), false);

        StageData sData;
        //// 다음 스테이지가 없거나 잠겨 있는 경우
        //if (UserInfo.Instance.CheckLockStage(curStageIdx + 1))
        //{
        //    ChangeStage(curStageIdx);
        //}
        //// 스테이지 데이터가 있으며, 잠겨있지 않은 경우
        //else
        //{
        //    ChangeStage((openNextStage) ? curStageIdx + 1 : curStageIdx);
        //}
        ChangeStage(curStageIdx);

        lobbyUIManager.Open();
    }

    public void CloseLobby()
    {
        UIManager.Instance.userResourcesUI.Close();

        lobbyUIManager.Close();

        lobbyManager.gameObject.SetActive(false);
        lobbyUIManager.gameObject.SetActive(false);
    }

    public void OpenIngame()
    {
        StageData stageData;
        if (dataManager.TryStageData(curStageIdx, out stageData))
        {
            gridMap.HideWayFinderFx();

            cameraController.SetDes(Vector3.zero);

            UIManager.Instance.userResourcesUI.Close();

            ingameManager.gameObject.SetActive(true);
            ingameUIManager.gameObject.SetActive(true);

            ingameManager.StartStage(stageData);
            isPlayingIngame = true;
        }
    }

    public void CloseIngame()
    {
        IngameManager.Instance.EndStage();

        if(curStageIdx == 7 && GameSettingsManager.IsWrotedReview == 0)
        {
            UIManager.ShowMessageBoxUI
                (UITextManager.GetText("알림")
                , UITextManager.GetText("평가/메시지/1")
                , UITextManager.GetText("평가/버튼/네")
                , () =>
                {
                    GameSettingsManager.IsWrotedReview = 9;
                    Application.OpenURL("https://play.google.com/store/apps/details?id=com.YUOffical.EffectTD&hl=ko&ah=wAkNvaLkXsmXxikjLlnvo-kaKDI");
                    UIManager.GetMessageBoxUI().Close();
                    GameSettingsManager.Save();
                }
                , UITextManager.GetText("평가/버튼/아니오")
                , () =>
                {
                    GameSettingsManager.IsWrotedReview = 1;
                    UIManager.GetMessageBoxUI().Close();
                });
        }
        else if (curStageIdx == 15 && GameSettingsManager.IsWrotedReview == 1)
        {
            UIManager.ShowMessageBoxUI
                (UITextManager.GetText("알림")
                , UITextManager.GetText("평가/메시지/2")
                , UITextManager.GetText("평가/버튼/네")
                , () =>
                {
                    GameSettingsManager.IsWrotedReview = 9;
                    Application.OpenURL("https://play.google.com/store/apps/details?id=com.YUOffical.EffectTD&hl=ko&ah=wAkNvaLkXsmXxikjLlnvo-kaKDI");
                    UIManager.GetMessageBoxUI().Close();
                    GameSettingsManager.Save();
                }
                , UITextManager.GetText("평가/버튼/아니오")
                , () =>
                {
                    GameSettingsManager.IsWrotedReview = 2;
                    UIManager.GetMessageBoxUI().Close();
                });
        }
        else if (curStageIdx == 23 && GameSettingsManager.IsWrotedReview == 2)
        {
            UIManager.ShowMessageBoxUI
                (UITextManager.GetText("알림")
                , UITextManager.GetText("평가/메시지/3")
                , UITextManager.GetText("평가/버튼/네")
                , () =>
                {
                    GameSettingsManager.IsWrotedReview = 9;
                    Application.OpenURL("https://play.google.com/store/apps/details?id=com.YUOffical.EffectTD&hl=ko&ah=wAkNvaLkXsmXxikjLlnvo-kaKDI");
                    UIManager.GetMessageBoxUI().Close();
                    GameSettingsManager.Save();
                }
                , UITextManager.GetText("평가/버튼/아니오")
                , () =>
                {
                    GameSettingsManager.IsWrotedReview = 3;
                    UIManager.GetMessageBoxUI().Close();
                });
        }
    }
      
    public bool ChangeStage(int stageIdx)
    {
        preStageIdx = Mathf.Clamp(curStageIdx, 0, DataManager.Instance.stageIdxById.Count - 1);
        curStageIdx = Mathf.Clamp(stageIdx, 0, DataManager.Instance.stageIdxById.Count - 1);

        StageData stageData;
        if(dataManager.TryStageData(curStageIdx, out stageData))
        {
            gridMap.Claer();

            if(stageData.mapData[stageData.mapData.Length - 1].Equals('/'))
            {
                stageData.mapData = stageData.mapData.Remove(stageData.mapData.Length - 1, 1);
            }

            gridMap.HideWayFinderFx();
            gridMap.Create(stageData.mapData
                , stageData.mapSkyData
                , gridMapCenterTf.position
                , stageData.tileSize
                , stageData.tileSize
                , stageData.wallData);
            gridMap.ShowWayFinderFx();

            var ySizeZoom = (isPlayingIngame) ? gridMap.yLength * 0.4f : gridMap.yLength * 0.5f;
            var xSizeZoom = (isPlayingIngame) ? gridMap.xLength * 0.3f : gridMap.xLength * 0.4f;
            cameraController.SetZoomSize((Mathf.Max(5, ySizeZoom, xSizeZoom)));
            //cameraController.SetZoomSize((isPlayingIngame) ? stageData.ingameZoomSize : stageData.lobbyZoomSize);

            return true;
        }

        return false;
    }

    private void Update()
    {
        if(isInitialized)
        {
            inputManager.Update(Time.deltaTime);
            gridMap.Update(Time.deltaTime);

            if (OnUpdate != null) OnUpdate(Time.deltaTime);
        }

#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.G))
        {
            UserInfo.Save(userInfo);
        }
#endif
    }

    public void Release()
    {
        if (appStatus == EStatus.Releasing || appStatus == EStatus.None)
        {
            return;
        }
        appStatus = EStatus.Releasing;

        ingameManager.Release();
        lobbyUIManager.Release();

        lobbyManager.Release();
        ingameUIManager.Release();

        uiManager.Release();

        cameraController.Release();

        audioManager.Release();

        inputManager.Release();
        spawnMaster.Release();
        resourceManager.Release();

        UserInfo.Save(userInfo);
        
        dataManager.Release();

        GameSettingsManager.Save();
        GameSettingsManager.DestroyInstance();

        gridMap.Claer();

        appStatus = EStatus.None;
    }
}
