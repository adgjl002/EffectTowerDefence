using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngameUI_Life : UIBase
{
    [SerializeField]
    private TextMeshProUGUI m_LifeTxt;
    public TextMeshProUGUI lifeTxt { get { return m_LifeTxt; } }

    [SerializeField]
    private Image m_Icon;
    public Image icon { get { return m_Icon; } }

    public GridBlock target { get; private set; }
    public int life { get; private set; }
    
    public void SetData(int life)
    {
        this.life = life;
    }

    public void UpdateUI()
    {
        lifeTxt.text = life.ToString();
    }
}
