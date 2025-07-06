using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public struct IntVector2 : IEquatable<IntVector2>
{
    public static IntVector2 zero = new IntVector2(0, 0);

    public IntVector2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int x;
    public int y;

    public static IntVector2 operator +(IntVector2 a, IntVector2 b)
    {
        return new IntVector2(a.x + b.x, a.y + b.y);
    }

    public static bool operator ==(IntVector2 a, IntVector2 b)
    {
        return (a.x == b.x) && (a.y == b.y);
    }

    public static bool operator !=(IntVector2 a, IntVector2 b)
    {
        return !((a.x == b.x) && (a.y == b.y));
    }

    public override bool Equals(object b)
    {
        return Equals((IntVector2)b);
    }

    public bool Equals(IntVector2 b)
    {
        return (x == b.x) && (y == b.y);
    }
}

public class WayInfo
{
    public WayInfo() { }
    public WayInfo(GridInfo wayPointGridInfo, GridInfo wallGridInfo = null, GridInfo wallNxtGridInfo = null)
    {
        this.wayPointGridInfo = wayPointGridInfo;
        this.wallGridInfo = wallGridInfo;
        this.wallNxtGridInfo = wallNxtGridInfo;
    }

    public GridInfo wayPointGridInfo;

    public GridInfo wallGridInfo;

    public int wallNxtWayPointIdx;
    public GridInfo wallNxtGridInfo;
}

public class GridInfo
{
    public GridInfo(IntVector2 gridPos, GridBlock gridBlock, GridBlock skyGridBlock = null)
    {
        this.gridPos = gridPos;
        this.gridBlock = gridBlock;
        this.skyGridBlock = skyGridBlock;
    }

    public IntVector2 gridPos;
    public GridBlock gridBlock;

    #region < Groud Way >

    public GridInfo preWayGridInfo;
    public GridInfo nxtWayGridInfo;

    public bool hasNxtWallGridInfo { get { return nxtWallGridInfo != null; } }
    public GridInfo nxtWallGridInfo;

    public bool TryGetNxtWayGridInfo(out GridInfo nxtGridInfo)
    {
        if(nxtWallGridInfo != null && (nxtWallGridInfo.gridBlock as GridBlock_Wall).isWallOpened)
        {
            nxtGridInfo = nxtWallGridInfo;
        }
        else
        {
            nxtGridInfo = nxtWayGridInfo;
        }
        return nxtGridInfo != null;
    }

    #endregion

    public bool hasSkyGridBlock { get { return skyGridBlock != null; } }
    public GridBlock skyGridBlock;

    #region < Sky Way >

    public GridInfo preSkyWayGridInfo;
    public GridInfo nxtSkyWayGridInfo;

    public bool TryGetSkyWayNxtGridInfo(out GridInfo nxtGridInfo)
    {
        nxtGridInfo = nxtSkyWayGridInfo;
        return nxtGridInfo != null;
    }

    #endregion

    public void Clear()
    {
        gridBlock?.Release();
        skyGridBlock?.Release();

        preWayGridInfo = null;
        nxtWayGridInfo = null;
        nxtWallGridInfo = null;

        preSkyWayGridInfo = null;
        nxtSkyWayGridInfo = null;
    }
}

public class GridMap
{
    public GridMap()
    {
        m_GridInfoMap = new Dictionary<int, List<GridInfo>>();
        m_GridInfoByType = new Dictionary<GridBlock.EType, List<GridInfo>>();
    }

    #region < Prop - WayFinder >

    public bool isShowingWayFinder { get; private set; }
    public float wayFinderDelayTimer = 0;
    public float wayFinderDelay = 2f;
    public IntVector2 curWayPointGridPos;
    private Fx_Switch m_FxWayFinder;
    public Fx_Switch fxWayFinder { get { return m_FxWayFinder; } private set { m_FxWayFinder = value; } }

    public float skyWayFinderDelayTimer = 0;
    public float skyWayFinderDelay = 2f;
    public IntVector2 curSkyWayPointGridPos;
    private Fx_Switch m_FxSkyWayFinder;
    public Fx_Switch fxSkyWayFinder { get { return m_FxSkyWayFinder; } private set { m_FxSkyWayFinder = value; } }

    #endregion

    public bool hasSkyWayPoint { get; private set; }

    private Dictionary<int, List<GridInfo>> m_GridInfoMap;
    public Dictionary<int, List<GridInfo>> gridInfoMap { get { return m_GridInfoMap; } private set { m_GridInfoMap = value; } }

