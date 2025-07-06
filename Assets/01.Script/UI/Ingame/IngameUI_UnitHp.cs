using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameUI_UnitHp : UIBase
{
    public const string PrefabKey = "IngameUI_UnitHp";

    private IngameObject target;

    [SerializeField]
    private SliceSliderUI m_HpBarUI;
    public SliceSliderUI hpBarUI { get { return m_HpBarUI; } }

    [SerializeField]
    private SliceSliderUI m_ShieldBarUI;
    public SliceSliderUI shieldBarUI { get { return m_ShieldBarUI; } }

    public void SetData(IngameObject target)
    {
        this.target = target;
    }

    public void UpdateUI()
    {
        hpBarUI.fillAmount = (float)target.nowHp / target.maxHp;

        var maxShield = target.maxShield;
        if(maxShield > 0) shieldBarUI.fillAmount = (float)target.nowShield / maxShield;
        else shieldBarUI.fillAmount = 0;
    }

    public void FollowTarget()
    {
        transform.position = target.transform.position + new Vector3(0, target.caledUIHeight, 0);
    }

    public void DestroySelf()
    {
        target = null;

        SpawnMaster.Destroy(gameObject, PrefabKey);
    }
}
