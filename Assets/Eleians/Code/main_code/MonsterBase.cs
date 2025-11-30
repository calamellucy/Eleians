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
    [Header("Stats")]
    public float speed;
    public float health;
    public float maxHealth;
    public float damage;
    public float originalSpeed;
    public float slowMultiplier = 1f;
    public int monsterType;

    // [중요] 아티팩트 매니저 오류 방지용 (절대 삭제 X)
    public MonsterType myType;

    [Header("State")]
    public bool isLive;
    protected bool isDeadProcessed = false;
    protected bool isKnockback = false;
    protected virtual bool IsSuperArmor => false;

    // 전기 스턴 확인용
    public bool isStunned = false;

    // ★★★ [추가] 이펙트 오브젝트 (인스펙터 연결) ★★★
    [Header("Effects Objects")]
    public GameObject effectFire;
    public GameObject effectIce;
    public GameObject effectLightning;

    [Header("Components")]
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
        isStunned = false;
        attackTimer = 0f;

        rigid.bodyType = RigidbodyType2D.Dynamic;
        rigid.simulated = true;
        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0f;

        coll.enabled = true;
        health = maxHealth;

        anim.ResetTrigger("hit");
        anim.SetBool("dead", false);

        slowMultiplier = 1f;
        spriter.color = Color.white;

        // ★ 부활 시 이펙트 초기화 (다 끄기)
        if (effectFire != null) effectFire.SetActive(false);
        if (effectIce != null) effectIce.SetActive(false);
        if (effectLightning != null) effectLightning.SetActive(false);
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

    // ★★★ 데미지 적용 함수 ★★★
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
            ArtifactManager.instance.OnCritProc();
        }

        // 아티팩트 데미지 보정
        ArtifactManager.instance.OnPlayerAttack(this, ref finalDamage, isCrit);

        health -= finalDamage;
        PoolManager.instance.ShowDamage(7, finalDamage, transform.position + Vector3.up * 0.5f, isCrit);

        if (health <= 0)
        {
            ArtifactManager.instance.OnEnemyKilled(this);
            Die(true); // 에러 났던 부분 (이제 정상 작동함)
            return;
        }

        if (!IsSuperArmor)
        {
            anim.SetTrigger("hit");
        }

        // 속성별 효과
        switch (element)
        {
            case ElementType.Fire:
                StartCoroutine(BurnRoutine());
                break;
            case ElementType.Ice:
                ApplySlow(0.3f);
                break;
            case ElementType.Earth:
                KnockBack(target.position); // 에러 났던 부분 (이제 정상 작동함)
                break;
            case ElementType.Lightning:
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

    // --- [불] 도트 데미지 ---
    IEnumerator BurnRoutine()
    {
        if (effectFire != null) effectFire.SetActive(true);

        float dotDamage = StatsManager.instance.Attack * 0.05f;

        for (int i = 0; i < 3; i++)
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
        if (effectFire != null) effectFire.SetActive(false);
    }

    // --- [전기] 스턴 ---
    IEnumerator StunRoutine()
    {
        if (isStunned) yield break;

        isStunned = true;
        if (effectLightning != null) effectLightning.SetActive(true);
        rigid.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.15f);

        if (effectLightning != null) effectLightning.SetActive(false);
        isStunned = false;
    }

    // --- [얼음] 슬로우 ---
    public void ApplySlow(float slowRate)
    {
        float newMultiplier = 1f - slowRate;
        slowMultiplier = Mathf.Min(slowMultiplier, newMultiplier);
        slowMultiplier = Mathf.Clamp(slowMultiplier, 0.2f, 1f);
        speed = originalSpeed * slowMultiplier;

        if (effectIce != null) effectIce.SetActive(true);
    }

    // --- [흙] 넉백 ---
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
        float force = 4f;
        rigid.AddForce(dir * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.1f);

        if (!isLive) yield break;
        isKnockback = false;
    }

    // --- 사망 처리 ---
    public virtual void Die(bool giveReward)
    {
        if (isDeadProcessed) return;
        isDeadProcessed = true;
        isLive = false;

        StopAllCoroutines();

        // 이펙트 끄기
        if (effectFire != null) effectFire.SetActive(false);
        if (effectIce != null) effectIce.SetActive(false);
        if (effectLightning != null) effectLightning.SetActive(false);

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