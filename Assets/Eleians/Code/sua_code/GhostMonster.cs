using UnityEngine;

public class GhostMonster : NormalMonster
{
    // 유령 전용 초기화 함수 (데이터 파일 없이 보스가 직접 스탯 주입)
    public void InitGhost(float hp, float dmg, float spd)
    {
        maxHealth = hp;
        health = hp;
        damage = dmg;
        speed = spd;
        originalSpeed = spd; // 슬로우 복구용

        // 내성은 없음 (필요하면 추가)
        myResistance.element = ElementType.None;

        // 타겟 설정 (NormalMonster OnEnable에서 하긴 하지만 확실하게)
        if (GameManager.instance.player != null)
        {
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        }
    }

    // ★ 보스 쫄몹은 죽어도 경험치/킬수 안 주는 게 국룰
    public override void Die(bool giveReward)
    {
        // giveReward가 true로 들어와도 강제로 false로 바꿔버림
        base.Die(false);

        // 유령 죽는 소리나 이펙트가 따로 있다면 여기서 처리
        // AudioManager.instance.PlaySfx(AudioManager.Sfx.GhostDead);
    }
}