using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StraightMovementData", menuName = "Movement/StraightMovementData")]
public class StraightMovementData : ProjectileMovementBasicData
{
    public override ProjectileMovement.EMovementType movementType
    {
        get
        {
            return ProjectileMovement.EMovementType.StraightMovement;
        }
    }

    public bool isRotate;
    public Vector3 rotateDirection;
    public bool isLookDestinaion;
}
