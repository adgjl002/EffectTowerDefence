using IngameGrid;
using IngameObject_;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타워 추가 효과
/// </summary>
public abstract class TowerAdditionalEffect
{
    public enum EType
    {   
        None = 0,

        /// <summary> 관통 효과 </summary>
        Penetrate = 1,

        /// <summary> 폭발 효과 </summary>
        Explosion = 2,

        /// <summary> 빙결 효과 </summary>
        Freezing = 3,

        /// <summary> 독 효과 </summary>
        Poison = 4,

        /// <summary> 감전 효과 </summary>
        ElectricShock = 5,

        /// <summary> 전이 효과 </summary>
        Transition = 6,

        /// <summary> 공격력 증가 효과 </summary>
        Ability_AttackDamage = 7,

        /// <summary> 공격범위 증가 효과 </summary>
        Ability_AttackRange = 8,

        /// <summary> 공격속도 증가 효과 </summary>
        Ability_AttackSpeed = 9,

        /// <summary> 치명타 확률 증가 효과 </summary>
        Ability_CriticalPerc = 10,

        /// <summary> 치명타 피해량 증가 효과 </summary>
        Ability_CriticalDamage = 11,

        /// <summary> 멀티샷 효과 </summary>
        MultyShot = 12,

        /// <summary> 버서커 효과 </summary>
        Berserker = 13,

        /// <summary> 더블샷 효과 </summary>
        DoubleShot = 14,

        /// <summary> 레이저 효과 </summary>
        Laser = 15,

        /// <summary> 파쇄 효과 </summary>
        Crush = 16,

        /// <summary> 치명상 효과 </summary>
        DeathBlow = 17,

        /// <summary> 성장통 효과 </summary>
        GrowingPains = 18,

        /// <summary> 통찰력 효과 </summary>
        Insight = 19,

        /// <summary> 화학반응 효과 </summary>
        ChemicalReaction = 20,

        /// <summary> 바람의기운 효과 </summary>
        AuraOfWind = 21,
    }

    public abstract EType effectType { get; }
    public virtual string spriteKey { get { return GetSpriteKey(effectType); } }
    public string GetSkillKey(int subNo, int level)
    {
        return string.Format("Skill_TAE_{0}_{1}_{2}", (int)effectType, subNo, level);
    }

    public GridBlock_Tower owner { get; private set; }
    public bool isRegisted { get; private set; }
    public int cost { get; private set; }

    public int appliedCount { get; private set; }
    public bool isSingle { get { return appliedCount == 1; } }
    public bool isDouble { get { return appliedCount == 2; } }
    public bool isTriple { get { return appliedCount == 3; } }
    public bool isQuadruple { get { return appliedCount == 4; } }
    
    public virtual void SetData(string[] datas)
    {
        cost = int.Parse(datas[0]);
    }
    public void Regist(GridBlock_Tower tower)
    {
        if(!isRegisted)
        {
            isRegisted = true;
            owner = tower;

            appliedCount = tower.GetAppliedAdditionalEffectCount(effectType);

            OnRegist(tower);
        }
    }
    public void Unregist(GridBlock_Tower tower)
    {
        if (isRegisted)
        {
            OnUnregsit(tower);
            owner = null;
            isRegisted = false;
        }
    }

    public abstract void OnRegist(GridBlock_Tower tower);
    public abstract void OnUnregsit(GridBlock_Tower tower);
    
    public virtual string GetLog()
    {
        return string.Format("TowerAdditionalEffect Type({0})", effectType);
    }
    public static string GetSpriteKey(EType effectType)
    {
        switch(effectType)
        {
            case EType.None: return string.Empty;
            case EType.Penetrate: return "TAE_1";
            case EType.Explosion: return "TAE_2";
            case EType.Freezing: return "TAE_3";
            case EType.Poison: return "TAE_4";
            case EType.ElectricShock: return "TAE_5";
            case EType.Transition: return "TAE_6";
            case EType.Ability_AttackDamage: return "TAE_7";
            case EType.Ability_AttackRange: return "TAE_8";
            case EType.Ability_AttackSpeed: return "TAE_9";
            case EType.Ability_CriticalPerc: return "TAE_10";
            case EType.Ability_CriticalDamage: return "TAE_11";
            case EType.MultyShot: return "TAE_12";
            case EType.Berserker: return "TAE_13";
            case EType.DoubleShot: return "TAE_14";
            case EType.Laser: return "TAE_15";
            case EType.Crush: return "TAE_16";
            case EType.DeathBlow: return "TAE_17";
            case EType.GrowingPains: return "TAE_18";
            case EType.Insight: return "TAE_19";
            case EType.ChemicalReaction: return "TAE_20";
            case EType.AuraOfWind: return "TAE_21";
            default: Debug.LogErrorFormat("TowerAdditionalEffectType({0}) is not implemented.", effectType); return string.Empty;
        }
    }

