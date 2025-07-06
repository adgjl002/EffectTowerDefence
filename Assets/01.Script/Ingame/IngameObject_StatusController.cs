using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IngameObject_
{
    public enum EAbilityType
    {
        None = 0,

        Fixed = 1,          // 0000 0001
        Percent = 2,        // 0000 0010

        MoveSpeed = 4,      // 0000 0100
        AttackSpeed = 8,    // 0000 1000
        Damage = 16,        // 0001 0000
        MaxHp = 32,         // 0010 0000
        Defense = 64,       // 0100 0000
        Regeneration = 128, // 1000 0000
        Resistance = 256,   // 0001 0000 0000
        MaxShield = 512,       // 0010 0000 0000
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

        public IngameObject owner;

        public void Initialize(IngameObject owner)
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
            if(abilitys_Percent.ContainsKey(abilityType))
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
            if(abilitys_Percent.TryGetValue(abilityType, out value))
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
            if(!statusEffects.TryGetValue(se.statusEffectType, out ses))
            {
                ses = new Dictionary<string, StatusEffect>();
                statusEffects.Add(se.statusEffectType, ses);
            }

            StatusEffect statusEffect;
            if(ses.TryGetValue(se.id, out statusEffect))
            {
                statusEffect.Apply(se);
            }
            else
            {
                se.Regist(this);
                ses.Add(se.id, se);
            }
        }

        public bool CheckAppliedStatusEffect(StatusEffect.EType seType)
        {
            return statusEffects.ContainsKey(seType);
        }

        public bool CheckAppliedStatusEffect(StatusEffect.EType seType, string id)
        {
            Dictionary<string, StatusEffect> ses;
            if(statusEffects.TryGetValue(seType, out ses))
            {
                return ses.ContainsKey(id);
            }
            return false;
        }

        public void Update(float deltaTime)
        {
            var calDeltaTime = deltaTime + (deltaTime * owner.resistance);

            foreach (var pair in statusEffects)
            {
                foreach(var se in pair.Value)
                {
                    if(!se.Value.Update(calDeltaTime))
                    {
                        se.Value.Unregist(this);
                        removeInfos.Add(new removeInfo() { seType = se.Value.statusEffectType, seId = se.Key });
                    }
                }
            }

            foreach(var info in removeInfos)
            {
                Dictionary<string, StatusEffect> appliedSE;
                if(statusEffects.TryGetValue(info.seType, out appliedSE))
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
                    foreach(var se in pair.Value)
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
            Freezing = 1,
            Poison = 2,
            ElectricShock = 3,
            Burning = 4,
            Frostbite = 5,
            Poison_Explosion = 6,
            Freezing_Stun = 7,
            Crush = 8,
            Regeneration = 9
        }

        public abstract EType statusEffectType { get; }

        public StatusController controller { get; protected set; }
        public string id;
        public float duration;
        public float remainTime { get; protected set; }
        public bool isRegisted { get; private set; }

        public void Regist(StatusController controller)
        {
            if (!isRegisted)
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
                isRegisted = false;
                OnUnregist();
            }

            controller = null;
        }

        public abstract void OnRegist();
        public abstract void OnUnregist();

        public virtual void Apply(StatusEffect newEffect)
        {
            id = newEffect.id;
            duration = remainTime = newEffect.duration;
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
    
    public sealed class SE_Freezing : StatusEffect
    {
        public override EType statusEffectType { get { return EType.Freezing; } }
        
        public float freezingPower;

        private Fx_Switch m_Fx;
        public Fx_Switch fx { get { return m_Fx; } private set { m_Fx = value; } }
        
        public override void OnRegist()
        {
            controller.AddPercentValue(EAbilityType.MoveSpeed, freezingPower);

            if (fx == null && !SpawnMaster.TrySpawnFx("Fx_2_Status", controller.owner.transform.position, Quaternion.identity, out m_Fx))
            {
                // 이펙트가 생성되지 않음.
            }

            fx.transform.SetParent(controller.owner.transform);
            fx.transform.localPosition = Vector3.zero;
            fx.transform.localScale = new Vector3(controller.owner.bodySize, controller.owner.bodySize, controller.owner.bodySize);
            fx.On();
        }

        public override void OnUnregist()
        {
            controller.AddPercentValue(EAbilityType.MoveSpeed, -freezingPower);

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
            freezingPower = e.freezingPower;
        }
    }

    public sealed class SE_Frostbite : StatusEffect
    {
        public override EType statusEffectType { get { return EType.Frostbite; } }

        public float damagePerSeconds;
        private float oneSeconds = 1;

        private Fx_Switch m_Fx;
        public Fx_Switch fx { get { return m_Fx; } private set { m_Fx = value; } }

        public override void OnRegist()
        {
            //if (fx == null && !SpawnMaster.TrySpawnFx("Fx_2_Status", controller.owner.transform.position, Quaternion.identity, out m_Fx))
            //{
            //    // 이펙트가 생성되지 않음.
            //}

            //fx.transform.SetParent(controller.owner.transform);
            //fx.transform.localPosition = Vector3.zero;
            //fx.On();
        }
        public override void OnUnregist()
        {
            //if (fx != null)
            //{
            //    fx.Off();
            //    fx.Destroy();
            //    fx.transform.SetParent(null);
            //    fx = null;
            //}
        }

        public override bool Update(float deltaTime)
        {
            oneSeconds -= deltaTime;

            if (oneSeconds <= 0)
            {
                oneSeconds += 1f;

                if (controller.owner != null)
                {
                    controller.owner.Attacked(new IngameAttackInfo()
                    {
                        damage = Mathf.RoundToInt(damagePerSeconds),
                        damageType = EDamageType.Frostbite,
                        penetration = 0,
                        attacker = null,
                        target = controller.owner
                    });
                }
            }

            return base.Update(deltaTime);
        }

        public override void Apply(StatusEffect newEffect)
        {
            base.Apply(newEffect);

            var e = newEffect as SE_Frostbite;
            damagePerSeconds = e.damagePerSeconds;
        }
    }

    public sealed class SE_Freezing_Stun : StatusEffect
    {
        public override EType statusEffectType { get { return EType.Freezing_Stun; } }

        public bool isMovable { get; private set; }
        public int appliedCount { get; private set; }

        public float stunDuration;
        public int needAppliedCount;
        public float freezingPower;
        
        public override void OnRegist()
        {
            isMovable = true;
            appliedCount = 1;
            controller.owner.OnCheckMovable += OnCheckMovable;
        }

        public override void OnUnregist()
        {
            controller.owner.OnCheckMovable -= OnCheckMovable;
        }

        public override void Apply(StatusEffect newEffect)
        {
            base.Apply(newEffect);

            var e = newEffect as SE_Freezing_Stun;
            freezingPower = e.freezingPower;
            ++appliedCount;

            if(needAppliedCount == appliedCount)
            {
                isMovable = false;
                duration = remainTime = stunDuration;

                Fx_OneTime hitFx;
                if(SpawnMaster.TrySpawnFx("Fx_2_Hit", controller.owner.transform.position, Quaternion.identity, out hitFx))
                {
                    hitFx.On();
                }
            }
        }

        private bool OnCheckMovable()
        {
            return isMovable;
        }
    }

    public sealed class SE_Poison : StatusEffect
    {
        public override EType statusEffectType { get { return EType.Poison; } }

        public float penetration;
        public int damagePerSeconds;
        private float oneSeconds = 1;

        private Fx_Switch m_Fx;
        public Fx_Switch fx { get { return m_Fx; } private set { m_Fx = value; } }

        public override void OnRegist()
        {
            if (fx == null && !SpawnMaster.TrySpawnFx("Fx_3_Status", controller.owner.transform.position, Quaternion.identity, out m_Fx))
            {
                // 이펙트가 생성되지 않음.
            }

            fx.transform.SetParent(controller.owner.transform);
            fx.transform.localPosition = Vector3.zero;
            fx.On();
        }
        public override void OnUnregist()
        {
            if (fx != null)
            {
                fx.Off();
                fx.Destroy();
                fx.transform.SetParent(null);
                fx = null;
            }
        }

        public override bool Update(float deltaTime)
        {
            oneSeconds -= deltaTime;

            if (oneSeconds <= 0)
            {
                oneSeconds += 1f;
                
                if(controller.owner != null)
                {
                    controller.owner.Attacked(new IngameAttackInfo()
                    {
                        damage = Mathf.RoundToInt(damagePerSeconds),
                        damageType = EDamageType.Poison,
                        penetration = penetration,
                        attacker = null,
                        target = controller.owner
                    });
                }
            }

            return base.Update(deltaTime);
        }

        public override void Apply(StatusEffect newEffect)
        {
            base.Apply(newEffect);

            var e = newEffect as SE_Poison;
            damagePerSeconds = e.damagePerSeconds;
            penetration = e.penetration;
        }
    }

    public sealed class SE_Poison_Explosion : StatusEffect
    {
        public override EType statusEffectType { get { return EType.Poison_Explosion; } }

        public int damagePerSeconds;
        public float splashRange;
        public int penetration;
        private float oneSeconds = 1;

        private Fx_Switch m_Fx;
        public Fx_Switch fx { get { return m_Fx; } private set { m_Fx = value; } }

        public override void OnRegist()
        {
            controller.owner.OnDie += OnDie;

            //if (fx == null && !SpawnMaster.TrySpawnFx("Fx_3_Status", controller.owner.transform.position, Quaternion.identity, out m_Fx))
            //{
            //    // 이펙트가 생성되지 않음.
            //}

            //fx.transform.SetParent(controller.owner.transform);
            //fx.transform.localPosition = Vector3.zero;
            //fx.On();
        }

        public override void OnUnregist()
        {
            controller.owner.OnDie -= OnDie;

            //if (fx != null)
            //{
            //    fx.Off();
            //    fx.Destroy();
            //    fx.transform.SetParent(null);
            //    fx = null;
            //}
        }

        //public override bool Update(float deltaTime)
        //{
        //    oneSeconds -= deltaTime;

        //    if (oneSeconds <= 0)
        //    {
        //        oneSeconds += 1f;

        //        if (controller.owner != null)
        //        {
        //            controller.owner.Attacked(new IngameAttackInfo()
        //            {
        //                damage = Mathf.RoundToInt(damagePerSeconds),
        //                damageType = EDamageType.Poison,
        //                penetration = 0,
        //                attaker = null,
        //            });
        //        }
        //    }

        //    return base.Update(deltaTime);
        //}

        public override void Apply(StatusEffect newEffect)
        {
            base.Apply(newEffect);

            var e = newEffect as SE_Poison_Explosion;
            damagePerSeconds = e.damagePerSeconds;
            splashRange = e.splashRange;
            penetration = e.penetration;
        }

        private void OnDie(IngameObject obj)
        {
            if (splashRange > 0)
            {
                Fx_OneTime fx;
                if (SpawnMaster.TrySpawnFx("Fx_7_Hit", obj.transform.position, Quaternion.identity, out fx))
                {
                    fx.On();
                }

                List<IngameObject> splashTargets;
                if (Helper.Ingame.FindAllTargets(obj.gameObject, obj, splashRange, out splashTargets))
                {
                    foreach (var t in splashTargets)
                    {
                        t.statusController.ApplyStatusEffect(new SE_Poison() { id = "Poison", duration = duration, damagePerSeconds = damagePerSeconds, penetration = penetration });
                    }
                }
            }
        }
    }

    public sealed class SE_Burning : StatusEffect
    {
        public override EType statusEffectType { get { return EType.Burning; } }
        
        public float damagePerSeconds;
        private float oneSeconds = 1;

        private Fx_Switch m_Fx;
        public Fx_Switch fx { get { return m_Fx; } private set { m_Fx = value; } }

        public override void OnRegist()
        {
            if (fx == null && !SpawnMaster.TrySpawnFx("Fx_1_Status", controller.owner.transform.position, Quaternion.identity, out m_Fx))
            {
                // 이펙트가 생성되지 않음.
            }

            fx.transform.SetParent(controller.owner.transform);
            fx.transform.localPosition = Vector3.zero;
            fx.On();
        }
        public override void OnUnregist()
        {
            if (fx != null)
            {
                fx.Off();
                fx.Destroy();
                fx.transform.SetParent(null);
                fx = null;
            }
        }

        public override bool Update(float deltaTime)
        {
            oneSeconds -= deltaTime;

            if (oneSeconds <= 0)
            {
                oneSeconds += 1f;

                if (controller.owner != null)
                {
                    controller.owner.Attacked(new IngameAttackInfo()
                    {
                        damage = Mathf.RoundToInt(damagePerSeconds),
                        damageType = EDamageType.Burning,
                        penetration = 0,
                        attacker = null,
                        target = controller.owner
                    });
                }
            }

            return base.Update(deltaTime);
        }

        public override void Apply(StatusEffect newEffect)
        {
            base.Apply(newEffect);

            var e = newEffect as SE_Burning;
            damagePerSeconds = e.damagePerSeconds;
        }
    }

    public sealed class SE_ElectricShock : StatusEffect
    {
        public override EType statusEffectType { get { return EType.ElectricShock; } }
        
        private Fx_Switch m_Fx;
        public Fx_Switch fx { get { return m_Fx; } private set { m_Fx = value; } }
        
        public override void OnRegist()
        {
            controller.owner.OnCheckMovable += OnCheckMovable;

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
            controller.owner.OnCheckMovable -= OnCheckMovable;

            if (fx != null)
            {
                fx.Off();
                fx.Destroy();
                fx.transform.SetParent(null);
                fx = null;
            }
        }

        private bool OnCheckMovable()
        {
            return false;
        }
    }

    public sealed class SE_Crush : StatusEffect
    {
        public override EType statusEffectType { get { return EType.Crush; } }

        public float crushPower;

        private Fx_Switch m_Fx;
        public Fx_Switch fx { get { return m_Fx; } private set { m_Fx = value; } }

        public override void OnRegist()
        {
            controller.AddFixedValue(EAbilityType.Defense, crushPower);

            if (fx == null && !SpawnMaster.TrySpawnFx("Fx_TAE_Crush_Status", controller.owner.transform.position, Quaternion.identity, out m_Fx))
            {
                // 이펙트가 생성되지 않음.
            }

            fx.transform.SetParent(controller.owner.transform);
            fx.transform.localPosition = Vector3.zero;
            fx.transform.localScale = new Vector3(controller.owner.bodySize, controller.owner.bodySize, controller.owner.bodySize);
            fx.On();

        }

        public override void OnUnregist()
        {
            controller.AddFixedValue(EAbilityType.Defense, -crushPower);

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

            var e = newEffect as SE_Crush;
            crushPower = e.crushPower;
        }
    }
}