using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBlock_SkyWayPoint : GridBlock
{
    public override EType gridBlockType => EType.SkyWayPoint;
    
    public Vector2 debugDir;
    public Vector2 cur2NxtDir;
    
    public void UpdateWayPointDir(Vector2 pre2CurDir, Vector2 cur2NxtDir)
    {
        debugDir = pre2CurDir;
        this.cur2NxtDir = cur2NxtDir;

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
