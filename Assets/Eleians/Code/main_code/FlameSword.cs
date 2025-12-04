using System.Collections;
using UnityEngine; // HashSet을 안 쓰니까 Collections.Generic 삭제

public class FlameSword : MonoBehaviour
{
    [Header("세팅")]
    public float interval = 1.5f;
    public float scale = 3f;
    public float subAngle = 55f;

    [Header("히트 타이밍")]
    public float hitStart = 0.1f;
    public float hitEnd = 0.3f;

    // DamageReceiver가 가져갈 데미지 변수
    public float damage = 150f;

    [Header("2차 각성 (Fire 20)")]
    public int fireStack = 0;
    private const int MaxFireStack = 7;

    Animator anim;
    Collider2D col;
    SpriteRenderer sr;

    float timer;
    // HashSet 삭제됨 (DamageReceiver가 처리함)

    void Awake()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        col.isTrigger = true;
        col.enabled = false;
        sr.enabled = false;
        transform.localScale = Vector3.one * scale;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            Slash();
        }
    }

    public void GiveLevelSystemToSkill2()
    {
        interval = 1.5f / (StatsManager.instance.AttackSpeed);

        // 불 - 공격계수 +7%
        damage = StatsManager.instance.Attack * 1.5f * (1 + 0.07f * StatsManager.instance.FireCnt);

        // 흙 - 검의 크기 +7%
        scale = 3f + (StatsManager.instance.EarthCnt * 0.07f);

        if (StatsManager.instance.FireCnt >= 5)
        {
            scale += 0.35f;
        }
    }

    void Slash()
    {
        GiveLevelSystemToSkill2();

        bool isTriggerAttack = false;

        if (StatsManager.instance.FireCnt >= 20)
        {
            fireStack++;

            float bonusMultiplier = 1f + (fireStack * 0.15f);

            scale *= bonusMultiplier;
            damage *= bonusMultiplier;

            if (fireStack >= MaxFireStack)
            {
                isTriggerAttack = true;
                fireStack = 0;
            }
        }

        transform.localScale = Vector3.one * scale;

        // --- 수정된 부분 시작 ---

        // 1. 스캔 결과를 배열로 먼저 받아서 안전하게 처리
        Transform[] targets = GameManager.instance.player.scans.GetNearest(1);
        Transform target = null;

        // 배열에 내용이 있을 때만 첫 번째 타겟 가져오기
        if (targets != null && targets.Length > 0)
        {
            target = targets[0];
        }

        Vector2 dir;

        if (target != null)
        {
            // 타겟이 있으면 : 타겟 방향으로 설정
            dir = (target.position - transform.parent.position).normalized;
        }
        else
        {
            // 타겟이 없으면(NULL이면) : 랜덤 방향 설정
            // Random.insideUnitCircle은 반지름 1인 원 안의 랜덤 좌표를 반환하므로 normalized로 방향만 따옴
            dir = Random.insideUnitCircle.normalized;

            // 혹시라도 (0,0)이 나오면 기본값(오른쪽) 설정
            if (dir == Vector2.zero) dir = Vector2.right;
        }

        // 결정된 방향(dir)에 맞춰 칼 회전 (타겟이 있든 없든 공통 적용)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - subAngle, Vector3.forward);

        // --- 수정된 부분 끝 ---

        if (isTriggerAttack)
        {
            FireProjectile(dir);
            Vector2 dirUp = Quaternion.AngleAxis(30f, Vector3.forward) * dir;
            FireProjectile(dirUp);
            Vector2 dirDown = Quaternion.AngleAxis(-30f, Vector3.forward) * dir;
            FireProjectile(dirDown);
        }
        else if (StatsManager.instance.FireCnt >= 10)
        {
            FireProjectile(dir);
        }

        sr.enabled = true;
        AudioManager.instance.PlaySfx(AudioManager.Sfx.flame_sword);

        anim.Play("fire slash", -1, 0f);
    }

    void FireProjectile(Vector2 direction)
    {
        GameObject shotObj = PoolManager.instance.Get(8);
        shotObj.transform.position = transform.parent.position;

        FireSlashShots shotScript = shotObj.GetComponent<FireSlashShots>();
        if (shotScript != null)
        {
            shotScript.Launch(direction);
        }
    }

    // 애니메이션 이벤트에서 호출
    void MakeHitBox()
    {
        col.enabled = true;
    }

    void DeleteHitbox()
    {
        col.enabled = false;
    }

    void Unact()
    {
        sr.enabled = false;
        col.enabled = false;
    }

    // OnTriggerEnter2D 삭제됨 (DamageReceiver로 이관)
}