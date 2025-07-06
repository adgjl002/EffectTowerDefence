using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IngameUI_Top : UIBase {

    [SerializeField]
    private TextMeshProUGUI m_TopStageTxt;
    public TextMeshProUGUI topStageTxt { get { return m_TopStageTxt; } }

    [SerializeField]
    private IngameUI_Cost m_CostUI;
    public IngameUI_Cost costUI { get { return m_CostUI; } }

    [SerializeField]
    private IngameUI_Life m_LifeUI;
    public IngameUI_Life lifeUI { get { return m_LifeUI; } }

    public override void Open()
    {
        base.Open();

        costUI.UpdateCost(IngameManager.Instance.curCost);
        costUI.Open();

        lifeUI.SetData(IngameManager.Instance.curLife);
        lifeUI.UpdateUI();
        lifeUI.Open();
    }

    public override void Close()
    {
        costUI.Close();
        lifeUI.Close();

        base.Close();
    }

    public void ClearStageTxt()
    {
        topStageTxt.text = string.Empty;
    }

    public void SetStageTxt(int stageNo, int waveNo)
    {
        topStageTxt.text = string.Format("<size={0}><b>Stage {1}</b></size>\n\rWave {2}", topStageTxt.fontSize + 10, stageNo + 1, waveNo);
    }
}
