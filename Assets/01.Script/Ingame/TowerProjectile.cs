using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public struct TowerProjectileData
{
    public TowerProjectileData(GridBlock_Tower attacker, IngameAttackInfo attackInfo)
    {
        this.attacker = attacker;
        this.attackInfo = attackInfo;
    }

    public GridBlock_Tower attacker;
    public IngameObject target { get { return attackInfo.target; } }
    public IngameAttackInfo attackInfo;
}

public class TowerProjectile : MonoBehaviour {

    public enum EType
    {
        Normal,
        Laser_R,
        Laser_P
    }

    public virtual EType projectileType { get { return EType.Normal; } }

    public TowerProjectileData data { get; private set; }

    [SerializeField]
    private string m_PrefabKey;
    public string prefabKey => m_PrefabKey;

    [SerializeField]
    private float m_MoveSpeed;
    public float moveSpeed => m_MoveSpeed;
    public float calculatedMoveSpeed { get; private set; }

    [SerializeField]
    private float m_DestroyDelay;
    public float destroyDelay => m_DestroyDelay;
    
    public Vector3 startPos;
    public Vector3 endPos;

    public bool isSetData;
    public bool isArrived;
    public bool isFired;

    private float arrivalDis = 0.2f;

    public UnityAction<TowerProjectile> onArrival;
    
    private List<FxBase> fxs = new List<FxBase>();

    public void SetData(TowerProjectileData data, UnityAction<TowerProjectile> onArrival = null)
    {
        isSetData = true;
        isArrived = false;
        isFired = false;

        this.data = data;
        this.onArrival = onArrival;

        var skillID = UserInfo.Instance.skillInv.GetHighestLevelSkillID(1, "ALL");

        float addPerc;
        SkillData skillData;
        if(DataManager.Instance.TryGetSkillData(skillID, out skillData) && float.TryParse(skillData.effectDatas[0], out addPerc))
        {
            Debug.Log(data.attackInfo);
            calculatedMoveSpeed = (moveSpeed + data.attackInfo.addProjectileMoveSpeed) * (1 + addPerc);
            arrivalDis = ((moveSpeed + data.attackInfo.addProjectileMoveSpeed) * 0.02f) * (1 + addPerc);
        }
        else
        {
            calculatedMoveSpeed = (moveSpeed + data.attackInfo.addProjectileMoveSpeed);
            arrivalDis = (moveSpeed + data.attackInfo.addProjectileMoveSpeed) * 0.02f;
        }
    }
    
    public void Fire()
    {
        if(isSetData && !isFired)
        {
            isFired = true;
            OnFire();
        }
    }

    public virtual void OnFire()
    {

    }

    public virtual void Move()
    {
        if (isArrived)
        {
            return;
        }
        else if (data.target.isDie)
        {
            StartCoroutine(DestroyDelay());
            return;
        }

        var dir = (data.target.transform.position - transform.position).normalized;
        var nextPos = transform.position + (dir * Time.deltaTime * (calculatedMoveSpeed + data.attackInfo.addProjectileMoveSpeed));

        var disToTarget = Vector3.Distance(data.target.transform.position, transform.position);
        var disToNextPos = Vector3.Distance(transform.position, nextPos);

        if (disToTarget > disToNextPos)
        {
            transform.position = nextPos;
            transform.transform.rotation = Quaternion.Euler(0, 0, Helper.Ingame.ContAngle(transform.forward, (transform.forward + dir).normalized));
        }
        else
        {
            transform.position = data.target.transform.position;
            StartCoroutine(DestroyDelay());
        }
    }

    private void Update()
    {
        if(isFired) Move();
    }

    protected IEnumerator DestroyDelay()
    {
        isArrived = true;

        IngameAttackedInfo attackedInfo;

        if(data.attackInfo.splashRange > 0)
        {
            Fx_OneTime fx;
            if (SpawnMaster.TrySpawnFx("Fx_1_Hit", transform.position, Quaternion.identity, out fx))
            {
                fx.On();
            }
        }
        else
        {
            Fx_OneTime fx;
            if (SpawnMaster.TrySpawnFx("Fx_6_Hit", transform.position, Quaternion.identity, out fx))
            {
                fx.On();
            }
        }

        if(data.attackInfo.transitionCount > 0)
        {
            IngameAttackInfo transitionAttackInfo = new IngameAttackInfo(data.attackInfo);
            transitionAttackInfo.damage = Mathf.RoundToInt(data.attackInfo.damage * data.attackInfo.transitionDmgRatio);
            transitionAttackInfo.excludeTransitionTargets.Add(data.target.name);
            --transitionAttackInfo.transitionCount;

            IngameObject transitionTarget;
            if(Helper.Ingame.FindNearestTarget(data.target.gameObject, transitionAttackInfo.excludeTransitionTargets, 0.6f, out transitionTarget))
            {
                transitionAttackInfo.target = transitionTarget;

                TowerProjectile proj;
                if (!SpawnMaster.TrySpawnMonoBehaviour(data.attacker.data.projectileKey, transform.position, Quaternion.identity, out proj))
                {
                    Debug.LogErrorFormat("{0} :: Can't spawned TowerProjectile({1})", GetType(), data.attacker.data.projectileKey);
                }
                else
                {
                    proj.SetData(new TowerProjectileData(data.attacker, transitionAttackInfo));
                    proj.Fire();
                }
            }
        }

        data.target.Attacked(data.attackInfo, out attackedInfo);

        if (data.attackInfo.splashRange > 0)
        {
            List<IngameObject> splashTargets;
            if(Helper.Ingame.FindAllTargets(data.target.gameObject, data.target, data.attackInfo.splashRange, out splashTargets))
            {
                foreach(var t in splashTargets)
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

        if (onArrival != null) onArrival(this);

        yield return new WaitForSeconds(destroyDelay);

        DestroySelf();
    }

    public virtual void Attack()
    {

    }

    public void DestroySelf()
    {
        if(fxs != null) fxs.Clear();

        isArrived = false;
        onArrival = null;
        isSetData = false;
        isFired = false;

        OnDestroySelf();

        SpawnMaster.Destroy(gameObject, prefabKey);
    }

    public virtual void OnDestroySelf()
    {

    }
}
