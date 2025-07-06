using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public class Fx_ParticleTarget : FxBase
{
    public override EFxType fxType
    {
        get
        {
            return EFxType.PARTICLE_TARGET;
        }
    }
    
    [Header("Fx Target")]
    public ParticleSystem mainPs;
    public string arrivePsKey;

    public float secondDelay = 1.2f;
    public float particleStartSpeed = 40;
    public float particleAccelation = 0.1f;

    private Vector3 destination;
    
    public Vector3 startPos { get; set; }
    public GameObject target { get; private set; }
    public bool isNonTargeting { get; private set; }

    [Header("Sfx")]
    public string onSfxKey;
    public string arriveSfxKey;

    private System.Func<bool> onSkipCondition;
    private UnityAction<Vector3> onArriveParticle;

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void SetFx(GameObject target, bool isNonTargeting = false, System.Func<bool> onSkipCondition = null, UnityAction<Vector3> onArriveParticle = null)
    {
        this.target = target;
        this.destination = target.transform.position;
        this.onSkipCondition = onSkipCondition;
        this.isNonTargeting = isNonTargeting;
        this.onArriveParticle = onArriveParticle;
    }

    public override void On(UnityAction endCallback = null)
    {
        this.endCallback = endCallback;

        startPos = transform.position += startOffset;

        if (!string.IsNullOrEmpty(onSfxKey))
        {
            //SoundManager.PlayUnitSfx(onSfxKey, transform.position, transform);
        }

        StartCoroutine(Move());
    }

    public override void Off()
    {
        Debug.LogErrorFormat("{0}에서 사용되지 않은 함수({1})를 호출함. 확인 바람.", GetType(), MethodBase.GetCurrentMethod().Name);
    }

    public override void Destroy()
    {
        if (endCallback != null) endCallback();

        StopAllCoroutines();

        if (destroyDelay > 0)
        {
            StartCoroutine(TimerDestroySelf());
        }
        else
        {
            DestroySelf();
        }
    }

    IEnumerator TimerDestroySelf()
    {
        float speed = destroyDelay;
        float timer = 0f;
        while (timer < speed)
        {
            yield return null;
            timer += Time.deltaTime;
        }
        DestroySelf();
    }

    IEnumerator Move()
    {
        yield return new WaitForSeconds(startDelay);

        if (mainPs) mainPs.Play();
        
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[mainPs.main.maxParticles];
        float[] particleSpeed = new float[mainPs.main.maxParticles];

        yield return new WaitForSeconds(secondDelay);

        bool flag = true;
        while (flag)
        {
            flag = false;

            yield return null;
            
            var result = mainPs.GetParticles(particles);
            
            if(!isNonTargeting)
            {
                destination = target.transform.position;
            }

            for (int i = 0; i < result; ++i)
            {
                if (particles[i].remainingLifetime == 0)
                {
                    continue;
                }
                else if (particles[i].remainingLifetime > particles[i].startLifetime - secondDelay)
                {
                    flag = true;
                    continue;
                }
                else
                {
                    flag = true;
                    particleSpeed[i] += particleAccelation * Time.deltaTime;
                }

                var dir = (destination - particles[i].position).normalized;
                var remainDis = Vector3.Distance(destination, particles[i].position);
                
                var add = dir * ((particleStartSpeed * Time.deltaTime) + particleSpeed[i]);
                var addDis = Vector3.Distance(particles[i].position + add, particles[i].position);

                if(Time.timeScale == 0)
                {

                }
                else if (remainDis < addDis)
                {
                    particles[i].position = destination;
                    particles[i].remainingLifetime = 0;

                    for (int j = 0; j < mainPs.subEmitters.subEmittersCount; ++j)
                    {
                        if (mainPs.subEmitters.GetSubEmitterType(j) == ParticleSystemSubEmitterType.Manual)
                        {
                            //mainPs.subEmitters.GetSubEmitterSystem(j).Play();
                            mainPs.TriggerSubEmitter(j, ref particles[i]);
                        }
                    }

                    if (!string.IsNullOrEmpty(arrivePsKey))
                    {
                        Fx_OneTime fx;
                        if(SpawnMaster.TrySpawnFx(arrivePsKey, destination, Quaternion.identity, out fx))
                        {
                            fx.On();
                        }
                    }
                    
                    if(onArriveParticle != null)
                    {
                        onArriveParticle(destination);
                    }
                }
                else
                {
                    particles[i].position += add;
                }
            }

            mainPs.SetParticles(particles, result);
        }

        if (mainPs) mainPs.Stop(true);

        onSkipCondition = null;

        Destroy();
    }

    public override void Release()
    {
        base.Release();

        onArriveParticle = null;
        onSkipCondition = null;
    }
}
