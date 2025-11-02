using UnityEngine;

public class GameM : MonoBehaviour
{
    public static GameM instance;
    public Play player;
    public PoolM pool;
    void Awake()
    {
        instance = this;
    }
}
