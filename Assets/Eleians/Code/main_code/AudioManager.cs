using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("#BGM")]
    public AudioClip bgmClip;
    public float bgmVolume;
    AudioSource bgmPlayer;

    [Header("#SFX")]
    public AudioClip[] sfxClips;
    public float sfxVolume;
    public int channels;
    AudioSource[] sfxPlayers;
    int channelIndex;

    // 쿨타임 관리를 위한 변수들
    float[] sfxLimitTimes;
    float sfxCooldown = 0.05f;

    public enum Sfx
    {
        mouse_on_button = 0,
        click = 1, Lvup = 2,
        slash_shot, ele_shot, ele_explo, fire_Ex, flame_sword,
        Lvup2 = 8, PerksAcqui, earthBump, stoneDust, stoneShot, stoneSummon,
        mobDead = 14, // ★ 몬스터 사망 (피치 낮춰서 사용)
        Ice, Ice_10, Ice_20, Ice_15,
        pop = 19,     // ★ 5개 랜덤 (피치는 정상)
        swoosh = 24   // ★ 4개 랜덤 (피치 정상)
    };

    void Awake()
    {
        instance = this;
        Init();
    }

    void Init()
    {
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;

        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];

        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            sfxPlayers[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[index].playOnAwake = false;
            sfxPlayers[index].volume = sfxVolume;
            sfxPlayers[index].spatialBlend = 0f;
        }

        sfxLimitTimes = new float[System.Enum.GetValues(typeof(Sfx)).Length];
    }

    public void PlaySfx(Sfx sfx)
    {
        // 1. 쿨타임 체크
        if (Time.time - sfxLimitTimes[(int)sfx] < sfxCooldown)
            return;

        sfxLimitTimes[(int)sfx] = Time.time;

        // 2. 빈 오디오 소스 찾기
        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            int loopIndex = (index + channelIndex) % sfxPlayers.Length;

            if (sfxPlayers[loopIndex].isPlaying)
                continue;

            channelIndex = loopIndex;

            // --- 인덱스 결정 ---
            int clipIndex = (int)sfx;

            if (sfx == Sfx.pop)
            {
                clipIndex += Random.Range(0, 5);
            }
            else if (sfx == Sfx.swoosh)
            {
                clipIndex += Random.Range(0, 4);
            }

            // ★★★ [극약 처방] ★★★
            // 리스트 크기가 모자라면(가짜 매니저라면) 에러 내지 말고 그냥 함수 종료
            if (clipIndex >= sfxClips.Length)
            {
                // 디버그 로그도 시끄러우면 주석 처리 가능
                // Debug.LogWarning($"[경고] 가짜 매니저가 소리 재생을 시도함! (ClipIndex: {clipIndex}, Size: {sfxClips.Length})");
                return;
            }
            // ---------------------

            sfxPlayers[loopIndex].clip = sfxClips[clipIndex];

            // 3. 피치 조절
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
            case Sfx.swoosh:
                return true;

            default:
                return false;
        }
    }
}