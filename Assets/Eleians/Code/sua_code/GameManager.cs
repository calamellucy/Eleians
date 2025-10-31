using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject Circle;
    public PoolManager pool;
    void Awake()
    {
        instance = this;
    }
}
