using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour 
{
    public static AudioManager sSingleton { get { return _sSingleton; } }
    static AudioManager _sSingleton;

    public AudioSource bgmSource, sfxSource;

    public enum BGM_Tracks
    {
        MAIN_MENU = 0,
        IN_GAME_DIALOGUE,
        IN_GAME_STAGE_1
    }

    // BGM audio.
    public float fadeInSpeed = 0.1f;
    public float fadeOutSpeed = 0.1f;
    public float minBGM_Pitch = 0.7f;
    public float slowDownBGM_PitchSpeed = 1;

    public AudioClip mainMenuBGM;
    public AudioClip inGameDialogueBGM;
    public AudioClip inGameS1BGM;
    public AudioClip inGameS1BossBGM;

    // SFX audio.
    public AudioClip mainMenuMove;
    public AudioClip mainMenuAccept;
    public AudioClip mainMenuAccept2;
    public AudioClip mainMenuBack;
    public AudioClip startGame;
    public AudioClip powerLevelUp;

    // Battle sfx audio.
    public AudioClip handgunShot;
    public AudioClip coinGet;
    public AudioClip bossExplosion;

    bool isKeepFadeIn = false, isKeepFadeOut = false;

    void Awake()
    {
        if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
        else _sSingleton = this;

        DontDestroyOnLoad(this.gameObject);
    }

    // ----------------------------------------------------------------------------------------------------
    // ------------------------------------------ BGM / SFX -----------------------------------------------
    // ----------------------------------------------------------------------------------------------------

    public void PlayBGM() { bgmSource.Play(); }
//    public void PlayBGMAfter(float sec) { StartCoroutine(WaitThenDo(sec, () => { bgmSource.Play(); } )); }
    public void StopBGM() { bgmSource.Stop(); }
    public void PlayMainMenuBGM() { if (!bgmSource.isPlaying) {bgmSource.clip = mainMenuBGM; bgmSource.Play();} }
    public void PlayInGameDialogueBGM() { if (!bgmSource.isPlaying) {bgmSource.clip = inGameDialogueBGM; bgmSource.Play();} }
    public void PlayInGameStage1BGM() { if (!bgmSource.isPlaying) {bgmSource.clip = inGameS1BGM; bgmSource.Play();} }
    public void PlayInGameStage1BossBGM() { if (!bgmSource.isPlaying) {bgmSource.clip = inGameS1BossBGM; bgmSource.Play();} }

    public void PlayMainMenuMoveSfx() { sfxSource.PlayOneShot(mainMenuMove); }
    public void PlayMainMenuAcceptSfx() { sfxSource.PlayOneShot(mainMenuAccept); }
    public void PlayMainMenuAccept2Sfx() { sfxSource.PlayOneShot(mainMenuAccept2); }
    public void PlayMainMenuBackSfx() { sfxSource.PlayOneShot(mainMenuBack); }
    public void PlayMainStartGameSfx() { sfxSource.PlayOneShot(startGame); }
    public void PlayPowerLevelUpSfx() { sfxSource.PlayOneShot(powerLevelUp); }

    public void PlayHandgunSfx() { sfxSource.PlayOneShot(handgunShot); }
    public void PlayCoinGetSfx() { AudioSource.PlayClipAtPoint(coinGet, Camera.main.transform.position, 1); }
    public void PlayBossExplodeSfx() { sfxSource.PlayOneShot(bossExplosion); }

    public void FadeInBGM ()
    {
        if (!isKeepFadeIn) StartCoroutine(FadeIn(bgmSource, fadeInSpeed));
    }

    public void FadeOutBGM ()
    {
        FadeOutFunc(bgmSource, fadeOutSpeed, StopBGM);
    }

    public void SetBGM_Volume(float val)
    {
        bgmSource.volume = val;
    }

    public void PauseBGM(bool isPause)
    {
        if (isPause) bgmSource.Pause();
        else bgmSource.Play();
    }

    public void SetMinBGM_Pitch()
    {
        StartCoroutine(SlowDownBGMToMinPitch(RevertBGMBackToNormalPitch));
    }

    public void ResetFadeIEnumerator()
    {
        isKeepFadeIn = false;
        isKeepFadeOut = false;
    }

    IEnumerator WaitThenDo(float sec, Action doLast)
    {
        yield return new WaitForSeconds(sec);
        doLast();
    }

    IEnumerator SlowDownBGMToMinPitch(Func<IEnumerator> doLast)
    {
        while (bgmSource.pitch > minBGM_Pitch)
        {
            bgmSource.pitch -= 0.01f * slowDownBGM_PitchSpeed;
            if (bgmSource.pitch < minBGM_Pitch) bgmSource.pitch = minBGM_Pitch;

            yield return null;
        }

        StartCoroutine(doLast());
    }

    IEnumerator RevertBGMBackToNormalPitch()
    {
        float defaultBGM_Pitch = bgmSource.pitch;
        float diff = 1 - minBGM_Pitch;

        while (Time.timeScale < 1)
        {
            bgmSource.pitch = defaultBGM_Pitch + (diff * Time.timeScale);
            if (bgmSource.pitch > 1) bgmSource.pitch = 1;

            yield return null;
        }
        bgmSource.pitch = 1;
    }

    IEnumerator FadeIn (AudioSource audioSource, float speed)
    {
        isKeepFadeIn = true;
        isKeepFadeOut = false;

        float audioVol = audioSource.volume = 0;
        while (audioVol < 1 && isKeepFadeIn)
        {
            audioVol += speed;
            if (audioVol > 1) audioVol = 1;

            audioSource.volume = audioVol;
            yield return null;
        }
    }

    void FadeOutFunc(AudioSource audioSource, float speed, Action doLast)
    {
        if (!isKeepFadeOut) StartCoroutine(FadeOut(bgmSource, fadeOutSpeed, doLast));
    }

    IEnumerator FadeOut (AudioSource audioSource, float speed, Action doLast)
    {
        isKeepFadeIn = false;
        isKeepFadeOut = true;

        float audioVol = audioSource.volume;
        while (audioVol > 0 && isKeepFadeOut)
        {
            audioVol -= speed;
            if (audioVol < 0) audioVol = 0;

            audioSource.volume = audioVol;
            yield return null;
        }
        doLast();
    }
}
