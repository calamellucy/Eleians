using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

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

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isLive)
            return;

        // 폭발 피격 추가
        if (collision.CompareTag("Explosion"))
        {
            Explosion exp = collision.GetComponent<Explosion>();
            if (exp != null)
            {
                health -= exp.damage;

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
            return; // 여기서 바로 종료
        }

        // 기존 총알 피격 로직 그대로 유지
        if (collision.CompareTag("Bullet") || collision.CompareTag("HomingBullet"))
        {
            float dmg = 0f;
            int per = 0;

            Bullet b = null;
            BulletHoming bh = null;

            if (collision.TryGetComponent(out b))
            {
                dmg = b.damage;
                per = b.per;
            }
            else if (collision.TryGetComponent(out bh))
            {
                dmg = bh.damage;
                per = bh.per;
            }

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

            per--;
            if (per < 0)
                collision.gameObject.SetActive(false);
            else
            {
                if (b != null) b.per = per;
                if (bh != null) bh.per = per;
            }
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
