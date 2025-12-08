using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float damage;
    public float speed = 10f;
    private string targetTag;
    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Init(float dmg, Vector3 dir, string tag)
    {
        damage = dmg;
        rigid.linearVelocity = dir * speed;
        targetTag = tag; // 목표 저장

        // 5초 뒤 자동 삭제 (메모리 관리)
        Destroy(gameObject, 5f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // [핵심 로직] 지정된 타겟하고 부딪혔을 때만 반응!
        if (collision.CompareTag(targetTag))
        {
            // 1. 타겟이 플레이어라면
            if (targetTag == "Player")
            {
                Player player = collision.GetComponent<Player>();
                if (player != null) player.ApplyDamage(damage);
            }
            // 2. 타겟이 타워라면
            else if (targetTag == "Tower")
            {
                Tower tower = collision.GetComponent<Tower>();
                if (tower != null) tower.TakeDamage(damage);
            }

            // 임무 완수했으니 삭제
            Destroy(gameObject);
        }

        // (선택사항) 벽에는 누가 쏘든 막히게 하고 싶다면?
        // if (collision.CompareTag("Wall")) Destroy(gameObject);
    }
}