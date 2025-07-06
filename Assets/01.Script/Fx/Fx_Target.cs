using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// <para>1. SetFx(...)를 통해 재생에 필요한 데이터를 설정한다.</para>
/// <para>2. On(UnityAction endCallback)을 호출하여 Fx를 재생한다.</para>
/// <para>3. Fx가 Target에 도달하면 자동으로 Destroy()와 EndCallback()을 호출한다.</para>
/// </summary>
public class Fx_Target : FxBase
{
    public override EFxType fxType
    {
        get
        {
            return EFxType.TARGET;
        }
    }
    
    [Header("Fx Target")]
    public ParticleSystem mainPs;
    public string arrivePsKey;
    public ProjectileMovementBasicData movementData;

    //public Unit target;
    private Vector3 destination;

    public EMovingType movingType;
    public float movingValue = 10f;
    public GameObject rotateObj;

    public Vector3 startPos { get; set; }
    public GameObject target { get; private set; }

    [Header("Sfx")]
    public string onSfxKey;
    public string arriveSfxKey;

    private System.Func<bool> onSkipCondition;

    private void OnEnable()
    {
#if CLIENT_SESSION
        if (reverseType != EEffectReverseType.NONE)
        {
            if (mainPs) EffectHelper.Reverse(reverseType, mainPs);
        }
#elif LOCALTEST
        if (/*!GameNetworkManagerBase.isServer && */reverseType != EEffectReverseType.NONE)
        {
            if (mainPs) EffectHelper.Reverse(reverseType, mainPs);
        }
#endif
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void SetFx(GameObject target, System.Func<bool> onSkipCondition = null)
    {
        this.target = target;
        this.destination = target.transform.position;
        this.onSkipCondition = onSkipCondition;
    }

    public override void On(UnityAction endCallback = null)
    {
        this.endCallback = endCallback;

        startPos = transform.position += startOffset;

        if(!string.IsNullOrEmpty(onSfxKey))
        {
            //SoundManager.PlayUnitSfx(onSfxKey, transform.position, transform);
        }

        StartCoroutine(Move());
    }

    public override void Off()
    {
        Debug.LogErrorFormat("{0}에서 사용되지 않은 함수({1})를 호출함. 확인 바람.", GetType(), MethodBase.GetCurrentMethod().Name);
    }

    public override void Destroy()
    {
        if (endCallback != null) endCallback();

        StopAllCoroutines();

        if (destroyDelay > 0)
        {
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

    IEnumerator Move()
    {
        // 데이터를 토대로 무브먼트를 생성한다.
        var movement = movementData.CreateMovement(new ProjectileMovementData()
        {
            arriveCallback = ArriveToTarget,
            moveObject = gameObject,
            rotateObject = rotateObj,
            startPos = startPos,
            destination = destination,
            movingType = movingType,
            movingValue = movingValue
        });
        
        yield return new WaitForSeconds(startDelay);

        if (mainPs) mainPs.Play();

        while (!movement.isArrived)
        {
            yield return null;

            if(onSkipCondition != null && onSkipCondition())
            {
                DestroySelf();
                break;
            }

            destination = target.transform.position;

            movement.Move(destination);
        }
    }
    
    private void ArriveToTarget()
    {
        if (mainPs) mainPs.Stop(true);

        if (!string.IsNullOrEmpty(arrivePsKey))
        {
            //GameNetworkManagerBase.Instance.transporter.RpcSpawnFxAtClient(arrivePsKey, target.transform.position);
        }
        
        if(!string.IsNullOrEmpty(arriveSfxKey))
        {
            //SoundManager.PlayUnitSfx(arriveSfxKey, target.transform.position);
        }
        
        onSkipCondition = null;
        
        Destroy();
    }

}
