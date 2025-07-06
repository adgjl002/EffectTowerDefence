using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public class ParabolaMovement : ProjectileMovement
{
    public override EMovementType movementType
    {
        get
        {
            return EMovementType.ParabolaMovement;
        }
    }

    /// <summary>
    /// 거리에 비례한 최대 높이 (거리 1당 최대 높이)
    /// </summary>
    public float maxHeightPerDistance;

    public bool isLookDestinaion = true;

    public bool isRotate;
    public Vector3 rotateDirection;

    /// <summary>
    /// <para>거리 비례한 최대 높이 값</para>
    /// <para>maxHeightPerDistance와 destination까지의 거리를 곱한 값</para>
    /// </summary>
    public float maxHeight { get; private set; }
    
    public ParabolaMovement(ParabolaMovementData _basicData, ProjectileMovementData _data) : base(_basicData, _data)
    {
        maxHeightPerDistance = _basicData.maxHeightPerDistance;
        maxHeight = maxHeightPerDistance * Vector3.Distance(data.startPos, _data.destination);

        isLookDestinaion = _basicData.isLookDestinaion;
        isRotate = _basicData.isRotate;
        rotateDirection = _basicData.rotateDirection;
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
        
        float y = Mathf.Lerp(0, maxHeight, Mathf.Sin(progress * 3.14f));

        if (isLookDestinaion)
        {
            Vector3 prePos = data.moveObject.transform.position;
            data.moveObject.transform.position = Vector3.Lerp(
                new Vector3(data.startPos.x, data.startPos.y + y, data.startPos.z),
                new Vector3(destination.x, destination.y + y, destination.z), progress);
            data.moveObject.transform.LookAt(data.moveObject.transform.position + (prePos - data.moveObject.transform.position));
        }
        else
        {
            data.moveObject.transform.position = Vector3.Lerp(
                new Vector3(data.startPos.x, data.startPos.y + y, data.startPos.z),
                new Vector3(destination.x, destination.y + y, destination.z), progress);
        }

        // 회전 적용
        if (isRotate && data.rotateObject)
        {
            data.rotateObject.transform.Rotate(rotateDirection * Time.deltaTime);
        }
    }
}