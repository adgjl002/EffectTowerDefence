using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CurveMovementData", menuName = "Movement/CurveMovementData")]
public class CurveMovementData : ProjectileMovementBasicData
{
    public override ProjectileMovement.EMovementType movementType
    {
        get
        {
            return ProjectileMovement.EMovementType.CurveMovement;
        }
    }

    public float widthRangePerDistance;
    public AnimationCurve curveX;

    public float heightRangePerDistance;
    public AnimationCurve curveY;

    public AnimationCurve speed;

    public bool isRotate;
    public Vector3 rotateDirection;

    [Range(0, 1)]
    public float randomRange;
}
