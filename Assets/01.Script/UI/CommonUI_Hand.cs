using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CommonUI_Hand : UIBase
{
    [SerializeField]
    private Image m_HandImg;
    public Image handImg => m_HandImg;

    [SerializeField]
    private RectTransform m_OriginLayerRtf;
    public RectTransform originLayerRtf => m_OriginLayerRtf;

    public float duration = 1f;
    public Ease animationEase = Ease.OutBack;

    public override void Open()
    {
        base.Open();

        handImg.transform.localScale = Vector3.zero;
        handImg.rectTransform.rotation = Quaternion.Euler(Vector3.zero);

        handImg.rectTransform.DOScale(1, 0.5f).SetEase(Ease.OutBack).Restart();
        handImg.rectTransform.DORotate(new Vector3(0, 0, 50), duration, RotateMode.WorldAxisAdd).SetLoops(-1, LoopType.Yoyo).SetEase(animationEase).SetDelay(0.5f).Restart();
    }

    public override void Close()
    {
        base.Close();
    }

    public void SetPosition(Vector3 uiWorldPos)
    {
        transform.SetParent(originLayerRtf);
        transform.localScale = Vector3.one;
        transform.position = new Vector3(uiWorldPos.x, uiWorldPos.y, transform.position.z);
    }

    public void SetPosition(Transform tf)
    {
        transform.SetParent(tf);
        transform.localScale = Vector3.one;
        transform.position = new Vector3(tf.position.x, tf.position.y, tf.position.z);
    }
}
