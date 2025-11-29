using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float damage;
    public float speed = 10f;
    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Init(float dmg, Vector3 dir)
    {
        damage = dmg;
        rigid.linearVelocity = dir * speed; // Unity 6 (구 velocity)

        // 5초 뒤 자동 삭제 (메모리 관리)
        Destroy(gameObject, 5f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 플레이어 피격 로직
            Player player = collision.GetComponent<Player>();
            if (player != null) player.ApplyDamage(damage);

            Destroy(gameObject); // 맞으면 사라짐
        }
    }
}