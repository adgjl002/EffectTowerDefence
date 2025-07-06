using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngameUI_Damage : UIBase
{
    [SerializeField]
    private TextMeshProUGUI m_DmgTxt;
    public TextMeshProUGUI dmgTxt { get { return m_DmgTxt; } }

    public void SetData(int dmg, EDamageType dmgType, bool isCritical = false)
    {
        dmgTxt.color = Global.GetDamageColor(dmgType, isCritical);
        dmgTxt.text = dmg.ToString();
    }

    public override void Open()
    {
        base.Open();

        StartCoroutine(DestroyDelay());
    }

    private IEnumerator DestroyDelay()
    {
        yield return new WaitForSeconds(1f);

        DestroySelf();
    }

    public void DestroySelf()
    {
        Close();
        Destroy(gameObject);
    }
}
