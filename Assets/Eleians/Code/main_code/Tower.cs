using UnityEngine;
using UnityEngine.UI;

public class Tower : MonoBehaviour
{
    [Header("Tower Settings")]
    public float maxHealth = 100;
    public float currentHealth;
    public Sprite normalSprite;      // 수호 성공 시 변경될 스프라이트
    public SpriteRenderer spriteRenderer;
    public Animator anim;

    [Header("UI")]
    public GameObject healthBarUI;   // 체력바 오브젝트 (Canvas 아래)
    public Slider healthSlider;

    bool isDestroyed = false;

    void Awake()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        healthBarUI.SetActive(false);
    }

    // TowerPhase 시작 시 호출
    public void OnTowerPhaseStart()
    {
        currentHealth = maxHealth;
        isDestroyed = false;
        healthBarUI.SetActive(true);
        UpdateHealthUI();
        anim.SetBool("isActive", true);
    }

    // TowerPhase 종료 시 호출
    public void OnTowerPhaseEnd()
    {
        healthBarUI.SetActive(false);
        anim.SetBool("isActive", false);

        if (!isDestroyed)
        {
            // 수호 성공 → 스프라이트 변경
            spriteRenderer.sprite = normalSprite;
            Debug.Log("거점 수호 성공");
        }
        else
        {
            Debug.Log("거점 파괴됨");
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDestroyed) return;

        currentHealth -= damage;
        UpdateHealthUI();
        anim.SetTrigger("hit"); // 피격 애니메이션 재생

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDestroyed = true;
            anim.SetBool("destroyed", true);
            Debug.Log("거점 파괴됨");
        }
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
            healthSlider.value = (float)currentHealth / maxHealth;
    }

}
