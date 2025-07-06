using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class CurveMovement : ProjectileMovement
{
    public override EMovementType movementType
    {
        get
        {
            return EMovementType.CurveMovement;
        }
    }

    /// <summary>
    /// 거리에 비례한 좌우 범위 (X축)
    /// </summary>
    public float widthRangePerDistance;
    public float widthRange { get; private set; }
    public AnimationCurve curveX;

    /// <summary>
    /// 거리에 비례한 상하 범위 (Y축)
    /// </summary>
    public float heightRangePerDistance;
    public float heightRange { get; private set; }
    public AnimationCurve curveY;

    public AnimationCurve speed;

    public bool isRotate;
    public Vector3 rotateDirection;

    [Range(0, 1)]
    public float randomRange;
    private float randomSeed;

    private int randomType;
    
    public CurveMovement(CurveMovementData _basicData, ProjectileMovementData _data) : base(_basicData, _data)
    {
        randomType = Random.Range(0, 3);

        float disToDestination = Vector3.Distance(data.startPos, _data.destination);
        int includeKeyCount = (int)disToDestination / 8;

        widthRangePerDistance = _basicData.widthRangePerDistance;
        widthRange = _basicData.widthRangePerDistance * disToDestination;

        // 커브 생성 or 등록
        if(includeKeyCount < _basicData.curveX.keys.Length)
        {
            curveX = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

            float timeInterval = (float)1 / (includeKeyCount + 1);
            float totalTime = 0;
            for (int i = 0; i < includeKeyCount; i++)
            {
                totalTime += timeInterval;
                curveX.AddKey(new Keyframe(totalTime, _basicData.curveX.Evaluate(totalTime)));
            }
        }
        else
        {
            curveX = _basicData.curveX;
        }

        heightRangePerDistance = _basicData.heightRangePerDistance;
        heightRange = _basicData.heightRangePerDistance * disToDestination;

        // 커브 생성 or 등록
        if (includeKeyCount < _basicData.curveY.keys.Length)
        {
            curveY = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

            float timeInterval = (float)1 / (includeKeyCount + 1);
            float totalTime = 0;
            for (int i = 0; i < includeKeyCount; i++)
            {
                totalTime += timeInterval;
                curveY.AddKey(new Keyframe(totalTime, _basicData.curveY.Evaluate(totalTime)));
            }
        }
        else
        {
            curveY = _basicData.curveY;
        }

        isRotate = _basicData.isRotate;
        speed = _basicData.speed;
        rotateDirection = _basicData.rotateDirection;
        randomRange = _basicData.randomRange;
    }

    public override void Init(ProjectileMovementBasicData _basicData, ProjectileMovementData _data)
    {
        randomSeed = Random.Range(-randomRange, randomRange);
    }

    public override void Move(Vector3 destination)
    {
        time += Time.deltaTime;

        float progress;
        switch (data.movingType)
        {
            case EMovingType.VELOCITY:
                progress = data.movingValue * time / Vector3.Distance(data.startPos, destination);
                break;

            case EMovingType.DURATION:
                progress = time / data.movingValue;
                break;

            default:
                // 타입이 추가되었으나 구현되지 않음.
                progress = progressEnd;
                Debug.LogErrorFormat("{0} : {1}, EMovingType({2}) is not implemented !", GetType(), MethodBase.GetCurrentMethod().Name, data.movingType);
                return;
        }
        
        // 어차피 파괴될거면 다른 연산 굳이 할필요가 없으니 먼저 확인
        if (progress >= progressEnd)
        {
            if (data.arriveCallback != null) data.arriveCallback();
            isArrived = true;
            return;
        }

        var curProgress = progress * speed.Evaluate(progress);
        float posX;
        float posY;

        switch (randomType)
        {
            default:
            case 0:
                posX = widthRange * ((curveX.Evaluate(progress)) - 1);
                posY = heightRange * ((curveY.Evaluate(progress)) - 1);
                break;

            case 1:
                posY = widthRange * ((curveX.Evaluate(progress)) - 1);
                posX = heightRange * ((curveY.Evaluate(progress)) - 1);
                break;

            case 2:
                posX = widthRange * ((curveX.Evaluate(1 - progress)) - 1);
                posY = heightRange * ((curveY.Evaluate(1 - progress)) - 1);
                break;
        }
        
        data.moveObject.transform.position = Vector3.Lerp(
            new Vector3(data.startPos.x + posX, data.startPos.y + posY, data.startPos.z),
            new Vector3(destination.x + posX, destination.y + posY, destination.z), curProgress);

        // 회전 적용
        if (isRotate && data.rotateObject)
        {
            data.rotateObject.transform.Rotate(rotateDirection * Time.deltaTime);
        }
    }
}