    public static TowerAdditionalEffect Create(SkillData skillData)
    {
        TowerAdditionalEffect tae;
        switch ((EType)skillData.skillType)
        {
            default:
            case EType.Ability_AttackDamage: tae = new TowerAdditionalEffect_Ability_AttackDamage(); break;
            case EType.Ability_AttackRange: tae = new TowerAdditionalEffect_Ability_AttackRange(); break;
            case EType.Ability_AttackSpeed: tae = new TowerAdditionalEffect_Ability_AttackSpeed(); break;
            case EType.Ability_CriticalDamage: tae = new TowerAdditionalEffect_Ability_CriticalDamage(); break;
            case EType.Ability_CriticalPerc: tae = new TowerAdditionalEffect_Ability_CriticalPerc(); break;

            case EType.Berserker:
                switch (skillData.skillStyle)
                {
                    default:
                    case 0: tae = new TowerAdditionalEffect_Berserker(); break;
                    case 1: tae = new TowerAdditionalEffect_Berserker_S1(); break;
                    case 2: tae = new TowerAdditionalEffect_Berserker_S2(); break;
                    case 3: tae = new TowerAdditionalEffect_Berserker_S3(); break;
                }
                break;

            case EType.DoubleShot:
                switch (skillData.skillStyle)
                {
                    default:
                    case 0: tae = new TowerAdditionalEffect_DoubleShot(); break;
                }
                break;

            case EType.ElectricShock:
                switch (skillData.skillStyle)
                {
                    default:
                    case 0: tae = new TowerAdditionalEffect_ElectricShock(); break;
                    case 1: tae = new TowerAdditionalEffect_ElectricShock_S1(); break;
                    case 2: tae = new TowerAdditionalEffect_ElectricShock_S2(); break;
                }
                break;

            case EType.Explosion:
                switch (skillData.skillStyle)
                {
                    default:
                    case 0: tae = new TowerAdditionalEffect_Explosion(); break;
                    case 1: tae = new TowerAdditionalEffect_Explosion_S1(); break;
                }
                break;

            case EType.Freezing:
                switch (skillData.skillStyle)
                {
                    default:
                    case 0: tae = new TowerAdditionalEffect_Freezing(); break;
                    case 1: tae = new TowerAdditionalEffect_Freezing_S1(); break;
                    case 2: tae = new TowerAdditionalEffect_Freezing_S2(); break;
                }
                break;

            case EType.MultyShot:
                switch (skillData.skillStyle)
                {
                    default:
                    case 0: tae = new TowerAdditionalEffect_MultyShot(); break;
                }
                break;

            case EType.Penetrate:
                switch (skillData.skillStyle)
                {
                    default:
                    case 0: tae = new TowerAdditionalEffect_Penetrate(); break;
                }
                break;

            case EType.Poison:
                switch (skillData.skillStyle)
                {
                    default:
                    case 0: tae = new TowerAdditionalEffect_Poison(); break;
                    case 1: tae = new TowerAdditionalEffect_Poison_S1(); break;
                    case 2: tae = new TowerAdditionalEffect_Poison_S2(); break;
                }
                break;

            case EType.Transition:
                switch (skillData.skillStyle)
                {
                    default:
                    case 0: tae = new TowerAdditionalEffect_Transition(); break;
                    case 1: tae = new TowerAdditionalEffect_Transition_S1(); break;
                    case 2: tae = new TowerAdditionalEffect_Transition_S2(); break;
                }
                break;

            case EType.Crush:
                switch (skillData.skillStyle)
                {
                    default:
                    case 0: tae = new TowerAdditionalEffect_Crush(); break;
                }
                break;

            case EType.DeathBlow:
                switch (skillData.skillStyle)
                {
                    default:
                    case 0: tae = new TowerAdditionalEffect_DeathBlow(); break;
                }
                break;

            case EType.GrowingPains:
                switch (skillData.skillStyle)
                {
                    default:
                    case 0: tae = new TowerAdditionalEffect_GrowingPains(); break;
                }
                break;

            case EType.Insight:
                switch (skillData.skillStyle)
                {
                    default:
                    case 0: tae = new TowerAdditionalEffect_Insight(); break;
                }
                break;

            case EType.Laser:
                switch (skillData.skillStyle)
                {
                    default:
                    case 0: tae = new TowerAdditionalEffect_Laser(); break;
                }
                break;

            case EType.ChemicalReaction:
                switch (skillData.skillStyle)
                {
                    default:
                    case 0: tae = new TowerAdditionalEffect_ChemicalReaction(); break;
                }
                break;

            case EType.AuraOfWind:
                switch (skillData.skillStyle)
                {
                    default:
                    case 0: tae = new TowerAdditionalEffect_AuraOfWind(); break;
                }
                break;
        }
        
        tae.SetData(skillData.effectDatas);
        return tae;
    }
}

#region < Ability >
public class TowerAdditionalEffect_Ability_AttackDamage : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.Ability_AttackDamage; } }

    public int addFixedDamage { get; private set; }
    public float addPercDamage { get; private set; }
    public int penetration { get; private set; }

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        addFixedDamage = int.Parse(datas[1]);
        addPercDamage = float.Parse(datas[2]);
        penetration = int.Parse(datas[3]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        owner.OnAttack += OnAttack;

        if (addPercDamage > 0) tower.statusController.AddPercentValue(IngameGrid.EAbilityType.Damage, addPercDamage);
        if (addFixedDamage > 0) tower.statusController.AddFixedValue(IngameGrid.EAbilityType.Damage, addFixedDamage);
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        owner.OnAttack -= OnAttack;

        if (addPercDamage > 0) tower.statusController.AddPercentValue(IngameGrid.EAbilityType.Damage, -addPercDamage);
        if (addFixedDamage > 0) tower.statusController.AddFixedValue(IngameGrid.EAbilityType.Damage, -addFixedDamage);
    }

    private IngameAttackInfo OnAttack(IngameAttackInfo attInfo)
    {
        if (isTriple)
        {
            attInfo.penetration = penetration;
        }
        return attInfo;
    }
}
public class TowerAdditionalEffect_Ability_AttackRange : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.Ability_AttackRange; } }

    public float addAttackRange { get; private set; }
    public float addProjMoveSpeed { get; private set; }

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        addAttackRange = float.Parse(datas[1]);
        addProjMoveSpeed = float.Parse(datas[2]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        owner.OnAttack += OnAttack;

        if (addAttackRange > 0) tower.statusController.AddPercentValue(IngameGrid.EAbilityType.AttackRange, addAttackRange);
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        owner.OnAttack -= OnAttack;

        if (addAttackRange > 0) tower.statusController.AddPercentValue(IngameGrid.EAbilityType.AttackRange, -addAttackRange);
    }

    private IngameAttackInfo OnAttack(IngameAttackInfo attInfo)
    {
        if (isDouble)
        {
            attInfo.addProjectileMoveSpeed += addProjMoveSpeed;
        }
        return attInfo;
    }
}
public class TowerAdditionalEffect_Ability_AttackSpeed : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.Ability_AttackSpeed; } }

    public float addPercAttackSpeed { get; private set; }
    public float addDamageRatio { get; private set; }

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        addPercAttackSpeed = float.Parse(datas[1]);
        addDamageRatio = float.Parse(datas[2]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        owner.OnAttack += OnAttack;

        if (addPercAttackSpeed > 0) tower.statusController.AddPercentValue(IngameGrid.EAbilityType.AttackSpeed, addPercAttackSpeed);
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        owner.OnAttack -= OnAttack;

        if (addPercAttackSpeed > 0) tower.statusController.AddPercentValue(IngameGrid.EAbilityType.AttackSpeed, -addPercAttackSpeed);
    }

    private IngameAttackInfo OnAttack(IngameAttackInfo attInfo)
    {
        if (isTriple)
        {
            attInfo.damage += Mathf.RoundToInt(attInfo.damage * addDamageRatio);
        }
        return attInfo;
    }
}
public class TowerAdditionalEffect_Ability_CriticalPerc : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.Ability_CriticalPerc; } }

    public float addPerc { get; private set; }
    public float addPerc_Triple { get; private set; }
    public int addPerc100_Triple { get; private set; }

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        addPerc = float.Parse(datas[1]);
        addPerc_Triple = float.Parse(datas[2]);
        addPerc100_Triple = Mathf.RoundToInt(addPerc_Triple * 100);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        if (isTriple)
        {
            tower.OnAttack += OnAttack;
        }

        if (addPerc > 0)
        {
            tower.statusController.AddFixedValue(IngameGrid.EAbilityType.CriticalPerc, addPerc);
        }
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        if (isTriple)
        {
            tower.OnAttack -= OnAttack;
        }

        if (addPerc > 0)
        {
            tower.statusController.AddFixedValue(IngameGrid.EAbilityType.CriticalPerc, -addPerc);
        }
    }

    private IngameAttackInfo OnAttack(IngameAttackInfo attInfo)
    {
        if (UnityEngine.Random.Range(0, 100) < addPerc100_Triple)
        {
            attInfo.AddAdditionalEffect(OnApplyAdditionalEffect, effectType);
        }
        return attInfo;
    }

    private void OnApplyAdditionalEffect(IngameObject ingameObj, IngameAttackInfo attInfo)
    {
        TowerProjectile proj;
        if (!SpawnMaster.TrySpawnMonoBehaviour("TD-PROJ-4", owner.projectileSpawnPointTf.position, Quaternion.identity, out proj))
        {
            Debug.LogErrorFormat("{0} :: Can't spawned TowerProjectile({1})", GetType(), owner.data.projectileKey);
            return;
        }

        // 무한 반복되는 현상을 없애기 위한 처리
        IngameAttackInfo newAttInfo = new IngameAttackInfo(attInfo);
        newAttInfo.target = ingameObj;
        newAttInfo.transitionCount = 0;
        newAttInfo.isCritical = false;
        newAttInfo.SubAdditionalEffect(OnApplyAdditionalEffect, effectType);
        //newAttInfo.OnApplyAdditionalEffect = null;

        proj.SetData(new TowerProjectileData(owner, newAttInfo));
        proj.Fire();
    }
}
public class TowerAdditionalEffect_Ability_CriticalDamage : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.Ability_CriticalDamage; } }

    public float addPerc;
    public float addPerc_Triple { get; private set; }

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        addPerc = float.Parse(datas[1]);
        addPerc_Triple = float.Parse(datas[2]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        if (addPerc > 0)
        {
            tower.statusController.AddFixedValue(IngameGrid.EAbilityType.CriticalDamage, (isTriple) ? addPerc_Triple : addPerc);
        }
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        if (addPerc > 0)
        {
            tower.statusController.AddFixedValue(IngameGrid.EAbilityType.CriticalDamage, (isTriple) ? -addPerc_Triple : -addPerc);
        }
    }
}
#endregion

