using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DarkVisionController : MonoBehaviour
{
    [Header("Overlay Settings")]
    public CanvasGroup overlay;

    [Header("Fade Settings")]
    public float fadeInTime = 1.0f;  // 어두워지는 시간
    public float fadeOutTime = 1.5f; // 밝아지는 시간

    private Coroutine fadeRoutine;

    void Awake()
    {
        if (overlay == null) overlay = GetComponent<CanvasGroup>();

        // ★ [중요] 게임 오브젝트를 끄지 않습니다! (SetActive 사용 금지)
        // 대신 투명하게 만들고 클릭만 안 되게 설정합니다.
        if (overlay != null)
        {
            overlay.alpha = 0f;
            overlay.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// 암흑 시야 시작
    /// </summary>
    public void Enable(float duration)
    {
        // 혹시 모르니 켜두기 (안전장치)
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        // 1. 화면 가리기 시작 (클릭 차단)
        if (overlay != null) overlay.blocksRaycasts = true;

        // 2. 페이드 인 시작 (투명 0 -> 불투명 1)
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(1f, fadeInTime));
    }

    /// <summary>
    /// 암흑 시야 종료 (보스가 호출)
    /// </summary>
    public void Disable()
    {
        // 1. 페이드 아웃 시작 (불투명 1 -> 투명 0)
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(0f, fadeOutTime));
    }

    /// <summary>
    /// 즉시 종료 (보스 사망 시 등)
    /// </summary>
    public void DisableImmediately()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);

        if (overlay != null)
        {
            overlay.alpha = 0f;
            overlay.blocksRaycasts = false;
        }
        // 여기서도 SetActive(false) 하지 않음!
    }

    // 통합 페이드 코루틴
    IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        if (overlay == null) yield break;

        float startAlpha = overlay.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            overlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        overlay.alpha = targetAlpha;

        // ★ [중요] 다 밝아졌으면(0), 그때 Raycast(클릭)만 끕니다. 오브젝트는 끄지 않습니다.
        if (targetAlpha <= 0.01f)
        {
            overlay.blocksRaycasts = false;
        }
    }
}