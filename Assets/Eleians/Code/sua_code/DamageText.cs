using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageText : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float lifeTime = 0.6f;

    public Text text;   // ★ UI Text

    Color originColor;

    void Awake()
    {
        if (text == null)
            text = GetComponent<Text>();

        originColor = text.color;
    }

    public void SetDamage(float dmg, bool isCrit)
    {
        text.text = Mathf.RoundToInt(dmg).ToString();

        if (isCrit)
            text.color = Color.red;
        else
            text.color = originColor;

        StartCoroutine(FadeRoutine());
    }


    IEnumerator FadeRoutine()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < lifeTime)
        {
            elapsed += Time.deltaTime;

            // 위로 떠오르게
            transform.position = startPos + Vector3.up * (elapsed * moveSpeed);

            // 알파값 감소
            float alpha = Mathf.Lerp(1f, 0f, elapsed / lifeTime);
            var c = text.color;
            c.a = alpha;
            text.color = c;

            yield return null;
        }

        // 사라짐
        gameObject.SetActive(false);
    }
}