#region < Penetrate >
public class TowerAdditionalEffect_Penetrate : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.Penetrate; } }

    public int penetration;
    public float addDamageRatio;
    
    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        penetration = int.Parse(datas[1]);
        addDamageRatio = float.Parse(datas[2]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        tower.OnAttack += OnAttacked;
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        tower.OnAttack -= OnAttacked;
    }

    public IngameAttackInfo OnAttacked(IngameAttackInfo attInfo)
    {
        attInfo.penetration = penetration;

        if(isTriple)
        {
            attInfo.damage += Mathf.RoundToInt(attInfo.damage * addDamageRatio);
        }

        return attInfo;
    }
}
#endregion

#region < Explosion >
public class TowerAdditionalEffect_Explosion : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.Explosion; } }

    public float range { get; private set; }
    public float dmgPerc { get; private set; }
    
    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        range = float.Parse(datas[1]);
        dmgPerc = float.Parse(datas[2]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        tower.OnAttack += OnAttacked;
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        tower.OnAttack -= OnAttacked;
    }

    public virtual IngameAttackInfo OnAttacked(IngameAttackInfo attInfo)
    {
        attInfo.splashRange = Mathf.Max(attInfo.splashRange, range);
        attInfo.splashDmgRatio = Mathf.Max(attInfo.splashDmgRatio, dmgPerc);
        return attInfo;
    }
}
public sealed class TowerAdditionalEffect_Explosion_S1 : TowerAdditionalEffect_Explosion
{
    public override EType effectType { get { return EType.Explosion; } }

    public float fireDmgRatio { get; private set; }
    public float fireDmgDuration { get; private set; }

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        fireDmgRatio = float.Parse(datas[3]);
        fireDmgDuration = float.Parse(datas[4]);
    }

    public override IngameAttackInfo OnAttacked(IngameAttackInfo attInfo)
    {
        if (isTriple)
        {
            attInfo.AddAdditionalEffect(OnApplyAdditionalEffect, effectType);
            attInfo.OnAddSplashDamage += OnAddSplashDamage;
        }
        return base.OnAttacked(attInfo);
    }

    private IngameAttackInfo OnAddSplashDamage(IngameAttackInfo attInfo)
    {
        attInfo.AddAdditionalEffect(OnApplyAdditionalEffect, effectType);
        return attInfo;
    }

    private void OnApplyAdditionalEffect(IngameObject ingameObj, IngameAttackInfo attInfo)
    {
        ingameObj.statusController.ApplyStatusEffect(new SE_Burning() { id = "Burning", duration = fireDmgDuration, damagePerSeconds = attInfo.damage * fireDmgRatio });
    }
}
public sealed class TowerAdditionalEffect_Explosion_S2 : TowerAdditionalEffect_Explosion
{
    public override EType effectType { get { return EType.Explosion; } }

    public float range_Double { get; private set; }
    public float dmgPerc_Double { get; private set; }

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        range_Double = float.Parse(datas[3]);
        dmgPerc_Double = float.Parse(datas[4]);
    }

    public override IngameAttackInfo OnAttacked(IngameAttackInfo attInfo)
    {

        if (isDouble)
        {
            attInfo.splashRange = Mathf.Max(attInfo.splashRange, range_Double);
            attInfo.splashDmgRatio = Mathf.Max(attInfo.splashDmgRatio, dmgPerc_Double);
            return attInfo;
        }
        else
        {
            return base.OnAttacked(attInfo);
        }
    }
}
#endregion

