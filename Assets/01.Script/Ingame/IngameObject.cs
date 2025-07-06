using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using DG.Tweening;

using IngameObject_;

[System.Serializable]
public struct IngameObjectData
{
    public IngameObjectData
        (IngameObject.EType unitType
        , int maxHp
        , float moveSpeed
        , int cost = 1
        , float defense = 0
        , int level = 1
        , IngameObject.EClass unitClass = IngameObject.EClass.Normal
        , float regeneration = 0
        , float resistance = 0
        , int maxShield = 0
        , string[] skillIds = null)
    {
        this.unitType = unitType;
        this.maxHp = maxHp;
        this.moveSpeed = moveSpeed;
        this.cost = cost;
        this.defense = defense;
        this.level = level;
        this.unitClass = unitClass;
        this.regeneration = regeneration;
        this.resistance = resistance;
        this.maxShield = maxShield;
        this.skillIds = skillIds;
    }

    //public IngameObjectData(IngameObjectData data)
    //{
    //    this.unitType = data.unitType;
    //    this.maxHp = data.maxHp;
    //    this.moveSpeed = data.moveSpeed;
    //    this.cost = data.cost;
    //    this.defense = data.defense;
    //    this.level = data.level;
    //    this.unitClass = data.unitClass;
    //    this.regeneration = data.regeneration;
    //    this.resistance = data.resistance;
    //    this.maxShield = data.maxShield;
    //}

    public IngameObject.EType unitType;
    public int maxHp;
    public float moveSpeed;
    public int cost;
    public float defense;
    public int level;
    public IngameObject.EClass unitClass;
    public float regeneration; // 재생력 (일정시간동안 피해를 받지 않았을 때, 체력이 지속적으로 회복된다.)
    public float resistance; // 저항력 (상태이상과 군중제어 효과가 빨리 풀린다.)
    public int maxShield; // 보호막
    public string[] skillIds;

    public string GetLog()
    {
        return string.Format("IngameObjectData({0} / {1} / {2} / {3} / {4})", unitType, maxHp, moveSpeed, cost, defense);
    }
}

public class IngameAttackInfo
{
    public IngameAttackInfo()
    {
        attacker = null;
        target = null;
        damage = 0;
        damageType = EDamageType.None;
        penetration = 0;

        transitionCount = 0;
        transitionDmgRatio = 0;
        excludeTransitionTargets = new List<string>();

        splashRange = 0;
        splashDmgRatio = 0;

        addProjectileMoveSpeed = 0;

        //additionalEffectTypes = new List<TowerAdditionalEffect.EType>();
    }

    public IngameAttackInfo(IngameAttackInfo attInfo)
    {
        attacker = attInfo.attacker;
        target = attInfo.target;

        damage = attInfo.damage;
        damageType = attInfo.damageType;
        penetration = attInfo.penetration;

        transitionCount = attInfo.transitionCount;
        transitionDmgRatio = attInfo.transitionDmgRatio;
        excludeTransitionTargets = new List<string>();
        foreach (var t in attInfo.excludeTransitionTargets)
        {
            excludeTransitionTargets.Add(t);
        }

        splashRange = attInfo.splashRange;
        splashDmgRatio = attInfo.splashDmgRatio;

        addProjectileMoveSpeed = attInfo.addProjectileMoveSpeed;

        //additionalEffectTypes = new List<TowerAdditionalEffect.EType>();
        //for(int i = 0; i<attInfo.additionalEffectTypes.Count; ++i)
        //{
        //    additionalEffectTypes.Add(attInfo.additionalEffectTypes[i]);
        //}

        OnApplyAdditionalEffect = attInfo.OnApplyAdditionalEffect;
    }

    public GridBlock_Tower attacker;
    public IngameObject target;

    public int damage;
    public EDamageType damageType;
    public float penetration; // 관통력
    public bool isCritical; // 치명타

    /// <summary>
    /// 전이 횟수
    /// </summary>
    public int transitionCount;
    /// <summary>
    /// 전이 투사체의 공격력 비율
    /// </summary>
    public float transitionDmgRatio;
    
    public float splashRange;
    public float splashDmgRatio;

