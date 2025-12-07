using UnityEngine;
using UnityEngine.UI;

public class Tower : MonoBehaviour
{
    [Header("Tower Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDestroyed = false;

    [Header("Components")]
    public SpriteRenderer towerSprite;
    public Animator anim;             // 흔들림 애니메이션 재생
    public Transform shakeGroup;      // 흔들리는 그룹

    [Header("Tower Jam Settings")]
    public GameObject towerJam;       // ★ 항상 켜져있을 잼 오브젝트
    public Animator jamAnimator;      // ★ 잼의 애니메이터 (파괴 모션용)

    [Header("Effects")]
    public GameObject dustLeft;
    public GameObject dustRight;

    [Header("UI")]
    public Slider healthSlider;
    public GameObject healthBarUI;

    void Awake()
    {
        currentHealth = maxHealth;
        healthBarUI.SetActive(false);
        if (towerJam != null) towerJam.SetActive(true);
        dustLeft.SetActive(false);
        dustRight.SetActive(false);
        UpdateHealthUI();
    }

    // TowerPhase 시작
    public void OnTowerPhaseStart()
    {
        currentHealth = maxHealth;
        isDestroyed = false;

        healthBarUI.SetActive(true);
        UpdateHealthUI();

        anim.SetBool("isActive", true);

        // 혹시 파괴 애니메이션 후 다시 시작할 때를 대비해 초기화 (필요시 사용)
        if (jamAnimator != null) jamAnimator.Rebind();
    }

    // TowerPhase 종료
    public void OnTowerPhaseEnd()
    {
        healthBarUI.SetActive(false);
        anim.SetBool("isActive", false);

        if (!isDestroyed)
        {
            Debug.Log("거점 수호 성공");
            GameManager.instance.OnTowerDefenseSuccess();
        }
        else
        {
            Debug.Log("거점 파괴됨");
        }
    }

    // ───────────────────────────────────────────
    //               ★ 피격 처리 ★
    // ───────────────────────────────────────────
    public void TakeDamage(float damage)
    {
        // [수정] 이미 파괴되었거나, 타워 페이즈가 끝났다면 데미지 무시
        if (isDestroyed || !GameManager.instance.isTowerPhase) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        // 흔들림 애니메이션 재생
        anim.SetTrigger("hit");

        if (currentHealth <= 0)
        {
            isDestroyed = true;

            Debug.Log("거점 파괴됨");

            GameManager.instance.GameOver("원소의 균형이 무너졌습니다. 지구는 혼돈에 잠식되었습니다...", true);
        }
    }

    // ★ [신규 추가] 매니저가 부를 함수 (이게 진짜 파괴 버튼)
    public void PlayDestructionEffect()
    {
        if (jamAnimator != null)
        {
            // 파괴 애니메이션 실행!
            jamAnimator.SetBool("Destroyed", true);
        }
    }

    // ───────────────────────────────────────────
    //            ★ HP 바 변경 ★
    // ───────────────────────────────────────────
    void UpdateHealthUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth / maxHealth;
    }

    // ───────────────────────────────────────────
    //      ★ 애니메이션 이벤트로 Dust 켜고 끄기 ★
    // ───────────────────────────────────────────
    public void ShowDust()
    {
        dustLeft.SetActive(true);
        dustRight.SetActive(true);
    }

    public void HideDust()
    {
        dustLeft.SetActive(false);
        dustRight.SetActive(false);
    }

}
