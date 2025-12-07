using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("# Game Control")]
    public bool Living = true;
    public bool isLive;
    public float gameTime;
    public float maxGameTime = 30 * 10f; // 5min
    [Header("# Player Info")]
    public float health;
    public float maxHealth = 100;
    public int level;
    public int kill;
    public int exp;
    [Header("Level Info")]
    public int maxLevel = 45;
    public int[] nextExp; // 인스펙터에서 입력 X, 코드로 생성
    [Header("# Game Object")]
    public Player player;
    public GameObject boss;
    public Transform bossSpawnPoint;
    public GameObject tower;
    public GameObject elecTower;
    public GameObject fireTower;
    public GameObject groundTower;
    public GameObject iceTower;
    public GameObject chestPrefab;
    public PoolManager pool;
    public LvUp uiLevelUp;
    public SelectArtifact uiSelectArt;
    public ArrowController arrow;
    private bool arrowActivatedThisPhase = false;
    [Header("# Game Phase")]
    public List<TowerType> towerPhaseOrder;
    private int towerIndex = 0;
    public bool isTowerPhase = false;
    public bool isElecTowerPhase = false;
    public bool isFireTowerPhase = false;
    public bool isGroundTowerPhase = false;
    public bool isIceTowerPhase = false;
    public bool isBossPhase = false;
    public float bossPhaseStartTime = 720f;
    public float phaseTimer = 0f;
    public float normalPhaseDuration = 150f;
    public float towerPhaseDuration = 30f;

    // [변경] 보스방 입장 대기 상태 확인용 변수
    private bool isWaitingForBossTrigger = false;

    [Header("# Boss Cutscene Settings")]
    public CinemachineCamera virtualCam;
    public RectTransform topUIPanel;     // 상단 UI 묶음 (위로 사라짐)
    public RectTransform bottomUIPanel;  // 하단 UI 묶음 (아래로 사라짐)
    public float uiSlideDuration = 1.0f; // UI 사라지는 데 걸리는 시간

    [Header("# Boss Cutscene Effects")]
    // [추가] 보스 연출이 시작되었는지 체크하는 변수
    private bool isCutsceneStarted = false;
    public Tilemap backgroundTilemap;    // ★ 배경 타일맵 (색 바꿀 대상)
    public Color bossPhaseBgColor = new Color(0.6f, 0.4f, 0.8f); // 칙칙한 보라색
    public Transform shockwaveEffect;    // 중앙에서 퍼질 링 모양 충격파 (Sprite)
    public Transform magicCircle;
    public GameObject bossBarrier;       // 결계 오브젝트 (네모난 테두리)
    public GameObject bossSpawnEffect;   // 보스 등장 시 터질 파티클 (Explosion 등)

    [Header("# Player Control")]
    // ★ 여기에 스킬 관련 오브젝트(무기, 스캐너, 마법봉 등)를 다 넣으세요
    public GameObject[] skillObjects;
    public MonoBehaviour[] skillScripts;

    [Header("# Game Over UI")]
    public GameObject gameOverPanel;
    public Text gameOverReasonText;

    [Header("# UI Control")]
    public GameObject expBarObject;    // ★ 기존 경험치바 오브젝트 (Slider나 부모 오브젝트)
    public Slider bossHpSlider;        // ★ 새로 만든 보스 체력바 Slider

    void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;
    }
    
    void Start()
    {
        health = maxHealth;
        isLive = true;
        boss.SetActive(false);

        // 연출용 오브젝트 초기화
        if (bossBarrier != null) bossBarrier.SetActive(false);
        if (shockwaveEffect != null) shockwaveEffect.gameObject.SetActive(false);
        if (magicCircle != null) magicCircle.gameObject.SetActive(false);


        InitLevelData(); // 레벨 데이터 생성
        InitTowerPhaseOrder();
    }

    // 레벨업 테이블 자동 생성 함수
    void InitLevelData()
    {
        nextExp = new int[maxLevel + 1];
        nextExp[0] = 10; // 1레벨 가는데 10 필요

        // 설정값
        float baseExp = 10f;  // 기본 경험치
        float growth = 1.3f;  // 성장 계수 (1.1~1.5 추천)
                              // 1.1 : 엄청 빠름 (선형에 가까움)
                              // 1.3 : 뱀서 느낌 (추천)
                              // 1.5 : 약간 빡빡함

        for (int i = 1; i <= maxLevel; i++)
        {
            // 공식: 10 * (레벨 ^ 1.3)
            // 레벨이 오를수록 요구량이 늘어나지만, 
            // 몹 잡는 속도도 빨라지므로 체감상 템포는 유지됨.
            float expCalc = baseExp * Mathf.Pow(i + 1, growth);

            // 정수로 변환 시 5단위나 10단위로 끊어주면 깔끔함 (선택사항)
            nextExp[i] = Mathf.RoundToInt(expCalc);
        }
    }

    // 타워 순서 랜덤
    void InitTowerPhaseOrder()
    {
        towerPhaseOrder = new List<TowerType>()
        {
            TowerType.Electric,
            TowerType.Fire,
            TowerType.Ground,
            TowerType.Ice
        };

        // 셔플
        for (int i = 0; i < towerPhaseOrder.Count; i++)
        {
            int rand = Random.Range(i, towerPhaseOrder.Count);
            TowerType tmp = towerPhaseOrder[i];
            towerPhaseOrder[i] = towerPhaseOrder[rand];
            towerPhaseOrder[rand] = tmp;
        }

        Debug.Log("타워 페이즈 랜덤 순서: " +
            towerPhaseOrder[0] + " → " +
            towerPhaseOrder[1] + " → " +
            towerPhaseOrder[2] + " → " +
            towerPhaseOrder[3]);
    }

    void Update()
    {
        if (!isLive)
            return;

        gameTime += Time.deltaTime;

        if (gameTime > maxGameTime) {
            gameTime = maxGameTime;
        }

        // 페이즈 계산 로직
        phaseTimer += Time.deltaTime;

        if (gameTime > bossPhaseStartTime)
        {
            if (!isBossPhase)
            {
                if (!isCutsceneStarted)
                {
                    // 1. 유도 모드 시작
                    if (!isWaitingForBossTrigger)
                    {
                        Debug.Log("보스전 시간 도달! 소환 위치로 이동하세요.");
                        isWaitingForBossTrigger = true;

                        // 화살표를 보스 소환 위치로 활성화
                        if (bossSpawnPoint != null)
                        {
                            arrow.Activate(bossSpawnPoint, false);
                        }
                        isWaitingForBossTrigger = true;
                    }
                    // 2. 플레이어가 도착했는지 체크
                    else
                    {
                        CheckPlayerArrival();
                    }
                }
            }
            else
            {
                BossMonster bm = boss.GetComponent<BossMonster>();
                if (!bm.isLive)
                {
                    // 게임 승리로 끝

                    // ★ [추가] 보스 처치 관련 업적(스피드러너, 노히트 등) 체크
                    AchievementManager.instance.OnBossKilled();
                }
            }

        }
        else
        {
            // 10초 전 화살표 표시
            if (!isTowerPhase && !arrowActivatedThisPhase && phaseTimer >= normalPhaseDuration - 10f)
            {
                TowerType nextTower = towerPhaseOrder[towerIndex];
                Transform nextTowerTransform = GetTowerTransform(nextTower);
                arrow.Activate(nextTowerTransform, true);

                arrowActivatedThisPhase = true; // 다시 실행되지 않도록
            }

            if (!isTowerPhase && phaseTimer >= normalPhaseDuration)
            {
                // 타워 페이즈 진입
                isTowerPhase = true;
                phaseTimer = 0f;

                arrowActivatedThisPhase = false;

                if (towerIndex < towerPhaseOrder.Count)
                {
                    TowerType nextTower = towerPhaseOrder[towerIndex];
                    Debug.Log("타워 페이즈 시작: " + nextTower);
                    StartTowerPhase(nextTower);
                }
                else
                {
                    Debug.Log("모든 타워 페이즈 완료! (여기서 보스 등장 가능)");
                    isTowerPhase = false;
                    // SpawnBoss();
                }
            }
            else if (isTowerPhase && phaseTimer >= towerPhaseDuration)
            {
                // 타워 페이즈 종료
                isTowerPhase = false;
                phaseTimer = 0f;

                if (towerIndex < towerPhaseOrder.Count)
                {
                    TowerType finished = towerPhaseOrder[towerIndex];
                    EndTowerPhase(finished);
                    towerIndex++;
                }

                Debug.Log("타워 페이즈 종료 → 일반 페이즈 재개");
            }
        }
    }

    // 플레이어가 소환 위치에 도착했는지 확인
    void CheckPlayerArrival()
    {
        if (bossSpawnPoint == null) return;

        // 플레이어와 소환 지점 사이 거리 계산
        float distance = Vector2.Distance(player.transform.position, bossSpawnPoint.position);

        if (distance < 2.5f) // 조금 더 빡빡하게 도착 판정
        {
            if (isCutsceneStarted) return;
            isCutsceneStarted = true;

            isWaitingForBossTrigger = false;

            // ★ [추가] 도착하자마자 화살표 끄기
            arrow.Deactivate();

            StartCoroutine(CoAngryBossEntry());
        }
    }

    // ★★★ [최종 연출 시퀀스] ★★★
    IEnumerator CoAngryBossEntry()
    {
        Debug.Log("연출 시작: 보스 난입");
        arrow.Deactivate();

        // 0. 조작 제한
        if (player != null)
        {
            player.LockState(true);    // 이동 멈춤 + Stand 자세
            player.isInvincible = true; // ★ 무적 켜기 (몹들이 때려도 안 아픔)
        }

        // ★ [카메라] 1. Damping을 높여서(2.0) 아주 부드럽게 이동하게 변경
        SetCameraDamping(2.0f);

        // ★ [카메라] 1. 원래 보고 있던 대상(플레이어) 저장 & 보스 위치 바라보기
        Transform originalTarget = null;
        if (virtualCam != null)
        {
            originalTarget = virtualCam.Follow; // 원래 타겟(플레이어) 기억
            virtualCam.Follow = bossSpawnPoint; // 카메라는 이제 보스 소환 위치를 비춤
        }

        // 스킬 오브젝트, 스크립트 off
        foreach (var obj in skillObjects) { if (obj != null) obj.SetActive(false); }
        foreach (var script in skillScripts) { if (script != null) script.enabled = false; }

        // 1. UI 사라짐 & 배경색 변경 시작
        StartCoroutine(ChangeBackgroundColor(2.0f)); // 배경색 서서히 변경 (2초)
        yield return StartCoroutine(SlideUI(false)); // UI 사라짐 (1초)

        // ★ [핵심 추가] 카메라가 부드럽게 도착할 때까지 0.5초 정도 '여백의 미'를 줌
        // Damping을 높였으니 이동하는 데 시간이 걸리기 때문
        yield return new WaitForSeconds(0.5f);

        // 2. [마법진] 등장
        SpriteRenderer magicSprite = null;
        Color magicOriginalColor = Color.white;

        if (magicCircle != null)
        {
            magicCircle.position = bossSpawnPoint.position;
            magicCircle.localScale = Vector3.zero;
            magicCircle.gameObject.SetActive(true);

            // 알파값 초기화
            magicSprite = magicCircle.GetComponent<SpriteRenderer>();
            if (magicSprite != null)
            {
                Color c = magicSprite.color;
                c.a = 1f;
                magicSprite.color = c;
            }

            float magicDuration = 1.5f; // 1.5초 동안 생성
            float timer = 0f;

            while (timer < magicDuration)
            {
                timer += Time.deltaTime;
                float t = timer / magicDuration;

                // 크기: 0 -> 1.5배
                magicCircle.localScale = Vector3.one * Mathf.Lerp(0f, 1.5f, t);
                magicCircle.Rotate(0, 0, 180 * Time.deltaTime);// 회전

                yield return null;
            }

            // ★★★ B. [추가됨] 밝아지는 단계 (0.5초) ★★★
            // 에너지가 모이면서 하얗게 빛나고 회전 속도 빨라짐
            float brightenDuration = 0.5f;
            timer = 0f;

            // 목표: 완전 하얀색 (빛나는 느낌)
            Color brightColor = new Color(1f, 1f, 1f, 1f);

            while (timer < brightenDuration)
            {
                timer += Time.deltaTime;
                float t = timer / brightenDuration;

                // 원래색 -> 하얀색으로 변경 (점점 밝아짐)
                if (magicSprite != null)
                    magicSprite.color = Color.Lerp(magicOriginalColor, brightColor, t);

                // 회전 속도 3배 증가 (우웅~ 하는 느낌)
                magicCircle.Rotate(0, 0, 500 * Time.deltaTime);
                yield return null;
            }
        }

        // 3. [충격파] 잡몹 정리
        if (shockwaveEffect != null)
        {
            shockwaveEffect.position = bossSpawnPoint.position;
            shockwaveEffect.localScale = Vector3.zero;
            shockwaveEffect.gameObject.SetActive(true);

            // 잡몹 정리 실행
            ClearFieldMonsters();

            float duration = 0.5f; // 0.5초만에 팍! 커짐
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                shockwaveEffect.localScale = Vector3.one * Mathf.Lerp(0f, 60f, t); // 60배 확대
                // ★ 충격파 커지는 동안에도 마법진 계속 회전
                if (magicCircle != null) magicCircle.Rotate(0, 0, 180 * Time.deltaTime);

                yield return null;
            }
            shockwaveEffect.gameObject.SetActive(false);
        }
        else
        {
            ClearFieldMonsters();
        }

        // 4. [보스 등장 - 실루엣 효과]
        if (boss != null)
        {
            boss.transform.position = bossSpawnPoint.position;
            boss.SetActive(true);
            // ★ [추가] 보스 등장 시간 기록 (스피드러너 업적용)
            AchievementManager.instance.OnBossSpawn();

            BossMonster bm = boss.GetComponent<BossMonster>();
            if (bm != null) bm.BossInit(); // 초기화

            // 보스 스프라이트 가져오기
            SpriteRenderer bossSprite = boss.GetComponent<SpriteRenderer>();
            if (bossSprite != null)
            {
                // ★ 시작: 완전 검정색 (실루엣)
                bossSprite.color = Color.black;

                // 등장 이펙트 (폭발)
                if (bossSpawnEffect != null)
                    Instantiate(bossSpawnEffect, bossSpawnPoint.position, Quaternion.identity);

                StartCoroutine(ShakeCinemachine(0.5f, 3.0f)); // 쾅!

                // 검정색 -> 원래 색으로 1.5초 동안 서서히 돌아옴
                float colorDuration = 1.5f;
                float colorTimer = 0f;
                while (colorTimer < colorDuration)
                {
                    colorTimer += Time.deltaTime;
                    float t = colorTimer / colorDuration;

                    // Color.black에서 Color.white(기본)로 Lerp
                    bossSprite.color = Color.Lerp(Color.black, Color.white, t);
                    // 마법진: 계속 회전
                    if (magicCircle != null) magicCircle.Rotate(0, 0, 360 * Time.deltaTime);
                    yield return null;
                }
                // 확실하게 흰색으로 마무리
                bossSprite.color = Color.white;
            }
        }

        // 결계 생성
        if (bossBarrier != null) bossBarrier.SetActive(true);

        // ★ [변경점 3] 보스 폼 잡는 시간 (1초) 동안에도 마법진 회전 유지
        float waitDuration = 1.0f;
        float waitTimer = 0f;
        while (waitTimer < waitDuration)
        {
            waitTimer += Time.deltaTime;
            if (magicCircle != null) magicCircle.Rotate(0, 0, 180 * Time.deltaTime);
            yield return null;
        }

        // ★★★ [변경점 4] 드디어 여기서 마법진 사라짐! (전투 시작 직전) ★★★
        if (magicCircle != null)
        {
            // 1초 동안 부드럽게 사라짐 (전투 시작과 겹쳐서 자연스러움)
            StartCoroutine(FadeOutMagicCircle(magicCircle, 1.0f));
        }

        // 5. 전투 시작 & UI 복구
        isBossPhase = true;
        StartCoroutine(SlideUI(true));

        // UI가 슬라이드되어 내려온 뒤에 내용을 바꿉니다.
        // ★★★ [여기 추가] UI 모드 변경 (경험치바 -> 보스바) ★★★
        SwitchToBossUI(true);

        StartCoroutine(SlideUI(true));


        // ★ [카메라] 2. 다시 플레이어를 비추도록 복구
        if (virtualCam != null && originalTarget != null)
        {
            virtualCam.Follow = originalTarget;
            SetCameraDamping(0f); // 다시 빠릿빠릿하게
        }

        // ★ [조작 해제] 플레이어 다시 움직이게 하기 & 스킬 복구
        if (player != null)
        {
            player.LockState(false);
            player.isInvincible = false; // 이제 맞으면 아픔
        }
        // 오브젝트 및 스크립트 활성화
        foreach (var obj in skillObjects) { if (obj != null) obj.SetActive(true); }
        foreach (var script in skillScripts) { if (script != null) script.enabled = true; }

        if (boss != null)
        {
            BossMonster bm = boss.GetComponent<BossMonster>();
            if (bm != null)
            {
                bm.StartBattle(); // 이 함수가 실행되어야 isBattleReady = true가 됨
            }
        }
    }

    // ★★★ 마법진 페이드 아웃 함수 ★★★
    IEnumerator FadeOutMagicCircle(Transform target, float duration)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            target.gameObject.SetActive(false);
            yield break;
        }

        Color startColor = sr.color;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // 투명도: 1 -> 0
            float newAlpha = Mathf.Lerp(1f, 0f, t);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);

            // 사라지는 동안에도 회전 (자연스러움)
            target.Rotate(0, 0, 180 * Time.deltaTime);

            yield return null;
        }

        // 다 사라지면 끄기
        target.gameObject.SetActive(false);

        // ★ 다음번 실행을 위해 알파값 복구 (중요)
        sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
    }

    // 배경색 변경 코루틴
    IEnumerator ChangeBackgroundColor(float duration)
    {
        if (backgroundTilemap == null) yield break;

        Color startColor = Color.white;
        Color endColor = bossPhaseBgColor; // 칙칙한 보라색
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            backgroundTilemap.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        backgroundTilemap.color = endColor;
    }


    // UI를 부드럽게 밀어내는 함수
    IEnumerator SlideUI(bool show)
    {
        float timer = 0f;
        Vector2 startTop = topUIPanel.anchoredPosition;
        Vector2 startBottom = bottomUIPanel.anchoredPosition;

        // 목표 위치 설정 (show가 true면 원위치(0), false면 화면 밖으로)
        // 화면 높이(1080 가정)보다 좀 더 밀어버림 (+300)
        Vector2 targetTop = show ? new Vector2(0, 0) : new Vector2(0, 300);
        Vector2 targetBottom = show ? new Vector2(0, 0) : new Vector2(0, -300);

        // 만약 현재 위치가 이미 목표라면 패스 (안전장치)
        if (!show && startTop.y > 100) { yield break; }

        while (timer < uiSlideDuration)
        {
            timer += Time.deltaTime;
            float t = timer / uiSlideDuration;

            // SmoothStep: 부드럽게 출발해서 부드럽게 도착
            t = t * t * (3f - 2f * t);

            if (topUIPanel != null)
                topUIPanel.anchoredPosition = Vector2.Lerp(startTop, targetTop, t);

            if (bottomUIPanel != null)
                bottomUIPanel.anchoredPosition = Vector2.Lerp(startBottom, targetBottom, t);

            yield return null;
        }
    }

    // 카메라 흔들기
    IEnumerator ShakeCinemachine(float duration, float intensity)
    {
        if (virtualCam == null) yield break;

        var perlin = virtualCam.GetComponent<CinemachineBasicMultiChannelPerlin>();

        if (perlin != null)
        {
            perlin.AmplitudeGain = intensity;
            yield return new WaitForSeconds(duration);
            perlin.AmplitudeGain = 0f;
        }
    }

    // 필드에 있는 잡몹들 제거 (보스 등장 시 방해 안 되게)
    void ClearFieldMonsters()
    {
        // "Enemy" 태그를 가진 모든 오브젝트를 찾아서 제거 (태그 설정 필요)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemyObj in enemies)
        {
            // ★ [핵심 1] 보스는 절대 건드리면 안 됨! (안전장치)
            if (enemyObj == boss) continue;

            MonsterBase monster = enemyObj.GetComponent<MonsterBase>();
            
            if (monster != null)
            {
                if (monster.isLive)
                {
                    monster.Die(false);
                }
            }
            else
            {
                Destroy(enemyObj);
            }
        }
        Debug.Log("필드 몬스터 정리 완료");
    }

    void StartTowerPhase(TowerType type)
    {
        switch (type)
        {
            case TowerType.Electric:
                isElecTowerPhase = true;
                tower = elecTower;
                elecTower.GetComponent<Tower>().OnTowerPhaseStart();
                
                break;
            case TowerType.Fire:
                isFireTowerPhase = true;
                tower = fireTower;
                fireTower.GetComponent<Tower>().OnTowerPhaseStart();
                break;
            case TowerType.Ground:
                isGroundTowerPhase = true;
                tower = groundTower;
                groundTower.GetComponent<Tower>().OnTowerPhaseStart();
                break;
            case TowerType.Ice:
                isIceTowerPhase = true;
                tower = iceTower;
                iceTower.GetComponent<Tower>().OnTowerPhaseStart();
                break;
        }
    }

    void EndTowerPhase(TowerType type)
    {
        switch (type)
        {
            case TowerType.Electric:
                elecTower.GetComponent<Tower>().OnTowerPhaseEnd();
                break;
            case TowerType.Fire:
                fireTower.GetComponent<Tower>().OnTowerPhaseEnd();
                break;
            case TowerType.Ground:
                groundTower.GetComponent<Tower>().OnTowerPhaseEnd();
                break;
            case TowerType.Ice:
                iceTower.GetComponent<Tower>().OnTowerPhaseEnd();
                break;
        }
        arrow.Deactivate();
        // ★ [추가] 업적 매니저에게 타워 방어 성공 알림
        AchievementManager.instance.OnTowerDefended();
    }

    Transform GetTowerTransform(TowerType type)
    {
        switch (type)
        {
            case TowerType.Electric: return elecTower.transform;
            case TowerType.Fire: return fireTower.transform;
            case TowerType.Ground: return groundTower.transform;
            case TowerType.Ice: return iceTower.transform;
        }
        return null;
    }

    public void GetExp(int amount)
    {
        if (!isLive) return;

        Debug.Log($"[경험치 획득] 들어온 양: {amount} | 현재 EXP: {exp} | 목표 EXP: {nextExp[level]}");

        if (level >= maxLevel) return; // 만렙이면 경험치 안 먹음

        exp += amount;
        /*
        if (exp == nextExp[level])
        {
            level++;
            exp = 0;
            uiLevelUp.Show();
        }
        */
        // 경험치 통 꽉 찼으면 레벨업 (반복문인 이유는 한번에 2업 할 수도 있어서)
        while (exp >= nextExp[level])
        {
            exp -= nextExp[level];
            level++;
            uiLevelUp.Show();

            if (level >= maxLevel)
            {
                exp = 0; // 만렙 경험치바 처리
                break;
            }
        }
    }

    public void Stop()
    {
        Living = false;
        Time.timeScale = 0f;
    }
    public void Resume()
    {
        Living = true;
        Time.timeScale = 1f;
    }

    public void OnTowerDefenseSuccess()
    {
        Instantiate(chestPrefab, tower.transform.position + Vector3.right * 2f, Quaternion.identity);
    }

    // ★ [신규] 카메라 부드러움(Damping) 조절 함수
    void SetCameraDamping(float dampingValue)
    {
        if (virtualCam == null) return;

        var composer = virtualCam.GetComponent<CinemachinePositionComposer>();

        if (composer != null)
        {
            // 신버전에서는 Damping이 Vector3로 통합되었습니다.
            composer.Damping = new Vector3(dampingValue, dampingValue, 0);
        }
        else
        {
            // 혹시 "Follow" 모드를 쓰고 있다면 이걸로 잡아야 합니다.
            // (Position Control이 "Follow"일 경우)
            var follow = virtualCam.GetComponent<CinemachineFollow>();
            if (follow != null)
            {
                follow.TrackerSettings.PositionDamping = new Vector3(dampingValue, dampingValue, 0);
            }
        }
    }

    // ★ [수정] 실패 원인을 텍스트로 받음
    public void GameOver(string reason, bool isTowerDeath = false)
    {
        isLive = false;
        if (isTowerDeath)
        {
            // 타워가 터졌을 때 (줌인, 시간 정지 연출)
            StartCoroutine(CoTowerDeathRoutine(reason));
        }
        else
        {
            // 플레이어가 죽었을 때 (카메라 고정, 조작/스킬 잠금)
            StartCoroutine(CoPlayerDeathRoutine(reason));
        }
    }
    // [경로 1] 플레이어 사망 연출 (담백하게)
    IEnumerator CoPlayerDeathRoutine(string reason)
    {
        // 1. 플레이어 조작/이동 잠금 & 무적
        if (player != null)
        {
            player.LockState(true);      // 이동/공격 불가
            player.isInvincible = true;  // 추가 피격 방지

            // 미끄러짐 방지 (완전 정지)
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null) playerRb.linearVelocity = Vector2.zero;
        }

        // 2. 스킬 끄기 (공격 중단)
        DisableAllSkills();

        // 3. 사망 모션을 봐야 하므로 시간은 바로 멈추지 않고 2초 정도 대기
        // (Time.timeScale은 1 상태 유지)
        yield return new WaitForSeconds(2.0f);

        // 4. 이제 UI 띄우고 시간 정지
        ShowGameOverUI(reason);
    }

    // [경로 2] 타워 파괴 연출 (화려하게)
    IEnumerator CoTowerDeathRoutine(string reason)
    {
        // 1. 시간 정지
        Time.timeScale = 0f;

        // ★ [수정 1] 'UnscaledTime' 대신 'ManualUpdate'로 설정
        // "이제부터 카메라는 내가 수동으로 업데이트한다!" 라고 선언
        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain != null)
        {
            brain.UpdateMethod = CinemachineBrain.UpdateMethods.ManualUpdate;
        }

        if (virtualCam != null && tower != null)
        {
            Transform originalTarget = virtualCam.Follow;
            virtualCam.Follow = tower.transform;

            var lensSettings = virtualCam.Lens;

            float startSize = lensSettings.OrthographicSize;
            float targetSize = 2.5f;
            float duration = 1.0f;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float t = timer / duration;
                t = t * (2f - t);

                lensSettings.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);
                virtualCam.Lens = lensSettings;

                // ★ [수정 2] 여기서 강제로 카메라를 갱신시킴
                // 시간이 0이어도 수동으로 "카메라야 일해라!" 하고 명령하는 코드
                if (brain != null) brain.ManualUpdate();

                yield return null;
            }
            lensSettings.OrthographicSize = targetSize;
            virtualCam.Lens = lensSettings;

            // 마지막으로 한 번 더 갱신해서 확실하게 맞춤
            if (brain != null) brain.ManualUpdate();
        }

        if (tower != null)
        {
            // 타워 스크립트에 있는 파괴 함수 호출
            tower.GetComponent<Tower>().PlayDestructionEffect();
        }

        // 3. 감상 타임
        yield return new WaitForSecondsRealtime(2.3f);

        // 4. UI 띄우기
        ShowGameOverUI(reason);
    }

    // [공통] 스킬 끄기 함수 분리
    void DisableAllSkills()
    {
        foreach (var obj in skillObjects) { if (obj != null) obj.SetActive(false); }
        foreach (var script in skillScripts) { if (script != null) script.enabled = false; }
    }

    // [공통] 게임오버 UI 표시 함수 분리
    void ShowGameOverUI(string reason)
    {
        Time.timeScale = 0f; // 확실하게 정지

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverReasonText != null)
            {
                gameOverReasonText.text = reason;
            }
        }
    }

    // ★ 보스전 UI 모드로 전환하는 함수
    public void SwitchToBossUI(bool isBossMode)
    {
        // 보스전이면 -> 경험치바 끄고, 보스바 켜기
        if (expBarObject != null)
            expBarObject.SetActive(!isBossMode);

        if (bossHpSlider != null)
        {
            bossHpSlider.gameObject.SetActive(isBossMode);
            // 켜질 때 체력 꽉 찬 상태로 초기화
            if (isBossMode) bossHpSlider.value = 1f;
        }
    }

    // ★ 보스 체력 업데이트 함수 (보스가 맞을 때마다 호출)
    public void UpdateBossHealthUI(float currentHp, float maxHp)
    {
        if (bossHpSlider == null) return;
        bossHpSlider.value = currentHp / maxHp;
    }

    // ★ [추가] 재시작 함수 (버튼에 연결할 것)
    public void Retry()
    {
        // 시간 다시 흐르게 하기 (중요! 안 하면 재시작해도 멈춰있음)
        Time.timeScale = 1f;

        // 현재 씬 다시 로드 (초기화)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoMain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainScreen_jw");
    }
}

public enum TowerType
{
    Electric,
    Fire,
    Ground,
    Ice
}