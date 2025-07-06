using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using DG.Tweening;

public enum EFxMessageType
{
    LevelUp,
    Double,
    Triple,
    Quadruple,
}

public class Fx_OneTime_Message : Fx_OneTime
{
    public override EFxType fxType => EFxType.ONE_TIME_MESSAGE;

    public EFxMessageType fxMsgType;

    [SerializeField]
    private ParticleSystem levelUpPs;

    [SerializeField]
    private ParticleSystem doublePs;

    [SerializeField]
    private ParticleSystem triplePs;

    [SerializeField]
    private ParticleSystem quadruplePs;

    [SerializeField]
    private TextMeshPro message;
    public TextMeshPro Message { get { return message; } }

    public override void On(UnityAction endCallback = null)
    {
        levelUpPs.gameObject.SetActive(false);
        doublePs.gameObject.SetActive(false);
        triplePs.gameObject.SetActive(false);
        quadruplePs.gameObject.SetActive(false);

        switch (fxMsgType)
        {
            default:
            case EFxMessageType.LevelUp:
                onPs = levelUpPs;
                break;

            case EFxMessageType.Double:
                onPs = doublePs;
                break;

            case EFxMessageType.Triple:
                onPs = triplePs;
                break;

            case EFxMessageType.Quadruple:
                onPs = quadruplePs;
                break;
        }

        onPs.gameObject.SetActive(true);

        base.On(endCallback);
        
        message.color = new Color(message.color.r, message.color.g, message.color.b, 0);
        message.transform.localScale = Vector3.zero;
        message.transform.localPosition = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        seq.Append(message.DOColor(new Color(message.color.r, message.color.g, message.color.b, 1), 1.5f).SetEase(Ease.OutSine));
        seq.Insert(0, message.DOScale(1.1f, 0.1f).SetEase(Ease.OutSine));
        seq.Insert(0.3f, message.DOScale(1, 0.1f).SetEase(Ease.OutSine));
        seq.Insert(0, message.transform.DOMoveY(0.1f, 0.4f).SetRelative().SetEase(Ease.OutSine));
        seq.Insert(0.4f, message.transform.DOMoveY(0.1f, 3f).SetRelative().SetEase(Ease.OutSine));
        seq.Insert(1.5f, message.DOColor(new Color(message.color.r, message.color.g, message.color.b, 0), 0.5f).SetEase(Ease.InSine));
        //seq.Insert(2, message.DOScale(0, 0.2f).SetEase(Ease.OutSine));
        seq.Restart();
    }
}
