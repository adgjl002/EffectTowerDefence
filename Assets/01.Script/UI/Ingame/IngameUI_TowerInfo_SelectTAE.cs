using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class IngameUI_TowerInfo_SelectTAE : UIBase
{
    [System.Serializable]
    public class TAEInfo
    {
        [SerializeField]
        private CustomButton m_Btn;
        public CustomButton btn { get { return m_Btn; } }

        [SerializeField]
        private Image m_Icon;
        public Image icon { get { return m_Icon; } }

        [SerializeField]
        private TextMeshProUGUI m_CostTxt;
        public TextMeshProUGUI costTxt { get { return m_CostTxt; } }
    }
    
    [SerializeField]
    private TextMeshProUGUI m_NameTxt;
    public TextMeshProUGUI nameTxt { get { return m_NameTxt; } }
    
    [SerializeField]
    private TextMeshProUGUI m_MessageTxt;
    public TextMeshProUGUI messageTxt { get { return m_MessageTxt; } }

    [SerializeField]
    private CustomButton m_SelectedFrameBtn;
    public CustomButton selectedFrameBtn { get { return m_SelectedFrameBtn; } }

    [SerializeField]
    private CustomButton m_RefreshBtn;
    public CustomButton refreshBtn { get { return m_RefreshBtn; } }

    [SerializeField]
    private List<TAEInfo> m_TAEInfos;
    public List<TAEInfo> taeInfos { get { return m_TAEInfos; } }

    public TowerAdditionalEffect[] taes { get; private set; }

    public int curSelectedTAEIdx { get; private set; }
    public IngameUI_TowerInfo parentTowerInfoUI { get; private set; }

    private void Awake()
    {
        refreshBtn.OnClick += OnClickRefreshBtn;

        taeInfos[0].btn.OnClick += () => { SelectTAE(0); };
        taeInfos[1].btn.OnClick += () => { SelectTAE(1); };
        taeInfos[2].btn.OnClick += () => { SelectTAE(2); };

        selectedFrameBtn.OnClick += OnClickConfirmBtn;
    }

    private void OnClickRefreshBtn()
    {
        // 자원을 소모하여 다시뽑기 실행
        if (IngameManager.Instance.isCanRefreshTowerAdditionalEffects 
            && IngameManager.Instance.GetRefreshPrice() <= UserInfo.Instance.starCount)
        {
            UIManager.ShowMessageBoxUI
                (UITextManager.GetText("알림")
                , UITextManager.GetText("00031")
                , string.Format(UITextManager.GetText("00033"), IngameManager.Instance.GetRefreshPrice())
                , ()=> 
                {
                    UIManager.GetMessageBoxUI().Close();

                    // 다시 뽑기 횟수 제한 확인
                    if (++IngameManager.Instance.refreshCount >= GameSettingsManager.TAERefreshLimitCount)
                    {
                        IngameManager.Instance.isCanRefreshTowerAdditionalEffects = false;
                    }

                    IngameManager.Instance.SetRandomTowerAdditionalEffects();
                    SetData(parentTowerInfoUI, IngameManager.Instance.randomTowerAdditionalEffects);
                    Open();
                    UpdateUI();
                }
                , UITextManager.GetText("취소")
                , ()=> { UIManager.GetMessageBoxUI().Close(); });
        }
        // 자원이 부족하다면 1회에 한하여 광고 시청 후 다시뽑기 실행
        else if (IngameManager.Instance.isCanRefreshTowerAdditionalEffects)
        {
            UIManager.ShowMessageBoxUI
                (UITextManager.GetText("알림")
                , UITextManager.GetText("00034")
                , UITextManager.GetText("광고시청")
                , () =>
                {
                    UIManager.GetMessageBoxUI().Close();
                    IngameManager.Instance.isGameStopped = true;
                    AdsManager.Instance.ShowRewardedVideoAds((r) =>
                    {
                        IngameManager.Instance.isGameStopped = false;

                        UnityEngine.Analytics.AnalyticsEvent.Custom("ads_video_end", new Dictionary<string, object>
                        {
                            { "effects_refresh" , r.ToString() }
                        });

                        if (r == UnityEngine.Advertisements.ShowResult.Finished)
                        {
                            // 효과를 배치하기 전까지 다시뽑기 못하게 잠금
                            IngameManager.Instance.SetRandomTowerAdditionalEffects();
                            IngameManager.Instance.isCanRefreshTowerAdditionalEffects = false;

                            SetData(parentTowerInfoUI, IngameManager.Instance.randomTowerAdditionalEffects);
                            Open();
                            UpdateUI();
                        }
                    });
                }
                , UITextManager.GetText("취소")
                , () => { UIManager.GetMessageBoxUI().Close(); });

        }
        // 다시 뽑기를 할 수 없는 경우
        else
        {
            UIManager.Instance.ShowNoticeUIByTexTNo("알림/다시뽑기제한");
        }
    }

    private void OnClickConfirmBtn()
    {
        var selectedTAE = taes[curSelectedTAEIdx];
        if (IngameManager.Instance.curCost >= selectedTAE.cost)
        {
            IngameManager.Instance.AddCost(-selectedTAE.cost);

            parentTowerInfoUI.targetTower.RegistAdditionalEffect(selectedTAE);
            parentTowerInfoUI.UpdateUI();

            IngameManager.Instance.SetRandomTowerAdditionalEffects();
            IngameManager.Instance.InitTAERefreshCount();
        }
        else
        {
            UIManager.Instance.ShowNoticeUI("자원이 부족합니다.", 1f);
        }

        Close();
    }

    public void SetData(IngameUI_TowerInfo parentTowerInfoUI,  List<TowerAdditionalEffect> towerAdditionalEffects)
    {
        this.parentTowerInfoUI = parentTowerInfoUI;
        taes = towerAdditionalEffects.ToArray();
    }

    public void UpdateUI()
    {
        if (taes.Length < 3) return;

        SkillData skillData;
        var tae = taes[0];
        if (DataManager.Instance.TryGetSkillData(UserInfo.Instance.skillInv.GetHighestLevelSkillID((int)tae.effectType), out skillData))
        {
            if (skillData.level > 0)
            {
                var colorCode = ColorUtility.ToHtmlStringRGBA(new Color(0, 1, (float)1.6f / skillData.level, 1));
                taeInfos[0].costTxt.text = string.Format("{0} <color=#{1}>{2}</color>", tae.cost, colorCode, Helper.ConvertToRomanNumeral(skillData.level));
            }
            else taeInfos[0].costTxt.text = tae.cost.ToString();

            taeInfos[0].icon.sprite = ResourceManager.Instance.GetSprite(tae.spriteKey);
        }

        tae = taes[1];
        if (DataManager.Instance.TryGetSkillData(UserInfo.Instance.skillInv.GetHighestLevelSkillID((int)tae.effectType), out skillData))
        {
            if (skillData.level > 0)
            {
                var colorCode = ColorUtility.ToHtmlStringRGBA(new Color(0, 1, (float)1.6f / skillData.level, 1));
                taeInfos[1].costTxt.text = string.Format("{0} <color=#{1}>{2}</color>", tae.cost, colorCode, Helper.ConvertToRomanNumeral(skillData.level));
            }
            else taeInfos[1].costTxt.text = tae.cost.ToString();

            taeInfos[1].icon.sprite = ResourceManager.Instance.GetSprite(tae.spriteKey);
        }

        tae = taes[2];
        if (DataManager.Instance.TryGetSkillData(UserInfo.Instance.skillInv.GetHighestLevelSkillID((int)tae.effectType), out skillData))
        {
            if (skillData.level > 0)
            {
                var colorCode = ColorUtility.ToHtmlStringRGBA(new Color(0, 1, (float)1.6f / skillData.level, 1));
                taeInfos[2].costTxt.text = string.Format("{0} <color=#{1}>{2}</color>", tae.cost, colorCode, Helper.ConvertToRomanNumeral(skillData.level));
            }
            else taeInfos[2].costTxt.text = tae.cost.ToString();

            taeInfos[2].icon.sprite = ResourceManager.Instance.GetSprite(tae.spriteKey);
        }

        if (IngameManager.Instance.isCanRefreshTowerAdditionalEffects)
        {
            refreshBtn.image.material = null;
            refreshBtn.label.material = null;
        }
        else
        {
            refreshBtn.image.material = ResourceManager.GetSpriteGrayScaleMaterial();
            refreshBtn.label.material = ResourceManager.GetSpriteGrayScaleMaterial();
        }

        refreshBtn.label.text = string.Format
            ("<font=BMHANNA_11yrs_ttf_OnlyNumbers><color={0}><sprite=0><size=120%>{1}</size></color></font>\n\r{2}"
            , Global.COLOR_CODE_COST
            , IngameManager.Instance.GetRefreshPrice()
            , UITextManager.GetText("다시뽑기"));
    }

    public override void Open()
    {
        curSelectedTAEIdx = -1;

        selectedFrameBtn.gameObject.SetActive(false);

        base.Open();

        nameTxt.text = string.Empty;
        messageTxt.text = UITextManager.GetText("00036");
    }

    public void SelectTAE(int idx)
    {
        curSelectedTAEIdx = idx;

        var curTae = taes[idx];

        var name = UITextManager.GetText(string.Format("TAE_{0}_Name", (int)curTae.effectType));
        var exp = UITextManager.GetText(string.Format("TAE_{0}_Exp", (int)curTae.effectType));

        var skillIDInfo = UserInfo.Instance.skillInv.GetHighestLevelSkillIDInfo((int)curTae.effectType);
        var subExp = UITextManager.GetText(string.Format("{0}_Exp", skillIDInfo.skillId));

        nameTxt.text = string.Format("<style=\"TAEName\">{0}</style>", name);

        if (skillIDInfo.level == 0)
        {
            messageTxt.text = string.Format("{0}", exp);
        }
        else
        {
            messageTxt.text = string.Format("{0}\n\r{1}", exp, subExp);
        }

        selectedFrameBtn.gameObject.SetActive(true);
        selectedFrameBtn.transform.SetParent(taeInfos[idx].btn.transform);
        selectedFrameBtn.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
}