    private Dictionary<GridBlock.EType, List<GridInfo>> m_GridInfoByType;
    public Dictionary<GridBlock.EType, List<GridInfo>> gridInfoByType { get { return m_GridInfoByType; } private set { m_GridInfoByType = value; } }

    public GridInfo startGridInfo { get; private set; }
    public GridInfo endGridInfo { get; private set; }

    private Vector3 m_GridCenterPos;
    public Vector3 gridCenterPos { get { return m_GridCenterPos; } private set { m_GridCenterPos = value; } }

    private float m_GridWidth;
    public float gridWidth { get { return m_GridWidth; } private set { m_GridWidth = value; } }

    private float m_GridHeight;
    public float gridHeight { get { return m_GridHeight; } private set { m_GridHeight = value; } }

    public int xLength { get; private set; }
    public int yLength { get; private set; }

    public bool TryGetGridInfo(IntVector2 gridPos, out GridInfo gridInfo)
    {
        List<GridInfo> infoList;
        if (gridInfoMap.TryGetValue(gridPos.y, out infoList))
        {
            if (gridPos.x >= 0 && gridPos.x < infoList.Count)
            {
                gridInfo = infoList[gridPos.x];
                return true;
            }
        }
        gridInfo = null;
        return false;
    }
    
    public bool TryFindGridInfo(Func<GridInfo, bool> findCondition, out GridInfo gridInfo)
    {
        foreach(var pair in gridInfoMap)
        {
            foreach(var info in pair.Value)
            {
                if(findCondition(info))
                {
                    gridInfo = info;
                    return true;
                }
            }
        }

        gridInfo = null;
        return false;
    }
    
    public void Claer()
    {
        foreach (var list in gridInfoMap)
        {
            foreach (var info in list.Value)
            {
                info.Clear();
            }
        }
        
        if (fxSkyWayFinder != null)
        {
            fxSkyWayFinder.Off();
            fxSkyWayFinder.Destroy();
        }
        fxSkyWayFinder = null;

        if (fxWayFinder != null)
        {
            fxWayFinder.Off();
            fxWayFinder.Destroy();
        }
        fxWayFinder = null;

        hasSkyWayPoint = false;

        startGridInfo = null;
        endGridInfo = null;
        
        gridInfoMap.Clear();
        gridInfoByType.Clear();

        isShowingWayFinder = false;
    }

