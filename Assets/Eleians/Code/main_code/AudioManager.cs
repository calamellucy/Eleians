using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

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
    AudioSource uiPlayer;
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
        Achieve = 24,
        elite_stone, elite_spit, elite_rush, elite_def,
        Loot, point_break, point_clear, Gameover, clear,
        boss_sword = 34, boss_darkness, boss_ghost, boss_summon
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

        // 3. ★★★ [추가] UI 전용 플레이어 생성 ★★★
        GameObject uiObject = new GameObject("UiPlayer");
        uiObject.transform.parent = transform;
        uiPlayer = uiObject.AddComponent<AudioSource>();
        uiPlayer.playOnAwake = false;
        uiPlayer.volume = sfxVolume; // 볼륨은 SFX와 동일하게
        uiPlayer.spatialBlend = 0f;  // 2D 사운드

        // ★★★ [핵심] 이 옵션을 켜야 일시정지 때도 소리가 납니다! ★★★
        uiPlayer.ignoreListenerPause = true;

        // ★★★ [수정됨] 인스펙터에 등록된 sfxClips 개수만큼 쿨타임 배열 생성
        // 이제 인스펙터에서 Size를 25로 늘려놨으니, 자동으로 크기 25짜리 배열이 됨!
        // Achieve(24번)도 안전하게 들어감.
        if (sfxClips != null)
        {
            sfxLimitTimes = new float[sfxClips.Length];
        }
        else
        {
            // 만약 실수로 클립을 하나도 등록 안 했을 때를 대비한 안전장치
            sfxLimitTimes = new float[0];
        }
    }

    public void PlaySfx(Sfx sfx)
    {
        // 1. 쿨타임 체크
        if (sfx != Sfx.mouse_on_button && sfx != Sfx.click && Time.time - sfxLimitTimes[(int)sfx] < sfxCooldown)
            return;

        sfxLimitTimes[(int)sfx] = Time.time;

        // 2. 인덱스 계산
        int clipIndex = (int)sfx;

        if (sfx == Sfx.pop)
        {
            clipIndex += Random.Range(0, 5);
        }

        if (clipIndex >= sfxClips.Length) return; // 예외처리

        // ★★★ [수정] UI 소리인지 확인하고 분기 처리 ★★★
        if (sfx == Sfx.click || sfx == Sfx.mouse_on_button || sfx == Sfx.Lvup2)
        {
            // UI 소리면 uiPlayer로 재생 (일시정지 무시함)
            uiPlayer.PlayOneShot(sfxClips[clipIndex]);
            return; // 여기서 끝냄 (아래 SFX 로직 실행 안 함)
        }

        // 2. 빈 오디오 소스 찾기
        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            int loopIndex = (index + channelIndex) % sfxPlayers.Length;

            if (sfxPlayers[loopIndex].isPlaying)
                continue;

            channelIndex = loopIndex;

            // --- 인덱스 결정 ---
            /*
            int clipIndex = (int)sfx;

            if (sfx == Sfx.pop)
            {
                clipIndex += Random.Range(0, 5);
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
            */

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
            case Sfx.elite_def:
            case Sfx.boss_ghost:
                return true;

            default:
                return false;
        }
    }
}