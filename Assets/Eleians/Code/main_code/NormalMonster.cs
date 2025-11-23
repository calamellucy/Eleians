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
        anim.runtimeAnimatorController = animCon[spriteIndex];
        monsterType = spriteIndex;
        speed = data.speed;
        maxHealth = data.health;
        damage = data.damage;
        health = maxHealth;
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

    public override void ApplyDamage(float dmg, int skillType)
    {
        switch(monsterType)
        {
            case 0: // 나무더지 약점: 불
                if (skillType == 2)
                {
                    base.ApplyDamage(WeaknessDamage(dmg), 2);
                    return;
                }
                else break;
            case 1: // 송충충 약점: 전기
                if (skillType == 1)
                {
                    base.ApplyDamage(WeaknessDamage(dmg), 1);
                    return;
                }
                else break;
            case 2: // 돌순이 약점: 얼음
                if (skillType == 3)
                {
                    base.ApplyDamage(WeaknessDamage(dmg), 3);
                    return;
                }
                else break;
            case 3: // 버섯탱이 약점: 흙
                if (skillType == 4)
                {
                    base.ApplyDamage(WeaknessDamage(dmg), 4);
                    return;
                }
                else break;
            default:
                break;

        }
        base.ApplyDamage(dmg, skillType);
    }

    float WeaknessDamage(float damage)
    {
        return damage * 1.2f;
    }
}