#region < Freezing >
public class TowerAdditionalEffect_Freezing : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.Freezing; } }

    public float slowPerc;
    public float duration;
    
    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        slowPerc = float.Parse(datas[1]);
        duration = float.Parse(datas[2]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        tower.OnAttack += OnAttacked;
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        tower.OnAttack -= OnAttacked;
    }

    public IngameAttackInfo OnAttacked(IngameAttackInfo attInfo)
    {
        attInfo.AddAdditionalEffect(OnApplyAdditionalEffect, effectType);
        return attInfo;
    }

    protected virtual void OnApplyAdditionalEffect(IngameObject ingameObj, IngameAttackInfo attInfo)
    {
        ingameObj.statusController.ApplyStatusEffect(new IngameObject_.SE_Freezing() { id = "Freezing", duration = duration, freezingPower = slowPerc });
    }
}
public sealed class TowerAdditionalEffect_Freezing_S1 : TowerAdditionalEffect_Freezing
{
    public override EType effectType { get { return EType.Freezing; } }

    public float fixedDotDmg;
    public float fixedDotDmgDuration;
    
    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        fixedDotDmg = float.Parse(datas[3]);
        fixedDotDmgDuration = float.Parse(datas[4]);
    }

    protected override void OnApplyAdditionalEffect(IngameObject ingameObj, IngameAttackInfo attInfo)
    {
        base.OnApplyAdditionalEffect(ingameObj, attInfo);

        if(isTriple)
        {
            ingameObj.statusController.ApplyStatusEffect(new SE_Frostbite() { id = "Frostbite", duration = duration, damagePerSeconds = fixedDotDmg });
        }
    }
}
public sealed class TowerAdditionalEffect_Freezing_S2 : TowerAdditionalEffect_Freezing
{
    public override EType effectType { get { return EType.Freezing; } }

    public int needCount;
    public float stunDuration;
    
    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        needCount = int.Parse(datas[3]);
        stunDuration = float.Parse(datas[4]);
    }
    
    protected override void OnApplyAdditionalEffect(IngameObject ingameObj, IngameAttackInfo attInfo)
    {
        base.OnApplyAdditionalEffect(ingameObj, attInfo);

        if (isDouble)
        {
            ingameObj.statusController.ApplyStatusEffect(new SE_Freezing_Stun() { id = "Freezing_Stun", duration = duration, stunDuration = 1.5f, needAppliedCount = 5 });
        }
    }
}
#endregion

#region < Poison >
public class TowerAdditionalEffect_Poison : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.Poison; } }

    public float duration;
    public float damagePercPerSeconds;
    
    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        duration = int.Parse(datas[1]);
        damagePercPerSeconds = float.Parse(datas[2]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        tower.OnAttack += OnAttacked;
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        tower.OnAttack -= OnAttacked;
    }

    public virtual IngameAttackInfo OnAttacked(IngameAttackInfo attInfo)
    {
        attInfo.AddAdditionalEffect(OnApplyAdditionalEffect, effectType);
        return attInfo;
    }

    protected virtual void OnApplyAdditionalEffect(IngameObject ingameObj, IngameAttackInfo attInfo)
    {
        ingameObj.statusController.ApplyStatusEffect(new SE_Poison() { id = "Poison", duration = duration, damagePerSeconds = Mathf.RoundToInt(attInfo.damage * damagePercPerSeconds) });
    }
}
public class TowerAdditionalEffect_Poison_S1 : TowerAdditionalEffect_Poison
{
    public override EType effectType { get { return EType.Poison; } }

    public float deathExplosionRange;
    
    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        deathExplosionRange = float.Parse(datas[3]);
    }

    protected override void OnApplyAdditionalEffect(IngameObject ingameObj, IngameAttackInfo attInfo)
    {
        base.OnApplyAdditionalEffect(ingameObj, attInfo);

        if (isTriple)
        {
            ingameObj.statusController.ApplyStatusEffect(new SE_Poison_Explosion() { id = "Poison_Explosion", duration = duration, damagePerSeconds = Mathf.RoundToInt(attInfo.damage * damagePercPerSeconds), splashRange = deathExplosionRange });
        }
    }
}
public class TowerAdditionalEffect_Poison_S2 : TowerAdditionalEffect_Poison
{
    public override EType effectType { get { return EType.Poison; } }
    
    public override void SetData(string[] datas)
    {
        base.SetData(datas);
    }

    protected override void OnApplyAdditionalEffect(IngameObject ingameObj, IngameAttackInfo attInfo)
    {
        ingameObj.statusController.ApplyStatusEffect(new SE_Poison()
            { id = "Poison"
            , duration = duration
            , damagePerSeconds = Mathf.RoundToInt(attInfo.damage * damagePercPerSeconds)
            , penetration = attInfo.penetration });
    }

    public override IngameAttackInfo OnAttacked(IngameAttackInfo attInfo)
    {
        if(isDouble)
        {
           attInfo.penetration = 100;
        }
        return base.OnAttacked(attInfo);
    }

    //private IngameAttackInfo OnAttack(IngameAttackInfo attInfo)
    //{
    //    if (isTriple)
    //    {
    //        attInfo.addMaxHpProportionalDmg += addMaxHpProportionalDmg;
    //    }
    //    return attInfo;
    //}
}
#endregion

#region < ElectricShock >
public class TowerAdditionalEffect_ElectricShock : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.ElectricShock; } }

    public float perc100;
    public float perc;
    public float duration;
    
    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        perc = float.Parse(datas[1]);
        perc100 = perc * 100;
        duration = float.Parse(datas[2]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        tower.OnAttack += OnAttack;
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        tower.OnAttack -= OnAttack;
    }

    protected virtual IngameAttackInfo OnAttack(IngameAttackInfo attInfo)
    {
        if(UnityEngine.Random.Range(0, 100) < perc100)
        {
            attInfo.AddAdditionalEffect(OnApplyAdditionalEffect, effectType);
        }
        return attInfo;
    }

    protected virtual void OnApplyAdditionalEffect(IngameObject ingameObj, IngameAttackInfo attInfo)
    {
        ingameObj.statusController.ApplyStatusEffect(new IngameObject_.SE_ElectricShock() { id = "Electric", duration = duration });
    }
}
public sealed class TowerAdditionalEffect_ElectricShock_S1 : TowerAdditionalEffect_ElectricShock
{
    public override EType effectType { get { return EType.ElectricShock; } }
    
