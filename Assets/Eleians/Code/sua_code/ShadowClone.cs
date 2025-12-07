using UnityEngine;

public class ShadowClone : MonoBehaviour
{
    public float lifeTime = 5f; // 5초 동안 유지

    private SpriteRenderer spriter;
    private float timer = 0f;

    void Awake()
    {
        spriter = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // 5초 뒤에 확실하게 삭제 (Update 로직과 별개로 안전장치)
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 타이머 흐름
        timer += Time.deltaTime;

        if (spriter != null)
        {
            // 비율 계산 (0초일 때 1, 5초일 때 0)
            float alpha = 1f - (timer / lifeTime);

            // 색상 가져와서 알파값만 변경 후 다시 적용
            Color color = spriter.color;
            color.a = alpha;
            spriter.color = color;
        }
    }
}
