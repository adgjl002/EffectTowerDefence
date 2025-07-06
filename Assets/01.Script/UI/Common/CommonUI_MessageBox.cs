using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class CommonUI_MessageBox : UIBase
{
    [SerializeField]
    private TextMeshProUGUI m_TitleTxt;
    public TextMeshProUGUI titleTxt { get { return m_TitleTxt; } }

    [SerializeField]
    private TextMeshProUGUI m_MessageTxt;
    public TextMeshProUGUI messageTxt { get { return m_MessageTxt; } }

    [SerializeField]
    private CustomButton m_YesBtn;
    public CustomButton yesBtn { get { return m_YesBtn; } }

    [SerializeField]
    private CustomButton m_NoBtn;
    public CustomButton noBtn { get { return m_NoBtn; } }

    /// <summary>
    /// 각 버튼들의 콜백을 지정하지 않으면 버튼은 비활성화된다.
    /// </summary>
    public void SetData(string title, string message, string yesBtnLabel, UnityAction onClickYesBtn, string noBtnLabel = null, UnityAction onClickNoBtn = null)
    {
        titleTxt.text = title;
        messageTxt.text = message;

        yesBtn.OnClick = null;
        noBtn.OnClick = null;

        if (onClickYesBtn != null)
        {
            yesBtn.OnClick += onClickYesBtn;
            yesBtn.gameObject.SetActive(true);
            yesBtn.label.text = yesBtnLabel;
        }
        else
        {
            yesBtn.gameObject.SetActive(false);
        }

        if (onClickNoBtn != null)
        {
            noBtn.OnClick += onClickNoBtn;
            noBtn.gameObject.SetActive(true);
            noBtn.label.text = noBtnLabel;
        }
        else
        {
            noBtn.gameObject.SetActive(false);
        }
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();

        yesBtn.OnClick = null;
        noBtn.OnClick = null;
    }
}
