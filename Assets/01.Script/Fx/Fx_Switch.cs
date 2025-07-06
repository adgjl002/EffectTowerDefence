using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// <para>1. On(UnityAction endCallback)을 호출하여 Fx를 재생한다.</para>
/// <para>2. Off()를 호출하여 Fx를 정지시키며, EndCallBack이 호출된다.</para>
/// <para>3. Destroy()를 직접 호출하여 오브젝트를 정리해야한다.(ObjectPool에서 재사용 가능한 상태로)</para>
/// </summary>
public class Fx_Switch : FxBase
{
    public override EFxType fxType
    {
        get
        {
            return EFxType.SWITCH;
        }
    }
    
    public ParticleSystem onPs;
    public ParticleSystemStopBehavior onPsStopType = ParticleSystemStopBehavior.StopEmitting;
    public string onSfxKey;

    public ParticleSystem offPs;
    public string offSfxKey;
    public ParticleSystemStopBehavior offPsStopType = ParticleSystemStopBehavior.StopEmitting;

    public bool isPlayingOn { get { return onPs != null && onPs.isPlaying; } }
    public bool isPlayingOff { get { return offPs != null && offPs.isPlaying; } }

    private void OnEnable()
    {
#if CLIENT_SESSION
        if (reverseType != EEffectReverseType.NONE)
        {
            if (onPs) EffectHelper.Reverse(reverseType, onPs);
            if (offPs) EffectHelper.Reverse(reverseType, offPs);
        }
#elif LOCALTEST
        if (/*!GameNetworkManagerBase.isServer && */reverseType != EEffectReverseType.NONE)
        {
            if (onPs) EffectHelper.Reverse(reverseType, onPs);
            if (offPs) EffectHelper.Reverse(reverseType, offPs);
        }
#endif
    }

    public override void On(UnityAction endCallback = null)
    {
        this.endCallback = endCallback;
        transform.position += startOffset;

        if (onPs) onPs.Play();

        if (!string.IsNullOrEmpty(onSfxKey))
        {
            //SoundManager.PlayUnitSfx(onSfxKey, transform.position, transform);
        }

        if (offPs) offPs.Stop(true, offPsStopType);
    }

    public override void Off()
    {
        if (onPs) onPs.Stop(true, onPsStopType);
        
        if (offPs) offPs.Play();

        if (!string.IsNullOrEmpty(offSfxKey))
        {
            //SoundManager.PlayUnitSfx(offSfxKey, transform.position, transform);
        }

        if (endCallback != null) endCallback();
    }

    public override void Destroy()
    {
        if (destroyDelay > 0)
        {
            //nvoke("DestroySelf", destroyDelay);
            StartCoroutine(TimerDestroySelf());
        }
        else
        {
            DestroySelf();
        }
    }

    IEnumerator TimerDestroySelf()
    {
        float speed = destroyDelay;
        float timer = 0f;
        while (timer < speed)
        {
            yield return null;
            timer += Time.deltaTime;
        }
        DestroySelf();
    }
}
