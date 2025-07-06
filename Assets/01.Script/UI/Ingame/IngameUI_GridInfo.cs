using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IngameUI_GridInfo : UIBase
{
    [SerializeField]
    private TextMeshProUGUI m_TitleTxt;
    public TextMeshProUGUI titleTxt { get { return m_TitleTxt; } }

    [SerializeField]
    private IngameUI_TowerInfo m_TowerInfoUI;
    public IngameUI_TowerInfo towerInfoUI { get { return m_TowerInfoUI; } }

    [SerializeField]
    private IngameUI_GridInfo_Ground m_GroundInfoUI;
    public IngameUI_GridInfo_Ground groundInfoUI { get { return m_GroundInfoUI; } }

    [SerializeField]
    private IngameUI_EnemyInfo m_EnemyInfoUI;
    public IngameUI_EnemyInfo enemyInfoUI { get { return m_EnemyInfoUI; } }

    [SerializeField]
    private IngameUI_StartInfo m_StartInfoUI;
    public IngameUI_StartInfo startInfoUI { get { return m_StartInfoUI; } }

    [SerializeField]
    private RectTransform m_MessageUIRtf;
    public RectTransform messageUIRtf { get { return m_MessageUIRtf; } }

    [SerializeField]
    private TextMeshProUGUI m_MessageUITxt;
    public TextMeshProUGUI messageUITxt { get { return m_MessageUITxt; } }

    [SerializeField]
    private CustomButton m_AddTAEBtn;
    public CustomButton addTAEBtn { get { return m_AddTAEBtn; } }

    public GridBlock_Tower targetTower { get; private set; }
    public GridBlock targetGridBlock { get; private set; }

    private void Awake()
    {
        addTAEBtn.OnClick += OnClickAddTAEBtn;
    }

    public void SetTowerInfoData(GridBlock_Tower tower)
    {
        targetTower = tower;
        messageUITxt.text = string.Empty;

        groundInfoUI.Close();
        messageUIRtf.gameObject.SetActive(false);
        enemyInfoUI.Close();
        startInfoUI.Close();

        towerInfoUI.SetData(tower);
        towerInfoUI.Open();
        towerInfoUI.UpdateUI();
    }

    public void SetMessageData(GridBlock gridBlock)
    {
        targetGridBlock = gridBlock;
        
        towerInfoUI.Close();
        enemyInfoUI.Close();
        startInfoUI.Close();

        switch (gridBlock.gridBlockType)
        {
            case GridBlock.EType.None:
                titleTxt.text = UITextManager.GetText("Grid_None_Title");
                messageUITxt.text = UITextManager.GetText("Grid_None_Exp");
                messageUIRtf.gameObject.SetActive(true);
                groundInfoUI.Close();
                break;

            case GridBlock.EType.Wall:
                var wall = (gridBlock as GridBlock_Wall);
                titleTxt.text = UITextManager.GetText("Grid_Wall_Title");
                messageUITxt.text = string.Format(UITextManager.GetText("Grid_Wall_Exp"), wall.nowHp, wall.maxHp);
                messageUIRtf.gameObject.SetActive(true);
                groundInfoUI.Close();
                break;

            case GridBlock.EType.Ground:
                titleTxt.text = UITextManager.GetText("Grid_Ground_Title");
                messageUITxt.text = UITextManager.GetText("Grid_Ground_Exp");
                messageUIRtf.gameObject.SetActive(false);
                
                List<string> towerKeys = new List<string>();
                if (UserInfo.Instance.skillInv.GetActiveSkill("Skill_TAE_101_1_0")) towerKeys.Add("TD-1");
                if (UserInfo.Instance.skillInv.GetActiveSkill("Skill_TAE_102_1_0")) towerKeys.Add("TD-2");
                if (UserInfo.Instance.skillInv.GetActiveSkill("Skill_TAE_103_1_0")) towerKeys.Add("TD-3");
                if (UserInfo.Instance.skillInv.GetActiveSkill("Skill_TAE_104_1_0")) towerKeys.Add("TD-4");
                if (UserInfo.Instance.skillInv.GetActiveSkill("Skill_TAE_105_1_0")) towerKeys.Add("TD-5");
                if (UserInfo.Instance.skillInv.GetActiveSkill("Skill_TAE_106_1_0")) towerKeys.Add("TD-6");

                groundInfoUI.SetData(this, towerKeys);
                groundInfoUI.Open();
                groundInfoUI.UpdateUI();
                break;

            case GridBlock.EType.StartPoint:
                titleTxt.text = UITextManager.GetText("Grid_StartPoint_Title");
                messageUITxt.text = UITextManager.GetText("Grid_StartPoint_Exp");
                messageUIRtf.gameObject.SetActive(true);
                groundInfoUI.Close();
                break;

            case GridBlock.EType.EndPoint:
                titleTxt.text = UITextManager.GetText("Grid_EndPoint_Title");
                messageUITxt.text = string.Format(UITextManager.GetText("Grid_EndPoint_Exp"), IngameManager.Instance.curLife);
                messageUIRtf.gameObject.SetActive(true);
                groundInfoUI.Close();
                break;

            case GridBlock.EType.WayPoint:
                titleTxt.text = UITextManager.GetText("Grid_WayPoint_Title");
                messageUITxt.text = UITextManager.GetText("Grid_WayPoint_Exp");
                messageUIRtf.gameObject.SetActive(true);
                groundInfoUI.Close();
                break;
        }
    }

    public void SetEnemyInfoData(IngameObject iObj)
    {
        string className;
        switch(iObj.unitClass)
        {
            default:
            case IngameObject.EClass.Normal:
                className = string.Format("<color={0}>{1}<size=90%>({2})</size></color>", Global.COLOR_CODE_UNIT_CLASS_NORMAL, UITextManager.GetText(iObj.prefabKey + "_Name"), UITextManager.GetText("일반"));
                break;

            case IngameObject.EClass.Hero:
                className = string.Format("<color={0}>{1}<size=90%>({2})</size></color>", Global.COLOR_CODE_UNIT_CLASS_HERO, UITextManager.GetText(iObj.prefabKey + "_Name"), UITextManager.GetText("영웅"));
                break;

            case IngameObject.EClass.Boss:
                className = string.Format("<color={0}>{1}<size=90%>({2})</size></color>", Global.COLOR_CODE_UNIT_CLASS_BOSS, UITextManager.GetText(iObj.prefabKey + "_Name"), UITextManager.GetText("보스"));
                break;
        }
        titleTxt.text = string.Format("{0} <font=\"BMHANNA_11yrs_ttf_OnlyNumbers\"><color=yellow>Lv.{1}</color></font>", className, iObj.data.level);
        
        groundInfoUI.Close();
        messageUIRtf.gameObject.SetActive(false);
        towerInfoUI.Close();
        startInfoUI.Close();

        enemyInfoUI.SetData(iObj);
        enemyInfoUI.Open();
        enemyInfoUI.UpdateUI();
    }

    public void SetStartInfoData(int curWave)
    {
        titleTxt.text = "Start";

        towerInfoUI.Close();
        groundInfoUI.Close();
        enemyInfoUI.Close();
        messageUIRtf.gameObject.SetActive(false);

        startInfoUI.SetData(curWave);
        startInfoUI.Open();
        startInfoUI.UpdateUI();
    }

    private void OnClickAddTAEBtn()
    {

    }

    public override void Close()
    {
        towerInfoUI.Close();
        groundInfoUI.Close();
        enemyInfoUI.Close();
        messageUIRtf.gameObject.SetActive(false);
        startInfoUI.Close();

        targetTower = null;
        targetGridBlock = null;

        base.Close();
    }
}
