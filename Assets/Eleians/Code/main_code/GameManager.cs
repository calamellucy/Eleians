using System.Collections.Generic;
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

    // ★★★ [추가] 레벨업 테이블 자동 생성 함수
    void InitLevelData()
    {
        nextExp = new int[maxLevel + 1];

        // [수정] 0레벨(시작) -> 1레벨로 갈 때 필요한 경험치 설정
        nextExp[0] = 10; // 예: 10xp 모으면 1레벨 됨

        for (int i = 1; i <= maxLevel; i++)
        {
            // 밸런스 공식 (필요하면 숫자를 조절하세요)
            // Lv 1->2 : 12 XP (몹 4~5마리)
            // Lv 10->11 : 310 XP
            // Lv 40->41 : 3600 XP (후반엔 몹이 쏟아지므로 적당함)
            nextExp[i] = 10 + (i * 10) + (i * i * 2);
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
                isBossPhase = true;
                SpawnBoss();
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
        arrow.Deactivate();
        arrowActivatedThisPhase = false;

        if (boss == null) return;

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