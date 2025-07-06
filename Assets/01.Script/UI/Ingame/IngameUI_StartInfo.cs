using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngameUI_StartInfo : UIBase
{
    [SerializeField]
    private IngameUI_StartInfo_EnemyInfo enemyInfoPrefab;

    [SerializeField]
    private RectTransform contentRtf;

    private UIItemCreator<IngameUI_StartInfo_EnemyInfo> itemCreator;

    public int curWave { get; private set; }


    public override void Open()
    {
        if (itemCreator == null)
        {
            itemCreator = new UIItemCreator<IngameUI_StartInfo_EnemyInfo>();
            itemCreator.SetData(contentRtf, enemyInfoPrefab);
        }

        base.Open();
    }

    public void SetData(int curWave)
    {
        this.curWave = curWave;
    }

    public void UpdateUI()
    {
        IngameWaveInfo waveInfo;
        if (IngameManager.Instance.waveInfos.TryGetValue(curWave, out waveInfo))
        {
            itemCreator.UpdateItems(waveInfo.spawnInfos.Count, (idx, item) =>
            {
                var info = waveInfo.spawnInfos[idx];
                item.SetData(this, info);
                item.Open();
                item.UpdateUI();
            });
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogErrorFormat("WaveInfo({0}) not found.", curWave);
#endif
        }
    }

    private void Update()
    {
        if(gameObject.activeInHierarchy)
        {
            UpdateUI();
        }
    }
}
