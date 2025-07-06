using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum EFxType
{
    // 일반 타입
    ONE_TIME,
    ONE_TIME_MESSAGE,

    SWITCH,
    PARTICLE_TARGET,
    TARGET,
    NON_TARGET
}

/// <summary>
/// 1. 외부에서 등록해야할 데이터는 SetFx()를 통해 등록한다.
/// 2. SetFx()호출 이후 On()을 호출하여 재생한다.
/// 3. 필요한 경우 Off()와 Destroy()를 호출하여 정리한다.
/// </summary>
public abstract class FxBase : MonoBehaviour {

    [Header("FxBase")]
    public string fxKey;
    
    public bool useScaleY = true;

    public float startDelay;
    public Vector3 startOffset;
    public float destroyDelay;

    public abstract EFxType fxType { get; }
    public UnityAction endCallback { get; set; }
    
    public abstract void On(UnityAction endCallback = null);
    public abstract void Off();
    public abstract void Destroy();

    /// <summary>
    /// DestroySelf가 호출될 때 Release를 호출하여 재사용을 위한 정리를 한다.
    /// </summary>
    public virtual void Release()
    {

    }

    public void DestroySelf()
    {
        Release();
        SpawnMaster.Destroy(gameObject, fxKey);
    }
}