    /// <summary>
    /// xLength는 무조건 홀수여야함.
    /// 짝수인 경우 처리를 해두지 않음.
    /// </summary>
    public void Create(string mapData, string skyMapData, Vector3 gridCenterPos, float tileSizeX, float tileSizeY, WallData wallData)
    {
        this.gridCenterPos = gridCenterPos;

        // 마지막 문자가 /로 끝나면 해당 문자를 지운다.
        if (mapData[mapData.Length - 1].Equals('/'))
        {
            mapData = mapData.Remove(mapData.Length - 1);
        }
        
        string[] rows = mapData.Split('/');
        yLength = rows.Length;
        xLength = 0;

        List<string> gridDatas = new List<string>();
        foreach(var row in rows)
        {
            var grids = row.Split('_');

            if(xLength == 0)
            {
                xLength = grids.Length;
            }

            foreach (var grid in grids)
            {
                gridDatas.Add(grid);
            }
        }

        gridWidth = xLength * tileSizeX;
        gridHeight = yLength * tileSizeY;

        List<string> skyGridDatas = new List<string>();
        if(!string.IsNullOrEmpty(skyMapData))
        {
            // 마지막 문자가 /로 끝나면 해당 문자를 지운다.
            if (skyMapData[skyMapData.Length - 1].Equals('/'))
            {
                skyMapData = skyMapData.Remove(skyMapData.Length - 1);
            }

            foreach (var row in skyMapData.Split('/'))
            {
                foreach (var grid in row.Split('_'))
                {
                    skyGridDatas.Add(grid);
                }
            }
            hasSkyWayPoint = true;
        }

        float spaceWorldPosX = gridWidth / xLength;
        float spaceWorldPosY = gridHeight / yLength;

        Vector3 startWorldPosLeftBottom 
            = new Vector3(gridCenterPos.x - spaceWorldPosX * (xLength / 2), gridCenterPos.y - spaceWorldPosY * (yLength / 2), gridCenterPos.z)
            + new Vector3
                ( (xLength % 2 == 0) ? spaceWorldPosX / 2 : 0
                , (yLength % 2 == 0) ? spaceWorldPosY / 2 : 0
                , 0);

        #region < Map Data Proc >

        for (int idx = 0; idx < gridDatas.Count; ++idx)
        {
            int curGridTypeInt = -1;
            IntVector2 gridPos = IntVector2.zero;
            if (!Helper.Grid.TryConvertIndexToGridPos(gridDatas.Count, idx, xLength, out gridPos))
            {
                Debug.LogErrorFormat("GridIndex({0}, {1}, {2}) can't convert to GridPos", gridDatas.Count, idx, xLength);
            }
            else if (!int.TryParse(gridDatas[idx], out curGridTypeInt))
            {
                Debug.LogErrorFormat("GridType({0}. {1}) can't parsed to int", idx, gridDatas[idx]);
            }

            var curGridWorldPos = startWorldPosLeftBottom + new Vector3(spaceWorldPosX * gridPos.x, spaceWorldPosY * gridPos.y, 0);

            GridInfo gridInfo = null;
            GridBlock gridBlock = null;
            var gridType = (GridBlock.EIntType)curGridTypeInt;
            switch (gridType)
            {
                default:
                    Debug.LogErrorFormat("GridType({0}) is not implemented.", gridType);
                    if (SpawnMaster.TrySpawnGridBlock("IGB", Vector3.zero, Quaternion.identity, out gridBlock))
                    {
                        gridBlock.SetData(gridPos);
                        gridBlock.transform.position = new Vector3(-100, -100);
                        gridInfo = new GridInfo(gridPos, gridBlock);
                    }
                    else Debug.LogError("IGB Can't spawn.");
                    break;

                case GridBlock.EIntType.None:
                    if (SpawnMaster.TrySpawnGridBlock("IGB", Vector3.zero, Quaternion.identity, out gridBlock))
                    {
                        gridBlock.SetData(gridPos);
                        gridBlock.transform.position = curGridWorldPos;
                        gridInfo = new GridInfo(gridPos, gridBlock);
                    }
                    else Debug.LogError("IGB Can't spawn.");
                    break;

                case GridBlock.EIntType.Effect:
                    if (SpawnMaster.TrySpawnGridBlock("IGB", Vector3.zero, Quaternion.identity, out gridBlock))
                    {
                        gridBlock.SetData(gridPos, 1);
                        gridBlock.transform.position = curGridWorldPos;
                        gridInfo = new GridInfo(gridPos, gridBlock);
                    }
                    else Debug.LogError("IGB Can't spawn.");
                    break;

                case GridBlock.EIntType.Ground:
                    GridBlock_Ground gridBlockGround;
                    if (SpawnMaster.TrySpawnGridBlock("IGBGround", Vector3.zero, Quaternion.identity, out gridBlockGround))
                    {
                        gridBlockGround.SetData(gridPos);
                        gridBlockGround.transform.position = curGridWorldPos;
                        gridBlock = gridBlockGround;
                        gridInfo = new GridInfo(gridPos, gridBlock);
                    }
                    else Debug.LogError("IGBGround Can't spawn.");
                    break;

                case GridBlock.EIntType.WayPoint:
                    GridBlock_WayPoint gridBlockWayPoint;
                    if (SpawnMaster.TrySpawnGridBlock("IGBWayPoint", Vector3.zero, Quaternion.identity, out gridBlockWayPoint))
                    {
                        gridBlockWayPoint.SetData(gridPos);
                        gridBlockWayPoint.transform.position = curGridWorldPos;
                        gridBlock = gridBlockWayPoint;
                        gridInfo = new GridInfo(gridPos, gridBlock);
                    }
                    else Debug.LogError("IGBWayPoint Can't spawn.");
                    break;

                case GridBlock.EIntType.StartPoint:
                    GridBlock_StartPoint gridBlockStartPoint;
                    if (SpawnMaster.TrySpawnGridBlock("IGBStartPoint", Vector3.zero, Quaternion.identity, out gridBlockStartPoint))
                    {
                        gridBlockStartPoint.SetData(gridPos);
                        gridBlockStartPoint.transform.position = curGridWorldPos;
                        gridBlock = gridBlockStartPoint;
                        startGridInfo = gridInfo = new GridInfo(gridPos, gridBlock);
                    }
                    else Debug.LogError("IGBStartPoint Can't spawn.");
                    break;

                case GridBlock.EIntType.EndPoint:
                    GridBlock_EndPoint gridBlockEndPoint;
                    if (SpawnMaster.TrySpawnGridBlock("IGBEndPoint", Vector3.zero, Quaternion.identity, out gridBlockEndPoint))
                    {
                        gridBlockEndPoint.SetData(gridPos);
                        gridBlockEndPoint.transform.position = curGridWorldPos;
                        gridBlock = gridBlockEndPoint;
                        endGridInfo = gridInfo = new GridInfo(gridPos, gridBlock);
                    }
                    else Debug.LogError("IGBEndPoint Can't spawn.");
                    break;

                case GridBlock.EIntType.Wall:
                    GridBlock_Wall gridBlockWall;
                    if (SpawnMaster.TrySpawnGridBlock("IGBWall", Vector3.zero, Quaternion.identity, out gridBlockWall))
                    {
                        gridBlockWall.SetData(gridPos);
                        gridBlockWall.SetWallData(wallData);
                        gridBlockWall.transform.position = curGridWorldPos;
                        gridBlock = gridBlockWall;
                        gridInfo = new GridInfo(gridPos, gridBlock);
                    }
                    else Debug.LogError("IGBWall Can't spawn.");
                    break;

                case GridBlock.EIntType.SkyWayPoint:
                    Debug.LogErrorFormat("GridType({0}) can't used.", gridType);
                    break;
            }

            List<GridInfo> infoList;
            if (!gridInfoMap.TryGetValue(gridPos.y, out infoList))
            {
                infoList = new List<GridInfo>();
                gridInfoMap.Add(gridPos.y, infoList);
            }
            infoList.Add(gridInfo);

            List<GridInfo> infoList2;
            if (!gridInfoByType.TryGetValue(gridBlock.gridBlockType, out infoList2))
            {
                infoList2 = new List<GridInfo>();
                gridInfoByType.Add(gridBlock.gridBlockType, infoList2);
            }
            infoList2.Add(gridInfo);
        }

        #endregion

        #region < Sky Map Data Proc >

        if (skyGridDatas != null)
        {
            for (int idx = 0; idx < skyGridDatas.Count; ++idx)
            {
                int curGridTypeInt = -1;
                IntVector2 gridPos = IntVector2.zero;
                GridInfo gridInfo;

                if (!Helper.Grid.TryConvertIndexToGridPos(skyGridDatas.Count, idx, xLength, out gridPos))
                {
                    Debug.LogErrorFormat("GridIndex({0}, {1}, {2}) can't convert to GridPos", skyGridDatas.Count, idx, xLength);
                    continue;
                }
                else if (!int.TryParse(skyGridDatas[idx], out curGridTypeInt))
                {
                    Debug.LogErrorFormat("GridType({0}. {1}) can't parsed to int", idx, skyGridDatas[idx]);
                    continue;
                }
                else if(!TryGetGridInfo(gridPos, out gridInfo))
                {
                    Debug.LogErrorFormat("GridInfo({0}, {1}) not found.", gridPos.x, gridPos.y);
                    continue;
                }

                var curGridWorldPos = startWorldPosLeftBottom + new Vector3(spaceWorldPosX * gridPos.x, spaceWorldPosY * gridPos.y, 0.5f);
                
                var gridType = (GridBlock.EIntType)curGridTypeInt;
                switch (gridType)
                {
                    default:
                        Debug.LogErrorFormat("GridType({0}) is not implemented.", gridType);
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPaused = true;
#endif
                        break;

                    case GridBlock.EIntType.None:
                    case GridBlock.EIntType.StartPoint:
                    case GridBlock.EIntType.EndPoint:
                        break;

                    case GridBlock.EIntType.Ground:
                    case GridBlock.EIntType.WayPoint:
                    case GridBlock.EIntType.Wall:
                        Debug.LogErrorFormat("GridType({0}) can't used.", gridType);
                        break;

                    case GridBlock.EIntType.SkyWayPoint:
                        GridBlock_SkyWayPoint gridBlockSkyWayPoint;
                        if (SpawnMaster.TrySpawnGridBlock("IGBSkyWayPoint", Vector3.zero, Quaternion.identity, out gridBlockSkyWayPoint))
                        {
                            gridBlockSkyWayPoint.SetData(gridPos);
                            gridBlockSkyWayPoint.transform.position = curGridWorldPos;
                            gridInfo.skyGridBlock = gridBlockSkyWayPoint;
                        }
                        else Debug.LogError("IGBSkyWayPoint Can't spawn.");
                        break;
                }
            }
        }

        #endregion

        UpdateWayGridInfo();

        UpdateWayGridDir();

#if UNITY_EDITOR
        Debug.Log(GetLog());
#endif
    }

