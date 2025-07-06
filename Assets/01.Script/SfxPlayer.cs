using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxPlayer : MonoBehaviour
{
    public string sfxKey { get; private set; }

    [SerializeField]
    private AudioSource m_AudioSource;
    public AudioSource audioSource => m_AudioSource;

    private System.Func<SfxPlayer, bool> skipCondition;

    public void SetRandomPitch(float minValue, float maxValue)
    {
        audioSource.pitch = Random.Range(minValue, maxValue);
    }

    public void Play(string sfxKey, System.Func<SfxPlayer, bool> skipCondition = null)
    {
        this.sfxKey = sfxKey;
        this.skipCondition = skipCondition;

        audioSource.clip = ResourceManager.GetAudioClip(sfxKey);
        audioSource.Play();

        StopAllCoroutines();

        if(audioSource.clip != null)
        {
            StartCoroutine(DestroyDelay(audioSource.clip.length + 0.2f));
        }
        else
        {
            DestroySelf();
        }
    }

    private IEnumerator DestroyDelay(float delay = 2f)
    {
        float timer = delay;
        do
        {
            yield return null;

            if (skipCondition != null && skipCondition(this))
            {
                break;
            }

            timer -= Time.deltaTime;
        }
        while (timer > 0);

        DestroySelf();
    }

    public void DestroySelf()
    {
        sfxKey = string.Empty;
        audioSource.clip = null;
        audioSource.pitch = 1f;
        skipCondition = null;

        SpawnMaster.Destroy(gameObject, "SfxPlayer");
    }
}
