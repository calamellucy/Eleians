using UnityEngine;

public class GameManagerSu : MonoBehaviour
{
    public static GameManagerSu instance;
    public GameObject Circle; // test
    public PlayerSu player; // test
    public PoolManagerSu pool;
    void Awake()
    {
        instance = this;
    }
}
