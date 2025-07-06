using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TowerProjectile_Laser : TowerProjectile
{
    public override EType projectileType => EType.Laser_R;

    [SerializeField]
    private LineRenderer m_Render;
    public LineRenderer render => m_Render;

    [SerializeField]
    private Fx_Switch m_StartFx;
    public Fx_Switch startFx => m_StartFx;

    private float timer;

    [Header("# 레이저 옵션")]
    public float startDuration = 0.02f;
    public float endDuration = 0.38f;

    public float minWidth = 0;
    public float maxWidth = 0.05f;

    public float laserLength = 10;

    public float maxWidthRatio = 1f;

    public override void OnFire()
    {
        if(data.target.isDie)
        {
            DestroySelf();
            return;
        }

        StartCoroutine(Firing());
    }

    private IEnumerator Firing()
    {
        yield return new WaitWhile(() => { return Time.timeScale == 0; });

        transform.position = Vector3.zero;
        
        var projectileTargetPos = data.target.transform.position;
        var projectileSpawnPos = data.attacker.projectileSpawnPointTf.position;

        var angle = Vector3.Angle(new Vector3(Vector3.up.x, Vector3.up.y, 0), new Vector3(projectileTargetPos.x, projectileTargetPos.y, 0) - new Vector3(projectileSpawnPos.x, projectileSpawnPos.y, 0));
        if (projectileSpawnPos.x < projectileTargetPos.x)
        {
            angle = 180 + (180 - angle);
        }

        var dir = (projectileTargetPos - projectileSpawnPos).normalized;
        render.SetPositions(new Vector3[] { projectileSpawnPos, projectileSpawnPos + new Vector3(dir.x * laserLength, dir.y * laserLength, projectileSpawnPos.z) });
        render.startWidth = render.endWidth = 0;
        render.startColor = render.endColor = Color.red;

        startFx.transform.localScale = new Vector3(maxWidth, maxWidth, maxWidth);
        startFx.transform.position = projectileSpawnPos;
        startFx.On();
        
        // 데미지 입힘.
        var hits = Physics2D.BoxCastAll(projectileSpawnPos, new Vector2(0.02f * maxWidthRatio, 0.02f * maxWidthRatio), angle, dir * laserLength);
        if(hits != null)
        {
            foreach(var h in hits)
            {
                if(h.collider.CompareTag("IngameObject"))
                {
                    var target = h.collider.GetComponent<IngameObject>();

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
                    
                    target.Attacked(new IngameAttackInfo(data.attackInfo), out attackedInfo);

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

                }
            }
        }

        float timer = startDuration;
        do
        {
            yield return null;

            timer = Mathf.Max(0, timer - Time.deltaTime);

            render.startWidth = render.endWidth = Mathf.Lerp(maxWidth * maxWidthRatio, minWidth, timer / startDuration);
        }
        while (timer > 0);

        timer = endDuration;
        while (timer > 0)
        {
            yield return null;

            timer -= Time.deltaTime;

            render.startWidth = render.endWidth = Mathf.Lerp(minWidth, maxWidth * maxWidthRatio, timer / endDuration);
        }

        render.startWidth = render.endWidth = minWidth;

        DestroySelf();
    }

    public override void OnDestroySelf()
    {
        base.OnDestroySelf();

        startFx.Off();

        maxWidthRatio = 1f;
        render.startWidth = render.endWidth = 0;
        render.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
    }

    public override void Move()
    {

    }
}