    private void UpdateWayGridInfo()
    {
        // 상하좌우 검색 순서
        IntVector2[] findGridPosList = new IntVector2[] { new IntVector2(0, 1), new IntVector2(-1, 0), new IntVector2(1, 0), new IntVector2(0, -1) };

        GridInfo preGridInfo = null;
        GridInfo curGridInfo = startGridInfo;
        GridInfo nxtGridInfo = null;
        GridInfo wallGridInfo = null;

        int tryCount = 0;

        #region < Regist Way GridInfos >

        while(!curGridInfo.gridBlock.EqualGridType(GridBlock.EType.EndPoint))
        {
            // 상하좌우에 있는 그리드를 순회하면서 다음 WayPoint 또는 EndPoint를 찾는다.
            foreach (var pos in findGridPosList)
            {
                var nxtGridPos = curGridInfo.gridPos + pos;

                if (AppManager.Instance.gridMap.TryGetGridInfo(nxtGridPos, out nxtGridInfo))
                {
                    if(nxtGridInfo.gridBlock.IncludedGridType(GridBlock.EType.WayPoint | GridBlock.EType.EndPoint)
                        && ((preGridInfo != null && preGridInfo.gridPos != nxtGridInfo.gridPos) || preGridInfo == null))
                    {
                        curGridInfo.preWayGridInfo = preGridInfo;
                        curGridInfo.nxtWayGridInfo = nxtGridInfo;

                        preGridInfo = curGridInfo;
                        curGridInfo = nxtGridInfo;
                        break;
                    }
                    else if(nxtGridInfo.gridBlock.IncludedGridType(GridBlock.EType.EndPoint))
                    {
                        curGridInfo.preWayGridInfo = preGridInfo;
                        curGridInfo.nxtWayGridInfo = nxtGridInfo;

                        nxtGridInfo.preWayGridInfo = curGridInfo;

                        preGridInfo = curGridInfo;
                        curGridInfo = nxtGridInfo;
                        break;
                    }
                }
            }

            // 무한루프 돌지 않도록 방지
            if(++tryCount >= 1000)
            {
                Debug.LogError("Failure Create WayPointMap !!");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPaused = true;
#endif
                break;
            }
        }

        #endregion

        #region < Regist Wall GridInfos >

        tryCount = 0;
        preGridInfo = null;
        curGridInfo = startGridInfo;
        nxtGridInfo = null;
        wallGridInfo = null;

        // StartPoint부터 EndPoint까지 WayPoint를 순회하면서 주변에 있는 Wall를 찾아서 등록한다.
        while (curGridInfo.TryGetNxtWayGridInfo(out nxtGridInfo))
        {
            // 상하좌우에 있는 그리드를 순회하면서 다음 WayPoint 또는 EndPoint를 찾는다.
            foreach (var pos in findGridPosList)
            {
                var nxtGridPos = curGridInfo.gridPos + pos;

                if (AppManager.Instance.gridMap.TryGetGridInfo(nxtGridPos, out wallGridInfo)
                    && wallGridInfo.gridBlock.IncludedGridType(GridBlock.EType.Wall))
                {
                    if(wallGridInfo.preWayGridInfo == null)
                    {
                        wallGridInfo.preWayGridInfo = curGridInfo;
                        curGridInfo.nxtWallGridInfo = wallGridInfo;
                    }
                    else if(wallGridInfo.nxtWayGridInfo == null)
                    {
                        wallGridInfo.nxtWayGridInfo = curGridInfo;
                    }
                }
            }
            curGridInfo = nxtGridInfo;

            // 무한루프 돌지 않도록 방지
            if (++tryCount >= 1000)
            {
                Debug.LogError("Failure Create WayPointMap !!");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPaused = true;
#endif
                break;
            }
        }

        #endregion

        #region < Regist SkyWay GridInfos >

        tryCount = 0;
        preGridInfo = null;
        curGridInfo = startGridInfo;
        nxtGridInfo = null;
        wallGridInfo = null;

        while (hasSkyWayPoint && !curGridInfo.gridBlock.EqualGridType(GridBlock.EType.EndPoint))
        {
            // 상하좌우에 있는 그리드를 순회하면서 다음 WayPoint 또는 EndPoint를 찾는다.
            foreach (var pos in findGridPosList)
            {
                var nxtGridPos = curGridInfo.gridPos + pos;

                if (AppManager.Instance.gridMap.TryGetGridInfo(nxtGridPos, out nxtGridInfo))
                {
                    if(nxtGridInfo.hasSkyGridBlock 
                       && nxtGridInfo.skyGridBlock.IncludedGridType(GridBlock.EType.SkyWayPoint)
                       && ((preGridInfo != null && preGridInfo.gridPos != nxtGridInfo.gridPos) || preGridInfo == null))
                    {
                        curGridInfo.preSkyWayGridInfo = preGridInfo;
                        curGridInfo.nxtSkyWayGridInfo = nxtGridInfo;

                        preGridInfo = curGridInfo;
                        curGridInfo = nxtGridInfo;
                        break;
                    }
                    else if(nxtGridInfo.gridBlock.IncludedGridType(GridBlock.EType.EndPoint))
                    {
                        curGridInfo.preSkyWayGridInfo = preGridInfo;
                        curGridInfo.nxtSkyWayGridInfo = nxtGridInfo;

                        nxtGridInfo.preSkyWayGridInfo = curGridInfo;

                        preGridInfo = curGridInfo;
                        curGridInfo = nxtGridInfo;
                        break;
                    }
                }
            }

            // 무한루프 돌지 않도록 방지
            if (++tryCount >= 1000)
            {
                Debug.LogError("Failure Create WayPointMap !!");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPaused = true;
#endif
                break;
            }
        }

        #endregion
    }

