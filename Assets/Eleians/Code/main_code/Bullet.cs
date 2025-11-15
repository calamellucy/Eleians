using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public int per;

    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Init(float damage, int per, Vector3 dir)
    {
        this.damage = damage; 
        this.per = per;

        if(per > -1)
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

        // 관통 횟수 감소
        per--;

        // 관통 횟수가 남아 있으면 그대로 통과
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
