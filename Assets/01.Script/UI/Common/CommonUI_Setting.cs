using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CommonUI_Setting : UIBase 
{
    [SerializeField]
    private CustomButton m_BGMBtn;
    public CustomButton BGMBtn { get { return m_BGMBtn; } }

    [SerializeField]
    private CustomButton m_SFXBtn;
    public CustomButton SFXBtn { get { return m_SFXBtn; } }

    [SerializeField]
    private Slider m_ScrollSensitivity;
    public Slider scrollSensitivity { get { return m_ScrollSensitivity; } }

    [SerializeField]
    private Slider m_ZoomSensitivity;
    public Slider zoomSensitivity { get { return m_ZoomSensitivity; } }

    [SerializeField]
    private TextMeshProUGUI m_VersionTxt;
    public TextMeshProUGUI versionTxt { get { return m_VersionTxt; } }

    private void Start()
    {
        versionTxt.text = string.Format("v{0}", Application.version);

        BGMBtn.OnClick += OnClickBGMBtn;
        SFXBtn.OnClick += OnClickSFXBtn;

        scrollSensitivity.onValueChanged.AddListener((value) =>
        {
            GameSettingsManager.CameraScrollSensitivity = value;
        });

        zoomSensitivity.onValueChanged.AddListener((value) =>
        {
            GameSettingsManager.CameraZoomSensitivity = value;
        });
    }

    public override void Open()
    {
        base.Open();

        UpdateUI();
    }

    public override void Close()
    {
        GameSettingsManager.Save();

        base.Close();
    }

    public void OnClickBGMBtn()
    {
        GameSettingsManager.IsOffBGM = !GameSettingsManager.IsOffBGM;

        UpdateUI();
    }

    public void OnClickSFXBtn()
    {
        GameSettingsManager.IsOffSFX = !GameSettingsManager.IsOffSFX;

        UpdateUI();
    }

    public void UpdateUI()
    {
        BGMBtn.label.text = (GameSettingsManager.IsOffBGM) ? "Off" : "On";
        BGMBtn.image.material = (GameSettingsManager.IsOffBGM) ? ResourceManager.GetSpriteGrayScaleMaterial() : null;

        SFXBtn.label.text = (GameSettingsManager.IsOffSFX) ? "Off" : "On";
        SFXBtn.image.material = (GameSettingsManager.IsOffSFX) ? ResourceManager.GetSpriteGrayScaleMaterial() : null;
    }
}
