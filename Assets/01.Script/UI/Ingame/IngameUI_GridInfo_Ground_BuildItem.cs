using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameUI_GridInfo_Ground_BuildItem : CustomButton
{
    [SerializeField]
    private Image m_TowerIcon;
    public Image towerIcon => m_TowerIcon;

    public IngameUI_GridInfo_Ground parentUI { get; private set; }
    public int idx { get; private set; }
    public TowerData data { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        OnClick += OnClickItem;
    }

    public void SetData(IngameUI_GridInfo_Ground parentUI, int idx, TowerData data)
    {
        this.parentUI = parentUI;
        this.idx = idx;
        this.data = data;
    }

    public void UpdateUI()
    {
        towerIcon.sprite = ResourceManager.Instance.GetSprite(data.towerIconKey);
        label.text = data.buildCost.ToString();
    }

    public void OnClickItem()
    {
        parentUI.OpenTowerInfoUI(idx);
    }
}
