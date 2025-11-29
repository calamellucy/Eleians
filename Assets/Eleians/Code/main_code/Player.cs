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

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;

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

        // [추가] 아티팩트 (도파민, 한화팬 등 받는데미지 감소) 적용
        // StatsManager에 DamageTakenMultiplier(받는 피해 계수)가 있으니 적용
        dmg *= StatsManager.instance.DamageTakenMultiplier;

        GameManager.instance.health -= dmg;

        // [추가] 맞았을 때 발동하는 아티팩트가 있다면 여기서 호출 (예: 반사 데미지)
        // ArtifactManager.instance.OnPlayerHit();

        if (GameManager.instance.health <= 0)
        {
            Die();
        }

        // hit �ִ�, �����ð�, ���� �߰� ����
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
    }


    void Die()
    {
        // 아티팩트로 부활 가능한지 체크
        if (ArtifactManager.instance.TryRevive())
        {
            return;
        }

        GameManager.instance.isLive = false;
        // anim.SetTrigger("Dead");
        // rigid.simulated = false;
    }
}
