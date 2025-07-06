using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TowerProjectile_Laser_P : TowerProjectile
{
    public override EType projectileType => EType.Laser_P;

    [SerializeField]
    private LineRenderer m_Render;
    public LineRenderer render => m_Render;

    [SerializeField]
    private Fx_Switch m_StartFx;
    public Fx_Switch startFx => m_StartFx;

    [SerializeField]
    private Fx_Switch m_HitFx;
    public Fx_Switch hitFx => m_HitFx;

    private float timer;
    private int count;

    [Header("# 레이저 옵션")]
    [Tooltip("1회 공격 시 레이저 너비 증가량")]
    public float increaseWidth = 0.01f;
    [Tooltip("레이저 너비의 최대 증가 횟수")]
    public int limitWidthIncreaseCount = 5;

    public float attackDelay = 0.1f;

    public override void OnFire()
    {
        if(data.target.isDie)
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(Firing());
    }

    private IEnumerator Firing()
    {
        ++count;
        transform.position = Vector3.zero;

        float maxWidth = increaseWidth * Mathf.Min(limitWidthIncreaseCount, count);
        float minWidth = increaseWidth * (Mathf.Min(limitWidthIncreaseCount, count) - 1);
        float startDuration = attackDelay;
        var target = data.target;
        var projectileTargetPos = data.target.transform.position;
        var projectileSpawnPos = data.attacker.projectileSpawnPointTf.position;

        var angle = Vector3.Angle(new Vector3(Vector3.up.x, Vector3.up.y, 0), new Vector3(projectileTargetPos.x, projectileTargetPos.y, 0) - new Vector3(projectileSpawnPos.x, projectileSpawnPos.y, 0));
        if (projectileSpawnPos.x < projectileTargetPos.x)
        {
            angle = 180 + (180 - angle);
        }

        var dir = (projectileTargetPos - projectileSpawnPos).normalized;
        render.SetPositions(new Vector3[] { projectileSpawnPos, projectileTargetPos });
        render.startWidth = render.endWidth = minWidth;

        startFx.transform.localScale = new Vector3(maxWidth, maxWidth, maxWidth);
        startFx.transform.position = projectileSpawnPos;
        startFx.On();

        hitFx.transform.localScale = new Vector3(maxWidth, maxWidth, maxWidth);
        hitFx.transform.position = projectileTargetPos;
        hitFx.On();

        float timer = startDuration;
        do
        {
            yield return null;

            timer = Mathf.Max(0, timer - Time.deltaTime);
            
            render.SetPositions(new Vector3[] { data.attacker.projectileSpawnPointTf.position, data.target.transform.position });
            render.startWidth = render.endWidth = Mathf.Lerp(maxWidth, minWidth, timer / startDuration);

            startFx.transform.localScale = hitFx.transform.localScale = new Vector3(render.endWidth, render.endWidth, render.endWidth);
            startFx.transform.position = data.attacker.projectileSpawnPointTf.position;
            hitFx.transform.position = data.target.transform.position;
        }
        while (timer > 0);

        // 데미지 입힘.
        IngameAttackedInfo attackedInfo;
        if (data.attackInfo.splashRange > 0)
        {
            Fx_OneTime fx;
            if (SpawnMaster.TrySpawnFx("Fx_1_Hit", target.transform.position, Quaternion.identity, out fx))
            {
                fx.On();
            }
        }
        //else
        //{
        //    Fx_OneTime fx;
        //    if (SpawnMaster.TrySpawnFx("Fx_6_Hit", target.transform.position, Quaternion.identity, out fx))
        //    {
        //        fx.On();
        //    }
        //}

        target.Attacked(data.attackInfo, out attackedInfo);

        if (data.attackInfo.splashRange > 0)
        {
            List<IngameObject> splashTargets;
            if (Helper.Ingame.FindAllTargets(target.gameObject, target, data.attackInfo.splashRange, out splashTargets))
            {
                foreach (var t in splashTargets)
                {
                    var newAttInfo = new IngameAttackInfo(data.attackInfo);
                    newAttInfo.damage = Mathf.RoundToInt(data.attackInfo.damage * data.attackInfo.splashDmgRatio);
                    newAttInfo.damageType = EDamageType.Explosion;
                    newAttInfo.OnApplyAdditionalEffect = null;

                    if (data.attackInfo.OnAddSplashDamage != null)
                    {
                        newAttInfo = data.attackInfo.OnAddSplashDamage(newAttInfo);
                    }

                    t.Attacked(newAttInfo);
                }
            }
        }
        
        timer = 10f;
        while (timer > 0)
        {
            render.SetPositions(new Vector3[] { data.attacker.projectileSpawnPointTf.position, data.target.transform.position });
            render.startWidth = render.endWidth = Mathf.Lerp(minWidth, maxWidth, Random.Range(0.2f, 1f));

            startFx.transform.localScale = hitFx.transform.localScale = new Vector3(render.endWidth, render.endWidth, render.endWidth);
            startFx.transform.position = data.attacker.projectileSpawnPointTf.position;
            hitFx.transform.position = data.target.transform.position;

            yield return null;
            
            timer -= Time.deltaTime;
        }

        OnDestroySelf();
    }

    public override void OnDestroySelf()
    {
        StopAllCoroutines();

        startFx.Off();
        hitFx.Off();

        base.OnDestroySelf();

        count = 0;

        render.startWidth = render.endWidth = 0;
        render.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
    }

    public override void Move()
    {

    }
}
