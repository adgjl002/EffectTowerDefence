using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TowerAttackComponent : MonoBehaviour
{
    public enum EType
    {
        Normal,
        Beam
    }

    public GridBlock_Tower owner { get; private set; }

    public abstract EType componentType { get; }
    public abstract IngameObject curTarget { get; }

    public float curAttackDelay;
    public int curAttackCount;

    public void Initialize(GridBlock_Tower owner)
    {
        this.owner = owner;
        OnInitialize();
    }

    public abstract void OnInitialize();

    public abstract void InitTarget();
    public abstract bool FindTarget();
    public abstract void Attack();

    public abstract void OnRelease();

    public void Release()
    {
        OnRelease();
    }
}
