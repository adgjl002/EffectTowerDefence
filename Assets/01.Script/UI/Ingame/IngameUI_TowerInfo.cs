using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class IngameUI_TowerInfo : UIBase
{
    [System.Serializable]
    public class TAEInfo
    {
        public RectTransform rtf;
        public Image icon;
        public TextMeshProUGUI txt;
        public TowerAdditionalEffect tae;
    }

    [SerializeField]
    private TextMeshProUGUI m_NameTxt;
    public TextMeshProUGUI nameTxt => m_NameTxt;

    [SerializeField]
    private Slider m_ExpGaugeBar;
    public Slider expGaugeBar { get { return m_ExpGaugeBar; } }

    [SerializeField]
    private TextMeshProUGUI m_ExpGaugeBarTxt;
    public TextMeshProUGUI expGaugeBarTxt { get { return m_ExpGaugeBarTxt; } }

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
    private GameObject m_AbilityPanel;
    public GameObject abilityPannel { get { return m_AbilityPanel; } }

    [SerializeField]
    private List<TAEInfo> m_TowerAdditionalEffectInfos;
    public List<TAEInfo> towerAdditionalEffectInfos { get { return m_TowerAdditionalEffectInfos; } }

    [SerializeField]
    private CustomButton m_OpenSelectTAEBtn;
    public CustomButton openSelectTAEBtn { get { return m_OpenSelectTAEBtn; } }

    [SerializeField]
    private CustomButton m_SellTowerBtn;
    public CustomButton sellTowerBtn { get { return m_SellTowerBtn; } }

    [SerializeField]
    private TextMeshProUGUI m_TowerAdditionalEffectExplainTxt;
    public TextMeshProUGUI towerAdditionalEffectExplainTxt { get { return m_TowerAdditionalEffectExplainTxt; } }

    [SerializeField]
    private CustomButton m_TowerAdditionalEffectSelectFrameBtn;
    public CustomButton towerAdditionalEffectSelectFrameBtn { get { return m_TowerAdditionalEffectSelectFrameBtn; } }

    [SerializeField]
    private IngameUI_TowerInfo_SelectTAE m_SelectTAEUI;
    public IngameUI_TowerInfo_SelectTAE selectTAEUI { get { return m_SelectTAEUI; } }

    public GridBlock_Tower targetTower { get; private set; }

    public float autoUpdateTime = 0.5f;
    private float timer;

    private void Awake()
    {
        openSelectTAEBtn.OnClick += OnClickOpenSelectTAEBtn;
        sellTowerBtn.OnClick += OnClickSellTowerBtn;

        towerAdditionalEffectSelectFrameBtn.OnClick += () =>
        {
            towerAdditionalEffectSelectFrameBtn.Close();
            towerAdditionalEffectExplainTxt.gameObject.SetActive(false);
            abilityPannel.SetActive(true);

            UpdateUI();
        };
    }

    private void OnClickOpenSelectTAEBtn()
    {
        if (targetTower.GetAdditionalEffectCount() < targetTower.maxAdditionalEffectCount)
        {
            selectTAEUI.SetData(this, IngameManager.Instance.randomTowerAdditionalEffects);
            selectTAEUI.Open();
            selectTAEUI.UpdateUI();
        }
        else
        {
            UIManager.Instance.ShowNoticeUIByTexTNo("00007");  
        }
    }

    private void OnClickSellTowerBtn()
    {
        UIManager.Instance.messageBoxUI.SetData
            ( "타워 판매"
            , string.Format("타워를 <b><color={0}>{1}(G)</color></b>에 판매하시겠습니까?", Global.COLOR_CODE_COST, targetTower.GetSellCost())
            , "네"
            , () => {
                UIManager.Instance.messageBoxUI.Close();
                if(!targetTower.ownerGrid.SellTower())
                {
                    UIManager.Instance.ShowNoticeUI("타워를 판매할 수 없습니다.");
                }
                UIManager_Ingame.Instance.gridInfoUI.Close();
                IngameManager.Instance.UnselectGridBlock();
            }
            , "아니오"
            , () => { UIManager.Instance.messageBoxUI.Close(); });

        UIManager.Instance.messageBoxUI.Open();
    }

    public void OnClickTowerAdditionalEffectSelectFrame(int idx)
    {
        var taeInfo = towerAdditionalEffectInfos[idx];

        towerAdditionalEffectSelectFrameBtn.transform.SetParent(taeInfo.rtf);
        towerAdditionalEffectSelectFrameBtn.transform.position = taeInfo.rtf.transform.position;
        towerAdditionalEffectSelectFrameBtn.gameObject.SetActive(true);

        var name = UITextManager.GetText(string.Format("TAE_{0}_Name", (int)taeInfo.tae.effectType));
        var exp = UITextManager.GetText(string.Format("TAE_{0}_Exp", (int)taeInfo.tae.effectType));

        var skillInfo = UserInfo.Instance.skillInv.GetHighestLevelSkillIDInfo((int)taeInfo.tae.effectType);
        var subExp = UITextManager.GetText(string.Format("{0}_Exp", skillInfo.skillId));
        
        nameTxt.text = string.Format("<style=\"TAEName\">{0}</style>", name);

        if(skillInfo.level == 0)
        {
            towerAdditionalEffectExplainTxt.text = string.Format("{0}", exp);
        }
        else
        {
            towerAdditionalEffectExplainTxt.text = string.Format("{0}\n\r{1}", exp, subExp);
        }

        expGaugeBar.gameObject.SetActive(false);
        expGaugeBar.value = 0;
        expGaugeBarTxt.text = string.Empty;

        towerAdditionalEffectExplainTxt.gameObject.SetActive(true);
        abilityPannel.SetActive(false);
    }

    public void SetData(GridBlock_Tower tower)
    {
        targetTower = tower;
    }

    public void UpdateUI()
    {
        if (targetTower != null && !towerAdditionalEffectExplainTxt.gameObject.activeInHierarchy)
        {
            // 타워 이름
            var towerName = UITextManager.GetText(string.Format("{0}_Name", targetTower.data.towerID));
            nameTxt.text = string.Format("{0} <font=\"BMHANNA_11yrs_ttf_OnlyNumbers\"><color=yellow>Lv.{1}</color><size=70%>  <color=red>({2}kill)</color></size></font>", towerName, targetTower.level, targetTower.killCount);

            expGaugeBar.gameObject.SetActive(true);

            if(targetTower.isMaxLevel)
            {
                expGaugeBar.minValue = 0;
                expGaugeBar.maxValue = 1;
                expGaugeBar.value = 1;
                expGaugeBarTxt.text = "MAX";
            }
            else
            {
                expGaugeBar.minValue = 0;
                expGaugeBar.maxValue = targetTower.maxExpPoint;
                expGaugeBar.value = targetTower.expPoint;
                var preMaxLevelUpExpPoint = GameSettingsManager.GetLevelUpExpPoint(targetTower.level - 1);
                expGaugeBarTxt.text = string.Format("{0}/{1}", targetTower.expPoint - preMaxLevelUpExpPoint, targetTower.maxExpPoint - preMaxLevelUpExpPoint);
            }

            // 능력치
            damageTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">{1}</style>", UITextManager.GetText("00001"), targetTower.damage);
            attackRangeTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">{1}</style>", UITextManager.GetText("00002"), targetTower.attackRange);
            attackSpeedTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">{1}/s</style>", UITextManager.GetText("00003"), targetTower.attackSpeed);
            criticalPercTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">{1}%</style>", UITextManager.GetText("00004"), targetTower.criticalPerc * 100);
            criticalDamageRatioTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">{1}%</style>", UITextManager.GetText("00005"), targetTower.criticalDamageRatio * 100);
            projectileCountTxt.text = string.Format("<style=\"AbilityLabel\">{0}</style>\n\r<style=\"AbilityValue\">x {1}</style>", UITextManager.GetText("00006"), targetTower.projectileCount);
            
            for (int i = 0; i < targetTower.GetAdditionalEffectCount(); ++i)
            {
                TowerAdditionalEffect tae;
                if (targetTower.TryGetAdditionalEffect(i, out tae))
                {
                    Sprite icon;
                    if (ResourceManager.Instance.TryGetSprite(tae.spriteKey, out icon))
                    {
                        towerAdditionalEffectInfos[i].icon.sprite = icon;
                    }
                    else
                    {
                        towerAdditionalEffectInfos[i].icon.sprite = null;
                    }

                    towerAdditionalEffectInfos[i].tae = tae;
                    towerAdditionalEffectInfos[i].icon.gameObject.SetActive(true);
                    
                    SkillData skillData;
                    if(DataManager.Instance.TryGetSkillData(UserInfo.Instance.skillInv.GetHighestLevelSkillID((int)tae.effectType), out skillData))
                    {
                        switch(skillData.subData)
                        {
                            case "isDouble": towerAdditionalEffectInfos[i].txt.text = (tae.isDouble) ? "Double" : string.Empty; break;
                            case "isTriple": towerAdditionalEffectInfos[i].txt.text = (tae.isTriple) ? "Triple" : string.Empty; break;
                            case "isQuadruple": towerAdditionalEffectInfos[i].txt.text = (tae.isQuadruple) ? "Quadruple" : string.Empty; break;
                            default: towerAdditionalEffectInfos[i].txt.text = string.Empty; break;
                        }
                        //Debug.LogFormat("<color=red>{0} / {1}</color>", UserInfo.Instance.skillInv.GetHighestLevelSkillID((int)tae.effectType), skillData.subData);
                    }
                    else
                    {
                        towerAdditionalEffectInfos[i].txt.text = string.Empty;
                    }

                    var txtColor = towerAdditionalEffectInfos[i].txt.color;
                    towerAdditionalEffectInfos[i].txt.color = new Color(txtColor.r, txtColor.g, txtColor.b, 1);
                    towerAdditionalEffectInfos[i].txt.DOColor(new Color(txtColor.r, txtColor.g, txtColor.b, 0), 2).SetEase(Ease.InFlash, 2).SetLoops(-1).Restart();
                }
                else
                {
                    towerAdditionalEffectInfos[i].icon.sprite = null;
                    towerAdditionalEffectInfos[i].tae = null;
                    towerAdditionalEffectInfos[i].icon.gameObject.SetActive(false);
                    towerAdditionalEffectInfos[i].txt.text = string.Empty;
                }
            }

            for (int j = targetTower.GetAdditionalEffectCount(); j < towerAdditionalEffectInfos.Count; ++j)
            {
                towerAdditionalEffectInfos[j].icon.sprite = null;
                towerAdditionalEffectInfos[j].tae = null;
                towerAdditionalEffectInfos[j].icon.gameObject.SetActive(false);
                towerAdditionalEffectInfos[j].txt.text = string.Empty;
            }

            if(targetTower.GetAdditionalEffectCount() < targetTower.maxAdditionalEffectCount)
            {
                openSelectTAEBtn.image.material = openSelectTAEBtn.label.material = null;
            }
            else
            {
                openSelectTAEBtn.image.material = openSelectTAEBtn.label.material = ResourceManager.GetSpriteGrayScaleMaterial();
            }
        }
    }

    public override void Open()
    {
        if (targetTower == null)
        {
            Debug.LogErrorFormat("{0} : targetTower is null reference.", GetType());
            return;
        }

        timer = autoUpdateTime;

        selectTAEUI.Close();

        towerAdditionalEffectSelectFrameBtn.Close();
        towerAdditionalEffectExplainTxt.gameObject.SetActive(false);
        abilityPannel.SetActive(true);

        UpdateUI();

        base.Open();
    }

    private void Update()
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            timer = autoUpdateTime;
            UpdateUI();
        }
    }

    public override void Close()
    {
        base.Close();

        targetTower = null;
    }
}