    public int transitionCount { get; private set; }
    public float transitionRange { get; private set; }
    
    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        transitionCount = int.Parse(datas[3]);
        transitionRange = float.Parse(datas[4]);
    }

    protected override IngameAttackInfo OnAttack(IngameAttackInfo attInfo)
    {
        if (UnityEngine.Random.Range(0, 100) < perc100)
        {
            attInfo.transitionCount += transitionCount;
            attInfo.AddAdditionalEffect(OnApplyAdditionalEffect, effectType);
        }
        return attInfo;
    }

    //protected override void OnApplyAdditionalEffect(IngameObject ingameObj, IngameAttackInfo attInfo)
    //{
    //    if(isTriple)
    //    {
    //        List<string> newExcludeTransitionTargets = new List<string>();
    //        foreach (var name in attInfo.excludeTransitionTargets)
    //        {
    //            newExcludeTransitionTargets.Add(name);
    //        }
    //        newExcludeTransitionTargets.Add(ingameObj.name);

    //        List<IngameObject> targets;
    //        if (Helper.Ingame.FindNearestTargets(ingameObj.gameObject, newExcludeTransitionTargets, transitionCount, 0.8f, out targets))
    //        {
    //            foreach (var t in targets)
    //            {
    //                TowerProjectile proj;
    //                if (!SpawnMaster.TrySpawnMonoBehaviour("TD-PROJ-5", ingameObj.transform.position, Quaternion.identity, out proj))
    //                {
    //                    Debug.LogErrorFormat("{0} :: Can't spawned TowerProjectile({1})", GetType(), "TD-PROJ-5");
    //                    continue;
    //                }

    //                for(int i = 0; i< targets.Count; ++i)
    //                {
    //                    newExcludeTransitionTargets.Add(targets[i].name);
    //                }

    //                var transitionAttackInfo = new IngameAttackInfo()
    //                {
    //                    attacker = attInfo.attacker,
    //                    target = t,
    //                    damage = 0,
    //                    damageType = EDamageType.None,
    //                    excludeTransitionTargets = newExcludeTransitionTargets,
    //                    transitionCount = transitionCount,
    //                    transitionDmgRatio = 0,
    //                };
    //                transitionAttackInfo.AddAdditionalEffect(base.OnApplyAdditionalEffect, effectType);

    //                proj.SetData(new TowerProjectileData(attInfo.attacker, transitionAttackInfo));
    //                proj.Fire();
    //            }
    //        }
    //    }
    //    else
    //    {
    //        base.OnApplyAdditionalEffect(ingameObj, attInfo);
    //    }
    //}
}
public sealed class TowerAdditionalEffect_ElectricShock_S2 : TowerAdditionalEffect_ElectricShock
{
    public override EType effectType { get { return EType.ElectricShock; } }
    
    public float addDamageRatio { get; private set; }
    
    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        addDamageRatio = float.Parse(datas[3]);
    }

    protected override IngameAttackInfo OnAttack(IngameAttackInfo attInfo)
    {
        if (UnityEngine.Random.Range(0, 100) < perc100)
        {
            if(isDouble)
            {
                attInfo.damage += Mathf.RoundToInt(attInfo.damage * addDamageRatio);
            }
            attInfo.AddAdditionalEffect(OnApplyAdditionalEffect, effectType);
        }
        return attInfo;
    }

    protected override void OnApplyAdditionalEffect(IngameObject ingameObj, IngameAttackInfo attInfo)
    {
        base.OnApplyAdditionalEffect(ingameObj, attInfo);
        
        Fx_OneTime fx;
        if(SpawnMaster.TrySpawnFx("Fx_4_Hit", ingameObj.transform.position, Quaternion.identity, out fx))
        {
            fx.On();
        }
    }
}
#endregion

#region < Transition >
public class TowerAdditionalEffect_Transition : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.Transition; } }

    public float range { get; private set; }
    public int targetCount { get; private set; }
    public float damagePerc { get; private set; }

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        range = float.Parse(datas[1]);
        targetCount = int.Parse(datas[2]);
        damagePerc = float.Parse(datas[3]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        tower.OnAttack += OnAttack;
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        tower.OnAttack -= OnAttack;
    }

    protected virtual IngameAttackInfo OnAttack(IngameAttackInfo attInfo)
    {
        attInfo.transitionCount += targetCount;
        attInfo.transitionDmgRatio = Mathf.Max(attInfo.transitionDmgRatio, damagePerc);
        
        return attInfo;
    }
}
public sealed class TowerAdditionalEffect_Transition_S1 : TowerAdditionalEffect_Transition
{
    public override EType effectType { get { return EType.Transition; } }

    public int addTargetCount { get; private set; }

    public override void SetData(string[] datas)
    {
        base.SetData(datas);
        
        addTargetCount = int.Parse(datas[4]);
    }

    protected override IngameAttackInfo OnAttack(IngameAttackInfo attInfo)
    {
        if (isDouble) attInfo.transitionCount += addTargetCount;

        return base.OnAttack(attInfo);
    }
}
public sealed class TowerAdditionalEffect_Transition_S2 : TowerAdditionalEffect_Transition
{
    public override EType effectType { get { return EType.Transition; } }

    public float splashRange { get; private set; }
    public float splashDmgRatio { get; private set; }

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        splashRange = float.Parse(datas[4]);
        splashDmgRatio = float.Parse(datas[5]);
    }

    protected override IngameAttackInfo OnAttack(IngameAttackInfo attInfo)
    {
        if (isTriple)
        {
            attInfo.splashRange = splashRange;
            attInfo.splashDmgRatio = splashDmgRatio;
        }

        return base.OnAttack(attInfo);
    }
}
#endregion 