    private void UpdateWayGridDir()
    {
        IntVector2 pre2CurDir, cur2NxtDir;

        foreach (var infos in gridInfoMap)
        {
            foreach(var info in infos.Value)
            {
                if (info.gridBlock.EqualGridType(GridBlock.EType.WayPoint)
                    && TryGetNxtWayPointGridPosDir(info.preWayGridInfo, info, out pre2CurDir)
                    && TryGetNxtWayPointGridPosDir(info, info.nxtWayGridInfo, out cur2NxtDir))
                {
                    var wayPoint = info.gridBlock as GridBlock_WayPoint;
                    wayPoint.UpdateWayPointDir(new Vector2(pre2CurDir.x, pre2CurDir.y), new Vector2(cur2NxtDir.x, cur2NxtDir.y));
                }
                else if(info.gridBlock.EqualGridType(GridBlock.EType.Wall)
                    && TryGetNxtWayPointGridPosDir(info.preWayGridInfo, info, out pre2CurDir)
                    && TryGetNxtWayPointGridPosDir(info, info.nxtWayGridInfo, out cur2NxtDir))
                {
                    var wall = info.gridBlock as GridBlock_Wall;
                    wall.UpdateWayPointDir(new Vector2(pre2CurDir.x, pre2CurDir.y), new Vector2(cur2NxtDir.x, cur2NxtDir.y));
                }
                
                if (info.hasSkyGridBlock
                    && TryGetNxtWayPointGridPosDir(info.preSkyWayGridInfo, info, out pre2CurDir)
                    && TryGetNxtWayPointGridPosDir(info, info.nxtSkyWayGridInfo, out cur2NxtDir)
                    && info.skyGridBlock.EqualGridType(GridBlock.EType.SkyWayPoint))
                {
                    var wayPoint = info.skyGridBlock as GridBlock_SkyWayPoint;
                    wayPoint.UpdateWayPointDir(new Vector2(pre2CurDir.x, pre2CurDir.y), new Vector2(cur2NxtDir.x, cur2NxtDir.y));
                }
            }
        }
    }