    public float addMaxHpProportionalDmg;
    public float addProjectileMoveSpeed;

    public List<string> excludeTransitionTargets;

    //public List<TowerAdditionalEffect.EType> additionalEffectTypes;
    public UnityAction<IngameObject, IngameAttackInfo> OnApplyAdditionalEffect;
    public void AddAdditionalEffect(UnityAction<IngameObject, IngameAttackInfo> OnApplyAdditionalEffect, TowerAdditionalEffect.EType taeType = TowerAdditionalEffect.EType.None)
    {
        this.OnApplyAdditionalEffect += OnApplyAdditionalEffect;
        //additionalEffectTypes.Add(taeType);
    }
    public void SubAdditionalEffect(UnityAction<IngameObject, IngameAttackInfo> OnApplyAdditionalEffect, TowerAdditionalEffect.EType taeType = TowerAdditionalEffect.EType.None)
    {
        this.OnApplyAdditionalEffect -= OnApplyAdditionalEffect;
    }

    public System.Func<IngameAttackInfo, IngameAttackInfo> OnAddSplashDamage;
}

public class IngameAttackedInfo
{
    public IngameAttackInfo attackInfo;
    public IngameObject defender;
    public int recvDmg;
}

/// <summary>
/// 인게임에서 체력을 보유하여 공격당할 수 있는 모든 오브젝트
/// </summary>
public class IngameObject : MonoBehaviour
{
    public enum EType
    {
        /// <summary>
        /// WayPoint를 따라 이동하는 유닛
        /// </summary>
        Walker = 1,

        /// <summary>
        /// SkyWayPoint를 따라 이동하는 유닛
        /// </summary>
        Flyer = 2,

        /// <summary>
        /// WayPoint를 따라 움직이다 가까이 있는 Wall을 파괴하는 유닛
        /// </summary>
        Destroyer = 4
    }

    public enum EClass
    {
        Normal = 0,
        Hero = 1,
        Boss = 2,
    }

    [SerializeField]
    private string m_PrefabKey;
    public string prefabKey { get { return m_PrefabKey; } }

    public IngameObjectData data;
    
    public IntVector2 curWayPointGridPos;
    public GridBlock startBlock;
    public GridBlock endBlock;

    public bool isInitialized { get; private set; }
    public float uiHeight = 0.1f;
    public float caledUIHeight = 0.1f;
    
    #region < UnitType >
    public EType unitType { get; private set; }
    public bool EuqalUnitType(EType unitType)
    {
        return (this.unitType & unitType) == unitType;
    }
    public bool IncludedUnitType(EType unitType)
    {
        return (this.unitType & unitType) > 0;
    }
    #endregion

    #region < Ability >

    public int level { get { return data.level; } }
    public EClass unitClass { get { return data.unitClass; } }
    public int cost { get { return data.cost; } }

    public int maxHp { get { return (int)((data.maxHp + (int)statusController.GetFixedValue(EAbilityType.MaxHp)) * statusController.GetPercentValue(EAbilityType.MaxHp)); } }
    [SerializeField]
    private int m_NowHp;
    public int nowHp
    {
        get { return m_NowHp; }
        set { m_NowHp = value; if (unitHpUI != null) unitHpUI.UpdateUI(); }
    }
    public bool isFullHp { get { return (maxHp <= nowHp); } }

    public int maxShield { get { return (int)((data.maxShield + (int)statusController.GetFixedValue(EAbilityType.MaxShield)) * statusController.GetPercentValue(EAbilityType.MaxShield)); } }
    [SerializeField]
    private int m_NowShield;
    public int nowShield
    {
        get { return m_NowShield; }
        set {
            m_NowShield = value;
            if (unitHpUI != null) unitHpUI.UpdateUI();
            
            if(m_NowShield > 0)
            {
                if(shieldFx == null && SpawnMaster.TrySpawnFx<Fx_Switch>("Fx_Shield", transform.position, Quaternion.identity, out shieldFx))
                {
                    shieldFx.transform.SetParent(transform);
                    shieldFx.transform.localPosition = Vector3.zero;
                    shieldFx.transform.localScale = new Vector3(bodySize, bodySize, bodySize);
                    shieldFx?.On();
                }
                else if (!shieldFx.isPlayingOn)
                {
                    shieldFx?.On();
                }
            }
            else if (shieldFx != null)
            {
                shieldFx.Off();
                shieldFx.Destroy();
                shieldFx = null;
            }
        }
    }
    public bool isFullShield { get { return (maxShield <= nowShield); } }
    public float healingShieldTimer { get; private set; }

