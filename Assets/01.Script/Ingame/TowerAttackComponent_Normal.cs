using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerAttackComponent_Normal : TowerAttackComponent
{
    public override EType componentType => EType.Normal;

    private IngameObject m_CurTarget;
    public override IngameObject curTarget => m_CurTarget;

    public override void OnInitialize()
    {

    }
    
    public override void InitTarget()
    {
        m_CurTarget = null; 
    }

    public override bool FindTarget()
    {
        var curAttackRange = owner.attackRange;

        // 기존 타겟이 살아 있으면서 사정거리 내에 존재하는가?
        if (curTarget != null && !curTarget.isDie && Vector3.Distance(curTarget.transform.position, transform.position) <= curAttackRange)
        {
            return true;
        }
        m_CurTarget = null;

        float nearestDis = 999999f;
        //Dictionary<float, IngameObject> targets = new Dictionary<float, IngameObject>();
        foreach (var pair in IngameManager.Instance.ingameObjects)
        {
            var dis = Vector3.Distance(pair.Value.transform.position, transform.position);
            if (dis <= curAttackRange && dis < nearestDis)
            {
                nearestDis = dis;
                m_CurTarget = pair.Value;
                //while(targets.ContainsKey(dis))
                //{
                //    dis += 0.0001f;
                //}
                //targets.Add(dis, pair.Value);
            }
        }

        return (m_CurTarget != null);
    }

    public override void Attack()
    {
        if (curTarget.isDie)
        {
            return;
        }

        owner.LookTarget();

        if (curAttackDelay > 0)
        {
            curAttackDelay -= Time.deltaTime;
            return;
        }

        if (++curAttackCount >= owner.attackCount)
        {
            curAttackDelay = 1.0f / owner.attackSpeed;
            curAttackCount = 0;
        }
        else
        {
            curAttackDelay = 0.05f;
        }

        List<IngameObject> targets;
        if (owner.projectileCount > 1 && Helper.Ingame.FindTarget(gameObject, curTarget, owner.projectileCount - 1, owner.attackRange, out targets))
        {

        }
        else
        {
            targets = new List<IngameObject>();
        }
        targets.Add(curTarget);

        foreach (var t in targets)
        {
            var attackInfo = new IngameAttackInfo()
            {
                attacker = owner,
                target = t,
                damage = owner.damage,
                damageType = EDamageType.None
            };

            if (UnityEngine.Random.Range(0, 100) <= owner.criticalPerc * 100)
            {
                attackInfo.isCritical = true;
                attackInfo.damage = Mathf.RoundToInt(attackInfo.damage * owner.criticalDamageRatio);
            }

            if (owner.OnAttack != null)
            {
                attackInfo = owner.OnAttack(attackInfo);
            }

            if(attackInfo != null)
            {
                TowerProjectile proj;
                if (!SpawnMaster.TrySpawnMonoBehaviour(owner.data.projectileKey, owner.projectileSpawnPointTf.position, Quaternion.identity, out proj))
                {
                    Debug.LogErrorFormat("{0} :: Can't spawned TowerProjectile({1})", GetType(), owner.data.projectileKey);
                    return;
                }

                proj.SetData(new TowerProjectileData(owner, attackInfo));
                proj.Fire();
            }
        }
    }

    public override void OnRelease()
    {

    }
}
