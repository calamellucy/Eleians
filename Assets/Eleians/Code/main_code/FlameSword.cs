using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameSword : MonoBehaviour
{
    [Header("세팅")]
    public float interval = 1.5f;
    public float scale = 3f;
    public float subAngle = 55f;

    [Header("히트 타이밍")]
    public float hitStart = 0.1f;
    public float hitEnd = 0.3f;
    public float damage = 150f;

    [Header("2차 각성 (Fire 20)")]
    public int fireStack = 0; // 현재 중첩 스택
    private const int MaxFireStack = 7; // 최대 스택(발동 조건)

    Animator anim;
    Collider2D col;
    SpriteRenderer sr;

    float timer;
    private HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

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

    // 레벨에 따른 기본 스탯 계산 (Slash 할 때마다 초기화용으로 사용)
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
        // 1. 먼저 기본 스탯으로 초기화 (이전 스택 효과가 영구 누적되지 않도록)
        GiveLevelSystemToSkill2();

        // ===============================================
        // [2차 각성] 불 20레벨: 타오르는 맹세 (스택 쌓기)
        // ===============================================
        bool isTriggerAttack = false; // 3방향 발사 타이밍인지 체크

        if (StatsManager.instance.FireCnt >= 20)
        {
            fireStack++;

            // 스택당 크기/공격력 15% 증가 (합연산)
            float bonusMultiplier = 1f + (fireStack * 0.15f);

            scale *= bonusMultiplier;
            damage *= bonusMultiplier;

            // 7회 중첩 시 발동 준비 및 스택 초기화
            if (fireStack >= MaxFireStack)
            {
                isTriggerAttack = true;
                fireStack = 0;
            }
        }

        // 실제 크기 적용
        transform.localScale = Vector3.one * scale;

        // 적 탐색 및 방향 설정
        Transform target = GameManager.instance.player.scans.GetNearest(1)[0];
        Vector2 dir = Vector2.right;

        if (target != null)
        {
            dir = (target.position - transform.parent.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - subAngle, Vector3.forward);
        }

        // ===============================================
        // [발사 로직] 10레벨(1발) vs 20레벨(3발)
        // ===============================================
        if (isTriggerAttack) // 20레벨 7스택 터짐: 3방향 발사
        {
            FireProjectile(dir); // 정면

            // 위쪽 (+30도)
            Vector2 dirUp = Quaternion.AngleAxis(30f, Vector3.forward) * dir;
            FireProjectile(dirUp);

            // 아래쪽 (-30도)
            Vector2 dirDown = Quaternion.AngleAxis(-30f, Vector3.forward) * dir;
            FireProjectile(dirDown);
        }
        else if (StatsManager.instance.FireCnt >= 10) // 평소(10레벨 이상)에는 1발 발사
        {
            FireProjectile(dir);
        }
        // ===============================================

        sr.enabled = true;
        hitTargets.Clear();
        anim.Play("fire slash", -1, 0f);
    }

    // 투사체 발사 헬퍼 함수
    void FireProjectile(Vector2 direction)
    {
        // 1. PoolManager에서 8번(FireSlashShots) 가져오기
        GameObject shotObj = PoolManager.instance.Get(8);

        // 2. 위치 설정
        shotObj.transform.position = transform.parent.position;

        // 3. 발사
        FireSlashShots shotScript = shotObj.GetComponent<FireSlashShots>();
        if (shotScript != null)
        {
            shotScript.Launch(direction);
        }
    }

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!col.enabled) return;
        if (!other.CompareTag("Enemy")) return;
        if (hitTargets.Contains(other)) return;

        hitTargets.Add(other);

        NormalMonster monster = other.GetComponent<NormalMonster>();
        if (monster != null)
        {
            monster.ApplyDamage(damage);
        }
    }
}