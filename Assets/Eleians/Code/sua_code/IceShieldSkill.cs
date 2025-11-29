using System.Collections;
using UnityEngine;

// 이름 변경: IceShield -> IceShieldSkill (역할 명확화)
public class IceShieldSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    public float duration = 4f; // 지속 시간
    public GameObject shieldPrefab; // 프리팹

    // [삭제] requiredIce, cooldown, isReady 등은 UI쪽(ActiveSkillBase)에서 관리함

    Player player;
    GameManager gm;
    GameObject activeShield;

    float originalSpeed;
    float originalMaxHP;

    void Awake()
    {
        player = GetComponent<Player>();
    }

    void Start()
    {
        gm = GameManager.instance;
    }

    // [삭제] Update() 함수 전체 삭제 (입력은 UI 스크립트가 함)

    // 외부에서 호출할 함수 (public)
    public void ActiveIceShield()
    {
        StartCoroutine(ShieldRoutine());
    }

    IEnumerator ShieldRoutine()
    {
        // ===== 원본 스탯 저장 =====
        originalSpeed = player.speed;
        originalMaxHP = gm.maxHealth;

        // ===== 버프 적용 =====
        gm.maxHealth += 200;
        gm.health += 200;
        player.speed = originalSpeed * 3f;

        if (DamageReduction.instance != null)
            DamageReduction.instance.IsIceShield = true;

        // ===== 실드 생성 =====
        if (shieldPrefab != null)
        {
            activeShield = Instantiate(shieldPrefab, player.transform);
            activeShield.transform.localPosition = Vector3.zero;

            // 반사 데미지 설정
            float reflect = (gm.maxHealth + StatsManager.instance.Attack) * 5f;
            ShieldReflect sr = activeShield.GetComponent<ShieldReflect>();
            if (sr != null) sr.reflectDamage = reflect;
        }

        // ===== 지속시간 대기 =====
        yield return new WaitForSeconds(duration);

        // ===== 버프 해제 (원복) =====
        gm.maxHealth = originalMaxHP;
        gm.health = Mathf.Clamp(gm.health, 0f, gm.maxHealth); // 체력 초과 방지
        player.speed = originalSpeed;

        if (DamageReduction.instance != null)
            DamageReduction.instance.IsIceShield = false;

        // ===== 실드 삭제 =====
        if (activeShield != null)
            Destroy(activeShield);

        // [삭제] 쿨타임 대기 로직 (UI 스크립트가 따로 쿨타임을 돌리므로 여기선 필요 없음)
    }
}