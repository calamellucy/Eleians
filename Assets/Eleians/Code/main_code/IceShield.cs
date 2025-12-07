using System.Collections;
using UnityEngine;

public class IceShield : MonoBehaviour
{
    public int requiredIce = 15;
    public float duration = 4f;
    public float cooldown = 8f;

    bool isReady = true;

    Player player;
    GameManager gm;

    public GameObject shieldPrefab;   // 프리펩 넣는 칸
    GameObject activeShield;          // 생성된 실드 객체 저장

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

    void Update()
    {
        if (StatsManager.instance.IceCnt < requiredIce)
            return;

        if (Input.GetKeyDown(KeyCode.E) && isReady)
        {
            StartCoroutine(Activate());
        }
    }

    IEnumerator Activate()
    {
        isReady = false;

        // ===== 원본 스탯 저장 =====
        originalSpeed = player.speed;
        originalMaxHP = gm.maxHealth;

        // ===== 버프 적용 =====
        gm.maxHealth += 200;
        gm.health += 200;

        player.speed = originalSpeed * 3f;

        DamageReduction.instance.IsIceShield = true;

        // ===== 실드 생성 (프리펩을 Player 자식으로) =====
        activeShield = Instantiate(shieldPrefab, player.transform);
        activeShield.transform.localPosition = Vector3.zero;

        // 반사 데미지 설정
        float reflect = (StatsManager.instance.MaxHP + StatsManager.instance.Attack) * 5f;
        activeShield.GetComponent<ShieldReflect>().reflectDamage = reflect;

        // ===== 지속시간 동안 유지 =====
        yield return new WaitForSeconds(duration);

        // ===== 버프 원복 =====
        gm.maxHealth = originalMaxHP;
        gm.health = Mathf.Clamp(gm.health, 0f, gm.maxHealth);

        player.speed = originalSpeed;

        DamageReduction.instance.IsIceShield = false;

        // ===== 실드 삭제 =====
        if (activeShield != null)
            Destroy(activeShield);

        // ===== 쿨타임 =====
        yield return new WaitForSeconds(cooldown);
        isReady = true;
    }
}
