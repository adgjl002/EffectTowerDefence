using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IngameGrid;

public class GridBlock_Tower : MonoBehaviour {

    [SerializeField]
    private string m_PrefabKey;
    public string prefabKey { get { return m_PrefabKey; } }

    public TowerData data;
    public int maxAdditionalEffectCount { get { return 4; } }

    #region < Ability >
    public int damage { get { return Mathf.RoundToInt(((data.damage + statusController.GetFixedValue(EAbilityType.Damage)) * statusController.GetPercentValue(EAbilityType.Damage)) + (data.damage * AdditionalStatusPerLevel));  } }
    public float attackRange { get { return ((data.range + statusController.GetFixedValue(EAbilityType.AttackRange)) * statusController.GetPercentValue(EAbilityType.AttackRange)); } }
    public float attackSpeed { get { return ((data.attackSpeed + statusController.GetFixedValue(EAbilityType.AttackSpeed)) * statusController.GetPercentValue(EAbilityType.AttackSpeed)); } }
    public float criticalPerc { get { return ((data.criticalPerc + statusController.GetFixedValue(EAbilityType.CriticalPerc)) * statusController.GetPercentValue(EAbilityType.CriticalPerc)); } }
    public float criticalDamageRatio { get { return ((data.criticalDamageRatio + statusController.GetFixedValue(EAbilityType.CriticalDamage)) * statusController.GetPercentValue(EAbilityType.CriticalDamage)); } }
    public int projectileCount { get { return Mathf.RoundToInt((data.projectileCount + statusController.GetFixedValue(EAbilityType.ProjectileCount))); } }
    public int attackCount { get { return Mathf.RoundToInt(1 + statusController.GetFixedValue(EAbilityType.AttackCount)); } }
    #endregion

    #region < Level >
    public float AdditionalStatusPerLevel { get; private set; }
    public int level { get; private set; }
    public bool isMaxLevel {
        get
        {
            var maxLevel = GameSettingsManager.TowerMaxLevel;
            if (OnAddTowerMaxLevel != null)
            {
                maxLevel = OnAddTowerMaxLevel(GameSettingsManager.TowerMaxLevel);
            }
            return maxLevel == level;
        }
    }
    public void AddLevel(int addLevel)
    {
        var maxLevel = GameSettingsManager.TowerMaxLevel;
        if(OnAddTowerMaxLevel != null)
        {
            maxLevel = OnAddTowerMaxLevel(GameSettingsManager.TowerMaxLevel);
        }
        
        if (level >= maxLevel)
        {
            return;
        }

        level += addLevel;

        Fx_OneTime_Message fxMsg;
        if (SpawnMaster.TrySpawnFx("Fx_Upgrade", transform.position, Quaternion.identity, out fxMsg))
        {
            fxMsg.fxMsgType = EFxMessageType.LevelUp;
            fxMsg.Message.text = "LevelUp!!";
            fxMsg.On();
        }

        if (OnAddStatusRatioPerLevel != null)
        {
            AdditionalStatusPerLevel = OnAddStatusRatioPerLevel(GameSettingsManager.TowerIncreaseRatioPerLevel) * (level - 1);
        }
        else
        {
            AdditionalStatusPerLevel = GameSettingsManager.TowerIncreaseRatioPerLevel * (level - 1);
        }
        // 이펙트!!
    }
    public void InitLevel()
    {
        AdditionalStatusPerLevel = 0f;
        level = 1;
    }
    #endregion

    #region < SkillCount >

    public int killCount { get; private set; }
    public void AddKillCount(int addCount)
    {
        killCount += addCount;
    }
    public void InitKillCount()
    {
        killCount = 0;
    }

    #endregion

    #region < Experience >

    /// <summary>
    /// 경험치
    /// </summary>
    public int expPoint;
    public int maxExpPoint => level * GameSettingsManager.NeededLevelUpExpPoint;
    public void AddExperiencePoint(int expPoint)
    {
        if(OnAddExpPoint != null)
        {
            this.expPoint += OnAddExpPoint(expPoint);
        }
        else
        {
            this.expPoint += expPoint;
        }
        
        if(this.expPoint >= GameSettingsManager.GetLevelUpExpPoint(level))
        {
            AddLevel(1);
        }
    }
    public void InitExperiencePoint()
    {
        expPoint = 0;
    }

