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

    // Å¸¿ö ¼ø¼­ ·£´ý
    void InitTowerPhaseOrder()
    {
        towerPhaseOrder = new List<TowerType>()
        {
            TowerType.Electric,
            TowerType.Fire,
            TowerType.Ground,
            TowerType.Ice
        };

        // ¼ÅÇÃ
        for (int i = 0; i < towerPhaseOrder.Count; i++)
        {
            int rand = Random.Range(i, towerPhaseOrder.Count);
            TowerType tmp = towerPhaseOrder[i];
            towerPhaseOrder[i] = towerPhaseOrder[rand];
            towerPhaseOrder[rand] = tmp;
        }

        Debug.Log("Å¸¿ö ÆäÀÌÁî ·£´ý ¼ø¼­: " +
            towerPhaseOrder[0] + " ¡æ " +
            towerPhaseOrder[1] + " ¡æ " +
            towerPhaseOrder[2] + " ¡æ " +
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

        // ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
        phaseTimer += Time.deltaTime;

        /*
        if (!isTowerPhase && phaseTimer >= normalPhaseDuration)
        {
            isTowerPhase = true;
            phaseTimer = 0f;
            Debug.Log("ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½!");
            tower.GetComponent<Tower>().OnTowerPhaseStart(); // È£ï¿½ï¿½
        }
        else if (isTowerPhase && phaseTimer >= towerPhaseDuration)
        {
            isTowerPhase = false;
            phaseTimer = 0f;
            Debug.Log("ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½, ï¿½Ï¹ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ç°³!");
            tower.GetComponent<Tower>().OnTowerPhaseEnd();   // È£ï¿½ï¿½
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
                    // °ÔÀÓ ½Â¸®·Î ³¡
                }
            }

        }
        else
        {
            // 10ÃÊ Àü È­»ìÇ¥ Ç¥½Ã
            if (!isTowerPhase && !arrowActivatedThisPhase && phaseTimer >= normalPhaseDuration - 10f)
            {
                TowerType nextTower = towerPhaseOrder[towerIndex];
                Transform nextTowerTransform = GetTowerTransform(nextTower);
                arrow.Activate(nextTowerTransform);

                arrowActivatedThisPhase = true; // ´Ù½Ã ½ÇÇàµÇÁö ¾Êµµ·Ï
            }

            if (!isTowerPhase && phaseTimer >= normalPhaseDuration)
            {
                // Å¸¿ö ÆäÀÌÁî ÁøÀÔ
                isTowerPhase = true;
                phaseTimer = 0f;

                arrowActivatedThisPhase = false;

                if (towerIndex < towerPhaseOrder.Count)
                {
                    TowerType nextTower = towerPhaseOrder[towerIndex];
                    Debug.Log("Å¸¿ö ÆäÀÌÁî ½ÃÀÛ: " + nextTower);
                    StartTowerPhase(nextTower);
                }
                else
                {
                    Debug.Log("¸ðµç Å¸¿ö ÆäÀÌÁî ¿Ï·á! (¿©±â¼­ º¸½º µîÀå °¡´É)");
                    isTowerPhase = false;
                    // SpawnBoss();
                }
            }
            else if (isTowerPhase && phaseTimer >= towerPhaseDuration)
            {
                // Å¸¿ö ÆäÀÌÁî Á¾·á
                isTowerPhase = false;
                phaseTimer = 0f;

                if (towerIndex < towerPhaseOrder.Count)
                {
                    TowerType finished = towerPhaseOrder[towerIndex];
                    EndTowerPhase(finished);
                    towerIndex++;
                }

                Debug.Log("Å¸¿ö ÆäÀÌÁî Á¾·á ¡æ ÀÏ¹Ý ÆäÀÌÁî Àç°³");
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

        boss.SetActive(true);  // º¸½º ¹ÖºÎ±â È°¼ºÈ­

        BossMonster bm = boss.GetComponent<BossMonster>();
        if (bm != null)
            bm.BossInit();     // HP, µ¥¹ÌÁö, ¼Óµµ µî ÃÊ±âÈ­
    }

}

public enum TowerType
{
    Electric,
    Fire,
    Ground,
    Ice
}