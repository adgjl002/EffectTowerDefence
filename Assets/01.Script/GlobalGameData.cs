using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Global
{
    public const string SKILL_SUB_DATA_IS_DOUBLE = "isDouble";
    public const string SKILL_SUB_DATA_IS_TRIPLE = "isTriple";
    public const string SKILL_SUB_DATA_IS_QUADRUPLE = "isQuadruple";

    public const int MAX_SKILL_TIER = 3;
    public const int MAX_SKILL_LEVEL = 5;
    public const int MAX_SkILL_SUBNO = 5;

    public static string COLOR_CODE_COST = "#FFC900";
    public static string COLOR_CODE_UNIT_CLASS_NORMAL = "#FFFFFF";
    public static string COLOR_CODE_UNIT_CLASS_HERO = "#008DFF";
    public static string COLOR_CODE_UNIT_CLASS_BOSS = "#FF9400";

    public static Color GetDamageColor(EDamageType dmgType, bool isCritical = false)
    {
        switch (dmgType)
        {
            default:
            case EDamageType.None:
                if (isCritical)
                    return new Color32(255, 0, 0, 255);
                else
                    return new Color32(255, 255, 255, 255);
            case EDamageType.Frostbite: return new Color32(0, 126, 255, 255);
            case EDamageType.Poison: return new Color32(0, 255, 0, 255);
            case EDamageType.Explosion: return new Color32(255, 127, 0, 255);
            case EDamageType.Transition: return new Color32(255, 255, 0, 255);
            case EDamageType.Burning: return new Color32(255, 94, 0, 255);
            case EDamageType.HealHp: return new Color32(255, 255, 0, 255);
            case EDamageType.HealShield: return new Color32(0, 0, 255, 255);
        }
    }

    public static int GetTowerSkillNo(string towerId)
    {
        int skillNo = 0;
        switch (towerId)
        {
            case "TD-1": skillNo = 101; break;
            case "TD-2": skillNo = 102; break;
            case "TD-3": skillNo = 103; break;
            case "TD-4": skillNo = 104; break;
            case "TD-5": skillNo = 105; break;
        }
        return skillNo;
    }

    public static void InitializeUserSkillInventory()
    {
        // 공짜 스킬 습득
        var etor = DataManager.Instance.GetSkillDatasEtor();
        while (etor.MoveNext())
        {
            if (GameSettingsManager.OpenAllSkill || etor.Current.Value.starCost == 0)
            {
                UserInfo.Instance.skillInv.SetActiveSkill(etor.Current.Value.skillID, true);
            }
        }
    }
}