#region < MultyShot & DoubleShot >
public class TowerAdditionalEffect_MultyShot : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.MultyShot; } }

    public int addProjectileCount { get; private set; }
    public int addProjectileCount_Double { get; private set; }
    public int addProjectileCount_Triple { get; private set; }
    public bool isAttackToAllInRange { get; private set; }

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        addProjectileCount = int.Parse(datas[1]);
        addProjectileCount_Double = int.Parse(datas[2]);
        addProjectileCount_Triple = int.Parse(datas[3]);
        isAttackToAllInRange = bool.Parse(datas[4]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        if (addProjectileCount > 0)
        {
            if (isTriple)
            {
                tower.statusController.AddFixedValue(IngameGrid.EAbilityType.ProjectileCount, addProjectileCount_Triple);
            }
            else if (isDouble)
            {
                tower.statusController.AddFixedValue(IngameGrid.EAbilityType.ProjectileCount, addProjectileCount_Double);
            }
            else
            {
                tower.statusController.AddFixedValue(IngameGrid.EAbilityType.ProjectileCount, addProjectileCount);
            }
        }
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        if (addProjectileCount > 0)
        {
            if(isTriple)
            {
                tower.statusController.AddFixedValue(IngameGrid.EAbilityType.ProjectileCount, -addProjectileCount_Triple);
            }
            else if(isDouble)
            {
                tower.statusController.AddFixedValue(IngameGrid.EAbilityType.ProjectileCount, -addProjectileCount_Double);
            }
            else
            {
                tower.statusController.AddFixedValue(IngameGrid.EAbilityType.ProjectileCount, -addProjectileCount);
            }
        }
    }
}
public class TowerAdditionalEffect_DoubleShot : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.DoubleShot; } }

    public int addAttackCount;
    public int addAttackCount_Quadruple;

    //public TowerAdditionalEffect_DoubleShot(int cost, int addAttackCount = 1) : base(cost)
    //{
    //    this.addAttackCount = addAttackCount;
    //}

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        addAttackCount = int.Parse(datas[1]);
        addAttackCount_Quadruple = int.Parse(datas[2]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        if (addAttackCount > 0)
        {
            tower.statusController.AddFixedValue(IngameGrid.EAbilityType.AttackCount, (isQuadruple) ? addAttackCount_Quadruple : addAttackCount);
        }
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        if (addAttackCount > 0)
        {
            tower.statusController.AddFixedValue(IngameGrid.EAbilityType.AttackCount, (isQuadruple) ? addAttackCount_Quadruple : addAttackCount);
        }
    }
}
#endregion

#region < Berserker >
public class TowerAdditionalEffect_Berserker : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.Berserker; } }
    
    public int activationPerc;
    public float addSpeedRatio;
    public float addSpeedDuration;
    public int limitedCount;

    public int curIdx;
    
    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        activationPerc = Mathf.RoundToInt(float.Parse(datas[1]) * 100);
        addSpeedRatio = float.Parse(datas[2]);
        addSpeedDuration = float.Parse(datas[3]);
        limitedCount = int.Parse(datas[4]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        tower.OnAttack += OnAttack;
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        tower.OnAttack -= OnAttack;
    }

    public virtual IngameAttackInfo OnAttack(IngameAttackInfo info)
    {
        if(UnityEngine.Random.Range(0, 100) < activationPerc)
        {
            owner.statusController.ApplyStatusEffect(new SE_Berserker() { id = "Berserker_" + curIdx, duration = addSpeedDuration, addAttackSpeedRatio = addSpeedRatio });
            curIdx = ++curIdx % limitedCount;
        }
        return info;
    }
}
public class TowerAdditionalEffect_Berserker_S1 : TowerAdditionalEffect_Berserker
{
    public override EType effectType { get { return EType.Berserker; } }

    public float addCriticalPerc { get; private set; }
    
    public override void SetData(string[] datas)
    {
        base.SetData(datas);
        
        addCriticalPerc = float.Parse(datas[5]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        base.OnRegist(tower);

        if(isDouble && addCriticalPerc > 0) tower.statusController.AddFixedValue(IngameGrid.EAbilityType.CriticalPerc, addCriticalPerc);
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        base.OnUnregsit(tower);

        if (isDouble && addCriticalPerc > 0) tower.statusController.AddFixedValue(IngameGrid.EAbilityType.CriticalPerc, -addCriticalPerc);
    }
}
public class TowerAdditionalEffect_Berserker_S2 : TowerAdditionalEffect_Berserker
{
    public override EType effectType { get { return EType.Berserker; } }
    
    public override void SetData(string[] datas)
    {
        base.SetData(datas);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        base.OnRegist(tower);

        if(isTriple) owner.statusController.AddFixedValue(IngameGrid.EAbilityType.AttackCount, 1);
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        base.OnUnregsit(tower);

        if (isTriple) owner.statusController.AddFixedValue(IngameGrid.EAbilityType.AttackCount, -1);
    }
}
public class TowerAdditionalEffect_Berserker_S3 : TowerAdditionalEffect_Berserker
{
    public override EType effectType { get { return EType.Berserker; } }

    public float addCritcalDmgPerc;

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        addCritcalDmgPerc = float.Parse(datas[5]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        base.OnRegist(tower);

        if (isDouble && addCritcalDmgPerc > 0) tower.statusController.AddFixedValue(IngameGrid.EAbilityType.CriticalDamage, addCritcalDmgPerc);
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        base.OnUnregsit(tower);

        if (isDouble && addCritcalDmgPerc > 0) tower.statusController.AddFixedValue(IngameGrid.EAbilityType.CriticalDamage, -addCritcalDmgPerc);
    }
}
#endregion

#region < Laser >
public class TowerAdditionalEffect_Laser : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.Laser; } }

    public float dmgRatio { get; private set; }
    public float laserWidthRatio { get; private set; }
    public float dmgRatio_Triple { get; private set; }
    public int useTripleEffect { get; private set; }

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        dmgRatio = float.Parse(datas[1]);
        laserWidthRatio = float.Parse(datas[2]);
        dmgRatio_Triple = float.Parse(datas[3]);
        useTripleEffect = int.Parse(datas[4]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        if(useTripleEffect == 0)
        {
            owner.statusController.ApplyStatusEffect(new SE_Laser()
            {
                id = "Laser"
                , duration = float.PositiveInfinity
                , dmgRatio = dmgRatio
                , laserWidthRatio = 1
                , isOnlyCritical = true
            });
        }
        else
        {
            owner.statusController.ApplyStatusEffect(new SE_Laser()
            {
                id = "Laser"
                , duration = float.PositiveInfinity
                , dmgRatio = (isTriple) ? dmgRatio_Triple : dmgRatio
                , laserWidthRatio = (isTriple) ? laserWidthRatio : 1
                , isOnlyCritical = !isTriple
            });
        }

        //tower.OnAttack += OnAttack;
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        owner.statusController.ApplyStatusEffect(new SE_Laser()
        {
            id = "Laser"
            , duration = 0
            , dmgRatio = dmgRatio
            , laserWidthRatio = 1
            , isOnlyCritical = false
        });
        //tower.OnAttack -= OnAttack;
    }

    private IngameAttackInfo OnAttack(IngameAttackInfo attInfo)
    {
        if (attInfo.isCritical)
        {
            attInfo.AddAdditionalEffect(OnApplyAdditionalEffect, effectType);
        }
        return attInfo;
    }

    private void OnApplyAdditionalEffect(IngameObject ingameObj, IngameAttackInfo attInfo)
    {
        TowerProjectile proj;
        if (!SpawnMaster.TrySpawnMonoBehaviour("TD-PROJ-4", owner.projectileSpawnPointTf.position, Quaternion.identity, out proj))
        {
            Debug.LogErrorFormat("{0} :: Can't spawned TowerProjectile({1})", GetType(), owner.data.projectileKey);
            return;
        }

        var newAttInfo = new IngameAttackInfo(attInfo);
        newAttInfo.isCritical = false;

        if (isTriple)
        {
            (proj as TowerProjectile_Laser).maxWidthRatio = laserWidthRatio;
        }
        else
        {
            (proj as TowerProjectile_Laser).maxWidthRatio = 1f;
        }

        newAttInfo.target = ingameObj;
        proj.SetData(new TowerProjectileData(owner, newAttInfo));
        proj.Fire();
    }
}
#endregion

