using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public enum EMovingType
{
    VELOCITY,
    DURATION
}

public struct ProjectileMovementData
{
    public GameObject moveObject;
    public Vector3 startPos;
    public Vector3 destination;

    public EMovingType movingType;
    public float movingValue;

    public GameObject rotateObject;
    public UnityAction arriveCallback;
}

/// <summary>
/// <para>* 새로운 Movement를 구현할 때 ProjectileMovement의 생성자 2가지를 꼭 구현해주어야 한다.</para>
/// <para>1. Init()함수를 호출하여 초기화된 객체를 생성한다.</para>
/// <para>2. 생성된 객체의 Move()를 호출하여 실제로 움직이도록 한다.</para>
/// </summary>
public abstract class ProjectileMovement {

    public enum EMovementType
    {
        StraightMovement,
        ParabolaMovement,
        CurveMovement
    }
    
    public ProjectileMovementData data { get; protected set; }

    public abstract EMovementType movementType { get; }
    public const float progressEnd = 0.99f;

    public bool isArrived { get; set; }
    protected float time;
    
    public ProjectileMovement(ProjectileMovementBasicData _basicData, ProjectileMovementData _data)
    {
        data = _data;

        isArrived = false;
        time = 0;
    }

    public virtual void Init(ProjectileMovementBasicData _basicData, ProjectileMovementData _data)
    {
        data = _data;

        isArrived = false;
        time = 0;
    }

    public abstract void Move(Vector3 destination);
}