    #endregion

    public bool isAttackable { get { return (OnCheckAttackable != null) ? OnCheckAttackable() : true; } }

    private StatusController m_StatusController = new StatusController();
    public StatusController statusController { get { return m_StatusController; } }

    /// <summary>
    /// 타워의 가격 + 추가 효과의 가격의 50%의 값
    /// </summary>
    /// <returns></returns>
    public int GetSellCost()
    {
        int totalCost = data.buildCost;
        for(int i = 0; i< additionalEffects.Count; ++i)
        {
            var tae = additionalEffects[i];
            if(tae != null)
            {
                totalCost += tae.cost;
            }
        }
        return Mathf.RoundToInt(totalCost * 0.5f);
    }

    #region < Tower Additional Effect >

    private List<TowerAdditionalEffect> additionalEffects = new List<TowerAdditionalEffect>();
    public int GetAppliedAdditionalEffectCount(TowerAdditionalEffect.EType appliedType)
    {
        int cnt = 0;
        for(int i = 0; i< additionalEffects.Count; ++i)
        {
            if(additionalEffects[i].effectType == appliedType)
            {
                ++cnt;
            }
        }
        return cnt;
    }
    public int GetAdditionalEffectCount() { return additionalEffects.Count; }
    public bool TryGetAdditionalEffect(int idx, out TowerAdditionalEffect towerAdditionalEffect)
    {
        if(idx >= 0 && idx < additionalEffects.Count)
        {
            towerAdditionalEffect = additionalEffects[idx];
            return true;
        }
        else
        {
            towerAdditionalEffect = null;
            return false;
        }

    }
    public bool RegistAdditionalEffect(TowerAdditionalEffect effect)
    {
        if(additionalEffects.Count < maxAdditionalEffectCount)
        {
            int appliedCount = 0;
            additionalEffects.Add(effect);

            foreach (var e in additionalEffects)
            {
                if (e.effectType == effect.effectType)
                {
                    ++appliedCount;
                }
            }

            effect.Regist(this);

            SkillData skillData;
            if(DataManager.Instance.TryGetSkillData(UserInfo.Instance.skillInv.GetHighestLevelSkillID((int)effect.effectType), out skillData))
            {
                Fx_OneTime_Message fxMsg;
                switch (skillData.subData)
                {
                    case "isDouble":
                        if (appliedCount == 2 && SpawnMaster.TrySpawnFx("Fx_Upgrade", transform.position, Quaternion.identity, out fxMsg))
                        {
                            fxMsg.fxMsgType = EFxMessageType.Double;
                            fxMsg.Message.text = "Double";
                            fxMsg.On();
                        }
                        break;
                    case "isTriple":
                        if (appliedCount == 3 && SpawnMaster.TrySpawnFx("Fx_Upgrade", transform.position, Quaternion.identity, out fxMsg))
                        {
                            fxMsg.fxMsgType = EFxMessageType.Triple;
                            fxMsg.Message.text = "Triple";
                            fxMsg.On();
                        }
                        break;
                    case "isQuadruple":
                        if (appliedCount == 4 && SpawnMaster.TrySpawnFx("Fx_Upgrade", transform.position, Quaternion.identity, out fxMsg))
                        {
                            fxMsg.fxMsgType = EFxMessageType.Quadruple;
                            fxMsg.Message.text = "Quadruple";
                            fxMsg.On();
                        }
                        break;
                }
            }
            
            for (int i = 0; i<additionalEffects.Count; ++i)
            {
                Sprite sprite = null;
                if(ResourceManager.Instance.TryGetSprite(additionalEffects[i].spriteKey, out sprite))
                {
                    // 이미지를 찾지 못함
                }
                additionalEffectRender[i].sprite = sprite;
            }

            return true;
        }
        return false;
    }
    public void ClearAdditionalEffects()
    {
        for (int i = 0; i < additionalEffects.Count; ++i)
        {
            additionalEffectRender[i].sprite = null;
        }

        while (additionalEffects.Count > 0)
        {
            additionalEffects[additionalEffects.Count - 1].Unregist(this);
            additionalEffects.RemoveAt(additionalEffects.Count - 1);
        }
    }

    #endregion

    public IngameObject curTarget => attackComponent.curTarget;