    #region < Way Finder >

    public void ShowWayFinderFx(float repeatTime = 2f)
    {
        wayFinderDelay = wayFinderDelayTimer = repeatTime;
        skyWayFinderDelay = skyWayFinderDelayTimer = repeatTime;

        if (isShowingWayFinder) return;

        if (fxWayFinder == null)
        {
            if(!SpawnMaster.TrySpawnFx("Fx_WayFinder", startGridInfo.gridBlock.transform.position, Quaternion.identity, out m_FxWayFinder))
            {
                
            }
        }

        if (hasSkyWayPoint && fxSkyWayFinder == null)
        {
            if (!SpawnMaster.TrySpawnFx("Fx_WayFinder", startGridInfo.gridBlock.transform.position, Quaternion.identity, out m_FxSkyWayFinder))
            {

            }
        }

        isShowingWayFinder = true;
    }

    private bool FindNextWayPoint(bool isSkyWayFind, out Vector3 dir)
    {
        GridInfo curGridInfo, nxtGridInfo;

        if(!isSkyWayFind)
        {
            if (TryGetGridInfo(curWayPointGridPos, out curGridInfo)
                && curGridInfo.TryGetNxtWayGridInfo(out nxtGridInfo))
            {
                var nxtWayPoint = nxtGridInfo.gridBlock;

                // 다음 WayPoint에 도달하지 못한 경우
                var dis = Vector3.Distance(fxWayFinder.transform.position, nxtWayPoint.transform.position);
                if (dis > 0.02f)
                {
                    dir = (nxtWayPoint.transform.position - fxWayFinder.transform.position).normalized;
                    dir = new Vector3(dir.x, dir.y, 0);
                    return true;
                }
                // EndPoint에 도달한 경우
                else if (nxtWayPoint.EqualGridType(GridBlock.EType.EndPoint))
                {
                    fxWayFinder.Off();
                    wayFinderDelayTimer = wayFinderDelay;
                    dir = Vector3.zero;
                    return true;
                }
                // EndPoint에 도달하지 못한 경우
                else
                {
                    curWayPointGridPos = nxtGridInfo.gridPos;
                    curGridInfo = nxtGridInfo;

                    if (nxtGridInfo.TryGetNxtWayGridInfo(out nxtGridInfo))
                    {
                        nxtWayPoint = nxtGridInfo.gridBlock;
                    }

                    dir = (nxtWayPoint.transform.position - fxWayFinder.transform.position).normalized;
                    dir = new Vector3(dir.x, dir.y, 0);
                    return true;
                }
            }
        }
        else
        {
            if (TryGetGridInfo(curSkyWayPointGridPos, out curGridInfo)
                && curGridInfo.TryGetSkyWayNxtGridInfo(out nxtGridInfo))
            {
                var nxtWayPoint = nxtGridInfo.gridBlock;

                // 다음 WayPoint에 도달하지 못한 경우
                var dis = Vector3.Distance(fxSkyWayFinder.transform.position, nxtWayPoint.transform.position);
                if (dis > 0.02f)
                {
                    dir = (nxtWayPoint.transform.position - fxSkyWayFinder.transform.position).normalized;
                    dir = new Vector3(dir.x, dir.y, 0);
                    return true;
                }
                // EndPoint에 도달한 경우
                else if (nxtWayPoint.EqualGridType(GridBlock.EType.EndPoint))
                {
                    fxSkyWayFinder.Off();
                    skyWayFinderDelayTimer = skyWayFinderDelay;
                    dir = Vector3.zero;
                    return true;
                }
                // EndPoint에 도달하지 못한 경우
                else
                {
                    curSkyWayPointGridPos = nxtGridInfo.gridPos;
                    curGridInfo = nxtGridInfo;

                    if (nxtGridInfo.TryGetSkyWayNxtGridInfo(out nxtGridInfo))
                    {
                        nxtWayPoint = nxtGridInfo.gridBlock;
                    }

                    dir = (nxtWayPoint.transform.position - fxSkyWayFinder.transform.position).normalized;
                    dir = new Vector3(dir.x, dir.y, 0);
                    return true;
                }
            }
        }

        dir = Vector3.zero;
        return false;
    }

