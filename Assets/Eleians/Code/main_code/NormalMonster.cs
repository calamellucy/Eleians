using UnityEngine;

public class NormalMonster : MonsterBase
{
    public RuntimeAnimatorController[] animCon;
    // public Rigidbody2D target;

    protected override void OnEnable()
    {
        base.OnEnable();

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
}
