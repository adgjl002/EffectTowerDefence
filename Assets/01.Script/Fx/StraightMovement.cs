using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class StraightMovement : ProjectileMovement
{
    public override EMovementType movementType
    {
        get
        {
            return EMovementType.StraightMovement;
        }
    }
    
    public bool isRotate;
    public Vector3 rotateDirection;

    public bool isLookDestinaion = true;

    public StraightMovement(StraightMovementData _basicData, ProjectileMovementData _data) : base(_basicData, _data)
    {
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
        
        data.moveObject.transform.position = Vector3.Lerp(
            new Vector3(data.startPos.x, data.startPos.y, data.startPos.z),
            new Vector3(destination.x, destination.y, destination.z), progress);

        if (isLookDestinaion)
        {
            data.moveObject.transform.LookAt(destination);
        }

        // 회전 적용
        if (isRotate && data.rotateObject)
        {
            data.rotateObject.transform.Rotate(rotateDirection * Time.deltaTime);
        }
    }
}
