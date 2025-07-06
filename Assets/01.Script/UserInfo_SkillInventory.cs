using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UserInfo_
{
    public class SkillInventory
    {
        public SkillInventory(UserInfo instance)
        {
            tierByActiveSkillTypes = new Dictionary<int, List<int>>();
            owner = instance;
        }

        public void Initialize()
        {
            tierByActiveSkillTypes.Clear();
            foreach(var s in skillInventory)
            {
                RegistTierByActiveSkillType(s.Key);
            }
        }
        private void RegistTierByActiveSkillType(string skillID)
        {
            SkillData sd;
            if (DataManager.Instance.TryGetSkillData(skillID, out sd))
            {
                List<int> activeSkillTypes;
                if (!tierByActiveSkillTypes.TryGetValue(sd.skillTier, out activeSkillTypes))
                {
                    activeSkillTypes = new List<int>();
                    tierByActiveSkillTypes.Add(sd.skillTier, activeSkillTypes);
                }

                var skillType = sd.skillType;
                foreach (var t in activeSkillTypes)
                {
                    if (t == skillType) goto EXIST_SKILLTYPE;
                }

                activeSkillTypes.Add(skillType);

                EXIST_SKILLTYPE:;
            }

        }
        private void UnregistTierByActiveSkillType(string skillID)
        {
            SkillData sd;
            if (DataManager.Instance.TryGetSkillData(skillID, out sd))
            {
                List<int> activeSkillTypes;
                if (!tierByActiveSkillTypes.TryGetValue(sd.skillTier, out activeSkillTypes))
                {
                    activeSkillTypes = new List<int>();
                    tierByActiveSkillTypes.Add(sd.skillTier, activeSkillTypes);
                }

                var skillType = sd.skillType;
                for(int i = 0; i<activeSkillTypes.Count; ++i)
                {
                    if (activeSkillTypes[i] == skillType)
                    {
                        activeSkillTypes.RemoveAt(i);
                        Debug.LogFormat("UN-REGITSTED SKILLTYPE {0} / {1}", sd.skillTier, sd.skillType);
                        break;
                    }
                }
            }
        }

        public UserInfo owner { get; private set; }

        public Dictionary<string, bool> skillInventory { get { return owner.data.skillInventory; } }
        public Dictionary<int, List<int>> tierByActiveSkillTypes { get; private set; }
        public bool TryGetTierByActiveSkillTypes(int tier, out List<int> activeSkillTypes)
        {
            return tierByActiveSkillTypes.TryGetValue(tier, out activeSkillTypes);
        }

        public bool GetActiveSkill(string skillID)
        {
            bool value;
            if (skillInventory == null)
            {
                return false;
            }
            else if (skillInventory.TryGetValue(skillID, out value))
            {
                return value;
            }
            return false;
        }
        public void SetActiveSkill(string skillID, bool isActivation)
        {
            if (skillInventory == null)
            {
                return;
            }
            else if (skillInventory.ContainsKey(skillID))
            {
                skillInventory[skillID] = isActivation;
            }
            else
            {
                skillInventory.Add(skillID, isActivation);
            }

            if(isActivation)
            {
                RegistTierByActiveSkillType(skillID);
            }
            else
            {
                UnregistTierByActiveSkillType(skillID);
            }
        }

        /// <summary>
        /// 배울 수 있는 스킬인지 판단하는 함수
        /// </summary>
        /// <param name="skillData"></param>
        /// <returns></returns>
        public bool CheckCanActiveSkill(SkillData skillData)
        {
            if (skillInventory == null)
            {
                return false;
            }

            // 스킬을 배우기 위해 필요한 스킬 목록이 없는 경우
            // 함께 보유할 수 없는 스킬목록이 없는 경우
            return CheckNeedSkillCondition(skillData) && CheckNoNeedSkillCondition(skillData);
        }

        /// <summary>
        /// 스킬을 배우기 위해 필요한 스킬을 가지고 있다면 True 가지고 있지않다면 False 반환
        /// </summary>
        /// <param name="skillData"></param>
        /// <returns></returns>
        public bool CheckNeedSkillCondition(SkillData skillData)
        {
            if (skillData.needSkillIDs == null)
            {
                return true;
            }
            // 필요 스킬 목록 중 하나 이상 보유해야하는 경우
            else if (skillData.needSkillIDsOperator == 0)
            {
                for (int i = 0; i < skillData.needSkillIDs.Length; ++i)
                {
                    if (!string.IsNullOrEmpty(skillData.needSkillIDs[i])
                        && GetActiveSkill(skillData.needSkillIDs[i]))
                    {
                        return true;
                    }
                }
            }
            // 필요 스킬 목록을 모두 보유해야하는 경우
            else if (skillData.needSkillIDsOperator == 1)
            {
                for (int i = 0; i < skillData.needSkillIDs.Length; ++i)
                {
                    if (string.IsNullOrEmpty(skillData.needSkillIDs[i])
                        || !GetActiveSkill(skillData.needSkillIDs[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 함께 보유할 수 없는 스킬을 가지고 있지 않다면 True 가지고 있다면 False 반환
        /// </summary>
        /// <param name="skillData"></param>
        /// <returns></returns>
        public bool CheckNoNeedSkillCondition(SkillData skillData)
        {
            // 함께 배울 수 없는 스킬 중 하나라도 가지고 있는가?
            if (skillData.noNeedSkillIDs != null)
            {
                for (int i = 0; i < skillData.noNeedSkillIDs.Length; ++i)
                {
                    if (string.IsNullOrEmpty(skillData.noNeedSkillIDs[i])
                        || GetActiveSkill(skillData.noNeedSkillIDs[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public string GetHighestLevelSkillID(int skillNo, string skillType = "TAE")
        {
            string skillKey = string.Format("Skill_{0}_{1}", skillType, skillNo);

            int highestLv = -1;
            string highestLvSkillID = string.Empty;
            for (int subNo = 1; subNo <= Global.MAX_SkILL_SUBNO; ++subNo)
            {
                for (int lv = 0; lv <= Global.MAX_SKILL_LEVEL; ++lv)
                {
                    string curSkillID = string.Format("{0}_{1}_{2}", skillKey, subNo, lv);
                    if (highestLv < lv && GetActiveSkill(curSkillID))
                    {
                        highestLv = lv;
                        highestLvSkillID = curSkillID;
                    }
                }
            }

            return highestLvSkillID;
        }

        public skillIDInfo GetHighestLevelSkillIDInfo(int skillNo, string skillType = "TAE")
        {
            string skillKey = string.Format("Skill_{0}_{1}", skillType, skillNo);

            int highestSubNo = 0;
            int highestLv = 0;
            string highestLvSkillID = string.Empty;
            for (int subNo = 1; subNo <= Global.MAX_SkILL_SUBNO; ++subNo)
            {
                for (int lv = 0; lv <= Global.MAX_SKILL_LEVEL; ++lv)
                {
                    string curSkillID = string.Format("{0}_{1}_{2}", skillKey, subNo, lv);
                    if (highestLv < lv && GetActiveSkill(curSkillID))
                    {
                        highestSubNo = subNo;
                        highestLv = lv;
                        highestLvSkillID = curSkillID;
                    }
                }
            }

            return new skillIDInfo()
            {
                skillId = highestLvSkillID,
                level = highestLv,
                skillKey = skillKey,
                subNo = highestSubNo
            };
        }
        
        public int GetHighestSkillLevel(int skillNo, string skillType = "TAE")
        {
            var idInfo = GetHighestLevelSkillIDInfo(skillNo, skillType);
            return idInfo.level;
        }

        public void ResetSkillInv()
        {
            owner.data.skillInventory = new Dictionary<string, bool>();
            tierByActiveSkillTypes.Clear();
        }
    }

    public struct skillIDInfo
    {
        public string skillId;
        public string skillKey;
        public int subNo;
        public int level;
    }
}