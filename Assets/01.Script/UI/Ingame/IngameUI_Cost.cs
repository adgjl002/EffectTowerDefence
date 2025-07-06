using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngameUI_Cost : UIBase {

    [SerializeField]
    private Image m_CostIcon;
    public Image costIcon { get { return m_CostIcon; } }

    [SerializeField]
    private TextMeshProUGUI m_AmountLabel;
    public TextMeshProUGUI amountLabel { get { return m_AmountLabel; } }

    public void UpdateCost(int cost)
    {
        amountLabel.text = cost.ToString();
    }
}
