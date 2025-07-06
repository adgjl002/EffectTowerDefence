using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using DG.Tweening;


public class UIManager_Common_NoticeUI : UIBase
{
    private Color backgroundOriginColor;
    private Color msgOriginColor;

    [SerializeField]
    private Image background;

    [SerializeField]
    private TextMeshProUGUI msgTxt;

    public float duration { get; private set; }

    private void Awake()
    {
        backgroundOriginColor = background.color;
        msgOriginColor = msgTxt.color;
    }

    public void SetData(string message, float duration)
    {
        msgTxt.text = message.ToString();
        this.duration = duration;
    }

    IEnumerator closeDelayEtor;

    public override void Open()
    {
        base.Open();

        msgTxt.color = new Color(msgOriginColor.r, msgOriginColor.g, msgOriginColor.b, 0);
        msgTxt.DOColor(msgOriginColor, 0.3f).SetEase(Ease.InSine);

        background.color = new Color(backgroundOriginColor.r, backgroundOriginColor.g, backgroundOriginColor.b, 0);
        background.DOColor(backgroundOriginColor, 0.3f).SetEase(Ease.InSine);

        if (closeDelayEtor != null)
        {
            StopCoroutine(closeDelayEtor);
        }
        closeDelayEtor = CloseDelay();
        StartCoroutine(closeDelayEtor);
    }

    private IEnumerator CloseDelay()
    {
        bool playingAnimation = false;
        float timer = duration;
        do
        {
            yield return null;

            if(timer > 0.3f)
            {
                timer -= Time.deltaTime;
            }
            else if(timer >= 0)
            {
                if(!playingAnimation)
                {
                    playingAnimation = true;
                    msgTxt.DOColor(new Color(msgOriginColor.r, msgOriginColor.g, msgOriginColor.b, 0), 0.3f).SetEase(Ease.InSine);
                    background.DOColor(new Color(backgroundOriginColor.r, backgroundOriginColor.g, backgroundOriginColor.b, 0), 0.3f).SetEase(Ease.InSine);
                }

                timer -= Time.deltaTime;
            }
            else if(!gameObject.activeInHierarchy)
            {
                yield break;
            }
            else
            {
                Close();
            }
        }
        while (true);
    }
}