    public void HideWayFinderFx()
    {
        if(isShowingWayFinder)
        {
            isShowingWayFinder = false;

            if(fxWayFinder != null) fxWayFinder.Off();

            if (fxSkyWayFinder != null) fxSkyWayFinder.Off();
        }
    }

    #endregion  

    public void Update(float deltaTime)
    {
        if(isShowingWayFinder)
        {
            if(wayFinderDelayTimer > 0)
            {
                wayFinderDelayTimer -= deltaTime;
            }
            else if(!fxWayFinder.isPlayingOn)
            {
                curWayPointGridPos = startGridInfo.gridBlock.gridPos;
                fxWayFinder.transform.position = startGridInfo.gridBlock.transform.position;
                fxWayFinder.On();
            }
            else
            {
                Vector3 dir;
                if (FindNextWayPoint(false, out dir))
                {
                    fxWayFinder.transform.position += dir * deltaTime * 0.8f;
                }
            }

            if(hasSkyWayPoint)
            {
                if (skyWayFinderDelayTimer > 0)
                {
                    skyWayFinderDelayTimer -= deltaTime;
                }
                else if (!fxSkyWayFinder.isPlayingOn)
                {
                    curSkyWayPointGridPos = startGridInfo.gridBlock.gridPos;
                    fxSkyWayFinder.transform.position = startGridInfo.gridBlock.transform.position;
                    fxSkyWayFinder.On();
                }
                else
                {
                    Vector3 dir;
                    if (FindNextWayPoint(true, out dir))
                    {
                        fxSkyWayFinder.transform.position += dir * deltaTime * 0.8f;
                    }
                }
            }
        }
    }
    
