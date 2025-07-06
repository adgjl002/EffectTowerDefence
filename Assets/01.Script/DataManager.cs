using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LitJson;

public enum EStageMode
{
    Normal = 0,
    Infinity = 1,
}

[System.Serializable]
public class CsvSkillData
{
    public string id { get; set; }
    public int tier { get; set; }
    public int style { get; set; }
    public string spriteName { get; set; }
    public int starCost { get; set; }
    public string needStageId { get; set; }
    public string needSkillIds { get; set; }
    public int needSkillIDsOperator { get; set; }
    public string noNeedSkillIds { get; set; }
    public string subData { get; set; }
    public string data1 { get; set; }
    public string data2 { get; set; }
    public string data3 { get; set; }
    public string data4 { get; set; }
    public string data5 { get; set; }
    public string data6 { get; set; }
    public string data7 { get; set; }
    public string data8 { get; set; }
}

public class StageMapData
{
    public float lobbyZoomSize;
    public float ingameZoomSize;
    public float tileSize;
    public string mapData;
    public string mapSkyData;
    public WallData wallData;
}

public class StageData
{
    public EStageMode stageMode;
    public string stageId;
    public float lobbyZoomSize;
    public float ingameZoomSize;
    public float tileSize;
    public int startLife;
    public int startCost;

    /// <summary>
    /// <para>0 : None</para>
    /// <para>1 : Ground</para>
    /// <para>2 : WayPoint</para>
    /// <para>3 : EndPoint</para>
    /// <para>4 : StartPoint</para>
    /// <para>5 : Wall</para>
    /// </summary>
    public string mapData;

    /// <summary>
    /// <para>0 : Null</para>
    /// <para>6 : SkyWayPoint</para>
    /// </summary>
    public string mapSkyData;
    
    public List<IngameObjectSpawnInfo> waveDatas;

    public WallData wallData;
}

public class DataManager
{
    public static DataManager Instance { get { return AppManager.Instance.dataManager; } }

    public Dictionary<string, int> stageIdxById { get; private set; }
    public int GetStageIdxById(string id)
    {
        int idx;
        if(stageIdxById.TryGetValue(id, out idx))
        {
            return idx;
        }
        return 0;
    }

    private List<StageData> stageDatas;
    private Dictionary<string, TowerData> towerDatas;

    #region < IngameObject >

    private Dictionary<string, IngameObjectData> ingameObjectDatas;
    public bool TryGetIngameObjectData
        (string id
        , int level
        , IngameObject.EClass unitClass
        , int count
        , out IngameObjectData data
        , float powerRatio = 1f
        , float costRatio = 1f
        , float defenseRatio = 1f
        , float maxShieldRatio = 1f)
    {
        IngameObjectData originData;
        if(ingameObjectDatas.TryGetValue(id, out originData))
        {
            data = new IngameObjectData
                ( originData.unitType
                , Mathf.RoundToInt((originData.maxHp / count) * level * powerRatio)
                , originData.moveSpeed
                , Mathf.Max(0, Mathf.RoundToInt((originData.cost / count) * costRatio))
                , Mathf.Min(1f, (originData.defense + (0.02f * (level - 1))) * defenseRatio)
                , level
                , unitClass
                , originData.regeneration
                , originData.resistance
                , Mathf.RoundToInt((originData.maxShield / count) * level * maxShieldRatio)
                , originData.skillIds);
            return true;
        }
        data = new IngameObjectData();
        return false;
    }

    #endregion

    #region < User Skill >

    private Dictionary<string, SkillData> skillDatas;
    public Dictionary<string, SkillData>.Enumerator GetSkillDatasEtor()
    {
        return skillDatas.GetEnumerator();
    }

    #endregion

    #region < IngameObject Skill >

    private Dictionary<string, IngameObject_.SkillData> m_IngameObjectSkills = new Dictionary<string, IngameObject_.SkillData>();
    public Dictionary<string, IngameObject_.SkillData> ingameObjectSkills => m_IngameObjectSkills;
    public bool TryGetIngameObjectSkill(string skillId, out IngameObject_.SkillData data)
    {
        return ingameObjectSkills.TryGetValue(skillId, out data);
    }

    #endregion

    #region < Map >

    private List<StageMapData> stageMapDatas;
    public StageMapData GetRandomStageMapData()
    {
        return stageMapDatas[Random.Range(0, stageMapDatas.Count)];
    }

    #endregion

    private Dictionary<IngameObject.EType, List<string>> prefabKeysByType;
    public string GetRandomPrefabKey(IngameObject.EType ingameObjectType)
    {
        List<string> prefabKeys;
        if(prefabKeysByType.TryGetValue(ingameObjectType, out prefabKeys))
        {
            return prefabKeys[Random.Range(0, prefabKeys.Count)];
        }
        return string.Empty;
    }

    private Dictionary<string, TowerAdditionalEffect> towerAdditionalEffects;
    
    public void Initialize()
    {
        if (stageIdxById == null) stageIdxById = new Dictionary<string, int>();
        stageIdxById.Clear();

        if (stageDatas == null) stageDatas = new List<StageData>();
        stageDatas.Clear();

        if (towerDatas == null) towerDatas = new Dictionary<string, TowerData>();
        towerDatas.Clear();

        if (ingameObjectDatas == null) ingameObjectDatas = new Dictionary<string, IngameObjectData>();
        ingameObjectDatas.Clear();

        if (skillDatas == null) skillDatas = new Dictionary<string, SkillData>();
        skillDatas.Clear();

        if (stageMapDatas == null) stageMapDatas = new List<StageMapData>();
        stageMapDatas.Clear();

        if (prefabKeysByType == null) prefabKeysByType = new Dictionary<IngameObject.EType, List<string>>();
        prefabKeysByType.Clear();

        if (towerAdditionalEffects == null) towerAdditionalEffects = new Dictionary<string, TowerAdditionalEffect>();
        towerAdditionalEffects.Clear();
        ingameObjectSkills.Clear();

        LoadPrefabKeysByType();
        //LoadSkillMapDatas();
        LoadSkillMapDatas2();
        LoadTowerDatas();
        LoadIngameObjectDatas();
        LoadIngameObjectSkillDatas();
        LoadStageDatas();
        LoadInfinityModeMapDatas();
    }

    #region < Load Datas >

    private void LoadPrefabKeysByType()
    {
        prefabKeysByType.Add(IngameObject.EType.Walker, new List<string>()
        {
            "IO-1",
            "IO-2",
            "IO-3",
            "IO-4",
            "IO-5",
            "IO-6",
            "IO-7",
            "IO-8",
            "IO-10",
            "IO-11",
            "IO-12",
            "IO-13",
            "IO-15",
            "IO-21",
            "IO-22",
            "IO-23",
        });
        prefabKeysByType.Add(IngameObject.EType.Destroyer, new List<string>()
        {
            "IO-9",
            "IO-14",
            "IO-16",
            "IO-17",
        });
        prefabKeysByType.Add(IngameObject.EType.Flyer, new List<string>()
        {
            "IO-18",
            "IO-19",
            "IO-20",
        });
    }

    private void LoadTowerDatas()
    {
        /* 
            1. Tower Prefab 추가
            2. Tower Projectile Prefab 추가
            3. Tower Icon 추가
            4. Tower Buidl Skill 추가
        */
        CsvHelper_CsvParser.Deserialize<TowerData>("TowerData", (datas) =>
        {
            foreach (var d in datas)
            {
                towerDatas.Add(d.towerID, d);
            }
        });
    }

    private void LoadIngameObjectDatas()
    {
        ingameObjectDatas.Add("IOD-1-1", new IngameObjectData(IngameObject.EType.Walker, 1200, 0.20f, 400, 0.2f, 1));
        ingameObjectDatas.Add("IOD-1-2", new IngameObjectData(IngameObject.EType.Walker, 0950, 0.35f, 400, 0.2f, 1));
        ingameObjectDatas.Add("IOD-1-3", new IngameObjectData(IngameObject.EType.Walker, 0700, 0.50f, 400, 0.2f, 1));

        ingameObjectDatas.Add("IOD-1-1-1", new IngameObjectData(IngameObject.EType.Walker, Mathf.RoundToInt(1200 * 0.85f), 0.20f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, Mathf.RoundToInt(1200 * 0.3f)));
        ingameObjectDatas.Add("IOD-1-2-1", new IngameObjectData(IngameObject.EType.Walker, Mathf.RoundToInt(0950 * 0.85f), 0.35f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, Mathf.RoundToInt(0950 * 0.3f)));
        ingameObjectDatas.Add("IOD-1-3-1", new IngameObjectData(IngameObject.EType.Walker, Mathf.RoundToInt(0700 * 0.85f), 0.50f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, Mathf.RoundToInt(0700 * 0.3f)));

        ingameObjectDatas.Add("IOD-1-1-2", new IngameObjectData(IngameObject.EType.Walker, 1200, 0.20f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, 1200));
        ingameObjectDatas.Add("IOD-1-2-2", new IngameObjectData(IngameObject.EType.Walker, 0950, 0.35f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, 1200));
        ingameObjectDatas.Add("IOD-1-3-2", new IngameObjectData(IngameObject.EType.Walker, 0700, 0.50f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, 1200));

        ingameObjectDatas.Add("IOD-2-1", new IngameObjectData(IngameObject.EType.Destroyer, 1200, 0.20f, 400, 0.2f, 1));
        ingameObjectDatas.Add("IOD-2-2", new IngameObjectData(IngameObject.EType.Destroyer, 0950, 0.35f, 400, 0.2f, 1));
        ingameObjectDatas.Add("IOD-2-3", new IngameObjectData(IngameObject.EType.Destroyer, 0700, 0.50f, 400, 0.2f, 1));

        ingameObjectDatas.Add("IOD-2-1-1", new IngameObjectData(IngameObject.EType.Destroyer, Mathf.RoundToInt(1200 * 0.85f), 0.20f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, Mathf.RoundToInt(1200 * 0.3f)));
        ingameObjectDatas.Add("IOD-2-2-1", new IngameObjectData(IngameObject.EType.Destroyer, Mathf.RoundToInt(0950 * 0.85f), 0.35f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, Mathf.RoundToInt(0950 * 0.3f)));
        ingameObjectDatas.Add("IOD-2-3-1", new IngameObjectData(IngameObject.EType.Destroyer, Mathf.RoundToInt(0700 * 0.85f), 0.50f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, Mathf.RoundToInt(0700 * 0.3f)));

        ingameObjectDatas.Add("IOD-2-1-2", new IngameObjectData(IngameObject.EType.Destroyer, 1200, 0.20f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, 1200));
        ingameObjectDatas.Add("IOD-2-2-2", new IngameObjectData(IngameObject.EType.Destroyer, 0950, 0.35f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, 0950));
        ingameObjectDatas.Add("IOD-2-3-2", new IngameObjectData(IngameObject.EType.Destroyer, 0700, 0.50f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, 0700));

        ingameObjectDatas.Add("IOD-3-1", new IngameObjectData(IngameObject.EType.Flyer, 1200, 0.20f, 400, 0.2f, 1));
        ingameObjectDatas.Add("IOD-3-2", new IngameObjectData(IngameObject.EType.Flyer, 0950, 0.35f, 400, 0.2f, 1));
        ingameObjectDatas.Add("IOD-3-3", new IngameObjectData(IngameObject.EType.Flyer, 0700, 0.50f, 400, 0.2f, 1));

        ingameObjectDatas.Add("IOD-3-1-1", new IngameObjectData(IngameObject.EType.Flyer, Mathf.RoundToInt(1200 * 0.85f), 0.20f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, Mathf.RoundToInt(1200 * 0.3f)));
        ingameObjectDatas.Add("IOD-3-2-1", new IngameObjectData(IngameObject.EType.Flyer, Mathf.RoundToInt(0950 * 0.85f), 0.35f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, Mathf.RoundToInt(0950 * 0.3f)));
        ingameObjectDatas.Add("IOD-3-3-1", new IngameObjectData(IngameObject.EType.Flyer, Mathf.RoundToInt(0700 * 0.85f), 0.50f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, Mathf.RoundToInt(0700 * 0.3f)));

        ingameObjectDatas.Add("IOD-3-1-2", new IngameObjectData(IngameObject.EType.Flyer, 1200, 0.20f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, 1200));
        ingameObjectDatas.Add("IOD-3-2-2", new IngameObjectData(IngameObject.EType.Flyer, 0950, 0.35f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, 0950));
        ingameObjectDatas.Add("IOD-3-3-2", new IngameObjectData(IngameObject.EType.Flyer, 0700, 0.50f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, 0700));

        // 찌릿찌릿
        ingameObjectDatas.Add("IOD-4-1", new IngameObjectData(IngameObject.EType.Flyer, 0700, 0.50f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, 0, new string[] { "ShockWave_1_1" }));
        ingameObjectDatas.Add("IOD-4-2", new IngameObjectData(IngameObject.EType.Flyer, 0700, 0.50f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, 0, new string[] { "IceWave_1_1" }));

        ingameObjectDatas.Add("IOD-BOSS-1", new IngameObjectData(IngameObject.EType.Walker, Mathf.RoundToInt(1200 * 0.90f), 0.20f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, Mathf.RoundToInt(1200 * 0.3f), new string[] { "Heal_1_1" }));
        ingameObjectDatas.Add("IOD-BOSS-2", new IngameObjectData(IngameObject.EType.Walker, Mathf.RoundToInt(0950 * 0.75f), 0.35f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, Mathf.RoundToInt(0950 * 0.3f), new string[] { "ElectricShock_1_1" }));
        ingameObjectDatas.Add("IOD-BOSS-3", new IngameObjectData(IngameObject.EType.Walker, Mathf.RoundToInt(0700 * 0.75f), 0.50f, 400, 0.2f, 1, IngameObject.EClass.Normal, 0, 0, Mathf.RoundToInt(0700 * 0.3f), new string[] { "Freezing_1_1" }));
    }

    private void LoadIngameObjectSkillDatas()
    {
        ingameObjectSkills.Add("Heal_1_1", new IngameObject_.SkillData()
        {
            skillId = "Heal_1_1",
            skillDatas = new string[] { "3", "1", "0.01", "2", "1" }
        });

        ingameObjectSkills.Add("Freezing_1_1", new IngameObject_.SkillData()
        {
            skillId = "Freezing_1_1",
            skillDatas = new string[] { "6", "0.8", "0.5", "4", "1" }
        });

        ingameObjectSkills.Add("ElectricShock_1_1", new IngameObject_.SkillData()
        {
            skillId = "ElectricShock_1_1",
            skillDatas = new string[] { "8", "0.8", "1", "2", "1" }
        });

        ingameObjectSkills.Add("ShockWave_1_1", new IngameObject_.SkillData()
        {
            skillId = "ShockWave_1_1",
            skillDatas = new string[] { "0.6", "2" }
        });

        ingameObjectSkills.Add("IceWave_1_1", new IngameObject_.SkillData()
        {
            skillId = "IceWave_1_1",
            skillDatas = new string[] { "0.8", "3", "0.5" }
        });
    }

    private void LoadStageDatas()
    {
        #region < Stage Test >
        #if UNITY_EDITOR
        // Stage TEST
        stageDatas.Add(new StageData()
        {
            stageId = "SN_Test",
            startLife = 50,
            startCost = 400,
            lobbyZoomSize = 5,
            ingameZoomSize = 3,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "0_0_0_0_0_0_0_0_0/" +
            "0_2_2_2_2_2_2_2_0/" +
            "0_2_1_1_1_1_1_2_0/" +
            "0_3_1_1_7_1_1_2_0/" +
            "0_0_1_1_1_1_1_2_0/" +
            "0_4_2_2_2_2_2_2_0/" +
            "0_0_0_0_0_0_0_0_0/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo(1, "IO-24", "IOD-4-1",    10, 2, 0, 0.40f, 0.5f),
                new IngameObjectSpawnInfo(1, "IO-25", "IOD-4-2",    10, 2, 1, 0.40f, 0.5f),
                new IngameObjectSpawnInfo(2, "IO-24", "IOD-BOSS-2", 01, 1, 0, 0.5f, 1, 1, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(3, "IO-25", "IOD-BOSS-3", 01, 1, 0, 0.5f, 1, 1, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(4, "IO-26", "IOD-BOSS-1", 01, 1, 0, 0.5f, 1, 1, IngameObject.EClass.Boss),
            }
        });
        #endif
        #endregion

        #region < Stage 1~10 >

        // Stage 1
        stageDatas.Add(new StageData()
        {
            stageId = "SN_1",
            startLife = 50,
            startCost = 200,
            lobbyZoomSize = 5,
            ingameZoomSize = 3,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "0_0_0_0_0_0_0_0_0/" +
            "0_2_2_2_2_2_2_2_0/" +
            "0_2_1_0_0_0_1_2_0/" +
            "0_3_0_0_7_0_0_2_0/" +
            "0_0_0_1_0_0_1_2_0/" +
            "0_4_2_2_2_2_2_2_0/" +
            "0_0_0_0_0_0_0_0_0/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo(1, "IO-1", "IOD-1-1", 20, 1, 0, 0.6f, 1, 1, IngameObject.EClass.Normal),
                new IngameObjectSpawnInfo(2, "IO-1", "IOD-1-1", 20, 1, 0, 0.6f, 1, 1, IngameObject.EClass.Normal),
                new IngameObjectSpawnInfo(3, "IO-1", "IOD-1-1", 01, 1, 0, 0.1f, 1, 1, IngameObject.EClass.Boss),
            }
        });

        // Stage 2
        stageDatas.Add(new StageData()
        {
            stageId = "SN_2",
            startLife = 40,
            startCost = 200,
            lobbyZoomSize = 5,
            ingameZoomSize = 3.5f,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "0_0_0_0_0_0_0_0_3/" +
            "0_0_0_0_0_0_0_0_2/" +
            "2_2_2_2_2_0_0_1_2/" +
            "2_1_0_1_2_0_0_0_2/" +
            "2_0_0_0_2_0_1_0_2/" +
            "2_0_0_0_2_1_0_1_2/" +
            "4_0_0_0_2_2_2_2_2/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo(1, "IO-3", "IOD-1-2", 20, 1, 0, 0.6f),
                new IngameObjectSpawnInfo(2, "IO-3", "IOD-1-2", 20, 1, 0, 0.6f),
                new IngameObjectSpawnInfo(3, "IO-3", "IOD-1-2", 20, 1, 0, 0.65f),
                new IngameObjectSpawnInfo(4, "IO-3", "IOD-1-2", 20, 1, 0, 0.65f),
                new IngameObjectSpawnInfo(5, "IO-3", "IOD-1-2", 20, 1, 0, 0.7f),
                new IngameObjectSpawnInfo(6, "IO-3", "IOD-1-2", 01, 1, 0, 0.2f, 0.5f, 1, IngameObject.EClass.Boss)
            }
        });

