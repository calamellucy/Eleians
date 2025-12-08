using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // [필수] 이게 없으면 작동 안 함!

// 인터페이스에서 '클릭(IPointerClickHandler)'은 뺐어.
public abstract class ActiveSkillBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Common Settings")]
    public float coolTime = 10f;
    public Image borderImage;

    [Header("UI Info")]
    public string skillName; // 인스펙터에서 "Fire", "Ice" 등을 꼭 적어줘!

    protected bool isCooldown = false;
    protected float timer = 0f;

    protected virtual void Start()
    {
        if (borderImage != null)
            borderImage.fillAmount = 1f;
    }

    protected virtual void Update()
    {
        // 1. 스킬 해금 체크
        if (!IsSkillUnlocked())
        {
            if (borderImage != null) borderImage.enabled = false;
            return;
        }
        else
        {
            if (borderImage != null) borderImage.enabled = true;
        }

        // 2. 쿨타임 로직
        if (isCooldown)
        {
            HandleCooldown();
            return;
        }

        // 3. 키 입력 (기존 T키 등)
        if (CheckInput())
        {
            ActivateSkill();
            StartCooldown();
        }
    }

    private void HandleCooldown()
    {
        timer += Time.deltaTime;
        float ratio = Mathf.Clamp01(timer / coolTime);
        if (borderImage != null) borderImage.fillAmount = ratio;

        if (timer >= coolTime)
        {
            isCooldown = false;
            timer = 0f;
            if (borderImage != null) borderImage.fillAmount = 1f;
        }
    }

    protected void StartCooldown()
    {
        isCooldown = true;
        timer = 0f;
        if (borderImage != null) borderImage.fillAmount = 0f;
    }

    // ==========================================
    // [마우스 감지 로직]
    // ==========================================

    // 마우스가 들어왔을 때 -> 툴팁 켜기
    public void OnPointerEnter(PointerEventData eventData)
    {
        //if (!IsSkillUnlocked()) return;

        if (SkillUIManager.instance != null)
        {
            AudioManager.instance.PlaySfx(AudioManager.Sfx.mouse_on_button);
            SkillUIManager.instance.ShowTooltip(skillName);
        }
    }

    // 마우스가 나갔을 때 -> 툴팁 끄기
    public void OnPointerExit(PointerEventData eventData)
    {
        if (SkillUIManager.instance != null)
        {
            SkillUIManager.instance.HideTooltip();
        }
    }

    // 추상 함수들
    protected abstract bool IsSkillUnlocked();
    protected abstract bool CheckInput();
    protected abstract void ActivateSkill();
}