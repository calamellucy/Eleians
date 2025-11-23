using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public class FlameSword : MonoBehaviour
{
    [Header("세팅")]
    public float interval = 1.5f;      // 1초마다 발동
    public float scale = 3f;         // 초기 스케일 배수
    public float subAngle = 55f;

    [Header("히트 타이밍")]
    public float hitStart = 0.1f;    // 애니 시작 후 몇 초 뒤부터 판정 있을지
    public float hitEnd = 0.3f;      // 언제까지 판정 줄지
    public float damage = 250f;

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
        col.enabled = false;                 // 시작엔 꺼두기
        sr.enabled = false;                       // 처음엔 안 보이게
        transform.localScale = Vector3.one * scale;  // 3배로 키우기
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval) {
            timer = 0f;
            Slash();
        }
    }

    public void GiveLevelSystemToSkill2()
    {
        interval = 1.5f / (StatsManager.instance.AttackSpeed);

        // 불 - 공격계수 +7%
        damage = StatsManager.instance.Attack * 2.5f * (1 + 0.07f * StatsManager.instance.FireCnt);

        // 얼음 - 검을 휘두를 때 이동속도 +2
        

        // 전기 - 검을 휘두를 때 크리티컬 배율 + 10%


        // 흙 - 검의 크기 +7% 
        scale = 3f + (StatsManager.instance.EarthCnt * 0.07f);

        if (StatsManager.instance.FireCnt >= 5) {
            scale += 0.35f;
        }
        if (StatsManager.instance.FireCnt >= 10)
        {
        }
        if (StatsManager.instance.FireCnt >= 15)
        {
        }
        if (StatsManager.instance.FireCnt >= 20)
        {
        }
    }

    void Slash()
    {
        transform.localScale = Vector3.one * scale;

        Transform target = GameManager.instance.player.scans.GetNearest(1)[0];

        if (target != null)
        {
            // 플레이어 위치 기준 방향 계산
            Vector2 dir = (target.position - transform.parent.position).normalized;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // 🔥 스프라이트 기본 방향이 "오른쪽(→)"이라고 가정
            // 만약 위(↑)가 기본이면 angle - 90f 로 바꿔줘
            transform.rotation = Quaternion.AngleAxis(angle - subAngle, Vector3.forward);

        }

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

        if (hitTargets.Contains(other)) return;
        hitTargets.Add(other);

        NormalMonster monster = other.GetComponent<NormalMonster>();
        if (monster != null)
        {
            monster.ApplyDamage(damage, 2);
        }
    }
}
