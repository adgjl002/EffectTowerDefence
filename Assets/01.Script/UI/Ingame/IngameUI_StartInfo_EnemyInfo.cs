using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngameUI_StartInfo_EnemyInfo : UIBase
{
    [SerializeField]
    private Image m_EnemyImg;
    public Image enemyImg { get { return m_EnemyImg; } }

    [SerializeField]
    private TextMeshProUGUI m_InfoTxt;
    public TextMeshProUGUI infoTxt { get { return m_InfoTxt; } }

    public IngameObjectSpawnInfo spawnInfo { get; private set; }

    public IngameObjectData data => spawnInfo.ingameObjData;
    public int spawnCount => spawnInfo.spawnCount;

    public IngameUI_StartInfo parentUI { get; private set; }

    public void SetData(IngameUI_StartInfo parentUI, IngameObjectSpawnInfo spawnInfo)
    {
        this.parentUI = parentUI;
        this.spawnInfo = spawnInfo;
    }

    public void UpdateUI()
    {
        GameObject gobj;
        if(ResourceManager.Instance.TryGetPrefab(spawnInfo.prefabKey, out gobj))
        {
            enemyImg.sprite = gobj.GetComponent<IngameObject>().bodyRender.sprite;
        }
        else
        {
            enemyImg.sprite = null;
        }

        infoTxt.text = string.Format("<sprite=2>{0}  <sprite=3>{1}  <sprite=7>{2}  <sprite=11>{3}%  <sprite=12>{4}  <b>x{5}</b>"
            , data.maxHp
            , Helper.ConverToMoveSpeedText(data.moveSpeed)
            , Helper.ConvertToUnitTypeText(data.unitType)
            , data.defense * 100f
            , (data.maxShield == 0) ? "-" : data.maxShield.ToString()
            , spawnCount);
    }
}
