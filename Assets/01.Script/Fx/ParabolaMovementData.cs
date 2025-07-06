using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ParabolaMovementData", menuName = "Movement/ParabolaMovementData")]
public class ParabolaMovementData : ProjectileMovementBasicData
{
    public override ProjectileMovement.EMovementType movementType
    {
        get
        {
            return ProjectileMovement.EMovementType.ParabolaMovement;
        }
    }
    
    public float maxHeightPerDistance;
    public bool isLookDestinaion;
    public bool isRotate;
    public Vector3 rotateDirection;
}