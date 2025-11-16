using System.Collections;
using UnityEngine;

public class MonsterBase : MonoBehaviour
{
    [Header("Monster Stats")]
    public float speed;
    public float health;
    public float maxHealth;
    public float damage;

    protected bool isLive;
    protected bool isDeadProcessed = false;
    protected bool isKnockback = false;
    public Rigidbody2D target;
    protected Rigidbody2D rigid;
    protected Collider2D coll;
    protected Animator anim;
    protected SpriteRenderer spriter;

    protected float attackDelay = 0.5f;
    protected float attackTimer = 0f;

    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
    }

    protected virtual void OnEnable()
    {
        isLive = true;
        isKnockback = false;
        isDeadProcessed = false;
        attackTimer = 0f; // 추가

        // ★ 물리 상태 완전 초기화
        rigid.bodyType = RigidbodyType2D.Dynamic;
        rigid.simulated = true;
        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0f;

        coll.enabled = true;
        //rigid.simulated = true;
        health = maxHealth;

        // ★ 애니메이션 상태 초기화
        anim.ResetTrigger("hit");
        anim.SetBool("dead", false);
    }

    // ---------------------
    // 충돌 메시지 단일 처리
    // ---------------------
    protected void OnCollisionStay2D(Collision2D collision)
    {
        if (!isLive) return;

        if (collision.collider.CompareTag("Player"))
        {
            OnHitPlayer(collision.collider.GetComponent<Player>());
        }
        else if (collision.collider.CompareTag("Tower"))
        {
            OnHitTower(collision.collider.GetComponent<Tower>());
        }
    }

    protected void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") ||
            collision.collider.CompareTag("Tower"))
        {
            attackTimer = 0f;
        }
    }

    // ---------------------
    // 자식이 override하는 공격 처리 함수
    // ---------------------
    protected virtual void OnHitPlayer(Player player) { }
    protected virtual void OnHitTower(Tower tower) { }

    // ---------------------
    // 총알 피격 처리
    // ---------------------
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isLive) return;

        if (collision.CompareTag("Explosion"))
        {
            var exp = collision.GetComponent<Explosion>();
            if (exp != null) ApplyDamage(exp.damage);
            return;
        }

        if (collision.CompareTag("Bullet"))
        {
            var b = collision.GetComponent<Bullet>();
            if (b != null)
            {
                ApplyDamage(b.damage);
                b.per--;
                if (b.per < 0) b.gameObject.SetActive(false);
            }
        }

        if (collision.CompareTag("Jeonjapa"))
        {
            var br = collision.GetComponent<Bullet_Re>();
            if (br != null) ApplyDamage(br.damage);
        }
    }

    public void ApplyDamage(float dmg)
    {
        if (!isLive) return;

        health -= dmg;

        if (health <= 0)
        {
            isLive = false;

            rigid.simulated = false;
            rigid.linearVelocity = Vector2.zero;
            rigid.angularVelocity = 0f;
            coll.enabled = false;
            //rigid.bodyType = RigidbodyType2D.Kinematic;
            anim.SetBool("dead", true);
            return;
        }

        anim.SetTrigger("hit");
        KnockBack(target.position);
    }

    protected virtual void KnockBack(Vector3 from)
    {
        StartCoroutine(KnockBackRoutine(from));
    }

    protected IEnumerator KnockBackRoutine(Vector3 from)
    {
        if (!isLive) yield break;

        isKnockback = true;
        yield return new WaitForFixedUpdate();

        if (!isLive) yield break;

        Vector2 dir = (transform.position - from).normalized;
        float force = 8f;
        rigid.AddForce(dir * force, ForceMode2D.Impulse);

        // 넉백 유지
        yield return new WaitForSeconds(0.1f);

        if (!isLive) yield break;
        isKnockback = false;
    }

    public void Dead()
    {
        if (isDeadProcessed) return;
        isDeadProcessed = true;

        health = 0;
        //coll.enabled = false;
        rigid.simulated = false;
        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0f;
        coll.enabled = false;

        // 비활성화 처리
        gameObject.SetActive(false);

        // 게임 매니저에게 보고
        GameManager.instance.kill++;
        GameManager.instance.GetExp();
    }
}
