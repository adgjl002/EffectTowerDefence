using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance => AppManager.Instance.audioManager;

    [SerializeField]
    private AudioMixer m_AudioMixer;
    public AudioMixer audioMixer { get { return m_AudioMixer; } }

    [SerializeField]
    private AudioSource m_BGMAudioSource;
    public AudioSource BGMAudioSource => m_BGMAudioSource;

    public void Initialize()
    {
        SetBGMVolume(GameSettingsManager.BGMVolume);
        SetSFXVolume(GameSettingsManager.SFXVolume);

        GameSettingsManager.Instance.OnChangedOffBGM += OnChangedOffBGM;
        GameSettingsManager.Instance.OnChangedOffSFX += OnChangedOffSFX;
    }

    public void OnChangedOffBGM(bool off)
    {
        if (off) StopBGM();
        else PlayBGM();
    }

    public void OnChangedOffSFX(bool off)
    {
        if (off) StopSFX();
        else PlaySFX();
    }

    public void Release()
    {

    }

    public void PlayBGM()
    {
        SetBGMVolume(1, true);
    }

    public void StopBGM()
    {
        SetBGMVolume(0);
    }

    public void PlaySFX()
    {
        SetSFXVolume(1, true);
    }

    public void StopSFX()
    {
        SetSFXVolume(0);
    }

    public void SetBGMVolume(float volume, bool useSmoothing = false)
    {
        if(volume == 0)
        {
            BGMAudioSource.Stop();
        }
        else
        {
            BGMAudioSource.Play();
        }

        if (useSmoothing)
        {
            audioMixer.DOSetFloat("BGMVolume", Mathf.Lerp(-80, 0, volume), 1.5f);
        }
        else
        {
            audioMixer.SetFloat("BGMVolume", Mathf.Lerp(-80, 0, volume));
        }
    }

    public void SetSFXVolume(float volume, bool useSmoothing = false)
    {
        if(useSmoothing)
        {
            audioMixer.DOSetFloat("SFXVolume", Mathf.Lerp(-80, 0, volume), 1.5f);
        }
        else
        {
            audioMixer.SetFloat("SFXVolume", Mathf.Lerp(-80, 0, volume));
        }
    }

    public static void PlaySfx(string sfxKey)
    {
        SfxPlayer sfxPlayer;
        if(SpawnMaster.TrySpawnObject<SfxPlayer>("SfxPlayer", Vector3.zero, Quaternion.identity, out sfxPlayer))
        {
            sfxPlayer.Play(sfxKey);
        }
    }

    public static void PlaySfx(string sfxKey, float rndPitchMinValue, float rndPitchMaxValue)
    {
        SfxPlayer sfxPlayer;
        if (SpawnMaster.TrySpawnObject<SfxPlayer>("SfxPlayer", Vector3.zero, Quaternion.identity, out sfxPlayer))
        {
            sfxPlayer.SetRandomPitch(rndPitchMinValue, rndPitchMaxValue);
            sfxPlayer.Play(sfxKey);
        }
    }
}
