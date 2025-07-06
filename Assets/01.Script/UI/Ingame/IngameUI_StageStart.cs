using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class IngameUI_StageStart : UIBase
{
    [SerializeField]
    private TextMeshProUGUI m_CountTxt;
    public TextMeshProUGUI countTxt { get { return m_CountTxt;} }

    public void SetText(string txt)
    {
        countTxt.text = txt;
    }

    public override void Open()
    {
        base.Open();

        countTxt.transform.localScale = new Vector3(2, 2, 2);
        countTxt.DOScale(1, 0.4f).SetEase(Ease.OutBack);
    }
}
