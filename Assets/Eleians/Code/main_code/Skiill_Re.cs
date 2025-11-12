using UnityEngine;
using System.Collections;

public class Skill1_Re : MonoBehaviour
{
    [Header("Skill Base")]
    public int id;
    public int prefabId;
    public float damage = 10f;
    public int count = 1;
    public float attackRate = 2f;
    public float projectileSize = 0.2f;

    [Header("Evolution State")]
    public int electricCount = 0; // 전기 원소 개수

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
        timer += Time.deltaTime;
        if (timer >= 1f / attackRate)
        {
            timer = 0f;
            TryFire();
        }
    }

    public void Init()
    {
        attackRate = 2f;
        damage = 10f;
        count = 1;
        projectileSize = 0.2f;
    }

    void TryFire()
    {
        if (!player.scanner.nearestTarget)
            return;

        Vector3 targetPos = player.scanner.nearestTarget.position;
        Vector3 dir = (targetPos - transform.position).normalized;

        // 진화 단계 반영
        int projectileCount = 1;
        int per = count;
        if (electricCount >= 5)
        {
            projectileCount += 2; // 투사체 +2
            per += 1;             // 관통 +1
        }

        // 여러 발 동시 발사
        float spreadAngle = 10f;
        for (int i = 0; i < projectileCount; i++)
        {
            float angleOffset = (i - (projectileCount - 1) / 2f) * spreadAngle;
            Vector3 newDir = Quaternion.Euler(0, 0, angleOffset) * dir;

            Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
            bullet.position = transform.position;
            bullet.rotation = Quaternion.FromToRotation(Vector3.up, newDir);
            bullet.localScale = Vector3.one * projectileSize;

            BulletEvolution evo = bullet.GetComponent<BulletEvolution>();
            if (evo != null)
                evo.Setup(this);

            bullet.GetComponent<Bullet_Re>().Init(damage, per, newDir);
        }
    }
}
