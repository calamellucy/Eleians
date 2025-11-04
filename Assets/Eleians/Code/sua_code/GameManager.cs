using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject Circle; // test
    public TestPlayer1 player; // test
    public PoolManager pool;
    void Awake()
    {
        instance = this;
    }
}
