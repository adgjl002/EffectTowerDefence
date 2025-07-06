using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI_SkillMap_SkillInfo : UIBase
{
    public LobbyUI_SkillMap parentUI { get; private set; }
    public UINodeGraph nodeGraph { get; private set; }

    [SerializeField]
    private TextMeshProUGUI m_TitleTxt;
    public TextMeshProUGUI titleTxt { get { return m_TitleTxt; } }
    
    [SerializeField]
    private CustomButton m_BuyBtn;
    public CustomButton buyBtn { get { return m_BuyBtn; } }

    [SerializeField]
    private TextMeshProUGUI m_BuyStarCostTxt;
    public TextMeshProUGUI buyStarCostTxt { get { return m_BuyStarCostTxt; } }

    [SerializeField]
    private List<SkillIconUI> m_NeedSkillIconUIs;
    public List<SkillIconUI> needSkillIconUIs { get { return m_NeedSkillIconUIs; } }

    [SerializeField]
    private TextMeshProUGUI m_NeedSkillMsg;
    public TextMeshProUGUI needSkillMsg { get { return m_NeedSkillMsg; } }

    [SerializeField]
    private TextMeshProUGUI m_SkillExplainTxt;
    public TextMeshProUGUI skillExplainTxt { get { return m_SkillExplainTxt; } }

    [SerializeField]
    private RectTransform m_NeedStageIdRtf;
    public RectTransform needStageIdRtf => m_NeedStageIdRtf;

    [SerializeField]
    private TextMeshProUGUI m_NeedStageIdTxt;
    public TextMeshProUGUI needStageIdTxt => m_NeedStageIdTxt;

    private void Awake()
    {
        buyBtn.OnClick += OnClickBuyBtn;
    }

    public void OnClickBuyBtn()
    {
        bool isCanActive = false;
        bool isActivated = UserInfo.Instance.skillInv.GetActiveSkill(nodeGraph.data.skillID);

        if(string.IsNullOrEmpty(nodeGraph.data.needStageID))
        {
            isCanActive = UserInfo.Instance.skillInv.CheckCanActiveSkill(nodeGraph.data);
        }
        else
        {
            isCanActive = UserInfo.Instance.skillInv.CheckCanActiveSkill(nodeGraph.data) && !UserInfo.Instance.CheckLockStage(DataManager.Instance.GetStageIdxById(nodeGraph.data.needStageID));
        }

        if(isActivated)
        {
            UIManager.Instance.ShowNoticeUIByTexTNo("00012");
            return;
        }
        else if(!isCanActive)
        {
            if(!UserInfo.Instance.skillInv.CheckNoNeedSkillCondition(nodeGraph.data))
            {
                UIManager.Instance.ShowNoticeUIByTexTNo("00042");
            }
            else
            {
                UIManager.Instance.ShowNoticeUIByTexTNo("00011");
            }
            return; 
        }
        else if (UserInfo.Instance.starCount >= nodeGraph.data.starCost)
        {
            UserInfo.Instance.AddStarCount(-nodeGraph.data.starCost, false, Vector3.zero);
            UserInfo.Instance.skillInv.SetActiveSkill(nodeGraph.data.skillID, true);
            UserInfo.Save(UserInfo.Instance);

            parentUI.UpdateUI();

            UIManager.Instance.ShowNoticeUIByTexTNo("00014");

            Close();
        }
        else
        {
            UIManager.Instance.ShowNoticeUIByTexTNo("00013");
            return;
        }
    }

    public void SetData(LobbyUI_SkillMap parentUI, UINodeGraph nodeGraph)
    {
        this.parentUI = parentUI;
        this.nodeGraph = nodeGraph;
    }

    public void UpdateUI()
    {
        titleTxt.text = UITextManager.GetText(nodeGraph.data.skillID);

        if(!string.IsNullOrEmpty(nodeGraph.data.needStageID))
        {
            var ids = nodeGraph.data.needStageID.Split('_');
            if (ids.Length > 1)
            {
                needStageIdRtf.gameObject.SetActive(true);
                needStageIdTxt.text = string.Format("<size=75%>STAGE</size>  <color=#5DB5FF>{0}</color>", ids[1]);
            }
            else
            {
                needStageIdRtf.gameObject.SetActive(false);
                needStageIdTxt.text = string.Empty;
            }
        }
        else
        {
            needStageIdRtf.gameObject.SetActive(false);
            needStageIdTxt.text = string.Empty;
        }

        for (int i = 0; i<needSkillIconUIs.Count; ++i)
        {
            SkillData needSKillData;
            if (nodeGraph.data.needSkillIDs != null && nodeGraph.data.needSkillIDs.Length > i && DataManager.Instance.TryGetSkillData(nodeGraph.data.needSkillIDs[i], out needSKillData))
            {
                needSkillIconUIs[i].SetData(needSKillData);
                needSkillIconUIs[i].Open();
                needSkillIconUIs[i].UpdateUI();
            }
            else
            {
                needSkillIconUIs[i].Close();
            }
        }

        if(nodeGraph.data.needSkillIDs != null && nodeGraph.data.needSkillIDs.Length > 0)
        {
            switch (nodeGraph.data.needSkillIDsOperator)
            {
                case 0:
                    needSkillMsg.text = UITextManager.GetText("00008");
                    break;

                case 1:
                    needSkillMsg.text = UITextManager.GetText("00009").Replace("[data1]", needSkillIconUIs.Count.ToString());
                    break;

                default:
                    Debug.LogErrorFormat("Need Skill IDS Operator({0}) is not implemented.", nodeGraph.data.needSkillIDsOperator);
                    needSkillMsg.text = UITextManager.GetText("00010");
                    break;
            }
        }
        else
        {
            needSkillMsg.text = UITextManager.GetText("00010");
        }

        if(nodeGraph.data.level == 0)
        {
            var splits = nodeGraph.data.skillID.Split('_');
            skillExplainTxt.text = UITextManager.GetText(string.Format("{0}_{1}_Exp", splits[1], splits[2]));
        }
        else
        {
            skillExplainTxt.text = UITextManager.GetText(nodeGraph.data.skillID + "_Exp");
        }

        bool isCanActive = false;
        if (string.IsNullOrEmpty(nodeGraph.data.needStageID))
        {
            isCanActive = UserInfo.Instance.skillInv.CheckCanActiveSkill(nodeGraph.data);
        }
        else
        {
            isCanActive = UserInfo.Instance.skillInv.CheckCanActiveSkill(nodeGraph.data) && !UserInfo.Instance.CheckLockStage(DataManager.Instance.GetStageIdxById(nodeGraph.data.needStageID));
        }

        bool isActivated = UserInfo.Instance.skillInv.GetActiveSkill(nodeGraph.data.skillID);
        if(isActivated)
        {
            buyBtn.Close();
            buyBtn.label.text = string.Empty;
            buyStarCostTxt.text = string.Empty;
            buyStarCostTxt.material = buyBtn.image.material = buyBtn.label.material = ResourceManager.GetSpriteGrayScaleMaterial();
        }
        else if(isCanActive)
        {
            buyBtn.Open();
            buyBtn.label.text = UITextManager.GetText("00015");
            buyStarCostTxt.text = string.Format("<size=100%><sprite=0></size> {0}", nodeGraph.data.starCost);
            buyStarCostTxt.material = buyBtn.image.material = buyBtn.label.material = null;
        }
        else
        {
            buyBtn.Open();
            buyBtn.label.text = UITextManager.GetText("00015");
            buyStarCostTxt.text = string.Format("<size=100%><sprite=0></size> {0}", nodeGraph.data.starCost);
            buyStarCostTxt.material = buyBtn.image.material = buyBtn.label.material = ResourceManager.GetSpriteGrayScaleMaterial();
        }
    }
}
