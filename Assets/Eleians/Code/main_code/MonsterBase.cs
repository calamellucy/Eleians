using System.Collections;
using UnityEngine;

public enum ElementType
{
    None,
    Fire,
    Ice,
    Earth,
    Lightning
}
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
    protected virtual bool IsSuperArmor => false;

    // 전기 속성 스턴 확인용 변수
    public bool isStunned = false;

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
        isStunned = false; // 스턴 초기화
        attackTimer = 0f;

        rigid.bodyType = RigidbodyType2D.Dynamic;
        rigid.simulated = true;
        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0f;

        coll.enabled = true;
        health = maxHealth;

        anim.ResetTrigger("hit");
        anim.SetBool("dead", false);

        // originalSpeed는 각 몬스터(NormalMonster 등)의 Init에서 설정됨
        slowMultiplier = 1f;
        spriter.color = Color.white; // 색깔 복구
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


    // ★★★ ApplyDamage : 속성(element) 인자 포함 ★★★
    public virtual void ApplyDamage(float dmg, ElementType element = ElementType.None)
    {
        if (!isLive) return;

        float finalDamage = dmg;
        bool isCrit = false;

        // 크리티컬 계산
        if (StatsManager.instance.RollCrit())
        {
            isCrit = true;
            finalDamage *= StatsManager.instance.CritDamage;
        }

        health -= finalDamage;
        PoolManager.instance.ShowDamage(7, finalDamage, transform.position + Vector3.up * 0.5f, isCrit);

        if (health <= 0)
        {
            Die(true);
            return;
        }

        if (!IsSuperArmor)
        {
            anim.SetTrigger("hit");
        }

        // ★★★ 속성별 특수 능력 적용 ★★★
        switch (element)
        {
            case ElementType.Fire:
                // 3초간 도트 데미지
                StartCoroutine(BurnRoutine());
                break;

            case ElementType.Ice:
                // 얼음은 슬로우 (기본 30% 감속)
                ApplySlow(0.3f);
                break;

            case ElementType.Earth:
                // 흙은 넉백
                KnockBack(target.position);
                break;

            case ElementType.Lightning:
                // 전기는 스턴
                StartCoroutine(StunRoutine());
                break;

            case ElementType.None:
                break;
        }
    }

    public void ApplyDamageWithoutKonckback(float dmg)
    {
        ApplyDamage(dmg, ElementType.None);
    }

    // --- [불] 도트 데미지 코루틴 (수정됨) ---
    IEnumerator BurnRoutine()
    {
        // 너무 쨍한 빨강 대신 부드러운 붉은색 (1, 0.6, 0.6)
        spriter.color = new Color(1f, 0.6f, 0.6f);

        float dotDamage = StatsManager.instance.Attack * 0.05f; // 공격력의 5%

        for (int i = 0; i < 3; i++) // 3회 반복 (3초)
        {
            yield return new WaitForSeconds(1f);
            if (!isLive) break;

            health -= dotDamage;
            PoolManager.instance.ShowDamage(7, dotDamage, transform.position + Vector3.up * 0.5f, false);

            if (health <= 0)
            {
                Die(true);
                yield break;
            }
        }
        spriter.color = Color.white; // 색상 복귀
    }

    // --- [전기] 스턴 코루틴 (수정됨) ---
    IEnumerator StunRoutine()
    {
        if (isStunned) yield break; // 이미 스턴이면 무시

        isStunned = true;

        rigid.linearVelocity = Vector2.zero;

        // 노란색 변경 삭제 (원래 색 유지)

        yield return new WaitForSeconds(0.15f); // 0.15초 (아주 짧은 경직)

        isStunned = false;
    }

    public void ApplySlow(float slowRate)
    {
        float newMultiplier = 1f - slowRate;
        slowMultiplier = Mathf.Min(slowMultiplier, newMultiplier);
        slowMultiplier = Mathf.Clamp(slowMultiplier, 0.2f, 1f);
        speed = originalSpeed * slowMultiplier;

        // 슬로우 시각적 효과 (파란색)
        spriter.color = new Color(0.6f, 0.6f, 1f);
    }

    protected virtual void KnockBack(Vector3 from)
    {
        StartCoroutine(KnockBackRoutine(from));
    }

    // --- [흙] 넉백 코루틴 (수정됨) ---
    protected IEnumerator KnockBackRoutine(Vector3 from)
    {
        if (!isLive) yield break;

        isKnockback = true;
        yield return new WaitForFixedUpdate();

        if (!isLive) yield break;

        Vector2 dir = (transform.position - from).normalized;
        float force = 4f; // 넉백 파워 절반으로 감소 (8 -> 4)
        rigid.AddForce(dir * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.1f);

        if (!isLive) yield break;
        isKnockback = false;
    }

    public virtual void Die(bool giveReward)
    {
        if (isDeadProcessed) return;
        isDeadProcessed = true;
        isLive = false;

        StopAllCoroutines(); // 도트딜, 스턴 등 모든 코루틴 정지
        spriter.color = Color.white;

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