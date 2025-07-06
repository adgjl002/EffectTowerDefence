using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameSelectFrame : MonoBehaviour
{
    [SerializeField]
    private GameObject m_RootGObj;
    public GameObject rootGobj { get { return m_RootGObj; } }

    [SerializeField]
    private GameObject m_GridFrameObj;
    public GameObject gridFrameObj { get { return m_GridFrameObj; } }

    [SerializeField]
    private GameObject m_AttackRangeObj;
    public GameObject attackRangeObj { get { return m_AttackRangeObj; } }

    [SerializeField]
    private GameObject m_ArrowObj;
    public GameObject arrowObj { get { return m_ArrowObj; } }

    public float attackRange { get; private set; }
    
    public IngameObject target { get; private set; }

    public void SetGridFrame(float attackRange)
    {
        rootGobj.transform.localScale = Vector3.one;

        target = null;
        this.attackRange = attackRange * 0.4f;
        attackRangeObj.transform.localScale = new Vector3(this.attackRange, this.attackRange, this.attackRange);
        attackRangeObj.SetActive(attackRange > 0);
        arrowObj.SetActive(false);
        gridFrameObj.SetActive(true);
    }

    public void SetIngameObjectFrame(IngameObject target)
    {
        rootGobj.transform.localScale = new Vector3(target.bodySize, target.bodySize, target.bodySize);

        this.target = target;
        attackRange = 0;
        attackRangeObj.SetActive(false);
        arrowObj.SetActive(true);
        gridFrameObj.SetActive(true);
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if(target)
        {
            if(!target.isDie)
            {
                transform.position = target.transform.position;
            }
            else
            {
                Close();
            }
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
