using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// <para>1. SetFx(...)를 통해 재생에 필요한 데이터를 설정한다.</para>
/// <para>2. On(UnityAction endCallback)을 호출하여 Fx를 재생한다.</para>
/// <para>3. Fx가 Destination에 도달하면 자동으로 Destroy()와 EndCallback()을 호출한다.</para>
/// </summary>
public class Fx_NonTarget : FxBase
{
    public override EFxType fxType
    {
        get
        {
            return EFxType.NON_TARGET;
        }
    }

    [Header("Fx NonTarget")]
    public ParticleSystem mainPs;
    [SerializeField]
    GameObject ObjPart;
    public string arrivePsKey;

    public ProjectileMovementBasicData movementData;

    public EMovingType movingType;
    public float movingValue = 10f;
    public GameObject rotateObj;

    [Tooltip("On()을 호출할 때마다 처음 위치를 Destination으로 사용한다.")]
    public bool useSpawnPosAsDestination;
    public Vector3 destination;
    public Vector3 startPos { get; set; }

    public bool isArrived { get; private set; }
    private UnityAction<Vector3> onArriveFx;

    [Header("Sfx")]
    public string onSfxKey;
    public string arriveSfxKey;
    
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void SetFx(Vector3 destination, UnityAction<Vector3> onArriveFx = null)
    {
        this.destination = destination;
        this.onArriveFx = onArriveFx;
    }

    public override void On(UnityAction endCallback = null)
    {
        this.endCallback = endCallback;

        if (useSpawnPosAsDestination)
        {
            destination = transform.position;
        }

        if(ObjPart != null)
        {
            ObjPart.SetActive(true);
        }

        if (!string.IsNullOrEmpty(onSfxKey))
        {
            //SoundManager.PlayUnitSfx(onSfxKey, transform.position, transform);
        }

        // Offset만큼 움직이고 StartPos로 저장한다.
        startPos = transform.position += startOffset;

        StartCoroutine(Move());
    }

    public override void Off()
    {
        Debug.LogErrorFormat("{0}에서 사용되지 않은 함수({1})를 호출함. 확인 바람.", GetType(), MethodBase.GetCurrentMethod().Name);
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

        if(mainPs) mainPs.Play();
        
        while (!movement.isArrived)
        {
            yield return null;

            movement.Move(destination);
        }
    }
    
    private void ArriveToTarget()
    {
        isArrived = true;

        if (mainPs) mainPs.Stop();

        if (!string.IsNullOrEmpty(arrivePsKey))
        {
            //GameNetworkManagerBase.Instance.transporter.RpcSyncFxToClient(arrivePsKey, destination);
            //GameNetworkManagerBase.Instance.transporter.RpcSpawnFxAtClient(arrivePsKey, destination);
        }
        
        if (!string.IsNullOrEmpty(arriveSfxKey))
        {
            //SoundManager.PlayUnitSfx(arriveSfxKey, destination);
        }

        if (ObjPart != null)
        {
            ObjPart.SetActive(false);
        }

        if (onArriveFx != null) onArriveFx(destination);

        Destroy();
    }
}
