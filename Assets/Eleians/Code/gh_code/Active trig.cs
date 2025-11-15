using Unity.VisualScripting;
using UnityEngine;

public class Activetrig : MonoBehaviour
{
    RectTransform rect;
    public Skill4 sk4;
    [Header("Cooldown")]
    public float Cool = 10f;

    [Header("Child UI")]
    public RectTransform coolImage;     // 자식 Image의 RectTransform (Height 33)

    float maxHeight = 33f;
    float timer = 0f;
    bool isCooldown = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        rect.localScale = Vector3.zero;
    }
    void Update()
    {
        if (sk4 == null) return;

        // StoneActive true → 버튼 활성화
        if (sk4.StoneActive)
            rect.localScale = Vector3.one;

        // 아직 StoneActive 아니면 기능 종료
        if (!sk4.StoneActive) return;

        // 쿨타임 중이면 UI 감소
        if (isCooldown)
        {
            timer += Time.deltaTime;
            float ratio = Mathf.Clamp01(timer / Cool);
            float newHeight = Mathf.Lerp(maxHeight, 0f, ratio);

            coolImage.sizeDelta = new Vector2(coolImage.sizeDelta.x, newHeight);

            // 쿨타임 끝남
            if (ratio >= 1f)
            {
                isCooldown = false;
                timer = 0f;
            }

            return;
        }

        // 쿨타임이 끝났을 때만 Q 키 입력 가능
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StatsManager.instance.FireCnt += 1;
            StartCooldown();
        }
    }

    void StartCooldown()
    {
        isCooldown = true;
        timer = 0f;

        // 초기화: Height를 다시 33으로
        coolImage.sizeDelta = new Vector2(coolImage.sizeDelta.x, maxHeight);
    }
}
