using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileMovementBasicData : ScriptableObject
{
    public abstract ProjectileMovement.EMovementType movementType { get; }

    public ProjectileMovement CreateMovement(ProjectileMovementData _data)
    {
        switch (movementType)
        {
            case ProjectileMovement.EMovementType.StraightMovement:
                return new StraightMovement(this as StraightMovementData, _data);

            case ProjectileMovement.EMovementType.ParabolaMovement:
                return new ParabolaMovement(this as ParabolaMovementData, _data);

            case ProjectileMovement.EMovementType.CurveMovement:
                return new CurveMovement(this as CurveMovementData, _data);

            default:
                Debug.LogError("ProjectileMovement : CreateProjectileMover, MovementType({2}) is not implemented !");
                return null;
        }
    }
}
