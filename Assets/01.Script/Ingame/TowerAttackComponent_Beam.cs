using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerAttackComponent_Beam : TowerAttackComponent
{
    public class TargetInfo
    {
        public IngameObject target;
        public TowerProjectile projectile;
        public int attackCount;
    }

    public override EType componentType => EType.Beam;
    public override IngameObject curTarget => (attackingTargetInfos.Count > 0) ? attackingTargetInfos[0].target : null;

    private List<TargetInfo> attackingTargetInfos = new List<TargetInfo>();
    private List<string> attackingTargetNames = new List<string>();

    public override void OnInitialize()
    {
        attackingTargetInfos?.Clear();
    }

    public override void InitTarget()
    {
        foreach (var t in attackingTargetInfos)
        {
            t.projectile?.DestroySelf();
        }
    }

    public override bool FindTarget()
    {
        List<TargetInfo> validTargets = new List<TargetInfo>();
        List<string> validTargetNames = new List<string>();
        for (int i = 0; i < attackingTargetInfos.Count; ++i)
        {
            var t = attackingTargetInfos[i].target;
            var dis = Vector3.Distance(owner.transform.position, t.transform.position);
            if (owner.projectileCount <= validTargets.Count)
            {
                attackingTargetInfos[i].projectile.DestroySelf();
            }
            else if (!t.isDie && dis <= owner.attackRange)
            {
                validTargets.Add(attackingTargetInfos[i]);
                validTargetNames.Add(attackingTargetInfos[i].target.name);
            }
            else
            {
                attackingTargetInfos[i].projectile.DestroySelf();
            }
        }

        attackingTargetInfos = validTargets;
        attackingTargetNames = validTargetNames;

        List<IngameObject> newTargets;
        var remainProjectileCount = Mathf.Max(0, owner.projectileCount - attackingTargetInfos.Count);
        if (remainProjectileCount > 0 && Helper.Ingame.FindNearestTargets(owner.gameObject, validTargetNames, remainProjectileCount, owner.attackRange, out newTargets))
        {
            foreach (var t in newTargets)
            {
                TargetInfo newTargetInfo = new TargetInfo();
                newTargetInfo.target = t;
                newTargetInfo.attackCount = 0;

                if (!SpawnMaster.TrySpawnMonoBehaviour(owner.data.projectileKey, owner.projectileSpawnPointTf.position, Quaternion.identity, out newTargetInfo.projectile))
                {
                    Debug.LogErrorFormat("{0} :: Can't spawned TowerProjectile({1})", GetType(), owner.data.projectileKey);
                    return false;
                }

                attackingTargetInfos.Add(newTargetInfo);
            }
        }

        return (attackingTargetInfos.Count > 0);
    }

    public override void Attack()
    {
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

        foreach (var info in attackingTargetInfos)
        {
            ++info.attackCount;

            var attackInfo = new IngameAttackInfo()
            {
                attacker = owner,
                target = info.target,
                damage = owner.damage * info.attackCount,
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
                info.projectile.SetData(new TowerProjectileData(owner, attackInfo));
                info.projectile.Fire();
            }
        }

        // 새로운 타겟을 리스트에 등록한다.
        // 프로젝타일을 생성하고 발사한다.
        // 공격 횟수를 카운팅하고 피해를 입힌다.

        // 기존의 타겟 리스트 중 죽거나 범위 밖으로 나간 적들을 제거한다.
        // 부족한 수만큼 새로운 타겟을 찾는다.
        // 프로젝타일을 생성하고 발사한다.
        // 반복한다.
    }

    public override void OnRelease()
    {
        InitTarget();

        attackingTargetInfos?.Clear();
        attackingTargetNames?.Clear();
    }
}
