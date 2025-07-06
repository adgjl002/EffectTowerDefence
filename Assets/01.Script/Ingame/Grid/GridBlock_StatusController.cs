using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IngameGrid
{
    public enum EAbilityType
    {
        None = 0,

        Fixed = 1,              // 0000 0001
        Percent = 2,            // 0000 0010

        MoveSpeed = 4,          // 0000 0100
        AttackSpeed = 8,        // 0000 1000
        Damage = 16,            // 0001 0000
        MaxHp = 32,             // 0010 0000
        AttackRange = 64,       // 0100 0000
        CriticalDamage = 128,   // 1000 0000
        CriticalPerc = 256,     // 0001 0000 0000
        ProjectileCount = 512,  // 0010 0000 0000
        AttackCount = 1024,     // 0100 0000 0000
    }

    public class StatusController
    {
        public struct removeInfo
        {
            public StatusEffect.EType seType;
            public string seId;
        }

        private Dictionary<EAbilityType, float> abilitys_Fixed;
        private Dictionary<EAbilityType, float> abilitys_Percent;

        private Dictionary<StatusEffect.EType, Dictionary<string, StatusEffect>> statusEffects;
        private List<removeInfo> removeInfos;

        public GridBlock_Tower owner;

        public void Initialize(GridBlock_Tower owner)
        {
            abilitys_Fixed = new Dictionary<EAbilityType, float>();
            abilitys_Percent = new Dictionary<EAbilityType, float>();

            statusEffects = new Dictionary<StatusEffect.EType, Dictionary<string, StatusEffect>>();
            removeInfos = new List<removeInfo>();

            this.owner = owner;
        }

        public void AddFixedValue(EAbilityType abilityType, float value)
        {
            if (abilitys_Fixed.ContainsKey(abilityType))
            {
                abilitys_Fixed[abilityType] += value;
            }
            else
            {
                abilitys_Fixed.Add(abilityType, value);
            }
        }

        public float GetFixedValue(EAbilityType abilityType)
        {
            float value;
            if (abilitys_Fixed.TryGetValue(abilityType, out value))
            {
                return value;
            }
            else
            {
                return 0;
            }
        }

        public void AddPercentValue(EAbilityType abilityType, float value)
        {
            if (abilitys_Percent.ContainsKey(abilityType))
            {
                abilitys_Percent[abilityType] += value;
            }
            else
            {
                abilitys_Percent.Add(abilityType, value);
            }
        }

        public float GetPercentValue(EAbilityType abilityType)
        {
            float value;
            if (abilitys_Percent.TryGetValue(abilityType, out value))
            {
                return Mathf.Max(0, 1 + value);
            }
            else
            {
                return 1;
            }
        }

        public void ApplyStatusEffect(StatusEffect se)
        {
            Dictionary<string, StatusEffect> ses;
            if (!statusEffects.TryGetValue(se.statusEffectType, out ses))
            {
                ses = new Dictionary<string, StatusEffect>();
                statusEffects.Add(se.statusEffectType, ses);
            }

            StatusEffect statusEffect;
            if (ses.TryGetValue(se.id, out statusEffect))
            {
                statusEffect.Apply(se);
            }
            else
            {
                se.Regist(this);
                ses.Add(se.id, se);
            }
        }

        public void Update(float deltaTime)
        {
            foreach (var pair in statusEffects)
            {
                foreach (var se in pair.Value)
                {
                    if (!se.Value.Update(deltaTime))
                    {
                        se.Value.Unregist(this);
                        removeInfos.Add(new removeInfo() { seType = se.Value.statusEffectType, seId = se.Key });
                    }
                }
            }

            foreach (var info in removeInfos)
            {
                Dictionary<string, StatusEffect> appliedSE;
                if (statusEffects.TryGetValue(info.seType, out appliedSE))
                {
                    appliedSE.Remove(info.seId);
                }
            }
            removeInfos.Clear();
        }

        public void Clear()
        {
            if (statusEffects != null)
            {
                foreach (var pair in statusEffects)
                {
                    foreach (var se in pair.Value)
                    {
                        se.Value.Unregist(this);
                    }
                    pair.Value.Clear();
                }
                statusEffects.Clear();
            }

            if (abilitys_Fixed != null) abilitys_Fixed.Clear();
            if (abilitys_Percent != null) abilitys_Percent.Clear();
            if (removeInfos != null) removeInfos.Clear();
        }

        public void Release()
        {
            Clear();

            abilitys_Fixed = null;
            abilitys_Percent = null;
            statusEffects = null;
            removeInfos = null;
            owner = null;
        }
    }

    public abstract class StatusEffect
    {
        public enum EType
        {
            None = 0,
            Berserker = 1,
            Laser = 2,
            ElectricShock = 4,
            Freezing = 8,
            AuraOfWind = 16,
        }

        public abstract EType statusEffectType { get; }

        public StatusController controller { get; protected set; }
        public bool isRegisted { get; private set; }
        public string id;
        public float duration;
        public float remainTime { get; protected set; }

        public void Regist(StatusController controller)
        {
            if(!isRegisted)
            {
                isRegisted = true;

                remainTime = duration;
                this.controller = controller;

                OnRegist();
            }
        }
        public void Unregist(StatusController controller)
        {
            if(isRegisted)
            {
                OnUnregist();
                this.controller = null;
                isRegisted = false;
            }
        }

        public abstract void OnRegist();
        public abstract void OnUnregist();

        public virtual void Apply(StatusEffect newEffect)
        {
            id = newEffect.id;
            duration = newEffect.duration;
        }

        /// <summary>
        /// 아직 남은 시간이 있는 경우 true 반환
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public virtual bool Update(float deltaTime)
        {
            remainTime -= deltaTime;

            return (remainTime > 0);
        }
    }

    public sealed class SE_Berserker : StatusEffect
    {
        public override EType statusEffectType { get { return EType.Berserker; } }
        
        public float addAttackSpeedRatio;

        private Fx_Switch m_Fx;
        public Fx_Switch fx { get { return m_Fx; } private set { m_Fx = value; } }

        public override void OnRegist()
        {
            controller.AddPercentValue(EAbilityType.AttackSpeed, addAttackSpeedRatio);

            if (fx == null && !SpawnMaster.TrySpawnFx("Fx_13_Status", controller.owner.transform.position, Quaternion.identity, out m_Fx))
            {
                // 이펙트가 생성되지 않음.
            }

            fx.transform.SetParent(controller.owner.transform);
            fx.transform.localPosition = Vector3.zero;
            fx.transform.localScale = Vector3.one;
            fx.On();
        }

        public override void OnUnregist()
        {
            controller.AddPercentValue(EAbilityType.AttackSpeed, -addAttackSpeedRatio);
            
            if (fx != null)
            {
                fx.Off();
                fx.Destroy();
                fx.transform.SetParent(null);
                fx = null;
            }
        }

        public override void Apply(StatusEffect newEffect)
        {
            base.Apply(newEffect);

            var e = newEffect as SE_Berserker;
            addAttackSpeedRatio = e.addAttackSpeedRatio;
            duration = remainTime = e.duration;
        }
    }

    public sealed class SE_Laser : StatusEffect
    {
        public override EType statusEffectType { get { return EType.Laser; } }

        public bool isOnlyCritical;
        public float dmgRatio;
        public float laserWidthRatio;

        private Fx_Switch m_Fx;
        public Fx_Switch fx { get { return m_Fx; } private set { m_Fx = value; } }

        public override void OnRegist()
        {
            controller.owner.OnAttack += OnAttack;

            //if (fx == null && !SpawnMaster.TrySpawnFx("Fx_13_Status", controller.owner.transform.position, Quaternion.identity, out m_Fx))
            //{
            //    // 이펙트가 생성되지 않음.
            //}

            //fx.transform.SetParent(controller.owner.transform);
            //fx.transform.localPosition = Vector3.zero;
            //fx.transform.localScale = Vector3.one;
            //fx.On();
        }

        public override void OnUnregist()
        {
            controller.owner.OnAttack -= OnAttack;

            //if (fx != null)
            //{
            //    fx.Off();
            //    fx.Destroy();
            //    fx.transform.SetParent(null);
            //    fx = null;
            //}
        }

        public override void Apply(StatusEffect newEffect)
        {
            base.Apply(newEffect);

            var e = newEffect as SE_Laser;
            laserWidthRatio = e.laserWidthRatio;
            duration = remainTime = e.duration;
            isOnlyCritical = e.isOnlyCritical;
        }

        private IngameAttackInfo OnAttack(IngameAttackInfo attInfo)
        {
            if (isOnlyCritical && !attInfo.isCritical)
            {
                return attInfo;
            }

            TowerProjectile proj;
            if (!SpawnMaster.TrySpawnMonoBehaviour("TD-PROJ-4", controller.owner.projectileSpawnPointTf.position, Quaternion.identity, out proj))
            {
                Debug.LogErrorFormat("{0} :: Can't spawned TowerProjectile({1})", GetType(), controller.owner.data.projectileKey);
                return attInfo;
            }

            var newAttInfo = new IngameAttackInfo(attInfo);
            newAttInfo.damage = Mathf.RoundToInt(attInfo.damage * dmgRatio);
            newAttInfo.isCritical = false;

            (proj as TowerProjectile_Laser).maxWidthRatio = laserWidthRatio;
            proj.SetData(new TowerProjectileData(controller.owner, newAttInfo));
            proj.Fire();

            return null;
        }
    }

    public sealed class SE_ElectricShock : StatusEffect
    {
        public override EType statusEffectType { get { return EType.ElectricShock; } }

        private Fx_Switch m_Fx;
        public Fx_Switch fx { get { return m_Fx; } private set { m_Fx = value; } }

        public override void OnRegist()
        {
            controller.owner.OnCheckAttackable += OnCheckAttackable;

            if (fx == null && !SpawnMaster.TrySpawnFx("Fx_4_Status", controller.owner.transform.position, Quaternion.identity, out m_Fx))
            {
                // 이펙트가 생성되지 않음.
            }

            fx.transform.SetParent(controller.owner.transform);
            fx.transform.localPosition = Vector3.zero;
            fx.On();
        }

        public override void OnUnregist()
        {
            controller.owner.OnCheckAttackable -= OnCheckAttackable;

            if (fx != null)
            {
                fx.Off();
                fx.Destroy();
                fx.transform.SetParent(null);
                fx = null;
            }
        }

        private bool OnCheckAttackable()
        {
            return false;
        }
    }

    public sealed class SE_Freezing : StatusEffect
    {
        public override EType statusEffectType { get { return EType.Freezing; } }

        public float power { get; private set; }

        private Fx_Switch m_Fx;
        public Fx_Switch fx { get { return m_Fx; } private set { m_Fx = value; } }

        public override void OnRegist()
        {
            controller.AddPercentValue(EAbilityType.MoveSpeed, power);

            if (fx == null && !SpawnMaster.TrySpawnFx("Fx_2_Status", controller.owner.transform.position, Quaternion.identity, out m_Fx))
            {
                // 이펙트가 생성되지 않음.
            }

            fx.transform.SetParent(controller.owner.transform);
            fx.transform.localPosition = Vector3.zero;
            fx.On();
        }

        public override void OnUnregist()
        {
            controller.AddPercentValue(EAbilityType.MoveSpeed, -power);

            if (fx != null)
            {
                fx.Off();
                fx.Destroy();
                fx.transform.SetParent(null);
                fx = null;
            }
        }

        public override void Apply(StatusEffect newEffect)
        {
            base.Apply(newEffect);

            var e = newEffect as SE_Freezing;
            power = e.power;
        }
    }

    public sealed class SE_AuraOfWind : StatusEffect
    {
        public override EType statusEffectType { get { return EType.AuraOfWind; } }

        public float addAttackSpeedRatio;

        private Fx_Switch m_Fx;
        public Fx_Switch fx { get { return m_Fx; } private set { m_Fx = value; } }

        public override void OnRegist()
        {
            controller.AddPercentValue(EAbilityType.AttackSpeed, addAttackSpeedRatio);

            if (fx == null && !SpawnMaster.TrySpawnFx("Fx_21_Status", controller.owner.transform.position, Quaternion.identity, out m_Fx))
            {
                // 이펙트가 생성되지 않음.
            }

            fx.transform.SetParent(controller.owner.transform);
            fx.transform.localPosition = Vector3.zero;
            fx.transform.localScale = Vector3.one;
            fx.On();
        }

        public override void OnUnregist()
        {
            controller.AddPercentValue(EAbilityType.AttackSpeed, -addAttackSpeedRatio);

            if (fx != null)
            {
                fx.Off();
                fx.Destroy();
                fx.transform.SetParent(null);
                fx = null;
            }
        }

        public override void Apply(StatusEffect newEffect)
        {
            base.Apply(newEffect);

            var e = newEffect as SE_Berserker;
            addAttackSpeedRatio = e.addAttackSpeedRatio;
            duration = remainTime = e.duration;
        }
    }

}