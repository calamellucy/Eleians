using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GroundActiveCool : MonoBehaviour
{
    // RectTransform rect; // 이제 Image 자체를 제어하므로 이건 필요 없을 수 있습니다.
    // 하지만 부모 오브젝트 활성화/비활성화 용도라면 남겨두거나 gameObject.SetActive를 씁니다.

    public Skill4 sk4;
    public EarthBumpSkill earthbump;
    public KeyCode triggerKey = KeyCode.Alpha4;

    [Header("Cooldown")]
    public float Cool = 10f;

    [Header("Child UI")]
    public Image borderImage;     // 기존 RectTransform 대신 Image를 받습니다.

    float timer = 0f;
    bool isCooldown = false;

    void Start() // Awake 대신 Start가 안전할 수 있습니다.
    {
        // 처음 시작할 때 쿨타임이 없는 상태라면 테두리가 꽉 차 있어야 함
        if (borderImage != null)
            borderImage.fillAmount = 1f;
    }

    void Update()
    {
        if (sk4 == null) return;

        // StoneActive가 아니면 아예 테두리를 꺼버리거나 로직을 안 돌림
        // (기존 코드의 의도를 살려 StoneActive일 때만 작동하도록 함)
        if (!sk4.StoneActive)
        {
            borderImage.enabled = false; // 혹은 gameObject.SetActive(false);
            return;
        }
        else
        {
            borderImage.enabled = true;
        }

        // --- 쿨타임 로직 ---
        if (isCooldown)
        {
            timer += Time.deltaTime;

            // 0초 -> Cool초 동안, 0 -> 1로 차오름
            float ratio = Mathf.Clamp01(timer / Cool);
            borderImage.fillAmount = ratio;

            // 쿨타임 끝남
            if (timer >= Cool)
            {
                isCooldown = false;
                timer = 0f;
                borderImage.fillAmount = 1f; // 확실하게 꽉 채움
            }
            return; // 쿨타임 중에는 아래 입력 로직 실행 안 함
        }

        // --- 입력 로직 ---
        // 쿨타임이 아니고(테두리가 꽉 참), Q를 눌렀을 때
        if (Input.GetKeyDown(triggerKey))
        {
            earthbump.ActiveEarthBump(); // 스킬 발동
            StartCooldown();             // 쿨타임 시작
        }
    }

    void StartCooldown()
    {
        isCooldown = true;
        timer = 0f;

        // 쿨타임 시작 시 테두리가 사라졌다가(0) 차올라야 하므로 0으로 초기화
        borderImage.fillAmount = 0f;
    }
}