#region < Crush >
public class TowerAdditionalEffect_Crush : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.Crush; } }

    public float duration;
    public float decreaseRatio;
    public float decreaseRatio_Triple;

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        duration = float.Parse(datas[1]);
        decreaseRatio = float.Parse(datas[2]);
        decreaseRatio_Triple = float.Parse(datas[3]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        tower.OnAttack += OnAttacked;
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        tower.OnAttack -= OnAttacked;
    }

    public IngameAttackInfo OnAttacked(IngameAttackInfo attInfo)
    {
        attInfo.AddAdditionalEffect(OnApplyAdditionalEffect, effectType);
        return attInfo;
    }

    protected virtual void OnApplyAdditionalEffect(IngameObject ingameObj, IngameAttackInfo attInfo)
    {
        Fx_OneTime fx;
        if(SpawnMaster.TrySpawnFx("Fx_TAE_Crush_Hit", ingameObj.transform.position, Quaternion.identity, out fx))
        {
            fx.On();
        }

        if (isTriple)
        {
            ingameObj.statusController.ApplyStatusEffect(new SE_Crush() { id = "Crush", duration = float.PositiveInfinity, crushPower = -decreaseRatio_Triple });
        }
        else
        {
            ingameObj.statusController.ApplyStatusEffect(new SE_Crush() { id = "Crush", duration = duration, crushPower = -decreaseRatio });
        }
    }
}
#endregion

#region < DeathBlow >
public class TowerAdditionalEffect_DeathBlow : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.DeathBlow; } }

    public float timer { get; private set; }
    public float dmgRatioPerMaxHp { get; private set; }
    public float cooltime { get; private set; }

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        dmgRatioPerMaxHp = float.Parse(datas[1]);
        cooltime = float.Parse(datas[2]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        tower.OnAttack += OnAttack;
        tower.OnUpdate += OnUpdate;
        timer = 0;
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        tower.OnAttack -= OnAttack;
        tower.OnUpdate -= OnUpdate;
    }

    private void OnUpdate(float deltaTime)
    {
        timer += deltaTime;
    }

    private IngameAttackInfo OnAttack(IngameAttackInfo attInfo)
    {
        if (attInfo.isCritical && timer >= cooltime)
        {
            timer -= cooltime;

            Fx_OneTime fx;
            if(SpawnMaster.TrySpawnFx("Fx_TAE_DeathBlow_Shot", owner.transform.position, Quaternion.identity, out fx))
            {
                fx.On();
            }

            attInfo.addMaxHpProportionalDmg += dmgRatioPerMaxHp;
        }
        return attInfo;
    }

    private void OnApplyAdditionalEffect(IngameObject ingameObj, IngameAttackInfo attInfo)
    {
        TowerProjectile proj;
        if (!SpawnMaster.TrySpawnMonoBehaviour("TD-PROJ-4", owner.projectileSpawnPointTf.position, Quaternion.identity, out proj))
        {
            Debug.LogErrorFormat("{0} :: Can't spawned TowerProjectile({1})", GetType(), owner.data.projectileKey);
            return;
        }

        attInfo.target = ingameObj;
        proj.SetData(new TowerProjectileData(owner, attInfo));
        proj.Fire();
    }
}
#endregion

#region < GrowingPains >
public class TowerAdditionalEffect_GrowingPains : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.GrowingPains; } }

    public float decreaseStatusRatio { get; private set; }
    public float increaseStatusRatio { get; private set; }

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        decreaseStatusRatio = float.Parse(datas[1]);
        increaseStatusRatio = float.Parse(datas[2]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        if (increaseStatusRatio > 0)
        {
            tower.OnAddStatusRatioPerLevel += OnAddStatusRatioPerLevel;
            tower.statusController.AddPercentValue(IngameGrid.EAbilityType.Damage, -decreaseStatusRatio);
            tower.statusController.AddPercentValue(IngameGrid.EAbilityType.AttackSpeed, -decreaseStatusRatio);
        }
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        if (increaseStatusRatio > 0)
        {
            tower.OnAddStatusRatioPerLevel -= OnAddStatusRatioPerLevel;
            tower.statusController.AddPercentValue(IngameGrid.EAbilityType.Damage, decreaseStatusRatio);
            tower.statusController.AddPercentValue(IngameGrid.EAbilityType.AttackSpeed, decreaseStatusRatio);
        }
    }

    private float OnAddStatusRatioPerLevel(float ratio)
    {
        return ratio + increaseStatusRatio;
    }
}
#endregion

#region < Insight >
public class TowerAdditionalEffect_Insight : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.Insight; } }

    public float addExpPointRatio { get; private set; }
    public int addMaxLevel { get; private set; }

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        addExpPointRatio = float.Parse(datas[1]);
        addMaxLevel = int.Parse(datas[2]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        if (isTriple && addMaxLevel > 0)
        {
            tower.OnAddTowerMaxLevel += OnAddTowerMaxLevel;
        }

        if (addExpPointRatio > 0)
        {
            tower.OnAddExpPoint += OnAddExpPoint;
        }
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        if (isTriple && addMaxLevel > 0)
        {
            tower.OnAddTowerMaxLevel -= OnAddTowerMaxLevel;
        }

        if (addExpPointRatio > 0)
        {
            tower.OnAddExpPoint -= OnAddExpPoint;
        }
    }

    private int OnAddExpPoint(int expPoint)
    {
        return expPoint + Mathf.RoundToInt(expPoint * addExpPointRatio);
    }

    private int OnAddTowerMaxLevel(int maxLevel)
    {
        return maxLevel + addMaxLevel;
    }
}
#endregion

