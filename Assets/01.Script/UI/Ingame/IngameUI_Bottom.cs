using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameUI_Bottom : UIBase {

    [SerializeField]
    private CustomButton m_InfoBtn;
    public CustomButton infoBtn { get { return m_InfoBtn; } }

    [SerializeField]
    private CustomButton m_PlayBtn;
    public CustomButton playBtn { get { return m_PlayBtn; } }

    [SerializeField]
    private CustomButton m_GameSpeedBtn;
    public CustomButton gameSpeedBtn { get { return m_GameSpeedBtn; } }

    [SerializeField]
    private CustomButton m_StageExitBtn;
    public CustomButton stageExitBtn { get { return m_StageExitBtn; } }

    private void Awake()
    {
        infoBtn.OnClick += OnClickInfoBtn;
        playBtn.OnClick += OnClickPlayBtn;
        gameSpeedBtn.OnClick += OnClickGameSpeedBtn;
        stageExitBtn.OnClick += OnClickStageExitBtn;
    }

    public override void Open()
    {
        base.Open();

        UpdateInfoBtn();
        UpdateGameSpeedBtn();
        UpdatePlayBtn();
    }

    public void OnClickInfoBtn()
    {
        IngameManager.Instance.isOnIngameInfo = !IngameManager.Instance.isOnIngameInfo;
        UpdateInfoBtn();
    }

    public void UpdateInfoBtn()
    {
        if (IngameManager.Instance.isOnIngameInfo)
        {
            infoBtn.image.material = null;
        }
        else
        {
            infoBtn.image.material = ResourceManager.GetSpriteGrayScaleMaterial();
        }
    }

    public void OnClickPlayBtn()
    {
        IngameManager.Instance.isGameStopped = !IngameManager.Instance.isGameStopped;
        UpdatePlayBtn();
    }

    public void UpdatePlayBtn()
    {
        Sprite sprite;
        if (ResourceManager.Instance.TryGetSprite((IngameManager.Instance.isGameStopped) ? "Icon_Play" : "Icon_Stop", out sprite))
        {
            playBtn.image.sprite = sprite;
        }
        else
        {
            playBtn.image.sprite = null;
        }
    }

    public void OnClickGameSpeedBtn()
    {
        IngameManager.Instance.ChangeGameSpeed();
        UpdateGameSpeedBtn();
    }

    public void UpdateGameSpeedBtn()
    {
        gameSpeedBtn.label.text = string.Format("x{0}", IngameManager.Instance.curGameSpeed);
    }

    public void OnClickStageExitBtn()
    {
        IngameManager.Instance.isGameStopped = true;

        UIManager.Instance.messageBoxUI.SetData
            ( UITextManager.GetText("알림")
            , UITextManager.GetText("00035")
            , UITextManager.GetText("네")
            , () => { UIManager.Instance.messageBoxUI.Close(); IngameManager.Instance.isGameStopped = false; IngameManager.Instance.ReadyEndStage(false, true); }
            , UITextManager.GetText("아니오")
            , () => { UIManager.Instance.messageBoxUI.Close(); IngameManager.Instance.isGameStopped = false; });
        UIManager.Instance.messageBoxUI.Open();
    }
}
