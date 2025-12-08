using UnityEngine;
using UnityEngine.UI;

public class FireActiveTrig : MonoBehaviour
{
    RectTransform rect;
    public FireExplosionSkill fireSkill; // 새로 만들 스킬 스크립트 연결

    [Header("Cooldown")]
    public float Cool = 12f; // 쿨타임 12초

    [Header("Child UI")]
    public RectTransform coolImage;

    float maxHeight = 33f;
    float timer = 0f;
    bool isCooldown = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        rect.localScale = Vector3.zero; // 처음엔 숨김
    }

    void Update()
    {
        // StatsManager가 없거나 불 속성이 15 미만이면 숨김
        if (StatsManager.instance == null) return;

        bool canUse = StatsManager.instance.FireCnt >= 15;

        // 활성화 조건 충족 시 UI 보이기
        rect.localScale = canUse ? Vector3.one : Vector3.zero;

        if (!canUse) return;

        // 쿨타임 로직
        if (isCooldown)
        {
            timer += Time.deltaTime;
            float ratio = Mathf.Clamp01(timer / Cool);
            float newHeight = Mathf.Lerp(maxHeight, 0f, ratio);

            if (coolImage != null)
                coolImage.sizeDelta = new Vector2(coolImage.sizeDelta.x, newHeight);

            if (ratio >= 1f)
            {
                isCooldown = false;
                timer = 0f;
            }
            return;
        }


        if (Input.GetKeyDown(KeyCode.E))
        {
            fireSkill.ActiveChainExplosion(); // 스킬 발동!
            StartCooldown();
        }
    }

    void StartCooldown()
    {
        isCooldown = true;
        timer = 0f;
        if (coolImage != null)
            coolImage.sizeDelta = new Vector2(coolImage.sizeDelta.x, maxHeight);
    }
}