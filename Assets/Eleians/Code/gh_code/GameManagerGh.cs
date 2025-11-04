using UnityEngine;

public class GameManagerGh : MonoBehaviour
{
    public static GameManagerGh instance;
    public PlayerGh player;
    public PoolManagerGh pool;
    void Awake()
    {
        instance = this;
    }
}
