using UnityEngine;

public class NormalMonster : MonsterBase
{
    // public RuntimeAnimatorController[] animCon;
    // public Rigidbody2D target; // MonsterBase에 target이 이미 있다면 주석 유지 혹은 삭제
    private float searchTimer = 0f;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (GameManager.instance.player != null)
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();
    }

    protected virtual void FixedUpdate()
    {
        if (!isLive) return;
        if (isKnockback) return;

        // ★ [추가] 스턴 상태면 움직이지 않음! (전기 속성 효과)
        if (isStunned) return;

        // ★ [추가] 0.5초마다 가장 가까운 타겟(플레이어 or 그림자) 찾기
        searchTimer += Time.fixedDeltaTime;
        if (searchTimer > 0.5f)
        {
            searchTimer = 0f;
            FindClosestTarget();
        }

        // ★ [추가] 타겟(플레이어)이 없으면 움직이지 않음 (에러 방지)
        if (target == null) return;

        Vector2 dir = target.position - rigid.position;
        rigid.MovePosition(rigid.position + dir.normalized * speed * Time.fixedDeltaTime);
        rigid.linearVelocity = Vector2.zero;
    }

    protected void LateUpdate()
    {
        if (!isLive) return;
        if (target == null) return; // 타겟 없으면 바라보기 스킵

        spriter.flipX = target.position.x > rigid.position.x;
    }

    public void Init(MonsterStats stats, MonsterType typeEnum)
    {
        // 1. 기본 설정 (외형 및 타입)
        this.myType = typeEnum;
        this.monsterType = stats.patternId;

        // ★★★ [핵심 변경] 데이터에 들어있는 애니메이션을 적용
        if (stats.animatorController != null)
        {
            anim.runtimeAnimatorController = stats.animatorController;
        }

        // 2. ★ 스탯 적용 (인스펙터에서 설정한 값 그대로 대입!) ★
        // 더 이상 switch문으로 배율 계산할 필요 없음!
        maxHealth = stats.maxHealth;
        health = maxHealth;
        damage = stats.damage;
        speed = stats.speed;
        originalSpeed = speed; // 슬로우 복구용
        this.exp = stats.exp;

        // 3. ★ 내성 정보 저장 (MonsterBase 변수에 저장) ★
        this.myResistance = stats.resistance;

    }

    // ---------------------
    // NormalMonster는 Player만 공격
    // ---------------------
    protected override void OnHitPlayer(Player player)
    {
        // 스턴 상태면 공격도 못하게 하려면 여기도 if (isStunned) return; 추가 가능
        // 하지만 보통 몸박 데미지는 스턴이어도 들어가는 경우가 많으니 선택사항이야!
        if (player == null) return;

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            Debug.Log($"[공격] {gameObject.name}이 때림! 데미지: {damage}, 딜레이: {attackDelay}");
            player.ApplyDamage(damage);
            attackTimer = attackDelay;
        }
    }

    // ★★★ 가장 가까운 'Player' 태그 찾기 ★★★
    protected virtual void FindClosestTarget()
    {
        // 씬에 있는 모든 "Player" 태그 오브젝트 찾기 (플레이어 + 그림자들)
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float closestDist = Mathf.Infinity;
        GameObject closestObj = null;

        foreach (GameObject p in players)
        {
            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestObj = p;
            }
        }

        if (closestObj != null)
        {
            target = closestObj.GetComponent<Rigidbody2D>();
        }
    }
}