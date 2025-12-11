using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("#BGMs")]
    public AudioClip[] bgmClips; // 인스펙터에서 BGM 파일들을 순서대로 넣어줘
    public float bgmVolume = 0.16f; // 요청한 기본 볼륨 0.2
    AudioSource bgmPlayer; // BGM을 재생할 전용 스피커
    AudioLowPassFilter bgmLowPass; // BGM 필터 (먹먹한 효과용)

    [Header("#SFX")]
    public AudioClip[] sfxClips;
    public float sfxVolume;
    public int channels;
    AudioSource[] sfxPlayers;
    AudioSource uiPlayer;
    int channelIndex;

    // 쿨타임 관리를 위한 변수들
    float[] sfxLimitTimes;
    float sfxCooldown = 0.05f;

    // 페이드 아웃 코루틴 제어용
    Coroutine fadeCoroutine;

    public enum Bgm { Main, Cutscene, Base, Battle, Boss, Win, Null }
    public enum Sfx
    {
        mouse_on_button = 0,
        click = 1, Lvup = 2,
        slash_shot, ele_shot, ele_explo, fire_Ex, flame_sword,
        Lvup2 = 8, PerksAcqui, earthBump, stoneDust, stoneShot, stoneSummon,
        mobDead = 14,
        Ice, Ice_10, Ice_20, Ice_15,
        pop = 19,      // 5개 랜덤 (19~23)
        Achieve = 24,
        elite_stone, elite_spit, elite_rush, elite_def,
        Loot, point_break, point_clear, Gameover, clear,
        boss_sword = 34, boss_darkness, boss_ghost, boss_summon,

        // Element 38번부터 시작
        type = 38
    };

    void Awake()
    {
        // 싱글톤 패턴
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Init()
    {
        // --- BGM 초기화 ---
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();

        // 필터 컴포넌트 추가
        bgmLowPass = bgmObject.AddComponent<AudioLowPassFilter>();
        bgmLowPass.cutoffFrequency = 22000f;

        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;

        // ★★★ [핵심] BGM은 무조건 들려야 함 (0 = 최우선 순위) ★★★
        bgmPlayer.priority = 0;


        // --- SFX 초기화 ---
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];

        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            sfxPlayers[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[index].playOnAwake = false;
            sfxPlayers[index].volume = sfxVolume;
            sfxPlayers[index].spatialBlend = 0f;

            // ★★★ [핵심] SFX는 BGM보다 중요도 낮게 설정 (128 = 기본값) ★★★
            // 이렇게 하면 소리가 꽉 찼을 때 BGM 대신 가장 오래된 SFX가 꺼짐
            sfxPlayers[index].priority = 128;
        }

        // --- UI 초기화 ---
        GameObject uiObject = new GameObject("UiPlayer");
        uiObject.transform.parent = transform;
        uiPlayer = uiObject.AddComponent<AudioSource>();
        uiPlayer.playOnAwake = false;
        uiPlayer.volume = sfxVolume;
        uiPlayer.spatialBlend = 0f;
        uiPlayer.ignoreListenerPause = true;

        // UI 소리도 중요하니까 조금 높게 줘도 됨 (선택사항)
        uiPlayer.priority = 50;

        if (sfxClips != null)
        {
            sfxLimitTimes = new float[sfxClips.Length];
        }
        else
        {
            sfxLimitTimes = new float[0];
        }
    }

    void Update()
    {
        // 일시정지(Time.timeScale == 0) 상태면 700Hz(먹먹함), 아니면 22000Hz(선명함)
        float targetFreq = (Time.timeScale == 0) ? 700f : 22000f;

        // 부드럽게 필터 적용 (unscaledDeltaTime 사용 필수)
        bgmLowPass.cutoffFrequency = Mathf.Lerp(bgmLowPass.cutoffFrequency, targetFreq, Time.unscaledDeltaTime * 10f);
    }

    public void PlayBgm(Bgm type)
    {
        int index = (int)type;

        if (index < 0 || index >= bgmClips.Length)
        {
            Debug.LogError("BGM 클립이 부족하거나 인덱스가 잘못되었습니다!");
            return;
        }

        // 이미 재생 중이면 다시 틀지 않음
        if (bgmPlayer.clip == bgmClips[index] && bgmPlayer.isPlaying) return;

        bgmPlayer.clip = bgmClips[index];
        bgmPlayer.volume = bgmVolume; // 볼륨 확실하게 설정
        bgmPlayer.Play();
    }

    public void StopBgm()
    {
        bgmPlayer.Stop();
    }

    // ★★★ [기능] 페이드 아웃 후 BGM 교체 ★★★
    public void TurnOffAudio(float duration, Bgm nextBgm)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutAndSwitchRoutine(duration, nextBgm));
    }

    IEnumerator FadeOutAndSwitchRoutine(float duration, Bgm nextBgm)
    {
        float startVolume = bgmPlayer.volume;
        float timer = 0f;

        // 1. n초 동안 볼륨 줄이기
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            bgmPlayer.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        bgmPlayer.volume = 0f;
        bgmPlayer.Stop(); // 일단 멈춤

        // 2. 다음 BGM으로 교체 및 즉시 재생
        int index = (int)nextBgm;
        if (index >= 0 && index < bgmClips.Length)
        {
            bgmPlayer.clip = bgmClips[index];
            bgmPlayer.volume = bgmVolume; // 기본 볼륨(0.2) 복구
            bgmPlayer.Play();
        }
    }

    public void PlaySfx(Sfx sfx)
    {
        // 쿨타임 및 예외 처리
        if (sfx != Sfx.type && sfx != Sfx.mouse_on_button && sfx != Sfx.click && Time.time - sfxLimitTimes[(int)sfx] < sfxCooldown)
            return;

        sfxLimitTimes[(int)sfx] = Time.time;
        int clipIndex = (int)sfx;

        // 랜덤 로직
        if (sfx == Sfx.pop)
        {
            clipIndex += Random.Range(0, 5); // 19 ~ 23
        }

        if (sfx == Sfx.type)
        {
            clipIndex += Random.Range(0, 8); // 38 ~ 45
        }

        if (clipIndex >= sfxClips.Length) return;

        // UI 소리는 uiPlayer 사용
        if (sfx == Sfx.click || sfx == Sfx.mouse_on_button || sfx == Sfx.Lvup2)
        {
            uiPlayer.PlayOneShot(sfxClips[clipIndex]);
            return;
        }

        // 일반 SFX 재생 (채널 순환)
        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            int loopIndex = (index + channelIndex) % sfxPlayers.Length;

            if (sfxPlayers[loopIndex].isPlaying)
                continue;

            channelIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[clipIndex];

            // 피치 조절 로직
            if (CheckPitchRandom(sfx))
            {
                if (sfx == Sfx.mobDead)
                    sfxPlayers[loopIndex].pitch = Random.Range(0.7f, 0.85f);
                else
                    sfxPlayers[loopIndex].pitch = Random.Range(0.95f, 1.05f);
            }
            else
            {
                sfxPlayers[loopIndex].pitch = 1f;
            }

            sfxPlayers[loopIndex].Play();
            break;
        }
    }

    bool CheckPitchRandom(Sfx sfx)
    {
        switch (sfx)
        {
            case Sfx.ele_shot:
            case Sfx.ele_explo:
            case Sfx.fire_Ex:
            case Sfx.earthBump:
            case Sfx.stoneDust:
            case Sfx.stoneShot:
            case Sfx.stoneSummon:
            case Sfx.mobDead:
            case Sfx.Ice:
            case Sfx.pop:
            case Sfx.elite_def:
            case Sfx.boss_ghost:
            case Sfx.type:
                return true;

            default:
                return false;
        }
    }
}