    public float moveSpeed { get { return Mathf.Max(0.1f, (data.moveSpeed + statusController.GetFixedValue(EAbilityType.MoveSpeed)) * statusController.GetPercentValue(EAbilityType.MoveSpeed)); } }
    public float defense { get { return Mathf.Clamp((data.defense + statusController.GetFixedValue(EAbilityType.Defense)) * statusController.GetPercentValue(EAbilityType.Defense), 0, 0.99f); } }
    public float resistance { get { return Mathf.Clamp01((data.resistance + statusController.GetFixedValue(EAbilityType.Resistance)) * statusController.GetPercentValue(EAbilityType.Regeneration)); } }
    public float regeneration { get { return Mathf.Clamp01((data.regeneration + statusController.GetFixedValue(EAbilityType.Regeneration)) * statusController.GetPercentValue(EAbilityType.Regeneration)); } }
    public float regenerationOneTimer { get; private set; }

    #endregion

    private Fx_Switch shieldFx;

    private IEnumerator itorDieDelay;
    public bool isDie { get { return nowHp <= 0; } }
    public bool isMovable { get { return startBlock != null && endBlock != null && !isDie && (OnCheckMovable != null) ? OnCheckMovable() : true; ; } }

    // 마지막 공격이후 지난 시간
    public float lastAttackedTime { get; private set; }

    private StatusController m_StatusController = new StatusController();
    public StatusController statusController => m_StatusController;

    private List<SkillComponent> m_SkillComponents = new List<SkillComponent>();
    public List<SkillComponent> skillComponents => m_SkillComponents;

    public IngameUI_UnitHp unitHpUI;

    [SerializeField]
    private GameObject m_Root;
    public GameObject root => m_Root;

    [SerializeField]
    private GameObject m_Body;
    public GameObject body => m_Body;

    [SerializeField]
    private SpriteRenderer m_BodyRender;
    public SpriteRenderer bodyRender => m_BodyRender;

    [SerializeField]
    private Sprite[] m_BodySprites;
    public Sprite[] bodySprites => m_BodySprites;

    public int bodySpriteIdx { get; private set; }
    public float bodySpriteTimer { get; private set; }

    [SerializeField]
    private float m_BodySize;
    public float bodySize { get { return m_BodySize; } private set { m_BodySize = value; } }

    public Vector3 originBodyLocalScale { get; private set; }
    public Vector3 originBodyLocalScaleOneTenth { get; private set; }

    #region < Animator - Prop >

    private Tween rotAnimator;
    private Tween scaleAnimator;
    private Tween posAnimator;

    #endregion

    #region < Event >

    public System.Func<bool> OnCheckMovable;
    public UnityAction<IngameObject> OnDie;
    public UnityAction OnAttacked;
    
    #endregion

    public void Initialize()
    {
        if(!isInitialized)
        {
            isInitialized = true;
            statusController.Initialize(this);
        }
    }

    #region < Animator - Func >

    public void PlayAnimator()
    {
        posAnimator?.Play();
        rotAnimator?.Play();
        scaleAnimator?.Play();
    }

    public void PlayAttackedAnimator(EDamageType dmgType = EDamageType.None)
    {
        if(dmgType == EDamageType.Poison || dmgType == EDamageType.Burning)
        {
            bodyRender.material.SetColor("_AttackedColor", Global.GetDamageColor(dmgType, false));
        }
        else
        {
            bodyRender.material.SetColor("_AttackedColor", Color.red);
        }

        bodyRender.material
            .SetFloat("_AttackedColorAmount", 1);

        bodyRender.material
            .DOFloat(0, "_AttackedColorAmount", 0.2f)
            .SetEase(Ease.OutSine);
    }

