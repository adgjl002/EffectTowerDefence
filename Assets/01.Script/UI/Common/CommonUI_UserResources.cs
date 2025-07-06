using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CommonUI_UserResources : UIBase
{
    [SerializeField]
    private Image m_StarIconImg;
    public Image starIconImg { get { return m_StarIconImg; } }
    
    [SerializeField]
    private TextMeshProUGUI m_StarCountTxt;
    public TextMeshProUGUI starCountTxt { get { return m_StarCountTxt; } }

    public int starCount { get; private set; }

    public void SetData(int starCount)
    {
        this.starCount = starCount;
        starCountTxt.text = starCount.ToString();
    }

    public void AddData(int addStarCount)
    {
        this.starCount = starCount + addStarCount;
        starCountTxt.text = starCount.ToString();
    }

    public void OnChangeStarCount(int preStarCount, int curStarCount)
    {
        starCountTxt.text = curStarCount.ToString();
    }
}
