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
    public int[] nextExp = {5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 , 5, 5, 5};
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

        InitTowerPhaseOrder();
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

        /*
        if (!isTowerPhase && phaseTimer >= normalPhaseDuration)
        {
            isTowerPhase = true;
            phaseTimer = 0f;
            Debug.Log("거점 페이즈 시작!");
            tower.GetComponent<Tower>().OnTowerPhaseStart(); // 호출
        }
        else if (isTowerPhase && phaseTimer >= towerPhaseDuration)
        {
            isTowerPhase = false;
            phaseTimer = 0f;
            Debug.Log("거점 페이즈 종료, 일반 페이즈 재개!");
            tower.GetComponent<Tower>().OnTowerPhaseEnd();   // 호출
        }
        */

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

    public void GetExp()
    {
        exp++;
        if(exp == nextExp[level])
        {
            level++;
            exp = 0;
            uiLevelUp.Show();
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