    public void StopAnimator()
    {
        posAnimator?.Pause();
        rotAnimator?.Pause();
        scaleAnimator?.Pause();
    }

    private void CreateAnimator()
    {
        root.transform.localPosition = new Vector3(0, 0, 0);
        root.transform.localRotation = Quaternion.Euler(0, 0, -5);
        root.transform.localScale = originBodyLocalScale;

        posAnimator = root.transform
            .DOLocalMoveY(0.05f, 0.3f)
            .SetEase(Ease.InFlash)
            .SetLoops(-1, LoopType.Yoyo);

        rotAnimator = root.transform
            .DOLocalRotate(new Vector3(0, 0, 5), 0.3f, RotateMode.Fast)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);

        scaleAnimator = root.transform
            .DOScale(originBodyLocalScale + originBodyLocalScaleOneTenth, 0.4f)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);

        posAnimator.Pause();
        rotAnimator.Pause();
        scaleAnimator.Pause();
    }

    private void DestroyAnimator()
    {
        if (posAnimator != null && posAnimator.IsActive())
        {
            posAnimator.Kill();
        }
        posAnimator = null;

        if (rotAnimator != null && rotAnimator.IsActive())
        {
            rotAnimator.Kill();
        }
        rotAnimator = null;
        
        if (scaleAnimator != null && scaleAnimator.IsActive())
        {
            scaleAnimator.Kill();
        }
        scaleAnimator = null;
    }

    #endregion

    public void SetData(IngameObjectData data, IngameUI_UnitHp unitHpUI)
    {
        this.data = data;
        this.unitHpUI = unitHpUI;

        unitType = data.unitType;
        nowHp = data.maxHp;
        nowShield = data.maxShield;
        
        for (int i = 0; i < skillComponents.Count; ++i)
        {
            skillComponents[i]?.Release();
        }
        skillComponents.Clear();

        switch (unitClass)
        {
            default:
            case EClass.Normal:
                originBodyLocalScale = Vector3.one;
                originBodyLocalScaleOneTenth = originBodyLocalScale * 0.1f;
                bodyRender.material = ResourceManager.Instance.GetMaterial("SpriteUnit_Normal");
                break;

            case EClass.Hero:
                originBodyLocalScale = new Vector3(1.25f, 1.25f, 1);
                originBodyLocalScaleOneTenth = originBodyLocalScale * 0.1f;
                bodyRender.material = ResourceManager.Instance.GetMaterial("SpriteUnit_Hero");
                break;

            case EClass.Boss:
                originBodyLocalScale = new Vector3(1.5f, 1.5f, 1);
                originBodyLocalScaleOneTenth = originBodyLocalScale * 0.1f;
                bodyRender.material = ResourceManager.Instance.GetMaterial("SpriteUnit_Boss");
                break;
        }

        if (data.skillIds != null && data.skillIds.Length > 0)
        {
            for (int i = 0; i < data.skillIds.Length; ++i)
            {
                var curSkillData = data.skillIds[i];
                IngameObject_.SkillData skillData;
                if (DataManager.Instance.TryGetIngameObjectSkill(curSkillData, out skillData))
                {
                    Debug.LogFormat("<color=red>{0}</color>", skillData.skillId);
                    var skill = SkillComponent.CreateSkill(skillData);
                    if (skill != null)
                    {
                        skill.Initialize(this);
                        skill.SetData(skillData);
                        skillComponents.Add(skill);
                    }
                }
            }
        }
        
        caledUIHeight = uiHeight * originBodyLocalScale.y;

        bodySpriteIdx = 0;

        CreateAnimator();
        PlayAnimator();
    }

    public void SetMoveRoot(GridBlock startBlock, GridBlock endBlock)
    {
        curWayPointGridPos = startBlock.gridPos;

        this.startBlock = startBlock;
        this.endBlock = endBlock;
    }
    
    public void Ready()
    {

    }

    public void HealHp(int fixedAmount, bool showFx = false)
    {
        var calAmount = Mathf.Clamp(nowHp + fixedAmount, 0, maxHp);
        nowHp = calAmount;

        if (showFx)
        {
            Fx_OneTime fx;
            if (SpawnMaster.TrySpawnFx("Fx_Heal_1", transform.position, Quaternion.identity, out fx))
            {
                fx.transform.localScale = new Vector3(bodySize, bodySize, bodySize);
                fx.On();
            }
        }

        UIManager_Ingame.Instance.ShowDamageUI(transform.position, calAmount, EDamageType.HealHp, false);
    }

    public void HealShield(int fixedAmount, bool showFx = false)
    {
        var calAmount = Mathf.Clamp(nowShield + fixedAmount, 0, maxShield);
        nowShield = calAmount;

        //if (showFx)
        //{
        //    Fx_OneTime fx;
        //    if (SpawnMaster.TrySpawnFx("Fx_Heal_1", transform.position, Quaternion.identity, out fx))
        //    {
        //        fx.transform.localScale = new Vector3(bodySize, bodySize, bodySize);
        //        fx.On();
        //    }
        //}

        UIManager_Ingame.Instance.ShowDamageUI(transform.position, calAmount, EDamageType.HealShield, false);
    }

    public bool Attacked(IngameAttackInfo attackInfo, out IngameAttackedInfo attackedInfo)
    {
        if (isDie)
        {
            attackedInfo = null;
            return false;
        }

        int damageToShield = 0;
        int damageToHp = 0;
        lastAttackedTime = 0;

        // 최대 체력 비례 추가 데미지 계산
        attackInfo.damage += Mathf.RoundToInt(maxHp * attackInfo.addMaxHpProportionalDmg);

        // 쉴드만큼 피해를 입힌다. (방어력 무시)
        if (nowShield >= attackInfo.damage)
        {
            // 피해량이 쉴드량보다 적은 경우
            nowShield -= attackInfo.damage;
            damageToShield = attackInfo.damage;
        }
        else
        {
            // 쉴드량보다 피해량이 큰 경우
            damageToShield = nowShield;
            nowShield = 0;
        }
        
        // 방어력 만큼 피해량 감소
        attackInfo.damage = Mathf.Max(0, attackInfo.damage - Mathf.RoundToInt((attackInfo.damage - damageToShield) * Mathf.Max(0, (defense - attackInfo.penetration))));

        attackedInfo = new IngameAttackedInfo()
        {
            attackInfo = attackInfo,
            defender = this,
            recvDmg = attackInfo.damage,
        };

        UIManager_Ingame.Instance.ShowDamageUI(transform.position, attackInfo.damage, attackInfo.damageType, attackInfo.isCritical);
        
        nowHp = Mathf.Max(0, nowHp - (attackInfo.damage - damageToShield));

        regenerationOneTimer = 1f;
        healingShieldTimer = 1f;

        if (attackInfo.OnApplyAdditionalEffect != null)
        {
            attackInfo.OnApplyAdditionalEffect(this, attackInfo);
        }

        if (nowHp == 0)
        {
            if (attackInfo.attacker != null)
            {
                attackInfo.attacker.AddKillCount(1);
                attackInfo.attacker.AddExperiencePoint(cost);
            }

            IngameManager.Instance.AddKillScore(cost);
            UIManager_Ingame.Instance.ShowCostParticle(transform.position, cost);

            // 공격당하여 죽음
            Die();
        }
        else
        {
            PlayAttackedAnimator(attackInfo.damageType);
        }

        return true;
    }
    
    public bool Attacked(IngameAttackInfo attackInfo)
    {
        if (isDie)
        {
            return false;
        }

        int damageToShield = 0;
        int damageToHp = 0;
        lastAttackedTime = 0;

        // 최대 체력 비례 추가 데미지 계산
        attackInfo.damage += Mathf.RoundToInt(maxHp * attackInfo.addMaxHpProportionalDmg);
        
        // 쉴드만큼 피해를 입힌다. (방어력 무시)
        if (nowShield >= attackInfo.damage)
        {
            // 피해량이 쉴드량보다 적은 경우
            nowShield -= attackInfo.damage;
            damageToShield = attackInfo.damage;
        }
        else
        {
            // 쉴드량보다 피해량이 큰 경우
            damageToShield = nowShield;
            nowShield = 0;
        }
        
        // 방어력 만큼 피해량 감소
        attackInfo.damage = Mathf.Max(0, attackInfo.damage - Mathf.RoundToInt((attackInfo.damage - damageToShield) * Mathf.Max(0, (defense - attackInfo.penetration))));

        UIManager_Ingame.Instance.ShowDamageUI(transform.position, attackInfo.damage, attackInfo.damageType);

        nowHp = Mathf.Max(0, nowHp - (attackInfo.damage - damageToShield));

        regenerationOneTimer = 1f;
        healingShieldTimer = 1f;

        OnAttacked?.Invoke();
        attackInfo.OnApplyAdditionalEffect?.Invoke(this, attackInfo);

        if (nowHp == 0)
        {
            if(attackInfo.attacker != null)
            {
                attackInfo.attacker.AddKillCount(1);
                attackInfo.attacker.AddExperiencePoint(cost);
            }

            UIManager_Ingame.Instance.ShowCostParticle(transform.position, cost);

            // 공격당하여 죽음
            Die();
        }
        else
        {
            PlayAttackedAnimator(attackInfo.damageType);
        }

        return true;
    }

    public bool FindNextWayPoint(out Vector3 dir)
    {
        GridInfo curGridInfo, nxtGridInfo;
        var gridMap = AppManager.Instance.gridMap;

        if (EuqalUnitType(EType.Destroyer))
        {
            if (gridMap.TryGetGridInfo(curWayPointGridPos, out curGridInfo))
            {
                if(curGridInfo.hasNxtWallGridInfo)
                {
                    nxtGridInfo = curGridInfo.nxtWallGridInfo;
                    var nxtWayPoint = nxtGridInfo.gridBlock;

                    // 다음 WayPoint에 도달하지 못한 경우
                    var dis = Vector3.Distance(transform.position, nxtWayPoint.transform.position);
                    if (dis > 0.02f)
                    {
                        dir = (nxtWayPoint.transform.position - transform.position).normalized;
                        dir = new Vector3(dir.x, dir.y, 0);
                        return true;
                    }
                    // EndPoint에 도달한 경우
                    else if (nxtWayPoint.EqualGridType(GridBlock.EType.EndPoint))
                    {
                        curWayPointGridPos = startBlock.gridPos;

                        // 시작 waypoint로 이동시키고 피해를 입힘
                        UIManager_Ingame.Instance.ShowAttackLifeParticle(transform.position, 1);
                        transform.position = startBlock.transform.position;

                        dir = Vector3.zero;
                        return true;
                    }
                    // EndPoint에 도달하지 못한 경우
                    else
                    {
                        GridBlock_Wall gridBlockWall = nxtGridInfo.gridBlock as GridBlock_Wall;
                        if (gridBlockWall.isWallOpened)
                        {
                            curWayPointGridPos = nxtGridInfo.gridPos;
                            curGridInfo = nxtGridInfo;

                            if (nxtGridInfo.TryGetNxtWayGridInfo(out nxtGridInfo))
                            {
                                nxtWayPoint = nxtGridInfo.gridBlock;
                            }

                            dir = (nxtWayPoint.transform.position - transform.position).normalized;
                            dir = new Vector3(dir.x, dir.y, 0);
                            return true;
                        }
                        else
                        {
                            WallAttackedInfo attedInfo;
                            gridBlockWall.Attacked(nowHp + nowShield, out attedInfo);

                            UIManager_Ingame.Instance.ShowCostParticle(transform.position, data.cost);

                            Attacked(new IngameAttackInfo()
                            {
                                attacker = null,
                                damage = attedInfo.recvDmg,
                                damageType = EDamageType.None,
                                target = this
                            });
                            statusController.ApplyStatusEffect(new SE_Freezing() { id = "Wall_Freezing", duration = 2f, freezingPower = -0.4f });

                            dir = Vector3.zero;
                            return false;
                        }
                    }
                }
                else if(curGridInfo.TryGetNxtWayGridInfo(out nxtGridInfo))
                {
                    var nxtWayPoint = nxtGridInfo.gridBlock;

                    // 다음 WayPoint에 도달하지 못한 경우
                    var dis = Vector3.Distance(transform.position, nxtWayPoint.transform.position);
                    if (dis > 0.02f)
                    {
                        dir = (nxtWayPoint.transform.position - transform.position).normalized;
                        dir = new Vector3(dir.x, dir.y, 0);
                        return true;
                    }
                    // EndPoint에 도달한 경우
                    else if (nxtWayPoint.EqualGridType(GridBlock.EType.EndPoint))
                    {
                        curWayPointGridPos = startBlock.gridPos;

                        // 시작 waypoint로 이동시키고 피해를 입힘
                        IngameManager.Instance.AddLife(-1);
                        transform.position = startBlock.transform.position;

                        dir = Vector3.zero;
                        return true;
                    }
                    // EndPoint에 도달하지 못한 경우
                    else
                    {
                        curWayPointGridPos = nxtGridInfo.gridPos;
                        curGridInfo = nxtGridInfo;

                        if (nxtGridInfo.TryGetNxtWayGridInfo(out nxtGridInfo))
                        {
                            nxtWayPoint = nxtGridInfo.gridBlock;
                        }

                        dir = (nxtWayPoint.transform.position - transform.position).normalized;
                        dir = new Vector3(dir.x, dir.y, 0);
                        return true;
                    }
                }
            }
        }
        else if (EuqalUnitType(EType.Walker))
        {
            if (gridMap.TryGetGridInfo(curWayPointGridPos, out curGridInfo)
                && curGridInfo.TryGetNxtWayGridInfo(out nxtGridInfo))
            {
                var nxtWayPoint = nxtGridInfo.gridBlock;

                // 다음 WayPoint에 도달하지 못한 경우
                var dis = Vector3.Distance(transform.position, nxtWayPoint.transform.position);
                if (dis > 0.02f)
                {
                    dir = (nxtWayPoint.transform.position - transform.position).normalized;
                    dir = new Vector3(dir.x, dir.y, 0);
                    return true;
                }
                // EndPoint에 도달한 경우
                else if (nxtWayPoint.EqualGridType(GridBlock.EType.EndPoint))
                {
                    curWayPointGridPos = startBlock.gridPos;

                    // 시작 waypoint로 이동시키고 피해를 입힘
                    UIManager_Ingame.Instance.ShowAttackLifeParticle(transform.position, 1);
                    transform.position = startBlock.transform.position;

                    dir = Vector3.zero;
                    return true;
                }
                // EndPoint에 도달하지 못한 경우
                else
                {
                    curWayPointGridPos = nxtGridInfo.gridPos;
                    curGridInfo = nxtGridInfo;

                    if (nxtGridInfo.TryGetNxtWayGridInfo(out nxtGridInfo))
                    {
                        nxtWayPoint = nxtGridInfo.gridBlock;
                    }

                    dir = (nxtWayPoint.transform.position - transform.position).normalized;
                    dir = new Vector3(dir.x, dir.y, 0);
                    return true;
                }
            }
        }
        else if(EuqalUnitType(EType.Flyer))
        {
            if (gridMap.TryGetGridInfo(curWayPointGridPos, out curGridInfo)
                && curGridInfo.TryGetSkyWayNxtGridInfo(out nxtGridInfo))
            {
                var nxtWayPoint = nxtGridInfo.gridBlock;

                // 다음 WayPoint에 도달하지 못한 경우
                var dis = Vector3.Distance(transform.position, nxtWayPoint.transform.position);
                if (dis > 0.02f)
                {
                    dir = (nxtWayPoint.transform.position - transform.position).normalized;
                    dir = new Vector3(dir.x, dir.y, 0);
                    return true;
                }
                // EndPoint에 도달한 경우
                else if (nxtWayPoint.EqualGridType(GridBlock.EType.EndPoint))
                {
                    curWayPointGridPos = startBlock.gridPos;

                    // 시작 waypoint로 이동시키고 피해를 입힘
                    UIManager_Ingame.Instance.ShowAttackLifeParticle(transform.position, 1);
                    transform.position = startBlock.transform.position;

                    dir = Vector3.zero;
                    return true;
                }
                // EndPoint에 도달하지 못한 경우
                else
                {
                    curWayPointGridPos = nxtGridInfo.gridPos;
                    curGridInfo = nxtGridInfo;

                    if (nxtGridInfo.TryGetSkyWayNxtGridInfo(out nxtGridInfo))
                    {
                        nxtWayPoint = nxtGridInfo.gridBlock;
                    }

                    dir = (nxtWayPoint.transform.position - transform.position).normalized;
                    dir = new Vector3(dir.x, dir.y, 0);
                    return true;
                }
            }
        }

        dir = Vector3.zero;
        return false;
    }
    
    public void Move()
    {
        if (!isMovable)
        {
            return;
        }

        Vector3 dir;
        if (FindNextWayPoint(out dir))
        {
            bodyRender.flipX = (dir.x < 0);
            transform.position += dir * Time.deltaTime * moveSpeed;
        }
    }

    public void Regenerate()
    {
        var curRegeneration = regeneration;
        if (curRegeneration == 0)
        {
            return;
        }
        if(regenerationOneTimer > 0)
        {
            regenerationOneTimer -= Time.deltaTime;
        }
        else
        {
            regenerationOneTimer = 1f;
            HealHp(Mathf.RoundToInt(maxHp * curRegeneration), true);
        }
    }

    public void HealingShield()
    {
        var curMaxShield = maxShield;
        if (curMaxShield <= 0 || lastAttackedTime < GameSettingsManager.RegenerationDelayTime)
        {
            return;
        }
        else if (healingShieldTimer > 0)
        {
            healingShieldTimer -= Time.deltaTime;
        }
        else
        {
            healingShieldTimer = 1f;
            HealShield(Mathf.RoundToInt(curMaxShield * 0.05f));
        }
    }
    
    private void Update()
    {
        if (isDie) return;

        // 상태이상
        statusController.Update(Time.deltaTime);

        // 재생
        Regenerate();
        HealingShield();

        // 이동
        Move();

        if(bodySprites != null && bodySprites.Length > 0)
        {
            if (bodySpriteTimer > 0)
            {
                bodySpriteTimer -= Time.deltaTime;
            }
            else
            {
                bodySpriteTimer = 0.3f;
                bodySpriteIdx = ++bodySpriteIdx % bodySprites.Length;
                bodyRender.sprite = bodySprites[bodySpriteIdx];
            }
        }

        // 체력 UI 이동
        unitHpUI?.FollowTarget();

        // 스킬 사용
        for(int i = 0; i< skillComponents.Count; ++i)
        {
            skillComponents[i].Update(Time.deltaTime, Time.unscaledDeltaTime);
            if(skillComponents[i].CheckCanUseSkill()) skillComponents[i].UseSkill();
        }

        lastAttackedTime += Time.deltaTime;
    }

    public void Die()
    {
        nowHp = 0;
        nowShield = 0;

        if (itorDieDelay == null)
        {
            if (OnDie != null) OnDie(this);

            itorDieDelay = DestroyDelay(0);
            StartCoroutine(itorDieDelay);
        }
    }

    private IEnumerator DestroyDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        DestroySelf();
    }

    public void DestroySelf()
    {
        Fx_OneTime fx;
        if(SpawnMaster.TrySpawnFx("Fx_Die_1", transform.position, Quaternion.identity, out fx))
        {
            fx.transform.localScale = new Vector3(bodySize, bodySize, bodySize);
            fx.On();
        }

        if(unitHpUI != null)
        {
            unitHpUI.DestroySelf();
            unitHpUI = null;
        }

        if (itorDieDelay != null)
        {
            StopCoroutine(itorDieDelay);
            itorDieDelay = null;
        }

        StopAnimator();

        statusController.Clear();

        for (int i = 0; i < skillComponents.Count; ++i)
        {
            skillComponents[i]?.Release();
        }
        skillComponents.Clear();

        IngameManager.Instance.UnregistObject(this);

        SpawnMaster.Destroy(gameObject, prefabKey);
    }

    private void OnDestroy()
    {
        DestroyAnimator();
    }
}
