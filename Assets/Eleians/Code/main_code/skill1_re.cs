using UnityEngine;

public class skill1_re : MonoBehaviour
{
    public int id;
    public int prefabId;
    public float damage;
    public int count;
    public float attackRate;

    float timer;
    Player player;

    void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    void Start()
    {
        Init();
    }

    void Update()
    {
        switch (id)
        {
            case 0:
                break;
            default:
                timer += Time.deltaTime;

                if (timer >= 1f / attackRate)
                {
                    timer = 0f;
                    TryFire();
                    Debug.Log("Fire»£√‚");
                }
                break;

        }
    }

    public void Init()
    {
        switch (id)
        {
            default:
                attackRate = 2f;
                break;
        }
    }

    void TryFire()
    {
        Debug.Log($"[TryFire] player={player}, scanner={(player ? player.scanner : null)}, target={(player && player.scanner ? player.scanner.nearestTarget : null)}");

        if (!player.scanner.nearestTarget)
            return;

        Vector3 targetPos = player.scanner.nearestTarget.position;
        Vector3 dir = targetPos - transform.position;
        dir = dir.normalized;

        Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
        bullet.position = transform.position;
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        bullet.GetComponent<bullet_re>().Init(damage, count, dir);
    }
}
