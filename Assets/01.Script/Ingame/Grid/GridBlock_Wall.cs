using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class WallAttackedInfo
{
    public int giveDmg;
    public int recvDmg;
    public GridBlock_Wall defender;
}

public class WallData
{
    public WallData(int maxHp)
    {
        this.maxHp = maxHp;
    }

    public int maxHp;
}

public class GridBlock_Wall : GridBlock
{
    public override EType gridBlockType => EType.Wall;

    public WallData data;

    public int maxHp { get { return data.maxHp; } }

    [SerializeField]
    private int m_NowHp;
    public int nowHp { 
        get { return m_NowHp; } 
        set { OnChangedNowHp?.Invoke(m_NowHp, value); m_NowHp = value; }
    }

    #region < Event > 

    public UnityAction<int, int> OnChangedNowHp;

    #endregion

    public bool isWallOpened { get; private set; }

    public Vector2 debugDir;
    public Vector2 cur2NxtDir;
    
    public override void Initialize()
    {
        base.Initialize();
        Close();
    }

    #region < Animator - Func >

    public void PlayAttackedAnimator()
    {
        render.material
            .SetFloat("_AttackedColorAmount", 1);

        render.material
            .DOFloat(0, "_AttackedColorAmount", 0.2f)
            .SetEase(Ease.OutSine);
    }

    #endregion

    public void SetWallData(WallData data)
    {
        this.data = data;

        nowHp = data.maxHp;
    }

    public void Attacked(int damage, out WallAttackedInfo attackedInfo)
    {
        UIManager_Ingame.Instance.ShowDamageUI(transform.position, damage, EDamageType.None, false);

        attackedInfo = new WallAttackedInfo()
        {
            giveDmg = damage,
            recvDmg = (damage > nowHp) ? nowHp : damage,
            defender = this
        };

        nowHp = Mathf.Max(0, nowHp - damage);

        if (nowHp == 0)
        {
            Open();
        }
        else
        {
            PlayAttackedAnimator();
        }
    }

    public void Attacked(int damage)
    {
        UIManager_Ingame.Instance.ShowDamageUI(transform.position, damage, EDamageType.None, false);

        nowHp = Mathf.Max(0, nowHp - damage);

        if (nowHp == 0)
        {
            Open();
        }
        else
        {
            PlayAttackedAnimator();
        }
    }

    public void Open()
    {
        isWallOpened = true;
        render.sprite = ResourceManager.Instance.GetSprite("WayPoint_Straight");

        UpdateWayPointDir(debugDir, cur2NxtDir, false);
    }

    public void Close()
    {
        isWallOpened = false;
        render.sprite = ResourceManager.Instance.GetSprite("WayPoint_Wall");
    }

    public void UpdateWayPointDir(Vector2 pre2CurDir, Vector2 cur2NxtDir, bool onlySet = true)
    {
        debugDir = pre2CurDir;
        this.cur2NxtDir = cur2NxtDir;

        if (onlySet) return;

        if (pre2CurDir == Vector2.right)
        {
            if (cur2NxtDir == Vector2.right) // 오른쪽
            {
                render.sprite = ResourceManager.Instance.GetSprite("WayPoint_Straight");
                render.transform.localRotation = Quaternion.identity;
            }
            else if (cur2NxtDir == Vector2.down) // 왼쪽 아래
            {
                render.sprite = ResourceManager.Instance.GetSprite("WayPoint_Curve");
                render.transform.localRotation = Quaternion.identity;
            }
            else if (cur2NxtDir == Vector2.up) // 왼쪽 위 
            {
                render.sprite = ResourceManager.Instance.GetSprite("WayPoint_Curve");
                render.transform.localRotation = Quaternion.Euler(0, 0, 270);
            }
        }
        else if (pre2CurDir == Vector2.left)
        {
            if (cur2NxtDir == Vector2.left) // 왼쪽
            {
                render.sprite = ResourceManager.Instance.GetSprite("WayPoint_Straight");
                render.transform.localRotation = Quaternion.identity;
            }
            else if (cur2NxtDir == Vector2.up) // 오른쪽 위
            {
                render.sprite = ResourceManager.Instance.GetSprite("WayPoint_Curve");
                render.transform.localRotation = Quaternion.Euler(0, 0, 180);
            }
            else if (cur2NxtDir == Vector2.down) // 오른쪽 아래
            {
                render.sprite = ResourceManager.Instance.GetSprite("WayPoint_Curve");
                render.transform.localRotation = Quaternion.Euler(0, 0, 90);
            }
        }
        else if (pre2CurDir == Vector2.up)
        {
            if (cur2NxtDir == Vector2.up) // 위
            {
                render.sprite = ResourceManager.Instance.GetSprite("WayPoint_Straight");
                render.transform.localRotation = Quaternion.Euler(0, 0, 90);
            }
            else if (cur2NxtDir == Vector2.right) // 아래 오른쪽
            {
                render.sprite = ResourceManager.Instance.GetSprite("WayPoint_Curve");
                render.transform.localRotation = Quaternion.Euler(0, 0, 90);
            }
            else if (cur2NxtDir == Vector2.left) // 아래 왼쪽
            {
                render.sprite = ResourceManager.Instance.GetSprite("WayPoint_Curve");
                render.transform.localRotation = Quaternion.identity;
            }
        }
        else if (pre2CurDir == Vector2.down)
        {
            if (cur2NxtDir == Vector2.down) // 아래
            {
                render.sprite = ResourceManager.Instance.GetSprite("WayPoint_Straight");
                render.transform.localRotation = Quaternion.Euler(0, 0, 90);
            }
            else if (cur2NxtDir == Vector2.right) // 위 오른쪽
            {
                render.sprite = ResourceManager.Instance.GetSprite("WayPoint_Curve");
                render.transform.localRotation = Quaternion.Euler(0, 0, 180);
            }
            else if (cur2NxtDir == Vector2.left) // 위 왼쪽
            {
                render.sprite = ResourceManager.Instance.GetSprite("WayPoint_Curve");
                render.transform.localRotation = Quaternion.Euler(0, 0, 270);

            }
        }
    }

}
