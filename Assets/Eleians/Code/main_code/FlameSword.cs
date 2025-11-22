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

        // FireCnt >= 10일 때의 로직은 Slash()에서 실행되므로 
        // 여기서는 스탯 수치만 관리하면 됨 (현재 추가 스탯 변경사항 없으면 비워둬도 무방)
    }

    void Slash()
    {
        transform.localScale = Vector3.one * scale;

        // 가장 가까운 적 탐색
        Transform target = GameManager.instance.player.scans.GetNearest(1)[0];

        // 기본 방향 (적이 없을 경우 오른쪽)
        Vector2 dir = Vector2.right;

        if (target != null)
        {
            dir = (target.position - transform.parent.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // 검 회전 (기본 방향 보정)
            transform.rotation = Quaternion.AngleAxis(angle - subAngle, Vector3.forward);
        }

        // ===============================================
        // [추가된 로직] 불 특성 10레벨 이상일 때 검기 발사
        // ===============================================
        if (StatsManager.instance.FireCnt >= 10)
        {
            // 1. PoolManager에서 8번(FireSlashShots) 가져오기
            GameObject shotObj = PoolManager.instance.Get(8);

            // 2. 위치를 플레이어(검의 부모) 위치로 설정
            shotObj.transform.position = transform.parent.position;

            // 3. 방향 설정 및 발사
            FireSlashShots shotScript = shotObj.GetComponent<FireSlashShots>();
            if (shotScript != null)
            {
                // 타겟이 있으면 타겟 방향, 없으면 기본(오른쪽) 방향으로 발사
                shotScript.Launch(dir);
            }
        }
        // ===============================================

        sr.enabled = true;
        hitTargets.Clear();

        anim.Play("fire slash", -1, 0f);
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