    private bool TryGetNxtWayPointGridPosDir(GridInfo curGridInfo, GridInfo nxtGridInfo, out IntVector2 gridPosDir)
    {
        if(curGridInfo == null || nxtGridInfo == null)
        {
            gridPosDir = IntVector2.zero;
            return false;
        }

        int dirX = 0, dirY = 0;
        if (curGridInfo.gridPos.x < nxtGridInfo.gridPos.x) dirX = 1;
        else if (curGridInfo.gridPos.x > nxtGridInfo.gridPos.x) dirX = -1;

        if (curGridInfo.gridPos.y < nxtGridInfo.gridPos.y) dirY = 1;
        else if (curGridInfo.gridPos.y > nxtGridInfo.gridPos.y) dirY = -1;

        gridPosDir = new IntVector2(dirX, dirY);
        return true;
    }

    public string GetLog()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine(string.Format("#### GridMap Log ####"));

        sb.AppendLine(string.Format("## GridInfoMap"));
        foreach (var infoList in gridInfoMap)
        {
            foreach (var gridInfo in infoList.Value)
            {
                // 여기서 null reference가 뜨면 GridMap.Create할 때 
                // GridInfo가 null인 상태로 추가된게 아닌지 확인
                sb.AppendLine(string.Format("# GridPos({0}, {1}) GridBlockType({2})", gridInfo.gridPos.x, gridInfo.gridPos.y, gridInfo.gridBlock.gridBlockType));
            }
        }

        //sb.AppendLine(string.Format("## WayPointMap"));
        //foreach(var info in gridWayPointMap)
        //{
        //    var wayPointGridInfo = info.wayPointGridInfo;
        //    if(wayPointGridInfo.gridBlock.EqualGridType(GridBlock.EType.WayPoint))
        //    {
        //        string wallGridInfoLog = string.Empty;
        //        string wallNxtGridInfoLog = string.Empty;
        //        if (info.wallGridInfo != null)
        //        {
        //            wallGridInfoLog = string.Format("WallPos({0}, {1})"
        //                            , info.wallGridInfo.gridPos.x
        //                            , info.wallGridInfo.gridPos.y);
        //        }

        //        if (info.wallNxtGridInfo != null)
        //        {
        //            wallGridInfoLog = string.Format("WallNxtPos({0}, {1})"
        //                            , info.wallNxtGridInfo.gridPos.x
        //                            , info.wallNxtGridInfo.gridPos.y);
        //        }

        //        sb.AppendLine(string.Format("# {0} GridPos({1}, {2}) NxtDir({3}) {4} {5}"
        //                    , wayPointGridInfo.gridBlock.gridBlockType
        //                    , wayPointGridInfo.gridPos.x
        //                    , wayPointGridInfo.gridPos.y
        //                    , (wayPointGridInfo.gridBlock as GridBlock_WayPoint).debugDir
        //                    , wallGridInfoLog
        //                    , wallNxtGridInfoLog));
        //    }
        //    else
        //    {
        //        sb.AppendLine(string.Format("# {0} GridPos({1}, {2})"
        //                    , wayPointGridInfo.gridBlock.gridBlockType
        //                    , wayPointGridInfo.gridPos.x
        //                    , wayPointGridInfo.gridPos.y));
        //    }
        //}
        
        //sb.AppendLine(string.Format("## SkyWayPointMap"));
        //foreach (var info in gridSkyWayPointMap)
        //{
        //    var wayPointGridInfo = info;
        //    if (wayPointGridInfo.gridBlock.EqualGridType(GridBlock.EType.SkyWayPoint))
        //    {
        //        sb.AppendLine(string.Format("# {0} GridPos({1}, {2}) NxtDir({3})"
        //                    , wayPointGridInfo.gridBlock.gridBlockType
        //                    , wayPointGridInfo.gridPos.x
        //                    , wayPointGridInfo.gridPos.y
        //                    , (wayPointGridInfo.gridBlock as GridBlock_SkyWayPoint).debugDir));
        //    }
        //    else
        //    {
        //        sb.AppendLine(string.Format("# {0} GridPos({1}, {2})"
        //                    , wayPointGridInfo.gridBlock.gridBlockType
        //                    , wayPointGridInfo.gridPos.x
        //                    , wayPointGridInfo.gridPos.y));
        //    }
        //}

        sb.AppendLine(string.Format("#############"));
        return sb.ToString();
    }
}
