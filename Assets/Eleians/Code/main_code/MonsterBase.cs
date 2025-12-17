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
    public int exp; // Spawner에서 Init으로 받아온 값
    public float speed;
    public float health;
    public float maxHealth;
    public float damage;
    public float originalSpeed;
    public float slowMultiplier = 1f;
    public int monsterType;
    protected Resistance myResistance;

    // [중요] 아티팩트 매니저 오류 방지용 (절대 삭제 X)
    public MonsterType myType;

    [Header("State")]
    public bool isLive;
    protected bool isDeadProcessed = false;
    protected bool isKnockback = false;
    protected virtual bool IsSuperArmor => false;

    // 전기 스턴 확인용
    public bool isStunned = false;

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
        rigid.linearVelocity = Vector2.zero; // Unity 6 대응
        rigid.angularVelocity = 0f;

        coll.enabled = true;
        health = maxHealth;

        anim.ResetTrigger("hit");
        anim.SetBool("dead", false);

        slowMultiplier = 1f;
        spriter.color = Color.white;

        Transform shadow = transform.Find("Shadow");
        if (shadow != null)
        {
            shadow.gameObject.SetActive(true);
        }

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

        // ★★★ [디버깅용 로그] 범인 색출 ★★★
        if (element == ElementType.Ice || myResistance.element == ElementType.Ice)
        {
            Debug.Log($"[얼음 분석] {gameObject.name} 피격! " +
                      $"공격속성: {element} vs 내성속성: {myResistance.element} | " +
                      $"CC무시: {myResistance.ignoreCC}");
        }

        float finalDamage = dmg;

        // ★★★ [핵심 로직] 내성 계산 ★★★
        // 공격 속성이 나의 내성 속성과 같다면?
        if (element != ElementType.None && element == myResistance.element)
        {
            // 1. 데미지 감소 적용
            finalDamage *= (1f - myResistance.damageReduction);

            // 2. CC 무시 옵션이 켜져있다면? -> 속성을 None으로 바꿔서 효과 발동 막음
            if (myResistance.ignoreCC)
            {
                element = ElementType.None;
                // 이렇게 하면 아래 switch문에서 default로 빠져서 상태이상이 안 걸림!
            }
        }

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
            Die(true);
            return;
        }

        if (!IsSuperArmor)
        {
            StartCoroutine(HitFlashRoutine());
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
                KnockBack(); // ★ 수정됨: 매개변수 없이 호출
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

    // ★★★ [추가] 피격 시 빨간색 깜빡임 효과 ★★★
    IEnumerator HitFlashRoutine()
    {
        // 1. 빨간색으로 변경
        spriter.color = Color.red;

        // 2. 0.2초 대기 (깜빡!)
        yield return new WaitForSeconds(0.2f);

        // 3. 원래 색으로 복구
        if (isLive)
        {
            spriter.color = Color.white;
        }
    }

    // --- [불] 도트 데미지 ---
    IEnumerator BurnRoutine()
    {
        if (effectFire != null) effectFire.SetActive(true);

        float dotDamage = StatsManager.instance.Attack * 0.05f * (1f + StatsManager.instance.ElectricCnt * 0.1f);

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
        rigid.linearVelocity = Vector2.zero; // Unity 6 대응

        yield return new WaitForSeconds(0.15f * (StatsManager.instance.FireCnt * 0.05f + 1f));

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

    // --- [흙] 넉백 (수정됨) ---
    // ★ 매개변수 제거 (플레이어 위치 자동 추적)
    protected virtual void KnockBack()
    {
        StartCoroutine(KnockBackRoutine());
    }

    protected IEnumerator KnockBackRoutine()
    {
        if (!isLive) yield break;

        isKnockback = true;

        // ★ 넉백 전 이동 관성 제거 (중요!)
        rigid.linearVelocity = Vector2.zero;

        yield return new WaitForFixedUpdate();

        if (!isLive) yield break;

        // ★ 플레이어 위치를 기준으로 반대 방향 계산
        // (GameManager에 player가 있다고 가정)
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector2 dir = (transform.position - playerPos).normalized;

        float force = 3f;
        rigid.AddForce(dir * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.1f);

        if (!isLive) yield break;

        // 넉백 끝난 후 다시 속도 0으로 (깔끔한 정지)
        rigid.linearVelocity = Vector2.zero;

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

        Transform shadow = transform.Find("Shadow");
        if (shadow != null)
        {
            shadow.gameObject.SetActive(false);
        }

        spriter.color = Color.white;
        rigid.simulated = false;
        rigid.linearVelocity = Vector2.zero; // Unity 6 대응
        rigid.angularVelocity = 0f;
        coll.enabled = false;

        if (giveReward)
        {
            GameManager.instance.kill++;
            GameManager.instance.GetExp(this.exp);
        }
        AudioManager.instance.PlaySfx(AudioManager.Sfx.mobDead);
        anim.SetBool("dead", true);
    }

    public void OnDeathAnimationEnd()
    {
        gameObject.SetActive(false);
    }
}