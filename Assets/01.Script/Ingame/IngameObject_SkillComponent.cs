using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IngameObject_
{
    public struct SkillData
    {
        public string skillId;
        public string[] skillDatas;
    }

    public abstract class SkillComponent
    {
        public IngameObject owner;
        public string skillId { get; private set; }

        public void Initialize(IngameObject owner)
        {
            this.owner = owner;
            OnInitialize();
        }

        public void SetData(SkillData skillData)
        {
            this.skillId = skillData.skillId;
            OnSetData(skillData);
        }

        public abstract void OnSetData(SkillData data);

        public abstract void OnInitialize();

        public void Update(float scaledDeltaTime, float unScaledDeltaTime)
        {
            OnUpdate(scaledDeltaTime, unScaledDeltaTime);
        }

        public abstract void OnUpdate(float scaledDeltaTime, float unScaledDeltaTime);
        public abstract bool CheckCanUseSkill();
        public abstract void UseSkill();

        public void Release()
        {
            OnRelease();
            owner = null;
        }

        public abstract void OnRelease();

        #region < Finder >
        
        public static bool TryFindAllTowerAtRangeFromPos(Vector3 pos, float range, out List<GridBlock_Tower> targets)
        {
            targets = new List<GridBlock_Tower>();
            foreach (var pair in AppManager.Instance.gridMap.gridInfoMap)
            {
                foreach (var gridInfoPair in pair.Value)
                {
                    if (gridInfoPair.gridBlock != null
                        && gridInfoPair.gridBlock.EqualGridType(GridBlock.EType.Ground)
                        && gridInfoPair.gridBlock.isBuildTower)
                    {
                        var dis = Vector2.Distance(pos, gridInfoPair.gridBlock.tower.transform.position);
                        if (dis <= range)
                        {
                            targets.Add(gridInfoPair.gridBlock.tower);
                        }
                    }
                }
            }
            return (targets.Count > 0);
        }

        #endregion

        #region < Creator >

        public static SkillComponent CreateSkill(SkillData data)
        {
            var datas = data.skillId.Split('_');
            if(datas.Length < 3)
            {
                return null;
            }

            SkillComponent skill;
            switch(datas[0])
            {
                case "ElectricShock":
                    Debug.LogFormat("<color=blue>{0}</color>", datas[0]);
                    skill = new Skill_ElectricShock();
                    //skill.SetData(data);
                    return skill;

                case "Freezing":
                    Debug.LogFormat("<color=blue>{0}</color>", datas[0]);
                    skill = new Skill_Freezing();
                    //skill.SetData(data);
                    return skill;

                case "Heal":
                    Debug.LogFormat("<color=blue>{0}</color>", datas[0]);
                    skill = new Skill_Heal();
                    //skill.SetData(data);
                    return skill;

                case "ShockWave":
                    Debug.LogFormat("<color=blue>{0}</color>", datas[0]);
                    skill = new Skill_ShockWave();
                    return skill;

                case "IceWave":
                    Debug.LogFormat("<color=blue>{0}</color>", datas[0]);
                    skill = new Skill_IceWave();
                    return skill;
            }
            return null;
        }

        #endregion
    }

    public class CooltimeSkillComponent : SkillComponent
    {
        public float cooltime { get; private set; }
        public float cooltimer { get; private set; }

        public override bool CheckCanUseSkill()
        {
            return cooltimer >= cooltime;
        }

        public override void OnInitialize()
        {
            cooltimer = 0;
        }

        public override void OnRelease()
        {

        }

        public override void OnSetData(SkillData data)
        {
            cooltime = Helper.Parser.StringToFloat(data.skillDatas[0]);
            cooltimer = 0;
        }

        public override void OnUpdate(float scaledDeltaTime, float unScaledDeltaTime)
        {
            cooltimer += scaledDeltaTime;
        }

        public override void UseSkill()
        {
            cooltimer = 0;
        }
    }

    public class Skill_ElectricShock : CooltimeSkillComponent
    {
        public float range { get; private set; }
        public float power { get; private set; }
        public float duration { get; private set; }
        public int targetCount { get; private set; }

        public override void OnSetData(SkillData data)
        {
            base.OnSetData(data);

            range = Helper.Parser.StringToFloat(data.skillDatas[1]);
            power = Helper.Parser.StringToFloat(data.skillDatas[2]);
            duration = Helper.Parser.StringToFloat(data.skillDatas[3]);
            targetCount = Helper.Parser.StringToInt(data.skillDatas[4]);
        }

        public override bool CheckCanUseSkill()
        {
            return base.CheckCanUseSkill();
        }

        public override void UseSkill()
        {
            base.UseSkill();

            Fx_OneTime fx;
            if (SpawnMaster.TrySpawnFx("Fx_ElectricShock_Explosion", owner.transform.position, Quaternion.identity, out fx))
            {
                fx.On();
                fx.transform.localScale = new Vector3(range, range, range);
            }

            List<GridBlock_Tower> targets;
            if (TryFindAllTowerAtRangeFromPos(owner.transform.position, range, out targets))
            {
                for (int i = 0; i < targets.Count; ++i)
                {
                    targets[i].statusController.ApplyStatusEffect(new IngameGrid.SE_ElectricShock() { id = "Stun", duration = duration });
                }
            }
        }
    }

    public class Skill_Freezing : CooltimeSkillComponent
    {
        public float range { get; private set; }
        public float power { get; private set; }
        public float duration { get; private set; }
        public int targetCount { get; private set; }

        public override void OnSetData(SkillData data)
        {
            base.OnSetData(data);

            range = Helper.Parser.StringToFloat(data.skillDatas[1]);
            power = Helper.Parser.StringToFloat(data.skillDatas[2]);
            duration = Helper.Parser.StringToFloat(data.skillDatas[3]);
            targetCount = Helper.Parser.StringToInt(data.skillDatas[4]);
        }

        public override bool CheckCanUseSkill()
        {
            //foreach (var pair in AppManager.Instance.gridMap.gridInfoMap)
            //{
            //    foreach (var gridInfoPair in pair.Value)
            //    {
            //        if (gridInfoPair.gridBlock != null
            //            && gridInfoPair.gridBlock.EqualGridType(GridBlock.EType.Ground)
            //            && gridInfoPair.gridBlock.isBuildTower)
            //        {
            //            var dis = Vector2.Distance(owner.transform.position, gridInfoPair.gridBlock.tower.transform.position);
            //            if (dis <= range)
            //            {
            //                return base.CheckCanUseSkill() && true;
            //            }
            //        }
            //    }
            //}
            return base.CheckCanUseSkill();
        }

        public override void UseSkill()
        {
            base.UseSkill();

            Fx_OneTime fx;
            if (SpawnMaster.TrySpawnFx("Fx_Freezing_Explosion", owner.transform.position, Quaternion.identity, out fx))
            {
                fx.On();
                fx.transform.localScale = new Vector3(range, range, range);
            }

            List<GridBlock_Tower> targets;
            if (TryFindAllTowerAtRangeFromPos(owner.transform.position, range, out targets))
            {
                for (int i = 0; i < targets.Count; ++i)
                {
                    targets[i]?.statusController.ApplyStatusEffect(new IngameGrid.SE_Freezing() { id = "Freezing", duration = duration });
                }
            }
        }
    }

    public class Skill_Heal : CooltimeSkillComponent
    {
        public float range { get; private set; }
        public float power { get; private set; }
        public float duration { get; private set; }
        public int targetCount { get; private set; }

        public override void OnSetData(SkillData data)
        {
            base.OnSetData(data);

            range = Helper.Parser.StringToFloat(data.skillDatas[1]);
            power = Helper.Parser.StringToFloat(data.skillDatas[2]);
            duration = Helper.Parser.StringToFloat(data.skillDatas[3]);
            targetCount = Helper.Parser.StringToInt(data.skillDatas[4]);
        }

        public override bool CheckCanUseSkill()
        {
            //foreach (var pair in AppManager.Instance.gridMap.gridInfoMap)
            //{
            //    foreach (var gridInfoPair in pair.Value)
            //    {
            //        if (gridInfoPair.gridBlock != null
            //            && gridInfoPair.gridBlock.EqualGridType(GridBlock.EType.Ground)
            //            && gridInfoPair.gridBlock.isBuildTower)
            //        {
            //            var dis = Vector2.Distance(owner.transform.position, gridInfoPair.gridBlock.tower.transform.position);
            //            if (dis <= range)
            //            {
            //                return base.CheckCanUseSkill() && true;
            //            }
            //        }
            //    }
            //}
            return base.CheckCanUseSkill();
        }

        public override void UseSkill()
        {
            base.UseSkill();

            Fx_OneTime fx;
            if (SpawnMaster.TrySpawnFx("Fx_Heal_Explosion", owner.transform.position, Quaternion.identity, out fx))
            {
                fx.On();
                fx.transform.localScale = new Vector3(range, range, range);
            }

            List<IngameObject> targets;
            if (Helper.Ingame.FindAllTargets(owner.gameObject, owner, range, out targets))
            {
                for (int i = 0; i < targets.Count; ++i)
                {
                    if (targets[i] != null && !targets[i].isDie)
                    {
                        targets[i].HealHp(Mathf.RoundToInt(owner.maxHp * power), true);
                    }
                }
            }
        }
    }

    public class Skill_ShockWave : SkillComponent
    {
        public float range { get; private set; }
        public float duration { get; private set; }

        public override bool CheckCanUseSkill()
        {
            return false;
        }

        public override void OnInitialize()
        {
            owner.OnDie += OnDie;
        }

        public override void OnSetData(SkillData data)
        {
            range = Helper.Parser.StringToFloat(data.skillDatas[0]);
            duration = Helper.Parser.StringToFloat(data.skillDatas[1]);
        }

        public void OnDie(IngameObject obj)
        {
            Fx_OneTime fx;
            if (SpawnMaster.TrySpawnFx("Fx_ElectricShock_Explosion", owner.transform.position, Quaternion.identity, out fx))
            {
                fx.On();
                fx.transform.localScale = new Vector3(range, range, range);
            }

            List<GridBlock_Tower> targets;
            if (TryFindAllTowerAtRangeFromPos(owner.transform.position, range, out targets))
            {
                for (int i = 0; i < targets.Count; ++i)
                {
                    targets[i].statusController.ApplyStatusEffect(new IngameGrid.SE_ElectricShock() { id = "Stun", duration = duration });
                }
            }
        }

        public override void OnRelease()
        {
            owner.OnDie -= OnDie;
        }

        public override void OnUpdate(float scaledDeltaTime, float unScaledDeltaTime)
        {

        }

        public override void UseSkill()
        {

        }
    }
    
    public class Skill_IceWave : SkillComponent
    {
        public float range { get; private set; }
        public float duration { get; private set; }
        public float power { get; private set; }

        public override bool CheckCanUseSkill()
        {
            return false;
        }

        public override void OnInitialize()
        {
            owner.OnDie += OnDie;
        }

        public override void OnSetData(SkillData data)
        {
            range = Helper.Parser.StringToFloat(data.skillDatas[0]);
            duration = Helper.Parser.StringToFloat(data.skillDatas[1]);
            power = Helper.Parser.StringToFloat(data.skillDatas[2]);
        }

        public void OnDie(IngameObject obj)
        {
            Fx_OneTime fx;
            if (SpawnMaster.TrySpawnFx("Fx_Freezing_Explosion", owner.transform.position, Quaternion.identity, out fx))
            {
                fx.On();
                fx.transform.localScale = new Vector3(range, range, range);
            }

            List<GridBlock_Tower> targets;
            if (TryFindAllTowerAtRangeFromPos(owner.transform.position, range, out targets))
            {
                for (int i = 0; i < targets.Count; ++i)
                {
                    targets[i]?.statusController.ApplyStatusEffect(new IngameGrid.SE_Freezing() { id = "Freezing", duration = duration });
                }
            }
        }

        public override void OnRelease()
        {
            owner.OnDie -= OnDie;
        }

        public override void OnUpdate(float scaledDeltaTime, float unScaledDeltaTime)
        {

        }

        public override void UseSkill()
        {

        }
    }
}

