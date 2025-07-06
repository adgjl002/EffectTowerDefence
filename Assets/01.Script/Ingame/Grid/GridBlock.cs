using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EDamageType
{
    None = 0,
    Poison = 1,
    Explosion = 2,
    Transition = 3,
    Touch = 4,
    Frostbite = 5,
    Burning = 6,
    HealHp = 7,
    HealShield = 8,
}

[System.Serializable]
public struct TowerData
{
    public TowerData
        ( string towerID, string prefabKey, string projectileKey, string towerIconKey
        , int buildCost, int damage, float range, float attackSpeed
        , int projectileCount = 1, float criticalPerc = 0.1f, float criticalDamageRatio = 1.5f, int taeSlotCount = 4, string skill1Id =  null)
    {
        this.towerID = towerID;
        this.prefabKey = prefabKey;
        this.projectileKey = projectileKey;
        this.towerIconKey = towerIconKey;
        this.buildCost = buildCost;
        this.damage = damage;
        this.range = range;
        this.attackSpeed = attackSpeed;
        this.projectileCount = projectileCount;
        this.criticalPerc = criticalPerc;
        this.criticalDamageRatio = criticalDamageRatio;
        this.taeSlotCount = taeSlotCount;
        this.skill1Id = skill1Id;
    }

    public string towerID { get; set; }
    public string prefabKey { get; set; }
    public string projectileKey { get; set; }
    public string towerIconKey { get; set; }
    public int buildCost { get; set; }
    public int damage { get; set; }
    public float range { get; set; }
    public float attackSpeed { get; set; }
    public int projectileCount { get; set; }
    public float criticalPerc { get; set; }
    public float criticalDamageRatio { get; set; }
    public int taeSlotCount { get; set; }
    public string skill1Id { get; set; }

    public string GetLog()
    {
        return string.Format("ID({0}) PID({1}) ProjID({2}) Icon({3}) Cost({4}) Dmg({5}) Rng({6}) AttSpd({7}) ProjCnt({8}) CtrlPerc({9}) CtrlDmg({10}) TaeCnt({11}) Skill1ID({12})"
            , towerID, prefabKey, projectileKey, towerIconKey, buildCost, damage, range, attackSpeed, projectileCount, criticalPerc, criticalDamageRatio, taeSlotCount, skill1Id);
    }
}

public class GridBlock : MonoBehaviour {

    public enum EIntType
    {
        None = 0,
        Ground = 1,
        WayPoint = 2,
        StartPoint = 3,
        EndPoint = 4,
        Wall = 5,
        SkyWayPoint = 6,
        Effect = 7
    }

    public enum EType
    {
        None = 1,
        Ground = 2,
        WayPoint = 4,
        StartPoint = 8,
        EndPoint = 16,
        Wall = 32,
        SkyWayPoint = 64,
    }

    private IntVector2 m_GridPos;
    public IntVector2 gridPos { get { return m_GridPos; } set { m_GridPos = value; } }

    public int effectType { get; private set; }
    public Fx_Switch fxSwitchEnv { get; private set; }
    
    public virtual EType gridBlockType { get { return EType.None; } }

    [SerializeField]
    private string m_PrefabKey;
    public string prefabKey { get { return m_PrefabKey; } }

    public bool IncludedGridType(EType gridType)
    {
        return (gridBlockType & gridType) > 0;
    }
    public bool EqualGridType(EType gridType)
    {
        return (gridBlockType & gridType) == gridType;
    }

    public virtual void Initialize()
    {

    }

    public virtual void Release()
    {
        if(isBuildTower) tower.Release();
        tower = null;

        SpawnMaster.Destroy(gameObject, prefabKey);
    }

    public virtual void SetData(IntVector2 gridPos, int effectType = 0)
    {
        this.gridPos = gridPos;
        this.effectType = effectType;
        
        if(fxSwitchEnv != null)
        {
            Destroy(fxSwitchEnv.gameObject);
            fxSwitchEnv = null;
        }

        if(this.effectType == 1)
        {
            GameObject fxGobj;
            if (ResourceManager.Instance.TryGetPrefab("Fx_Env_1", out fxGobj))
            {
                fxSwitchEnv = Instantiate(fxGobj, transform).GetComponent<Fx_Switch>();
                fxSwitchEnv.transform.localPosition = Vector3.zero;
                fxSwitchEnv.On();
            }
        }
    }

    public bool isBuildTower { get { return tower != null; } }
    public GridBlock_Tower tower;

    [SerializeField]
    private SpriteRenderer m_Render;
    public SpriteRenderer render { get { return m_Render; } }
    
    public virtual bool BuildTower(TowerData towerData)
    {
        return false;
    }

    public virtual bool SellTower()
    {
        if (isBuildTower)
        {
            IngameManager.Instance.AddCost(tower.GetSellCost());
            tower.Release();
            tower = null;
            return true;
        }
        return false;
    }

    public virtual void Select()
    {

    }

    public virtual void Unselect()
    {

    }
}
