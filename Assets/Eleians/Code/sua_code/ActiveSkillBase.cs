using UnityEngine;
using UnityEngine.UI;

// abstract: 이 클래스만으로는 사용할 수 없고 상속받아 써야 함
public abstract class ActiveSkillBase : MonoBehaviour
{
    [Header("Common Settings")]
    public float coolTime = 10f;
    public Image borderImage; // 쿨타임 도는 테두리 이미지 (Filled 타입)

    protected bool isCooldown = false; // protected: 자식도 접근 가능
    protected float timer = 0f;

    protected virtual void Start()
    {
        if (borderImage != null)
            borderImage.fillAmount = 1f; // 시작할 때 꽉 채워두기
    }

    protected virtual void Update()
    {
        // 1. 스킬이 해금되었는지 체크 (자식마다 조건이 다름 -> 추상 함수로 위임)
        if (!IsSkillUnlocked())
        {
            if (borderImage != null) borderImage.enabled = false;
            return;
        }
        else
        {
            if (borderImage != null) borderImage.enabled = true;
        }

        // 2. 쿨타임 계산 로직 (모든 스킬 공통)
        if (isCooldown)
        {
            HandleCooldown();
            return;
        }

        // 3. 키 입력 및 스킬 발동 (자식마다 키와 스킬이 다름 -> 추상 함수로 위임)
        if (CheckInput())
        {
            ActivateSkill(); // 실제 스킬 실행
            StartCooldown(); // 쿨타임 시작
        }
    }

    // 쿨타임 UI 처리 함수
    private void HandleCooldown()
    {
        timer += Time.deltaTime;
        float ratio = Mathf.Clamp01(timer / coolTime);

        if (borderImage != null)
            borderImage.fillAmount = ratio; // 0에서 1로 차오름

        if (timer >= coolTime)
        {
            isCooldown = false;
            timer = 0f;
            if (borderImage != null) borderImage.fillAmount = 1f;
        }
    }

    // 쿨타임 시작 함수
    protected void StartCooldown()
    {
        isCooldown = true;
        timer = 0f;
        if (borderImage != null) borderImage.fillAmount = 0f; // 비우고 시작
    }

    // --- 자식 클래스에서 반드시 구현해야 할 내용들 ---
    protected abstract bool IsSkillUnlocked(); // 언제 활성화되는가? (예: StoneActive)
    protected abstract bool CheckInput();      // 무슨 키를 눌러야 하는가? (예: Q, W)
    protected abstract void ActivateSkill();   // 무슨 스킬이 나가는가?
}