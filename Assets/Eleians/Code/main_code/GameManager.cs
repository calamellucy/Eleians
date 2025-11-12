using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("# Game Control")]
<<<<<<< Updated upstream
=======
    public bool Living = true;
    public bool isLive;
>>>>>>> Stashed changes
    public float gameTime;
    [Header("# Player Info")]
    public int health;
    public int maxHealth = 100;
    public int level;
    public int kill;
    public int exp;
    public int[] nextExp = { 2, 3, 4, 5, 6, 6, 6, 6, 7, 100000 };
    [Header("# Game Object")]
    public Player player;
    public PoolManager pool;
<<<<<<< Updated upstream
=======
    public LvUp uiLevelUp;
    [Header("# Game Phase")]
    public bool isTowerPhase = false;
    public float phaseTimer = 0f;
    public float normalPhaseDuration = 60f;
    public float towerPhaseDuration = 30f;

>>>>>>> Stashed changes
    void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        health = maxHealth;
    }

    void Update()
    {
<<<<<<< Updated upstream
=======
        if (!Living)
            return;

        if (!isLive)
            return;

>>>>>>> Stashed changes
        gameTime += Time.deltaTime;
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
}
