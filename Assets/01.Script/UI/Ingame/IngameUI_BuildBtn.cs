using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameUI_BuildBtn : CustomButton
{
    [SerializeField]
    private Image m_CostGaugeBar;
    public Image costGaugeBar { get { return m_CostGaugeBar; } }

    public void UpdateBuildCost(int buildCost)
    {
        //label.text = buildCost.ToString();
    }

    public void UpdateGaugeBar(float fillAmount)
    {
        costGaugeBar.fillAmount = fillAmount;
    }
}
