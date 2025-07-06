using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public enum ERewardType
{
    Star,
    Life,
    Coin,
    Skill
}

public struct RewardData
{
    public ERewardType rewardType;
    public string rewardContents;

    public int ConetentsAsInt()
    {
        int amount = 0;
        if(!int.TryParse(rewardContents, out amount))
        {
            Debug.LogErrorFormat("RewardData Contents({0}) can't parse to Int", rewardContents);
        }
        return amount;
    }
}

public class UIManager_Common_TakeRewardUI : CustomButton
{
    [SerializeField]
    private Image m_RewardImg;
    public Image rewardImg { get { return m_RewardImg; } }

    [SerializeField]
    private SkillIconUI m_SkillIconUI;
    public SkillIconUI skillIconUI { get { return m_SkillIconUI; } }

    [SerializeField]
    private TextMeshProUGUI m_MessageTxt;
    public TextMeshProUGUI messageTxt { get { return m_MessageTxt; } }

    public RewardData data { get; private set; }

    public void SetData(RewardData data)
    {
        this.data = data;
    }
    
    public override void Open()
    {
        OnClick += OnClickUI;

        base.Open();
    }

    public override void Close()
    {
        base.Close();

        OnClick -= OnClickUI;
    }

    public void UpdateUI()
    {
        switch(data.rewardType)
        {
            case ERewardType.Coin:
                skillIconUI.Close();
                rewardImg.gameObject.SetActive(true);
                rewardImg.sprite = ResourceManager.Instance.GetSprite("Icon_Coin");
                messageTxt.text = string.Format(UITextManager.GetText("00029"), data.ConetentsAsInt());
                break;

            case ERewardType.Life:
                skillIconUI.Close();
                rewardImg.gameObject.SetActive(true);
                rewardImg.sprite = ResourceManager.Instance.GetSprite("Icon_Life");
                messageTxt.text = string.Format(UITextManager.GetText("00029"), data.ConetentsAsInt());
                break;

            case ERewardType.Star:
                skillIconUI.Close();
                rewardImg.gameObject.SetActive(true);
                rewardImg.sprite = ResourceManager.Instance.GetSprite("Icon_Star");
                messageTxt.text = string.Format(UITextManager.GetText("00029"), data.ConetentsAsInt());
                break;

            case ERewardType.Skill:
                SkillData sData;
                if(DataManager.Instance.TryGetSkillData(data.rewardContents, out sData))
                {
                    skillIconUI.SetData(sData);
                    skillIconUI.Open();
                    skillIconUI.UpdateUI();
                }
                else
                {
                    skillIconUI.Close();
                }
                rewardImg.gameObject.SetActive(false);
                messageTxt.text = string.Format(UITextManager.GetText("00029"), UITextManager.GetText("data.skillID"));
                break;

            default:
                skillIconUI.Close();
                rewardImg.gameObject.SetActive(false);
                messageTxt.text = string.Empty;
                break;
        }
    }

    public void OnClickUI()
    {
        Close();
    }
}
