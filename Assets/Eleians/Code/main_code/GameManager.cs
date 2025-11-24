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
    public GameObject tower;
    public GameObject chestPrefab;
    public PoolManager pool;
    public LvUp uiLevelUp;
    public SelectArtifact uiSelectArt;
    [Header("# Game Phase")]
    public bool isTowerPhase = false;
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
    }

    void Update()
    {
        if (!isLive)
            return;

        gameTime += Time.deltaTime;

        if (gameTime > maxGameTime) {
            gameTime = maxGameTime;
        }

        // ������ ��� ����
        phaseTimer += Time.deltaTime;

        if (!isTowerPhase && phaseTimer >= normalPhaseDuration)
        {
            isTowerPhase = true;
            phaseTimer = 0f;
            Debug.Log("���� ������ ����!");
            tower.GetComponent<Tower>().OnTowerPhaseStart(); // ȣ��
        }
        else if (isTowerPhase && phaseTimer >= towerPhaseDuration)
        {
            isTowerPhase = false;
            phaseTimer = 0f;
            Debug.Log("���� ������ ����, �Ϲ� ������ �簳!");
            tower.GetComponent<Tower>().OnTowerPhaseEnd();   // ȣ��
        }
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

}
