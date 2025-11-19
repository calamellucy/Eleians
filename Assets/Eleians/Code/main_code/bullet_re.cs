using UnityEngine;

public class Bullet_Re : MonoBehaviour
{
    public float damage;
    public int per;
    public int elecCount;

    Rigidbody2D rigid;
    Skill1_Re skill1;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        skill1 = GetComponentInParent<Skill1_Re>();
    }

    public void Init(float damage, int per, Vector3 dir, int elecCount)
    {
        this.damage = damage;
        this.per = per;
        this.elecCount = elecCount;

        if (per >= 0)
        {
            rigid.linearVelocity = dir * (10f + elecCount * 0.05f);
            // (10f): 기본속도
            // (electricCount*0.05f): 전기 개수에 비례한 투사체 속도 증가
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