        // Stage 3
        stageDatas.Add(new StageData()
        {
            stageId = "SN_3",
            startLife = 30,
            startCost = 200,
            lobbyZoomSize = 5,
            ingameZoomSize = 3.5f,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "2_2_2_2_0_2_2_2_2/" +
            "2_1_1_2_0_2_1_1_2/" +
            "2_1_0_2_0_2_0_1_2/" +
            "2_0_0_4_0_3_0_0_2/" +
            "2_1_0_0_0_0_0_1_2/" +
            "2_2_2_2_2_2_2_2_2/" +
            "0_0_0_1_1_1_0_0_0/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-1", "IOD-1-1", 20, 1, 0, 0.60f),
                new IngameObjectSpawnInfo( 2, "IO-3", "IOD-1-2", 20, 1, 0, 0.70f),
                new IngameObjectSpawnInfo( 3, "IO-1", "IOD-1-1", 20, 1, 0, 0.75f),
                new IngameObjectSpawnInfo( 4, "IO-3", "IOD-1-2", 20, 1, 0, 0.80f),
                new IngameObjectSpawnInfo( 5, "IO-1", "IOD-1-1", 01, 1, 0, 0.15f, 0.5f, 1, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-3", "IOD-1-2", 10, 2, 1, 0.30f, 0.5f),
                new IngameObjectSpawnInfo( 6, "IO-1", "IOD-1-1", 10, 2, 0, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 6, "IO-3", "IOD-1-2", 10, 2, 1, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 7, "IO-1", "IOD-1-1", 10, 2, 0, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 7, "IO-3", "IOD-1-2", 10, 2, 1, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 8, "IO-1", "IOD-1-1", 10, 2, 0, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 8, "IO-3", "IOD-1-2", 10, 2, 1, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 9, "IO-1", "IOD-1-1", 10, 2, 0, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 9, "IO-3", "IOD-1-2", 10, 2, 1, 0.40f, 0.5f),
                new IngameObjectSpawnInfo(10, "IO-1", "IOD-1-1", 10, 1, 0, 0.40f, 0.5f),
                new IngameObjectSpawnInfo(10, "IO-3", "IOD-1-2", 01, 1, 0, 0.40f, 0.5f, 1, IngameObject.EClass.Boss),
            }
        });

        // Stage 4
        stageDatas.Add(new StageData()
        {
            stageId = "SN_4",
            startLife = 20,
            startCost = 200,
            lobbyZoomSize = 5,
            ingameZoomSize = 3,
            tileSize = 0.36f,
            wallData = new WallData(200),
            mapData =
            "2_2_2_2_0_2_2_2/" +
            "4_0_1_2_2_2_1_2/" +
            "0_0_0_0_0_5_0_2/" +
            "0_0_0_1_2_2_2_2/" +
            "0_0_0_1_2_1_0_0/" +
            "2_2_2_2_2_1_0_0/" +
            "2_0_5_0_0_0_0_0/" +
            "2_1_2_2_2_1_0_3/" +
            "2_2_2_0_2_2_2_2/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-2", "IOD-1-3", 20, 1, 0, 0.60f),
                new IngameObjectSpawnInfo( 2, "IO-5", "IOD-2-1", 05, 4, 0, 0.70f),
                new IngameObjectSpawnInfo( 3, "IO-2", "IOD-1-3", 20, 1, 0, 0.75f),
                new IngameObjectSpawnInfo( 4, "IO-2", "IOD-1-3", 20, 1, 0, 0.80f),
                new IngameObjectSpawnInfo( 5, "IO-5", "IOD-2-1", 01, 1, 0, 0.25f, 1, 1, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 6, "IO-2", "IOD-1-3", 20, 1, 0, 0.80f),
                new IngameObjectSpawnInfo( 7, "IO-2", "IOD-1-3", 20, 1, 0, 0.80f),
                new IngameObjectSpawnInfo( 8, "IO-5", "IOD-2-1", 05, 4, 0, 0.85f),
                new IngameObjectSpawnInfo( 9, "IO-2", "IOD-1-3", 20, 1, 0, 0.85f),
                new IngameObjectSpawnInfo(10, "IO-5", "IOD-2-1", 01, 1, 0, 0.35f, 0.5f, 1, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-2", "IOD-1-3", 10, 1, 0, 0.35f, 0.5f),
            }
        });

        // Stage 5
        stageDatas.Add(new StageData()
        {
            stageId = "SN_5",
            startLife = 20,
            startCost = 200,
            lobbyZoomSize = 5,
            ingameZoomSize = 3.5f,
            tileSize = 0.36f,
            wallData = new WallData(200),
            mapData =
            "2_2_2_2_2_2_2_4/" +
            "2_1_5_1_0_0_1_0/" +
            "2_2_2_2_2_2_2_2/" +
            "0_1_0_0_1_5_1_2/" +
            "2_2_2_2_2_2_2_2/" +
            "2_1_5_1_0_0_1_0/" +
            "2_2_2_2_2_2_2_2/" +
            "0_1_0_0_1_5_1_2/" +
            "3_2_2_2_2_2_2_2/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-4", "IOD-1-3", 40, 0.5f, 0, 0.60f),
                new IngameObjectSpawnInfo( 2, "IO-5", "IOD-2-1", 05, 4.0f, 0, 0.70f),
                new IngameObjectSpawnInfo( 3, "IO-4", "IOD-1-3", 40, 0.5f, 0, 0.80f),
                new IngameObjectSpawnInfo( 4, "IO-4", "IOD-1-3", 40, 0.5f, 0, 0.80f),
                new IngameObjectSpawnInfo( 5, "IO-5", "IOD-2-1", 01, 1.0f, 0, 0.25f, 1, 1, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 6, "IO-4", "IOD-1-3", 40, 0.5f, 0, 0.80f),
                new IngameObjectSpawnInfo( 7, "IO-4", "IOD-1-3", 40, 0.5f, 0, 0.85f),
                new IngameObjectSpawnInfo( 8, "IO-5", "IOD-2-1", 05, 4.0f, 0, 0.85f),
                new IngameObjectSpawnInfo( 9, "IO-4", "IOD-1-3", 40, 0.5f, 0, 0.90f),
                new IngameObjectSpawnInfo(10, "IO-4", "IOD-1-3", 01, 1.0f, 0, 0.30f, 0.5f, 1, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-4", "IOD-1-3", 20, 1.0f, 1, 0.30f, 0.5f)
            }
        });

        // Stage 6
        stageDatas.Add(new StageData()
        {
            stageId = "SN_6",
            startLife = 20,
            startCost = 200,
            lobbyZoomSize = 5,
            ingameZoomSize = 4,
            tileSize = 0.36f,
            wallData = new WallData(800),
            mapData =
            "3_0_2_2_2_2_0_2_2_2/" +
            "2_1_2_1_1_2_0_2_1_2/" +
            "2_0_2_5_2_2_0_2_0_2/" +
            "2_0_2_0_2_1_1_2_0_2/" +
            "2_1_2_1_2_4_0_2_1_2/" +
            "2_0_2_1_0_0_1_2_0_2/" +
            "2_0_2_2_2_2_2_2_5_2/" +
            "2_1_0_1_0_0_0_1_1_2/" +
            "2_2_2_2_2_2_2_2_2_2/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-4", "IOD-1-3", 40, 0.5f, 0, 0.60f),
                new IngameObjectSpawnInfo( 2, "IO-5", "IOD-2-1", 05, 4.0f, 0, 0.70f),
                new IngameObjectSpawnInfo( 3, "IO-4", "IOD-1-3", 40, 0.5f, 0, 0.80f),
                new IngameObjectSpawnInfo( 4, "IO-4", "IOD-1-3", 40, 0.5f, 0, 0.80f),
                new IngameObjectSpawnInfo( 5, "IO-5", "IOD-2-1", 01, 4.0f, 0, 0.30f, 1, 1, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 6, "IO-4", "IOD-1-3", 40, 0.5f, 0, 0.80f),
                new IngameObjectSpawnInfo( 7, "IO-4", "IOD-1-3", 40, 0.5f, 0, 0.85f),
                new IngameObjectSpawnInfo( 8, "IO-5", "IOD-2-1", 05, 4.0f, 0, 0.85f),
                new IngameObjectSpawnInfo( 9, "IO-4", "IOD-1-3", 40, 0.5f, 0, 0.90f),
                new IngameObjectSpawnInfo(10, "IO-4", "IOD-1-3", 01, 1.0f, 0, 0.35f, 0.5f, 1, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-4", "IOD-1-3", 20, 1.0f, 1, 0.35f, 0.5f),
            }
        });

        // Stage 7
        stageDatas.Add(new StageData()
        {
            stageId = "SN_7",
            startLife = 20,
            startCost = 250,
            lobbyZoomSize = 5,
            ingameZoomSize = 3.5f,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "4_2_2_2_2_2_2/" +
            "0_1_0_0_0_1_2/" +
            "0_1_0_0_0_1_2/" +
            "0_1_0_1_0_1_2/" +
            "2_2_2_2_2_2_2/" +
            "2_1_0_1_0_1_0/" +
            "2_1_0_0_0_1_0/" +
            "2_1_0_1_0_1_0/" +
            "2_2_2_2_2_2_3/",
            mapSkyData =
            "4_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0/" +
            "6_6_6_6_6_6_6/" +
            "0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_3/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-5" , "IOD-1-1", 10, 2, 0, 0.30f, 0.5f),
                new IngameObjectSpawnInfo( 1, "IO-18", "IOD-3-3", 10, 2, 1, 0.30f, 0.5f),
                new IngameObjectSpawnInfo( 2, "IO-5" , "IOD-1-1", 10, 2, 0, 0.35f, 0.5f),
                new IngameObjectSpawnInfo( 2, "IO-18", "IOD-3-3", 10, 2, 1, 0.35f, 0.5f),
                new IngameObjectSpawnInfo( 3, "IO-5" , "IOD-1-1", 10, 2, 0, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 3, "IO-18", "IOD-3-3", 10, 2, 1, 0.35f, 0.5f),
                new IngameObjectSpawnInfo( 4, "IO-5" , "IOD-2-1", 10, 2, 0, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 4, "IO-18", "IOD-3-3", 10, 2, 1, 0.35f, 0.5f),
                new IngameObjectSpawnInfo( 5, "IO-5" , "IOD-1-1", 01, 2, 0, 0.30f, 0.5f, 1, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-18", "IOD-3-3", 10, 2, 1, 0.35f, 0.5f),
                new IngameObjectSpawnInfo( 6, "IO-5" , "IOD-1-1", 10, 2, 0, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 6, "IO-18", "IOD-3-3", 10, 2, 1, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 7, "IO-5" , "IOD-1-1", 10, 2, 0, 0.45f, 0.5f),
                new IngameObjectSpawnInfo( 7, "IO-18", "IOD-3-3", 10, 2, 1, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 8, "IO-5" , "IOD-1-1", 10, 2, 0, 0.45f, 0.5f),
                new IngameObjectSpawnInfo( 8, "IO-18", "IOD-3-3", 10, 2, 1, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 9, "IO-5" , "IOD-1-1", 10, 2, 0, 0.50f, 0.5f),
                new IngameObjectSpawnInfo( 9, "IO-18", "IOD-3-3", 10, 2, 1, 0.45f, 0.5f),
                new IngameObjectSpawnInfo(10, "IO-5" , "IOD-1-1", 01, 2, 0, 0.35f, 0.5f, 1, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-18", "IOD-3-3", 10, 2, 1, 0.45f, 0.5f),
            }
        });

        // Stage 8
        stageDatas.Add(new StageData()
        {
            stageId = "SN_8",
            startLife = 20,
            startCost = 250,
            lobbyZoomSize = 5,
            ingameZoomSize = 4.4f,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "0_0_0_2_2_2_2_2_0_0_0_0/" +
            "0_0_0_2_1_0_1_2_1_0_1_0/" +
            "0_0_0_2_0_0_2_2_0_1_0_0/" +
            "4_2_2_2_1_0_2_0_3_2_2_2/" +
            "0_0_0_0_0_0_2_2_0_1_0_2/" +
            "0_0_0_0_1_0_1_2_1_0_1_2/" +
            "0_0_0_0_0_0_0_2_2_2_2_2",
            mapSkyData =
            "0_0_0_0_0_0_0_6_6_6_6_6/" +
            "0_0_0_0_0_0_0_6_0_0_0_6/" +
            "0_0_0_0_0_0_6_6_0_0_0_6/" +
            "4_6_6_6_0_0_6_0_3_6_6_6/" +
            "0_0_0_6_0_0_6_6_0_0_0_0/" +
            "0_0_0_6_0_0_0_6_0_0_0_0/" +
            "0_0_0_6_6_6_6_6_0_0_0_0",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-6" , "IOD-1-1", 05, 4, 0, 0.35f, 0.5f),
                new IngameObjectSpawnInfo( 1, "IO-18", "IOD-3-3", 10, 2, 1, 0.35f, 0.5f),
                new IngameObjectSpawnInfo( 2, "IO-6" , "IOD-1-1", 05, 4, 0, 0.35f, 0.5f),
                new IngameObjectSpawnInfo( 2, "IO-18", "IOD-3-3", 10, 2, 1, 0.35f, 0.5f),
                new IngameObjectSpawnInfo( 3, "IO-6" , "IOD-1-1", 05, 4, 0, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 3, "IO-18", "IOD-3-3", 10, 2, 1, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 4, "IO-6" , "IOD-2-1", 05, 4, 0, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 4, "IO-18", "IOD-3-3", 10, 2, 1, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 5, "IO-6" , "IOD-1-1", 01, 2, 0, 0.25f, 0.5f, 1, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-18", "IOD-3-3", 10, 2, 1, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 6, "IO-6" , "IOD-1-1", 05, 4, 0, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 6, "IO-18", "IOD-3-3", 10, 2, 1, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 7, "IO-6" , "IOD-1-1", 05, 4, 0, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 7, "IO-18", "IOD-3-3", 10, 2, 1, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 8, "IO-6" , "IOD-1-1", 05, 4, 0, 0.45f, 0.5f),
                new IngameObjectSpawnInfo( 8, "IO-18", "IOD-3-3", 10, 2, 1, 0.45f, 0.5f),
                new IngameObjectSpawnInfo( 9, "IO-6" , "IOD-1-1", 05, 4, 0, 0.45f, 0.5f),
                new IngameObjectSpawnInfo( 9, "IO-18", "IOD-3-3", 10, 2, 1, 0.45f, 0.5f),
                new IngameObjectSpawnInfo(10, "IO-6" , "IOD-1-1", 01, 2, 0, 0.30f, 0.5f, 1, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-18", "IOD-3-3", 10, 2, 1, 0.30f, 0.5f),
            }
        });

        // Stage 9
        stageDatas.Add(new StageData()
        {
            stageId = "SN_9",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 5,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "0_0_0_0_0_0_0_0_0_1_0_1_0_0/" +
            "0_0_0_0_1_0_1_2_2_2_2_2_2_0/" +
            "0_0_0_0_0_0_1_2_1_0_0_0_2_3/" +
            "4_2_2_2_2_2_2_2_1_0_1_0_0_0/" +
            "0_0_0_0_1_0_1_0_0_0_0_0_0_0",
            mapSkyData =
            "0_0_0_0_0_0_0_0_0_0_0_0_0_0/" +
            "0_0_0_0_0_0_0_0_0_0_0_0_0_0/" +
            "6_6_6_6_6_6_6_6_6_6_6_6_6_3/" +
            "4_0_0_0_0_0_0_0_0_0_0_0_0_0/" +
            "0_0_0_0_0_0_0_0_0_0_0_0_0_0",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-7" , "IOD-1-1", 10, 2, 0, 0.30f, 0.5f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-2", 10, 2, 1, 0.25f, 0.5f),
                new IngameObjectSpawnInfo( 2, "IO-7" , "IOD-1-1", 10, 2, 0, 0.30f, 0.5f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-2", 10, 2, 1, 0.25f, 0.5f),
                new IngameObjectSpawnInfo( 3, "IO-7" , "IOD-1-1", 10, 2, 0, 0.35f, 0.5f),
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-2", 10, 2, 1, 0.30f, 0.5f),
                new IngameObjectSpawnInfo( 4, "IO-7" , "IOD-2-1", 10, 2, 0, 0.35f, 0.5f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-2", 10, 2, 1, 0.30f, 0.5f),
                new IngameObjectSpawnInfo( 5, "IO-7" , "IOD-1-1", 01, 2, 0, 0.20f, 0.5f, 1, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-2", 10, 2, 1, 0.30f, 0.5f),
                new IngameObjectSpawnInfo( 6, "IO-7" , "IOD-1-1", 10, 2, 0, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-2", 10, 2, 1, 0.35f, 0.5f),
                new IngameObjectSpawnInfo( 7, "IO-7" , "IOD-1-1", 10, 2, 0, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-2", 10, 2, 1, 0.35f, 0.5f),
                new IngameObjectSpawnInfo( 8, "IO-7" , "IOD-1-1", 10, 2, 0, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-2", 10, 2, 1, 0.35f, 0.5f),
                new IngameObjectSpawnInfo( 9, "IO-7" , "IOD-1-1", 10, 2, 0, 0.45f, 0.5f),
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-2", 10, 2, 1, 0.40f, 0.5f),
                new IngameObjectSpawnInfo(10, "IO-7" , "IOD-1-1", 01, 2, 0, 0.25f, 0.5f, 1, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-2", 10, 2, 1, 0.40f, 0.5f),
            }
        });

        // Stage 10
        stageDatas.Add(new StageData()
        {
            stageId = "SN_10",
            startLife = 20,
            startCost = 250,
            lobbyZoomSize = 5,
            ingameZoomSize = 3.5f,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "0_0_0_4_0_3_0_0_0/" +
            "0_1_0_2_1_2_0_1_0/" +
            "0_0_0_2_0_2_0_0_0/" +
            "2_2_2_2_0_2_2_2_2/" +
            "2_1_0_0_1_0_0_1_2/" +
            "2_1_0_0_1_0_0_1_2/" +
            "2_2_2_2_2_2_2_2_2/" +
            "0_0_0_1_1_1_0_0_0",
            mapSkyData =
            "6_6_6_4_3_6_6_6_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_6_6_6_6_6_6_6_6/" +
            "0_0_0_0_0_0_0_0_0",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-7" , "IOD-1-2", 20, 2, 0, 0.35f, 0.5f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-2", 10, 2, 1, 0.35f, 0.5f),
                new IngameObjectSpawnInfo( 2, "IO-7" , "IOD-1-2", 20, 2, 0, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-2", 10, 2, 1, 0.40f, 0.5f),
                new IngameObjectSpawnInfo( 3, "IO-7" , "IOD-1-2", 20, 2, 0, 0.42f, 0.5f),
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-2", 10, 2, 1, 0.42f, 0.5f),
                new IngameObjectSpawnInfo( 4, "IO-7" , "IOD-2-2", 20, 2, 0, 0.44f, 0.5f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-2", 10, 2, 1, 0.44f, 0.5f),
                new IngameObjectSpawnInfo( 5, "IO-7" , "IOD-1-2", 01, 2, 0, 0.30f, 0.5f, 1, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-2", 10, 2, 1, 0.35f, 0.5f),
                new IngameObjectSpawnInfo( 6, "IO-7" , "IOD-1-2", 20, 2, 0, 0.46f, 0.5f),
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-2", 10, 2, 1, 0.46f, 0.5f),
                new IngameObjectSpawnInfo( 7, "IO-7" , "IOD-2-2", 20, 2, 0, 0.48f, 0.5f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-2", 10, 2, 1, 0.48f, 0.5f),
                new IngameObjectSpawnInfo( 8, "IO-7" , "IOD-1-2", 20, 2, 0, 0.50f, 0.5f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-2", 10, 2, 1, 0.50f, 0.5f),
                new IngameObjectSpawnInfo( 9, "IO-7" , "IOD-1-2", 20, 2, 0, 0.52f, 0.5f),
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-2", 10, 2, 1, 0.52f, 0.5f),
                new IngameObjectSpawnInfo(10, "IO-7" , "IOD-1-2", 01, 2, 0, 0.35f, 0.5f, 1, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-2", 10, 2, 1, 0.35f, 0.5f),
            }
        });

        #endregion

        #region < Stage 11~20 >

        // Stage 11
        stageDatas.Add(new StageData()
        {
            stageId = "SN_11",
            startLife = 20,
            startCost = 250,
            lobbyZoomSize = 5,
            ingameZoomSize = 3.5f,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "2_2_2_2_2_2_2_2_2_2/" +
            "2_1_0_0_0_0_0_0_1_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_0_0_0_1_1_0_0_0_2/" +
            "2_0_0_0_1_1_0_0_0_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_1_1_4_0_3_0_0_1_2/" +
            "2_2_2_2_0_2_2_2_2_2/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-9", "IOD-1-1", 20, 1, 00, 0.70f, 1.0f, 1.0f),
                new IngameObjectSpawnInfo( 2, "IO-7", "IOD-1-2", 20, 1, 00, 0.80f, 1.0f, 1.0f),
                new IngameObjectSpawnInfo( 3, "IO-9", "IOD-1-1", 20, 1, 00, 0.90f, 1.0f, 1.0f),
                new IngameObjectSpawnInfo( 4, "IO-7", "IOD-1-2", 20, 1, 00, 1.00f, 1.0f, 1.0f),
                new IngameObjectSpawnInfo( 5, "IO-9", "IOD-1-1", 01, 1, 00, 0.35f, 0.5f, 1.0f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-7", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.0f),
                new IngameObjectSpawnInfo( 6, "IO-9", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.0f),
                new IngameObjectSpawnInfo( 6, "IO-7", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.0f),
                new IngameObjectSpawnInfo( 7, "IO-9", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.0f),
                new IngameObjectSpawnInfo( 7, "IO-7", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.0f),
                new IngameObjectSpawnInfo( 8, "IO-9", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.0f),
                new IngameObjectSpawnInfo( 8, "IO-7", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.0f),
                new IngameObjectSpawnInfo( 9, "IO-9", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.0f),
                new IngameObjectSpawnInfo( 9, "IO-7", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.0f),
                new IngameObjectSpawnInfo(10, "IO-9", "IOD-1-1", 01, 2, 00, 0.45f, 0.5f, 1.0f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-7", "IOD-1-2", 10, 2, 01, 0.45f, 0.5f, 1.0f),
            }
        });

        // Stage 12
        stageDatas.Add(new StageData()
        {
            stageId = "SN_12",
            startLife = 20,
            startCost = 250,
            lobbyZoomSize = 6.2f,
            ingameZoomSize = 4.5f,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "2_2_2_2_2_2_2_2_2_4/" +
            "2_1_0_0_0_0_0_0_0_0/" +
            "2_0_0_0_0_0_0_0_0_0/" +
            "2_0_0_0_0_1_1_0_0_0/" +
            "2_1_0_0_0_0_0_0_0_0/" +
            "2_2_2_2_0_0_0_0_0_0/" +
            "0_0_1_2_0_1_1_0_0_0/" +
            "0_0_1_2_0_1_1_0_0_0/" +
            "2_2_2_2_0_0_0_0_0_0/" +
            "2_1_0_0_0_0_0_0_0_0/" +
            "2_0_0_0_0_1_1_0_0_0/" +
            "2_0_0_0_0_0_0_0_0_0/" +
            "2_1_0_0_0_0_0_0_0_0/" +
            "2_2_2_2_2_2_2_2_2_3/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-9" , "IOD-1-1", 20, 1, 00, 0.70f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-11", "IOD-1-3", 20, 1, 00, 0.80f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-9" , "IOD-1-1", 20, 1, 00, 0.90f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-11", "IOD-1-3", 20, 1, 00, 1.00f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-9" , "IOD-1-1", 01, 1, 00, 0.35f, 0.5f, 1.50f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-11", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 6, "IO-9" , "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 6, "IO-11", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 7, "IO-9" , "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 7, "IO-11", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 8, "IO-9" , "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 8, "IO-11", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 9, "IO-9" , "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 9, "IO-11", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo(10, "IO-9" , "IOD-1-1", 10, 2, 00, 0.45f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(10, "IO-11", "IOD-1-3", 01, 2, 01, 0.45f, 0.5f, 1.50f, IngameObject.EClass.Boss),
            }
        });

        // Stage 13
        stageDatas.Add(new StageData()
        {
            stageId = "SN_13",
            startLife = 20,
            startCost = 250,
            lobbyZoomSize = 6.2f,
            ingameZoomSize = 4.5f,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "2_2_2_2_2_2_2_2_2_2/" +
            "2_1_0_0_0_0_0_0_0_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_0_0_0_1_1_0_0_0_2/" +
            "2_0_0_0_1_1_0_0_0_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_0_4_2_2_2_2_2_1_2/" +
            "2_1_0_0_0_0_1_2_0_2/" +
            "2_2_2_2_2_2_2_2_0_2/" +
            "0_0_0_1_1_0_0_0_0_3/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-9" , "IOD-1-1", 20, 1, 00, 0.70f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-12", "IOD-1-2", 20, 1, 00, 0.80f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-9" , "IOD-1-1", 20, 1, 00, 0.90f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-12", "IOD-1-2", 20, 1, 00, 1.00f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-9" , "IOD-1-1", 01, 1, 00, 0.40f, 0.5f, 1.50f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-12", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 6, "IO-9" , "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 6, "IO-12", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 7, "IO-9" , "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 7, "IO-12", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 8, "IO-9" , "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 8, "IO-12", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 9, "IO-9" , "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 9, "IO-12", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo(10, "IO-9" , "IOD-1-1", 10, 2, 01, 0.50f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(10, "IO-12", "IOD-1-2", 01, 2, 00, 0.50f, 0.5f, 1.50f, IngameObject.EClass.Boss),
            }
        });

        // Stage 14
        stageDatas.Add(new StageData()
        {
            stageId = "SN_14",
            startLife = 20,
            startCost = 250,
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 4,
            tileSize = 0.36f,
            wallData = new WallData(400),
            mapData =
            "2_2_2_2_2_2_2_2_2_2/" +
            "2_1_5_1_0_0_1_0_1_2/" +
            "2_2_2_2_0_0_0_0_0_2/" +
            "0_1_5_2_0_0_1_0_0_2/" +
            "2_2_2_2_0_0_0_0_0_2/" +
            "2_1_5_1_0_0_1_0_1_2/" +
            "2_2_2_2_0_0_0_0_0_2/" +
            "0_1_5_2_0_0_1_0_0_2/" +
            "2_2_2_2_0_0_0_0_0_2/" +
            "2_1_5_1_0_0_1_0_1_2/" +
            "2_2_2_2_2_2_2_4_0_3/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-10", "IOD-1-3", 20, 1.0f, 00, 0.70f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-16", "IOD-2-1", 10, 2.0f, 00, 0.80f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-10", "IOD-1-3", 40, 0.5f, 00, 0.90f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-10", "IOD-1-3", 20, 1.0f, 00, 1.00f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-10", "IOD-1-3", 20, 1.0f, 01, 0.35f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo( 5, "IO-16", "IOD-2-1", 01, 1.0f, 00, 0.40f, 0.5f, 1.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 6, "IO-10", "IOD-1-3", 40, 0.5f, 00, 1.00f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 7, "IO-10", "IOD-1-3", 20, 1.0f, 00, 1.00f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 8, "IO-16", "IOD-2-1", 10, 2.0f, 00, 1.00f, 1.0f, 1.05f),
                new IngameObjectSpawnInfo( 9, "IO-10", "IOD-1-3", 40, 0.5f, 00, 1.00f, 1.0f, 1.10f),
                new IngameObjectSpawnInfo(10, "IO-10", "IOD-1-3", 01, 2.0f, 00, 0.50f, 0.5f, 1.50f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-16", "IOD-2-1", 10, 2.0f, 01, 0.50f, 0.5f, 1.00f),
            }
        });

        // Stage 15
        stageDatas.Add(new StageData()
        {
            stageId = "SN_15",
            startLife = 20,
            startCost = 250,
            lobbyZoomSize = 6f,
            ingameZoomSize = 4.8f,
            tileSize = 0.36f,
            wallData = new WallData(450),
            mapData =
            "0_1_1_1_1_0_0_2_2_2_0_0_1_0/" +
            "0_0_0_0_0_0_0_2_0_2_0_0_1_0/" +
            "0_0_0_0_0_0_1_2_1_2_0_0_1_0/" +
            "2_2_2_2_2_2_2_2_5_2_0_0_1_0/" +
            "2_0_1_5_0_0_0_0_0_3_0_0_0_0/" +
            "2_2_2_2_2_0_1_1_0_1_0_0_0_0/" +
            "0_0_0_1_2_0_1_1_4_2_2_2_2_2/" +
            "0_0_0_0_2_0_0_0_0_0_5_1_0_2/" +
            "0_1_0_0_2_0_2_2_2_2_2_2_2_2/" +
            "0_1_0_0_2_5_2_1_0_0_0_0_0_0/" +
            "0_1_0_0_2_1_2_0_0_0_0_0_0_0/" +
            "0_1_0_0_2_0_2_0_0_1_1_1_1_0/" +
            "0_0_0_0_2_2_2_0_0_0_0_0_0_0/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-10", "IOD-1-2", 20, 1, 00, 0.70f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-16", "IOD-2-1", 10, 2, 00, 0.80f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-10", "IOD-1-2", 20, 1, 00, 0.90f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-10", "IOD-1-2", 20, 1, 00, 1.00f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-10", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-16", "IOD-2-1", 01, 1, 00, 0.40f, 0.5f, 1.55f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 6, "IO-10", "IOD-1-2", 20, 1, 00, 1.00f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 7, "IO-10", "IOD-1-2", 20, 1, 00, 1.00f, 1.0f, 1.05f),
                new IngameObjectSpawnInfo( 8, "IO-16", "IOD-2-1", 10, 2, 00, 1.00f, 1.0f, 1.10f),
                new IngameObjectSpawnInfo( 9, "IO-10", "IOD-1-2", 20, 1, 00, 1.00f, 1.0f, 1.10f),
                new IngameObjectSpawnInfo(10, "IO-10", "IOD-1-2", 01, 2, 00, 0.50f, 0.5f, 1.55f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-16", "IOD-2-1", 10, 2, 01, 0.50f, 0.5f, 1.55f),
            }
        });

        // Stage 16
        stageDatas.Add(new StageData()
        {
            stageId = "SN_16",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.6f,
            ingameZoomSize = 4,
            tileSize = 0.36f,
            wallData = new WallData(500),
            mapData =
            "2_2_2_2_2_0_0_2_2_2_2/" +
            "2_1_0_0_2_0_0_2_0_1_2/" +
            "2_2_2_0_2_0_1_2_0_0_2/" +
            "0_0_2_5_2_2_2_2_0_0_2/" +
            "2_2_2_0_1_0_0_0_0_1_2/" +
            "2_1_0_0_0_1_1_0_0_0_2/" +
            "2_2_2_0_0_0_0_0_0_0_2/" +
            "0_0_2_5_2_2_2_2_0_1_2/" +
            "2_2_2_1_2_1_0_2_1_0_2/" +
            "2_1_0_0_2_0_0_2_0_0_2/" +
            "2_2_2_2_2_0_0_4_0_0_3/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-10", "IOD-1-3", 20, 1, 00, 0.70f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-16", "IOD-2-1", 10, 1, 00, 0.80f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-10", "IOD-1-3", 40, 1, 00, 0.90f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-10", "IOD-1-3", 20, 1, 00, 1.00f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-10", "IOD-1-3", 20, 1, 01, 0.50f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-16", "IOD-2-1", 01, 1, 00, 0.40f, 1.0f, 1.60f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 6, "IO-10", "IOD-1-3", 40, 2, 00, 1.00f, 1.0f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-10", "IOD-1-3", 20, 2, 00, 1.00f, 1.0f, 1.05f),
                new IngameObjectSpawnInfo( 8, "IO-16", "IOD-2-1", 10, 2, 00, 1.00f, 1.0f, 1.10f),
                new IngameObjectSpawnInfo( 9, "IO-10", "IOD-1-3", 40, 2, 00, 1.00f, 1.0f, 1.10f),
                new IngameObjectSpawnInfo(10, "IO-10", "IOD-1-3", 01, 2, 00, 0.50f, 0.5f, 1.60f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-16", "IOD-2-1", 10, 2, 01, 0.50f, 0.5f, 1.55f),
            }
        });

        // Stage 17
        stageDatas.Add(new StageData()
        {
            stageId = "SN_17",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.4f,
            ingameZoomSize = 3.5f,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "2_2_2_2_2_2_2_2_2_2/" +
            "2_1_0_0_0_0_0_0_1_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_0_0_0_1_1_0_0_0_2/" +
            "2_0_0_0_1_1_0_0_0_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_1_1_4_0_3_0_0_1_2/" +
            "2_2_2_2_0_2_2_2_2_2/",
            mapSkyData =
            "0_0_0_0_0_0_0_0_0_0/" +
            "0_6_6_6_6_6_6_6_6_0/" +
            "0_6_0_0_0_0_0_0_6_0/" +
            "0_6_0_0_0_0_0_0_6_0/" +
            "0_6_0_0_0_0_0_0_6_0/" +
            "0_6_0_0_0_0_0_0_6_0/" +
            "0_6_0_0_0_0_0_0_6_0/" +
            "0_6_0_0_0_0_0_0_6_0/" +
            "0_6_6_4_0_3_6_6_6_0/" +
            "0_0_0_0_0_0_0_0_0_0/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-5" , "IOD-1-1", 10, 2, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-18", "IOD-3-3", 10, 2, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-5" , "IOD-1-1", 10, 2, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-18", "IOD-3-3", 10, 2, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-5" , "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-18", "IOD-3-3", 10, 2, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-5" , "IOD-2-1", 10, 2, 00, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-18", "IOD-3-3", 10, 2, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 5, "IO-5" , "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 1.60f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-18", "IOD-3-3", 10, 2, 01, 0.50f, 0.5f, 1.55f),
                new IngameObjectSpawnInfo( 6, "IO-5" , "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 6, "IO-18", "IOD-3-3", 10, 2, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-5" , "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-18", "IOD-3-3", 10, 2, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 8, "IO-5" , "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 8, "IO-18", "IOD-3-3", 10, 2, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 9, "IO-5" , "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 9, "IO-18", "IOD-3-3", 10, 2, 01, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(10, "IO-5" , "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 1.60f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-18", "IOD-3-3", 10, 2, 01, 0.50f, 0.5f, 1.55f),
            }
        });

        // Stage 18
        stageDatas.Add(new StageData()
        {
            stageId = "SN_18",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.2f,
            ingameZoomSize = 4,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "2_2_2_1_2_2_2_0_4/" +
            "2_1_2_0_2_0_2_1_2/" +
            "2_0_2_0_2_0_2_0_2/" +
            "2_0_2_1_2_1_2_0_2/" +
            "2_1_2_0_2_0_2_1_2/" +
            "2_0_2_1_2_1_2_0_2/" +
            "2_0_2_0_2_0_2_0_2/" +
            "2_1_2_0_2_0_2_1_2/" +
            "3_0_2_2_2_1_2_2_2/",
            mapSkyData =
            "6_6_6_6_6_6_6_6_4/" +
            "6_0_0_0_0_0_0_0_0/" +
            "6_6_6_6_6_6_6_6_6/" +
            "0_0_0_0_0_0_0_0_6/" +
            "6_6_6_6_6_6_6_6_6/" +
            "6_0_0_0_0_0_0_0_0/" +
            "6_6_6_6_6_6_6_6_6/" +
            "0_0_0_0_0_0_0_0_6/" +
            "3_6_6_6_6_6_6_6_6/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-7" , "IOD-1-2", 10, 2, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-2", 10, 2, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-7" , "IOD-1-2", 10, 2, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-2", 10, 2, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-7" , "IOD-1-2", 10, 2, 00, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-7" , "IOD-2-2", 10, 2, 00, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 5, "IO-7" , "IOD-1-2", 01, 2, 00, 0.50f, 0.5f, 1.60f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f, 0.5f, 1.60f),
                new IngameObjectSpawnInfo( 6, "IO-7" , "IOD-1-2", 10, 2, 00, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-7" , "IOD-1-2", 10, 2, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 8, "IO-7" , "IOD-1-2", 10, 2, 00, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 9, "IO-7" , "IOD-1-2", 10, 2, 00, 0.50f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(10, "IO-7" , "IOD-1-2", 01, 2, 00, 0.50f, 0.5f, 1.60f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f, 0.5f, 1.60f),
            }
        });

        // Stage 19
        stageDatas.Add(new StageData()
        {
            stageId = "SN_19",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 6.2f,
            ingameZoomSize = 3.5f,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "2_2_2_2_2_2_2_2_2_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_0_0_0_1_1_0_0_0_2/" +
            "2_0_0_0_1_1_0_0_0_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_0_0_0_0_0_0_0_0_2/" +
            "2_0_4_2_2_2_2_2_1_2/" +
            "2_1_0_0_0_0_1_2_0_2/" +
            "2_2_2_2_2_2_2_2_0_2/" +
            "0_0_0_1_1_0_0_0_0_3/",
            mapSkyData =
            "0_0_0_0_0_0_0_0_0_0/" +
            "0_0_0_0_0_0_0_0_0_0/" +
            "0_0_0_0_0_0_0_0_0_0/" +
            "0_0_0_6_6_6_6_0_0_0/" +
            "0_0_0_6_0_0_6_0_0_0/" +
            "0_0_0_6_0_0_6_0_0_0/" +
            "0_0_0_6_0_0_6_0_0_0/" +
            "0_0_0_6_6_0_6_0_0_0/" +
            "0_0_0_0_6_0_6_0_0_0/" +
            "6_6_4_0_6_0_6_0_0_0/" +
            "6_0_0_0_6_0_6_0_0_0/" +
            "6_0_0_0_6_0_6_0_0_0/" +
            "6_6_6_6_6_0_6_6_6_3/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-9" , "IOD-1-1", 10, 2, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-18", "IOD-3-3", 10, 2, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-9" , "IOD-1-1", 10, 2, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-18", "IOD-3-3", 10, 2, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-9" , "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-18", "IOD-3-3", 10, 2, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-9" , "IOD-2-1", 10, 2, 00, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-18", "IOD-3-3", 10, 2, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 5, "IO-9" , "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 1.65f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-18", "IOD-3-3", 10, 2, 01, 0.50f, 0.5f, 1.60f),
                new IngameObjectSpawnInfo( 6, "IO-9" , "IOD-1-1", 10, 2, 00, 0.51f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 6, "IO-18", "IOD-3-3", 10, 2, 01, 0.51f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-9" , "IOD-1-1", 10, 2, 00, 0.52f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 7, "IO-18", "IOD-3-3", 10, 2, 01, 0.52f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 8, "IO-9" , "IOD-1-1", 10, 2, 00, 0.53f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 8, "IO-18", "IOD-3-3", 10, 2, 01, 0.53f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 9, "IO-9" , "IOD-1-1", 10, 2, 00, 0.54f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 9, "IO-18", "IOD-3-3", 10, 2, 01, 0.54f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(10, "IO-9" , "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 1.65f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-18", "IOD-3-3", 10, 2, 10, 0.50f, 0.5f, 1.60f),
            }
        });

        // Stage 20
        stageDatas.Add(new StageData()
        {
            stageId = "SN_20",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 6f,
            ingameZoomSize = 5f,
            tileSize = 0.36f,
            wallData = new WallData(600),
            mapData =
            "0_1_0_0_1_0_0_2_2_2_0_0_1_0/" +
            "0_0_0_0_0_0_0_2_0_2_0_0_0_0/" +
            "0_0_0_0_0_0_1_2_1_2_0_0_0_0/" +
            "2_2_2_2_2_2_2_2_5_2_0_0_1_0/" +
            "2_0_1_5_0_0_0_0_0_3_0_0_0_0/" +
            "2_2_2_2_2_0_1_1_0_1_0_0_0_0/" +
            "0_0_0_1_2_0_1_1_4_2_2_2_2_2/" +
            "0_0_0_0_2_0_0_0_0_0_5_1_0_2/" +
            "0_1_0_0_2_0_2_2_2_2_2_2_2_2/" +
            "0_0_0_0_2_5_2_1_0_0_0_0_0_0/" +
            "0_0_0_0_2_1_2_0_0_0_0_0_0_0/" +
            "0_1_0_0_2_0_2_0_0_1_0_0_1_0/" +
            "0_0_0_0_2_2_2_0_0_0_0_0_0_0/",
            mapSkyData =
            "6_6_6_6_6_6_6_6_6_6_6_6_6_6/" +
            "6_0_0_0_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_0_3_6_6_6_6/" +
            "6_0_0_0_0_0_0_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0_0_4_6_6_6_6_6/" +
            "6_0_0_0_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_0_0_0_0_0_6/" +
            "6_6_6_6_6_6_6_6_6_6_6_6_6_6/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-9" , "IOD-1-1", 10, 2, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-2", 10, 2, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-9" , "IOD-1-1", 10, 2, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-2", 10, 2, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-9" , "IOD-2-1", 05, 4, 00, 0.50f, 0.5f, 1.00f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-9" , "IOD-2-1", 10, 2, 00, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-9" , "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 1.65f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f, 0.5f, 1.65f),
                new IngameObjectSpawnInfo( 6, "IO-9" , "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-2", 10, 2, 01, 0.52f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-9" , "IOD-1-1", 10, 2, 00, 0.54f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-2", 10, 2, 01, 0.54f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 8, "IO-9" , "IOD-1-1", 10, 2, 00, 0.56f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-2", 10, 2, 01, 0.56f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 9, "IO-9" , "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-2", 10, 2, 01, 0.58f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(10, "IO-9" , "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 1.65f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f, 0.5f, 1.65f),
            }
        });

        #endregion

        #region < Stage 21~30 >

        // Stage 21
        stageDatas.Add(new StageData()
        {
            stageId = "SN_21",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5,
            ingameZoomSize = 3.5f,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "2_2_2_0_1_1_1_0_0_0_0_0_0/" +
            "2_1_2_0_0_0_0_0_1_1_0_0_0/" +
            "2_0_2_0_2_2_2_0_1_1_0_0_0/" +
            "2_0_2_1_2_1_2_0_0_0_0_0_0/" +
            "2_1_2_0_2_0_2_0_2_2_2_1_0/" +
            "2_0_2_0_2_0_2_0_2_1_2_1_0/" +
            "2_0_2_1_2_0_2_1_2_0_2_1_0/" +
            "4_0_2_2_2_0_2_2_2_0_3_1_0/" +
            "1_0_0_0_0_1_0_0_0_0_0_0_0/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-11", "IOD-1-2", 40, 0.5f, 00, 0.80f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-12", "IOD-1-3", 40, 0.5f, 00, 0.90f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-11", "IOD-1-2", 40, 0.5f, 00, 1.00f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-12", "IOD-1-3", 40, 0.5f, 00, 1.00f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-11", "IOD-1-2", 01, 1.0f, 00, 0.50f, 1.0f, 1.70f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-12", "IOD-1-3", 20, 1.0f, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 6, "IO-11", "IOD-1-2", 20, 1.0f, 00, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 6, "IO-12", "IOD-1-3", 20, 1.0f, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-11", "IOD-1-2", 20, 1.0f, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 7, "IO-12", "IOD-1-3", 20, 1.0f, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 8, "IO-11", "IOD-1-2", 20, 1.0f, 00, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 8, "IO-12", "IOD-1-3", 20, 1.0f, 01, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 9, "IO-11", "IOD-1-2", 20, 1.0f, 00, 0.50f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 9, "IO-12", "IOD-1-3", 20, 1.0f, 01, 0.50f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(10, "IO-11", "IOD-1-2", 01, 1.0f, 00, 0.50f, 0.5f, 1.70f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-12", "IOD-1-3", 20, 1.0f, 01, 0.50f, 0.5f, 1.35f),
            }
        });

        // Stage 22
        stageDatas.Add(new StageData()
        {
            stageId = "SN_22",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 6.2f,
            ingameZoomSize = 5f,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "2_2_2_2_2_0_0_0_0_0_0_0_0/" +
            "2_0_0_0_2_0_0_0_0_0_0_0_0/" +
            "2_0_1_0_2_0_1_1_1_1_1_0_0/" +
            "2_0_0_0_2_0_0_0_0_0_0_0_0/" +
            "2_2_2_0_2_2_2_2_2_2_2_0_0/" +
            "0_0_2_1_0_0_0_0_0_1_2_0_0/" +
            "0_0_2_0_0_0_1_0_0_0_2_0_0/" +
            "0_1_2_0_0_1_1_1_0_0_2_2_3/" +
            "0_0_2_0_0_0_1_0_0_0_0_0_0/" +
            "0_0_2_1_0_0_0_0_0_1_0_0_0/" +
            "0_0_2_2_2_2_2_2_2_0_4_2_2/" +
            "0_0_0_0_0_0_0_0_2_0_0_0_2/" +
            "0_0_1_1_1_1_1_0_2_0_1_0_2/" +
            "0_0_0_0_0_0_0_0_2_0_0_0_2/" +
            "0_0_0_0_0_0_0_0_2_2_2_2_2/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-16", "IOD-1-1", 20, 1, 00, 0.80f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-13", "IOD-1-2", 20, 1, 00, 0.90f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-16", "IOD-1-1", 20, 1, 00, 1.00f, 1.0f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-13", "IOD-1-2", 20, 1, 00, 1.00f, 1.0f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-16", "IOD-1-1", 01, 1, 00, 0.50f, 1.0f, 1.70f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-13", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 6, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 6, "IO-13", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 7, "IO-13", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 8, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 8, "IO-13", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 9, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 9, "IO-13", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(10, "IO-16", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 1.70f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-13", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.40f),
            }
        });

        // Stage 23
        stageDatas.Add(new StageData()
        {
            stageId = "SN_23",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 6.2f,
            ingameZoomSize = 5f,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "0_2_2_2_2_2_2_2_2_2_1/" +
            "1_2_1_1_0_0_0_1_1_2_1/" +
            "1_2_1_1_0_0_0_1_1_2_0/" +
            "0_2_0_2_2_2_2_2_0_2_0/" +
            "0_2_0_2_1_0_0_2_0_2_1/" +
            "1_2_0_2_0_0_1_4_0_2_1/" +
            "1_2_0_2_2_2_0_0_0_2_0/" +
            "0_2_1_0_1_2_0_0_0_2_0/" +
            "0_2_0_0_0_2_0_1_0_2_1/" +
            "1_2_1_0_1_2_0_1_0_2_1/" +
            "1_2_2_2_2_2_0_1_0_3_0/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-13", "IOD-1-1", 10, 2, 00, 0.80f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-15", "IOD-1-3", 20, 1, 00, 0.90f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-13", "IOD-1-1", 10, 2, 00, 1.00f, 1.0f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-15", "IOD-1-3", 20, 1, 00, 1.00f, 1.0f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-13", "IOD-1-1", 01, 1, 00, 0.50f, 0.5f, 1.70f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-15", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo( 6, "IO-13", "IOD-1-1", 05, 2, 00, 0.50f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 6, "IO-15", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-13", "IOD-1-1", 05, 2, 00, 0.50f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 7, "IO-15", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 8, "IO-13", "IOD-1-1", 05, 2, 00, 0.50f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 8, "IO-15", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 9, "IO-13", "IOD-1-1", 05, 2, 00, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 9, "IO-15", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(10, "IO-13", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 1.75f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-15", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.40f),
            }
        });

        // Stage 24
        stageDatas.Add(new StageData()
        {
            stageId = "SN_24",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 4.6f,
            tileSize = 0.36f,
            wallData = new WallData(800),
            mapData =
            "2_2_2_0_1_1_1_1_0_0_0/" +
            "2_1_2_0_0_0_0_0_0_0_0/" +
            "2_0_2_0_2_2_2_0_1_1_1/" +
            "2_0_2_1_2_1_2_0_0_0_0/" +
            "2_1_2_5_2_0_2_0_2_2_2/" +
            "2_0_2_0_2_0_2_5_2_1_2/" +
            "2_0_2_1_2_0_2_1_2_0_2/" +
            "4_0_2_2_2_0_2_2_2_0_3/" +
            "0_0_1_1_1_0_1_1_1_0_0/",
            mapSkyData = string.Empty,   
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-14", "IOD-1-2", 20, 1, 00, 0.80f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-15", "IOD-1-3", 40, 0.5f, 00, 0.90f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-14", "IOD-1-2", 20, 1, 00, 1.00f, 1.0f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-15", "IOD-1-3", 40, 0.5f, 00, 1.00f, 1.0f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-14", "IOD-1-2", 01, 1, 00, 0.50f, 1.0f, 1.75f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 6, "IO-14", "IOD-1-2", 10, 2, 00, 0.50f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 6, "IO-15", "IOD-1-3", 20, 1, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-14", "IOD-1-2", 10, 2, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 7, "IO-15", "IOD-1-3", 20, 1, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 8, "IO-14", "IOD-1-2", 10, 2, 00, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 8, "IO-15", "IOD-1-3", 20, 1, 01, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 9, "IO-14", "IOD-1-2", 10, 2, 00, 0.50f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 9, "IO-15", "IOD-1-3", 20, 1, 01, 0.50f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(10, "IO-14", "IOD-1-2", 01, 2, 00, 0.50f, 0.5f, 1.75f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(10, "IO-15", "IOD-1-3", 20, 1, 01, 0.50f, 0.5f, 1.40f, IngameObject.EClass.Boss),
            }
        });

        // Stage 25
        stageDatas.Add(new StageData()
        {
            stageId = "SN_25",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 6.5f,
            ingameZoomSize = 5f,
            tileSize = 0.36f,
            wallData = new WallData(800),
            mapData =
            "0_0_0_0_0_0_0_0_2_2_2_2_2/" +
            "0_0_0_0_0_0_0_0_2_0_0_0_2/" +
            "0_0_1_1_1_1_1_0_2_0_1_0_2/" +
            "0_0_0_0_0_0_0_0_2_0_0_0_2/" +
            "0_0_2_2_2_2_2_2_2_5_2_2_2/" +
            "0_0_2_1_0_0_0_0_0_1_2_0_0/" +
            "0_0_2_0_0_0_1_0_0_0_2_0_0/" +
            "0_1_2_0_0_1_1_1_0_0_2_2_3/" +
            "0_0_2_0_0_0_1_0_0_0_0_0_0/" +
            "0_0_2_1_0_0_0_0_0_1_0_0_0/" +
            "2_2_2_5_2_2_2_2_2_2_4_0_0/" +
            "2_0_0_0_2_0_0_0_0_0_0_0_0/" +
            "2_0_1_0_2_0_1_1_1_1_1_0_0/" +
            "2_0_0_0_2_0_0_0_0_0_0_0_0/" +
            "2_2_2_2_2_0_0_0_0_0_0_0_0/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-16", "IOD-1-1", 20, 1, 00, 0.80f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-14", "IOD-1-2", 20, 1, 00, 0.90f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-16", "IOD-1-1", 20, 1, 00, 1.00f, 1.0f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-14", "IOD-1-2", 20, 1, 00, 1.00f, 1.0f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-16", "IOD-1-1", 01, 1, 00, 0.50f, 0.5f, 1.75f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-14", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 6, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 6, "IO-14", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 7, "IO-14", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 8, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 8, "IO-14", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 9, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 9, "IO-14", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(10, "IO-16", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 1.80f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-14", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(11, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(11, "IO-14", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(12, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(12, "IO-14", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(13, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(13, "IO-14", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(14, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(14, "IO-14", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(15, "IO-16", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 1.80f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-14", "IOD-1-2", 10, 2, 01, 0.50f, 0.5f, 1.40f),
            }
        });

        // Stage 26
        stageDatas.Add(new StageData()
        {
            stageId = "SN_26",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 6.2f,
            ingameZoomSize = 4,
            tileSize = 0.36f,
            wallData = new WallData(1600),
            mapData =
            "0_2_2_2_2_2_2_2_2_2_1/" +
            "1_2_0_0_0_0_0_0_0_2_1/" +
            "1_2_0_1_1_1_1_1_0_2_0/" +
            "0_2_0_2_2_2_2_2_0_2_0/" +
            "0_2_0_2_1_0_0_2_0_2_1/" +
            "1_2_0_2_0_0_1_4_0_2_1/" +
            "1_2_5_2_2_2_0_0_0_2_0/" +
            "0_2_0_1_0_2_0_0_0_2_0/" +
            "0_2_1_0_1_2_0_1_0_2_1/" +
            "1_2_0_1_0_2_0_1_0_2_1/" +
            "1_2_2_2_2_2_0_1_0_3_0/",
            mapSkyData = string.Empty,
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-17", "IOD-1-1", 20, 1, 00, 0.80f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-15", "IOD-1-3", 20, 1, 00, 0.90f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-1-1", 20, 1, 00, 1.00f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-15", "IOD-1-3", 20, 1, 00, 1.00f, 1.0f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-17", "IOD-1-1", 01, 1, 00, 0.50f, 0.5f, 1.80f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-15", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 6, "IO-15", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-17", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 7, "IO-15", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 8, "IO-17", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 8, "IO-15", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 9, "IO-15", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(10, "IO-17", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 1.80f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-15", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(11, "IO-15", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(12, "IO-17", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(12, "IO-15", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(13, "IO-17", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(13, "IO-15", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-1-1", 10, 2, 00, 0.50f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(14, "IO-15", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(15, "IO-17", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 1.80f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-15", "IOD-1-3", 10, 2, 01, 0.50f, 0.5f, 1.40f),
            }
        });

        // Stage 27
        stageDatas.Add(new StageData()
        {
            stageId = "SN_27",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.2f,
            ingameZoomSize = 4f,
            tileSize = 0.36f,
            wallData = new WallData(1600),
            mapData =
            "2_2_2_0_1_0_0_0_1_0_0_0_1/" +
            "2_1_2_0_0_0_0_0_0_0_1_0_0/" +
            "2_0_2_0_2_2_2_0_1_0_0_0_1/" +
            "2_0_2_1_2_1_2_0_0_0_0_0_0/" +
            "2_1_2_5_2_0_2_0_2_2_2_0_0/" +
            "2_0_2_0_2_0_2_5_2_1_2_0_1/" +
            "2_0_2_1_2_0_2_1_2_0_2_0_1/" +
            "4_0_2_2_2_0_2_2_2_0_3_0_1/" +
            "1_0_0_0_0_1_0_0_0_0_0_0_0/",
            mapSkyData =
            "0_6_6_6_6_6_6_6_6_0_6_6_6/" +
            "0_6_0_0_0_0_0_0_6_0_6_0_6/" +
            "0_6_0_0_0_0_0_0_6_0_6_0_6/" +
            "0_6_0_0_0_0_0_0_6_6_6_0_6/" +
            "0_6_0_0_0_0_0_0_0_0_0_0_6/" +
            "0_6_0_0_0_0_0_0_0_0_0_0_6/" +
            "0_6_0_0_0_0_0_0_0_0_0_0_6/" +
            "4_6_0_0_0_0_0_0_0_0_3_0_6/" +
            "0_0_0_0_0_0_0_0_0_0_6_6_6/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-17", "IOD-1-1", 10, 2, 00, 0.35f - 0.05f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-19", "IOD-3-1", 10, 2, 01, 0.35f - 0.05f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-17", "IOD-1-1", 05, 4, 00, 0.45f - 0.05f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-19", "IOD-3-1", 05, 4, 01, 0.45f - 0.05f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-1-1", 05, 4, 00, 0.50f - 0.05f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f - 0.05f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-17", "IOD-2-1", 05, 4, 00, 0.50f - 0.05f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f - 0.05f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-17", "IOD-1-1", 01, 1, 00, 0.50f - 0.05f, 0.5f, 1.85f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f - 0.05f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-1-1", 05, 4, 00, 0.50f - 0.05f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 6, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f - 0.05f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-17", "IOD-1-1", 05, 4, 00, 0.50f - 0.05f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 7, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f - 0.05f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 8, "IO-17", "IOD-1-1", 05, 4, 00, 0.50f - 0.05f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 8, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f - 0.05f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-1-1", 05, 4, 00, 0.50f - 0.05f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 9, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f - 0.05f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(10, "IO-17", "IOD-1-1", 01, 1, 00, 0.50f - 0.05f, 0.5f, 1.85f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f - 0.05f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-1-1", 05, 4, 00, 0.50f - 0.05f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(11, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f - 0.05f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(12, "IO-17", "IOD-1-1", 05, 4, 00, 0.50f - 0.05f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(12, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f - 0.05f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(13, "IO-17", "IOD-1-1", 05, 4, 00, 0.50f - 0.05f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(13, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f - 0.05f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-1-1", 05, 4, 00, 0.50f - 0.05f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo(14, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f - 0.05f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo(15, "IO-17", "IOD-1-1", 01, 1, 00, 0.50f - 0.05f, 0.5f, 1.85f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f - 0.05f, 0.5f, 1.45f),
            }
        });

        // Stage 28
        stageDatas.Add(new StageData()
        {
            stageId = "SN_28",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 6.5f,
            ingameZoomSize = 5.5f,
            tileSize = 0.36f,
            wallData = new WallData(600),
            mapData =
            "2_2_2_2_2_1_1_2_2_2_2_2_0/" +
            "2_0_0_0_2_0_0_2_0_0_0_2_0/" +
            "2_0_1_0_2_0_0_2_0_1_0_2_0/" +
            "2_0_0_0_2_0_0_2_0_0_0_2_0/" +
            "2_2_2_5_2_2_2_2_5_2_2_2_0/" +
            "1_0_2_1_0_0_0_0_1_2_0_0_0/" +
            "0_1_2_0_0_1_1_0_0_2_2_3_0/" +
            "1_0_2_1_0_0_0_0_1_0_0_0_0/" +
            "2_2_2_5_2_2_2_2_0_4_2_2_0/" +
            "2_0_0_0_2_0_0_2_0_0_0_2_0/" +
            "2_0_1_0_2_0_0_2_0_1_0_2_0/" +
            "2_0_0_0_2_0_0_2_0_0_0_2_0/" +
            "2_2_2_2_2_1_1_2_2_2_2_2_0/",
            mapSkyData =
            "6_6_6_6_6_6_6_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_6_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_6_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_6_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_6_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_6_6_6_6_6_6_0/" +
            "6_0_0_0_0_0_0_0_0_0_0_3_0/" +
            "6_0_0_0_0_0_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0_0_0_4_0_0_0/" +
            "6_0_0_0_0_0_0_0_0_6_0_0_0/" +
            "6_0_0_0_0_0_0_0_0_6_0_0_0/" +
            "6_0_0_0_0_0_0_0_0_6_0_0_0/" +
            "6_6_6_6_6_6_6_6_6_6_0_0_0/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-14", "IOD-1-2", 20, 01, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-18", "IOD-3-3", 20, 01, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-14", "IOD-1-2", 20, 01, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-18", "IOD-3-3", 20, 01, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-14", "IOD-1-2", 20, 01, 00, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-18", "IOD-3-3", 20, 01, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-14", "IOD-2-2", 20, 01, 00, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-18", "IOD-3-3", 20, 01, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-14", "IOD-1-2", 02, 10, 00, 0.50f, 0.5f, 1.90f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-18", "IOD-3-3", 20, 01, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 6, "IO-14", "IOD-1-2", 20, 01, 00, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 6, "IO-18", "IOD-3-3", 20, 01, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-14", "IOD-1-2", 20, 01, 00, 0.50f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 7, "IO-18", "IOD-3-3", 20, 01, 01, 0.50f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 8, "IO-14", "IOD-1-2", 20, 01, 00, 0.50f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 8, "IO-18", "IOD-3-3", 20, 01, 01, 0.50f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 9, "IO-14", "IOD-1-2", 20, 01, 00, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 9, "IO-18", "IOD-3-3", 20, 01, 01, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(10, "IO-14", "IOD-1-2", 01, 01, 00, 0.50f, 0.5f, 1.90f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-18", "IOD-3-3", 20, 01, 10, 0.50f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(11, "IO-14", "IOD-1-2", 20, 01, 00, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(11, "IO-18", "IOD-3-3", 20, 01, 01, 0.50f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(12, "IO-14", "IOD-1-2", 20, 01, 00, 0.50f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(12, "IO-18", "IOD-3-3", 20, 01, 01, 0.50f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(13, "IO-14", "IOD-1-2", 20, 01, 00, 0.50f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo(13, "IO-18", "IOD-3-3", 20, 01, 01, 0.50f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo(14, "IO-14", "IOD-1-2", 20, 01, 00, 0.50f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(14, "IO-18", "IOD-3-3", 20, 01, 01, 0.50f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(15, "IO-14", "IOD-1-2", 02, 10, 00, 0.50f, 0.5f, 1.90f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-18", "IOD-3-3", 20, 01, 01, 0.50f, 0.5f, 1.45f),
            }
        });

        // Stage 29
        stageDatas.Add(new StageData()
        {
            stageId = "SN_29",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 6.5f,
            ingameZoomSize = 5.5f,
            tileSize = 0.36f,
            wallData = new WallData(1600),
            mapData =
            "1_0_0_0_1_0_1_0_0_0_1/" +
            "0_2_2_2_2_2_2_2_2_2_0/" +
            "0_2_0_0_1_1_0_0_0_2_0/" +
            "0_2_0_1_0_0_0_1_0_2_0/" +
            "0_2_0_2_2_2_2_2_0_2_1/" +
            "0_2_0_2_1_0_0_2_0_2_0/" +
            "1_2_0_2_0_0_1_4_0_2_0/" +
            "1_2_5_2_2_2_0_0_0_2_0/" +
            "0_2_1_0_1_2_0_0_0_2_1/" +
            "0_2_0_0_0_2_0_1_0_2_0/" +
            "0_2_1_0_1_2_0_0_0_2_0/" +
            "0_2_2_2_2_2_0_1_0_3_0/" +
            "1_0_0_0_1_0_0_0_0_0_1/",
            mapSkyData =
            "6_6_6_6_6_6_6_6_6_6_6/" +
            "6_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_4_6_6_6/" +
            "6_0_0_0_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0_0_0_0_0/" +
            "6_0_6_6_6_0_6_6_6_0_0/" +
            "6_0_6_0_6_0_6_0_6_3_0/" +
            "6_6_6_0_6_6_6_0_0_0_0/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-16", "IOD-1-1", 10, 2, 00, 0.35f - 0.10f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-2", 10, 2, 01, 0.35f - 0.10f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-16", "IOD-1-1", 10, 2, 00, 0.45f - 0.10f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-2", 10, 2, 01, 0.45f - 0.10f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f - 0.10f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f - 0.10f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-16", "IOD-2-1", 10, 2, 00, 0.50f - 0.10f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f - 0.10f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-16", "IOD-1-1", 01, 2, 00, 0.50f - 0.10f, 0.5f, 1.95f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f - 0.10f, 0.5f, 1.95f),
                new IngameObjectSpawnInfo( 6, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f - 0.10f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f - 0.10f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f - 0.10f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f - 0.10f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 8, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f - 0.10f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f - 0.10f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 9, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f - 0.10f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f - 0.10f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(10, "IO-16", "IOD-1-1", 01, 2, 00, 0.50f - 0.10f, 0.5f, 1.95f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-2", 01, 2, 10, 0.50f - 0.10f, 0.5f, 1.45f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(11, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f - 0.10f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f - 0.10f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(12, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f - 0.10f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f - 0.10f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(13, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f - 0.10f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f - 0.10f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo(14, "IO-16", "IOD-1-1", 10, 2, 00, 0.50f - 0.10f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-2", 10, 2, 01, 0.50f - 0.10f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(15, "IO-16", "IOD-1-1", 01, 2, 00, 0.50f - 0.10f, 0.5f, 1.95f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-2", 10, 2, 10, 0.50f - 0.10f, 0.5f, 1.50f),
            }
        });

        // Stage 30
        stageDatas.Add(new StageData()
        {
            stageId = "SN_30",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 6.5f,
            ingameZoomSize = 5.5f,
            tileSize = 0.36f,
            wallData = new WallData(1400),
            mapData =
            "1_0_0_0_1_1_0_0_0_1_0_0_0_1/" +
            "0_0_0_0_0_0_0_0_1_0_0_0_0_0/" +
            "1_0_2_2_2_2_2_2_2_2_2_2_2_0/" +
            "0_0_2_0_0_0_0_0_0_1_0_0_2_0/" +
            "0_1_2_0_0_1_0_1_2_2_2_1_2_0/" +
            "0_0_2_0_0_0_0_0_2_1_2_0_2_0/" +
            "1_0_2_0_0_0_0_0_2_0_2_5_2_0/" +
            "0_0_2_0_0_1_0_0_2_0_2_1_2_0/" +
            "0_1_2_0_0_0_0_0_2_0_2_0_2_0/" +
            "0_0_2_0_0_1_0_0_2_0_2_1_2_0/" +
            "1_0_3_0_0_0_0_0_4_0_2_2_2_0/" +
            "0_0_0_0_0_0_0_0_0_0_0_0_0_0/" +
            "1_0_1_0_0_0_1_1_0_0_0_1_0_1/",
            mapSkyData =
            "0_0_0_0_6_6_6_6_6_6_6_6_6_6/" +
            "0_0_0_0_6_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_6_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_6_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_6_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_6_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_6_0_0_0_6_6_6_6_0_6/" +
            "0_0_0_0_6_0_0_0_6_0_0_6_0_6/" +
            "0_0_0_0_6_0_0_0_6_0_0_6_0_6/" +
            "0_0_0_0_6_0_0_0_6_0_6_6_0_6/" +
            "6_6_3_0_6_0_0_0_4_0_6_0_0_6/" +
            "6_0_0_0_6_0_0_0_0_0_6_6_6_6/" +
            "6_6_6_6_6_0_0_0_0_0_0_0_0_0/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-13", "IOD-1-2", 10, 2, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-2", 20, 2, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-13", "IOD-1-2", 10, 2, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-2", 20, 2, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1", 05, 4, 00, 0.50f, 0.5f, 1.00f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-2", 20, 2, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-13", "IOD-2-2", 10, 2, 00, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-2", 20, 2, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-13", "IOD-1-2", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-2", 20, 2, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-2", 20, 2, 01, 0.52f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-13", "IOD-1-2", 10, 2, 00, 0.54f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-2", 20, 2, 01, 0.54f, 0.5f, 1.08f),
                new IngameObjectSpawnInfo( 8, "IO-13", "IOD-1-2", 10, 2, 00, 0.56f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-2", 20, 2, 01, 0.56f, 0.5f, 1.11f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-2", 20, 2, 01, 0.58f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo(10, "IO-13", "IOD-1-2", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-2", 20, 2, 10, 0.50f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.20f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-2", 20, 2, 01, 0.52f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(12, "IO-13", "IOD-1-2", 10, 2, 00, 0.54f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-2", 20, 2, 01, 0.54f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo(13, "IO-13", "IOD-1-2", 10, 2, 00, 0.56f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-2", 20, 2, 01, 0.56f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.40f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-2", 20, 2, 01, 0.58f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(15, "IO-13", "IOD-1-2", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-2", 20, 2, 10, 0.50f, 0.5f, 1.50f),
            }
        });

        #endregion

        #region < Stage 31~40 >

        // Stage 31
        stageDatas.Add(new StageData()
        {
            stageId = "SN_31",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 6.0f,
            ingameZoomSize = 5.0f,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "0_0_0_0_0_0_0_0_0_0_0_0_0/" +
            "0_0_2_2_2_2_2_2_2_2_2_0_0/" +
            "0_0_2_1_0_0_0_0_0_1_2_0_0/" +
            "0_0_2_0_2_2_2_2_2_0_2_0_0/" +
            "0_0_2_0_2_1_1_1_2_0_2_0_0/" +
            "0_0_2_0_2_1_1_1_4_0_2_0_0/" +
            "0_0_2_0_2_1_1_1_0_1_2_0_0/" +
            "0_0_2_0_2_2_2_2_2_2_2_0_0/" +
            "0_0_2_1_0_0_0_0_0_0_1_0_0/" +
            "0_0_2_2_2_2_2_2_2_2_3_0_0/" +
            "0_0_0_0_0_0_0_0_0_0_0_0_0/",
            mapSkyData =
            "0_0_0_0_0_0_0_0_0_0_0_0_0/" +
            "0_0_0_0_0_0_0_0_0_0_0_0_0/" +
            "0_0_0_6_6_6_6_6_6_6_6_0_0/" +
            "0_0_0_6_0_0_0_0_0_0_6_0_0/" +
            "0_0_0_6_0_0_0_0_0_0_6_0_0/" +
            "0_0_0_6_0_0_0_0_4_0_6_0_0/" +
            "0_0_0_6_0_0_0_0_6_0_6_0_0/" +
            "0_0_0_6_0_0_0_0_6_0_6_0_0/" +
            "0_0_0_6_6_6_6_6_6_0_6_0_0/" +
            "0_0_0_0_0_0_0_0_0_0_3_0_0/" +
            "0_0_0_0_0_0_0_0_0_0_0_0_0/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-21", "IOD-1-3", 40, 0.5f, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-18", "IOD-3-3", 40, 0.5f, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-21", "IOD-1-3", 40, 0.5f, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-18", "IOD-3-3", 40, 0.5f, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.50f, 0.5f, 1.00f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-18", "IOD-3-3", 40, 0.5f, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-21", "IOD-2-3", 40, 0.5f, 00, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-18", "IOD-3-3", 40, 0.5f, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-21", "IOD-1-3", 01, 0.5f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-18", "IOD-3-3", 40, 0.5f, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-18", "IOD-3-3", 40, 0.5f, 01, 0.52f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-21", "IOD-1-3", 40, 0.5f, 00, 0.54f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 7, "IO-18", "IOD-3-3", 40, 0.5f, 01, 0.54f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 8, "IO-21", "IOD-1-3", 40, 0.5f, 00, 0.56f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 8, "IO-18", "IOD-3-3", 40, 0.5f, 01, 0.56f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.58f, 0.5f, 1.20f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-18", "IOD-3-3", 40, 0.5f, 01, 0.58f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(10, "IO-21", "IOD-1-3", 01, 0.5f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-18", "IOD-3-3", 40, 0.5f, 10, 0.50f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.25f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-18", "IOD-3-3", 40, 0.5f, 01, 0.52f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo(12, "IO-21", "IOD-1-3", 40, 0.5f, 00, 0.54f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(12, "IO-18", "IOD-3-3", 40, 0.5f, 01, 0.54f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(13, "IO-21", "IOD-1-3", 40, 0.5f, 00, 0.56f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(13, "IO-18", "IOD-3-3", 40, 0.5f, 01, 0.56f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.58f, 0.5f, 1.40f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-18", "IOD-3-3", 40, 0.5f, 01, 0.58f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(15, "IO-21", "IOD-1-3", 01, 0.5f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-18", "IOD-3-3", 40, 0.5f, 10, 0.50f, 0.5f, 1.50f),
            }
        });

        // Stage 32
        stageDatas.Add(new StageData()
        {
            stageId = "SN_32",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 6.5f,
            ingameZoomSize = 5.5f,
            tileSize = 0.36f,
            wallData = new WallData(2000),
            mapData =
            "0_0_0_0_0_0_0_0_0_0_0_0_0/" +
            "0_1_0_0_2_2_2_0_4_0_0_1_0/" +
            "0_1_0_0_2_1_2_0_2_0_0_1_0/" +
            "0_1_0_0_2_5_2_0_2_0_0_1_0/" +
            "0_1_0_0_2_1_2_0_2_0_0_1_0/" +
            "0_1_0_0_2_0_2_0_2_0_0_1_0/" +
            "0_1_0_0_2_0_2_0_2_0_0_1_0/" +
            "0_1_0_0_2_0_2_1_2_0_0_1_0/" +
            "0_1_0_0_2_0_2_5_2_0_0_1_0/" +
            "0_1_0_0_2_0_2_1_2_0_0_1_0/" +
            "0_1_0_0_3_0_2_2_2_0_0_1_0/" +
            "0_0_0_0_0_0_0_0_0_0_0_0_0/",
            mapSkyData =
            "6_6_6_6_6_6_6_0_6_6_6_6_6/" +
            "6_0_0_0_0_0_6_0_4_0_0_0_6/" +
            "6_0_0_0_0_0_6_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_6_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_6_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_6_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_6_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_6_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_6_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_6_0_0_0_0_0_6/" +
            "6_0_0_0_3_0_6_0_0_0_0_0_6/" +
            "6_6_6_6_6_0_6_6_6_6_6_6_6/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1", 10, 2, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-18", "IOD-3-3", 20, 1, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1", 10, 2, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-18", "IOD-3-3", 20, 1, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1", 05, 4, 00, 0.50f, 0.5f, 1.00f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-18", "IOD-3-3", 20, 1, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-2", 10, 2, 00, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-18", "IOD-3-3", 20, 1, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-18", "IOD-3-3", 20, 1, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-18", "IOD-3-3", 20, 1, 01, 0.52f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1", 10, 2, 00, 0.54f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 7, "IO-18", "IOD-3-3", 20, 1, 01, 0.54f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1", 10, 2, 00, 0.56f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 8, "IO-18", "IOD-3-3", 20, 1, 01, 0.56f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.20f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-18", "IOD-3-3", 20, 1, 01, 0.58f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(10, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-18", "IOD-3-3", 20, 1, 10, 0.50f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.25f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-18", "IOD-3-3", 20, 1, 01, 0.52f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1", 10, 2, 00, 0.54f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(12, "IO-18", "IOD-3-3", 20, 1, 01, 0.54f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1", 10, 2, 00, 0.56f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(13, "IO-18", "IOD-3-3", 20, 1, 01, 0.56f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.40f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-18", "IOD-3-3", 20, 1, 01, 0.58f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(15, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-18", "IOD-3-3", 20, 1, 10, 0.50f, 0.5f, 1.50f),
            }
        });

        // Stage 33
        stageDatas.Add(new StageData()
        {
            stageId = "SN_33",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 4.5f,
            tileSize = 0.36f,
            wallData = new WallData(2000),
            mapData =
            "2_2_2_2_2_0_0_0_0_0_0/" +
            "2_0_0_1_2_0_0_0_0_0_0/" +
            "2_0_1_0_2_0_1_1_0_0_1/" +
            "2_1_0_0_2_0_1_1_0_0_1/" +
            "2_2_3_0_2_0_0_0_0_0_0/" +
            "0_0_0_0_2_0_4_2_2_0_0/" +
            "0_1_1_0_2_1_0_1_2_0_0/" +
            "0_1_1_0_2_0_0_0_2_0_1/" +
            "0_0_0_0_2_1_0_1_2_0_1/" +
            "0_0_0_0_2_2_2_2_2_0_0/",
            mapSkyData =
            "0_0_0_0_0_6_6_6_6_0_0/" +
            "0_0_0_0_0_6_0_0_6_0_0/" +
            "0_0_0_0_0_6_0_0_6_0_0/" +
            "0_0_0_0_0_6_0_0_6_0_0/" +
            "6_6_3_0_0_6_0_0_6_0_0/" +
            "6_0_0_0_0_6_4_0_6_0_0/" +
            "6_0_0_0_0_0_0_0_6_0_0/" +
            "6_0_0_0_0_0_0_0_6_0_0/" +
            "6_0_0_0_0_0_0_0_6_0_0/" +
            "6_6_6_6_6_6_6_6_6_0_0/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-23", "IOD-1-2", 10, 2, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-2", 20, 1, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-23", "IOD-1-2", 10, 2, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-2", 20, 1, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1", 05, 4, 00, 0.50f, 0.5f, 1.00f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-2", 20, 1, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-23", "IOD-2-2", 10, 2, 00, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-2", 20, 1, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-23", "IOD-1-2", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-2", 20, 1, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-2", 20, 1, 01, 0.52f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-23", "IOD-1-2", 10, 2, 00, 0.54f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-2", 20, 1, 01, 0.54f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 8, "IO-23", "IOD-1-2", 10, 2, 00, 0.56f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-2", 20, 1, 01, 0.56f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.20f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-2", 20, 1, 01, 0.58f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(10, "IO-23", "IOD-1-2", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-2", 20, 1, 10, 0.50f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.25f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-2", 20, 1, 01, 0.52f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo(12, "IO-23", "IOD-1-2", 10, 2, 00, 0.54f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-2", 20, 1, 01, 0.54f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(13, "IO-23", "IOD-1-2", 10, 2, 00, 0.56f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-2", 20, 1, 01, 0.56f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.40f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-2", 20, 1, 01, 0.58f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(15, "IO-23", "IOD-1-2", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-2", 20, 1, 10, 0.50f, 0.5f, 1.50f),
            }
        });

        // Stage 34
        stageDatas.Add(new StageData()
        {
            stageId = "SN_34",
            startLife = 20,
            startCost = 350,
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 4.5f,
            tileSize = 0.36f,
            wallData = new WallData(2000),
            mapData =
            "3_0_0_0_0_0_0_0_0_0/" +
            "2_0_1_0_0_1_1_1_1_0/" +
            "2_1_0_0_0_0_0_0_1_0/" +
            "2_2_2_2_0_0_0_0_1_0/" +
            "0_0_0_2_0_0_0_0_1_0/" +
            "0_1_0_2_0_0_0_0_0_0/" +
            "0_1_0_2_2_2_2_0_0_0/" +
            "0_1_0_0_0_0_2_0_1_0/" +
            "0_1_1_1_1_0_2_1_0_0/" +
            "0_0_0_0_0_0_2_2_2_4/",
            mapSkyData =
            "3_6_6_6_0_0_0_0_0_0/" +
            "0_0_0_6_0_0_0_0_0_0/" +
            "0_0_0_6_0_0_0_0_0_0/" +
            "0_0_0_6_6_6_6_0_0_0/" +
            "0_0_0_0_0_0_6_0_0_0/" +
            "0_0_0_0_0_0_6_0_0_0/" +
            "0_0_0_0_0_0_6_6_6_6/" +
            "0_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_0_4/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1", 05, 4, 00, 0.25f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-19", "IOD-3-1", 05, 4, 01, 0.25f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1", 05, 4, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-19", "IOD-3-1", 05, 4, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1", 05, 4, 00, 0.45f, 0.5f, 1.00f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-19", "IOD-3-1", 05, 4, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1", 05, 4, 00, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 5, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-19", "IOD-3-1", 05, 4, 01, 0.52f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1", 05, 4, 00, 0.54f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 7, "IO-19", "IOD-3-1", 05, 4, 01, 0.54f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1", 05, 4, 00, 0.56f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 8, "IO-19", "IOD-3-1", 05, 4, 01, 0.56f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.20f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-19", "IOD-3-1", 05, 4, 01, 0.58f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo(10, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-19", "IOD-3-1", 05, 4, 10, 0.50f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.25f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-19", "IOD-3-1", 05, 4, 01, 0.52f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1", 05, 4, 00, 0.54f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(12, "IO-19", "IOD-3-1", 05, 4, 01, 0.54f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1", 05, 4, 00, 0.56f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(13, "IO-19", "IOD-3-1", 05, 4, 01, 0.56f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.40f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-19", "IOD-3-1", 05, 4, 01, 0.58f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(15, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-19", "IOD-3-1", 05, 4, 10, 0.50f, 0.5f, 1.50f),
            }
        });

        // Stage 35
        stageDatas.Add(new StageData()
        {
            stageId = "SN_35",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 4.5f,
            tileSize = 0.36f,
            wallData = new WallData(3000),
            mapData =
            "3_0_0_0_0_0_1_0_0_0/" +
            "2_0_0_0_0_0_0_0_1_0/" +
            "2_0_1_1_0_0_1_0_0_0/" +
            "2_0_1_1_0_0_0_0_1_0/" +
            "2_0_0_0_0_0_0_0_0_0/" +
            "2_0_0_0_0_0_0_0_0_0/" +
            "2_1_2_2_2_0_1_1_0_0/" +
            "2_0_2_1_2_0_1_1_0_0/" +
            "2_1_2_5_2_0_0_0_0_0/" +
            "2_2_2_1_2_2_2_2_2_4/",
            mapSkyData =
            "3_6_6_6_6_6_0_6_6_6/" +
            "0_0_0_0_0_6_0_6_0_6/" +
            "0_0_0_0_0_6_0_6_0_6/" +
            "0_0_0_0_0_6_6_6_0_6/" +
            "0_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_0_4/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1", 05, 4, 00, 0.25f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-19", "IOD-3-1", 05, 4, 01, 0.25f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1", 05, 4, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-19", "IOD-3-1", 05, 4, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1", 05, 4, 00, 0.45f, 0.5f, 1.00f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-19", "IOD-3-1", 05, 4, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1", 05, 4, 00, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 5, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.10f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-19", "IOD-3-1", 05, 4, 01, 0.52f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1", 05, 4, 00, 0.54f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-19", "IOD-3-1", 05, 4, 01, 0.54f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1", 05, 4, 00, 0.56f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-19", "IOD-3-1", 05, 4, 01, 0.56f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.25f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-19", "IOD-3-1", 05, 4, 01, 0.58f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo(10, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-19", "IOD-3-1", 05, 4, 10, 0.50f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-19", "IOD-3-1", 05, 4, 01, 0.52f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1", 05, 4, 00, 0.54f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-19", "IOD-3-1", 05, 4, 01, 0.54f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1", 05, 4, 00, 0.56f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-19", "IOD-3-1", 05, 4, 01, 0.56f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.45f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-19", "IOD-3-1", 05, 4, 01, 0.58f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(15, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-19", "IOD-3-1", 05, 4, 10, 0.50f, 0.5f, 1.50f),
            }
        });

        // Stage 36
        stageDatas.Add(new StageData()
        {
            stageId = "SN_36",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 4.5f,
            tileSize = 0.36f,
            wallData = new WallData(3000),
            mapData =
            "3_2_2_2_0_0_0_0_0_0_0/" +
            "0_0_1_2_0_1_1_0_1_1_0/" +
            "0_0_1_2_0_0_0_0_1_1_0/" +
            "0_0_0_2_0_2_2_2_0_0_0/" +
            "0_1_0_2_1_2_1_2_0_1_0/" +
            "0_1_0_2_1_2_5_2_0_1_0/" +
            "0_0_0_2_2_2_1_2_0_0_0/" +
            "0_1_1_0_0_0_0_2_1_0_0/" +
            "0_1_1_0_1_1_0_2_1_0_0/" +
            "0_0_0_0_0_0_0_2_2_2_4/",
            mapSkyData =
            "3_0_0_0_0_6_6_6_6_6_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_6_6_6_6_6_0_0_0_0_4/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.50f, 0.5f, 1.00f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.50f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1", 05, 4.0f, 00, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 5, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.10f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.52f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.54f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.54f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.56f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.56f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.58f, 0.5f, 1.25f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.58f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo(10, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-19", "IOD-3-3", 40, 0.5f, 10, 0.50f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.52f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.54f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.54f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.56f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.56f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.58f, 0.5f, 1.45f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.58f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(15, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-19", "IOD-3-3", 40, 0.5f, 10, 0.50f, 0.5f, 1.50f),
            }
        });

        // Stage 37
        stageDatas.Add(new StageData()
        {
            stageId = "SN_37",
            startLife = 20,
            startCost = 400,
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 4.5f,
            tileSize = 0.36f,
            wallData = new WallData(3000),
            mapData =
            "2_2_2_2_2_2_2_2_2_2_4/" +
            "2_1_0_0_0_0_0_0_0_1_0/" +
            "2_0_1_0_0_0_0_0_1_0_0/" +
            "2_0_0_1_1_1_1_1_0_0_0/" +
            "2_0_0_1_0_0_0_1_0_0_0/" +
            "2_0_0_1_0_0_0_1_0_0_0/" +
            "2_0_0_1_1_1_1_1_0_0_0/" +
            "2_0_1_0_0_0_0_0_1_0_0/" +
            "2_1_0_0_0_0_0_0_0_1_0/" +
            "3_0_0_0_0_0_0_0_0_0_0/",
            mapSkyData =
            "0_0_0_0_0_0_0_0_0_0_4/" +
            "0_0_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_0_0_6/" +
            "3_6_6_6_6_6_6_6_6_6_6/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.35f, 0.5f, 1.00f - 0.05f),
                new IngameObjectSpawnInfo( 1, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.35f, 0.5f, 1.00f - 0.05f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.45f, 0.5f, 1.00f - 0.05f),
                new IngameObjectSpawnInfo( 2, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.45f, 0.5f, 1.00f - 0.05f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.50f, 0.5f, 1.00f - 0.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.50f, 0.5f, 1.00f - 0.05f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1", 05, 4.0f, 00, 0.50f, 0.5f, 1.05f - 0.05f),
                new IngameObjectSpawnInfo( 4, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.50f, 0.5f, 1.05f - 0.05f),
                new IngameObjectSpawnInfo( 5, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f - 0.05f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.50f, 0.5f, 2.00f - 0.05f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.10f - 0.05f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.52f, 0.5f, 1.10f - 0.05f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.54f, 0.5f, 1.15f - 0.05f),
                new IngameObjectSpawnInfo( 7, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.54f, 0.5f, 1.15f - 0.05f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.56f, 0.5f, 1.20f - 0.05f),
                new IngameObjectSpawnInfo( 8, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.56f, 0.5f, 1.20f - 0.05f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.58f, 0.5f, 1.25f - 0.05f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.58f, 0.5f, 1.25f - 0.05f),
                new IngameObjectSpawnInfo(10, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f - 0.05f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-19", "IOD-3-3", 40, 0.5f, 10, 0.50f, 0.5f, 1.50f - 0.05f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.30f - 0.05f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.52f, 0.5f, 1.30f - 0.05f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.54f, 0.5f, 1.35f - 0.05f),
                new IngameObjectSpawnInfo(12, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.54f, 0.5f, 1.35f - 0.05f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.56f, 0.5f, 1.40f - 0.05f),
                new IngameObjectSpawnInfo(13, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.56f, 0.5f, 1.40f - 0.05f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.58f, 0.5f, 1.45f - 0.05f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.58f, 0.5f, 1.45f - 0.05f),
                new IngameObjectSpawnInfo(15, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f - 0.05f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-19", "IOD-3-3", 40, 0.5f, 10, 0.50f, 0.5f, 1.50f - 0.05f),
            }
        });

        // Stage 38
        stageDatas.Add(new StageData()
        {
            stageId = "SN_38",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 4.5f,
            tileSize = 0.36f,
            wallData = new WallData(3000),
            mapData =
            "2_2_2_2_2_2_2_2_2_2_2/" +
            "2_1_1_1_5_1_0_0_1_1_2/" +
            "2_2_2_2_2_2_0_0_4_2_2/" +
            "0_0_0_0_0_2_0_0_0_0_0/" +
            "0_1_1_1_0_2_0_1_1_1_0/" +
            "0_1_0_1_0_2_0_1_0_1_0/" +
            "0_1_1_1_0_2_0_1_1_1_0/" +
            "0_0_0_0_0_2_0_0_0_0_0/" +
            "2_2_3_0_0_2_2_2_2_2_2/" +
            "2_1_1_0_0_1_5_1_1_1_2/" +
            "2_2_2_2_2_2_2_2_2_2_2/",
            mapSkyData =
            "6_6_6_0_0_0_0_0_6_6_6/" +
            "6_0_6_0_0_0_0_0_6_0_6/" +
            "6_0_6_0_0_0_0_0_4_0_6/" +
            "6_0_6_0_0_0_0_0_0_0_6/" +
            "6_0_6_0_0_0_0_0_0_0_6/" +
            "6_0_6_6_6_6_6_6_6_0_6/" +
            "6_0_0_0_0_0_0_0_6_0_6/" +
            "6_0_0_0_0_0_0_0_6_0_6/" +
            "6_0_3_0_0_0_0_0_6_0_6/" +
            "6_0_6_0_0_0_0_0_6_0_6/" +
            "6_6_6_0_0_0_0_0_6_6_6/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1", 10, 2.0f, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1", 10, 2.0f, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.50f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1", 10, 2.0f, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.52f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1", 10, 2.0f, 00, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1", 10, 2.0f, 00, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 10, 2.0f, 00, 0.58f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.58f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-2", 20, 1.0f, 10, 0.50f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.52f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1", 10, 2.0f, 00, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1", 10, 2.0f, 00, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 10, 2.0f, 00, 0.58f, 0.5f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.58f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-2", 20, 1.0f, 10, 0.50f, 0.5f, 1.50f),
            }
        });

        // Stage 39
        stageDatas.Add(new StageData()
        {
            stageId = "SN_39",
            startLife = 20,
            startCost = 200,
            lobbyZoomSize = 4.5f,
            ingameZoomSize = 3f,
            tileSize = 0.36f,
            wallData = new WallData(3000),
            mapData =
            "3_2_2_2_0_1_1/" +
            "0_1_1_2_0_1_1/" +
            "0_1_1_2_0_0_0/" +
            "0_0_0_2_0_0_0/" +
            "0_0_0_2_1_1_0/" +
            "1_1_0_2_1_1_0/" +
            "1_1_0_2_2_2_4/",
            mapSkyData =
            "3_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0/" +
            "6_6_6_6_6_6_6/" +
            "0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_4/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.50f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1", 05, 4.0f, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.52f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.58f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.58f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-1", 05, 4.0f, 10, 0.50f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.52f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.58f, 0.5f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.58f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-1", 05, 4.0f, 10, 0.50f, 0.5f, 1.50f),
            }
        });

        // Stage 40 (쉴드 추가)
        stageDatas.Add(new StageData()
        {
            stageId = "SN_40",
            startLife = 20,
            startCost = 500,
            lobbyZoomSize = 5.0f,
            ingameZoomSize = 4.0f,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "0_0_0_3_2_2_2_2_2_2_2/" +
            "0_1_0_0_0_0_0_1_1_1_2/" +
            "0_1_0_0_0_1_0_2_2_2_2/" +
            "0_1_0_0_0_1_0_2_0_1_0/" +
            "0_1_0_0_0_0_0_2_0_1_0/" +
            "0_1_0_0_0_1_0_2_0_1_0/" +
            "0_0_0_0_0_1_0_2_0_1_0/" +
            "0_1_1_1_0_0_0_2_0_1_0/" +
            "0_0_0_0_0_0_0_4_0_0_0/",
            mapSkyData =
            "0_0_0_3_0_0_0_0_0_0_0/" +
            "0_0_0_6_0_0_0_0_0_0_0/" +
            "0_0_0_6_0_0_0_0_0_0_0/" +
            "0_0_0_6_0_0_0_0_0_0_0/" +
            "0_0_0_6_0_0_0_0_0_0_0/" +
            "0_0_0_6_0_0_0_0_0_0_0/" +
            "6_6_6_6_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0_0_0_0_0/" +
            "6_6_6_6_6_6_6_4_0_0_0/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-21", "IOD-1-3"  , 40, 0.5f, 00, 0.35f - 0.10f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.35f - 0.10f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-21", "IOD-1-3"  , 40, 0.5f, 00, 0.45f - 0.10f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.45f - 0.10f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1-1", 05, 4.0f, 00, 0.50f - 0.10f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-18", "IOD-3-3-1", 40, 0.5f, 01, 0.50f - 0.10f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-21", "IOD-2-3"  , 40, 0.5f, 00, 0.50f - 0.10f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.50f - 0.10f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-21", "IOD-1-3"  , 01, 0.5f, 00, 0.50f - 0.10f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.50f - 0.10f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1"  , 05, 4.0f, 00, 0.52f - 0.10f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.52f - 0.10f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-21", "IOD-1-3"  , 40, 0.5f, 00, 0.54f - 0.10f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.54f - 0.10f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-21", "IOD-1-3-1", 40, 0.5f, 00, 0.56f - 0.10f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-18", "IOD-3-3-1", 40, 0.5f, 01, 0.56f - 0.10f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1"  , 05, 4.0f, 00, 0.58f - 0.10f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.58f - 0.10f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-21", "IOD-1-3"  , 01, 0.5f, 00, 0.50f - 0.10f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-18", "IOD-3-3"  , 40, 0.5f, 10, 0.50f - 0.10f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1"  , 05, 4.0f, 00, 0.52f - 0.10f, 0.5f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.52f - 0.10f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-21", "IOD-1-3"  , 40, 0.5f, 00, 0.54f - 0.10f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.54f - 0.10f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-21", "IOD-1-3-1", 40, 0.5f, 00, 0.56f - 0.10f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-18", "IOD-3-3-1", 40, 0.5f, 01, 0.56f - 0.10f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1"  , 05, 4.0f, 00, 0.58f - 0.10f, 0.5f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.58f - 0.10f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-21", "IOD-1-3"  , 01, 0.5f, 00, 0.50f - 0.10f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-18", "IOD-3-3"  , 40, 0.5f, 10, 0.50f - 0.10f, 0.5f, 1.50f),
            }
        });

        #endregion

        #region < Stage 41~50 >

        // Stage 41
        stageDatas.Add(new StageData()
        {
            stageId = "SN_41",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.0f,
            ingameZoomSize = 4.0f,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "0_0_0_0_0_0_0_0_0_0_0/" +
            "0_1_1_1_0_0_0_1_1_1_0/" +
            "0_0_0_0_0_0_0_0_0_0_0/" +
            "2_2_2_3_1_1_1_0_0_0_0/" +
            "2_0_0_0_1_1_1_0_0_0_0/" +
            "2_0_0_0_1_1_1_4_2_2_2/" +
            "2_0_0_0_0_0_0_0_0_0_2/" +
            "2_1_1_1_0_0_0_1_1_1_2/" +
            "2_2_2_2_2_2_2_2_2_2_2/",
            mapSkyData =
            "6_6_6_6_6_6_6_6_6_6_6/" +
            "6_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_0_0_6/" +
            "6_6_6_3_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_4_6_6_6/" +
            "0_0_0_0_0_0_0_0_0_0_0/" +
            "0_0_0_0_0_0_0_0_0_0_0/" +
            "0_0_0_0_0_0_0_0_0_0_0/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-21", "IOD-1-3"  , 40, 0.5f, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-21", "IOD-1-3"  , 40, 0.5f, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1-1", 05, 4.0f, 00, 0.50f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-18", "IOD-3-3-1", 40, 0.5f, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-21", "IOD-2-3"  , 40, 0.5f, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-21", "IOD-1-3"  , 01, 0.5f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1"  , 05, 4.0f, 00, 0.52f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.52f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-21", "IOD-1-3"  , 40, 0.5f, 00, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-21", "IOD-1-3-1", 40, 0.5f, 00, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-18", "IOD-3-3-1", 40, 0.5f, 01, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1"  , 05, 4.0f, 00, 0.58f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.58f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-21", "IOD-1-3"  , 01, 0.5f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-18", "IOD-3-3"  , 40, 0.5f, 10, 0.50f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1"  , 05, 4.0f, 00, 0.52f, 0.5f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.52f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-21", "IOD-1-3"  , 40, 0.5f, 00, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-21", "IOD-1-3-1", 40, 0.5f, 00, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-18", "IOD-3-3-1", 40, 0.5f, 01, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1"  , 05, 4.0f, 00, 0.58f, 0.5f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-18", "IOD-3-3"  , 40, 0.5f, 01, 0.58f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-21", "IOD-1-3"  , 01, 0.5f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-18", "IOD-3-3"  , 40, 0.5f, 10, 0.50f, 0.5f, 1.50f),
            }
        });
        
        // Stage 42
        stageDatas.Add(new StageData()
        {
            stageId = "SN_42",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 4.5f,
            tileSize = 0.36f,
            wallData = new WallData(600),
            mapData =
            "1_1_1_1_1_0_0_0_0_0_0/" +
            "0_0_0_0_2_2_2_0_0_0_0/" +
            "0_1_0_1_2_1_2_1_0_1_0/" +
            "0_0_0_0_2_5_2_0_2_2_4/" +
            "0_1_0_1_2_1_2_1_2_1_0/" +
            "3_2_2_0_2_0_2_5_2_0_0/" +
            "0_1_2_1_2_1_2_1_2_1_0/" +
            "0_0_2_2_2_0_2_2_2_0_0/" +
            "0_0_0_0_0_0_1_1_1_1_1/",
            mapSkyData =
            "0_0_0_0_0_0_0_0_0_0_0/" +
            "6_6_6_6_6_0_0_0_0_0_0/" +
            "6_0_0_0_6_0_0_0_0_0_0/" +
            "6_0_0_0_6_0_0_0_0_0_4/" +
            "6_0_0_0_6_6_6_0_0_0_6/" +
            "3_0_0_0_0_0_6_0_0_0_6/" +
            "0_0_0_0_0_0_6_0_0_0_6/" +
            "0_0_0_0_0_0_6_6_6_6_6/" +
            "0_0_0_0_0_0_0_0_0_0_0/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1", 10, 2, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-18", "IOD-3-3", 20, 1, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1", 10, 2, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-18", "IOD-3-3", 20, 1, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1-1", 05, 4, 00, 0.50f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-18", "IOD-3-3-1", 20, 1, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-2", 10, 2, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-18", "IOD-3-3", 20, 1, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-18", "IOD-3-3", 20, 1, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-18", "IOD-3-3", 20, 1, 01, 0.52f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1", 10, 2, 00, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-18", "IOD-3-3", 20, 1, 01, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1-1", 10, 2, 00, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-18", "IOD-3-3-1", 20, 1, 01, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-18", "IOD-3-3", 20, 1, 01, 0.58f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-18", "IOD-3-3", 20, 1, 10, 0.50f, 0.5f, 1.55f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-18", "IOD-3-3", 20, 1, 01, 0.52f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1", 10, 2, 00, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-18", "IOD-3-3", 20, 1, 01, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1-1", 10, 2, 00, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-18", "IOD-3-3-1", 20, 1, 01, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-18", "IOD-3-3", 20, 1, 01, 0.58f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-18", "IOD-3-3", 20, 1, 10, 0.50f, 0.5f, 1.55f),
            }
        });

        // Stage 43
        stageDatas.Add(new StageData()
        {
            stageId = "SN_43",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 4.5f,
            tileSize = 0.36f,
            wallData = new WallData(1200),
            mapData =
            "0_0_0_0_2_2_2_2_2_0_0/" +
            "1_0_0_0_2_0_0_0_2_0_1/" +
            "1_0_0_0_2_0_4_0_2_0_1/" +
            "0_0_0_0_2_0_2_1_2_0_0/" +
            "1_0_0_0_2_1_2_0_2_0_1/" +
            "1_0_0_0_2_0_2_1_2_0_1/" +
            "1_0_0_0_2_1_2_0_2_0_1/" +
            "1_0_0_0_2_0_2_1_2_0_1/" +
            "0_0_0_0_2_1_2_5_2_0_0/" +
            "1_0_0_0_3_0_2_0_2_0_1/" +
            "1_0_0_0_0_0_2_0_2_0_1/" +
            "0_0_0_0_0_0_2_2_2_0_0/",
            mapSkyData =
            "0_0_6_6_6_0_0_0_0_0_0/" +
            "0_0_6_0_6_0_0_0_0_0_0/" +
            "0_0_6_0_6_0_4_0_0_0_0/" +
            "0_0_6_0_6_0_6_0_0_0_0/" +
            "0_0_6_0_6_0_6_0_0_0_0/" +
            "0_0_6_0_6_0_6_0_0_0_0/" +
            "0_0_6_0_6_0_6_0_0_0_0/" +
            "0_0_6_0_6_0_6_0_0_0_0/" +
            "0_0_6_0_6_0_6_0_0_0_0/" +
            "0_0_6_0_3_0_6_0_0_0_0/" +
            "0_0_6_0_0_0_6_0_0_0_0/" +
            "0_0_6_6_6_6_6_0_0_0_0/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-23", "IOD-1-2", 10, 2, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-2", 20, 1, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-23", "IOD-1-2", 10, 2, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-2", 20, 1, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1-1", 05, 4, 00, 0.50f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-2-1", 20, 1, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-23", "IOD-2-2", 10, 2, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-2", 20, 1, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-23", "IOD-1-2", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-2", 20, 1, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-2", 20, 1, 01, 0.52f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-23", "IOD-1-2", 10, 2, 00, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-2", 20, 1, 01, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-23", "IOD-1-2-1", 10, 2, 00, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-2-1", 20, 1, 01, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-2", 20, 1, 01, 0.58f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-23", "IOD-1-2", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-2", 20, 1, 10, 0.50f, 0.5f, 1.60f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-2", 20, 1, 01, 0.52f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-23", "IOD-1-2", 10, 2, 00, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-2", 20, 1, 01, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-23", "IOD-1-2-1", 10, 2, 00, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-2-1", 20, 1, 01, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-2", 20, 1, 01, 0.58f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-23", "IOD-1-2", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-2", 20, 1, 10, 0.50f, 0.5f, 1.60f),
            }
        });

        // Stage 44
        stageDatas.Add(new StageData()
        {
            stageId = "SN_44",
            startLife = 20,
            startCost = 350,
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 4.5f,
            tileSize = 0.36f,
            wallData = new WallData(600),
            mapData =
            "2_2_2_2_2_3_0_0_0_0/" +
            "2_1_5_1_0_0_0_1_1_0/" +
            "2_2_2_2_2_1_0_0_0_0/" +
            "0_0_0_0_2_0_0_0_0_0/" +
            "0_1_1_0_2_0_0_1_1_0/" +
            "0_1_1_0_2_0_0_1_1_0/" +
            "0_0_0_0_2_0_0_0_0_0/" +
            "2_2_2_2_2_1_0_0_0_0/" +
            "2_1_5_1_0_0_0_1_1_0/" +
            "2_2_2_2_2_4_0_0_0_0/",
            mapSkyData =
            "0_0_0_0_0_3_6_6_6_6/" +
            "0_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_6_6_6_6/" +
            "0_0_0_0_0_0_6_0_0_0/" +
            "0_0_0_0_0_0_6_0_0_0/" +
            "0_0_0_0_0_0_6_0_0_0/" +
            "0_0_0_0_0_0_6_0_0_0/" +
            "0_0_0_0_0_0_6_6_6_6/" +
            "0_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_4_6_6_6_6/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1", 05, 4, 00, 0.25f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-19", "IOD-3-1", 05, 4, 01, 0.25f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1", 05, 4, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-19", "IOD-3-1", 05, 4, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1-1", 05, 4, 00, 0.45f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-19", "IOD-3-1-1", 05, 4, 01, 0.45f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1", 05, 4, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-19", "IOD-3-1", 05, 4, 01, 0.52f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1", 05, 4, 00, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-19", "IOD-3-1", 05, 4, 01, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1-1", 05, 4, 00, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-19", "IOD-3-1-1", 05, 4, 01, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-19", "IOD-3-1", 05, 4, 01, 0.58f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-19", "IOD-3-1", 05, 4, 10, 0.50f, 0.5f, 1.65f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-19", "IOD-3-1", 05, 4, 01, 0.52f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1", 05, 4, 00, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-19", "IOD-3-1", 05, 4, 01, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1-1", 05, 4, 00, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-19", "IOD-3-1-1", 05, 4, 01, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-19", "IOD-3-1", 05, 4, 01, 0.58f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-19", "IOD-3-1", 05, 4, 10, 0.50f, 0.5f, 1.65f),
            }
        });

        // Stage 45
        stageDatas.Add(new StageData()
        {
            stageId = "SN_45",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 4.5f,
            tileSize = 0.36f,
            wallData = new WallData(2000),
            mapData =
            "0_0_0_1_1_0_0_0_0_0_0/" +
            "0_3_2_2_2_2_0_0_0_0_0/" +
            "0_0_0_0_0_2_0_1_1_0_0/" +
            "1_1_0_0_0_2_0_1_1_0_0/" +
            "1_2_2_2_2_2_0_0_0_0_0/" +
            "0_2_0_0_0_0_2_2_2_2_0/" +
            "0_2_0_1_1_0_2_0_0_2_1/" +
            "0_2_0_1_1_0_2_0_0_2_1/" +
            "0_2_0_0_0_0_2_0_0_2_0/" +
            "1_2_2_2_2_2_2_1_0_4_0/" +
            "1_1_0_0_0_0_1_1_0_0_0/",
            mapSkyData =
            "0_0_0_0_0_0_0_0_0_0_0/" +
            "0_3_0_0_0_6_6_6_6_6_0/" +
            "0_6_0_0_0_6_0_0_0_6_0/" +
            "0_6_0_0_0_6_0_0_0_6_0/" +
            "0_6_6_6_6_6_0_0_0_6_0/" +
            "0_0_0_0_0_0_6_6_6_6_0/" +
            "0_0_0_0_0_0_6_0_0_0_0/" +
            "0_0_0_0_0_0_6_0_0_0_0/" +
            "0_0_0_0_0_0_6_0_0_0_0/" +
            "0_0_0_0_0_0_6_6_6_4_0/" +
            "0_0_0_0_0_0_0_0_0_0_0/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1", 05, 4, 00, 0.25f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-19", "IOD-3-1", 05, 4, 01, 0.25f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1", 05, 4, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-19", "IOD-3-1", 05, 4, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1-1", 05, 4, 00, 0.45f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-19", "IOD-3-1-1", 05, 4, 01, 0.45f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1", 05, 4, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-19", "IOD-3-1", 05, 4, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-19", "IOD-3-1", 05, 4, 01, 0.52f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1", 05, 4, 00, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-19", "IOD-3-1", 05, 4, 01, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1-1", 05, 4, 00, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-19", "IOD-3-1-1", 05, 4, 01, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-19", "IOD-3-1", 05, 4, 01, 0.58f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-19", "IOD-3-1", 05, 4, 10, 0.50f, 0.5f, 1.70f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4, 00, 0.52f, 0.5f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-19", "IOD-3-1", 05, 4, 01, 0.52f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1", 05, 4, 00, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-19", "IOD-3-1", 05, 4, 01, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1-1", 05, 4, 00, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-19", "IOD-3-1-1", 05, 4, 01, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 05, 4, 00, 0.58f, 0.5f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-19", "IOD-3-1", 05, 4, 01, 0.58f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-22", "IOD-1-1", 01, 2, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-19", "IOD-3-1", 05, 4, 10, 0.50f, 0.5f, 1.70f),
            }
        });

        // Stage 46
        stageDatas.Add(new StageData()
        {
            stageId = "SN_46",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 4.5f,
            tileSize = 0.36f,
            wallData = new WallData(1000),
            mapData =
            "1_1_1_1_0_0_0_0_0_0/" +
            "1_3_2_2_2_2_2_0_1_0/" +
            "1_0_0_0_5_0_2_0_1_0/" +
            "1_2_2_2_2_2_2_0_1_0/" +
            "0_2_1_0_1_0_1_0_1_0/" +
            "0_2_1_0_1_0_1_0_0_0/" +
            "0_2_2_2_2_2_2_2_2_1/" +
            "0_0_0_0_0_0_0_0_2_1/" +
            "0_1_1_1_1_0_0_0_4_1/" +
            "0_0_0_0_0_0_1_1_1_1/",
            mapSkyData =
            "0_0_0_0_0_0_0_0_0_0/" +
            "0_3_0_6_6_6_0_0_0_0/" +
            "0_6_0_6_0_6_0_0_0_0/" +
            "0_6_0_6_0_6_0_0_0_0/" +
            "0_6_0_6_0_6_0_0_0_0/" +
            "0_6_0_6_0_6_0_0_0_0/" +
            "0_6_6_6_0_6_0_0_0_0/" +
            "0_0_0_0_0_6_0_0_0_0/" +
            "0_0_0_0_0_6_6_6_4_0/" +
            "0_0_0_0_0_0_0_0_0_0/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1-1", 05, 4.0f, 00, 0.50f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-19", "IOD-3-3-1", 40, 0.5f, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1", 05, 4.0f, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.52f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1-1", 05, 4.0f, 00, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-19", "IOD-3-3-1", 40, 0.5f, 01, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.58f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.58f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-19", "IOD-3-3", 40, 0.5f, 10, 0.50f, 0.5f, 1.75f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.52f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1-1", 05, 4.0f, 00, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-19", "IOD-3-3-1", 40, 0.5f, 01, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.58f, 0.5f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.58f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-19", "IOD-3-3", 40, 0.5f, 10, 0.50f, 0.5f, 1.75f),
            }
        });

        // Stage 47
        stageDatas.Add(new StageData()
        {
            stageId = "SN_47",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 4.5f,
            tileSize = 0.36f,
            wallData = new WallData(1000),
            mapData =
            "0_0_0_0_0_0_0_0_0_2_4/" +
            "0_1_0_0_0_0_0_0_2_2_1/" +
            "0_0_1_1_0_1_1_2_2_0_0/" +
            "0_0_1_1_0_0_2_2_1_0_0/" +
            "0_0_0_0_1_2_2_0_1_0_0/" +
            "0_0_1_0_2_2_1_0_0_0_0/" +
            "0_0_1_2_2_0_0_1_1_0_0/" +
            "0_0_2_2_0_0_0_1_1_0_0/" +
            "1_2_2_1_1_0_0_0_0_1_0/" +
            "3_2_0_0_0_0_0_0_0_0_0/",
            mapSkyData =
            "6_6_6_6_6_6_0_0_0_0_4/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "3_0_0_0_0_6_6_6_6_6_6/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.35f, 0.5f, 1.00f - 0.05f),
                new IngameObjectSpawnInfo( 1, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.35f, 0.5f, 1.00f - 0.05f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.45f, 0.5f, 1.00f - 0.05f),
                new IngameObjectSpawnInfo( 2, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.45f, 0.5f, 1.00f - 0.05f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1-1", 05, 4.0f, 00, 0.50f, 0.5f, 1.05f - 0.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-19", "IOD-3-3-1", 40, 0.5f, 01, 0.50f, 0.5f, 1.05f - 0.05f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1", 05, 4.0f, 00, 0.50f, 0.5f, 1.10f - 0.05f),
                new IngameObjectSpawnInfo( 4, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.50f, 0.5f, 1.10f - 0.05f),
                new IngameObjectSpawnInfo( 5, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f - 0.05f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.50f, 0.5f, 2.00f - 0.05f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.15f - 0.05f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.52f, 0.5f, 1.15f - 0.05f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.54f, 0.5f, 1.20f - 0.05f),
                new IngameObjectSpawnInfo( 7, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.54f, 0.5f, 1.20f - 0.05f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1-1", 05, 4.0f, 00, 0.56f, 0.5f, 1.25f - 0.05f),
                new IngameObjectSpawnInfo( 8, "IO-19", "IOD-3-3-1", 40, 0.5f, 01, 0.56f, 0.5f, 1.25f - 0.05f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.58f, 0.5f, 1.30f - 0.05f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.58f, 0.5f, 1.30f - 0.05f),
                new IngameObjectSpawnInfo(10, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f - 0.05f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-19", "IOD-3-3", 40, 0.5f, 10, 0.50f, 0.5f, 1.80f - 0.05f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.35f - 0.05f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.52f, 0.5f, 1.35f - 0.05f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.54f, 0.5f, 1.40f - 0.05f),
                new IngameObjectSpawnInfo(12, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.54f, 0.5f, 1.40f - 0.05f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1-1", 05, 4.0f, 00, 0.56f, 0.5f, 1.45f - 0.05f),
                new IngameObjectSpawnInfo(13, "IO-19", "IOD-3-3-1", 40, 0.5f, 01, 0.56f, 0.5f, 1.45f - 0.05f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.58f, 0.5f, 1.50f - 0.05f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-19", "IOD-3-3", 40, 0.5f, 01, 0.58f, 0.5f, 1.50f - 0.05f),
                new IngameObjectSpawnInfo(15, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f - 0.05f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-19", "IOD-3-3", 40, 0.5f, 10, 0.50f, 0.5f, 1.80f - 0.05f),
            }
        });

        // Stage 48
        stageDatas.Add(new StageData()
        {
            stageId = "SN_48",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 4.5f,
            tileSize = 0.36f,
            wallData = new WallData(600),
            mapData =
            "2_2_2_2_2_2_2_2_2_2_2/" +
            "2_1_1_1_5_1_0_0_1_1_2/" +
            "2_2_2_2_2_2_0_0_4_2_2/" +
            "0_0_0_0_0_2_0_0_0_0_0/" +
            "0_1_1_1_0_2_0_1_1_1_0/" +
            "0_1_0_1_0_2_0_1_0_1_0/" +
            "0_1_1_1_0_2_0_1_1_1_0/" +
            "0_0_0_0_0_2_0_0_0_0_0/" +
            "2_2_3_0_0_2_2_2_2_2_2/" +
            "2_1_1_0_0_1_5_1_1_1_2/" +
            "2_2_2_2_2_2_2_2_2_2_2/",
            mapSkyData =
            "6_6_6_0_0_0_0_0_6_6_6/" +
            "6_0_6_0_0_0_0_0_6_0_6/" +
            "6_0_6_0_0_0_0_0_4_0_6/" +
            "6_0_6_0_0_0_0_0_0_0_6/" +
            "6_0_6_0_0_0_0_0_0_0_6/" +
            "6_0_6_6_6_6_6_6_6_0_6/" +
            "6_0_0_0_0_0_0_0_6_0_6/" +
            "6_0_0_0_0_0_0_0_6_0_6/" +
            "6_0_3_0_0_0_0_0_6_0_6/" +
            "6_0_6_0_0_0_0_0_6_0_6/" +
            "6_6_6_0_0_0_0_0_6_6_6/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1", 10, 2.0f, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1", 10, 2.0f, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1-1", 05, 4.0f, 00, 0.50f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-2-1", 20, 1.0f, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1", 10, 2.0f, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.52f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1", 10, 2.0f, 00, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1-1", 10, 2.0f, 00, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-2-1", 20, 1.0f, 01, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 10, 2.0f, 00, 0.58f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.58f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-2", 20, 1.0f, 10, 0.50f, 0.5f, 1.85f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.52f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1", 10, 2.0f, 00, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1-1", 10, 2.0f, 00, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-2-1", 20, 1.0f, 01, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 10, 2.0f, 00, 0.58f, 0.5f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-2", 20, 1.0f, 01, 0.58f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-2", 20, 1.0f, 10, 0.50f, 0.5f, 1.85f),
            }
        });

        // Stage 49
        stageDatas.Add(new StageData()
        {
            stageId = "SN_49",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 4.5f,
            ingameZoomSize = 3f,
            tileSize = 0.36f,
            wallData = new WallData(2000),
            mapData =
            "1_0_2_2_2_2_0_1/" +
            "0_0_2_0_0_2_0_1/" +
            "2_2_2_1_1_2_0_0/" +
            "3_1_1_1_1_2_1_4/" +
            "0_0_0_1_1_2_2_2/" +
            "0_0_0_0_0_0_0_1/" +
            "1_0_0_0_0_0_0_1/",
            mapSkyData =
            "0_0_0_0_0_0_0_0/" +
            "0_0_0_0_0_0_0_0/" +
            "0_0_0_0_0_6_6_6/" +
            "3_0_0_0_0_6_0_4/" +
            "6_6_6_0_0_6_0_0/" +
            "0_0_6_0_0_6_0_0/" +
            "0_0_6_6_6_6_0_0/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1-1", 05, 4.0f, 00, 0.50f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-1-1", 05, 4.0f, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1", 05, 4.0f, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.52f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1-1", 05, 4.0f, 00, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-1-1", 05, 4.0f, 01, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.58f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.58f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-1", 05, 4.0f, 10, 0.50f, 0.5f, 1.90f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.52f, 0.5f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.52f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1", 05, 4.0f, 00, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1-1", 05, 4.0f, 00, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-1-1", 05, 4.0f, 01, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1", 05, 4.0f, 00, 0.58f, 0.5f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-1", 05, 4.0f, 01, 0.58f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-22", "IOD-1-1", 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-1", 05, 4.0f, 10, 0.50f, 0.5f, 1.90f),
            }
        });

        // Stage 50
        stageDatas.Add(new StageData()
        {
            stageId = "SN_50",
            startLife = 20,
            startCost = 400,
            tileSize = 0.36f,
            wallData = new WallData(600),
            mapData =
            "3_2_2_2_2_2_2_2/" +
            "0_0_1_1_0_1_1_2/" +
            "0_0_1_1_0_1_1_2/" +
            "2_2_2_2_2_2_2_2/" +
            "2_0_0_0_0_0_0_0/" +
            "2_0_1_1_0_1_1_0/" +
            "2_0_1_1_0_1_1_0/" +
            "2_0_0_0_0_0_0_0/" +
            "2_2_2_2_2_2_2_4/",
            mapSkyData =
            "3_0_0_0_6_6_6_6/" +
            "6_0_0_0_6_0_0_6/" +
            "6_0_0_0_6_0_0_6/" +
            "6_0_0_0_6_0_0_6/" +
            "6_0_0_0_6_0_0_6/" +
            "6_0_0_0_6_0_0_6/" +
            "6_0_0_0_6_0_0_6/" +
            "6_0_0_0_6_0_0_6/" +
            "6_6_6_6_6_0_0_4/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1"    , 05, 4.0f, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1"    , 05, 4.0f, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1-1"  , 05, 4.0f, 00, 0.50f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-1-1"  , 05, 4.0f, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1"    , 05, 4.0f, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-24", "IOD-BOSS-2" , 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1"    , 05, 4.0f, 00, 0.52f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.52f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1"    , 05, 4.0f, 00, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1-1"  , 05, 4.0f, 00, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-1-1"  , 05, 4.0f, 01, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1"    , 05, 4.0f, 00, 0.58f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.58f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-24", "IOD-BOSS-2" , 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-1"    , 05, 4.0f, 10, 0.50f, 0.5f, 1.90f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1"    , 05, 4.0f, 00, 0.52f, 0.5f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.52f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1"    , 05, 4.0f, 00, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.54f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1-1"  , 05, 4.0f, 00, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-1-1"  , 05, 4.0f, 01, 0.56f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1"    , 05, 4.0f, 00, 0.58f, 0.5f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.58f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-24", "IOD-BOSS-2" , 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-1"    , 05, 4.0f, 10, 0.50f, 0.5f, 1.90f),
            }
        });

        #endregion

        #region < Stage 51~60 >

        // Stage 51
        stageDatas.Add(new StageData()
        {
            stageId = "SN_51",
            startLife = 20,
            startCost = 300,
            tileSize = 0.36f,
            wallData = new WallData(800),
            mapData =
            "3_2_2_2_2_2_2_0/" +
            "0_1_1_1_1_1_2_0/" +
            "2_2_2_2_2_2_2_0/" +
            "2_0_0_0_0_0_0_0/" +
            "2_1_2_2_2_0_1_1/" +
            "2_1_2_1_2_0_1_1/" +
            "2_5_2_1_2_0_1_1/" +
            "2_1_2_1_2_0_1_1/" +
            "2_2_2_0_4_0_1_1/",
            mapSkyData =
            "3_0_6_6_6_6_6_0/" +
            "6_0_6_0_0_0_6_0/" +
            "6_0_6_0_6_6_6_0/" +
            "6_0_6_0_6_0_0_0/" +
            "6_0_6_0_6_0_0_0/" +
            "6_0_6_0_6_0_0_0/" +
            "6_0_6_0_6_0_0_0/" +
            "6_0_6_0_6_0_0_0/" +
            "6_6_6_0_4_0_0_0/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-2"    , 20, 1.0f, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-2"    , 20, 1.0f, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-2-1"  , 20, 1.0f, 00, 0.50f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-2-1"  , 20, 1.0f, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-2"    , 20, 1.0f, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-2"    , 20, 1.0f, 00, 0.52f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.52f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-2"    , 20, 1.0f, 00, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-2-1"  , 20, 1.0f, 00, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-2-1"  , 20, 1.0f, 01, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-2"    , 20, 1.0f, 00, 0.58f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.58f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00, 0.50f + 0.01f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-2"    , 20, 1.0f, 10, 0.50f + 0.01f, 0.5f, 1.90f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-2"    , 20, 1.0f, 00, 0.52f + 0.01f, 0.5f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.52f + 0.01f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-2"    , 20, 1.0f, 00, 0.54f + 0.01f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.54f + 0.01f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-2-1"  , 20, 1.0f, 00, 0.56f + 0.01f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-2-1"  , 20, 1.0f, 01, 0.56f + 0.01f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-2"    , 20, 1.0f, 00, 0.58f + 0.01f, 0.5f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.58f + 0.01f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00, 0.50f + 0.01f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-2"    , 20, 1.0f, 10, 0.50f + 0.01f, 0.5f, 1.90f),
            }
        });

        // Stage 52
        stageDatas.Add(new StageData()
        {
            stageId = "SN_52",
            startLife = 20,
            startCost = 300,
            tileSize = 0.36f,
            wallData = new WallData(400),
            mapData =
            "3_2_2_1_1_1_2_2_4/" +
            "0_0_2_0_1_0_2_0_0/" +
            "1_0_2_0_1_0_2_0_1/" +
            "1_0_2_2_1_2_2_0_1/" +
            "1_0_0_2_1_2_0_0_1/" +
            "0_0_2_2_1_2_2_0_0/" +
            "2_2_2_0_1_0_2_2_2/" +
            "2_1_5_1_1_1_5_1_2/" +
            "2_2_2_2_2_2_2_2_2/",
            mapSkyData =
            "3_0_6_6_6_6_6_6_4/" +
            "6_0_6_0_0_0_0_0_0/" +
            "6_0_6_6_6_6_6_6_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_6_6_6_6_6_6_6_6/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-2"    , 20, 1.0f, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-2"    , 20, 1.0f, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-2-1"  , 20, 1.0f, 00, 0.50f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-2-1"  , 20, 1.0f, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-2"    , 20, 1.0f, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-25", "IOD-BOSS-3" , 01, 1.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-2"    , 20, 1.0f, 00, 0.52f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.52f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-2"    , 20, 1.0f, 00, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-2-1"  , 20, 1.0f, 00, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-2-1"  , 20, 1.0f, 01, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-2"    , 20, 1.0f, 00, 0.58f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.58f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-25", "IOD-BOSS-3" , 01, 1.0f, 00, 0.50f + 0.011f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-2"    , 20, 1.0f, 10, 0.50f + 0.011f, 0.5f, 1.90f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-2"    , 20, 1.0f, 00, 0.52f + 0.011f, 0.5f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.52f + 0.011f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-2"    , 20, 1.0f, 00, 0.54f + 0.011f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.54f + 0.011f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-2-1"  , 20, 1.0f, 00, 0.56f + 0.011f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-2-1"  , 20, 1.0f, 01, 0.56f + 0.011f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-2"    , 20, 1.0f, 00, 0.58f + 0.011f, 0.5f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-2"    , 20, 1.0f, 01, 0.58f + 0.011f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-25", "IOD-BOSS-3" , 01, 1.0f, 00, 0.50f + 0.011f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-2"    , 20, 1.0f, 10, 0.50f + 0.011f, 0.5f, 1.90f),
            }
        });

        // Stage 53
        stageDatas.Add(new StageData()
        {
            stageId = "SN_53",
            startLife = 20,
            startCost = 300,
            tileSize = 0.36f,
            wallData = new WallData(1000),
            mapData =
            "2_2_2_2_2_2_2_2/" +
            "2_1_1_0_0_1_1_2/" +
            "2_0_0_0_0_0_0_2/" +
            "2_0_0_1_1_0_0_2/" +
            "2_0_0_1_1_0_0_2/" +
            "2_0_0_1_1_0_0_2/" +
            "2_0_0_1_1_0_0_2/" +
            "2_0_0_0_0_0_0_2/" +
            "2_2_2_1_1_2_2_2/" +
            "0_1_2_1_1_2_1_0/" +
            "3_2_2_1_1_2_2_4/",
            mapSkyData =
            "0_0_0_0_0_0_0_0/" +
            "0_0_0_0_0_0_0_0/" +
            "0_0_0_0_0_0_0_0/" +
            "6_6_6_0_0_6_6_6/" +
            "6_0_6_0_0_6_0_6/" +
            "6_0_6_0_0_6_0_6/" +
            "6_0_6_0_0_6_0_6/" +
            "6_0_6_0_0_6_0_6/" +
            "6_0_6_0_0_6_0_6/" +
            "6_0_6_0_0_6_0_6/" +
            "3_0_6_6_6_6_0_4/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-3"    , 40, 0.5f, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-3"    , 40, 0.5f, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-3"    , 40, 0.5f, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-3"    , 40, 0.5f, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-2-1"  , 20, 1.0f, 00, 0.50f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-3-1"  , 40, 0.5f, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-3"    , 40, 0.5f, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-3"    , 40, 0.5f, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-25", "IOD-BOSS-3" , 01, 0.5f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-3"    , 40, 0.5f, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-2"    , 20, 1.0f, 00, 0.52f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-3"    , 40, 0.5f, 01, 0.52f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-3"    , 40, 0.5f, 00, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-3"    , 40, 0.5f, 01, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-3-1"  , 40, 0.5f, 00, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-3-1"  , 40, 0.5f, 01, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-2"    , 20, 1.0f, 00, 0.58f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-3"    , 40, 0.5f, 01, 0.58f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-25", "IOD-BOSS-3" , 01, 2.0f, 00, 0.50f + 0.012f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-3"    , 40, 0.5f, 10, 0.50f + 0.012f, 0.5f, 1.90f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-2"    , 20, 1.0f, 00, 0.52f + 0.012f, 0.5f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-3"    , 40, 0.5f, 01, 0.52f + 0.012f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-3"    , 40, 0.5f, 00, 0.54f + 0.012f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-3"    , 40, 0.5f, 01, 0.54f + 0.012f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-3-1"  , 40, 0.5f, 00, 0.56f + 0.012f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-3-1"  , 40, 0.5f, 01, 0.56f + 0.012f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-2"    , 20, 1.0f, 00, 0.58f + 0.012f, 0.5f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-3"    , 40, 0.5f, 01, 0.58f + 0.012f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-25", "IOD-BOSS-3" , 01, 0.5f, 00, 0.50f + 0.012f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-3"    , 40, 0.5f, 10, 0.50f + 0.012f, 0.5f, 1.90f),
            }
        });
        
        // Stage 54
        stageDatas.Add(new StageData()
        {
            stageId = "SN_54",
            startLife = 20,
            startCost = 300,
            tileSize = 0.36f,
            wallData = new WallData(800),
            mapData =
            "2_2_2_2_5_2_2_2_2/" +
            "2_1_1_2_0_2_1_1_2/" +
            "2_1_1_2_2_2_1_1_2/" +
            "2_0_0_0_0_0_0_0_2/" +
            "2_2_2_3_0_1_1_0_2/" +
            "0_0_0_0_0_1_1_0_2/" +
            "2_2_2_4_0_1_1_0_2/" +
            "2_1_1_0_0_0_0_0_2/" +
            "2_1_1_2_2_2_2_2_2/" +
            "2_2_2_2_1_1_1_1_1/",
            mapSkyData =
            "6_6_6_6_6_6_6_6_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_6_6_6_6/" +
            "6_6_6_3_0_6_0_0_0/" +
            "0_0_0_0_0_6_0_0_0/" +
            "6_6_6_4_0_6_6_6_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_6_6_6_6_6_6_6_6/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1"    , 05, 4.0f, 00, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.35f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1"    , 05, 4.0f, 00, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.45f, 0.5f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1-1"  , 05, 4.0f, 00, 0.50f, 0.5f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-1-1"  , 05, 4.0f, 01, 0.50f, 0.5f, 1.05f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1"    , 05, 4.0f, 00, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.50f, 0.5f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00, 0.50f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.50f, 0.5f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1"    , 05, 4.0f, 00, 0.52f, 0.5f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.52f, 0.5f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1"    , 05, 4.0f, 00, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.54f, 0.5f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1-1"  , 05, 4.0f, 00, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-1-1"  , 05, 4.0f, 01, 0.56f, 0.5f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1"    , 05, 4.0f, 00, 0.58f, 0.5f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.58f, 0.5f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00, 0.50f + 0.013f, 0.5f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-1"    , 05, 4.0f, 10, 0.50f + 0.013f, 0.5f, 1.90f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1"    , 05, 4.0f, 00, 0.52f + 0.013f, 0.5f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.52f + 0.013f, 0.5f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1"    , 05, 4.0f, 00, 0.54f + 0.013f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.54f + 0.013f, 0.5f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1-1"  , 05, 4.0f, 00, 0.56f + 0.013f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-1-1"  , 05, 4.0f, 01, 0.56f + 0.013f, 0.5f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1"    , 05, 4.0f, 00, 0.58f + 0.013f, 0.5f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-1"    , 05, 4.0f, 01, 0.58f + 0.013f, 0.5f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00, 0.50f + 0.013f, 0.5f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-1"    , 05, 4.0f, 10, 0.50f + 0.013f, 0.5f, 1.90f),
            }
        });

        // Stage 55
        stageDatas.Add(new StageData()
        {
            stageId = "SN_55",
            startLife = 20,
            startCost = 300,
            tileSize = 0.36f,
            wallData = new WallData(800),
            mapData =
            "0_0_0_0_0_0_0_0_0/" +
            "0_1_1_0_0_0_0_0_0/" +
            "0_2_2_3_0_0_0_0_0/" +
            "0_2_1_0_0_1_1_1_0/" +
            "0_2_1_0_0_0_0_0_0/" +
            "0_2_1_2_2_2_2_2_0/" +
            "0_2_1_2_1_0_1_2_0/" +
            "0_2_2_2_0_4_0_2_0/" +
            "0_1_1_1_1_2_1_2_0/" +
            "0_0_0_0_0_2_2_2_0/",
            mapSkyData =
            "6_6_6_6_0_0_0_0_0/" +
            "6_0_0_6_0_0_0_0_0/" +
            "6_0_0_3_0_0_0_0_0/" +
            "6_0_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_6_6_6_6/" +
            "6_0_0_0_0_6_0_0_6/" +
            "6_0_0_0_0_6_0_0_6/" +
            "6_0_0_0_0_4_0_0_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_6_6_6_6_6_6_6_6/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.35f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.35f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.45f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.45f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1-1"  , 04, 4.0f, 00.0f, 0.50f, 0.50f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-3-1"  , 20, 1.0f, 01.0f, 0.50f, 0.25f, 1.05f),
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-4-1"    , 10, 2.0f, 02.5f, 0.50f, 0.25f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.50f, 0.50f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.50f, 0.50f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00.0f, 0.50f, 0.50f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.50f, 0.50f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.52f, 0.50f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.52f, 0.50f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.54f, 0.50f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.54f, 0.50f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1-1"  , 05, 4.0f, 00.0f, 0.56f, 0.50f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-3-1"  , 20, 1.0f, 01.0f, 0.56f, 0.25f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-4-1"    , 10, 2.0f, 02.5f, 0.50f, 0.25f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.58f, 0.50f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.58f, 0.50f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00.0f, 0.50f + 0.013f, 0.50f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-3"    , 40, 0.5f, 10.0f, 0.50f + 0.012f, 0.50f, 1.90f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.52f + 0.013f, 0.50f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.52f + 0.012f, 0.50f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.54f + 0.013f, 0.50f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.54f + 0.012f, 0.50f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1-1"  , 05, 4.0f, 00.0f, 0.56f + 0.013f, 0.50f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-3-1"  , 20, 1.0f, 01.0f, 0.56f + 0.012f, 0.25f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-4-1"    , 10, 2.0f, 02.5f, 0.50f + 0.012f, 0.25f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.58f + 0.013f, 0.50f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.58f + 0.012f, 0.50f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00.0f, 0.50f + 0.013f, 0.50f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-3"    , 40, 0.5f, 10.0f, 0.50f + 0.012f, 0.50f, 1.90f),
            }
        });

        // Stage 56
        stageDatas.Add(new StageData()
        {
            stageId = "SN_56",
            startLife = 20,
            startCost = 300,
            tileSize = 0.36f,
            wallData = new WallData(800),
            mapData =
            "3_2_2_2_0_0_1_1_1_1_1/" +
            "0_1_1_2_0_0_0_0_0_1_1/" +
            "0_1_1_2_0_0_0_0_0_0_0/" +
            "0_0_0_2_2_2_2_2_2_2_2/" +
            "0_0_0_0_1_0_0_0_0_0_2/" +
            "0_0_0_0_0_1_0_0_0_0_2/" +
            "1_0_0_0_0_0_4_0_0_0_2/" +
            "1_0_0_0_0_0_2_1_0_1_2/" +
            "1_0_0_0_0_0_2_0_1_0_2/" +
            "1_1_0_0_0_0_2_1_0_1_2/" +
            "1_1_0_0_0_0_2_2_2_2_2/",
            mapSkyData =
            "3_0_0_0_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0_0_0_0_0/" +
            "6_6_6_6_0_0_0_0_0_0_0/" +
            "0_0_0_6_0_0_0_0_0_0_0/" +
            "0_0_0_6_0_0_0_0_0_0_0/" +
            "0_0_0_6_0_0_4_6_6_6_6/" +
            "0_0_0_6_0_0_0_0_0_0_6/" +
            "0_0_0_6_0_0_0_0_0_0_6/" +
            "0_0_0_6_0_0_0_0_0_0_6/" +
            "0_0_0_6_6_6_6_6_6_6_6/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.35f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.35f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.45f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.45f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1-1"  , 04, 4.0f, 00.0f, 0.50f, 0.50f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-3-1"  , 20, 1.0f, 01.0f, 0.50f, 0.25f, 1.05f),
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-4-1"    , 10, 2.0f, 02.5f, 0.50f, 0.25f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.50f, 0.50f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.50f, 0.50f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00.0f, 0.50f, 0.50f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.50f, 0.50f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.52f, 0.50f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.52f, 0.50f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.54f, 0.50f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.54f, 0.50f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1-1"  , 05, 4.0f, 00.0f, 0.56f, 0.50f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-3-1"  , 20, 1.0f, 01.0f, 0.56f, 0.25f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-4-1"    , 10, 2.0f, 02.5f, 0.50f, 0.25f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.58f, 0.50f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.58f, 0.50f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00.0f, 0.50f + 0.013f, 0.50f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-3"    , 40, 0.5f, 10.0f, 0.50f + 0.012f, 0.50f, 1.90f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.52f + 0.013f, 0.50f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.52f + 0.012f, 0.50f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.54f + 0.013f, 0.50f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.54f + 0.012f, 0.50f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1-1"  , 05, 4.0f, 00.0f, 0.56f + 0.013f, 0.50f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-3-1"  , 20, 1.0f, 01.0f, 0.56f + 0.012f, 0.25f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-4-1"    , 10, 2.0f, 02.5f, 0.50f + 0.012f, 0.25f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.58f + 0.013f, 0.50f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.58f + 0.012f, 0.50f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00.0f, 0.50f + 0.013f, 0.50f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-3"    , 40, 0.5f, 10.0f, 0.50f + 0.012f, 0.50f, 1.90f),
            }
        });

        // Stage 57
        stageDatas.Add(new StageData()
        {
            stageId = "SN_57",
            startLife = 20,
            startCost = 300,
            tileSize = 0.36f,
            wallData = new WallData(800),
            mapData =
            "1_1_0_0_1_1_0_3/" +
            "1_1_0_0_0_0_0_2/" +
            "0_2_2_2_2_2_2_2/" +
            "0_2_1_5_1_1_1_0/" +
            "0_2_2_2_2_2_2_0/" +
            "0_0_1_1_5_1_2_0/" +
            "0_2_2_2_2_2_2_0/" +
            "0_2_1_5_1_1_1_0/" +
            "0_2_2_2_2_2_2_2/" +
            "1_1_0_0_0_0_0_2/" +
            "1_1_0_0_1_1_0_4/",
            mapSkyData =
            "6_6_6_6_6_6_6_3/" +
            "6_0_0_0_0_0_0_0/" +
            "6_0_6_6_6_6_6_0/" +
            "6_0_6_0_0_0_6_0/" +
            "6_0_6_0_6_6_6_0/" +
            "6_0_6_0_6_0_0_0/" +
            "6_0_6_0_6_6_6_6/" +
            "6_0_6_0_0_0_0_6/" +
            "6_0_6_0_6_6_6_6/" +
            "6_6_6_0_6_0_0_0/" +
            "0_0_0_0_6_6_6_4/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.35f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.35f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.45f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.45f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1-1"  , 04, 4.0f, 00.0f, 0.50f, 0.50f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-3-1"  , 20, 1.0f, 01.0f, 0.50f, 0.25f, 1.05f),
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-4-1"    , 10, 2.0f, 02.5f, 0.50f, 0.25f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.50f, 0.50f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.50f, 0.50f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00.0f, 0.50f, 0.50f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.50f, 0.50f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.52f, 0.50f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.52f, 0.50f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.54f, 0.50f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.54f, 0.50f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1-1"  , 05, 4.0f, 00.0f, 0.56f, 0.50f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-3-1"  , 20, 1.0f, 01.0f, 0.56f, 0.25f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-4-1"    , 10, 2.0f, 02.5f, 0.50f, 0.25f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.58f, 0.50f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.58f, 0.50f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00.0f, 0.50f + 0.013f, 0.50f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-3"    , 40, 0.5f, 10.0f, 0.50f + 0.012f, 0.50f, 1.90f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.52f + 0.013f, 0.50f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.52f + 0.012f, 0.50f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.54f + 0.013f, 0.50f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.54f + 0.012f, 0.50f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1-1"  , 05, 4.0f, 00.0f, 0.56f + 0.013f, 0.50f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-3-1"  , 20, 1.0f, 01.0f, 0.56f + 0.012f, 0.25f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-4-1"    , 10, 2.0f, 02.5f, 0.50f + 0.012f, 0.25f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.58f + 0.013f, 0.50f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.58f + 0.012f, 0.50f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00.0f, 0.50f + 0.013f, 0.50f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-3"    , 40, 0.5f, 10.0f, 0.50f + 0.012f, 0.50f, 1.90f),
            }
        });

        // Stage 58
        stageDatas.Add(new StageData()
        {
            stageId = "SN_58",
            startLife = 20,
            startCost = 300,
            tileSize = 0.36f,
            wallData = new WallData(800),
            mapData =
            "2_2_3_0_0_0_0_0_0/" +
            "2_0_0_0_0_1_1_1_0/" +
            "2_0_1_1_0_1_1_1_0/" +
            "2_0_0_0_0_0_0_0_0/" +
            "2_1_2_2_2_1_2_2_2/" +
            "2_5_2_1_2_5_2_1_2/" +
            "2_1_2_5_2_1_2_5_2/" +
            "2_2_2_1_2_2_2_1_2/" +
            "0_0_0_0_0_0_0_0_2/" +
            "0_1_1_1_0_1_1_0_2/" +
            "0_1_1_1_0_0_0_0_2/" +
            "0_0_0_0_0_0_4_2_2/",
            mapSkyData =
            "0_0_3_6_6_6_6_6_6/" +
            "0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_6/" +
            "6_6_6_0_6_6_6_0_6/" +
            "6_0_6_0_6_0_6_0_6/" +
            "6_0_6_6_6_0_6_6_6/" +
            "6_0_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0_0_0/" +
            "6_6_6_6_6_6_4_0_0/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.35f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.35f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.45f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.45f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1-1"  , 04, 4.0f, 00.0f, 0.50f, 0.50f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-3-1"  , 20, 1.0f, 01.0f, 0.50f, 0.25f, 1.05f),
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-4-1"    , 10, 2.0f, 02.5f, 0.50f, 0.25f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.50f, 0.50f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.50f, 0.50f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00.0f, 0.50f, 0.50f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.50f, 0.50f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.52f, 0.50f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.52f, 0.50f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.54f, 0.50f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.54f, 0.50f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1-1"  , 05, 4.0f, 00.0f, 0.56f, 0.50f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-3-1"  , 20, 1.0f, 01.0f, 0.56f, 0.25f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-4-1"    , 10, 2.0f, 02.5f, 0.50f, 0.25f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.58f, 0.50f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.58f, 0.50f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00.0f, 0.50f + 0.013f, 0.50f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-3"    , 40, 0.5f, 10.0f, 0.50f + 0.012f, 0.50f, 1.90f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.52f + 0.013f, 0.50f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.52f + 0.012f, 0.50f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.54f + 0.013f, 0.50f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.54f + 0.012f, 0.50f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1-1"  , 05, 4.0f, 00.0f, 0.56f + 0.013f, 0.50f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-3-1"  , 20, 1.0f, 01.0f, 0.56f + 0.012f, 0.25f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-4-1"    , 10, 2.0f, 02.5f, 0.50f + 0.012f, 0.25f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.58f + 0.013f, 0.50f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.58f + 0.012f, 0.50f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00.0f, 0.50f + 0.013f, 0.50f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-3"    , 40, 0.5f, 10.0f, 0.50f + 0.012f, 0.50f, 1.90f),
            }
        });
        
        // Stage 59
        stageDatas.Add(new StageData()
        {
            stageId = "SN_59",
            startLife = 20,
            startCost = 300,
            tileSize = 0.36f,
            wallData = new WallData(800),
            mapData =
            "2_2_2_2_0_0_0_0_0/" +
            "2_1_1_2_1_0_1_1_0/" +
            "2_1_1_2_1_0_1_1_0/" +
            "2_0_0_3_0_0_2_2_2/" +
            "2_1_0_0_0_0_2_1_2/" +
            "2_5_2_2_2_2_2_5_2/" +
            "2_1_2_0_0_0_0_1_2/" +
            "2_2_2_0_0_4_0_0_2/" +
            "0_1_1_0_1_2_1_1_2/" +
            "0_1_1_0_1_2_1_1_2/" +
            "0_0_0_0_0_2_2_2_2/",
            mapSkyData =
            "6_6_6_6_6_6_6_6_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_6_6_3_0_0_0_0_6/" +
            "0_0_0_0_0_0_0_0_6/" +
            "6_6_6_6_6_6_6_6_6/" +
            "6_0_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_4_6_6_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_6/" +
            "6_6_6_6_6_6_6_6_6/",
            waveDatas = new List<IngameObjectSpawnInfo>()
            {
                new IngameObjectSpawnInfo( 1, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.35f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 1, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.35f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.45f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 2, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.45f, 0.50f, 1.00f),
                new IngameObjectSpawnInfo( 3, "IO-17", "IOD-2-1-1"  , 04, 4.0f, 00.0f, 0.50f, 0.50f, 1.05f), // Destroy
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-3-3-1"  , 20, 1.0f, 01.0f, 0.50f, 0.25f, 1.05f),
                new IngameObjectSpawnInfo( 3, "IO-20", "IOD-4-1"    , 10, 2.0f, 02.5f, 0.50f, 0.25f, 1.00f),
                new IngameObjectSpawnInfo( 4, "IO-22", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.50f, 0.50f, 1.10f),
                new IngameObjectSpawnInfo( 4, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.50f, 0.50f, 1.10f),
                new IngameObjectSpawnInfo( 5, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00.0f, 0.50f, 0.50f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo( 5, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.50f, 0.50f, 2.00f),
                new IngameObjectSpawnInfo( 6, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.52f, 0.50f, 1.15f), // Destroy
                new IngameObjectSpawnInfo( 6, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.52f, 0.50f, 1.15f),
                new IngameObjectSpawnInfo( 7, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.54f, 0.50f, 1.20f),
                new IngameObjectSpawnInfo( 7, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.54f, 0.50f, 1.20f),
                new IngameObjectSpawnInfo( 8, "IO-22", "IOD-1-1-1"  , 05, 4.0f, 00.0f, 0.56f, 0.50f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-3-3-1"  , 20, 1.0f, 01.0f, 0.56f, 0.25f, 1.25f),
                new IngameObjectSpawnInfo( 8, "IO-20", "IOD-4-1"    , 10, 2.0f, 02.5f, 0.50f, 0.25f, 1.25f),
                new IngameObjectSpawnInfo( 9, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.58f, 0.50f, 1.30f), // Destroy
                new IngameObjectSpawnInfo( 9, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.58f, 0.50f, 1.30f),
                new IngameObjectSpawnInfo(10, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00.0f, 0.50f + 0.013f, 0.50f, 2.00f, IngameObject.EClass.Hero),
                new IngameObjectSpawnInfo(10, "IO-20", "IOD-3-3"    , 40, 0.5f, 10.0f, 0.50f + 0.012f, 0.50f, 1.90f),
                new IngameObjectSpawnInfo(11, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.52f + 0.013f, 0.50f, 1.35f), // Destroy
                new IngameObjectSpawnInfo(11, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.52f + 0.012f, 0.50f, 1.35f),
                new IngameObjectSpawnInfo(12, "IO-22", "IOD-1-1"    , 05, 4.0f, 00.0f, 0.54f + 0.013f, 0.50f, 1.40f),
                new IngameObjectSpawnInfo(12, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.54f + 0.012f, 0.50f, 1.40f),
                new IngameObjectSpawnInfo(13, "IO-22", "IOD-1-1-1"  , 05, 4.0f, 00.0f, 0.56f + 0.013f, 0.50f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-3-3-1"  , 20, 1.0f, 01.0f, 0.56f + 0.012f, 0.25f, 1.45f),
                new IngameObjectSpawnInfo(13, "IO-20", "IOD-4-1"    , 10, 2.0f, 02.5f, 0.50f + 0.012f, 0.25f, 1.45f),
                new IngameObjectSpawnInfo(14, "IO-17", "IOD-2-1"    , 05, 4.0f, 00.0f, 0.58f + 0.013f, 0.50f, 1.50f), // Destroy
                new IngameObjectSpawnInfo(14, "IO-20", "IOD-3-3"    , 40, 0.5f, 01.0f, 0.58f + 0.012f, 0.50f, 1.50f),
                new IngameObjectSpawnInfo(15, "IO-26", "IOD-BOSS-1" , 01, 2.0f, 00.0f, 0.50f + 0.013f, 0.50f, 2.00f, IngameObject.EClass.Boss),
                new IngameObjectSpawnInfo(15, "IO-20", "IOD-3-3"    , 40, 0.5f, 10.0f, 0.50f + 0.012f, 0.50f, 1.90f),
            }
        });

        #endregion

        // Infinity Mode
        stageDatas.Add(new StageData()
        {
            stageMode = EStageMode.Infinity,
            stageId = "SN_Infinity",
            startLife = 20,
            startCost = 300,
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 3.5f,
            tileSize = 0.36f,
            wallData = new WallData(1600),
            mapData =
            "0_0_0_0_0_0_0_0_0_0_0_0_0_0/" +
            "0_0_0_0_0_0_0_4_0_0_0_0_0_0/" +
            "0_0_0_0_0_0_0_2_0_0_0_0_0_0/" +
            "0_0_0_0_0_0_0_2_0_0_0_0_0_0/" +
            "0_0_0_0_0_0_0_2_0_0_0_0_0_0/" +
            "0_0_0_0_0_2_2_2_0_0_0_0_0_0/" +
            "0_0_0_0_2_2_0_0_0_0_0_0_0_0/" +
            "0_0_0_0_2_0_0_0_0_0_0_0_0_0/" +
            "0_0_0_0_2_0_0_0_0_0_3_0_0_0/" +
            "0_0_0_0_2_0_0_0_0_0_2_0_0_0/" +
            "0_0_0_0_2_2_0_0_0_2_2_0_0_0/" +
            "0_0_0_0_0_2_2_2_2_2_0_0_0_0/" +
            "0_0_0_0_0_0_0_0_0_0_0_0_0_0/",
            mapSkyData = null,
            waveDatas = new List<IngameObjectSpawnInfo>() { }
        });

        for(int i = 0; i<stageDatas.Count; ++i)
        {
            stageIdxById.Add(stageDatas[i].stageId, i);
        }
    }

    private void LoadInfinityModeMapDatas()
    {
        stageMapDatas.Add(new StageMapData()
        {
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 4.8f,
            tileSize = 0.36f,
            wallData = new WallData(1600),
            mapData =
            "2_2_2_0_1_0_0_0_1_0_0_0_1/" +
            "2_1_2_0_0_0_0_0_0_0_1_0_0/" +
            "2_0_2_0_2_2_2_0_1_0_0_0_1/" +
            "2_0_2_1_2_1_2_0_0_0_0_0_0/" +
            "2_1_2_5_2_0_2_0_2_2_2_0_0/" +
            "2_0_2_0_2_0_2_5_2_1_2_0_1/" +
            "2_0_2_1_2_0_2_1_2_0_2_0_1/" +
            "4_0_2_2_2_0_2_2_2_0_3_0_1/" +
            "1_0_0_0_0_1_0_0_0_0_0_0_0/",
            mapSkyData =
            "0_6_6_6_6_6_6_6_6_0_6_6_6/" +
            "0_6_0_0_0_0_0_0_6_0_6_0_6/" +
            "0_6_0_0_0_0_0_0_6_0_6_0_6/" +
            "0_6_0_0_0_0_0_0_6_6_6_0_6/" +
            "0_6_0_0_0_0_0_0_0_0_0_0_6/" +
            "0_6_0_0_0_0_0_0_0_0_0_0_6/" +
            "0_6_0_0_0_0_0_0_0_0_0_0_6/" +
            "4_6_0_0_0_0_0_0_0_0_3_0_6/" +
            "0_0_0_0_0_0_0_0_0_0_6_6_6/",
        });

        stageMapDatas.Add(new StageMapData()
        {
            lobbyZoomSize = 6.5f,
            ingameZoomSize = 6f,
            tileSize = 0.36f,
            wallData = new WallData(1600),
            mapData =
            "2_2_2_2_2_1_0_1_2_2_2_2_2_0/" +
            "2_0_0_0_2_0_0_0_2_0_0_0_2_0/" +
            "2_0_1_0_2_0_1_0_2_0_1_0_2_0/" +
            "2_0_0_0_2_0_0_0_2_0_0_0_2_0/" +
            "2_2_2_5_2_2_2_2_2_5_2_2_2_0/" +
            "1_0_2_1_0_0_0_0_0_1_2_0_0_0/" +
            "0_0_2_0_0_0_1_0_0_0_2_0_0_0/" +
            "0_1_2_0_0_1_1_1_0_0_2_2_3_0/" +
            "0_0_2_0_0_0_1_0_0_0_0_0_0_0/" +
            "1_0_2_1_0_0_0_0_0_1_0_0_0_0/" +
            "2_2_2_5_2_2_2_2_2_0_4_2_2_0/" +
            "2_0_0_0_2_0_0_0_2_0_0_0_2_0/" +
            "2_0_1_0_2_0_1_0_2_0_1_0_2_0/" +
            "2_0_0_0_2_0_0_0_2_0_0_0_2_0/" +
            "2_2_2_2_2_1_0_1_2_2_2_2_2_0/",
            mapSkyData =
            "6_6_6_6_6_6_6_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_6_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_6_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_6_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_6_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_6_6_6_6_6_6_6_0/" +
            "6_0_0_0_0_0_0_0_0_0_0_0_6_0/" +
            "6_0_0_0_0_0_0_0_0_0_0_0_3_0/" +
            "6_0_0_0_0_0_0_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0_0_0_0_4_0_0_0/" +
            "6_0_0_0_0_0_0_0_0_0_6_0_0_0/" +
            "6_0_0_0_0_0_0_0_0_0_6_0_0_0/" +
            "6_0_0_0_0_0_0_0_0_0_6_0_0_0/" +
            "6_6_6_6_6_6_6_6_6_6_6_0_0_0/",
        });

        stageMapDatas.Add(new StageMapData()
        {
            lobbyZoomSize = 6.5f,
            ingameZoomSize = 5.5f,
            tileSize = 0.36f,
            wallData = new WallData(1600),
            mapData =
            "1_0_0_0_1_0_1_0_0_0_1/" +
            "0_2_2_2_2_2_2_2_2_2_0/" +
            "0_2_0_0_1_1_0_0_0_2_0/" +
            "0_2_0_1_0_0_0_1_0_2_0/" +
            "0_2_0_2_2_2_2_2_0_2_1/" +
            "0_2_0_2_1_0_0_2_0_2_0/" +
            "1_2_0_2_0_0_1_4_0_2_0/" +
            "1_2_5_2_2_2_0_0_0_2_0/" +
            "0_2_1_0_1_2_0_0_0_2_1/" +
            "0_2_0_0_0_2_0_1_0_2_0/" +
            "0_2_1_0_1_2_0_0_0_2_0/" +
            "0_2_2_2_2_2_0_1_0_3_0/" +
            "1_0_0_0_1_0_0_0_0_0_1/",
            mapSkyData =
            "6_6_6_6_6_6_6_6_6_6_6/" +
            "6_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_0_0_0_6/" +
            "6_0_0_0_0_0_0_4_6_6_6/" +
            "6_0_0_0_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0_0_0_0_0/" +
            "6_0_0_0_0_0_0_0_0_0_0/" +
            "6_0_6_6_6_0_6_6_6_0_0/" +
            "6_0_6_0_6_0_6_0_6_3_0/" +
            "6_6_6_0_6_6_6_0_0_0_0/",
        });

        stageMapDatas.Add(new StageMapData()
        {
            lobbyZoomSize = 6.5f,
            ingameZoomSize = 5.5f,
            tileSize = 0.36f,
            wallData = new WallData(1600),
            mapData =
            "1_0_0_0_1_1_0_0_0_1_0_0_0_1/" +
            "0_0_0_0_0_0_0_0_1_0_0_0_0_0/" +
            "1_0_2_2_2_2_2_2_2_2_2_2_2_0/" +
            "0_0_2_0_0_0_0_0_0_1_0_0_2_0/" +
            "0_1_2_0_0_1_0_1_2_2_2_1_2_0/" +
            "0_0_2_0_0_0_0_0_2_1_2_0_2_0/" +
            "1_0_2_0_0_0_0_0_2_0_2_5_2_0/" +
            "0_0_2_0_0_1_0_0_2_0_2_1_2_0/" +
            "0_1_2_0_0_0_0_0_2_0_2_0_2_0/" +
            "0_0_2_0_0_1_0_0_2_0_2_1_2_0/" +
            "1_0_3_0_0_0_0_0_4_0_2_2_2_0/" +
            "0_0_0_0_0_0_0_0_0_0_0_0_0_0/" +
            "1_0_1_0_0_0_1_1_0_0_0_1_0_1/",
            mapSkyData =
            "0_0_0_0_6_6_6_6_6_6_6_6_6_6/" +
            "0_0_0_0_6_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_6_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_6_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_6_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_6_0_0_0_0_0_0_0_0_6/" +
            "0_0_0_0_6_0_0_0_6_6_6_6_0_6/" +
            "0_0_0_0_6_0_0_0_6_0_0_6_0_6/" +
            "0_0_0_0_6_0_0_0_6_0_0_6_0_6/" +
            "0_0_0_0_6_0_0_0_6_0_6_6_0_6/" +
            "6_6_3_0_6_0_0_0_4_0_6_0_0_6/" +
            "6_0_0_0_6_0_0_0_0_0_6_6_6_6/" +
            "6_6_6_6_6_0_0_0_0_0_0_0_0_0/",
        });

        stageMapDatas.Add(new StageMapData()
        {
            lobbyZoomSize = 6.0f,
            ingameZoomSize = 5.0f,
            tileSize = 0.36f,
            wallData = null,
            mapData =
            "0_0_0_0_0_0_0_0_0_0_0_0_0/" +
            "0_0_2_2_2_2_2_2_2_2_2_0_0/" +
            "0_0_2_1_0_0_0_0_0_1_2_0_0/" +
            "0_0_2_0_2_2_2_2_2_0_2_0_0/" +
            "0_0_2_0_2_1_1_1_2_0_2_0_0/" +
            "0_0_2_0_2_1_1_1_4_0_2_0_0/" +
            "0_0_2_0_2_1_1_1_0_1_2_0_0/" +
            "0_0_2_0_2_2_2_2_2_2_2_0_0/" +
            "0_0_2_1_0_0_0_0_0_0_1_0_0/" +
            "0_0_2_2_2_2_2_2_2_2_3_0_0/" +
            "0_0_0_0_0_0_0_0_0_0_0_0_0/",
            mapSkyData =
            "0_0_0_0_0_0_0_0_0_0_0_0_0/" +
            "0_0_0_0_0_0_0_0_0_0_0_0_0/" +
            "0_0_0_6_6_6_6_6_6_6_6_0_0/" +
            "0_0_0_6_0_0_0_0_0_0_6_0_0/" +
            "0_0_0_6_0_0_0_0_0_0_6_0_0/" +
            "0_0_0_6_0_0_0_0_4_0_6_0_0/" +
            "0_0_0_6_0_0_0_0_6_0_6_0_0/" +
            "0_0_0_6_0_0_0_0_6_0_6_0_0/" +
            "0_0_0_6_6_6_6_6_6_0_6_0_0/" +
            "0_0_0_0_0_0_0_0_0_0_3_0_0/" +
            "0_0_0_0_0_0_0_0_0_0_0_0_0/",
        });

        stageMapDatas.Add(new StageMapData()
        {
            lobbyZoomSize = 5.5f,
            ingameZoomSize = 4.5f,
            tileSize = 0.36f,
            wallData = new WallData(3000),
            mapData =
            "3_2_2_2_0_0_0_0_0_0_0/" +
            "0_0_1_2_0_1_1_0_1_1_0/" +
            "0_0_1_2_0_0_0_0_1_1_0/" +
            "0_0_0_2_0_2_2_2_0_0_0/" +
            "0_1_0_2_1_2_1_2_0_1_0/" +
            "0_1_0_2_1_2_5_2_0_1_0/" +
            "0_0_0_2_2_2_1_2_0_0_0/" +
            "0_1_1_0_0_0_0_2_1_0_0/" +
            "0_1_1_0_1_1_0_2_1_0_0/" +
            "0_0_0_0_0_0_0_2_2_2_4/",
            mapSkyData =
            "3_0_0_0_0_6_6_6_6_6_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_0_0_0_0_6_0_0_0_0_6/" +
            "6_6_6_6_6_6_0_0_0_0_4/",
        });

    }

    #region < Lagacy >

    private void LoadSkillMapDatas()
    {
        List<SkillData> datas = new List<SkillData>();
        string _IS_DOUBLE = Global.SKILL_SUB_DATA_IS_DOUBLE;
        string _IS_TRIPLE = Global.SKILL_SUB_DATA_IS_TRIPLE;
        string _IS_QUADRUPLE = Global.SKILL_SUB_DATA_IS_QUADRUPLE;

        #region < Create All Skill Datas >

        #region < 순풍 >

        // 순풍
        datas.Add(new SkillData()
        {
            skillID = "Skill_ALL_1_1_0",
            skillTier = 0,
            spriteName = "ALL_1",
            starCost = 10,
            needSkillIDs = null,
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "0.2" }
        });

        // 순풍
        datas.Add(new SkillData()
        {
            skillID = "Skill_ALL_1_1_1",
            skillTier = 0,
            spriteName = "ALL_1",
            starCost = 20,
            needSkillIDs = null,
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "0.4" }
        });

        // 순풍
        datas.Add(new SkillData()
        {
            skillID = "Skill_ALL_1_1_2",
            skillTier = 0,
            spriteName = "ALL_1",
            starCost = 30,
            needSkillIDs = null,
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "0.6" }
        });

        #endregion

        #region < 최대 레벨 증가 >



        #endregion

        #endregion

        #region < Create Build Skill Datas >

        #region < TD - 1 >
        // 기본 타워
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_101_1_0",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_101",
            starCost = 0,
            subData = "TD-1"
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_101_1_1",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_101",
            starCost = 40,
            subData = "TD-1",
            needSkillIDs = new string[] { "Skill_TAE_101_1_0" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_101_1_2",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_101",
            starCost = 60,
            subData = "TD-1",
            needSkillIDs = new string[] { "Skill_TAE_101_1_1" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_101_1_3",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_101",
            starCost = 80,
            subData = "TD-1",
            needSkillIDs = new string[] { "Skill_TAE_101_1_2" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_101_1_4",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_101",
            starCost = 100,
            subData = "TD-1",
            needSkillIDs = new string[] { "Skill_TAE_101_1_3" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_101_1_5",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_101",
            starCost = 120,
            subData = "TD-1",
            needSkillIDs = new string[] { "Skill_TAE_101_1_4" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        #endregion

        #region < TD - 2 >
        // 스나이퍼 타워
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_102_1_0",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_102",
            starCost = 10,
            subData = "TD-2"
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_102_1_1",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_102",
            starCost = 40,
            subData = "TD-2",
            needSkillIDs = new string[] { "Skill_TAE_102_1_0" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_102_1_2",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_102",
            starCost = 60,
            subData = "TD-2",
            needSkillIDs = new string[] { "Skill_TAE_102_1_1" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_102_1_3",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_102",
            starCost = 80,
            subData = "TD-2",
            needSkillIDs = new string[] { "Skill_TAE_102_1_2" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_102_1_4",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_102",
            starCost = 100,
            subData = "TD-2",
            needSkillIDs = new string[] { "Skill_TAE_102_1_3" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_102_1_5",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_102",
            starCost = 120,
            subData = "TD-2",
            needSkillIDs = new string[] { "Skill_TAE_102_1_4" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        #endregion

        #region < TD - 3 >
        // 멀티샷 타워
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_103_1_0",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_103",
            starCost = 20,
            subData = "TD-3"
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_103_1_1",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_103",
            starCost = 40,
            subData = "TD-3",
            needSkillIDs = new string[] { "Skill_TAE_103_1_0" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_103_1_2",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_103",
            starCost = 60,
            subData = "TD-3",
            needSkillIDs = new string[] { "Skill_TAE_103_1_1" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_103_1_3",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_103",
            starCost = 80,
            subData = "TD-3",
            needSkillIDs = new string[] { "Skill_TAE_103_1_2" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_103_1_4",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_103",
            starCost = 100,
            subData = "TD-3",
            needSkillIDs = new string[] { "Skill_TAE_103_1_3" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_103_1_5",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_103",
            starCost = 120,
            subData = "TD-3",
            needSkillIDs = new string[] { "Skill_TAE_103_1_4" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        #endregion

        #region < TD - 4 >
        // 레이저 타워
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_104_1_0",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_104",
            starCost = 50,
            subData = "TD-4"
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_104_1_1",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_104",
            starCost = 60,
            subData = "TD-4",
            needSkillIDs = new string[] { "Skill_TAE_104_1_0" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_104_1_2",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_104",
            starCost = 80,
            subData = "TD-4",
            needSkillIDs = new string[] { "Skill_TAE_104_1_1" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_104_1_3",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_104",
            starCost = 100,
            subData = "TD-4",
            needSkillIDs = new string[] { "Skill_TAE_104_1_2" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_104_1_4",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_104",
            starCost = 120,
            subData = "TD-4",
            needSkillIDs = new string[] { "Skill_TAE_104_1_3" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_104_1_5",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_104",
            starCost = 150,
            subData = "TD-4",
            needSkillIDs = new string[] { "Skill_TAE_104_1_4" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        #endregion

        #region < TD - 5 >
        // 빔 타워
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_105_1_0",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_105",
            starCost = 50,
            subData = "TD-5"
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_105_1_1",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_105",
            starCost = 60,
            subData = "TD-5",
            needSkillIDs = new string[] { "Skill_TAE_105_1_0" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_105_1_2",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_105",
            starCost = 80,
            subData = "TD-5",
            needSkillIDs = new string[] { "Skill_TAE_105_1_1" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_105_1_3",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_105",
            starCost = 100,
            subData = "TD-5",
            needSkillIDs = new string[] { "Skill_TAE_105_1_2" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_105_1_4",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_105",
            starCost = 120,
            subData = "TD-5",
            needSkillIDs = new string[] { "Skill_TAE_105_1_3" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_105_1_5",
            skillTier = 0,
            skillStyle = 0,
            spriteName = "TAE_105",
            starCost = 150,
            subData = "TD-5",
            needSkillIDs = new string[] { "Skill_TAE_105_1_4" },
            needSkillIDsOperator = 0,
            needStageID = "SN_30",
        });
        #endregion  

        #endregion

        #region < Create 1Tier Skill Datas >

        #region < ElectricShock >

        // TowerAdditionalEffect_ElectricShock
        datas.Add(new SkillData() { skillID = "Skill_TAE_5_1_0", skillTier = 1, spriteName = "TAE_5", starCost = 0, needSkillIDs = null, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "250", "0.2", "3" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_5_1_1", skillTier = 1, spriteName = "TAE_5", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0 
                                    , noNeedSkillIDs = new string[] { "Skill_TAE_5_2_1" }
                                    , skillStyle = 0, effectDatas = new string[]{ "250", "0.25", "3" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_5_1_2", skillTier = 1, spriteName = "TAE_5", starCost = 30, needSkillIDs = new string[] { "Skill_TAE_5_1_1" }, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "250", "0.3", "3" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_5_1_3", skillTier = 1, spriteName = "TAE_5", starCost = 40, needSkillIDs = new string[] { "Skill_TAE_5_1_2" }, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "250", "0.35", "3" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_5_1_4", skillTier = 1, spriteName = "TAE_5", starCost = 50, needSkillIDs = new string[] { "Skill_TAE_5_1_3" }, needSkillIDsOperator = 0 
                                    , skillStyle = 1, effectDatas = new string[]{ "250", "0.35", "3", "4", "0.4" }, subData = _IS_TRIPLE });
        
        // TowerAdditionalEffect_ElectricShock
        datas.Add(new SkillData() { skillID = "Skill_TAE_5_2_0", skillTier = 1, spriteName = "TAE_5", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "250", "0.2", "3" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_5_2_1", skillTier = 1, spriteName = "TAE_5", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0 
                                    , noNeedSkillIDs = new string[] { "Skill_TAE_5_1_1" }
                                    , skillStyle = 0, effectDatas = new string[]{ "250", "0.2", "3.75" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_5_2_2", skillTier = 1, spriteName = "TAE_5", starCost = 40, needSkillIDs = new string[] { "Skill_TAE_5_2_1" }, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "250", "0.2", "4.5" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_5_2_3", skillTier = 1, spriteName = "TAE_5", starCost = 60, needSkillIDs = new string[] { "Skill_TAE_5_2_2" }, needSkillIDsOperator = 0
                                    , skillStyle = 2, effectDatas = new string[]{ "250", "0.2", "4.5", "1.5" }, subData = _IS_DOUBLE });

        #endregion

        #region < Transition >

        // TowerAdditionalEffect_Transition
        datas.Add(new SkillData() { skillID = "Skill_TAE_6_1_0", skillTier = 1, spriteName = "TAE_6", starCost = 0, needSkillIDs = null, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "200", "0.6", "2", "0.7" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_6_1_1", skillTier = 1, spriteName = "TAE_6", starCost = 40, needSkillIDs = null, needSkillIDsOperator = 0 
                                    , noNeedSkillIDs = new string[] { "Skill_TAE_6_2_1" }
                                    , skillStyle = 0, effectDatas = new string[]{ "200", "0.6", "3", "0.7" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_6_1_2", skillTier = 1, spriteName = "TAE_6", starCost = 70, needSkillIDs = new string[] { "Skill_TAE_6_1_1" }, needSkillIDsOperator = 0 
                                    , skillStyle = 1, effectDatas = new string[]{ "200", "0.6", "3", "0.7", "1" }, subData = _IS_DOUBLE });
        
        // TowerAdditionalEffect_Transition
        datas.Add(new SkillData() { skillID = "Skill_TAE_6_2_0", skillTier = 1, spriteName = "TAE_6", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "200", "0.6", "2", "0.7" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_6_2_1", skillTier = 1, spriteName = "TAE_6", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0 
                                    , noNeedSkillIDs = new string[] { "Skill_TAE_6_1_1" }
                                    , skillStyle = 0, effectDatas = new string[]{ "200", "0.6", "2", "0.85" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_6_2_2", skillTier = 1, spriteName = "TAE_6", starCost = 40, needSkillIDs = new string[] { "Skill_TAE_6_2_1" }, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "200", "0.6", "2", "1" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_6_2_3", skillTier = 1, spriteName = "TAE_6", starCost = 60, needSkillIDs = new string[] { "Skill_TAE_6_2_2" }, needSkillIDsOperator = 0
                                    , skillStyle = 2, effectDatas = new string[]{ "200", "0.6", "2", "1", "0.4", "0.5" }, subData = _IS_TRIPLE });

        #endregion

        #region < Berserker >

        // TowerAdditionalEffect_Berserker
        datas.Add(new SkillData() { skillID = "Skill_TAE_13_1_0", skillTier = 1, spriteName = "TAE_13", starCost = 0, needSkillIDs = null, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "200", "0.15", "0.2", "3", "6" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_13_1_1", skillTier = 1, spriteName = "TAE_13", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0
                                    , noNeedSkillIDs = new string[] { "Skill_TAE_13_2_1", "Skill_TAE_13_3_1" }
                                    , skillStyle = 0, effectDatas = new string[]{ "200", "0.19", "0.2", "3", "6" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_13_1_2", skillTier = 1, spriteName = "TAE_13", starCost = 40, needSkillIDs = new string[] { "Skill_TAE_13_1_1" }, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "200", "0.23", "0.2", "3", "6" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_13_1_3", skillTier = 1, spriteName = "TAE_13", starCost = 60, needSkillIDs = new string[] { "Skill_TAE_13_1_2" }, needSkillIDsOperator = 0 
                                    , skillStyle = 1, effectDatas = new string[]{ "200", "0.23", "0.2", "3", "6", "0.65" }, subData = _IS_DOUBLE });
        
        // TowerAdditionalEffect_Berserker
        datas.Add(new SkillData() { skillID = "Skill_TAE_13_2_0", skillTier = 1, spriteName = "TAE_13", starCost = 10, needSkillIDs = null, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "200", "0.15", "0.2", "2", "6" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_13_2_1", skillTier = 1, spriteName = "TAE_13", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0
                                    , noNeedSkillIDs = new string[] { "Skill_TAE_13_1_1", "Skill_TAE_13_3_1" }
                                    , skillStyle = 0, effectDatas = new string[]{ "200", "0.15", "0.2", "2.5", "6" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_13_2_2", skillTier = 1, spriteName = "TAE_13", starCost = 40, needSkillIDs = new string[] { "Skill_TAE_13_2_1" }, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "200", "0.15", "0.2", "3", "6" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_13_2_3", skillTier = 1, spriteName = "TAE_13", starCost = 60, needSkillIDs = new string[] { "Skill_TAE_13_2_2" }, needSkillIDsOperator = 0
                                    , skillStyle = 2, effectDatas = new string[]{ "200", "0.15", "0.2", "3.5", "6" }, subData = _IS_TRIPLE });
        
        // TowerAdditionalEffect_Berserker
        datas.Add(new SkillData() { skillID = "Skill_TAE_13_3_0", skillTier = 1, spriteName = "TAE_13", starCost = 10, needSkillIDs = null, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "200", "0.15", "0.2", "2", "6" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_13_3_1", skillTier = 1, spriteName = "TAE_13", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0
                                    , noNeedSkillIDs = new string[] { "Skill_TAE_13_1_1", "Skill_TAE_13_2_1" }
                                    , skillStyle = 0, effectDatas = new string[]{ "200", "0.15", "0.25", "2", "6" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_13_3_2", skillTier = 1, spriteName = "TAE_13", starCost = 40, needSkillIDs = new string[] { "Skill_TAE_13_3_1" }, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "200", "0.15", "0.3", "2", "6" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_13_3_3", skillTier = 1, spriteName = "TAE_13", starCost = 60, needSkillIDs = new string[] { "Skill_TAE_13_3_2" }, needSkillIDsOperator = 0
                                    , skillStyle = 3, effectDatas = new string[]{ "200", "0.15", "0.3", "2", "6", "2" }, subData = _IS_DOUBLE });

        #endregion

        #region < MultyShot >

        // TowerAdditionalEffect_MultyShot
        datas.Add(new SkillData() { skillID = "Skill_TAE_12_1_0", skillTier = 1, spriteName = "TAE_12", starCost = 0, needSkillIDs = null, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "200", "1", "1", "1", "false" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_12_1_1", skillTier = 1, spriteName = "TAE_12", starCost = 40, needSkillIDs = null, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "200", "1", "2", "2", "false" }, subData = _IS_DOUBLE });
        datas.Add(new SkillData() { skillID = "Skill_TAE_12_1_2", skillTier = 1, spriteName = "TAE_12", starCost = 70, needSkillIDs = new string[] { "Skill_TAE_12_1_1" }, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "200", "1", "2", "5", "false" }, subData = _IS_TRIPLE });

        #endregion

        #region < DoubleShot >

        // TowerAdditionalEffect_DoubleShot
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_14_1_0",
            skillTier = 1,
            spriteName = "TAE_14",
            starCost = 40,
            needSkillIDs = null,
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "300", "1", "1" }
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_14_1_1",
            skillTier = 1,
            spriteName = "TAE_14",
            starCost = 80,
            needSkillIDs = new string[] { "Skill_TAE_14_1_0" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "300", "1", "8" },
            subData = _IS_QUADRUPLE
        });

        #endregion

        #region < Laser >

        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_15_1_0",
            skillTier = 1,
            spriteName = "TAE_15",
            starCost = 20,
            needSkillIDs = null,
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "300", "0.4", "1", "0.6", "0" }
        });

        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_15_1_1",
            skillTier = 1,
            spriteName = "TAE_15",
            starCost = 30,
            needSkillIDs = new string[] { "Skill_TAE_15_1_0" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "300", "0.5", "1", "0.6", "0" }
        });

        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_15_1_2",
            skillTier = 1,
            spriteName = "TAE_15",
            starCost = 40,
            needSkillIDs = new string[] { "Skill_TAE_15_1_1" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "300", "0.6", "1", "0.6", "0" }
        });

        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_15_1_3",
            skillTier = 1,
            spriteName = "TAE_15",
            starCost = 60,
            needSkillIDs = new string[] { "Skill_TAE_15_1_2" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "300", "0.6", "12", "3.0", "1" },
            subData = _IS_TRIPLE
        });

        #endregion

        #region < ChemicalReaction >

        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_20_1_0",
            skillTier = 1,
            spriteName = "TAE_20",
            starCost = 60,
            needSkillIDs = null,
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "500", "0.3", "0.03", "2", "-0.3", "2", "0.2" }
        });

        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_20_1_1",
            skillTier = 1,
            spriteName = "TAE_20",
            starCost = 70,
            needSkillIDs = new string[] { "Skill_TAE_20_1_0" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "500", "0.325", "0.032", "2.3", "-0.32", "2.5", "0.23" }
        });

        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_20_1_2",
            skillTier = 1,
            spriteName = "TAE_20",
            starCost = 80,
            needSkillIDs = new string[] { "Skill_TAE_20_1_1" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "500", "0.35", "0.034", "2.5", "-0.35", "3", "0.25" }
        });

        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_20_1_3",
            skillTier = 1,
            spriteName = "TAE_20",
            starCost = 90,
            needSkillIDs = new string[] { "Skill_TAE_20_1_2" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "500", "0.375", "0.036", "2.7", "-0.38", "3.5", "0.28" }
        });

        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_20_1_4",
            skillTier = 1,
            spriteName = "TAE_20",
            starCost = 100,
            needSkillIDs = new string[] { "Skill_TAE_20_1_3" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "500", "0.4", "0.038", "3", "-0.4", "4", "0.3" }
        });

        #endregion

        #endregion

        #region < Create 2Tier Skill Datas >

        #region < Penetrate >

        // TowerAdditionalEffect_Penetrate
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_1_1_0",
            skillTier = 2,
            spriteName = "TAE_1",
            starCost = 0,
            needSkillIDs = null,
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "100", "50", "0" }
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_1_1_1",
            skillTier = 2,
            spriteName = "TAE_1",
            starCost = 80,
            needSkillIDs = null,
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "100", "50", "5.0" },
            subData = _IS_TRIPLE
        });

        #endregion

        #region < Explosion >

        // TowerAdditionalEffect_Explosion
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_2_1_0",
            skillTier = 2,
            spriteName = "TAE_2",
            starCost = 0,
            needSkillIDs = null,
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "100", "0.4", "0.25" }
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_2_1_1",
            skillTier = 2,
            spriteName = "TAE_2",
            starCost = 20,
            needSkillIDs = null,
            needSkillIDsOperator = 0,
            noNeedSkillIDs = new string[] { "Skill_TAE_2_2_1" }
                                    ,
            skillStyle = 0,
            effectDatas = new string[] { "100", "0.42", "0.25" }
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_2_1_2",
            skillTier = 2,
            spriteName = "TAE_2",
            starCost = 40,
            needSkillIDs = new string[] { "Skill_TAE_2_1_1" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "100", "0.44", "0.25" }
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_2_1_3",
            skillTier = 2,
            spriteName = "TAE_2",
            starCost = 60,
            needSkillIDs = new string[] { "Skill_TAE_2_1_2" },
            needSkillIDsOperator = 0,
            skillStyle = 2,
            effectDatas = new string[] { "100", "0.46", "0.25", "0.50", "0.50" },
            subData = _IS_DOUBLE
        });

        // TowerAdditionalEffect_Explosion_S1
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_2_2_0",
            skillTier = 2,
            spriteName = "TAE_2",
            starCost = 20,
            needSkillIDs = null,
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "100", "0.4", "0.25" }
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_2_2_1",
            skillTier = 2,
            spriteName = "TAE_2",
            starCost = 20,
            needSkillIDs = null,
            needSkillIDsOperator = 0,
            noNeedSkillIDs = new string[] { "Skill_TAE_2_1_1" },
            skillStyle = 0,
            effectDatas = new string[] { "100", "0.4", "0.25" }
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_2_2_2",
            skillTier = 2,
            spriteName = "TAE_2",
            starCost = 30,
            needSkillIDs = new string[] { "Skill_TAE_2_2_1" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "100", "0.4", "0.31" }
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_2_2_3",
            skillTier = 2,
            spriteName = "TAE_2",
            starCost = 40,
            needSkillIDs = new string[] { "Skill_TAE_2_2_2" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "100", "0.4", "0.38" }
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_2_2_4",
            skillTier = 2,
            spriteName = "TAE_2",
            starCost = 50,
            needSkillIDs = new string[] { "Skill_TAE_2_2_3" },
            needSkillIDsOperator = 0,
            skillStyle = 1,
            effectDatas = new string[] { "100", "0.4", "0.44", "2", "8" },
            subData = _IS_TRIPLE
        });

        #endregion

        #region < Freezing >

        // TowerAdditionalEffect_Freezing
        datas.Add(new SkillData() { skillID = "Skill_TAE_3_1_0", skillTier = 2, spriteName = "TAE_3", starCost = 0, needSkillIDs = null, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "100", "-0.3", "3" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_3_1_1", skillTier = 2, spriteName = "TAE_3", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0
                                    , noNeedSkillIDs = new string[] { "Skill_TAE_3_2_1" }
                                    , skillStyle = 0, effectDatas = new string[]{ "100", "-0.3", "3.75" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_3_1_2", skillTier = 2, spriteName = "TAE_3", starCost = 30, needSkillIDs = new string[] { "Skill_TAE_3_1_1" }, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "100", "-0.3", "4.5" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_3_1_3", skillTier = 2, spriteName = "TAE_3", starCost = 40, needSkillIDs = new string[] { "Skill_TAE_3_1_2" }, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "100", "-0.3", "5.25" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_3_1_4", skillTier = 2, spriteName = "TAE_3", starCost = 50, needSkillIDs = new string[] { "Skill_TAE_3_1_3" }, needSkillIDsOperator = 0
                                    , skillStyle = 1, effectDatas = new string[]{ "100", "-0.3", "5.25", "60", "5" }, subData = _IS_TRIPLE });
        
        // TowerAdditionalEffect_Freezing
        datas.Add(new SkillData() { skillID = "Skill_TAE_3_2_0", skillTier = 2, spriteName = "TAE_3", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "100", "-0.3", "3" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_3_2_1", skillTier = 2, spriteName = "TAE_3", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0
                                    , noNeedSkillIDs = new string[] { "Skill_TAE_3_1_1" }
                                    , skillStyle = 0, effectDatas = new string[]{ "100", "-0.38", "3" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_3_2_2", skillTier = 2, spriteName = "TAE_3", starCost = 40, needSkillIDs = new string[] { "Skill_TAE_3_2_1" }, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "100", "-0.45", "3" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_3_2_3", skillTier = 2, spriteName = "TAE_3", starCost = 60, needSkillIDs = new string[] { "Skill_TAE_3_2_2" }, needSkillIDsOperator = 0
                                    , skillStyle = 2, effectDatas = new string[]{ "100", "-0.45", "3", "5", "2" }, subData = _IS_DOUBLE });

        #endregion

        #region < Poison >

        // TowerAdditionalEffect_Poison
        datas.Add(new SkillData() { skillID = "Skill_TAE_4_1_0", skillTier = 2, spriteName = "TAE_4", starCost = 0, needSkillIDs = null, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "100", "5", "0.5" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_4_1_1", skillTier = 2, spriteName = "TAE_4", starCost = 40, needSkillIDs = null, needSkillIDsOperator = 0
                                    , noNeedSkillIDs = new string[] { "Skill_TAE_4_2_1" }
                                    , skillStyle = 0, effectDatas = new string[]{ "100", "6", "0.5" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_4_1_2", skillTier = 2, spriteName = "TAE_4", starCost = 70, needSkillIDs = new string[] { "Skill_TAE_4_1_1" }, needSkillIDsOperator = 0 
                                    , skillStyle = 1, effectDatas = new string[]{ "100", "6", "0.5", "0.6" }, subData = _IS_TRIPLE });
        
        // TowerAdditionalEffect_Poison
        datas.Add(new SkillData() { skillID = "Skill_TAE_4_2_0", skillTier = 2, spriteName = "TAE_4", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "100", "5", "0.5" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_4_2_1", skillTier = 2, spriteName = "TAE_4", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0
                                    , noNeedSkillIDs = new string[] { "Skill_TAE_4_1_1" }
                                    , skillStyle = 0, effectDatas = new string[]{ "100", "5", "0.63" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_4_2_2", skillTier = 2, spriteName = "TAE_4", starCost = 30, needSkillIDs = new string[] { "Skill_TAE_4_2_1" }, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "100", "5", "0.75" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_4_2_3", skillTier = 2, spriteName = "TAE_4", starCost = 50, needSkillIDs = new string[] { "Skill_TAE_4_2_2" }, needSkillIDsOperator = 0
                                    , skillStyle = 2, effectDatas = new string[]{ "100", "5", "0.75" }, subData = _IS_DOUBLE });

        #endregion

        #region < AttackSpeed >

        // TowerAdditionalEffect_Ability_AttackSpeed
        datas.Add(new SkillData() { skillID = "Skill_TAE_9_1_0", skillTier = 2, spriteName = "TAE_9", starCost = 0, needSkillIDs = null, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "100", "0.5", "0" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_9_1_1", skillTier = 2, spriteName = "TAE_9", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "100", "0.63", "0" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_9_1_2", skillTier = 2, spriteName = "TAE_9", starCost = 30, needSkillIDs = new string[] { "Skill_TAE_9_1_1" }, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "100", "0.75", "0" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_9_1_3", skillTier = 2, spriteName = "TAE_9", starCost = 40, needSkillIDs = new string[] { "Skill_TAE_9_1_2" }, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "100", "0.88", "0" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_9_1_4", skillTier = 2, spriteName = "TAE_9", starCost = 50, needSkillIDs = new string[] { "Skill_TAE_9_1_3" }, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "100", "0.88", "1.00" }, subData = _IS_TRIPLE });

        #endregion

        #region < Crush >

        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_16_1_0",
            skillTier = 2,
            spriteName = "TAE_16",
            starCost = 20,
            needSkillIDs = null,
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "100", "5", "0.2", "0.2" }
        });

        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_16_1_1",
            skillTier = 2,
            spriteName = "TAE_16",
            starCost = 30,
            needSkillIDs = new string[] { "Skill_TAE_16_1_0" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "100", "5", "0.25", "0.25" }
        });

        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_16_1_2",
            skillTier = 2,
            spriteName = "TAE_16",
            starCost = 40,
            needSkillIDs = new string[] { "Skill_TAE_16_1_1" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "100", "5", "0.3", "0.3" }
        });
        
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_16_1_3",
            skillTier = 2,
            spriteName = "TAE_16",
            starCost = 50,
            needSkillIDs = new string[] { "Skill_TAE_16_1_2" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "100", "5", "0.3", "0.6" },
            subData = _IS_TRIPLE
        });

        #endregion

        #region < DeathBlow >

        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_17_1_0",
            skillTier = 2,
            spriteName = "TAE_17",
            starCost = 20,
            needSkillIDs = null,
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "150", "0.05", "4.0" }
        });

        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_17_1_1",
            skillTier = 2,
            spriteName = "TAE_17",
            starCost = 40,
            needSkillIDs = new string[] { "Skill_TAE_17_1_0" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "150", "0.055", "4.0" }
        });

        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_17_1_2",
            skillTier = 2,
            spriteName = "TAE_17",
            starCost = 60,
            needSkillIDs = new string[] { "Skill_TAE_17_1_1" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "150", "0.06", "4.0" }
        });

        #endregion  
        
        #endregion

        #region < Create 3Tier Skill Datas >

        #region < AttackDamage >

        // TowerAdditionalEffect_Ability_AttackDamage
        datas.Add(new SkillData() { skillID = "Skill_TAE_7_1_0", skillTier = 3, spriteName = "TAE_7", starCost = 0, needSkillIDs = null, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "0", "0.4", "0" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_7_1_1", skillTier = 3, spriteName = "TAE_7", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "0", "0.5", "0" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_7_1_2", skillTier = 3, spriteName = "TAE_7", starCost = 30, needSkillIDs = new string[] { "Skill_TAE_7_1_1" }, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "0", "0.6", "0" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_7_1_3", skillTier = 3, spriteName = "TAE_7", starCost = 40, needSkillIDs = new string[] { "Skill_TAE_7_1_2" }, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "0", "0.7", "0" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_7_1_4", skillTier = 3, spriteName = "TAE_7", starCost = 50, needSkillIDs = new string[] { "Skill_TAE_7_1_3" }, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "0", "0.7", "100" }, subData = _IS_TRIPLE });

        #endregion

        #region < AttackRange >

        // TowerAdditionalEffect_Ability_AttackRange
        datas.Add(new SkillData() { skillID = "Skill_TAE_8_1_0", skillTier = 3, spriteName = "TAE_8", starCost = 0, needSkillIDs = null, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "0.30", "0" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_8_1_1", skillTier = 3, spriteName = "TAE_8", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "0.37", "0" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_8_1_2", skillTier = 3, spriteName = "TAE_8", starCost = 40, needSkillIDs = new string[] { "Skill_TAE_8_1_1" }, needSkillIDsOperator = 0 
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "0.45", "0" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_8_1_3", skillTier = 3, spriteName = "TAE_8", starCost = 60, needSkillIDs = new string[] { "Skill_TAE_8_1_2" }, needSkillIDsOperator = 0
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "0.45", "9999999" }, subData = _IS_DOUBLE });

        #endregion

        #region < CriticalPerc >

        // TowerAdditionalEffect_Ability_CriticalPerc
        datas.Add(new SkillData() { skillID = "Skill_TAE_10_1_0", skillTier = 3, spriteName = "TAE_10", starCost = 0, needSkillIDs = null, needSkillIDsOperator = 0  
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "0.3", "0" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_10_1_1", skillTier = 3, spriteName = "TAE_10", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0  
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "0.375", "0" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_10_1_2", skillTier = 3, spriteName = "TAE_10", starCost = 30, needSkillIDs = new string[] { "Skill_TAE_10_1_1" }, needSkillIDsOperator = 0  
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "0.45", "0" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_10_1_3", skillTier = 3, spriteName = "TAE_10", starCost = 40, needSkillIDs = new string[] { "Skill_TAE_10_1_2" }, needSkillIDsOperator = 0  
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "0.525", "0" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_10_1_4", skillTier = 3, spriteName = "TAE_10", starCost = 50, needSkillIDs = new string[] { "Skill_TAE_10_1_3" }, needSkillIDsOperator = 0  
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "0.525", "0.2" }, subData = _IS_TRIPLE });

        #endregion

        #region < CriticalDamage >

        // TowerAdditionalEffect_Ability_CriticalDamage
        datas.Add(new SkillData() { skillID = "Skill_TAE_11_1_0", skillTier = 3, spriteName = "TAE_11", starCost = 0, needSkillIDs = null, needSkillIDsOperator = 0   
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "0.8", "0.8" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_11_1_1", skillTier = 3, spriteName = "TAE_11", starCost = 20, needSkillIDs = null, needSkillIDsOperator = 0   
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "1.0", "1.0" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_11_1_2", skillTier = 3, spriteName = "TAE_11", starCost = 30, needSkillIDs = new string[] { "Skill_TAE_11_1_1" }, needSkillIDsOperator = 0   
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "1.2", "1.2" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_11_1_3", skillTier = 3, spriteName = "TAE_11", starCost = 40, needSkillIDs = new string[] { "Skill_TAE_11_1_2" }, needSkillIDsOperator = 0   
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "1.4", "1.4" } });
        datas.Add(new SkillData() { skillID = "Skill_TAE_11_1_4", skillTier = 3, spriteName = "TAE_11", starCost = 50, needSkillIDs = new string[] { "Skill_TAE_11_1_3" }, needSkillIDsOperator = 0   
                                    , skillStyle = 0, effectDatas = new string[]{ "50", "1.4", "4" }, subData = _IS_TRIPLE });

        #endregion

        #region < GrowingPains >

        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_18_1_0",
            skillTier = 3,
            spriteName = "TAE_18",
            starCost = 20,
            needSkillIDs = null,
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "50", "0.1", "0.1" }
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_18_1_1",
            skillTier = 3,
            spriteName = "TAE_18",
            starCost = 40,
            needSkillIDs = new string[] { "Skill_TAE_18_1_0" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "50", "0.1", "0.125" }
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_18_1_2",
            skillTier = 3,
            spriteName = "TAE_18",
            starCost = 60,
            needSkillIDs = new string[] { "Skill_TAE_18_1_1" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "50", "0.1", "0.15" }
        });

        #endregion

        #region < Insight >

        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_19_1_0",
            skillTier = 3,
            spriteName = "TAE_19",
            starCost = 20,
            needSkillIDs = null,
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "50", "0.8", "0" }
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_19_1_1",
            skillTier = 3,
            spriteName = "TAE_19",
            starCost = 30,
            needSkillIDs = new string[] { "Skill_TAE_19_1_0" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "50", "1.0", "0" }
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_19_1_2",
            skillTier = 3,
            spriteName = "TAE_19",
            starCost = 40,
            needSkillIDs = new string[] { "Skill_TAE_19_1_1" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "50", "1.2", "0" }
        });
        datas.Add(new SkillData()
        {
            skillID = "Skill_TAE_19_1_3",
            skillTier = 3,
            spriteName = "TAE_19",
            starCost = 50,
            needSkillIDs = new string[] { "Skill_TAE_19_1_2" },
            needSkillIDsOperator = 0,
            skillStyle = 0,
            effectDatas = new string[] { "50", "1.2", "10" },
            subData = _IS_TRIPLE
        });

        #endregion  
        
        #endregion

        foreach (var d in datas)
        {
            skillDatas.Add(d.skillID, d);
        }
    }

    #endregion

    private void LoadSkillMapDatas2()
    {
        //Dictionary<string, SkillData> skillDatas2 = new Dictionary<string, SkillData>();

        CsvHelper_CsvParser.Deserialize<CsvSkillData>("SkillData", (datas) =>
        {
            foreach(var d in datas)
            {
                skillDatas.Add(d.id, new SkillData(d));
            }
        });

        #region < Checking >

        //foreach (var d in skillDatas)
        //{
        //    SkillData nData;
        //    if(skillDatas2.TryGetValue(d.Key, out nData))
        //    {
        //        if(nData.skillTier != d.Value.skillTier)
        //        {
        //            Debug.LogFormat("<color=blue>Skill({0}) skillTier({1}/{2}) miss match 1</color>", d.Key, nData.skillTier, d.Value.skillTier);
        //        }
        //        else if (nData.skillStyle != d.Value.skillStyle)
        //        {
        //            Debug.LogFormat("<color=blue>Skill({0}) skillStyle({1}/{2}) miss match 1</color>", d.Key, nData.skillStyle, d.Value.skillStyle);
        //        }
        //        else if ((!string.IsNullOrEmpty(nData.spriteName) || !string.IsNullOrEmpty(nData.spriteName)) && nData.spriteName != d.Value.spriteName)
        //        {
        //            Debug.LogFormat("<color=blue>Skill({0}) spriteName({1}/{2}) miss match 1</color>", d.Key, nData.spriteName, d.Value.spriteName);
        //        }
        //        else if (nData.starCost != d.Value.starCost)
        //        {
        //            Debug.LogFormat("<color=blue>Skill({0}) starCost({1}/{2}) miss match 1</color>", d.Key, nData.starCost, d.Value.starCost);
        //        }
        //        else if ((!string.IsNullOrEmpty(nData.needStageID) || !string.IsNullOrEmpty(nData.needStageID)) && nData.needStageID != d.Value.needStageID)
        //        {
        //            Debug.LogFormat("<color=blue>Skill({0}) needStageID({1}/{2}) miss match 1</color>", d.Key, nData.needStageID, d.Value.needStageID);
        //        }
        //        else if (nData.needSkillIDsOperator != d.Value.needSkillIDsOperator)
        //        {
        //            Debug.LogFormat("<color=blue>Skill({0}) needSkillIDsOperator({1}/{2}) miss match 1</color>", d.Key, nData.needSkillIDsOperator, d.Value.needSkillIDsOperator);
        //        }
        //        else if ((!string.IsNullOrEmpty(nData.subData) || !string.IsNullOrEmpty(nData.subData)) && nData.subData != d.Value.subData)
        //        {
        //            Debug.LogFormat("<color=blue>Skill({0}) subData({1}/{2}) miss match 1</color>", d.Key, nData.subData, d.Value.subData);
        //        }
        //        else
        //        {
        //            //if (nData.needSkillIDs != null || d.Value.needSkillIDs != null)
        //            //{
        //            //    Debug.LogFormat("<color=blue>Skill({0}) needSkillIDs({1}/{2}) miss match 1</color>", nData.skillID, nData.needSkillIDs, d.Value.needSkillIDs);
        //            //}
        //            //else 
        //            if ((nData.needSkillIDs != null && d.Value.needSkillIDs != null) && nData.needSkillIDs.Length != d.Value.needSkillIDs.Length)
        //            {
        //                System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //                sb.AppendLine("needSkillIDs " + nData.skillID);
        //                foreach (var id in nData.needSkillIDs) sb.Append(id + "/");
        //                sb.Append("\n\r");
        //                foreach (var id in d.Value.needSkillIDs) sb.Append(id + "/");
        //                Debug.LogFormat("<color=blue>{0}</color>", sb.ToString());
        //            }
        //            //else if (nData.noNeedSkillIDs != null || d.Value.noNeedSkillIDs != null)
        //            //{
        //            //    Debug.LogFormat("<color=blue>Skill({0}) noNeedSkillIDs({1}/{2}) miss match 1</color>", nData.skillID, nData.noNeedSkillIDs, d.Value.noNeedSkillIDs);
        //            //}
        //            else if ((nData.noNeedSkillIDs != null && d.Value.noNeedSkillIDs != null) && nData.noNeedSkillIDs.Length != d.Value.noNeedSkillIDs.Length)
        //            {
        //                System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //                sb.AppendLine("noNeedSkillIDs " + nData.skillID);
        //                foreach (var id in nData.noNeedSkillIDs) sb.Append(id + "/");
        //                sb.Append("\n\r");
        //                foreach (var id in d.Value.noNeedSkillIDs) sb.Append(id + "/");
        //                Debug.LogFormat("<color=blue>{0}</color>", sb.ToString());
        //            }
        //            //else if (nData.effectDatas != null || d.Value.effectDatas != null)
        //            //{
        //            //    Debug.LogFormat("<color=blue>Skill({0}) effectDatas({1}/{2}) miss match 1</color>", nData.skillID, nData.effectDatas, d.Value.effectDatas);
        //            //}
        //            else if ((nData.effectDatas != null && d.Value.effectDatas != null) && nData.effectDatas.Length != d.Value.effectDatas.Length)
        //            {
        //                System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //                sb.AppendLine("effectDatas " + nData.skillID);
        //                foreach (var id in nData.effectDatas) sb.Append(id + "/");
        //                sb.Append("\n\r");
        //                foreach (var id in d.Value.effectDatas) sb.Append(id + "/");
        //                Debug.LogFormat("<color=blue>{0}</color>", sb.ToString());
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogFormat("<color=red>Skill({0}) Not Found</color>", d.Key);
        //    }
        //}
        
        #endregion
    }

    #endregion

    public bool TryStageData(int stageIdx, out StageData stageData)
    {
        if(stageIdx >= 0 && stageDatas.Count > stageIdx)
        {
            stageData = this.stageDatas[stageIdx];
            return true;
        }

        stageData = new StageData();
        return false;
    }
    public int GetStageCount() { return stageDatas.Count; }

    public bool TryGetTowerData(string towerKey, out TowerData towerData, int skillLv = 0)
    {
        if(skillLv == 0)
        {
            return towerDatas.TryGetValue(towerKey, out towerData);
        }
        else
        {
            return towerDatas.TryGetValue(string.Format("{0}-{1}", towerKey, skillLv), out towerData);
        }
    }
    public int GetTowerDataCount() { return towerDatas.Count; }

    public bool TryGetSkillData(string skillID, out SkillData skillData)
    {
        return skillDatas.TryGetValue(skillID, out skillData);
    }
    public int GetSkillDataCount() { return skillDatas.Count; }

    public bool TryGetTowerAdditionalEffect(string skillID, out TowerAdditionalEffect tae)
    {
        return towerAdditionalEffects.TryGetValue(skillID, out tae);
    }

    public void Release()
    {
        towerAdditionalEffects?.Clear();
        towerAdditionalEffects = null;

        prefabKeysByType?.Clear();
        prefabKeysByType = null;

        stageMapDatas?.Clear();
        stageMapDatas = null;
        
        skillDatas?.Clear();
        skillDatas = null;

        stageDatas?.Clear();
        stageDatas = null;

        ingameObjectDatas?.Clear();
        ingameObjectDatas = null;

        towerDatas?.Clear();
        towerDatas = null;

        stageIdxById?.Clear();
        stageIdxById = null;

        ingameObjectSkills?.Clear();
    }

    public string GetLog()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine("#### Data Manager Log ####");
        sb.AppendLine("## Tower Datas ##");
        foreach(var pair in towerDatas)
        {
            sb.AppendLine(pair.Value.GetLog());
        }

        sb.AppendLine("## Skill Datas ##");
        foreach(var d in skillDatas)
        {
            sb.AppendLine(string.Format("# ID({0}/{1}/{2}) Cost({3}) SubData({4})", d.Value.skillID, d.Value.skillTier, d.Value.skillStyle, d.Value.starCost, d.Value.subData));
        }

        return sb.ToString();
    }
}