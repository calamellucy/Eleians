using System.Collections;
using UnityEngine;

public class MonsterBase : MonoBehaviour
{
    [Header("Monster Stats")]
    public float speed;
    public float health;
    public float maxHealth;
    public float damage;
    public float originalSpeed;
    public float slowMultiplier = 1f;

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
        attackTimer = 0f; // �߰�

        // �� ���� ���� ���� �ʱ�ȭ
        rigid.bodyType = RigidbodyType2D.Dynamic;
        rigid.simulated = true;
        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0f;

        coll.enabled = true;
        //rigid.simulated = true;
        health = maxHealth;

        // �� �ִϸ��̼� ���� �ʱ�ȭ
        anim.ResetTrigger("hit");
        anim.SetBool("dead", false);

        originalSpeed = speed;
        slowMultiplier = 1f;
    }

    // ---------------------
    // �浹 �޽��� ���� ó��
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
    // �ڽ��� override�ϴ� ���� ó�� �Լ�
    // ---------------------
    protected virtual void OnHitPlayer(Player player) { }
    protected virtual void OnHitTower(Tower tower) { }

    // ---------------------
    // �Ѿ� �ǰ� ó��
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

        if (collision.CompareTag("dust"))
        {
             ApplyDamage(StatsManager.instance.ApplyCrit((StatsManager.instance.Attack + (StatsManager.instance.EarthCnt * 2))) * 0.3f);
        }

        if (collision.CompareTag("Bump"))
        {
            ApplyDamage(StatsManager.instance.ApplyCrit((StatsManager.instance.Attack + (StatsManager.instance.EarthCnt * 8))) * 2f);
        }

        if (collision.CompareTag("Jeonjapa"))
        {
            var br = collision.GetComponent<Bullet_Re>();
            if (br != null) ApplyDamage(br.damage);
        }

        if (collision.CompareTag("Seori"))
        {
            Seori_Shuri seo = collision.GetComponent<Seori_Shuri>();
            if (seo == null) return;

            ApplyDamage(seo.damage);

            // ─────── 둔화 적용하는 부분 ─────────
            ApplySlow(seo.slowRate);
        }

        if (collision.CompareTag("dhwyy"))
        {
            BlizzardArea dhw = collision.GetComponent<BlizzardArea>();
            if (dhw == null) return;

            ApplyDamage(dhw.baseDamage);

            ApplySlow(dhw.slowRate);
        }
    }

    public void ApplyDamage(float dmg)
    {
        if (!isLive) return;

        health -= dmg;

        PoolManager.instance.ShowDamage(7, dmg, transform.position + Vector3.up * 0.5f);


        if (health <= 0)
        {
            Die(true);
            return;
        }

        anim.SetTrigger("hit");
        KnockBack(target.position);
    }
    // 둔화처리함수
    public void ApplySlow(float slowRate)
    {
        // slowRate = 둔화 퍼센트 (예: 0.3 = 30% 감소)
        float newMultiplier = 1f - slowRate;

        // 기존 slowMultiplier보다 더 낮으면 적용
        slowMultiplier = Mathf.Min(slowMultiplier, newMultiplier);

        slowMultiplier = Mathf.Clamp(slowMultiplier, 0.2f, 1f);
        speed = originalSpeed * slowMultiplier;
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

        // �˹� ����
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

        // ��Ȱ��ȭ ó��
        gameObject.SetActive(false);

        // ���� �Ŵ������� ����
        GameManager.instance.kill++;
        GameManager.instance.GetExp();
    }

    public void Die(bool giveReward)
    {
        if (isDeadProcessed) return;
        isDeadProcessed = true;
        isLive = false;

        // ����/�浹 ����
        rigid.simulated = false;
        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0f;
        coll.enabled = false;

        // ���� ó��
        if (giveReward)
        {
            GameManager.instance.kill++;
            GameManager.instance.GetExp();
        }

        // ��� �ִ� ���
        anim.SetBool("dead", true);
    }

    public void OnDeathAnimationEnd()
    {
        gameObject.SetActive(false);
    }
}
