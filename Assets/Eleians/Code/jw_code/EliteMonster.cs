using System.Collections;
using UnityEngine;

public class EliteMonster : NormalMonster
{
    [Header("Elite Skill Settings")]
    public float skillCooldown = 5f; // 스킬 쿨타임
    private float skillTimer;
    private bool isUsingSkill = false; // 스킬 사용 중인지 (이동 멈춤용)

    [Header("Projectile (For Type 2, 3)")]
    public GameObject[] projectilePrefabs; // 인스펙터에서 총알 프리팹 연결

    [Header("Attack Timing")]
    public float rockThrowDelay = 0.12f; // 돌 던지기 전 딜레이 (인스펙터에서 조절!)
    public float spitThrowDelay = 0.12f; // 침 뱉기 전 딜레이

    [Header("Rapid Fire Settings")]
    public int rapidShotCount = 3;      // 몇 발 쏠지 (3발)
    public float rapidShotInterval = 0.2f; // 연사 속도 (0.2초마다 발사)

    // 방어 스킬용 변수
    private bool isDefending = false;
    
    protected override bool IsSuperArmor
    {
        get
        {
            // 스킬 사용 중이거나, 방어 스킬 중이면 -> 슈퍼아머 발동 (true)
            return isUsingSkill || isDefending;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        skillTimer = skillCooldown; // 나오자마자 바로 스킬 쓰지 않게 쿨타임 채워둠 (원하면 0으로)
        isUsingSkill = false;
        isDefending = false;
    }

    // NormalMonster의 이동 로직(FixedUpdate)을 그대로 쓰되,
    // 스킬 쓰는 중(돌진 준비 등)에는 움직이지 않도록 막습니다.
    protected new void FixedUpdate()
    {
        if (isUsingSkill) return; // 스킬 중엔 이동 로직 정지
        base.FixedUpdate();       // 평소엔 NormalMonster 이동
    }

    void Update()
    {
        if (!isLive || isUsingSkill) return;

        skillTimer -= Time.deltaTime;

        if (skillTimer <= 0)
        {
            skillTimer = skillCooldown;
            UseSkill();
        }
    }

    void UseSkill()
    {
        if (target == null || !target.gameObject.activeSelf) return; // 타겟 없으면 스킬 취소

        switch (monsterType) // SpriteIndex와 동일하게 매칭
        {
            case 0: // Elite 1: 돌진
                StartCoroutine(DashRoutine());
                break;
            case 1: // Elite 2: 방어
                StartCoroutine(DefenseRoutine());
                break;
            case 2: // Elite 3: 돌 2개 V자 던지기
                StartCoroutine(FireTwoRocksRoutine(projectilePrefabs[0]));
                break;
            case 3: // Elite 4: 침 뱉기 (부채꼴)
                StartCoroutine(FireRapidShotRoutine(projectilePrefabs[1]));
                break;
        }
    }

    // --- 스킬 0: 돌진 ---
    IEnumerator DashRoutine()
    {
        isUsingSkill = true; // 이동 멈춤
        anim.SetTrigger("skill"); // 공격 애니메이션 있다면 실행

        // 1. 텔레그래프 (빨개지면서 경고)
        spriter.color = Color.red;
        yield return new WaitForSeconds(0.5f); // 0.5초 대기
        spriter.color = Color.white;

        // 2. 플레이어 방향으로 돌진
        Vector2 dir = (target.position - rigid.position).normalized;
        rigid.AddForce(dir * 10f, ForceMode2D.Impulse); // 팍 튀어나감

        yield return new WaitForSeconds(0.5f); // 돌진 시간
        rigid.linearVelocity = Vector2.zero;   // 정지

        isUsingSkill = false; // 다시 추격 시작
    }

    // --- 스킬 1: 방어 ---
    IEnumerator DefenseRoutine()
    {
        isDefending = true;
        spriter.color = Color.blue; // 파랗게 변함 (방어 상태 표시)

        // 3초간 방어 상태
        yield return new WaitForSeconds(3f);

        spriter.color = Color.white;
        isDefending = false;
    }


    // --- [수정됨] 스킬 2: 돌 2개 V자 발사 (코루틴) ---
    IEnumerator FireTwoRocksRoutine(GameObject prefab)
    {
        if (prefab == null) yield break;

        isUsingSkill = true;       // 1. 이동 멈춤
        anim.SetTrigger("attack");  // 2. 애니메이션 시작

        // [핵심] 공격 모션이 나올 때까지 기다림!
        // 인스펙터에서 rockThrowDelay 값을 조절해서 0.07 프레임과 딱 맞추세요.
        yield return new WaitForSeconds(rockThrowDelay);

        // 3. 딜레이 후 실제 발사
        if (target != null) // 기다리는 동안 타겟이 죽었을 수도 있으니 체크
        {
            Vector2 centerDir = (target.position - (Vector2)transform.position).normalized;
            float angle = 15f;

            Vector2 dirLeft = RotateVector2(centerDir, -angle);
            CreateProjectile(prefab, dirLeft);

            Vector2 dirRight = RotateVector2(centerDir, angle);
            CreateProjectile(prefab, dirRight);
        }

        // 후딜레이 (공격 끝나고 잠시 멈춰있을지? 필요 없으면 삭제 가능)
        // yield return new WaitForSeconds(0.5f);

        isUsingSkill = false;      // 4. 다시 이동 시작
    }

    IEnumerator FireRapidShotRoutine(GameObject prefab)
    {
        if (prefab == null) yield break;

        isUsingSkill = true;       // 이동 멈춤
        anim.SetTrigger("attack");  // 공격 모션 시작

        // 1. 첫 발 발사 전, 애니메이션 타이밍 맞추기 (선딜레이)
        // 아까 설정한 spitThrowDelay(예: 0.12초) 만큼 기다림
        yield return new WaitForSeconds(spitThrowDelay);

        // 2. 따닥- 따닥- 발사 로직
        for (int i = 0; i < rapidShotCount; i++)
        {
            // [핵심] 쏠 때마다 타겟이 살아있는지 확인
            if (target == null || !target.gameObject.activeSelf) break;

            // [핵심] 쏠 때마다 플레이어 위치를 '새로' 갱신해서 조준
            // 플레이어가 움직였으면 바뀐 위치로 쏩니다.
            Vector2 aimDir = (target.position - (Vector2)transform.position).normalized;

            // 발사!
            CreateProjectile(prefab, aimDir);

            // 다음 발사까지 대기 (연사 간격)
            yield return new WaitForSeconds(rapidShotInterval);
        }

        // 3. 후딜레이 (마지막 발사 후 잠시 대기)
        // yield return new WaitForSeconds(0.5f);

        isUsingSkill = false; // 다시 추격 시작
    }

    // 투사체 생성 도우미 함수
    void CreateProjectile(GameObject prefab, Vector2 direction)
    {
        GameObject bullet = Instantiate(prefab, transform.position, Quaternion.identity);
        // 투사체 회전 (날아가는 방향을 보게 함)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        EnemyProjectile projScript = bullet.GetComponent<EnemyProjectile>();
        projScript.Init(damage, direction);
    }

    // [핵심] 벡터 회전 도우미 함수 (수학 공식)
    Vector2 RotateVector2(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        float tx = v.x;
        float ty = v.y;

        // 회전 행렬 공식 적용
        return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
    }

    // 피격 함수 오버라이드 (방어 스킬 적용을 위해)
    public override void ApplyDamage(float dmg, ElementType element = ElementType.None)
    {
        if (isDefending)
        {
            // 방어 중이면 데미지 0 혹은 절반
            dmg *= 0.1f;
            // 팅~ 하는 소리나 이펙트 추가 가능
        }

        // [추가] 슈퍼아머(스킬 중)일 때는 상태이상 무시
        if (IsSuperArmor)
        {
            base.ApplyDamage(dmg, ElementType.None);
        }
        else
        {
            // [추가] 평소에는 속성 효과(이펙트/상태이상) 그대로 적용
            base.ApplyDamage(dmg, element);
        }
    }
}