using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager instance;

    [Header("Settings")]
    public float displayTime = 3.0f;   // 떠 있는 시간

    [Header("Position Info")]
    public float visiblePosX = 0f;     // 화면에 보일 때 X 좌표
    public float hiddenPosX = -250f;   // 숨겨둘 때 X 좌표 (너비 190 고려)

    [Header("Achievement Objects")]
    // ★ 구조체 대신 그냥 GameObject 배열! 
    // 인스펙터 Lock 걸고 16개 한 번에 드래그해서 넣으세요.
    public GameObject[] achievementUIs;

    // 달성 여부 관리 (배열 인덱스와 1:1 매칭)
    private bool[] isUnlocked;

    // 내부 변수
    private Queue<GameObject> displayQueue = new Queue<GameObject>();
    private bool isAnimating = false;

    // 통계용 변수
    [HideInInspector] public int protectedTowerCount = 0;
    [HideInInspector] public bool isNoDamage = true;
    [HideInInspector] public float bossSpawnTime = -1f;

    // 카운트 변수들
    private int fireSkillUseCnt = 0;   // 화폭술 사용 횟수
    private int iceChargeHitCount = 0; // 빙벽 돌진 타격 횟수

    private bool checkBlueFrog = false;
    private float blueFrogTimer = 0f;


    void Awake()
    {
        instance = this;
        // 업적 개수만큼 불리언 배열 초기화 (기본 false)
        if (achievementUIs != null)
            isUnlocked = new bool[achievementUIs.Length];
    }

    void Start()
    {
        InitAchievementUI();
    }

    void Update()
    {
        if (!GameManager.instance.isLive) return;
        CheckStatAchievements();
        CheckBlueFrog();
    }

    // 시작 시 모든 UI 숨기기
    void InitAchievementUI()
    {
        if (achievementUIs == null) return;

        foreach (var ui in achievementUIs)
        {
            if (ui != null)
            {
                RectTransform rect = ui.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(hiddenPosX, rect.anchoredPosition.y);
                ui.SetActive(true); // 활성화는 해두고 위치만 숨김
            }
        }
    }

    public bool CheckUnlocked(int index)
    {
        // 배열이 없거나 인덱스가 범위를 벗어나면 false (잠김 처리)
        if (isUnlocked == null || index < 0 || index >= isUnlocked.Length)
            return false;

        // 해당 인덱스(0~15)가 true인지 반환
        return isUnlocked[index];
    }

    // ★ 업적 달성 함수 (ID는 1부터 시작하는 걸로 가정)
    public void Unlock(int id)
    {
        int index = id - 1;

        // 예외 처리
        if (achievementUIs == null || index < 0 || index >= achievementUIs.Length) return;
        if (achievementUIs[index] == null) return;

        // 이미 깼으면 패스
        if (isUnlocked[index]) return;

        // 달성 처리
        isUnlocked[index] = true;
        Debug.Log($"업적 달성! ID: {id} (Index: {index})");

        // UI 큐에 추가 및 애니메이션 시작
        displayQueue.Enqueue(achievementUIs[index]);

        if (!isAnimating)
        {
            StartCoroutine(CoAnimateAchievement());
        }
    }

    // 애니메이션 코루틴
    IEnumerator CoAnimateAchievement()
    {
        isAnimating = true;
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Achieve);
        while (displayQueue.Count > 0)
        {
            GameObject currentUI = displayQueue.Dequeue();
            RectTransform rect = currentUI.GetComponent<RectTransform>();

            // 1. 등장
            yield return StartCoroutine(SlideUI(rect, hiddenPosX, visiblePosX));

            // 2. 대기 (리얼타임)
            yield return new WaitForSecondsRealtime(displayTime);

            // 3. 퇴장
            yield return StartCoroutine(SlideUI(rect, visiblePosX, hiddenPosX));

            // 딜레이 (리얼타임)
            yield return new WaitForSecondsRealtime(0.2f);
        }

        isAnimating = false;
    }

    // 부드럽게 이동 (TimeScale 무시)
    IEnumerator SlideUI(RectTransform rect, float startX, float endX)
    {
        float timer = 0f;
        float duration = 0.5f;

        Vector2 startPos = new Vector2(startX, rect.anchoredPosition.y);
        Vector2 endPos = new Vector2(endX, rect.anchoredPosition.y);

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / duration;
            t = t * t * (3f - 2f * t); // SmoothStep

            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
        rect.anchoredPosition = endPos;
    }

    // ====================================================
    // 로직 조건 체크 (ID는 리스트 순서대로 1번부터 호출)
    // ====================================================

    void CheckStatAchievements()
    {
        StatsManager stats = StatsManager.instance;
        if (stats == null) return;

        // 10. 럭키가이 (치명타 50%)
        if (stats.CritChance >= 0.5f) Unlock(10);

        // ★ [복구] 7. 토르 (전기 28레벨 -> 발사체 10개 자동 달성)
        // 네 말대로 레벨만 체크하면 끝!
        if (stats.ElectricCnt >= 28) Unlock(7);

        // 11~14. 속성 지배자
        if (stats.ElectricCnt >= 40) Unlock(11);
        if (stats.FireCnt >= 40) Unlock(12);
        if (stats.IceCnt >= 40) Unlock(13);
        if (stats.EarthCnt >= 40) Unlock(14);

        // 15. 균형의 수호자
        if (stats.ElectricCnt >= 10 && stats.FireCnt >= 10 &&
            stats.IceCnt >= 10 && stats.EarthCnt >= 10) Unlock(15);
    }

    public void OnTowerDefended()
    {
        protectedTowerCount++;
        Unlock(1);
        if (protectedTowerCount >= 4)
        {
            Unlock(2);
            checkBlueFrog = true;
        }
    }

    public void OnBossSpawn() => bossSpawnTime = GameManager.instance.gameTime;

    public void OnBossKilled()
    {
        Unlock(3);

        if (bossSpawnTime > 0 && (GameManager.instance.gameTime - bossSpawnTime) <= 30f)
        {
            Unlock(4);
        }

        float hpPercent = GameManager.instance.health / GameManager.instance.maxHealth;
        if (hpPercent <= 0.1f) Unlock(5);

        if (isNoDamage)
            Unlock(6);
    }

    public void OnPlayerTakeDamage() => isNoDamage = false;

    // ★ [정리] 화폭술 전용 함수가 됨
    public void OnSkillUsed(string skillName)
    {
        if (skillName == "Hwapoksul")
        {
            fireSkillUseCnt++;
            // 8. 사실 나도 학교에 가본 적이 없어 (화폭술 15회 사용)
            if (fireSkillUseCnt >= 15) Unlock(8);
        }
    }

    // ★ 빙벽 돌진 타격 카운트
    public void OnIceChargeHit()
    {
        iceChargeHitCount++;
        // 9. 폭주기관차 (빙벽 돌진으로 적 300회 타격)
        if (iceChargeHitCount >= 300)
        {
            Unlock(9);
        }
    }

    void CheckBlueFrog()
    {
        if (!checkBlueFrog) return;
        float dist = Vector2.Distance(GameManager.instance.player.transform.position, Vector3.zero);

        if (dist < 5.0f) blueFrogTimer = 0f;
        else
        {
            blueFrogTimer += Time.deltaTime;
            if (blueFrogTimer >= 30f)
            {
                Unlock(16);
                checkBlueFrog = false;
            }
        }
    }
}