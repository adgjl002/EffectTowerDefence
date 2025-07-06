using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillIconUI : UIBase
{
    [SerializeField]
    private Image m_Icon;
    public Image icon { get { return m_Icon; } }

    [SerializeField]
    private TextMeshProUGUI m_LevelTxt;
    public TextMeshProUGUI levelTxt { get { return m_LevelTxt; } }

    [SerializeField]
    private TextMeshProUGUI m_CostTxt;
    public TextMeshProUGUI costTxt { get { return m_CostTxt; } }

    public SkillData data { get; private set; }
    public bool isActivated { get; private set; }

    public void SetData(SkillData data, bool isActivated = true)
    {
        this.data = data;
        this.isActivated = isActivated;
    }

    public void UpdateUI()
    {
        icon.sprite = ResourceManager.Instance.GetSprite(data.spriteName);

        costTxt.text = string.Format("<sprite=0> {0}", data.starCost);

        levelTxt.text = Helper.ConvertToRomanNumeral(data.level);
        levelTxt.color = new Color(0, 1, (float)1.6f / data.level, 1);

        if (isActivated)
        {
            icon.material = null;
        }
        else
        {
            icon.material = ResourceManager.GetSpriteGrayScaleMaterial();
        }
    }
}
