using System.Collections;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public float duration = 0.5f; // 커지는 시간
    public float maxScale = 8f;   // 최종 크기 (반경 4m면 지름 8m)
    public SpriteRenderer spriter;

    void OnEnable()
    {
        if (spriter == null) spriter = GetComponent<SpriteRenderer>();
        StartCoroutine(ExpandRoutine());
    }

    IEnumerator ExpandRoutine()
    {
        float timer = 0f;
        Color startColor = spriter.color;
        Vector3 startScale = Vector3.zero;

        // 투명도 100%에서 시작
        spriter.color = new Color(startColor.r, startColor.g, startColor.b, 1f);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            // Easing 함수: 팍 커졌다가 천천히 멈춤
            float easeT = 1f - Mathf.Pow(1f - t, 3);

            // 1. 크기 증가
            transform.localScale = Vector3.Lerp(startScale, Vector3.one * maxScale, easeT);

            // 2. 서서히 투명해짐 (후반부에)
            if (t > 0.5f)
            {
                float alphaT = (t - 0.5f) * 2f; // 0~1
                spriter.color = new Color(startColor.r, startColor.g, startColor.b, 1f - alphaT);
            }

            yield return null;
        }

        // 끝나면 삭제
        Destroy(gameObject);
    }
}