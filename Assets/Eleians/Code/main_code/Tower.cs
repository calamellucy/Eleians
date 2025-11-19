using UnityEngine;
using UnityEngine.UI;

public class Tower : MonoBehaviour
{
    [Header("Tower Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDestroyed = false;

    [Header("Sprite Phases (20%씩)")]
    public Sprite[] phaseSprites;     // 0~4단계 스프라이트 넣기
    public Sprite destroyedSprite;
    public Sprite activeSprite;

    [Header("Components")]
    public SpriteRenderer towerSprite;
    public Animator anim;             // 흔들림 애니메이션 재생
    public Transform shakeGroup;      // 흔들리는 그룹

    [Header("Effects")]
    public GameObject dustLeft;
    public GameObject dustRight;
    public GameObject towerJam;

    [Header("UI")]
    public Slider healthSlider;
    public GameObject healthBarUI;

    void Awake()
    {
        currentHealth = maxHealth;
        healthBarUI.SetActive(false);
        towerJam.SetActive(false);
        dustLeft.SetActive(false);
        dustRight.SetActive(false);
        UpdateHealthUI();
        UpdateTowerSprite();
    }

    // TowerPhase 시작
    public void OnTowerPhaseStart()
    {
        currentHealth = maxHealth;
        isDestroyed = false;

        healthBarUI.SetActive(true);
        UpdateHealthUI();
        UpdateTowerSprite();

        anim.SetBool("isActive", true);
    }

    // TowerPhase 종료
    public void OnTowerPhaseEnd()
    {
        healthBarUI.SetActive(false);
        anim.SetBool("isActive", false);

        if (!isDestroyed)
        {
            Debug.Log("거점 수호 성공");
            towerSprite.sprite = activeSprite;
            towerJam.SetActive(true);
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
        if (isDestroyed) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
        UpdateTowerSprite();

        // 흔들림 애니메이션 재생
        anim.SetTrigger("hit");

        if (currentHealth <= 0)
        {
            isDestroyed = true;
            anim.SetBool("destroyed", true);

            towerSprite.sprite = destroyedSprite;
            Debug.Log("거점 파괴됨");
        }
    }

    // ───────────────────────────────────────────
    //            ★ HP 바 & 스프라이트 변경 ★
    // ───────────────────────────────────────────
    void UpdateHealthUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth / maxHealth;
    }

    void UpdateTowerSprite()
    {
        if (isDestroyed) return;

        float ratio = currentHealth / maxHealth;
        int phase = Mathf.Clamp(4 - Mathf.FloorToInt(ratio * 5), 0, 4);

        towerSprite.sprite = phaseSprites[phase];
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
