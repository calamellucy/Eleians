using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public Scanner scanner;
    public ScanALot scans;

    public bool IsFacingRight { get; private set; } = true;
    public Vector2 MoveDir => inputVec;

    public bool isInvincible = false; // 현재 무적 상태인가?
    private float invincibleTimer = 0f; // 무적 남은 시간

    public bool isLocked = false;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;

    [Header("Effects")]
    public GameObject healEffectPrefab; // ★ 인스펙터에서 힐 이펙트 프리팹 연결!

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        scans = GetComponent<ScanALot>();
    }

    void Update()
    {
        // ★ 이 코드를 Update 최상단에 넣으세요!
        if (isLocked)
        {
            // 1. 입력값 초기화 (이래야 걷는 모션이 멈춤)
            inputVec = Vector2.zero;

            // 2. 애니메이션 멈춤 (Run -> Idle)
            // 본인 애니메이터 파라미터 이름에 맞춰주세요! (예: Speed, IsRun 등)
            /*
            if (anim != null)
            {
                anim.Play("Stand");
                anim.Play("Stand");
            }
            */

            // 3. 물리 속도 0으로 고정 (미끄러짐 방지)
            rigid.linearVelocity = Vector2.zero;

            return; // 아래쪽 이동 코드 실행 금지
        }

        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");

        // [추가] 무적 타이머 및 깜빡임 효과 로직
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;

            if (invincibleTimer <= 0)
            {
                // 무적 종료
                isInvincible = false;
                invincibleTimer = 0;

                // 색깔 원상복구 (투명도 100%)
                Color color = spriter.color;
                color.a = 1f;
                spriter.color = color;
            }
            else
            {
                // 무적 중 깜빡임 효과 (투명도를 0.4 ~ 1.0 사이로 왔다갔다)
                // Sine 함수를 이용해 부드럽게 깜빡임
                Color color = spriter.color;
                // Time.time * 30 하면 빠르게 깜빡임
                color.a = Mathf.Abs(Mathf.Sin(Time.time * 30f)) * 0.5f + 0.5f;
                spriter.color = color;
            }
        }
    }

    void FixedUpdate()
    {
        if (isLocked)
        {
            // 물리 연산 중에도 속도를 0으로 꽉 잡고 있어야 함
            rigid.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    void LateUpdate()
    {
        anim.SetFloat("Speed", inputVec.magnitude);

        if (inputVec.x != 0) {
            spriter.flipX = inputVec.x > 0;
        }
    }

    // [추가] 외부(ArtifactManager 등)에서 무적을 걸어주는 함수
    public void SetInvincible(float duration)
    {
        isInvincible = true;
        invincibleTimer = duration;
        Debug.Log($"플레이어 {duration}초간 무적!");
    }

    public void ApplyDamage(float dmg)
    {
        if (!GameManager.instance.isLive) return;

        // [추가] 무적 상태면 데미지 무시 (가장 먼저 체크!)
        if (isInvincible) return;

        // [추가] 위기 탈출 넘버원 체크!
        // 아티팩트가 "true"를 반환하면(발동하면) 데미지 무시하고 리턴
        if (ArtifactManager.instance.OnPlayerTakeDamage())
        {
            return;
        }

        // 기존 데미지 감소 로직
        // dmg = DamageReduction.instance.ProcessDamage(dmg); 

        // ★★★ [디버깅용 로그 추가] 이 줄을 넣어보세요! ★★★
        float multiplier = StatsManager.instance.DamageTakenMultiplier;
        Debug.Log($"[피격 분석] 몬스터공격력: {dmg} | 받는피해계수: {multiplier} | 최종데미지: {dmg * multiplier}");

        // [추가] 아티팩트 (도파민, 한화팬 등 받는데미지 감소) 적용
        // StatsManager에 DamageTakenMultiplier(받는 피해 계수)가 있으니 적용
        dmg *= StatsManager.instance.DamageTakenMultiplier;

        GameManager.instance.health -= dmg;

        StartCoroutine(HitFlashRoutine());

        // [추가] 맞았을 때 발동하는 아티팩트가 있다면 여기서 호출 (예: 반사 데미지)
        // ArtifactManager.instance.OnPlayerHit();

        if (GameManager.instance.health <= 0)
        {
            Die();
        }

    }

    // 피격 깜빡임 코루틴
    IEnumerator HitFlashRoutine()
    {
        spriter.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriter.color = Color.white;
    }

    public void Heal(float amount)
    {
        if (!GameManager.instance.isLive) return;

        // 절대값 보장
        amount = Mathf.Abs(amount);

        GameManager.instance.health += amount;

        // 최대 체력 초과 방지
        GameManager.instance.health =
            Mathf.Clamp(GameManager.instance.health, 0f, GameManager.instance.maxHealth);

        Debug.Log("HEAL!!");

        // 힐 이펙트, 힐 텍스트 같은 것 원하면 여기에 추가하면 된다
        // ★★★ 이펙트 생성 ★★★
        if (healEffectPrefab != null)
        {
            // 플레이어 위치에 생성 (플레이어의 자식으로 넣어서 따라다니게 함)
            GameObject vfx = Instantiate(healEffectPrefab, transform.position, Quaternion.identity, transform);

            // 이펙트가 너무 크면 가리니까 위치를 발 밑이나 머리 위로 조정 가능
            // vfx.transform.localPosition += Vector3.up * 0.5f; 

            // 2초 뒤에 삭제 (파티클 지속시간에 맞춰 조절)
            Destroy(vfx, 2.0f);
        }
    }


    void Die()
    {
        // 아티팩트로 부활 가능한지 체크
        if (ArtifactManager.instance.TryRevive())
        {
            StartCoroutine(CoReviveSequence());
            return;
        }

        GameManager.instance.isLive = false;
        anim.SetTrigger("Dead");
        rigid.simulated = false;
        isLocked = true;
    }

    // ★★★ [핵심] 사망 -> 부활 연출 코루틴 ★★★
    IEnumerator CoReviveSequence()
    {
        // 조작 잠금
        isLocked = true;
        rigid.simulated = false;
        rigid.linearVelocity = Vector2.zero;
        isInvincible = true;

        // ★★★ [추가] 사망 시 스킬 끄기 ★★★
        if (GameManager.instance.skillObjects != null)
        {
            foreach (var obj in GameManager.instance.skillObjects)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
        if (GameManager.instance.skillScripts != null)
        {
            foreach (var script in GameManager.instance.skillScripts)
            {
                if (script != null) script.enabled = false;
            }
        }

        anim.SetTrigger("Dead");
        yield return new WaitForSeconds(2.0f);
        anim.SetTrigger("Revive");

        // 부활 모션 길이만큼 대기
        float reviveHP = StatsManager.instance.MaxHP * 0.5f;
        GameManager.instance.health = reviveHP;
        yield return new WaitForSeconds(2.1f);

        isLocked = false;
        rigid.simulated = true;

        // ★★★ [추가] 부활 시 스킬 다시 켜기 ★★★
        if (GameManager.instance.skillObjects != null)
        {
            foreach (var obj in GameManager.instance.skillObjects)
            {
                if (obj != null) obj.SetActive(true);
            }
        }
        if (GameManager.instance.skillScripts != null)
        {
            foreach (var script in GameManager.instance.skillScripts)
            {
                if (script != null) script.enabled = true;
            }
        }

        ArtifactManager.instance.ActivateReviveBurst();

        // 부활 직후 3초간 추가 무적 (안전하게 도망갈 시간)
        SetInvincible(3.0f);
        Debug.Log("플레이어 부활 완료!");
    }

    // GameManager에서 호출할 함수
    public void LockState(bool lockPlayer)
    {
        isLocked = lockPlayer;

        if (lockPlayer)
        {
            // 잠그는 순간 즉시 멈춤!
            inputVec = Vector2.zero;
            if (rigid != null) rigid.linearVelocity = Vector2.zero;
            if (anim != null)
            {
                anim.Play("Stand");
            }
        }
    }
}
