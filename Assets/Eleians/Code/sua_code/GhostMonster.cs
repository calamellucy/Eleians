using System.Collections;
using UnityEngine;

public class GhostMonster : NormalMonster
{
    [Header("Ghost Settings")]
    public float spawnDuration = 1.0f; // 생성 모션 재생 시간 (애니메이션 길이와 맞춰주세요!)
    private bool isSpawning = false;   // 현재 생성 중인가?

    // 부모의 OnEnable을 덮어씌워서 생성 로직을 추가
    protected override void OnEnable()
    {
        base.OnEnable(); // 초기화 (체력, 상태 등)

        // ★ 생성 시작!
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        isSpawning = true; // 이동/피격 잠금

        // 1. 아직은 때릴 수 없게 콜라이더 끄기 (선택사항)
        // 유령이 나타나는 중인데 맞으면 이상하니까 보통 끕니다.
        if (coll != null) coll.enabled = false;

        // 2. 애니메이션은 Animator 설정(Entry->Spawn) 덕분에 자동으로 재생됨

        // 3. 애니메이션 길이만큼 대기
        yield return new WaitForSeconds(spawnDuration);

        // 4. 생성 완료!
        isSpawning = false;
        if (coll != null) coll.enabled = true; // 이제 맞을 수 있음
    }

    protected new void LateUpdate()
    {
        if (!isLive) return;
        if (target == null) return;

        // 유령이 반대로 움직인다면 이 부등호를 반대로 바꾸면 해결됨
        spriter.flipX = target.position.x < rigid.position.x;
    }

    // ★ 이동 로직 차단
    protected override void FixedUpdate()
    {
        // 생성 중이면 움직이지 마라 (부모의 이동 로직 무시)
        if (isSpawning)
        {
            rigid.linearVelocity = Vector2.zero;
            return;
        }

        base.FixedUpdate(); // 생성 끝나면 정상적으로 이동
    }

    // 유령 전용 초기화 함수 (데이터 파일 없이 보스가 직접 스탯 주입)
    public void InitGhost(float hp, float dmg, float spd)
    {
        maxHealth = hp;
        health = hp;
        damage = dmg;
        speed = spd;
        originalSpeed = spd;
        myResistance.element = ElementType.None;

        if (GameManager.instance.player != null)
        {
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        }
    }

    public override void ApplyDamage(float dmg, ElementType element = ElementType.None)
    {
        if (isSpawning) return; // 생성 중엔 무적
        base.ApplyDamage(dmg, ElementType.None);
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