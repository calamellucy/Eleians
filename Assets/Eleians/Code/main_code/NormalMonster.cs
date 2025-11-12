using System.Collections;
using UnityEngine;

public class NormalMonster : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHealth;
    public RuntimeAnimatorController[] animCon;
    public Rigidbody2D target;

    protected bool isLive;

    protected Rigidbody2D rigid;
    protected Collider2D coll;
    protected Animator anim;
    protected SpriteRenderer spriter;
    WaitForFixedUpdate wait;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        wait = new WaitForFixedUpdate();
    }

    private void FixedUpdate()
    {
        if (!isLive || anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            return;

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        rigid.linearVelocity = Vector2.zero;
    }

    private void LateUpdate()
    {
        if (!isLive)
            return;

        spriter.flipX = target.position.x > rigid.position.x;
    }

    protected virtual void OnEnable()
    {
        if (GameManager.instance == null || GameManager.instance.player == null)
            return;

        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 2;
        anim.SetBool("dead", false);
        health = maxHealth;
    }

    public void Init(SpawnData data, int spriteIndex)
    {
        anim.runtimeAnimatorController = animCon[spriteIndex];
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;
    }

    // 폭발, 기본 탄, 전자파 탄 전부 여기서 처리 가능
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isLive)
            return;

        // 폭발 처리
        if (collision.CompareTag("Explosion"))
        {
            Explosion exp = collision.GetComponent<Explosion>();
            if (exp != null)
                ApplyDamage(exp.damage);
            return;
        }

        // 일반 총알 (Bullet)
        if (collision.CompareTag("Bullet"))
        {
            Bullet b = collision.GetComponent<Bullet>();
            if (b == null) return;

            ApplyDamage(b.damage);

            b.per--;
            if (b.per < 0)
                collision.gameObject.SetActive(false);
        }

        // 전자파 총알 (Bullet_Re)
        if (collision.CompareTag("Jeonjapa"))
        {
            Bullet_Re br = collision.GetComponent<Bullet_Re>();
            if (br == null) return;

            ApplyDamage(br.damage);

            // 관통 처리만 Bullet_Re 안에서 하도록 (중복 방지)
            // 즉 여기선 per-- 하지 않음
        }
    }

    // ✅ 통합된 데미지 처리 함수
    public void ApplyDamage(float dmg)
    {
        if (!isLive)
            return;

        health -= dmg;

        if (health > 0)
        {
            anim.SetTrigger("hit");
            StartCoroutine(KnockBack());
        }
        else
        {
            isLive = false;
            coll.enabled = false;
            rigid.simulated = false;
            spriter.sortingOrder = 1;
            anim.SetBool("dead", true);
            GameManager.instance.kill++;
            GameManager.instance.GetExp();
        }
    }

    IEnumerator KnockBack()
    {
        yield return wait;
        Vector3 playerpos = GameManager.instance.player.transform.position;
        Vector3 dirVec = transform.position - playerpos;
        rigid.AddForce(dirVec.normalized * 1, ForceMode2D.Impulse);
    }

    void Dead()
    {
        gameObject.SetActive(false);
    }
}