    [SerializeField]
    private SpriteRenderer m_HeadRender;
    public SpriteRenderer headRender { get { return m_HeadRender; } }

    [SerializeField]
    private GameObject m_HeadRoot;
    public GameObject headRoot { get { return m_HeadRoot; } }

    [SerializeField]
    private SpriteRenderer m_BodyRender;
    public SpriteRenderer bodyRender { get { return m_BodyRender; } }

    [SerializeField]
    private GameObject m_BodyRoot;
    public GameObject bodyRoot { get { return m_BodyRoot; } }

    [SerializeField]
    private Transform m_ProjectileSpawnPointTf;
    public Transform projectileSpawnPointTf { get { return m_ProjectileSpawnPointTf; } }

    [SerializeField]
    private GameObject m_AdditionalEffectRoot;
    public GameObject additionalEffectRoot { get { return m_AdditionalEffectRoot; } }

    [SerializeField]
    private SpriteRenderer[] m_AdditionalEffectRender;
    public SpriteRenderer[] additionalEffectRender { get { return m_AdditionalEffectRender; } }

    public GridBlock_Ground ownerGrid { get; private set; }

    [SerializeField]
    private TowerAttackComponent m_AttackComponent;
    public TowerAttackComponent attackComponent => m_AttackComponent;

    #region < Event >
    
    public System.Func<bool> OnCheckAttackable;
    public Func<float, float> OnAddStatusRatioPerLevel;
    public Func<int, int> OnAddExpPoint;
    public Func<int, int> OnAddTowerMaxLevel;

    public Func<IngameAttackInfo, IngameAttackInfo> OnAttack;
    public UnityEngine.Events.UnityAction<float> OnUpdate;

    #endregion

    public void SetData(GridBlock_Ground ownerGrid, TowerData data)
    {
        this.ownerGrid = ownerGrid;
        this.data = data;
    }

    public void Initialize()
    {
        InitLevel();
        InitKillCount(); 
        InitExperiencePoint();
        
        statusController.Initialize(this);
        attackComponent.Initialize(this);

        //int skillNo = 0;
        //switch (data.towerID)
        //{
        //    case "TD-1": skillNo = 101; break;
        //    case "TD-2": skillNo = 102; break;
        //    case "TD-3": skillNo = 103; break;
        //    case "TD-4": skillNo = 104; break;
        //    case "TD-5": skillNo = 105; break;
        //}

        //if (skillNo != 0)
        //{
        //    var skillIDInfo = UserInfo.Instance.skillInv.GetHighestLevelSkillIDInfo(skillNo);
        //    if (skillIDInfo.level > 0)
        //    {
        //        statusController.AddFixedValue(EAbilityType.Damage, 0.05f * skillIDInfo.level * data.damage);
        //    }
        //}

        SkillData skillData;
        if(!string.IsNullOrEmpty(data.skill1Id))
        {
            if (DataManager.Instance.TryGetSkillData(data.skill1Id, out skillData))
            {
                RegistAdditionalEffect(TowerAdditionalEffect.Create(skillData));
            }
            else
            {
                Debug.LogErrorFormat("Skill({0}) not found.", data.skill1Id);
            }
        }
    }

    public void Release()
    {
        ClearAdditionalEffects();

        attackComponent.Release();
        statusController.Clear();

        SpawnMaster.Destroy(gameObject, prefabKey);
    }
    
    public void LookTarget()
    {
        var angle = Vector3.Angle(new Vector3(Vector3.up.x, Vector3.up.y, 0), new Vector3(curTarget.transform.position.x, curTarget.transform.position.y, 0) - new Vector3(transform.position.x, transform.position.y, 0));

        if (transform.position.x < curTarget.transform.position.x)
        {
            angle = 180 + (180 - angle);
        }

        headRoot.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    private void Update()
    {
        statusController.Update(Time.deltaTime);

        OnUpdate?.Invoke(Time.deltaTime);

        if (isAttackable && attackComponent.FindTarget())
        {
            attackComponent.Attack();
        }
        else
        {
            attackComponent.InitTarget();
        }

        additionalEffectRoot.transform.Rotate(Vector3.forward * Time.deltaTime * 20);
        foreach(var render in additionalEffectRender)
        {
            render.transform.rotation = Quaternion.identity;
        }
    }
}