#region < ChemicalReaction >
public class TowerAdditionalEffect_ChemicalReaction : TowerAdditionalEffect
{
    public override EType effectType { get { return EType.ChemicalReaction; } }

    // 공통 프로퍼티
    public float splashRange { get; private set; }

    // 독 상태
    public float splashDamageRatio { get; private set; }

    // 빙결 상태
    public float freezingDuration { get; private set; }
    public float freezingPower { get; private set; }

    // 감전 상태
    public float crushDuration { get; private set; }
    public float crushPower { get; private set; }

    public override void SetData(string[] datas)
    {
        base.SetData(datas);

        // { "400", "0.4", "0.05", "3", "-0.3", "3", "0.3" }

        splashRange = float.Parse(datas[1]);
        splashDamageRatio = float.Parse(datas[2]);

        freezingDuration = float.Parse(datas[3]);
        freezingPower = float.Parse(datas[4]);

        crushDuration = float.Parse(datas[5]);
        crushPower = float.Parse(datas[6]);
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        tower.OnAttack += OnAttack;
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        tower.OnAttack -= OnAttack;
    }
    
    protected virtual IngameAttackInfo OnAttack(IngameAttackInfo attInfo)
    {
        attInfo.AddAdditionalEffect(OnApplyAdditionalEffect, effectType);
        return attInfo;
    }

    protected virtual void OnApplyAdditionalEffect(IngameObject ingameObj, IngameAttackInfo attInfo)
    {
        if(ingameObj.statusController.CheckAppliedStatusEffect(IngameObject_.StatusEffect.EType.Poison))
        {
            Fx_OneTime fx;
            if (SpawnMaster.TrySpawnFx("Fx_20_Hit_1", ingameObj.transform.position, Quaternion.identity, out fx))
            {
                fx.On();
            }

            var newAttInfo = new IngameAttackInfo();
            newAttInfo.attacker = attInfo.attacker;
            newAttInfo.damage = Mathf.RoundToInt(attInfo.damage * splashDamageRatio);
            newAttInfo.damageType = EDamageType.None;

            List<IngameObject> splashTargets;
            if (Helper.Ingame.FindAllTargets(ingameObj.gameObject, ingameObj, splashRange, out splashTargets))
            {
                foreach (var t in splashTargets)
                {
                    var newAttInfo2 = new IngameAttackInfo(newAttInfo);
                    newAttInfo2.target = t;

                    t.Attacked(newAttInfo2);
                }
            }
        }

        if (ingameObj.statusController.CheckAppliedStatusEffect(IngameObject_.StatusEffect.EType.Freezing))
        {
            Fx_OneTime fx;
            if (SpawnMaster.TrySpawnFx("Fx_20_Hit_2", ingameObj.transform.position, Quaternion.identity, out fx))
            {
                fx.On();
            }
            
            var newAttInfo = new IngameAttackInfo();
            newAttInfo.attacker = attInfo.attacker;
            newAttInfo.damage = attInfo.damage;
            newAttInfo.damageType = EDamageType.None;

            List<IngameObject> splashTargets;
            if (Helper.Ingame.FindAllTargets(ingameObj.gameObject, ingameObj, splashRange, out splashTargets))
            {
                foreach (var t in splashTargets)
                {
                    var newAttInfo2 = new IngameAttackInfo(newAttInfo);
                    newAttInfo2.target = t;

                    t.Attacked(newAttInfo2);
                    t.statusController.ApplyStatusEffect(new IngameObject_.SE_Freezing() { id = "Freezing", duration = freezingDuration, freezingPower = freezingPower });
                }
            }
        }

        if (ingameObj.statusController.CheckAppliedStatusEffect(IngameObject_.StatusEffect.EType.ElectricShock))
        {
            Fx_OneTime fx;
            if (SpawnMaster.TrySpawnFx("Fx_20_Hit_3", ingameObj.transform.position, Quaternion.identity, out fx))
            {
                fx.On();
            }
            
            var newAttInfo = new IngameAttackInfo();
            newAttInfo.attacker = attInfo.attacker;
            newAttInfo.damage = attInfo.damage;
            newAttInfo.damageType = EDamageType.None;

            List<IngameObject> splashTargets;
            if (Helper.Ingame.FindAllTargets(ingameObj.gameObject, ingameObj, splashRange, out splashTargets))
            {
                foreach (var t in splashTargets)
                {
                    var newAttInfo2 = new IngameAttackInfo(newAttInfo);
                    newAttInfo2.target = t;

                    t.Attacked(newAttInfo2);
                    t.statusController.ApplyStatusEffect(new SE_Crush() { id = "Crush", duration = crushDuration, crushPower = crushPower });
                }
            }
        }
    }
}
#endregion

public class TowerAdditionalEffect_AuraOfWind : TowerAdditionalEffect
{
    public override EType effectType => EType.AuraOfWind;
    
    public float attackSpeedIncRatio { get; private set; }
    private float timer;

    private Fx_Switch m_Fx;
    public Fx_Switch fx => m_Fx;

    public override void SetData(string[] datas)
    {
        base.SetData(datas);
        
        attackSpeedIncRatio = float.Parse(datas[1]);
        timer = 1f;
    }

    public override void OnRegist(GridBlock_Tower tower)
    {
        owner.OnUpdate += OnUpdate;

        if(SpawnMaster.TrySpawnFx("Fx_TAE_AuraOfWind_Cast", owner.transform.position, Quaternion.identity, out m_Fx))
        {
            var range = owner.attackRange;
            fx.On();
            fx.transform.localScale = new Vector3(range, range, 1);
        }
    }

    public override void OnUnregsit(GridBlock_Tower tower)
    {
        if(fx != null)
        {
            fx.DestroySelf();
            m_Fx = null;
        }

        owner.OnUpdate -= OnUpdate;
    }

    public void OnUpdate(float deltaTime)
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            timer = 1;
            
            List<GridBlock_Tower> targets;
            if (IngameObject_.SkillComponent.TryFindAllTowerAtRangeFromPos(owner.transform.position, owner.attackRange, out targets))
            {
                for(int i = 0; i<targets.Count; ++i)
                {
                    // 스킬을 사용한 타워아 다른 경우에만 적용
                    if(!targets[i].name.Equals(owner.name))
                    {
                        targets[i].statusController.ApplyStatusEffect(new IngameGrid.SE_AuraOfWind() { id = "AuraOfWind", duration = 1f, addAttackSpeedRatio = attackSpeedIncRatio });
                    }
                }
            }
        }
    }
}