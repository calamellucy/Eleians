using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

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
    public float bossZoomSize = 8f; // 보스전 때 줌아웃 할 사이즈 (기존 사이즈가 5라면 8~10 정도 추천)
    private float originCamSize; // 원래 카메라 사이즈 저장용

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
                // 1. 아직 "위치 도달 대기" 상태가 아니라면 -> 유도 모드 시작
                if (!isWaitingForBossTrigger)
                {
                    Debug.Log("보스전 시간 도달! 소환 위치로 이동하세요.");
                    isWaitingForBossTrigger = true;

                    // 화살표를 보스 소환 위치로 활성화
                    if (bossSpawnPoint != null)
                    {
                        arrow.Activate(bossSpawnPoint);
                    }
                }
                // 2. 유도 모드 중이라면 -> 플레이어가 도착했는지 체크
                else
                {
                    CheckPlayerArrival();
                }
            }
            else
            {
                BossMonster bm = boss.GetComponent<BossMonster>();
                if (!bm.isLive)
                {
                    // 게임 승리로 끝
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
                arrow.Activate(nextTowerTransform);

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

    // [신규] 플레이어가 소환 위치에 도착했는지 확인
    void CheckPlayerArrival()
    {
        if (bossSpawnPoint == null) return;

        // 플레이어와 소환 지점 사이 거리 계산
        float distance = Vector2.Distance(player.transform.position, bossSpawnPoint.position);

        if (distance < 2.5f) // 조금 더 빡빡하게 도착 판정
        {
            isWaitingForBossTrigger = false;

            // [핵심] 그냥 SpawnBoss()를 부르는 게 아니라 연출 코루틴을 시작
            StartCoroutine(CoBossSequence());
        }
    }

    // [신규] 보스 등장 시네마틱 코루틴
    IEnumerator CoBossSequence()
    {
        // 1. 안전 확보 및 초기화
        Debug.Log("연출 시작: 플레이어 조작 금지 & 무적");
        // player.inputEnabled = false; // (플레이어 이동 스크립트에 조작 멈추는 기능이 있다면 호출)
        // player.isInvincible = true;  // (플레이어 무적 기능이 있다면 호출)
        arrow.Deactivate(); // 화살표 제거

        // ★ [줌아웃 해결 1] Lens는 구조체(Struct)라 이렇게 값을 받아와야 합니다.
        var currentLens = virtualCam.Lens;
        originCamSize = currentLens.OrthographicSize;

        // 2. 쾅! 임팩트 (화면 흔들림 + 잡몹 증발)
        StartCoroutine(ShakeCinemachine(2.0f, 5.0f)); // 지속시간 2초 강도 5.0f
        yield return new WaitForSeconds(0.5f);
        ClearFieldMonsters(); // 흔들리는 순간 몬스터들이 펑! 하고 사라짐

        yield return new WaitForSeconds(1f); // 흔들리는 시간동안 대기

        // 5. 이제 흔들림이 멈추고 줌아웃 시작 (1.5초간)
        float time = 0f;
        float duration = 1.5f;

        // 시작값은 저장해둔 원래 사이즈
        float startSize = originCamSize;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // 부드러운 보간
            float newSize = Mathf.Lerp(startSize, bossZoomSize, t);

            // ★ [줌아웃 해결 핵심 코드]
            // 1. 구조체를 꺼낸다
            var tempLens = virtualCam.Lens;
            // 2. 값을 바꾼다
            tempLens.OrthographicSize = newSize;
            // 3. 다시 집어넣는다 (이래야 적용됨!)
            virtualCam.Lens = tempLens;

            yield return null;
        }

        // 혹시 모르니 최종값 한번 더 강제 적용
        var finalLens = virtualCam.Lens;
        finalLens.OrthographicSize = bossZoomSize;
        virtualCam.Lens = finalLens;

        // 4. 보스 소환
        SpawnBoss(); // 보스 활성화

        // 5. 보스전 시작 (게임 재개)
        isBossPhase = true;

        // player.inputEnabled = true; // 플레이어 조작 재개
        // player.isInvincible = false; // 무적 해제 (필요시)
    }

    // 카메라 흔들기
    IEnumerator ShakeCinemachine(float duration, float intensity)
    {
        if (virtualCam == null) yield break;

        // ★ [변경 4] GetCinemachineComponent 대신 그냥 GetComponent 사용
        var perlin = virtualCam.GetComponent<CinemachineBasicMultiChannelPerlin>();

        if (perlin != null)
        {
            perlin.AmplitudeGain = intensity; // m_AmplitudeGain -> AmplitudeGain (m_ 빠짐)

            yield return new WaitForSeconds(duration);

            perlin.AmplitudeGain = 0f;
        }
    }

    // [신규] 필드에 있는 잡몹들 제거 (보스 등장 시 방해 안 되게)
    void ClearFieldMonsters()
    {
        // "Enemy" 태그를 가진 모든 오브젝트를 찾아서 제거 (태그 설정 필요)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObj in enemies)
        {
            MonsterBase monster = enemyObj.GetComponent<MonsterBase>();
            if (monster != null && monster.isLive)
                monster.Die(false);
            else
                Destroy(enemyObj);
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

    void SpawnBoss()
    {
        Debug.Log("플레이어 도착! 보스 소환 시작!");
        arrow.Deactivate();
        arrowActivatedThisPhase = false;

        if (boss == null) return;

        // 여기서 카메라 줌아웃이나 컷씬 코루틴을 실행하면 좋습니다.
        // 지금은 즉시 활성화
        boss.transform.position = bossSpawnPoint.position; // 보스 위치를 소환 지점으로 강제 이동
        boss.SetActive(true);  // 보스 밍부기 활성화

        BossMonster bm = boss.GetComponent<BossMonster>();
        if (bm != null)
            bm.BossInit();     // HP, 데미지, 속도 등 초기화
    }

}

public enum TowerType
{
    Electric,
    Fire,
    Ground,
    Ice
}