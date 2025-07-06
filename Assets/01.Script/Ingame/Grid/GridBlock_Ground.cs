using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBlock_Ground : GridBlock
{
    public override EType gridBlockType { get { return EType.Ground; } }

    public override bool BuildTower(TowerData towerData)
    {
        if (!SpawnMaster.TrySpawnMonoBehaviour(towerData.prefabKey, transform.position, Quaternion.identity, out tower))
        {
            Debug.LogErrorFormat("{0} :: Can't spawned GridBlock_Tower", GetType());
            return false;
        }

        Fx_OneTime fx;
        if(SpawnMaster.TrySpawnFx("Fx_Building", transform.position, Quaternion.identity, out fx))
        {
            fx.On();
        }

        tower.transform.SetParent(transform);
        tower.transform.localPosition = Vector3.zero;
        tower.SetData(this, towerData);
        tower.Initialize();
        
        return true;
    }
}
