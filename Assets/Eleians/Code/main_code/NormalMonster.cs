using UnityEngine;

public class NormalMonster : MonsterBase
{
    public RuntimeAnimatorController[] animCon;
    // public Rigidbody2D target; // MonsterBase에 target이 이미 있다면 주석 유지 혹은 삭제

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

        // ★ [추가] 스턴 상태면 움직이지 않음! (전기 속성 효과)
        if (isStunned) return;

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

    public void Init(SpawnData data, int spriteIndex)
    {
        anim.runtimeAnimatorController = animCon[spriteIndex];
        monsterType = spriteIndex;
        speed = data.speed;

        // ★ [중요] 초기화할 때 원래 속도를 저장해둬야 슬로우가 풀릴 때 돌아갈 곳이 생김
        originalSpeed = speed;

        maxHealth = data.health;
        damage = data.damage;
        health = maxHealth;
    }

    // ---------------------
    // NormalMonster는 Player만 공격
    // ---------------------
    protected override void OnHitPlayer(Player player)
    {
        // 스턴 상태면 공격도 못하게 하려면 여기도 if (isStunned) return; 추가 가능
        // 하지만 보통 몸박 데미지는 스턴이어도 들어가는 경우가 많으니 선택사항이야!

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            player.ApplyDamage(damage);
            attackTimer = attackDelay;
        }
    }

    /*
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
   */
}