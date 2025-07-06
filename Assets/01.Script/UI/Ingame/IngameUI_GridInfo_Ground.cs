using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngameUI_GridInfo_Ground : UIBase
{
    [SerializeField]
    private IngameUI_GridInfo_Ground_BuildItem itemPrefab;

    [SerializeField]
    private RectTransform m_ContentRtf;
    public RectTransform contentRtf => m_ContentRtf;

    private UIItemCreator<IngameUI_GridInfo_Ground_BuildItem> itemCreator;
    public bool TryGetGroundBuildItem(int idx, out IngameUI_GridInfo_Ground_BuildItem item)
    {
        return itemCreator.TryGetItem(idx, out item);
    }

    [SerializeField]
    private RectTransform m_TowerAbilityUIRtf;
    public RectTransform towerAbilityUIRtf { get { return m_TowerAbilityUIRtf; } }

    [SerializeField]
    private TextMeshProUGUI m_DamageTxt;
    public TextMeshProUGUI damageTxt { get { return m_DamageTxt; } }

    [SerializeField]
    private TextMeshProUGUI m_AttackRangeTxt;
    public TextMeshProUGUI attackRangeTxt { get { return m_AttackRangeTxt; } }

    [SerializeField]
    private TextMeshProUGUI m_AttackSpeedTxt;
    public TextMeshProUGUI attackSpeedTxt { get { return m_AttackSpeedTxt; } }

    [SerializeField]
    private TextMeshProUGUI m_CriticalPercTxt;
    public TextMeshProUGUI criticalPercTxt { get { return m_CriticalPercTxt; } }

    [SerializeField]
    private TextMeshProUGUI m_CriticalDamageRatioTxt;
    public TextMeshProUGUI criticalDamageRatioTxt { get { return m_CriticalDamageRatioTxt; } }

    [SerializeField]
    private TextMeshProUGUI m_ProjectileCountTxt;
    public TextMeshProUGUI projectileCountTxt { get { return m_ProjectileCountTxt; } }

    [SerializeField]
    private TextMeshProUGUI m_MessageTxt;
    public TextMeshProUGUI messsageTxt { get { return m_MessageTxt; } }

    [SerializeField]
    private CustomButton m_BuildConfirmBtn;
    public CustomButton buildConfirmBtn { get { return m_BuildConfirmBtn; } }
    
    private List<string> towerKeys = new List<string>();
    public int curSelectedTowerBtnIdx { get; private set; }

    public IngameUI_GridInfo parentGridInfoUI { get; private set; }

    private void Awake()
    {
        itemCreator = new UIItemCreator<IngameUI_GridInfo_Ground_BuildItem>();
        itemCreator.SetData(contentRtf, itemPrefab);

        curSelectedTowerBtnIdx = -1;
        buildConfirmBtn.OnClick += OnClickBuildConfirmBtn;
    }

    public void OnClickBuildConfirmBtn()
    {
        if(curSelectedTowerBtnIdx == -1)
        {
            return;
        }
        else if(parentGridInfoUI.targetGridBlock.EqualGridType(GridBlock.EType.Ground))
        {
            var curTowerKey = towerKeys[curSelectedTowerBtnIdx];
            TowerData curTowerData;
            if (DataManager.Instance.TryGetTowerData(curTowerKey, out curTowerData, UserInfo.Instance.skillInv.GetHighestSkillLevel(Global.GetTowerSkillNo(curTowerKey)))
                && curTowerData.buildCost <= IngameManager.Instance.curCost)
            {
                IngameManager.Instance.AddCost(-curTowerData.buildCost, true);
                parentGridInfoUI.targetGridBlock.BuildTower(curTowerData);
            }
            else
            {
                UIManager.Instance.ShowNoticeUI("자원이 부족합니다.", 1f);
            }
        }

        IngameManager.Instance.UnselectGridBlock();

        parentGridInfoUI.Close();
    }

    public void SetData(IngameUI_GridInfo parentGridInfoUI, List<string> towerKeys)
    {
        this.parentGridInfoUI = parentGridInfoUI;
        this.towerKeys = towerKeys;
    }

    public override void Open()
    {
        if (itemCreator == null)
        {
            itemCreator = new UIItemCreator<IngameUI_GridInfo_Ground_BuildItem>();
            itemCreator.SetData(contentRtf, itemPrefab);
        }

        buildConfirmBtn.Close();
        CloseTowerInfoUI();

        base.Open();
    }

    public override void Close()
    {
        base.Close();

        itemCreator?.CloseAllItems();
    }

    public void UpdateUI()
    {
        itemCreator.UpdateItems(towerKeys.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int idx, IngameUI_GridInfo_Ground_BuildItem item)
    {
        var curTowerKey = towerKeys[idx];
        TowerData towerData;
        if (DataManager.Instance.TryGetTowerData(curTowerKey, out towerData, UserInfo.Instance.skillInv.GetHighestSkillLevel(Global.GetTowerSkillNo(curTowerKey))))
        {
            item.SetData(this, idx, towerData);
            item.Open();
            item.UpdateUI();
        }
        else
        {
            item.Close();
        }
    }

    public void OpenTowerInfoUI(int idx)
    {
        curSelectedTowerBtnIdx = idx;

        messsageTxt.gameObject.SetActive(false);
        towerAbilityUIRtf.gameObject.SetActive(true);

        var curTowerKey = towerKeys[curSelectedTowerBtnIdx];
        TowerData towerData;
        if(DataManager.Instance.TryGetTowerData(curTowerKey, out towerData, UserInfo.Instance.skillInv.GetHighestSkillLevel(Global.GetTowerSkillNo(curTowerKey))))
        {
            damageTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">{1}</style>", UITextManager.GetText("00001"), towerData.damage);
            attackRangeTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">{1}</style>", UITextManager.GetText("00002"), towerData.range);
            attackSpeedTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">{1}/s</style>", UITextManager.GetText("00003"), towerData.attackSpeed);
            criticalPercTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">{1}%</style>", UITextManager.GetText("00004"), towerData.criticalPerc * 100);
            criticalDamageRatioTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">{1}%</style>", UITextManager.GetText("00005"), towerData.criticalDamageRatio * 100);
            projectileCountTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">x {1}</style>", UITextManager.GetText("00006"), towerData.projectileCount);
            
            IngameManager.Instance.ingameSelectFrame.SetGridFrame(towerData.range);
            IngameManager.Instance.ingameSelectFrame.Open();
        }
        else
        {
            damageTxt.text = string.Empty;
            attackRangeTxt.text = string.Empty;
            attackSpeedTxt.text = string.Empty;
            criticalPercTxt.text = string.Empty;
            criticalDamageRatioTxt.text = string.Empty;
            projectileCountTxt.text = string.Empty;

            IngameManager.Instance.ingameSelectFrame.SetGridFrame(0);
            IngameManager.Instance.ingameSelectFrame.Open();
        }

        buildConfirmBtn.transform.SetParent(itemCreator.GetItem(curSelectedTowerBtnIdx).transform);
        buildConfirmBtn.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        buildConfirmBtn.Open();
    }

    public void CloseTowerInfoUI()
    {
        buildConfirmBtn.Close();
        towerAbilityUIRtf.gameObject.SetActive(false);
        messsageTxt.gameObject.SetActive(true);

        IngameManager.Instance.ingameSelectFrame.SetGridFrame(0);
        IngameManager.Instance.ingameSelectFrame.Open();
    }
}
