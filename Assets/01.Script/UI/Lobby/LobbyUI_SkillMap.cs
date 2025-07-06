using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI_SkillMap : UIBase
{
    public UINodeGraph[] nodes { get; private set; }

    [SerializeField]
    private RectTransform m_ForeCanvasUIRtf;
    public RectTransform foreCanvasUIRtf { get { return m_ForeCanvasUIRtf; } }

    [SerializeField]
    private CustomButton m_CloseBtn;
    public CustomButton closeBtn { get { return m_CloseBtn; } }

    [SerializeField]
    private GameObject m_SkillMapLine;
    public GameObject skillMapLine { get { return m_SkillMapLine; } }

    [SerializeField]
    private ScrollRect m_SkillMapScrollRect;
    public ScrollRect skillMapScrollRect { get { return m_SkillMapScrollRect; } }
    
    [SerializeField]
    private RectTransform m_SelectedSkillFrameRtf;
    public RectTransform selectedSkillFrameRtf { get { return m_SelectedSkillFrameRtf; } }

    public UINodeGraph selectedSkillNode { get; private set; }

    [SerializeField]
    private LobbyUI_SkillMap_SkillInfo m_SkillInfoUI;
    public LobbyUI_SkillMap_SkillInfo skillInfoUI { get { return m_SkillInfoUI; } }

    [SerializeField]
    private UINodeGraph m_TutorialNode;
    public UINodeGraph tutorialNode => m_TutorialNode;

    [SerializeField]
    private CustomButton m_ResetAllSkillBtn;
    public CustomButton resetAllSkillBtn => m_ResetAllSkillBtn;

    private void Awake()
    {
        closeBtn.OnClick += Close;
        resetAllSkillBtn.OnClick += OnClickAllSkillBtn;
        resetAllSkillBtn.label.text = string.Format(UITextManager.GetText("버튼/스킬초기화"), GameSettingsManager.ResetAllSkillPrice);

        skillMapScrollRect.onValueChanged.AddListener(OnSkillMapScroll);
    }

    public void OnClickAllSkillBtn()
    {
        if (UserInfo.Instance.starCount >= GameSettingsManager.ResetAllSkillPrice)
        {
            // 정말로 초기화 하시겠습니까?
            UIManager.ShowMessageBoxUI
                (UITextManager.GetText("알림")
                , string.Format(UITextManager.GetText("알림/메시지/스킬초기화"), GameSettingsManager.ResetAllSkillPrice)
                , UITextManager.GetText("네")
                , () =>
                {
                    UIManager.GetMessageBoxUI().Close();

                    // 이미 구매한 스킬 가격 계산
                    int totalSkillCost = 0;
                    var skillInv = UserInfo.Instance.skillInv.skillInventory;
                    foreach (var id in skillInv)
                    {
                        SkillData sData;
                        if (DataManager.Instance.TryGetSkillData(id.Key, out sData))
                        {
                            totalSkillCost += sData.starCost;
                        }
                    }

                    // 초기화 가격만큼 별 소모
                    UserInfo.Instance.AddStarCount(-GameSettingsManager.ResetAllSkillPrice, false, Vector3.zero);

                    // 스킬 구매에 사용되었던 별 지급
                    UserInfo.Instance.AddStarCount(totalSkillCost, false, Vector3.zero);

                    // 모든 스킬 초기화
                    UserInfo.Instance.skillInv.ResetSkillInv();
                    UserInfo.Instance.InitializeSkillInventory();

                    Close();

                    UnityEngine.Analytics.AnalyticsEvent.Custom("user_action_1", new Dictionary<string, object>
                    {
                        { "reset_all_skill" , string.Empty }
                    });

                    // 초기화 결과 메시지
                    UIManager.ShowMessageBoxUI
                    ( UITextManager.GetText("알림")
                    , string.Format(UITextManager.GetText("알림/메시지/스킬초기화완료"), totalSkillCost)
                    , UITextManager.GetText("확인")
                    , UIManager.GetMessageBoxUI().Close);
                }
                , UITextManager.GetText("아니오")
                , UIManager.GetMessageBoxUI().Close);
        }
        else
        {
            // 별 부족
            UIManager.ShowMessageBoxUI
                ( UITextManager.GetText("알림")
                , UITextManager.GetText("알림/메시지/별부족")
                , UITextManager.GetText("확인")
                , UIManager.GetMessageBoxUI().Close);
        }
    }

    public void OnSkillMapScroll(Vector2 pos)
    {
        UnselectSkill();
    }

    public override void Open()
    {
        skillMapScrollRect.normalizedPosition = new Vector2(0, 0);

        UnselectSkill();

        base.Open();

        foreCanvasUIRtf.gameObject.SetActive(true);
        skillMapLine.SetActive(true);
    }

    public void UpdateUI(bool initSelect = false)
    {
        if (nodes == null)
        {
            nodes = foreCanvasUIRtf.GetComponentsInChildren<UINodeGraph>();
        }

        foreach (var n in nodes)
        {
            SkillData skillData;
            if (DataManager.Instance.TryGetSkillData(n.skillID, out skillData))
            {
                n.SetData(skillData, UserInfo.Instance.skillInv.GetActiveSkill(skillData.skillID));
                n.Open();
                n.UpdateUI();

                n.OnClickNode = null;
                n.OnClickNode += SelectSkill;
            }
        }
        skillMapLine.transform.SetParent(skillMapScrollRect.content.transform);

        if(initSelect)
        {
            UnselectSkill();
            SelectSkill(nodes[0]);
        }
    }

    public override void Close()
    {
        UnselectSkill();

        skillMapLine.SetActive(false);
        foreCanvasUIRtf.gameObject.SetActive(false);

        base.Close();
        
        skillMapLine.transform.SetParent(null);
    }

    public void SelectSkill(UINodeGraph nodeGraph)
    {
        if(selectedSkillNode != null && selectedSkillNode.skillID.Equals(nodeGraph.skillID))
        {
            UnselectSkill();
        }
        else
        {
            UnselectSkill();

            selectedSkillNode = nodeGraph;

            selectedSkillFrameRtf.SetParent(nodeGraph.transform);
            selectedSkillFrameRtf.anchoredPosition3D = Vector3.zero;
            selectedSkillFrameRtf.localScale = Vector3.one;
            selectedSkillFrameRtf.gameObject.SetActive(true);

            skillInfoUI.SetData(this, nodeGraph);
            skillInfoUI.Open();
            skillInfoUI.UpdateUI();
        }
    }

    public void UnselectSkill()
    {
        skillInfoUI.Close();

        selectedSkillNode = null;

        selectedSkillFrameRtf.gameObject.SetActive(false);
    }
}
