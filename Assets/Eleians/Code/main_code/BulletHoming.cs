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
        UpdateVelocity();
    }

    void FixedUpdate()
    {
        UpdateVelocity();
    }

    void UpdateVelocity()
    {
        if (rigid == null) return;

        // 타겟이 없거나 죽었으면 직진 유지
        if (target == null || !target.gameObject.activeSelf)
        {
            rigid.linearVelocity = rigid.linearVelocity.normalized * speed;
            return;
        }

        // 타겟 방향으로 발사 (현재는 초기 방향만 유지)
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
