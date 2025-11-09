using UnityEngine;

public class BulletHoming : MonoBehaviour
{
    [Header("Stat")]
    public float damage = 10f;
    public int per = 1;
    public float speed = 10f;

    Transform target;
    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void SetTarget(Transform t)
    {
        target = t;

        // 초기 발사 방향 설정
        if (t != null)
        {
            Vector2 dir = ((Vector2)t.position - rigid.position).normalized;
            rigid.linearVelocity = dir * speed;
        }
    }

    void FixedUpdate()
    {
        // 타겟이 죽었거나 비활성화되면 기존 방향으로 직진
        if (target == null || !target.gameObject.activeSelf)
        {
            target = null;
            rigid.linearVelocity = rigid.linearVelocity.normalized * speed;
            return;
        }

        // 살아있는 타겟 추적
        Vector2 dir = ((Vector2)target.position - rigid.position).normalized;
        rigid.linearVelocity = dir * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy"))
            return;

        NormalMonster monster = collision.GetComponent<NormalMonster>();
        if (monster != null && monster.gameObject.activeSelf)
            monster.health -= damage;

        per--;

        if (per < 0)
            gameObject.SetActive(false);
    }

    void OnDisable()
    {
        if (rigid != null)
            rigid.linearVelocity = Vector2.zero;
        target = null;
    }
}
