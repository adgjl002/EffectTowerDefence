using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Ingame : UIBase {

    public static UIManager_Ingame Instance { get { return AppManager.Instance.ingameUIManager; } }

    [SerializeField]
    private IngameUI_Top m_TopUI;
    public IngameUI_Top topUI { get { return m_TopUI; } }

    [SerializeField]
    private IngameUI_Middle m_MiddleUI;
    public IngameUI_Middle middleUI { get { return m_MiddleUI; } }

    [SerializeField]
    private IngameUI_Bottom m_BottomUI;
    public IngameUI_Bottom bottomUI { get { return m_BottomUI; } }
    
    [SerializeField]
    private IngameUI_GridInfo m_GridInfoUI;
    public IngameUI_GridInfo gridInfoUI { get { return m_GridInfoUI; } }
    
    [SerializeField]
    private IngameUI_GameResult m_GameResultUI;
    public IngameUI_GameResult gameResultUI { get { return m_GameResultUI; } }

    [SerializeField]
    private IngameUI_StageStart m_StageStartUI;
    public IngameUI_StageStart stageStartUI { get { return m_StageStartUI; } }
    
    public void Initialize()
    {

    }

    public void Release()
    {

    }

    private void Update()
    {
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Alpha0))
        {
            UserInfo.Instance.AddStarCount(100, true, UIManager.Instance.foreCanvas.transform.position);
        }
#endif
    }

    public override void Open()
    {
        base.Open();

        topUI.Open();
        middleUI.Open();
        bottomUI.Open();

        IngameManager.Instance.OnChangeLife += OnChangeLife;
    }

    public override void Close()
    {
        IngameManager.Instance.OnChangeLife -= OnChangeLife;
        
        gridInfoUI.Close();

        bottomUI.Close();
        middleUI.Close();
        topUI.Close();

        base.Close();
    }

    public void ShowDamageUI(Vector3 worldPos, int damage, EDamageType damageType, bool isCritical = false)
    {
        if (!IngameManager.Instance.isOnIngameInfo) return;

        IngameUI_Damage ui;
        if(SpawnMaster.TrySpawnUI("IngameUI_Damage", middleUI.transform, out ui))
        {
            ui.transform.position = worldPos + new Vector3(0, 0, -1.5f);
            ui.SetData(damage, damageType, isCritical);
            ui.Open();
        }
        else
        {
            Debug.LogError("UIManager_Ingame :: Can't spawn DamageUI");
        }
    }

    private void OnChangeLife(int life, int changedAmount)
    {
        topUI.lifeUI.SetData(life);
        topUI.lifeUI.UpdateUI();
    }
    
    /// <summary>
    /// Cost는 무조건 5의 배수 값이 입력되야함.
    /// </summary>
    /// <param name="worldPos"></param>
    /// <param name="cost"></param>
    public void ShowCostParticle(Vector3 worldPos, int cost)
    {
        if(cost <= 0) return;

        int bigParticleCnt = cost / 50;
        int smallParticleCnt = (cost % 50) / 5;
        int verySmallParticleCnt = (cost % 5);

        Fx_ParticleTarget fx;
        if (verySmallParticleCnt > 0 && SpawnMaster.TrySpawnFx("Fx_TakeCostParticle", worldPos, Quaternion.identity, out fx))
        {
            fx.mainPs.emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst() { count = 1, maxCount = 1, minCount = 1, cycleCount = verySmallParticleCnt, probability = 1f, time = 0f, repeatInterval = 0.05f }
            });

            var psMain = fx.mainPs.main;
            psMain.maxParticles = verySmallParticleCnt;
            psMain.startSize = 0.075f;

            fx.SetFx
                (topUI.costUI.costIcon.gameObject
                , true
                , () => { return !IngameManager.Instance.gameObject.activeInHierarchy; }
                , (des) => {
                    IngameManager.Instance.AddCost(1);
                });
            fx.On();
        }

        if (smallParticleCnt > 0 && SpawnMaster.TrySpawnFx("Fx_TakeCostParticle", worldPos, Quaternion.identity, out fx))
        {
            fx.mainPs.emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst() { count = 1, maxCount = 1, minCount = 1, cycleCount = smallParticleCnt, probability = 1f, time = 0f, repeatInterval = 0.05f }
            });
            
            var psMain = fx.mainPs.main;
            psMain.maxParticles = smallParticleCnt;
            psMain.startSize = 0.15f;

            fx.SetFx
                ( topUI.costUI.costIcon.gameObject
                , true
                , () => { return !IngameManager.Instance.gameObject.activeInHierarchy; }
                , (des) => {
                    IngameManager.Instance.AddCost(5);
                });
            fx.On();
        }

        if (bigParticleCnt > 0 && SpawnMaster.TrySpawnFx("Fx_TakeCostParticle", worldPos, Quaternion.identity, out fx))
        {
            fx.mainPs.emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst() { count = 1, maxCount = 1, minCount = 1, cycleCount = bigParticleCnt, probability = 1f, time = 0f, repeatInterval = 0.05f }
            });

            var psMain = fx.mainPs.main;
            psMain.maxParticles = bigParticleCnt;
            psMain.startSize = 0.25f;

            fx.SetFx
                (topUI.costUI.costIcon.gameObject
                , true
                , () => { return !IngameManager.Instance.gameObject.activeInHierarchy; }
                , (des) => {
                    IngameManager.Instance.AddCost(50);
                });
            fx.On();
        }
    }

    public void ShowAttackLifeParticle(Vector3 worldPos, int attackedLife)
    {
        Fx_NonTarget fx;
        for (int i = 0;i< attackedLife; ++i)
        {
            if (SpawnMaster.TrySpawnFx("Fx_AttackLifeParticle", worldPos, Quaternion.identity, out fx))
            {
                fx.SetFx
                    ( topUI.lifeUI.transform.position
                    , (des) => {
                        IngameManager.Instance.AddLife(-1);
                        Fx_OneTime hitFx;
                        if(SpawnMaster.TrySpawnFx("Fx_AttackLifeParticle_Hit", des, Quaternion.identity, out hitFx))
                        {
                            hitFx.On();
                        }
                    });
                fx.On();
            }
        }
    }

}
