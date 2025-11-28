using System.Collections;
using UnityEngine;

public class MonsterBase : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHealth;
    public float damage;
    public float originalSpeed;
    public float slowMultiplier = 1f;
    public int monsterType;

    public bool isLive;
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
        attackTimer = 0f;

        rigid.bodyType = RigidbodyType2D.Dynamic;
        rigid.simulated = true;
        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0f;

        coll.enabled = true;
        health = maxHealth;

        anim.ResetTrigger("hit");
        anim.SetBool("dead", false);

        //originalSpeed = speed;
        slowMultiplier = 1f;
    }

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

    protected virtual void OnHitPlayer(Player player) { }
    protected virtual void OnHitTower(Tower tower) { }


    public virtual void ApplyDamage(float dmg)
    {
        if (!isLive) return;

        float finalDamage = dmg;
        bool isCrit = false; // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ũ�� ���� �߰�

        if (StatsManager.instance.RollCrit()) {
            isCrit = true;
            finalDamage *= StatsManager.instance.CritDamage;
        }

        health -= finalDamage;

        // !!!!!!!! isCrit�� �Ѱܼ� Ǯ�Ŵ����� �������ؽ�Ʈ���� ���� ��¦ �� ��.
        // Ǯ�Ŵ������� �׳� SetDamage���� isCirt�� �߰��߰�
        // �������ؽ�Ʈ���� if (isCrit) text.color = Color.red; �� ����� SetDamage���� �߰���.
        PoolManager.instance.ShowDamage(7, finalDamage, transform.position + Vector3.up * 0.5f, isCrit); 

        if (health <= 0)
        {
            Die(true);
            return;
        }

        anim.SetTrigger("hit");
        KnockBack(target.position);
    }

    public void ApplyDamageWithoutKonckback(float dmg)
    {
        if (!isLive) return;

        float finalDamage = dmg;
        bool isCrit = false; // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ũ�� ���� �߰�

        if (StatsManager.instance.RollCrit())
        {
            isCrit = true;
            finalDamage *= StatsManager.instance.CritDamage;
        }

        health -= finalDamage;

        // !!!!!!!! isCrit�� �Ѱܼ� Ǯ�Ŵ����� �������ؽ�Ʈ���� ���� ��¦ �� ��.
        // Ǯ�Ŵ������� �׳� SetDamage���� isCirt�� �߰��߰�
        // �������ؽ�Ʈ���� if (isCrit) text.color = Color.red; �� ����� SetDamage���� �߰���.
        PoolManager.instance.ShowDamage(7, finalDamage, transform.position + Vector3.up * 0.5f, isCrit);

        if (health <= 0)
        {
            Die(true);
            return;
        }

        anim.SetTrigger("hit");
        //KnockBack(target.position);
    }

    public void ApplySlow(float slowRate)
    {
        float newMultiplier = 1f - slowRate;
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

        yield return new WaitForSeconds(0.1f);

        if (!isLive) yield break;
        isKnockback = false;
    }
    /*
     * �� ���� ����
    public void Dead()
    {
        if (isDeadProcessed) return;
        isDeadProcessed = true;

        health = 0;
        rigid.simulated = false;
        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0f;
        coll.enabled = false;

        gameObject.SetActive(false);

        GameManager.instance.kill++;
        GameManager.instance.GetExp();
    }
    */

    public virtual void Die(bool giveReward)
    {
        if (isDeadProcessed) return;
        isDeadProcessed = true;
        isLive = false;

        rigid.simulated = false;
        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0f;
        coll.enabled = false;

        if (giveReward)
        {
            GameManager.instance.kill++;
            GameManager.instance.GetExp();
        }

        anim.SetBool("dead", true);
    }

    public void OnDeathAnimationEnd()
    {
        gameObject.SetActive(false);
    }
}
