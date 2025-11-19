using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BulletHoming : MonoBehaviour
{
    [Header("Stat")]
    public float damage = 10f;
    public int per = 1;
    public float speed = 15f;

    Transform target;
    Rigidbody2D rigid;
    BulletEvolution evolution;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        evolution = GetComponent<BulletEvolution>();

        rigid.bodyType = RigidbodyType2D.Kinematic;
        rigid.gravityScale = 0;
        rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rigid.interpolation = RigidbodyInterpolation2D.Interpolate;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    public void SetTarget(Transform t)
    {
        target = t;
        Vector2 dir;

        if (t != null && t.gameObject.activeSelf)
            dir = ((Vector2)t.position - rigid.position).normalized;
        else
            dir = transform.right;

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

        // ⚡ 관통력이 모두 소모된 “실제 적 명중 시점”에서 진화 트리거
        if (per < 0)
        {
            if (evolution != null)
                evolution.TriggerEvolution();

            rigid.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
        if (rigid != null)
            rigid.linearVelocity = Vector2.zero;

        transform.localScale = Vector3.one;
        target = null;
    }
}
