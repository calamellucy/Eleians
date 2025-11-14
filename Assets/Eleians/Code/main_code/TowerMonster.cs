using UnityEngine;

public class TowerMonster : NormalMonster
{
    protected override void OnEnable()
    {
        base.OnEnable();

        target = GameManager.instance.tower.GetComponent<Rigidbody2D>();
    }

    // Tower만 공격
    protected override void OnHitTower(Tower tower)
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            tower.TakeDamage(damage);
            attackTimer = attackDelay;
        }
    }

    // Player는 무시하도록 비워두기
    protected override void OnHitPlayer(Player player)
    {
        // 아무것도 안 함
    }
}
