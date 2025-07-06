using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

using SqeuentialActionQueue;

/// <summary>
/// <para>1. On(UnityAction endCallback)을 호출하여 Fx를 재생한다.</para>
/// <para>2. Fx의 재생이 끝나면 자동으로 Destroy()가 호출되어 정리한다.</para>
/// </summary>
public class Fx_OneTime : FxBase
{

    public override EFxType fxType
    {
        get
        {
            return EFxType.ONE_TIME;
        }
    }

    /// <summary>
    /// 값이 0일 때 onPs로 등록된 파티클 시스템의 재생시간으로 계산하여 사용됨.
    /// </summary>
    [Header("Fx OneTime")]
    public float duration;

    public ParticleSystem onPs;

    [Header("Sfx")]
    public string onSfxKey;
    //public ESfxPlayType onSfxPlayType = ESfxPlayType.None;

    private IEnumerator checkDestroyEnumerator;
    private GameObject followTarget;

    private void OnEnable()
    {
#if CLIENT_SESSION || INHOUSE_DEBUG
        if (reverseType != EEffectReverseType.NONE)
        {
            if (onPs) EffectHelper.Reverse(reverseType, onPs);
        }
#elif LOCALTEST
        if (/*!GameNetworkManagerBase.isServer && */reverseType != EEffectReverseType.NONE)
        {
            if (onPs) EffectHelper.Reverse(reverseType, onPs);
        }
#endif
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
    
    public void SetFollowTarget(GameObject followTarget)
    {
        this.followTarget = followTarget;
    }

    public void On(UnityAction timecallback, float callbackInvokeTime)
    {
        SequentialActionQueue saq = new SequentialActionQueue();
        saq.EnqueueActionByTime(new SequentialActionByTime(null, callbackInvokeTime));
        saq.SequentialInvoke(this, timecallback);
        
        On();
    }

    public override void On(UnityAction endCallback = null)
    {
        this.endCallback = endCallback;
        transform.position += startOffset;

        if (onPs) onPs.Play();
        
        if (!string.IsNullOrEmpty(onSfxKey))
        {
            //switch (onSfxPlayType)
            //{
            //    case ESfxPlayType.None:
            //        SoundManager.PlayUnitSfx(onSfxKey, transform.position);
            //        break;

            //    case ESfxPlayType.Looping_Until_Destroy:
            //        SoundManager.PlayUnitSfx_Loop(onSfxKey, transform.position, null, () =>
            //        {
            //            return !gameObject.activeInHierarchy;
            //        });
            //        break;
            //}
        }

        if (duration == 0f)
        {
            checkDestroyEnumerator = CheckDestroySelf();
            StartCoroutine(checkDestroyEnumerator);
        }
        else
        {
            //nvoke("Destroy", duration);
            StartCoroutine(TimerDestroy());
        }
    }

    IEnumerator TimerDestroy()
    {
        float speed = duration;
        float timer = 0f;

        if (followTarget != null)
        {
            while (timer < speed)
            {
                transform.position = followTarget.transform.position;
                yield return null;
                timer += Time.deltaTime;
            }
        }
        else
        {
            while (timer < speed)
            {
                yield return null;
                timer += Time.deltaTime;
            }
        }


        Destroy();
    }

    public override void Off()
    {
        Debug.LogErrorFormat("{0}에서 사용되지 않은 함수({1})를 호출함. 확인 바람.", GetType(), MethodBase.GetCurrentMethod().Name);
    }

    public override void Release()
    {
        base.Release();

        followTarget = null;
    }

    public override void Destroy()
    {
        if (onPs) onPs.Stop();

        if (endCallback != null) endCallback();

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

    IEnumerator CheckDestroySelf()
    {
        if (!onPs)
        {
            if (followTarget != null)
            {
                transform.position = followTarget.transform.position;
            }
            yield return null;
            
            goto Skip;
        }

        if (followTarget != null)
        {
            do
            {
                transform.position = followTarget.transform.position;
                yield return null;

            } while (onPs.isEmitting);
        }
        else
        {
            do
            {
                yield return null;

            } while (onPs.isEmitting);
        }

        Skip:;

        Destroy();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            On();
        }
    }
#endif
}


