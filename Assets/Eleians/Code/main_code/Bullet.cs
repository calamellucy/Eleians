using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public int per;

    Rigidbody2D rigid;
    public StoneDust stoneDust;   // ← 추가

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Init(float damage, int per, Vector3 dir)
    {
        this.damage = damage;
        this.per = per;

        if (per > -1)
        {
            rigid.linearVelocity = dir;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy"))
            return;

        NormalMonster monster = collision.GetComponent<NormalMonster>();
        if (monster != null)
        {
            monster.ApplyDamage(damage);
        }

        // 🔹 흙 카운트가 10 초과면 폭발 소환
        if (StatsManager.instance != null && StatsManager.instance.EarthCnt >= 10)
        {
            if (stoneDust != null)
            {
                // 총알이 날아가던 방향 기준
                Vector2 dir = rigid.linearVelocity.normalized; ;

                stoneDust.SpawnExplosion(transform.position, dir);
            }
        }

        // 관통 횟수 감소
        per--;

        // 관통이 남아 있으면 탄은 계속 날아감
        if (per >= 0)
        {
            GetComponent<BulletEvolution>()?.TriggerEvolution();
            return;
        }

        // 관통이 끝났을 때만 종료
        rigid.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
    }
}
