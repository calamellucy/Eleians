using UnityEngine;

public class TowerMonster : NormalMonster
{
    [Header("Ranged Settings (If assigned, acts as Ranged)")]
    public GameObject projectilePrefab; // 투사체 프리팹 (이게 있으면 원거리!)
    public float attackRange = 5f;      // 사거리 (이 거리 안에 오면 멈춤)

    [Header("Attack Settings")]
    public float attackInterval = 1.0f; // 기본값 1초

    protected override void OnEnable()
    {
        base.OnEnable();

        // 부모(MonsterBase)에 있는 공격 딜레이 변수를 내가 설정한 값으로 덮어쓰기
        attackDelay = attackInterval;

        // 타워가 존재하는지 확인 후 타겟 설정
        if (GameManager.instance.tower != null)
        {
            target = GameManager.instance.tower.GetComponent<Rigidbody2D>();
        }
    }

    // ★★★ [추가] 이동 로직 재정의 (원거리는 쏘기 위해 멈춰야 함) ★★★
    protected new void FixedUpdate()
    {
        if (!isLive || isStunned) return;
        if (target == null) return;

        // 1. 원거리 몬스터(프리팹 있음)이고, 사거리 안에 들어왔다면?
        if (projectilePrefab != null)
        {
            float dist = Vector2.Distance(transform.position, target.position);

            if (dist <= attackRange)
            {
                // 멈춰서 공격 준비
                rigid.linearVelocity = Vector2.zero;
                return; // 더 이상 이동하지 않음
            }
        }

        // 2. 그 외(근거리거나, 사거리 밖)라면 -> 그냥 닥돌 (부모 로직 사용)
        base.FixedUpdate();
    }

    // ★★★ [추가] 공격 로직 (원거리 발사) ★★★
    void Update()
    {
        if (!isLive || target == null) return;

        // 원거리 몬스터일 때만 실행
        if (projectilePrefab != null)
        {
            float dist = Vector2.Distance(transform.position, target.position);

            // 사거리 안에 들어왔으면 발사 타이머 가동
            if (dist <= attackRange)
            {
                attackTimer -= Time.deltaTime;

                if (attackTimer <= 0f)
                {
                    FireProjectile();
                    attackTimer = attackDelay; // 공속 적용
                }
            }
        }
    }

    // 투사체 발사 함수
    void FireProjectile()
    {
        if (projectilePrefab == null) return;

        GameObject bullet = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        EnemyProjectile projScript = bullet.GetComponent<EnemyProjectile>();

        // 타워 방향 계산
        Vector2 dir = (target.position - (Vector2)transform.position).normalized;

        // 투사체 회전 (날아가는 방향 보기)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 데미지 전달 (StatsManager에서 받아온 damage 사용)
        projScript.Init(damage, dir, "Tower");

        // 공격 애니메이션이 있다면 실행
        if (anim != null) anim.SetTrigger("attack");
    }

    // Tower만 공격
    protected override void OnHitTower(Tower tower)
    {
        // 원거리 몬스터는 몸박 데미지를 주면 안 됨 (총알로만 때려야 함)
        if (projectilePrefab != null) return;

        if (!GameManager.instance.isTowerPhase)
        {
            Die(false);
            return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            tower.TakeDamage(damage);
            attackTimer = attackDelay;
        }
    }

    // Player는 무시하도록 비워두기
    protected override void OnHitPlayer(Player player) { }
    
    public override void ApplyDamage(float dmg, ElementType element = ElementType.None)
    {
        base.ApplyDamage(dmg, element);
    }
}
