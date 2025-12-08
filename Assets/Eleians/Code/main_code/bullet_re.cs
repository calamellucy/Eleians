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
        }

        // ★ 총알 발사될 때마다 3초 뒤 비활성화 타이머 실행
        CancelInvoke(nameof(DisableSelf)); // 기존 타이머 중복 실행 방지
        Invoke(nameof(DisableSelf), 2f);
    }

    void DisableSelf()
    {
        gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy"))
            return;

        per--;

        if (per >= 0)
        {
            GetComponent<BulletEvolution>()?.TriggerEvolution();
            return;
        }

        rigid.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
    }

    // ❌ 화면에서 사라지면 비활성화하는 기능 제거
    // void OnBecameInvisible()
    // {
    //     gameObject.SetActive(false);
    // }
}
