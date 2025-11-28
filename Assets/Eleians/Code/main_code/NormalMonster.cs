using UnityEngine;

public class NormalMonster : MonsterBase
{
    public RuntimeAnimatorController[] animCon;
    // public Rigidbody2D target;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (GameManager.instance == null || GameManager.instance.player == null)
            return;

        // Player를 타겟으로
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
    }

    protected void FixedUpdate()
    {
        if (!isLive) return;
        if (isKnockback) return;

        Vector2 dir = target.position - rigid.position;
        rigid.MovePosition(rigid.position + dir.normalized * speed * Time.fixedDeltaTime);
        rigid.linearVelocity = Vector2.zero;
    }

    protected void LateUpdate()
    {
        if (!isLive) return;

        spriter.flipX = target.position.x > rigid.position.x;
    }

    public void Init(SpawnData data, int spriteIndex)
    {
        /*
        anim.runtimeAnimatorController = animCon[spriteIndex];
        monsterType = spriteIndex;
        speed = data.speed;
        originalSpeed = speed;

        maxHealth = data.health;
        damage = data.damage;
        health = maxHealth;
        */

        // 1. 기본 설정 (외형 및 타입)
        anim.runtimeAnimatorController = animCon[spriteIndex];
        monsterType = spriteIndex;

        // 2. 기본 데이터 적용 (SpawnData 기준)
        float speedMultiplier = 1f;
        float hpMultiplier = 1f;
        float dmgMultiplier = 1f;

        // 3. 몬스터 타입별 특성 적용 (비율 조정)
        switch (monsterType)
        {
            case 0: // 나무더지 (기본) 
                // 변화 없음
                break;

            case 1: // 송충충 (빠른 속도)
                speedMultiplier = 1.5f; // 50% 더 빠름
                break;

            case 2: // 돌순이 (체력 높음, 공격력 낮음)
                hpMultiplier = 2.0f;    // 체력 2배
                dmgMultiplier = 0.5f;   // 공격력 절반
                break;

            case 3: // 버섯탱이 (공격력 높음, 체력 낮음)
                hpMultiplier = 0.6f;    // 체력 40% 감소
                dmgMultiplier = 2.0f;   // 공격력 2배
                break;
        }

        // 4. 최종 스탯 계산 및 적용
        speed = data.speed * speedMultiplier;
        maxHealth = data.health * hpMultiplier;
        damage = data.damage * dmgMultiplier;

        // 5. 중요: 변경된 MaxHealth를 현재 체력에 적용
        health = maxHealth;

        // 6. 중요: MonsterBase의 감속 로직을 위해 originalSpeed 갱신
        originalSpeed = speed;
    }

    // ---------------------
    // NormalMonster는 Player만 공격
    // ---------------------
    protected override void OnHitPlayer(Player player)
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            player.ApplyDamage(damage);
            attackTimer = attackDelay;
        }
    }

    public override void ApplyDamage(float dmg)
    {
        switch(monsterType)
        {
            case 0:
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            default:
                break;

        }
        base.ApplyDamage(dmg);
    }

}
