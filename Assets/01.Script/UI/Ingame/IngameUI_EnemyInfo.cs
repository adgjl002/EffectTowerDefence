using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IngameUI_EnemyInfo : UIBase
{
    [SerializeField]
    private TextMeshProUGUI m_HpTxt;
    public TextMeshProUGUI hpTxt { get { return m_HpTxt; } }

    [SerializeField]
    private TextMeshProUGUI m_MoveSpeedTxt;
    public TextMeshProUGUI moveSpeedTxt { get { return m_MoveSpeedTxt; } }

    [SerializeField]
    private TextMeshProUGUI m_UnitTypeTxt;
    public TextMeshProUGUI unitTypeTxt { get { return m_UnitTypeTxt; } }

    [SerializeField]
    private TextMeshProUGUI m_DefenseTxt;
    public TextMeshProUGUI defenseTxt { get { return m_DefenseTxt; } }

    [SerializeField]
    private TextMeshProUGUI m_ShiedlTxt;
    public TextMeshProUGUI shieldTxt { get { return m_ShiedlTxt; } }

    [SerializeField]
    private TextMeshProUGUI m_RewardTxt;
    public TextMeshProUGUI rewardTxt { get { return m_RewardTxt; } }

    public IngameObject targetIObj { get; private set; }

    public void SetData(IngameObject targetIObj)
    {
        this.targetIObj = targetIObj;
    }

    public void UpdateUI()
    {
        hpTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">{1}/{2}</style>", UITextManager.GetText("체력"), targetIObj.nowHp, targetIObj.maxHp);
        moveSpeedTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">{1}</style>", UITextManager.GetText("이동속도"), Helper.ConverToMoveSpeedText(targetIObj.moveSpeed));
        unitTypeTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">{1}</style>", UITextManager.GetText("유닛타입"), Helper.ConvertToUnitTypeText(targetIObj.unitType));
        defenseTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">{1}%</style>", UITextManager.GetText("방어력"), targetIObj.defense * 100);

        if ((targetIObj.maxShield == 0))
        { 
            shieldTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">{1}</style>", UITextManager.GetText("보호막"), "-");
        }
        else
        {
            shieldTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">{1}/{2}</style>", UITextManager.GetText("보호막"), targetIObj.nowShield, targetIObj.maxShield);
        }

        rewardTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">{1}G</style>", UITextManager.GetText("보상"), targetIObj.data.cost);
    }

    private void Update()
    {
        if(gameObject.activeInHierarchy && targetIObj != null)
        {
            UpdateUI();
        }
    }
}
