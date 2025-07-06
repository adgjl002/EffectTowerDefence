using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PopUpUI : UIBase, IPointerClickHandler
{
    [SerializeField]
    private bool m_UseBackspaceClose;
    public bool useBackspaceClose { get { return m_UseBackspaceClose; } }

    [SerializeField]
    private CustomButton m_CloseBtn;
    public CustomButton closeBtn { get { return m_CloseBtn; } }

    protected virtual void Awake()
    {
        if(closeBtn != null)
        {
            closeBtn.OnClick += Close;
        }
    }

    public override void Open()
    {
        base.Open();

        if(useBackspaceClose)
        {
            InputManager.RegistBackspaceEvent(this);
        }
    }

    public override void Close()
    {
        base.Close();

        if(useBackspaceClose)
        {
            InputManager.UnregistBackspaceEvent(this);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Close();
    }
}
